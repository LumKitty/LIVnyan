using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using UnityEngine;
using VNyanInterface;

namespace VNyan_Liv {
    public class VNyan_Liv : MonoBehaviour, IVNyanPluginManifest, IButtonClickedHandler, ITriggerHandler {
        public string PluginName { get; } = SharedValues.PluginName;
        public string Version { get; } = SharedValues.Version;
        public string Title { get; } = "VNyan to LIV camera sync";
        public string Author { get; } = SharedValues.Author;
        public string Website { get; } = SharedValues.Website;

        private const string SettingsFileName = "LIVnyan.cfg";

        private static float[] CamData = new float[9];
        private static MemoryMappedFile mmf = null;
        private static MemoryMappedViewAccessor mmfAccess;
        private const int MMFSize = sizeof(float) * 9;
        private static int VNyanSettings = 2;
        private static GameObject objLIVnyan;

        private void ErrorHandler(Exception e) {
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterString("_lum_liv_err", e.ToString());
            UnityEngine.Debug.Log("[LIVnyanERR] " + e.ToString());
        }

        private void Log(string message) {
            if ((VNyanSettings & SharedValues.LOGENABLED) != 0) {
                UnityEngine.Debug.Log("[LIVnyan] " + message);
            }
        }

        public void InitializePlugin() {
            try {
                Log("LumKitty's VNyan-LIV plugin version " + Version + " started");
                Log("Spawning gameobject: VNyan_LIV");
                objLIVnyan = new GameObject("VNyan_LIV", typeof(VNyan_Liv));
                objLIVnyan.SetActive(false);
                VNyanInterface.VNyanInterface.VNyanTrigger.registerTriggerListener(this);
                VNyanInterface.VNyanInterface.VNyanUI.registerPluginButton("LIVnyan", this);
                LoadPluginSettings();
                objLIVnyan.SetActive((VNyanSettings & SharedValues.CAMENABLED) != 0);
                InitialiseMMF();
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }

        private void LoadPluginSettings() {
            try { 
                // Get settings in dictionary
                Dictionary<string, string> settings = VNyanInterface.VNyanInterface.VNyanSettings.loadSettings(SettingsFileName);
                bool SettingMissing = false;
                int tempVNyanSettings = 0;
                if (settings != null) {
                    // Read string value
                    string tempSetting;

                    if (settings.TryGetValue("ActiveOnStart", out tempSetting)) {
                        if (bool.Parse(tempSetting)) {
                            tempVNyanSettings += 1;
                            Log("Camera sync enabled on startup");
                        } else {
                            Log("Camera sync disabled on startup");
                        }
                    } else {
                        Log("ActiveOnStart setting missing, defaulting to disabled");
                        SettingMissing = true;
                    }
                    if (settings.TryGetValue("LogEnabled", out tempSetting)) {
                        if (bool.Parse(tempSetting)) {
                            tempVNyanSettings += 2;
                            Log("Logging enabled");
                        } else {
                            Log("Logging disabled"); 
                        }
                    } else {
                        Log("LogEnabled setting missing, defaulting to disabled");
                        SettingMissing = true; 
                    }
                    if (settings.TryGetValue("LogSpam", out tempSetting)) {
                        if (bool.Parse(tempSetting)) {
                            tempVNyanSettings += 4;
                            Log("Log spam enabled");
                        } else {
                            Log("Log spam disabled");
                        }
                    } else {
                        Log("ActiveOnStart setting missing, defaulting to disabled");
                        SettingMissing = true;
                    }
                } else {
                    Log("No settings file detected, using defaults");
                    SettingMissing = true;
                }
                if (SettingMissing) {
                    Log("Writing settings file");
                    SavePluginSettings();
                }
                VNyanSettings = tempVNyanSettings;
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }

        private void SavePluginSettings() {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            settings["ActiveOnStart"] = ((VNyanSettings & SharedValues.CAMENABLED) != 0).ToString();
            settings["LogEnabled"]    = ((VNyanSettings & SharedValues.LOGENABLED) != 0).ToString();
            settings["LogSpam"]       = false.ToString();

            VNyanInterface.VNyanInterface.VNyanSettings.saveSettings(SettingsFileName, settings);
        }

        public void pluginButtonClicked() {
            Log("Plugin button clicked");
            VNyanSettings = VNyanSettings ^ SharedValues.CAMENABLED;
            InitialiseMMF();
            objLIVnyan.SetActive((VNyanSettings & SharedValues.CAMENABLED) != 0 );
            mmfAccess.Write(sizeof(float) * 8, VNyanSettings);
            Log("Enabled: " + ((VNyanSettings & SharedValues.CAMENABLED) != 0).ToString());
        }
        
        private void InitialiseMMF() {
            if ((mmf == null) && ((VNyanSettings & SharedValues.CAMENABLED) != 0)) {
                Log("Creating file");
                mmf = MemoryMappedFile.CreateOrOpen(SharedValues.MMFname, SharedValues.MMFSize);
                Log("Creating accessor");
                mmfAccess = mmf.CreateViewAccessor(0, MMFSize);
            }
        }

        public void triggerCalled(string name, int int1, int int2, int int3, string text1, string text2, string text3) {
            try {
                if (name.Length > 10) {
                    name = name.ToLower();
                    if (name.Substring(0, 9) == "_lum_liv_") {
                        Log("LIV: Detected trigger: " + name);
                        switch (name.Substring(8)) {
                            case "_enable":
                                VNyanSettings = VNyanSettings | SharedValues.CAMENABLED;
                                objLIVnyan.SetActive(true);
                                mmfAccess.Write(sizeof(float) * 8, VNyanSettings);
                                InitialiseMMF();
                                break;
                            case "_disable":
                                VNyanSettings = (VNyanSettings | SharedValues.CAMENABLED) - SharedValues.CAMENABLED;
                                objLIVnyan.SetActive(false);
                                mmfAccess.Write(sizeof(float) * 8, VNyanSettings);
                                break;
                        }
                    }
                }
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }
        public void Update() {
            try {
                // var camera = Camera.main;
                mmfAccess.Write(0, Camera.main.transform.position.x);
                mmfAccess.Write(sizeof(float) * 1, Camera.main.transform.position.y);
                mmfAccess.Write(sizeof(float) * 2, Camera.main.transform.position.z);
                mmfAccess.Write(sizeof(float) * 3, Camera.main.transform.rotation.w);
                mmfAccess.Write(sizeof(float) * 4, Camera.main.transform.rotation.x);
                mmfAccess.Write(sizeof(float) * 5, Camera.main.transform.rotation.y);
                mmfAccess.Write(sizeof(float) * 6, Camera.main.transform.rotation.z);
                mmfAccess.Write(sizeof(float) * 7, Camera.main.fieldOfView);
                if ((VNyanSettings & SharedValues.LOGSPAMENABLED) !=0) {
                    Log("Set POS: " + Camera.main.transform.position.ToString() + " ROT: " + Camera.main.transform.rotation.ToString() + " FOV: " + Camera.main.fieldOfView + " Settings: " + VNyanSettings);
                }
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }
    }
}
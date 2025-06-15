using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Net;
using System.Net.Sockets;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;
using VNyanInterface;
using static UnityEngine.Experimental.Rendering.RayTracingAccelerationStructure;

namespace VNyan_Liv {
    public class VNyan_Liv : MonoBehaviour, IVNyanPluginManifest, IButtonClickedHandler, ITriggerHandler {
        public string PluginName { get; } = "VNyan_Liv";
        public string Version { get; } = "v0.5";
        public string Title { get; } = "VNyan to LIV camera sync";
        public string Author { get; } = "LumKitty";
        public string Website { get; } = "https://lum.uk/";

        private const string SettingsFileName = "LIVnyan.cfg";

        private static float[] CamData = new float[9];
        private static Mutex mutex;
        private static MemoryMappedFile mmf;
        private static MemoryMappedViewAccessor mmfAccess;
        private const int MMFSize = sizeof(float) * 9;

        //private static bool Enabled = false;
        //private static bool LogEnabled;
        private static int VNyanSettings = 2;

        void ErrorHandler(Exception e) {
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterString("_lum_liv_err", e.ToString());
            UnityEngine.Debug.Log("[LIVnyanERR] " + e.ToString());
        }

        public void Log(string message) {
            if ((VNyanSettings & 2) == 2) {
                //System.IO.File.AppendAllText("D:\\Dev\\VNyan-liv.log", message+"\n");
                UnityEngine.Debug.Log("[LIVnyan] " + message);
            }
        }

        public void InitializePlugin() {
            try {
                Log("Lum's VNyan-LIV plugin version " + Version + " started");
                VNyanInterface.VNyanInterface.VNyanTrigger.registerTriggerListener(this);
                VNyanInterface.VNyanInterface.VNyanUI.registerPluginButton("LumKitty's LIV Camera sync", this);
                Log("Float size: " + sizeof(float).ToString() + " bytes");
                Log("Bool size: " + sizeof(bool).ToString() + " bytes");
                Log("Creating file");

                mmf = MemoryMappedFile.CreateOrOpen("uk.lum.livnyan.cameradata." + Version, MMFSize);

                Log("Creating accessor");
                mmfAccess = mmf.CreateViewAccessor(0, MMFSize);
                LoadPluginSettings();

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
            settings["ActiveOnStart"] = ((VNyanSettings & 1) == 1).ToString();
            settings["LogEnabled"] = ((VNyanSettings & 2) == 2).ToString();
            settings["LogSpam"] = false.ToString();

            VNyanInterface.VNyanInterface.VNyanSettings.saveSettings(SettingsFileName, settings);
        }

        unsafe private void SetCam() {
            try {
                if ((VNyanSettings & 1) == 1) {
                    var camera = Camera.main;
                    mmfAccess.Write(0, camera.transform.position.x);
                    mmfAccess.Write(sizeof(float) * 1, camera.transform.position.y);
                    mmfAccess.Write(sizeof(float) * 2, camera.transform.position.z);
                    mmfAccess.Write(sizeof(float) * 3, camera.transform.rotation.w);
                    mmfAccess.Write(sizeof(float) * 4, camera.transform.rotation.x);
                    mmfAccess.Write(sizeof(float) * 5, camera.transform.rotation.y);
                    mmfAccess.Write(sizeof(float) * 6, camera.transform.rotation.z);
                    mmfAccess.Write(sizeof(float) * 7, camera.fieldOfView);
                    if ((VNyanSettings & 4) == 4) {
                        Log("Set POS: " + camera.transform.position.ToString() + " ROT: " + camera.transform.rotation.ToString() + " FOV: " + camera.fieldOfView + " Settings: " + VNyanSettings);
                    }
                }
                mmfAccess.Write(sizeof(float) * 8, VNyanSettings);
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }

        public void pluginButtonClicked() {
            VNyanSettings = VNyanSettings ^ 1;
            Log("Enabled :" + ((VNyanSettings & 1) == 1).ToString());
        }

        public void triggerCalled(string name, int int1, int int2, int int3, string text1, string text2, string text3) {
            try {
                if (name.Length > 10) {
                    name = name.ToLower();
                    if (name.Substring(0, 9) == "_lum_liv_") {
                        Log("LIV: Detected trigger: " + name);
                        switch (name.Substring(8)) {
                            case "_enable":
                                VNyanSettings = VNyanSettings | 1;
                                break;
                            case "_disable":
                                VNyanSettings = (VNyanSettings | 1) - 1;
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
                SetCam();
                //if ((VNyanSettings & 1) == 1) { SetCam(); }
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }
    }
}
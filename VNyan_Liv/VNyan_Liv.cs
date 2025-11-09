using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Threading;
using UnityEngine;
using VNyanInterface;

namespace VNyan_Liv {
    [DefaultExecutionOrder(15000)]
    public class CameraTransform {
        public Vector3 Position;
        public Quaternion Rotation;
        public DateTime TargetTime;

        public CameraTransform(Vector3 _Position, Quaternion _Rotation, DateTime _TargetTime) {
            Position = _Position;
            Rotation = _Rotation;
            TargetTime = _TargetTime;
        }
        public bool Ready {
            get { return (DateTime.UtcNow >= TargetTime); }
        }
        public void SetCam() {
            Camera.main.transform.position = Position;
            Camera.main.transform.rotation = Rotation;
        }
    }
    
    public class VNyan_Liv : MonoBehaviour, IVNyanPluginManifest, IButtonClickedHandler, ITriggerHandler {
        public string PluginName { get; } = SharedValues.PluginName;
        public string Version { get; } = SharedValues.Version;
        public string Title { get; } = "VNyan to LIV camera sync";
        public string Author { get; } = SharedValues.Author;
        public string Website { get; } = SharedValues.Website;

        private readonly string SettingsFileName = VNyanInterface.VNyanInterface.VNyanSettings.getProfilePath()+"\\LIVnyan.cfg";

        private static float[] CamData = new float[9];
        private static MemoryMappedFile mmf = null;
        private static MemoryMappedViewAccessor mmfAccess;
        private const int MMFSize = sizeof(float) * 9;
        private static int VNyanSettings = 2;
        private static GameObject objLIVnyan;
        //private static int FramesElapsed = 0;
        private static uint CursedCameraDelay = 0;
        private static List<CameraTransform> CursedCamera = new List<CameraTransform>();


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
                Log("Reading settings from: " + SettingsFileName);
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
                    if (settings.TryGetValue("CursedCamera", out tempSetting)) {
                        if (uint.TryParse(tempSetting, out CursedCameraDelay)) {
                            Log("Cursed Camera delay set to: "+CursedCameraDelay.ToString());
                        } else {
                            Log("Cursed Camera disabled");
                            SettingMissing = true;
                        }
                    } else {
                        Log("Cursed Camera setting missing, defaulting to disabled");
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
            settings["CursedCamera"]  = CursedCameraDelay.ToString();

            VNyanInterface.VNyanInterface.VNyanSettings.saveSettings(SettingsFileName, settings);
        }

        public void pluginButtonClicked() {
            Log("Plugin button clicked");
            SetActive(!objLIVnyan.activeInHierarchy);
            Log("Enabled: " + ((VNyanSettings & SharedValues.CAMENABLED) != 0).ToString());
        }
        
        private void InitialiseMMF() {
            if (mmf == null) {
                Log("Creating file");
                mmf = MemoryMappedFile.CreateOrOpen(SharedValues.MMFname, SharedValues.MMFSize);
                Log("Creating accessor");
                mmfAccess = mmf.CreateViewAccessor(0, MMFSize);
            }
        }

        private void SetActive(bool Active) {
            if (Active) {
                Log("Initialise MMF");
                InitialiseMMF();
                Log("Update Settings");
                VNyanSettings = VNyanSettings | SharedValues.CAMENABLED;
                Log("Write settings to MMF");
                mmfAccess.Write(sizeof(float) * 8, VNyanSettings);
                Log("Enable LIVnyan GameObject");
                objLIVnyan.SetActive(true);
                Log("Disable physical camera");
                Camera.main.usePhysicalProperties = false;
            } else {
                VNyanSettings = (VNyanSettings | SharedValues.CAMENABLED) - SharedValues.CAMENABLED;
                objLIVnyan.SetActive(false);
                CursedCamera.Clear();
                mmfAccess.Write(sizeof(float) * 8, VNyanSettings);
                Camera.main.usePhysicalProperties = true;
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
                                if (int1 > 0) {
                                    CursedCameraDelay = (uint)int1;
                                    Log("CursedCamera set to: " + CursedCameraDelay.ToString());
                                } else if (int1 <0) {
                                    CursedCameraDelay = 0;
                                    Log("CursedCamera disabled");
                                }
                                SetActive(true);
                                break;
                            case "_disable":
                                SetActive(false);
                                break;
                        }
                    }
                }
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }
        public void LateUpdate() {
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
                    /*if (FramesElapsed >= 60) { FramesElapsed = 0; }
                    if (FramesElapsed == 0) {
                        Log("FOV                    : " + Camera.main.fieldOfView.ToString());
                        Log("Physical Camera Enabled: " + Camera.main.usePhysicalProperties.ToString());
                        Log("Focal Length           : " + Camera.main.focalLength.ToString());
                        Log("Orthograhpic           : " + Camera.main.orthographic.ToString());
                        Log("Sensor Size            : " + Camera.main.sensorSize.ToString());
                        Log("Lens Shift             : " + Camera.main.lensShift.ToString());
                        Log("Gate Fit               : " + Camera.main.gateFit.ToString());
                        Log("Height                 : " + Camera.main.pixelHeight.ToString());
                        Log("Width                  : " + Camera.main.pixelWidth.ToString());
                        Log("----------------------------------------------------");
                    }
                    FramesElapsed++;*/
                }
                if (CursedCameraDelay > 0) {
                    CursedCamera.Add(new CameraTransform(Camera.main.transform.position, Camera.main.transform.rotation, DateTime.UtcNow.AddMilliseconds(CursedCameraDelay)));
                    //Log("New Frame");
                    int Count = CursedCamera.Count;
                    //Log("0/" + Count.ToString());
                    
                    if (!CursedCamera[0].Ready) {
                        CursedCamera[0].SetCam();
                    } else {
                        int n = 1;
                        while (n < CursedCamera.Count && CursedCamera[n].Ready) {
                            //Log(n.ToString()+"/" + CursedCamera.Count.ToString());
                            n++;
                        }
                        CursedCamera[n - 1].SetCam();
                        CursedCamera.RemoveRange(0, n);
                    }
                    //Log ("Queue Len: "+CursedCamera.Count.ToString()+" Time: "+DateTime.UtcNow.ToString()+" Next trg time: " + CursedCamera[0].TargetTime);
                }
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }
    }
}
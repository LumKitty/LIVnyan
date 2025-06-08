using System;
using UnityEngine;
using VNyanInterface;
using UnityEngine.XR;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Runtime;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO.MemoryMappedFiles;
using static UnityEngine.Experimental.Rendering.RayTracingAccelerationStructure;

namespace VNyan_Liv {
    public class VNyan_Liv : MonoBehaviour, IVNyanPluginManifest, IButtonClickedHandler, ITriggerHandler {
        public string PluginName { get; } = "VNyan_Liv";
        public string Version { get; } = "v0.4";
        public string Title { get; } = "VNyan to LIV camera sync";
        public string Author { get; } = "LumKitty";
        public string Website { get; } = "https://lum.uk/";

        private const string SettingsFileName = "LIVnyan.cfg";

        private static float[] CamData = new float[9];
        private static Mutex mutex;
        private static MemoryMappedFile mmf;
        private static MemoryMappedViewAccessor mmfAccess;
        private const int MMFSize = sizeof(float) * 9;

        private static bool Enabled = false;
        private static bool LogEnabled;

        void ErrorHandler(Exception e) {
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterString("_lum_liv_err", e.ToString());
            Log("LIVNyan ERR:" + e.ToString());
        }

        public void Log(string message) {
            if (LogEnabled) {
                //System.IO.File.AppendAllText("D:\\Dev\\VNyan-liv.log", message+"\n");
                UnityEngine.Debug.Log("LIVnyan|" + message);
            }
        }

        public void InitializePlugin() {
            try {
                VNyanInterface.VNyanInterface.VNyanTrigger.registerTriggerListener(this);
                VNyanInterface.VNyanInterface.VNyanUI.registerPluginButton("LumKitty's LIV Camera sync", this);
                //LoadPluginSettings();
                LogEnabled = true;
                System.IO.File.WriteAllText("D:\\Dev\\VNyan-liv.log", "");
                Log("Lum's VNyan-LIV plugin version " + Version + " started");
                Log("Float size: " + sizeof(float).ToString() + " bytes");
                Log("Bool size: " + sizeof(bool).ToString() + " bytes");
                Log("meow");
                bool MutexCreated;
                Log("Creating file");
                mmf = MemoryMappedFile.CreateOrOpen("uk.lum.livnyan.cameradata3", MMFSize);
                Log("Creating mutex");
                Mutex mutex = new Mutex(true, "uk.lum.livnyan.cameradata.mutex", out MutexCreated);
                Log("Getting mutex");
                // mutex.WaitOne();
                Log("Creating accessor");
                mmfAccess = mmf.CreateViewAccessor(0, MMFSize);

            } catch (Exception e) {
                ErrorHandler(e);
            }
        }

        private void LoadPluginSettings() {
            // Get settings in dictionary
            Dictionary<string, string> settings = VNyanInterface.VNyanInterface.VNyanSettings.loadSettings(SettingsFileName);
            bool SettingMissing = false;
            if (settings != null) {
                // Read string value
                string temp_LogEnabled;

                settings.TryGetValue("LogEnabled", out temp_LogEnabled);
                if (temp_LogEnabled != null) { LogEnabled = bool.Parse(temp_LogEnabled); } else { LogEnabled = false; SettingMissing = true; }
            } else {
                Log("No settings file detected, using defaults");
                LogEnabled = false;
                SettingMissing = true;
            }
            if (SettingMissing) {
                Log("Writing settings file");
                SavePluginSettings();
            }
        }

        private void SavePluginSettings() {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            settings["LogEnabled"] = LogEnabled.ToString();

            VNyanInterface.VNyanInterface.VNyanSettings.saveSettings(SettingsFileName, settings);
        }

        unsafe private void SetCam() {
            try { 
                if (Enabled) {
                    var camera = Camera.main;
                    mmfAccess.Write(                0, camera.transform.position.x);
                    mmfAccess.Write(sizeof(float) * 1, camera.transform.position.y);
                    mmfAccess.Write(sizeof(float) * 2, camera.transform.position.z);
                    mmfAccess.Write(sizeof(float) * 3, camera.transform.rotation.w);
                    mmfAccess.Write(sizeof(float) * 4, camera.transform.rotation.x);
                    mmfAccess.Write(sizeof(float) * 5, camera.transform.rotation.y);
                    mmfAccess.Write(sizeof(float) * 6, camera.transform.rotation.z);
                    mmfAccess.Write(sizeof(float) * 7, camera.fieldOfView);
                    if (Enabled) {
                        mmfAccess.Write(sizeof(float) * 8, 1);
                    } else {
                        mmfAccess.Write(sizeof(float) * 8, 0);
                    }
                }
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }

        public void pluginButtonClicked() {
            Enabled = !Enabled;
            Log("Enabled :" + Enabled.ToString());
        }

        public void triggerCalled(string name, int int1, int int2, int int3, string text1, string text2, string text3) {
            try {
                if (name.Length > 10) {
                    name = name.ToLower();
                    if (name.Substring(0, 9) == "_lum_liv_") {
                        Log("LIV: Detected trigger: " + name);
                        switch (name.Substring(8)) {
                            case "_setcam":
                                if (int1 > 0) {

                                } else {
                                    SetCam();
                                }
                                break;
                            case "_enable":
                                Enabled = true;
                                break;
                            case "_disable":
                                Enabled = false;
                                break;
                        }
                    }
                }
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }
        public void Update() {
            if (Enabled) { SetCam(); }
        }
    }
}
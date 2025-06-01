using System;
using UnityEngine;
using VNyanInterface;
using UnityEngine.XR;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace VNyan_Liv {
    public class VNyan_Liv : MonoBehaviour, IVNyanPluginManifest, IButtonClickedHandler, ITriggerHandler {
        public string PluginName { get; } = "VNyan_Liv";
        public string Version { get; } = "v0.2";
        public string Title { get; } = "VNyan to LIV camera sync";
        public string Author { get; } = "LumKitty";
        public string Website { get; } = "https://lum.uk/";

        public class State {
            public byte[] buffer = new byte[bufSize];
        }

        private const string DecFormat = "0.0000000000000";
        private const string SettingsFileName = "LIVnyan.cfg";
        private string LIVAddress;
        private int LIVPort;
        private int VNyanPort;
        private static Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private const int bufSize = 8 * 1024;
        private static State state = new State();
        private static EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
        private static AsyncCallback recv = null;
        private static bool Enabled = false;
        private static Vector3 OldCameraPos;
        private static Quaternion OldCameraRot;
        private static float OldCameraFOV;

        void ErrorHandler(Exception e) {
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterString("_lum_ext_err", e.ToString());
            UnityEngine.Debug.Log("DBG:" + e.ToString());
        }

        public void Send(string text) {
            byte[] data = Encoding.ASCII.GetBytes(text);
            _socket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) => {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndSend(ar);
                // Console.WriteLine("SEND: {0}, {1}", bytes, text);
            }, state);
        }

        public void InitializePlugin() {
            try {
                VNyanInterface.VNyanInterface.VNyanTrigger.registerTriggerListener(this);
                VNyanInterface.VNyanInterface.VNyanUI.registerPluginButton("LumKitty's LIV Camera sync", this);
                UnityEngine.Debug.Log("Lum's VNyan-LIV plugin started");
                LoadPluginSettings();
                _socket.Connect(IPAddress.Parse(LIVAddress), LIVPort);
                //Receive();
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
                string temp_LIVAddress;
                string temp_LIVPort;
                string temp_VNyanPort;

                settings.TryGetValue("LIVAddress", out temp_LIVAddress);
                settings.TryGetValue("LIVPort",    out temp_LIVPort);
                settings.TryGetValue("VNyanPort",  out temp_VNyanPort);
                if (temp_LIVAddress != null) { LIVAddress = temp_LIVAddress;           } else { LIVAddress = "127.0.0.1"; SettingMissing = true; }
                if (temp_LIVPort    != null) { LIVPort    = int.Parse(temp_LIVPort);   } else { LIVPort    = 42069;       SettingMissing = true; }
                if (temp_VNyanPort  != null) { VNyanPort  = int.Parse(temp_VNyanPort); } else { VNyanPort  = 42070;       SettingMissing = true; }
            } else {
                UnityEngine.Debug.Log("No settings file detected, using defaults");
                LIVAddress = "127.0.0.1";
                LIVPort = 42069;
                VNyanPort = 42070;
                SettingMissing = true;
            }
            if (SettingMissing) { 
                UnityEngine.Debug.Log("Writing settings file"); 
                SavePluginSettings(); 
            }
        }

        private void SavePluginSettings() {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            settings["LIVAddress"] = LIVAddress;
            settings["LIVPort"]    = LIVPort.ToString();
            settings["VNyanPort"]  = VNyanPort.ToString();

            VNyanInterface.VNyanInterface.VNyanSettings.saveSettings(SettingsFileName, settings);
        }


        private void UpdateCameraPos(Camera camera) {
            float pX = camera.transform.position.x;
            float pY = camera.transform.position.y;
            float pZ = camera.transform.position.z;
            float rX = camera.transform.rotation.eulerAngles.x;
            float rY = camera.transform.rotation.eulerAngles.y;
            float rZ = camera.transform.rotation.eulerAngles.z;
            string message = "POS:" + pX.ToString(DecFormat) + "," + pY.ToString(DecFormat) + "," + pZ.ToString(DecFormat) + ","
                                    + rX.ToString(DecFormat) + "," + rY.ToString(DecFormat) + "," + rZ.ToString(DecFormat);
            UnityEngine.Debug.Log(message);
            Send(message);
            OldCameraPos = camera.transform.position;
            OldCameraRot = camera.transform.rotation;
        }
        private void UpdateCameraFOV(Camera camera) {
            float FOV = camera.fieldOfView;
            string message = "FOV:" + FOV.ToString(DecFormat);
            UnityEngine.Debug.Log(message);
            Send(message);
            OldCameraFOV = FOV;
        }
        private void SetCam() {
            var camera = Camera.main;
            if ((camera.transform.position != OldCameraPos) || (camera.transform.rotation != OldCameraRot)) {
                UpdateCameraPos(camera);
            }
            if (camera.fieldOfView != OldCameraFOV) {
                UpdateCameraFOV(camera);
            }
        }
        
        public void pluginButtonClicked() {
            Enabled = !Enabled;
            if (Enabled) {
                var camera = Camera.main;
                UpdateCameraPos(camera);
                UpdateCameraFOV(camera);
            }
        }

        public void triggerCalled(string name, int int1, int int2, int int3, string text1, string text2, string text3) {
            try {
                if (name.Length > 10) {
                    name = name.ToLower();
                    if (name.Substring(0, 9) == "_lum_liv_") {
                        UnityEngine.Debug.Log("LIV: Detected trigger: " + name );
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

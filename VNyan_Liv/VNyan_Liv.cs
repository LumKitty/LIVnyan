using System;
using UnityEngine;
using VNyanInterface;
using UnityEngine.XR;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace VNyan_Liv {
    public class VNyan_Liv : MonoBehaviour, IVNyanPluginManifest, IButtonClickedHandler, ITriggerHandler {
        public string PluginName { get; } = "VNyan_Liv";
        public string Version { get; } = "v0.1";
        public string Title { get; } = "VNyan to LIV camera sync";
        public string Author { get; } = "LumKitty";
        public string Website { get; } = "https://lum.uk/";

        public class State {
            public byte[] buffer = new byte[bufSize];
        }

        private static Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private const int bufSize = 8 * 1024;
        private static State state = new State();
        private static EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
        private static AsyncCallback recv = null;
        private static bool Enabled = false;

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
                _socket.Connect(IPAddress.Parse("127.0.0.1"), 42069);
                //Receive();
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }

        private void SetCam() {
            var camera = Camera.main;
            float pX = camera.transform.position.x;
            float pY = camera.transform.position.y;
            float pZ = camera.transform.position.z;
            float rX = camera.transform.rotation.eulerAngles.x;
            float rY = camera.transform.rotation.eulerAngles.y;
            float rZ = camera.transform.rotation.eulerAngles.z;
            float FOV = camera.fieldOfView;
            string message = "POS:" + pX.ToString("0.0000000000000") + "," + pY.ToString("0.0000000000000") + "," + pZ.ToString("0.0000000000000") + ","
                                    + rX.ToString("0.0000000000000") + "," + rY.ToString("0.0000000000000") + "," + rZ.ToString("0.0000000000000") + ","
                                    + FOV.ToString("0.0000000000000");
            // UnityEngine.Debug.Log(message);
            Send(message);
        }
        
        public void pluginButtonClicked() {
            Enabled = !Enabled;
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

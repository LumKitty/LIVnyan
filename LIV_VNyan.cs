/*
Copyright 2019 LIV inc.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
and associated documentation files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO.Pipes;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Signals.Examples;

// User defined settings which will be serialized and deserialized with Newtonsoft Json.Net.
// Only public variables will be serialized.
public class VNyanCameraPluginSettings : IPluginSettings {
    public bool FromVNyan = true;
    public int LivUdpPort = 42069;
    public int VNyanUdpPort = 42070;
    public string VNyanAddress = "127.0.0.1";
}

// The class must implement IPluginCameraBehaviour to be recognized by LIV as a plugin.
public class VNyanCameraPlugin : IPluginCameraBehaviour {

    // Store your settings localy so you can access them.
    VNyanCameraPluginSettings _settings = new VNyanCameraPluginSettings();

    // Provide your own settings to store user defined settings .   
    public IPluginSettings settings => _settings;

    // Invoke ApplySettings event when you need to save your settings.
    // Do not invoke event every frame if possible.
    public event EventHandler ApplySettings;

    // ID is used for the camera behaviour identification when the behaviour is selected by the user.
    // It has to be unique so there are no plugin collisions.
    public string ID => "VNyanCameraPlugin";
    // Readable plugin name "Keep it short".
    public string name => "VNyan Camera";
    // Author name.
    public string author => "LumKitty";
    // Plugin version.
    public string version => "0.2";
    // Localy store the camera helper provided by LIV.
    PluginCameraHelper _helper;

    // Constructor is called when plugin loads
    public VNyanCameraPlugin() { }

    // OnActivate function is called when your camera behaviour was selected by the user.
    // The pluginCameraHelper is provided to you to help you with Player/Camera related operations.

    public const string LogFileName = "D:\\Dev\\Livcam.log";

    private UDPSocket UDPServer;

    public void OnActivate(PluginCameraHelper helper) {
        _helper = helper;
        Log("Creating UDP Server");
        UDPServer = new UDPSocket();
        UDPServer.Server("127.0.0.1", _settings.LivUdpPort);
        File.WriteAllText(LogFileName, "Lum's VNyan camera plugin version " + version + " starting");
    }

    // OnSettingsDeserialized is called only when the user has changed camera profile or when the.
    // last camera profile has been loaded. This overwrites your settings with last data if they exist.
    public void OnSettingsDeserialized() {

    }

    // OnFixedUpdate could be called several times per frame. 
    // The delta time is constant and it is ment to be used on robust physics simulations.
    public void OnFixedUpdate() {

    }

    // OnUpdate is called once every frame and it is used for moving with the camera so it can be smooth as the framerate.
    // When you are reading other transform positions during OnUpdate it could be possible that the position comes from a previus frame
    // and has not been updated yet. If that is a concern, it is recommended to use OnLateUpdate instead.
    public void Log(string message) {
        File.AppendAllText(LogFileName, message + "\r\n");
    }

    //float _elaspedTime;
    Vector3 TargetCameraPosition = new Vector3((float)-0.8, (float)1.1,(float)1.6);
    Quaternion TargetCameraRotation = new Quaternion((float)0.0, (float)1.0, (float)0.0, (float)0.2);
    float FOV=35;

    [Obsolete]
    public void OnUpdate() {

        try {

            string LineInput = UDPServer.LastMessage;

            if (LineInput != null) {
                Log("Received :" + LineInput);
                if (LineInput.Length > 4) {
                    switch (LineInput.Substring(0, 4)) {
                        case "POS:":
                            float X;
                            float Y;
                            float Z;
                            float rX;
                            float rY;
                            float rZ;
                            Log("Position change requested");
                            string Positions = LineInput.Substring(4);
                            string[] Values = Positions.Split(',');
                            float.TryParse(Values[0], out X);
                            float.TryParse(Values[1], out Y);
                            float.TryParse(Values[2], out Z);
                            float.TryParse(Values[3], out rX);
                            float.TryParse(Values[4], out rY);
                            float.TryParse(Values[5], out rZ);
                            Log("Desired position: X:" + X + " Y:" + Y + " Z:" + X);
                            Log("Desired rotation: X:" + rX + " Y:" + rY + " Z:" + rZ);
                            TargetCameraPosition = new Vector3(X, Y, Z);
                            TargetCameraRotation = Quaternion.Euler(rX, rY, rZ);
                            Log("TargetCamPos: " + TargetCameraPosition.ToString());
                            Log("TargetCamRot: " + TargetCameraRotation.ToString());
                            // _helper.UpdateCameraPose(TargetCameraPosition, TargetCameraRotation);
                            break;
                        case "FOV:":
                            float.TryParse(LineInput.Substring(4), out FOV);
                            Log("Desired FOV: " + FOV);
                            // _helper.UpdateFov(FOV);
                            break;
                    }
                }
            }
            _helper.UpdateCameraPose(TargetCameraPosition, TargetCameraRotation);
            _helper.UpdateFov(FOV);
        } catch (Exception ex) {
            Log(ex.ToString());
            throw;
        }
    }

    // OnLateUpdate is called after OnUpdate also everyframe and has a higher chance that transform updates are more recent.
    public void OnLateUpdate() {

    }

    // OnDeactivate is called when the user changes the profile to other camera behaviour or when the application is about to close.
    // The camera behaviour should clean everything it created when the behaviour is deactivated.
    public void OnDeactivate() {
        // Saving settings here
        ApplySettings?.Invoke(this, EventArgs.Empty);
        File.WriteAllText(LogFileName, "Lum's VNyan camera plugin version " + version + " closing");
        
    }

    // OnDestroy is called when the users selects a camera behaviour which is not a plugin or when the application is about to close.
    // This is the last chance to clean after your self.
    public void OnDestroy() {

    }
}


public class UDPSocket {
    private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    private const int bufSize = 8 * 1024;
    private State state = new State();
    private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
    private AsyncCallback recv = null;
    public string _LastMessage;
    public string LastMessage {
        get {
            string temp = _LastMessage;
            _LastMessage = null;
            return temp;
        }
    }

    public class State {
        public byte[] buffer = new byte[bufSize];
    }

    public void Server(string address, int port) {
        _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
        _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
        Receive();
    }

    public void Client(string address, int port) {
        _socket.Connect(IPAddress.Parse(address), port);
        Receive();
    }

    public void Send(string text) {
        byte[] data = Encoding.ASCII.GetBytes(text);
        _socket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) => {
            State so = (State)ar.AsyncState;
            int bytes = _socket.EndSend(ar);
            Console.WriteLine("SEND: {0}, {1}", bytes, text);
        }, state);
    }

    private void Receive() {
        string result = "";
        _socket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) => {
            State so = (State)ar.AsyncState;
            int bytes = _socket.EndReceiveFrom(ar, ref epFrom);
            _socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);
            //VNyanCameraPlugin.Log("RECV: " +epFrom.ToString() + " " + bytes.ToString() + " " + Encoding.ASCII.GetString(so.buffer, 0, bytes));
            _LastMessage = Encoding.ASCII.GetString(so.buffer, 0, bytes);
        }, state);
    }
}
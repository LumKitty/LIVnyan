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
using System.Runtime.InteropServices;
using System.IO.MemoryMappedFiles;
using System.Threading;

// User defined settings which will be serialized and deserialized with Newtonsoft Json.Net.
// Only public variables will be serialized.
public class VNyanCameraPluginSettings : IPluginSettings {
    public bool FromVNyan = true;
    public string LogFileName = "D:\\Dev\\Livcam.log";
    public bool LogEnabled = true;
}

// The class must implement IPluginCameraBehaviour to be recognized by LIV as a plugin.
public class VNyanCameraPlugin : IPluginCameraBehaviour {
    VNyanCameraPluginSettings _settings = new VNyanCameraPluginSettings();

    public IPluginSettings settings => _settings;
    public event EventHandler ApplySettings;
    public string ID => "VNyanCameraPlugin";
    public string name => "VNyan Camera";
    public string author => "LumKitty";
    public string version => "0.4";
    PluginCameraHelper _helper;
    string LogFileName;
    bool LogEnabled = true;
    

    // Constructor is called when plugin loads
    public VNyanCameraPlugin() { }

    // OnActivate function is called when your camera behaviour was selected by the user.
    // The pluginCameraHelper is provided to you to help you with Player/Camera related operations.

    //private UDPSocket UDPServer;
    //private UDPSocket UDPClient;

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
        if (LogEnabled) {
            File.AppendAllText(LogFileName, message + "\r\n");
        }
    }

    //float _elaspedTime;
    private static Mutex mutex;
    private static MemoryMappedFile mmf;
    private static MemoryMappedViewAccessor mmfAccess;
    const int MMFSize = (sizeof(float) * 9);
    private float[] CamData = new float[9];
    private Vector3 CamPos;
    private Quaternion CamRot;
    private float CamFOV;

    [Obsolete]
    public void OnActivate(PluginCameraHelper helper) {
        try {
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string settingLoc = Path.Combine(docPath, @"LIV\Plugins\CameraBehaviours\");
            LogFileName = settingLoc + "LIVNyan.log";
            //if (_settings.LogFileName != "") {
            File.WriteAllText(LogFileName, "");
            //}
            Log("Lum's VNyan camera plugin version " + version + " starting");
            Log("Float size: " + sizeof(float).ToString() + " bytes");
            Log("Bool size: " + sizeof(bool).ToString() + " bytes");
            _helper = helper;
            bool MutexCreated;
            Log("Creating file");
            mmf = MemoryMappedFile.CreateOrOpen("uk.lum.livnyan.cameradata3", MMFSize);
            Log("Creating mutex");
            mutex = new Mutex(true, "uk.lum.livnyan.cameradata.mutex", out MutexCreated);
            Log("Getting mutex");
            mutex.WaitOne();
            Log("Creating accessor");
            mmfAccess = mmf.CreateViewAccessor(0, MMFSize, MemoryMappedFileAccess.Read);
        } catch (Exception ex) {
            Log(ex.ToString());
        }
    }
    public void OnUpdate() {
        try {
            mmfAccess.ReadArray<float>(0, CamData, 0, 9);
            CamPos.x = CamData[0];
            CamPos.y = CamData[1];
            CamPos.z = CamData[2];
            CamRot.w = CamData[3];
            CamRot.x = CamData[4];
            CamRot.y = CamData[5];
            CamRot.z = CamData[6];
            CamFOV   = CamData[7];
            //Log("Read POS: " + CamPos.ToString() + ", ROT: " + CamRot.ToString() + " FOV: " + CamFOV.ToString());
            _helper.UpdateCameraPose(CamPos, CamRot);
            _helper.UpdateFov(CamFOV);
        } catch (Exception ex) {
            Log(ex.ToString());
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
        Log("Lum's VNyan camera plugin version " + version + " closing");
        mmfAccess.Dispose();
        mutex.Close();
        mutex.Dispose();
        mmf.Dispose();
    }

    // OnDestroy is called when the users selects a camera behaviour which is not a plugin or when the application is about to close.
    // This is the last chance to clean after your self.
    public void OnDestroy() {

    }
}

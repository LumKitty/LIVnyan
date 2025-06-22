using System;
using System.Security.Cryptography.X509Certificates;

public class SharedValues {
    public const string PluginName = "LIVnyan";
    public const string Author = "LumKitty";
    public const string Website = "https://lum.uk/";
    public const string Version = "v1.0-RC2";
    public const string MMFname = "uk.lum.livnyan.cameradata." + Version;
    public const int MMFSize = (sizeof(float) * 8)+sizeof(int);

    public const int CAMENABLED = 0x1;
    public const int LOGENABLED = 0x2;
    public const int LOGSPAMENABLED = 0x4;
}

#region

using System.Windows.Forms;
using PyroCommon.PyroFunctions;
using Rage;

#endregion

namespace SuperEvents;

public static class Settings
{
    internal static bool Fight = true;
    internal static bool CarFire = true;
    internal static bool CarAccident = true;
    internal static bool PulloverShooting = true;
    internal static bool RecklessDriver = true;
    internal static bool AbandonedCar = true;
    internal static bool OpenCarry = true;
    internal static bool WildAnimal = true;
    public static bool ShowBlips = true;
    public static bool ShowHints = true;
    public static int TimeBetweenEvents = 300;
    public static Keys Interact = Keys.Y;
    public static Keys EndEvent = Keys.End;

    internal static void LoadSettings()
    {
        var ini = new InitializationFile("Plugins/LSPDFR/SuperEvents.ini");
        ini.Create();
        Fight = ini.ReadBoolean("Events", "Fight", true);
        PulloverShooting = ini.ReadBoolean("Events", "PulloverShooting", true);
        CarFire = ini.ReadBoolean("Events", "CarFire", true);
        CarAccident = ini.ReadBoolean("Events", "CarAccident", true);
        RecklessDriver = ini.ReadBoolean("Events", "RecklessDriver", true);
        AbandonedCar = ini.ReadBoolean("Events", "AbandonedCar", true);
        OpenCarry = ini.ReadBoolean("Events", "OpenCarry", true);
        WildAnimal = ini.ReadBoolean("Events", "WildAnimal", true);
        ShowBlips = ini.ReadBoolean("Settings", "ShowBlips", true);
        ShowHints = ini.ReadBoolean("Settings", "ShowHints", true);
        TimeBetweenEvents = ini.ReadInt32("Settings", "TimeBetweenEvents", 150);
        Interact = ini.ReadEnum("Keys", "Interact", Keys.Y);
        EndEvent = ini.ReadEnum("Keys", "EndEvent", Keys.End);
        PyroCommon.PyroFunctions.UIManager.Manager.AddManagerKey(ini.ReadEnum("Keys", "PluginManager", Keys.K));
    }
}
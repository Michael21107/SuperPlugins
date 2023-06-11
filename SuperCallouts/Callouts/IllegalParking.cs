#region

using System.Drawing;
using CalloutInterfaceAPI;
using LSPD_First_Response.Mod.Callouts;
using PyroCommon.API;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using Functions = LSPD_First_Response.Mod.API.Functions;

#endregion

namespace SuperCallouts.Callouts;

[CalloutInterface("Illegal Parking", CalloutProbability.Medium, "Reports of a vehicle parked illegally", "LOW")]
internal class IllegalParking : Callout
{
    private Blip _cBlip;
    private Vehicle _cVehicle;
    private UIMenuItem _endCall;
    private float _heading;
    private MenuPool _interaction;
    private UIMenu _mainMenu;
    private bool _onScene;
    private Vector3 _spawnPoint;

    public override bool OnBeforeCalloutDisplayed()
    {
        PyroFunctions.FindSideOfRoad(750, 280, out _spawnPoint, out _heading);
        ShowCalloutAreaBlipBeforeAccepting(_spawnPoint, 10f);
        CalloutMessage = "~r~" + Settings.EmergencyNumber + " Report:~s~ Reports of a vehicle parked illegally.";
        CalloutAdvisory = "Caller says a vehicle is parked on their property without permission.";
        CalloutPosition = _spawnPoint;
        Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS_05 WE_HAVE CRIME_11_351_02 IN_OR_ON_POSITION",
            _spawnPoint);
        return base.OnBeforeCalloutDisplayed();
    }

    public override bool OnCalloutAccepted()
    {
        //Setup
        Log.Info("illegally parked car callout accepted...");
        Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "~b~Dispatch", "~y~Traffic",
            "Reports of an empty vehicle on private property, respond ~g~CODE-1");
        //cVehicle
        PyroFunctions.SpawnNormalCar(out _cVehicle, _spawnPoint, _heading);
        //Blip
        _cBlip = _cVehicle.AttachBlip();
        _cBlip.Color = Color.DodgerBlue;
        _cBlip.EnableRoute(Color.DodgerBlue);
        //UI Items
        PyroFunctions.BuildUi(out _interaction, out _mainMenu, out _, out _, out _endCall);
        _mainMenu.OnItemSelect += InteractionProcess;
        return base.OnCalloutAccepted();
    }

    public override void Process()
    {
        //GamePlay
        if (!_onScene && Game.LocalPlayer.Character.DistanceTo(_cVehicle) < 25f)
        {
            _onScene = true;
            CalloutInterfaceAPI.Functions.SendMessage(this, "Arriving on scene. 10-23");
            _cBlip.DisableRoute();
            Game.DisplayHelp($"Press ~{Settings.Interact.GetInstructionalId()}~ to open interaction menu.");
            Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "~b~Investigate The Vehicle", "~y~Traffic",
                "The vehicle appears abandoned. Decide how to deal with it.");
        }

        //Keybinds
        if (Game.IsKeyDown(Settings.EndCall)) End();
        if (Game.IsKeyDown(Settings.Interact)) _mainMenu.Visible = !_mainMenu.Visible;
        _interaction.ProcessMenus();
        base.Process();
    }

    public override void End()
    {
        if (_cBlip) _cBlip.Delete();
        if (_cVehicle) _cVehicle.Dismiss();

        Game.DisplayHelp("Scene ~g~CODE 4", 5000);
        CalloutInterfaceAPI.Functions.SendMessage(this, "Scene clear, Code4");
        base.End();
    }

    private void InteractionProcess(UIMenu sender, UIMenuItem selItem, int index)
    {
        if (selItem == _endCall)
        {
            Game.DisplaySubtitle("~y~Callout Ended.");
            End();
        }
    }
}
#region

using System;
using System.Drawing;
using CalloutInterfaceAPI;
using LSPD_First_Response.Mod.Callouts;
using PyroCommon.API;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using SuperCallouts.SimpleFunctions;
using Functions = LSPD_First_Response.Mod.API.Functions;

#endregion

namespace SuperCallouts.Callouts;

[CalloutInterface("Blocking Traffic", CalloutProbability.Medium, "Vehicle parked in the road", "Code 3")]
internal class BlockingTraffic : Callout
{
    private readonly UIMenuItem _endCall = new("~y~End Callout", "Ends the callout.");
    private readonly MenuPool _interaction = new();
    private readonly UIMenu _mainMenu = new("SuperCallouts", "~y~Choose an option.");
    private Blip _cBlip;
    private Vehicle _cVehicle;
    private bool _onScene;
    private Vector3 _spawnPoint;

    public override bool OnBeforeCalloutDisplayed()
    {
        _spawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(450f));
        ShowCalloutAreaBlipBeforeAccepting(_spawnPoint, 10f);
        CalloutMessage = "~b~Dispatch:~s~ Reports of a vehicle blocking traffic.";
        CalloutAdvisory = "Caller says the vehicle is abandoned in the middle of the road.";
        CalloutPosition = _spawnPoint;
        Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT_04 CRIME_11_351_01 IN_OR_ON_POSITION",
            _spawnPoint);
        return base.OnBeforeCalloutDisplayed();
    }

    public override bool OnCalloutAccepted()
    {
        //Setup
        Log.Info("car blocking traffic callout accepted...");
        Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "~b~Dispatch", "~r~Blocking Traffic",
            "Reports of a car blocking the road, respond ~y~CODE-2");
        //cVehicle
        PyroFunctions.SpawnNormalCar(out _cVehicle, _spawnPoint);
        //Start UI
        _mainMenu.MouseControlsEnabled = false;
        _mainMenu.AllowCameraMovement = true;
        _interaction.Add(_mainMenu);
        _mainMenu.AddItem(_endCall);
        _mainMenu.RefreshIndex();
        _mainMenu.OnItemSelect += Interactions;
        //cBlip
        _cBlip = _cVehicle.AttachBlip();
        _cBlip.Color = Color.Red;
        _cBlip.EnableRoute(Color.Red);
        return base.OnCalloutAccepted();
    }

    public override void Process()
    {
        try
        {
            //GamePlay
            if (!_onScene && Game.LocalPlayer.Character.DistanceTo(_cVehicle) < 25f)
            {
                _onScene = true;
                _cBlip.DisableRoute();
                Game.DisplayHelp($"Press ~{Settings.Interact.GetInstructionalId()}~ to open interaction menu.");
                CalloutInterfaceAPI.Functions.SendMessage(this, "Officer on scene.");
            }

            //Keybinds
            if (Game.IsKeyDown(Settings.EndCall)) End();
            if (Game.IsKeyDown(Settings.Interact)) _mainMenu.Visible = !_mainMenu.Visible;
            _interaction.ProcessMenus();
        }
        catch (Exception e)
        {
Log.Error(e.ToString());
            End();
        }

        base.Process();
    }

    public override void End()
    {
        if (_cBlip.Exists()) _cBlip.Delete();
        if (_cVehicle.Exists()) _cVehicle.Dismiss();
        _mainMenu.Visible = false;
        
        Game.DisplayHelp("Scene ~g~CODE 4", 5000);
        CalloutInterfaceAPI.Functions.SendMessage(this, "Scene clear, Code4");
        base.End();
    }

    private void Interactions(UIMenu sender, UIMenuItem selItem, int index)
    {
        if (selItem == _endCall)
        {
            Game.DisplaySubtitle("~y~Callout Ended.");
            End();
        }
    }
}
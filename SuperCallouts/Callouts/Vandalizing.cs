﻿/*using System.Drawing;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using SuperCallouts.SimpleFunctions;

namespace SuperCallouts.Callouts;

[CalloutInterface("Vandalizing", CalloutProbability.Medium, "Reports of a person vandalizing property", "Code 3")]
internal class Vandalizing : Callout
{
    private CState _state = CState.CheckDistance;
    private UIMenu _convoMenu;
    private UIMenuItem _endCall;
    private MenuPool _interaction;
    private UIMenu _mainMenu;
    private Vector3 _spawnPoint;
    private Vehicle _cVehicle;
    private Ped _bad;
    private Blip _cBlip;
    
        public override bool OnBeforeCalloutDisplayed()
    {
        _spawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(350f));
        ShowCalloutAreaBlipBeforeAccepting(_spawnPoint, 30f);
        CalloutMessage = "~b~Dispatch:~s~ Person vandalizing a vehicle.";
        CalloutAdvisory = "Caller states a person is damaging a parked vehicle.";
        CalloutPosition = _spawnPoint;
        Functions.PlayScannerAudioUsingPosition(
            "WE_HAVE CRIME_SUSPECT_ON_THE_RUN_03 IN_OR_ON_POSITION", _spawnPoint);
        return base.OnBeforeCalloutDisplayed();
    }
    
    public override bool OnCalloutAccepted()
    {
        //Setup
        Log.Info("Vandalizing callout accepted...");
        Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "~b~Dispatch", "~r~Vandalizing",
            "A suspect has been reported damaging a vehicle. Respond ~r~CODE-3");
        Wrapper.CiSendMessage(this, "A call came in about a person attacking a vehicle causing serious damage to it. Further details are unknown.");
        //cVehicle
        PyroFunctions.SpawnNormalCar(out _cVehicle, _spawnPoint);
        //Bad
        _bad = new Ped(_spawnPoint.Around(15f));
        _bad.WarpIntoVehicle(_cVehicle, -1);
        _bad.IsPersistent = true;
        _bad.BlockPermanentEvents = true;
        _bad.Metadata.stpDrugsDetected = true;
        _bad.Metadata.stpAlcoholDetected = true;
        PyroFunctions.SetDrunk(_bad, true);
        //Blip
        _cBlip = _bad.AttachBlip();
        _cBlip.EnableRoute(Color.Red);
        _cBlip.Color = Color.Red;
        _cBlip.Scale = .5f;
        //Task
        _bad.Tasks.CruiseWithVehicle(_cVehicle, 100f, VehicleDrivingFlags.Emergency);
        //UI
        PyroFunctions.BuildUi(out _interaction, out _mainMenu, out _convoMenu, out _, out _endCall);
        _mainMenu.OnItemSelect += Interactions;
        return base.OnCalloutAccepted();
    }
    
    private void Interactions(UIMenu sender, UIMenuItem selItem, int index)
    {
        if (selItem == _endCall)
        {
            Game.DisplaySubtitle("~y~Callout Ended.");
            End();
        }
    }
    
    private enum CState
    {
        CheckDistance,
        OnScene,
        End
    }
}
*/

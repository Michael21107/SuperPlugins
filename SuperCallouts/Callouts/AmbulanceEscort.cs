#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

[CalloutInterface("Ambulance Escort", CalloutProbability.Medium, "Ambulance requires escort", "Code 3")]
internal class AmbulanceEscort : Callout
{
    private readonly UIMenuItem _endCall = new("~y~End Callout", "Ends the callout early.");
    private readonly List<Vector3> _hospitals = new();
    private readonly MenuPool _interaction = new();
    private readonly UIMenu _mainMenu = new("SuperCallouts", "~y~Choose an option.");
    private Blip _cBlip;
    private Blip _cBlip2;
    private Vehicle _cVehicle;
    private Ped _doc1;
    private Ped _doc2;
    private Vector3 _hospital;
    private bool _onScene;
    private Vector3 _spawnPoint;
    private float _spawnPointH;
    private Ped _victim;

    public override bool OnBeforeCalloutDisplayed()
    {
        //Hospital Locations
        _hospitals.Add(new Vector3(1825, 3692, 34));
        _hospitals.Add(new Vector3(-454, -339, 34));
        _hospitals.Add(new Vector3(293, -1438, 29));
        _hospitals.Add(new Vector3(-232, 6316, 30));
        _hospitals.Add(new Vector3(294, -1439, 29));
        _hospital = _hospitals.OrderBy(x => x.DistanceTo(Game.LocalPlayer.Character.Position)).FirstOrDefault();
        //Startup
        PyroFunctions.FindSideOfRoad(400, 70, out _spawnPoint, out _spawnPointH);
        ShowCalloutAreaBlipBeforeAccepting(_spawnPoint, 10f);
        CalloutMessage = "~b~Dispatch:~s~ Ambulance requests police escort.";
        CalloutAdvisory = "Ambulance needs assistance clearing traffic.";
        CalloutPosition = _spawnPoint;
        Functions.PlayScannerAudioUsingPosition(
            "ATTENTION_ALL_UNITS_05 WE_HAVE CRIME_AMBULANCE_REQUESTED_01 IN_OR_ON_POSITION", _spawnPoint);
        return base.OnBeforeCalloutDisplayed();
    }

    public override bool OnCalloutAccepted()
    {
        //Setup
        Log.Info("Ambulance Escort callout accepted...");
        Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "~b~Dispatch", "~r~Ambulance Escort",
            "Ambulance has a wounded police officer in critical condition, ensure the ambulance has a clear path to the nearest hospital, get to the scene! High priority, respond ~y~CODE-3");
        //cVehicle
        _cVehicle = new Vehicle("AMBULANCE", _spawnPoint)
            { Heading = _spawnPointH, IsPersistent = true, IsSirenOn = true };
        //Doc1
        _doc1 = new Ped("s_m_m_paramedic_01", _spawnPoint, 0f) { IsPersistent = true, BlockPermanentEvents = true };
        _doc1.WarpIntoVehicle(_cVehicle, -1);
        //Doc2
        _doc2 = new Ped("s_m_m_paramedic_01", _spawnPoint, 0f) { IsPersistent = true, BlockPermanentEvents = true };
        _doc2.WarpIntoVehicle(_cVehicle, 0);
        //Victim
        _victim = new Ped("s_m_y_hwaycop_01", _spawnPoint, 0f) { IsPersistent = true, BlockPermanentEvents = true };
        _victim.WarpIntoVehicle(_cVehicle, 1);
        //Start UI
        _mainMenu.MouseControlsEnabled = false;
        _mainMenu.AllowCameraMovement = true;
        _interaction.Add(_mainMenu);
        _mainMenu.AddItem(_endCall);
        _mainMenu.RefreshIndex();
        _mainMenu.OnItemSelect += Interactions;
        //cBlip
        _cBlip = _cVehicle.AttachBlip();
        _cBlip.EnableRoute(Color.Green);
        _cBlip.Color = Color.Green;
        return base.OnCalloutAccepted();
    }

    public override void Process()
    {
        try
        {
            //GamePlay
            if (!_onScene && Game.LocalPlayer.Character.DistanceTo(_cVehicle) < 35f)
            {
                _onScene = true;
                Game.DisplayHelp("Ensure the ambulance has a clear path!");
                _cBlip.DisableRoute();
                _doc1.Tasks.DriveToPosition(_cVehicle, _hospital, 20f, VehicleDrivingFlags.Emergency, 10f);
                _cBlip2 = new Blip(_hospital);
                _cBlip2.EnableRoute(Color.Blue);
                _cBlip2.Color = Color.Blue;
                CalloutInterfaceAPI.Functions.SendMessage(this, "Officer on scene, proceed to nearest medical center.");
            }

            if (_cVehicle.DistanceTo(_hospital) < 15f && _onScene)
            {
                _cVehicle.IsSirenSilent = true;
                _doc1.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
                _doc2.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
                _victim.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
                End();
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
        if (_doc1.Exists()) _doc1.Dismiss();
        if (_doc2.Exists()) _doc2.Dismiss();
        if (_victim.Exists()) _victim.Dismiss();
        if (_cVehicle.Exists()) _cVehicle.Dismiss();
        if (_cBlip.Exists()) _cBlip.Delete();
        if (_cBlip2.Exists()) _cBlip2.Delete();
        CalloutInterfaceAPI.Functions.SendMessage(this, "Scene handled. Code 4.");
        _mainMenu.Visible = false;
        
        Game.DisplayHelp("Scene ~g~CODE 4", 5000);
        CalloutInterfaceAPI.Functions.SendMessage(this, "Scene clear, Code4");
        base.End();
    }

    //UI Items
    private void Interactions(UIMenu sender, UIMenuItem selItem, int index)
    {
        if (selItem == _endCall)
        {
            Game.DisplaySubtitle("~y~Callout Ended.");
            End();
        }
    }
}
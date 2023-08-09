#region

using System;
using System.Drawing;
using CalloutInterfaceAPI;
using LSPD_First_Response.Mod.Callouts;
using PyroCommon.API;
using Rage;
using Functions = LSPD_First_Response.Mod.API.Functions;

#endregion

namespace SuperCallouts.Callouts;

[CalloutInterface("[SC] Car Accident3", CalloutProbability.Medium, "Reports of a vehicle crash, limited details", "Code 3")]
internal class CarAccident3 : SuperCallout
{
    private readonly int _choice = new Random().Next(0, 4);
    private Blip _eBlip;
    private Ped _ePed;
    private Ped _ePed2;
    private Vehicle _eVehicle;
    private Vehicle _eVehicle2;
    private float _spawnPointH;
    internal override Vector3 SpawnPoint { get; set; }
    internal override float OnSceneDistance { get; set; } = 25;
    internal override string CalloutName { get; set; } = "Car Accident (3)";

    internal override void CalloutPrep()
    {
        PyroFunctions.FindSideOfRoad(120, 45, out var tempSpawn, out _spawnPointH);
        SpawnPoint = tempSpawn;
        CalloutMessage = "~b~Dispatch:~s~ Reports of a motor vehicle accident.";
        CalloutAdvisory = "Caller reports the drivers are violently arguing.";
        Functions.PlayScannerAudioUsingPosition(
            "CITIZENS_REPORT_04 CRIME_HIT_AND_RUN_03 IN_OR_ON_POSITION UNITS_RESPOND_CODE_03_01",
            SpawnPoint);
    }

    internal override void CalloutAccepted()
    {
        Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~b~Dispatch", "~r~MVA",
            "Reports of a car accident, respond ~r~CODE-3");

        PyroFunctions.SpawnNormalCar(out _eVehicle, SpawnPoint, _spawnPointH);
        PyroFunctions.DamageVehicle(_eVehicle, 200, 200);
        EntitiesToClear.Add(_eVehicle);

        PyroFunctions.SpawnNormalCar(out _eVehicle2, _eVehicle.GetOffsetPositionFront(7f));
        _eVehicle2.Rotation = new Rotator(0f, 0f, 90f);
        PyroFunctions.DamageVehicle(_eVehicle2, 200, 200);
        EntitiesToClear.Add(_eVehicle2);

        _ePed = _eVehicle.CreateRandomDriver();
        _ePed.IsPersistent = true;
        _ePed.BlockPermanentEvents = true;
        EntitiesToClear.Add(_ePed);

        _ePed2 = _eVehicle2.CreateRandomDriver();
        _ePed2.IsPersistent = true;
        _ePed2.BlockPermanentEvents = true;
        EntitiesToClear.Add(_ePed2);

        _eBlip = new Blip(SpawnPoint, 15f);
        _eBlip.Color = Color.Red;
        _eBlip.Alpha /= 2;
        _eBlip.Name = "Callout";
        _eBlip.Flash(500, 8000);
        _eBlip.EnableRoute(Color.Red);
        BlipsToClear.Add(_eBlip);

        Log.Info("Car Accident Scenario #" + _choice);
        switch (_choice)
        {
            case 0: //Peds fight
                _ePed.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                _ePed2.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                break;
            case 1: //Ped Dies, other flees
                _ePed.Kill();
                _ePed2.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                break;
            case 2: //Hit and run
                _ePed2.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                break;
            case 3: //Fire + dead ped.
                _ePed.Kill();
                _ePed2.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                break;
            default:
                CalloutEnd(true);
                break;
        }
    }

    internal override void CalloutOnScene()
    {
        Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept",
            "~y~On Scene",
            "~r~Car Accident", "Investigate the scene.");
        _eBlip.DisableRoute();
        _ePed.BlockPermanentEvents = false;
        _ePed2.BlockPermanentEvents = false;
        switch (_choice)
        {
            case 0: //Peds fight
                _ePed.Tasks.FightAgainst(_ePed2);
                _ePed2.Tasks.FightAgainst(_ePed);
                CalloutInterfaceAPI.Functions.SendMessage(this, "Subjects are fighting!");
                break;
            case 1: //Ped Dies, other flees
                var pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(pursuit, _ePed2);
                Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                CalloutInterfaceAPI.Functions.SendMessage(this,
                    "Subject running on foot, currently in pursuit!");
                break;
            case 2: //Hit and run
                var pursuit2 = Functions.CreatePursuit();
                Functions.AddPedToPursuit(pursuit2, _ePed);
                Functions.SetPursuitIsActiveForPlayer(pursuit2, true);
                _ePed2.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                CalloutInterfaceAPI.Functions.SendMessage(this, "Appears to be a 480, hit and run.");
                break;
            case 3: //Fire + dead ped.
                _ePed2.Tasks.Cower(-1);
                PyroFunctions.FireControl(SpawnPoint.Around2D(7f), 24, true);
                CalloutInterfaceAPI.Functions.SendMessage(this, "We have a fire, and someone is injured!");
                break;
            default:
                End();
                break;
        }
    }
}
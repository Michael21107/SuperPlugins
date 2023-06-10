#region

using System;
using System.Drawing;
using CalloutInterfaceAPI;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using SuperCallouts.SimpleFunctions;
using Functions = LSPD_First_Response.Mod.API.Functions;

#endregion

namespace SuperCallouts.Callouts;

[CalloutInterface("Car Accident", CalloutProbability.Medium, "Reports of a vehicle crash, limited details", "Code 3")]
internal class CarAccident3 : Callout
{
    private readonly int _choice = new Random().Next(0, 4);
    private Blip _eBlip;
    private UIMenuItem _endCall;
    private Ped _ePed;
    private Ped _ePed2;
    private Vehicle _eVehicle;
    private Vehicle _eVehicle2;
    private MenuPool _interaction;
    private UIMenu _mainMenu;
    private Vector3 _spawnPoint;
    private float _spawnPointH;
    private Tasks _tasks = Tasks.CheckDistance;

    public override bool OnBeforeCalloutDisplayed()
    {
        //Setup
        CFunctions.FindSideOfRoad(120, 45, out _spawnPoint, out _spawnPointH);
        ShowCalloutAreaBlipBeforeAccepting(_spawnPoint, 10f);
        CalloutMessage = "~b~Dispatch:~s~ Reports of a motor vehicle accident.";
        CalloutAdvisory = "Caller reports the drivers are violently arguing.";
        CalloutPosition = _spawnPoint;
        Functions.PlayScannerAudioUsingPosition(
            "CITIZENS_REPORT_04 CRIME_HIT_AND_RUN_03 IN_OR_ON_POSITION UNITS_RESPOND_CODE_03_01",
            _spawnPoint);
        return base.OnBeforeCalloutDisplayed();
    }

    public override bool OnCalloutAccepted()
    {
        //Setup
        Game.Console.Print("SuperCallouts Log: car accident callout accepted...");
        Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~b~Dispatch", "~r~MVA",
            "Reports of a car accident, respond ~r~CODE-3");
        //Vehicles
        CFunctions.SpawnNormalCar(out _eVehicle, _spawnPoint, _spawnPointH);
        CFunctions.Damage(_eVehicle, 200, 200);
        CFunctions.SpawnNormalCar(out _eVehicle2, _eVehicle.GetOffsetPositionFront(7f));
        _eVehicle2.Rotation = new Rotator(0f, 0f, 90f);
        CFunctions.Damage(_eVehicle2, 200, 200);
        //Peds
        _ePed = _eVehicle.CreateRandomDriver();
        _ePed.IsPersistent = true;
        _ePed.BlockPermanentEvents = true;
        _ePed2 = _eVehicle2.CreateRandomDriver();
        _ePed2.IsPersistent = true;
        _ePed2.BlockPermanentEvents = true;
        //Blip
        _eBlip = new Blip(_spawnPoint, 15f);
        _eBlip.Color = Color.Red;
        _eBlip.Alpha /= 2;
        _eBlip.Name = "Callout";
        _eBlip.Flash(500, 8000);
        _eBlip.EnableRoute(Color.Red);
        //Randomize
        Game.Console.Print("PragmaticCallouts: Car Accident Scenorio #" + _choice);
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
                End();
                break;
        }

        //UI Items
        CFunctions.BuildUi(out _interaction, out _mainMenu, out _, out _, out _endCall);
        _mainMenu.OnItemSelect += InteractionProcess;
        return base.OnCalloutAccepted();
    }

    public override void Process()
    {
        try
        {
            switch (_tasks)
            {
                case Tasks.CheckDistance:
                    if (Game.LocalPlayer.Character.DistanceTo(_spawnPoint) < 25f)
                    {
                        Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept",
                            "~y~On Scene",
                            "~r~Car Accident", "Investigate the scene.");
                        CalloutInterfaceAPI.Functions.SendMessage(this, "Arriving on scene. 10-23");
                        _tasks = Tasks.OnScene;
                    }

                    break;
                case Tasks.OnScene:
                    _eBlip.DisableRoute();
                    _ePed.BlockPermanentEvents = false;
                    _ePed2.BlockPermanentEvents = false;
                    Game.DisplayHelp($"Press ~{Settings.Interact.GetInstructionalId()}~ to open interaction menu.");
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
                            CalloutInterfaceAPI.Functions.SendMessage(this, "Subject running on foot, currently in pursuit!");
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
                            CFunctions.FireControl(_spawnPoint.Around2D(7f), 24, true);
                            CalloutInterfaceAPI.Functions.SendMessage(this, "We have a fire, and someone is injured!");
                            break;
                        default:
                            End();
                            break;
                    }

                    _tasks = Tasks.End;
                    break;
                case Tasks.End:
                    break;
                default:
                    End();
                    break;
            }

            if (Game.IsKeyDown(Settings.EndCall)) End();
            if (Game.IsKeyDown(Settings.Interact)) _mainMenu.Visible = !_mainMenu.Visible;
            _interaction.ProcessMenus();
        }
        catch (Exception e)
        {
            Game.Console.Print("Oops there was an error here. Please send this log to https://dsc.gg/ulss");
            Game.Console.Print("SuperCallouts Error Report Start");
            Game.Console.Print("======================================================");
            Game.Console.Print(e.ToString());
            Game.Console.Print("======================================================");
            Game.Console.Print("SuperCallouts Error Report End");
            End();
        }

        base.Process();
    }

    public override void End()
    {
        if (_ePed) _ePed.Dismiss();
        if (_ePed2) _ePed2.Dismiss();
        if (_eVehicle) _eVehicle.Dismiss();
        if (_eVehicle2) _eVehicle2.Dismiss();
        if (_eBlip) _eBlip.Delete();
        CFunctions.Code4Message();
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

    private enum Tasks
    {
        CheckDistance,
        OnScene,
        End
    }
}
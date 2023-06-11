#region

using System;
using System.Drawing;
using CalloutInterfaceAPI;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using PyroCommon.API;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;
using SuperCallouts.SimpleFunctions;
using Functions = LSPD_First_Response.Mod.API.Functions;

#endregion

namespace SuperCallouts.Callouts;

[CalloutInterface("Open Carry", CalloutProbability.Low, "Person walking around with an assault rifle", "Code 2")]
internal class OpenCarry : Callout
{
    private readonly UIMenu _convoMenu = new("SuperCallouts", "~y~Choose a subject to speak with.");
    private readonly UIMenuItem _endCall = new("~y~End Callout", "Ends the callout early.");
    private readonly MenuPool _interaction = new();
    private readonly UIMenu _mainMenu = new("SuperCallouts", "~y~Choose an option.");
    private readonly UIMenuItem _questioning = new("Speak With Subjects");
    private readonly Random _rNd = new();
    private readonly UIMenuItem _stopSuspect = new("~r~ Stop Suspect", "Tells the suspect to stop.");
    private Ped _bad1;
    private Blip _cBlip;
    private string _name1;
    private bool _onScene;
    private LHandle _pursuit;
    private Vector3 _spawnPoint;
    private UIMenuItem _speakSuspect;
    private bool _startScene;

    public override bool OnBeforeCalloutDisplayed()
    {
        _spawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(350f));
        ShowCalloutAreaBlipBeforeAccepting(_spawnPoint, 10f);
        CalloutMessage = "~b~Dispatch:~s~ Reports of a person with a firearm.";
        CalloutAdvisory =
            "Caller reports the person is walking around with a firearm out but has not caused any trouble.";
        CalloutPosition = _spawnPoint;
        Functions.PlayScannerAudioUsingPosition(
            "ATTENTION_ALL_UNITS_05 WE_HAVE CRIME_DISTURBING_THE_PEACE_01 IN_OR_ON_POSITION", _spawnPoint);
        return base.OnBeforeCalloutDisplayed();
    }

    public override bool OnCalloutAccepted()
    {
        //Setup
        Log.Info("Open Carry callout accepted...");
        Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "~b~Dispatch", "~r~Person With Gun",
            "Reports of a person walking around with an assault rifle. Respond ~y~CODE-2");
        //Bad
        _bad1 = new Ped(_spawnPoint) { IsPersistent = true };
        _bad1.Inventory.GiveNewWeapon(WeaponHash.AdvancedRifle, -1, true);
        _bad1.Tasks.Wander();
        _name1 = Functions.GetPersonaForPed(_bad1).FullName;
        PyroFunctions.SetDrunk(_bad1, true);
        _bad1.Metadata.stpAlcoholDetected = true;
        _bad1.Metadata.hasGunPermit = false;
        _bad1.Metadata.searchPed = "~r~assaultrifle~s~, ~y~pocket knife~s~, ~g~wallet~s~";
        //Blip
        _cBlip = _bad1.AttachBlip();
        _cBlip.EnableRoute(Color.Red);
        _cBlip.Color = Color.Red;
        //Start UI
        _mainMenu.MouseControlsEnabled = false;
        _mainMenu.AllowCameraMovement = true;
        _interaction.Add(_mainMenu);
        _interaction.Add(_convoMenu);
        _mainMenu.AddItem(_stopSuspect);
        _mainMenu.AddItem(_questioning);
        _mainMenu.AddItem(_endCall);
        _speakSuspect = new UIMenuItem("Speak with ~y~" + _name1);
        _mainMenu.RefreshIndex();
        _convoMenu.RefreshIndex();
        _mainMenu.BindMenuToItem(_convoMenu, _questioning);
        _mainMenu.OnItemSelect += Interactions;
        _convoMenu.OnItemSelect += Conversations;
        _convoMenu.ParentMenu = _mainMenu;
        _questioning.Enabled = false;
        _speakSuspect.Enabled = false;
        _stopSuspect.Enabled = false;
        return base.OnCalloutAccepted();
    }

    public override void Process()
    {
        try
        {
            //Gameplay
            if (!_onScene && Game.LocalPlayer.Character.Position.DistanceTo(_bad1) < 20f)
            {
                Game.DisplayHelp($"Press ~{Settings.Interact.GetInstructionalId()}~ to open interaction menu.");
                _onScene = true;
                _stopSuspect.Enabled = true;
                Game.DisplaySubtitle("~g~You~s~: Hey, stop for a second.");
                _bad1.Tasks.ClearImmediately();
                NativeFunction.Natives.x5AD23D40115353AC(_bad1, Game.LocalPlayer.Character, -1);
            }

            if (_startScene)
            {
                _startScene = false;
                _pursuit = Functions.CreatePursuit();
                _cBlip.DisableRoute();
                var choices = _rNd.Next(1, 6);
                switch (choices)
                {
                    case 1:
                        Game.DisplaySubtitle("~r~Suspect: ~s~I know my rights, leave me alone!", 5000);
                        Functions.AddPedToPursuit(_pursuit, _bad1);
                        Functions.SetPursuitIsActiveForPlayer(_pursuit, true);
                        break;
                    case 2:
                        Game.DisplayNotification("Investigate the person.");
                        _bad1.Tasks.ClearImmediately();
                        _bad1.Inventory.Weapons.Clear();
                        NativeFunction.Natives.x5AD23D40115353AC(_bad1, Game.LocalPlayer.Character, -1);
                        _speakSuspect.Enabled = true;
                        break;
                    case 3:
                        Game.DisplaySubtitle("~r~Suspect: ~s~REEEEEE", 5000);
                        _bad1.Tasks.AimWeaponAt(Game.LocalPlayer.Character, -1);
                        break;
                    case 4:
                        Game.DisplayNotification("Investigate the person.");
                        _bad1.Tasks.ClearImmediately();
                        _bad1.Inventory.Weapons.Clear();
                        NativeFunction.Natives.x5AD23D40115353AC(_bad1, Game.LocalPlayer.Character, -1);
                        _bad1.Metadata.hasGunPermit = true;
                        _speakSuspect.Enabled = true;
                        break;
                    case 5:
                        _bad1.Tasks.FireWeaponAt(Game.LocalPlayer.Character, -1, FiringPattern.FullAutomatic);
                        break;
                    default:
                        Game.DisplayNotification(
                            "An error has been detected! Ending callout early to prevent LSPDFR crash!");
                        End();
                        break;
                }
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
        if (_bad1.Exists()) _bad1.Dismiss();
        if (_cBlip.Exists()) _cBlip.Delete();
        _mainMenu.Visible = false;
        
        Game.DisplayHelp("Scene ~g~CODE 4", 5000);
        CalloutInterfaceAPI.Functions.SendMessage(this, "Scene clear, Code4");
        base.End();
    }

    //UI Items
    private void Interactions(UIMenu sender, UIMenuItem selItem, int index)
    {
        if (selItem == _stopSuspect)
        {
            Game.DisplaySubtitle("~g~You~s~: Hey, I need to speak with you.");
            _stopSuspect.Enabled = false;
            _startScene = true;
        }

        if (selItem == _endCall)
        {
            Game.DisplaySubtitle("~y~Callout Ended.");
            End();
        }
    }

    private void Conversations(UIMenu sender, UIMenuItem selItem, int index)
    {
        if (selItem == _speakSuspect)
            GameFiber.StartNew(delegate
            {
                Game.DisplaySubtitle(
                    "~g~You~s~: I'm with the police. What is the reason for carrying your weapon out?", 5000);
                NativeFunction.Natives.x5AD23D40115353AC(_bad1, Game.LocalPlayer.Character, -1);
                GameFiber.Wait(5000);
                _bad1.PlayAmbientSpeech("GENERIC_CURSE_MED");
                Game.DisplaySubtitle(
                    "~r~" + _name1 + "~s~: It's my right officer. Nobody can tell me I can't have my gun.''", 5000);
                GameFiber.Wait(5000);
                Game.DisplaySubtitle(
                    "~g~You~s~: Alright, I understand your rights and with the proper license you can open carry, but you cannot carry your weapon in your hands like that.",
                    5000);
                GameFiber.Wait(5000);
                Game.DisplaySubtitle("~r~" + _name1 + "~s~: I don't see why not!", 5000);
                GameFiber.Wait(5000);
                Game.DisplaySubtitle(
                    "~g~You~s~: It's the law, as well as it scares people to see someone walking around with a rifle in their hands. There's no reason to. Do you have a  for it?",
                    5000);
                GameFiber.Wait(5000);
                Game.DisplaySubtitle("~r~" + _name1 + "~s~: Check for yourself.", 5000);
            });
    }
}
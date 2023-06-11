﻿#region

using System;
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

[CalloutInterface("Call Dropped", CalloutProbability.Medium, "911 call dropped - conduct wellness check", "LOW")]
internal class FakeCall : Callout
{
    private Blip _cBlip;
    private UIMenuItem _endCall;
    private MenuPool _interaction;
    private UIMenu _mainMenu;
    private bool _onScene;
    private Vector3 _spawnPoint;

    public override bool OnBeforeCalloutDisplayed()
    {
        PyroFunctions.FindSideOfRoad(750, 280, out _spawnPoint, out _);
        ShowCalloutAreaBlipBeforeAccepting(_spawnPoint, 10f);
        CalloutMessage = "~r~" + Settings.EmergencyNumber + " Report:~s~ Emergency call dropped.";
        CalloutAdvisory = "Call dropped and dispatch is unable to reach caller back.";
        CalloutPosition = _spawnPoint;
        Functions.PlayScannerAudioUsingPosition(
            "ATTENTION_ALL_UNITS_05 WE_HAVE CRIME_11_351_02 IN_OR_ON_POSITION",
            _spawnPoint);
        return base.OnBeforeCalloutDisplayed();
    }

    public override bool OnCalloutAccepted()
    {
        //Setup
        Log.Info("Dead body callout accepted...");
        Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "~b~Dispatch", "~y~Call Dropped",
            "Caller disconnected from call quickly. Unable to reach them back. Last location recorded, respond to the last known location. ~r~CODE-2");
        //cBlip
        _cBlip = new Blip(_spawnPoint);
        _cBlip.Color = Color.Red;
        _cBlip.EnableRoute(Color.Red);
        //UI
        PyroFunctions.BuildUi(out _interaction, out _mainMenu, out _, out _, out _endCall);
        _mainMenu.OnItemSelect += InteractionProcess;
        return base.OnCalloutAccepted();
    }

    public override void Process()
    {
        try
        {
            if (!_onScene && Game.LocalPlayer.Character.Position.DistanceTo(_spawnPoint) < 60)
            {
                _onScene = true;
                _cBlip.DisableRoute();
                Game.DisplayHelp("Investigate the area.", 5000);
                GameFiber.StartNew(delegate
                {
                    GameFiber.Wait(10000);
                    Game.DisplaySubtitle(
                        "~g~You~s~: Dispatch, not seeing anyone out here.",
                        4000);
                    GameFiber.Wait(4000);
                    Functions.PlayScannerAudioUsingPosition(
                        "REPORT_RESPONSE_COPY_02",
                        _spawnPoint);
                    GameFiber.Wait(3500);
                    CalloutInterfaceAPI.Functions.SendMessage(this,
                        "Area has been checked, appears to be a fake call.");
                    End();
                });
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
        _mainMenu.Visible = false;

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
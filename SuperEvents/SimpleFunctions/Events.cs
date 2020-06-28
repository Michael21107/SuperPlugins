﻿#region

using System;
using LSPD_First_Response.Mod.API;
using Rage;
using SuperEvents.Events;

#endregion

namespace SuperEvents.SimpleFunctions
{
    public class Events : AmbientEvent
    {
        private static readonly Random RNd = new Random();

        public static void InitEvents()
        {
            try
            {
                GameFiber.StartNew(delegate
                {
                    while (true)
                    {
                        GameFiber.Wait(500);
                        if (!Functions.IsCalloutRunning() && !Functions.IsPlayerPerformingPullover() && Functions.GetActivePursuit() == null && TimeStart && !EventsActive)
                        {
                            GameFiber.Yield();
                            if (!Functions.IsCalloutRunning() && !Functions.IsPlayerPerformingPullover() && Functions.GetActivePursuit() == null && TimeStart && !EventsActive)
                            {
                                Game.LogTrivial("SuperEvents: Generating random event.");
                                var choices = RNd.Next(1, 12);
                            
                                switch(choices)
                                {
                                    case 1:
                                        if (Settings.Fight)
                                        {
                                            Game.LogTrivial("SuperEvents: Starting fight event.");
                                            TimeStart = false;
                                        }
                                        else { Game.LogTrivial("SuperEvents: Fight event disabled in config.. Trying again for another event."); }
                                        break;
                                    case 2:
                                        if (Settings.OpenCarry)
                                        {
                                            Game.LogTrivial("SuperEvents: Starting open carry event.");
                                            OpenCarry.Launch();
                                            TimeStart = false;
                                        }
                                        else { Game.LogTrivial("SuperEvents: OpenCarry event disabled in config.. Trying again for another event."); }
                                        break;
                                    case 3:
                                        if (Settings.PulloverShooting)
                                        {
                                            Game.LogTrivial("SuperEvents: Starting pullover shooting event.");
                                            TimeStart = false;
                                        }
                                        else { Game.LogTrivial("SuperEvents: PulloverShooting event disabled in config.. Trying again for another event."); }
                                        break;
                                    case 4:
                                        if (Settings.CarFire)
                                        {
                                            Game.LogTrivial("SuperEvents: Starting car fire event.");
                                            TimeStart = false;
                                        }
                                        else { Game.LogTrivial("SuperEvents: CarFire event disabled in config.. Trying again for another event."); }
                                        break;
                                    case 5:
                                        if (Settings.CarAccident)
                                        {
                                            Game.LogTrivial("SuperEvents: Starting car accident event.");
                                            CarAccident.Launch();
                                            TimeStart = false;
                                        }
                                        else { Game.LogTrivial("SuperEvents: car accident event disabled in config.. Trying again for another event."); }
                                        break;
                                    case 6:
                                        if (Settings.InjuredPed)
                                        {
                                            Game.LogTrivial("SuperEvents: Starting injured ped event.");
                                            InjuredPed.Launch();
                                            TimeStart = false;
                                        }
                                        else { Game.LogTrivial("SuperEvents: injured ped event disabled in config.. Trying again for another event."); }
                                        break;
                                    case 7:
                                        if (Settings.RecklessDriver)
                                        {
                                            Game.LogTrivial("SuperEvents: Starting reckless driver event.");
                                            RecklessDriver.Launch();
                                            TimeStart = false;
                                        }
                                        else { Game.LogTrivial("SuperEvents: reckless driver event disabled in config.. Trying again for another event."); }
                                        break;
                                    case 8:
                                        if (Settings.SuicidalPed)
                                        {
                                            Game.LogTrivial("SuperEvents: Starting suicidal peds event.");
                                            SuicidalPed.Launch();
                                            TimeStart = false;
                                        }
                                        else { Game.LogTrivial("SuperEvents: suicidal peds event disabled in config.. Trying again for another event."); }
                                        break;
                                    case 9:
                                        if (Settings.Mugging)
                                        {
                                            Game.LogTrivial("SuperEvents: Starting mugging event.");
                                            Mugging.Launch();
                                            TimeStart = false;
                                        }
                                        else { Game.LogTrivial("SuperEvents: mugging event disabled in config.. Trying again for another event."); }
                                        break;
                                    case 10:
                                        if (Settings.RoadRage)
                                        {
                                            Game.LogTrivial("SuperEvents: Starting road rage event.");
                                            RoadRage.Launch();
                                            TimeStart = false;
                                        }
                                        else { Game.LogTrivial("SuperEvents: road rage event disabled in config.. Trying again for another event."); }
                                        break;
                                    case 11:
                                        if (Settings.WildAnimal)
                                        {
                                            Game.LogTrivial("SuperEvents: Starting wild animal event.");
                                            WildAnimal.Launch();
                                            TimeStart = false;
                                        }
                                        else { Game.LogTrivial("SuperEvents: wild animal event disabled in config.. Trying again for another event."); }
                                        break;
                                    default:
                                        Game.LogTrivial("SuperEvents: If you see this error please tell SuperPyroManiac he is a fool. This error should never pop up unless I forget how to count.");
                                        break;
                                }
                            }else
                            {
                                GameFiber.Wait(10000);
                            }
                        }
                    }
                });
            }
            catch (Exception e)
            {
                        Game.LogTrivial("Oops there was a MAJOR error here. Please send this log to SuperPyroManiac!");
                        Game.LogTrivial("SuperEvents Error Report Start");
                        Game.LogTrivial("======================================================");
                        Game.LogTrivial(e.ToString());
                        Game.LogTrivial("======================================================");
                        Game.LogTrivial("SuperEvents Error Report End");
                        Game.DisplaySubtitle("~r~SuperEvents: Plugin has found a major error. Please send your RagePluginHook.log to SuperPyroManiac on the LSPDFR website!");
            }
        }
    }
}

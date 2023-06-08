﻿using System;
using PyroCommon.Events;
using Rage;

namespace SuperEvents.Events;

internal class WildAnimal : AmbientEvent
{
    private Tasks _tasks = Tasks.CheckDistance;
    private Ped _animal;
    private Vector3 _spawnPoint;

    protected internal override void StartEvent()
    {
        //Ped
        _spawnPoint = World.GetNextPositionOnStreet(Player.Position.Around(150f));
        EventLocation = _spawnPoint;
        Model[] meanAnimal = {"A_C_MTLION", "A_C_COYOTE"};
        _animal = new Ped(meanAnimal[new Random().Next(meanAnimal.Length)], _spawnPoint, 50) {IsPersistent = true};
        base.StartEvent();
    }

    protected internal override void Process()
    {
        try
        {
            switch (_tasks)
            {
                case Tasks.CheckDistance:
                    if (Player.DistanceTo(_animal) < 20f)
                    {
                        _animal.Tasks.FightAgainst(Player);
                        if (Settings.ShowHints)
                            Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "~y~Officer Sighting",
                                "~r~Wild Animal", "Stop the animal from hurting anyone.");
                        Game.DisplayHelp("~y~Press ~r~" + Settings.Interact + "~y~ to open interaction menu.");
                        _tasks = Tasks.OnScene;
                    }

                    break;
                case Tasks.OnScene:
                    break;
                default:
                    End(true);
                    break;
            }

            base.Process();
        }
        catch (Exception e)
        {
            Game.LogTrivial("Oops there was an error here. Please send this log to https://dsc.gg/ulss");
            Game.LogTrivial("SuperEvents Error Report Start");
            Game.LogTrivial("======================================================");
            Game.LogTrivial(e.ToString());
            Game.LogTrivial("======================================================");
            Game.LogTrivial("SuperEvents Error Report End");
            End(true);
        }
    }

    private enum Tasks
    {
        CheckDistance,
        OnScene
    }
}
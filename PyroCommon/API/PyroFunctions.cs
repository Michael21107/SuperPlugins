﻿using System;
using System.Collections.Generic;
using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;

namespace PyroCommon.API;

public class PyroFunctions
{
    private static readonly TupleList<Vector3, float> SideOfRoads = new();
    private static Tuple<Vector3, float> _chosenSpawnData;
    private static readonly Random RNd = new();
    
    public static void SpawnNormalCar(out Vehicle cVehicle, Vector3 spawnPoint) //Spawn normal random car..
        {
            Model[] vehicleModels =
            {
                "PRAIRIE", "EXEMPLAR", "ORACLE", "PREVION", "BUFFALO4", "DOMINATOR", "DOMINATOR3", "GAUNTLET",
                "GAUNTLET4", "ASTEROPE", "CINQUEMILA", "PREMIER", "SCHAFTER2", "TAILGATER", "WASHINGTON", "BUFFALO",
                "BUFFALO2", "CALICO", "SULTAN", "SULTAN2", "BALLER2", "GRANGER2", "BURRITO3", "SPEEDO", "BLISTA",
                "KANJO", "F620", "FELON", "JACKAL", "ORACLE2", "SENTINEL", "SENTINEL2", "ZION", "KANJOSJ", "RUINER",
                "ELLIE", "VIGERO2", "KOMODA", "VECTRE", "CAVALCADE", "CAVALCADE2", "REBLA", "SEMINOLE2", "ASTRON",
                "ASTRON2"
            };
            cVehicle = new Vehicle(vehicleModels[new Random().Next(vehicleModels.Length)], spawnPoint);
            cVehicle.IsPersistent = true;
        }

    public static void SpawnAnyCar(out Vehicle cVehicle, Vector3 spawnPoint) //Spawn ANY random car..
        {
            Model[] vehicleModels =
            {
                "NINFEF2", "BUS", "COACH", "AIRBUS", "AMBULANCE", "BARRACKS", "BARRACKS2", "BALLER", "BALLER2",
                "BANSHEE", "BJXL", "BENSON", "BOBCATXL", "BUCCANEER", "BUFFALO", "BUFFALO2", "BULLDOZER", "BULLET",
                "BURRITO", "BURRITO2", "BURRITO3", "BURRITO4", "BURRITO5", "CAVALCADE", "CAVALCADE2", "POLICET",
                "GBURRITO", "CAMPER", "CARBONIZZARE", "CHEETAH", "COMET2", "COGCABRIO", "COQUETTE", "GRESLEY", "DUNE2",
                "HOTKNIFE", "DUBSTA", "DUBSTA2", "DUMP", "DOMINATOR", "EMPEROR", "EMPEROR2", "EMPEROR3", "ENTITYXF",
                "EXEMPLAR", "ELEGY2", "F620", "FBI", "FBI2", "FELON", "FELON2", "FELTZER2", "FIRETRUK", "FQ2",
                "FUGITIVE", "FUTO", "GRANGER", "GAUNTLET", "HABANERO", "INFERNUS", "INTRUDER", "JACKAL", "JOURNEY",
                "JB700", "KHAMELION", "LANDSTALKER", "MESA", "MESA2", "MESA3", "MIXER", "MINIVAN", "MIXER2", "MULE",
                "MULE2", "ORACLE", "ORACLE2", "MONROE", "PATRIOT", "PBUS", "PACKER", "PENUMBRA", "PEYOTE", "POLICE",
                "POLICE2", "POLICE3", "POLICE4", "PHANTOM", "PHOENIX", "PICADOR", "POUNDER", "PRANGER", "PRIMO",
                "RANCHERXL", "RANCHERXL2", "RAPIDGT", "RAPIDGT2", "RENTALBUS", "RUINER", "RIOT", "RIPLEY", "SABREGT",
                "SADLER", "SADLER2", "SANDKING", "SANDKING2", "SHERIFF", "SHERIFF2", "SPEEDO", "SPEEDO2", "STINGER",
                "STOCKADE", "STINGERGT", "SUPERD", "STRATUM", "SULTAN", "AKUMA", "PCJ", "FAGGIO2", "DAEMON", "BATI2"
            };
            cVehicle = new Vehicle(vehicleModels[new Random().Next(vehicleModels.Length)], spawnPoint);
            cVehicle.IsPersistent = true;
        }
    
    public static void DamageVehicle(Vehicle vehicle, float radius, float amount)
    {
        var model = vehicle.Model;
        model.GetDimensions(out var vector31, out var vector32);
        var num = new Random().Next(10, 45);
        for (var index = 0; index < num; ++index)
        {
            var randomInt1 = MathHelper.GetRandomSingle(vector31.X, vector32.X);
            var randomInt2 = MathHelper.GetRandomSingle(vector31.Y, vector32.Y);
            var randomInt3 = MathHelper.GetRandomSingle(vector31.Z, vector32.Z);
            vehicle.Deform(new Vector3(randomInt1, randomInt2, randomInt3), radius, amount);
        }
    }
    
    public static Ped SetWanted(Ped ped, bool isWanted) //Used to set a ped as wanted.
    {
        if (!ped.Exists()) return null;
        var thePersona = Functions.GetPersonaForPed(ped);
        thePersona.Wanted = true;
        return ped;
    }
    
    public static void SetDrunk(Ped ped, bool isDrunk)
    {
        GameFiber.StartNew(delegate
        {
            GameFiber.Yield();
            if (!ped.Exists()) return;
            ped.Metadata.stpAlcoholDetected = isDrunk;
            var drunkAnimset = new AnimationSet("move_m@drunk@verydrunk");
            drunkAnimset.LoadAndWait();
            ped.MovementAnimationSet = drunkAnimset;
            NativeFunction.Natives.x95D2D383D5396B8A(ped, isDrunk);
        });
    }
    internal static void FindSideOfRoad(int maxDistance, int minDistance, out Vector3 spawnPoint,
        out float spawnPointH)
    {
        foreach (var tuple in API.PulloverSpots.SideOfRoad)
            if (Vector3.Distance(tuple.Item1, Game.LocalPlayer.Character.Position) < maxDistance &&
                Vector3.Distance(tuple.Item1, Game.LocalPlayer.Character.Position) > minDistance)
                SideOfRoads.Add(tuple);
        if (SideOfRoads.Count == 0)
        {
            Game.LogTrivial("SuperEvents: Failed to find valid spawnpoint. Spawning on road.");
            spawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(45f, 100f));
            spawnPointH = 0;
        }
        else
        {
            _chosenSpawnData = SideOfRoads[RNd.Next(SideOfRoads.Count)];
            //_sideOfRoads.OrderBy(x => x.Item1.DistanceTo(Game.LocalPlayer.Character.Position)).FirstOrDefault();
            spawnPoint = _chosenSpawnData.Item1;
            spawnPointH = _chosenSpawnData.Item2;
        }
    }
}
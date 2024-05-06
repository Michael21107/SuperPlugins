﻿using System;
using PolicingRedefined.Interaction.Assets.PedAttributes;
using PolicingRedefined.Interaction.Modules.Resistance;
using Rage;

namespace PyroCommon.API.Wrappers;

public static class PedInfo
{
    internal static void SetDrunk(Ped ped, Enums.DrunkState drunkState)
    {
        PolicingRedefined.API.PedAPI.SetPedDrunk(ped, (EDrunkLevel)drunkState);
    }

    internal static void SetPermit(Ped ped, Enums.Permits permit, Enums.PermitStatus status)
    {
        switch (permit)
        {
            case Enums.Permits.Guns:
                PolicingRedefined.API.PedDocumentationAPI.SetWeaponPermitStatus(ped, (EDocumentStatus)status);
                break;
            case Enums.Permits.Hunting:
                PolicingRedefined.API.PedDocumentationAPI.SetHuntingPermitStatus(ped, (EDocumentStatus)status);
                break;
            case Enums.Permits.Fishing:
                PolicingRedefined.API.PedDocumentationAPI.SetFishingPermitStatus(ped, (EDocumentStatus)status);
                break;
            case Enums.Permits.Drivers:
            default:
                throw new ArgumentOutOfRangeException(nameof(permit), permit, null);
        }
    }

    internal static void SetResistance(Ped ped, Enums.ResistanceAction resistanceAction, bool walkAway, int resistanceChance = 50)
    {
        PolicingRedefined.API.PedAPI.SetPedResistanceChance(ped, resistanceChance);
        PolicingRedefined.API.PedAPI.SetShouldWalkAwayBeforeResisting(ped, walkAway);
        PolicingRedefined.API.PedAPI.SetPedResistanceAction(ped, (EResistanceAction)resistanceAction);
    }
}
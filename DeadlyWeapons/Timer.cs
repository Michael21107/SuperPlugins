using System;
using LSPD_First_Response;
using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;

namespace DeadlyWeapons
{
    internal static class Timer
    {
        private static bool _panic;
        internal static void Ragdoll(Ped ped)
        {
            try
            {
                GameFiber.StartNew(delegate
                {
                    ped.IsRagdoll = true;
                    GameFiber.Wait(2000);
                    ped.IsRagdoll = false;
                });
            }
            catch (Exception e)
            {
                Game.LogTrivial("Deadly Weapons: Unable to remove ragdoll due to player death.");
            }
        }

        internal static void Panic()
        {
            if (_panic) return;
            _panic = true;
            GameFiber.StartNew(delegate
            {
                try
                {
                    UltimateBackup.API.Functions.callCode3Backup();
                }
                catch (Exception)
                {
                    Game.LogTrivial("Deadly Weapons: Ultimate Backup not installed - Using default LSPDFR backup!");
                    Functions.RequestBackup(Game.LocalPlayer.Character.Position, EBackupResponseType.Code3,
                        EBackupUnitType.LocalUnit);
                }
                Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "~r~Shots Fired", "~y~Panic Activated", "Your weapon has been discharged. Dispatch has been alerted.");

                GameFiber.Wait(Settings.PanicCooldown * 1000);
                _panic = false;
            });
        }

        internal static void PedAi(Ped ped)
        {
            try
            {
                GameFiber.StartNew(delegate
                {
                    if (!ped || ped.IsDead) return;
                    ped.Accuracy = 20;
                    if (ped.IsShooting)
                    {
                        GameFiber.Wait(500);
                        if (!ped.Metadata.choiceMade)
                        {
                            var rnd = new Random().Next(0, 5);
                            switch (rnd)
                            {
                                case 0:
                                    ped.Tasks.ClearImmediately();
                                    ped.Tasks.Flee(Game.LocalPlayer.Character, 30, 20000);
                                    break;
                            }
                            ped.Metadata.choiceMade = true;
                        }
                    }
                
                    foreach(var w in DeadlyWeapons.WeaponHashes)
                    {
                        if(NativeFunction.Natives.HAS_ENTITY_BEEN_DAMAGED_BY_WEAPON<bool>(ped, (uint) w, 0) && Settings.EnableDamageSystem)
                        {
                            if (ped.Armor >= 60)
//                         If player has armor:
//                         20% chance to be fine, and armor survive for 1 more use.
//                         10% chance to fall over.
//                         10% chance to injure player.
//                         80% chance for armor to be destroyed.
                                {
                                    var rnd = new Random().Next(0, 10);
                                    switch (rnd)
                                    {
                                        case 1:
                                            ped.Health = 100;
                                            ped.Armor = 61;
                                            break;
                                        case 2:
                                            ped.Health = 100;
                                            ped.Armor = 61;
                                            Ragdoll(ped);
                                            break;
                                        case 3:
                                            ped.Health = 80;
                                            ped.Armor = 0;
                                            break;
                                        default:
                                            ped.Health = 100;
                                            ped.Armor = 0;
                                            break;
                                    }
                                }else 
//                         If player has no armor:
//                         30% chance to loose half health and fall.
//                         10% chance to die.
//                         60% chance to loose 80% of health.
                                {
                                    var rnd = new Random().Next(0, 10);
                                    switch (rnd)
                                    {
                                        case 1:
                                            ped.Health -= 50;
                                            Ragdoll(ped);
                                            break;
                                        case 2:
                                            goto case 1;
                                        case 3:
                                            goto case 1;
                                        case 4:
                                            ped.Kill();
                                            break;
                                        default:
                                            ped.Health -= 80;
                                            break;
                                    }
                                }
                                NativeFunction.Natives.CLEAR_ENTITY_LAST_WEAPON_DAMAGE(ped);
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Game.LogTrivial("Oops there was an error here. Please send this log to SuperPyroManiac!");
                Game.LogTrivial("Deadly Weapons Error Report Start");
                Game.LogTrivial("======================================================");
                Game.LogTrivial(e.ToString());
                Game.LogTrivial("======================================================");
                Game.LogTrivial("Deadly Weapons Error Report End");
            }
        }
    }
}
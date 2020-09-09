using System;
using DeadlyWeapons2.DFunctions;
using Rage;
using Rage.Native;

namespace DeadlyWeapons2.Modules
{
    internal class PlayerShot
    {
        private Ped Player => Game.LocalPlayer.Character;
        private GameFiber _playerShotFiber;

        internal void StartEvent()
        {
            _playerShotFiber = new GameFiber(delegate
            {
                while (true)
                {
                    PlayerShotEvent();
                    GameFiber.Yield();
                }
            });
            Game.LogTrivial("Deadly Weapons: Starting PlayerShotFiber.");
            _playerShotFiber.Start();
        }

        private void PlayerShotEvent()
        {
            foreach (var w in WeaponHashs.WeaponHashes)
                if (NativeFunction.Natives.HAS_ENTITY_BEEN_DAMAGED_BY_WEAPON<bool>(Player, (uint) w, 0) &&
                    Settings.EnableDamageSystem)
                {
                    var rnd = new Random().Next(1,5);

                    if (Player.LastDamageBone == PedBoneId.Head)
                    {
                        Player.Health -= Settings.HeadshotDamange;
                        Game.LogTrivial("Deadly Weapons: Player shot in head.");
                    }

                    if (Player.Armor >= 5)
                    {
                        Game.LogTrivial("Deadly Weapons: Player shot, chose: 1 - " + rnd);

                        switch (rnd)
                        {
                            case 1:
                                Player.Armor = 0;
                                break;
                            case 2:
                                Player.Health -= 25;
                                Player.Armor = 0;
                                break;
                            case 3:
                                Player.Armor -= 35;
                                break;
                            case 4:
                                Player.Armor -= 50;
                                break;
                        }
                    }
                    
                    if (Player.Armor < 5)
                    {
                        Game.LogTrivial("Deadly Weapons: Player shot, chose: 0 - " + rnd);

                        switch (rnd)
                        {
                            case 1:
                                Player.Health = 5;
                                break;
                            case 2:
                                Player.Kill();
                                break;
                            case 3:
                                Player.Health -= 40;
                                break;
                            case 4:
                                Player.Health -= 50;
                                break;
                        }
                    }

                    NativeFunction.Natives.CLEAR_ENTITY_LAST_WEAPON_DAMAGE(Player);
                }
        }
    }
}
using System;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using System.Drawing;
using LSPD_First_Response;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace SuperCallouts.Callouts
{
    [CalloutInfo("OfficerShootout", CalloutProbability.Medium)]
    internal class OfficerShootout : Callout
    {
        #region Variables
        private Ped _bad1;
        private Ped _bad2;
        private Blip _cBlip;
        private Ped _cop1;
        private Ped _cop2;
        private Vehicle _copVehicle;
        private Vector3 _cSpawnPoint;
        private Vehicle _cVehicle;
        private bool _onScene;
        private readonly Random _rNd = new Random();
        private Vector3 _spawnPoint;
        private float _spawnPointH;
        //UI Items
        private readonly MenuPool _interaction = new MenuPool();
        private readonly UIMenu _mainMenu = new UIMenu("SuperCallouts", "~y~Choose an option.");
        private readonly UIMenuItem _endCall = new UIMenuItem("~y~End Callout", "Ends the callout early.");
        #endregion

        public override bool OnBeforeCalloutDisplayed()
        {
            SimpleFunctions.CFunctions.FindSideOfRoad(400, 100, out _spawnPoint, out _spawnPointH);
            ShowCalloutAreaBlipBeforeAccepting(_spawnPoint, 10f);
            CalloutMessage = "~b~Dispatch:~s~ Felony stop. Shots fired.";
            CalloutAdvisory = "Panic alert issues, shots fired.";
            CalloutPosition = _spawnPoint;
            Functions.PlayScannerAudioUsingPosition(
                "ATTENTION_ALL_UNITS_05 WE_HAVE CRIME_SHOTS_FIRED_AT_AN_OFFICER_01 IN_OR_ON_POSITION UNITS_RESPOND_CODE_99_02",
                _spawnPoint);
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            //Setup
            Game.LogTrivial("SuperCallouts Log: Officer Shootout accepted...");
            Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "~b~Dispatch", "~r~Officer Shot",
                "Officer reports shots fired during felony stop, panic button hit. Respond ~r~CODE-99 EMERGENCY");
            //cVehicle
            SimpleFunctions.CFunctions.SpawnNormalCar(out _cVehicle, _spawnPoint);
            _cVehicle.Heading = _spawnPointH;
            _cSpawnPoint = _cVehicle.GetOffsetPositionFront(-9f);
            _cVehicle.IsStolen = true;
            //copVehicle
            _copVehicle = new Vehicle("POLICE", _cSpawnPoint)
            {
                IsPersistent = true, Heading = _spawnPointH, IsSirenOn = true, IsSirenSilent = true
            };
            //bad1
            _bad1 = new Ped {IsPersistent = true, Health = 400};
            _bad1.Inventory.Weapons.Add(WeaponHash.AssaultShotgun).Ammo = -1;
            _bad1.WarpIntoVehicle(_cVehicle, -1);
            _bad1.RelationshipGroup = new RelationshipGroup("BADGANG");
            SimpleFunctions.CFunctions.SetWanted(_bad1, true);
            _bad1.Tasks.LeaveVehicle(_cVehicle, LeaveVehicleFlags.LeaveDoorOpen);
            //bad2
            _bad2 = new Ped {IsPersistent = true, Health = 400};
            _bad2.Inventory.Weapons.Add(WeaponHash.CarbineRifle).Ammo = -1;
            _bad2.WarpIntoVehicle(_cVehicle, 0);
            _bad2.RelationshipGroup = new RelationshipGroup("BADGANG");
            SimpleFunctions.CFunctions.SetWanted(_bad2, true);
            _bad2.Tasks.LeaveVehicle(_cVehicle, LeaveVehicleFlags.LeaveDoorOpen);
            //cop1
            _cop1 = new Ped("s_m_y_cop_01", _spawnPoint, 0f) {IsPersistent = true};
            _cop1.WarpIntoVehicle(_copVehicle, -1);
            _cop1.Inventory.Weapons.Add(WeaponHash.CombatPistol).Ammo = -1;
            _cop1.Tasks.LeaveVehicle(_copVehicle, LeaveVehicleFlags.LeaveDoorOpen);
            //cop2
            _cop2 = new Ped("s_f_y_cop_01", _spawnPoint, 0f) {IsPersistent = true};
            _cop2.WarpIntoVehicle(_copVehicle, 0);
            _cop2.Inventory.Weapons.Add(WeaponHash.CombatPistol).Ammo = -1;
            _cop2.Tasks.LeaveVehicle(_copVehicle, LeaveVehicleFlags.LeaveDoorOpen);
            //Blips
            _cBlip = _copVehicle.AttachBlip();
            _cBlip.Color = Color.Red;
            _cBlip.EnableRoute(Color.Red);
            //Start UI
            _mainMenu.MouseControlsEnabled = false;
            _mainMenu.AllowCameraMovement = true;
            _interaction.Add(_mainMenu);
            _mainMenu.AddItem(_endCall);
            _mainMenu.RefreshIndex();
            _mainMenu.OnItemSelect += Interactions;
            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            try
            {
                //Gameplay
                if (!_onScene && Game.LocalPlayer.Character.DistanceTo(_copVehicle.Position) < 50f)
                {
                    _onScene = true;
                    _cop1.Tasks.FightAgainst(_bad1, 60000);
                    _bad1.Tasks.FightAgainst(_cop1, 60000);
                    _cop2.Tasks.FightAgainst(_bad2, 60000);
                    _bad2.Tasks.FightAgainst(_cop2, 60000);
                    Functions.PlayScannerAudioUsingPosition("REQUEST_BACKUP", _spawnPoint);
                    Game.SetRelationshipBetweenRelationshipGroups("BADGANG", "COP", Relationship.Hate);
                    Game.SetRelationshipBetweenRelationshipGroups("BADGANG", "PLAYER", Relationship.Hate);
                    Functions.RequestBackup(_copVehicle.Position, EBackupResponseType.Code3,
                        EBackupUnitType.LocalUnit);
                    Functions.RequestBackup(_copVehicle.Position, EBackupResponseType.Code3,
                        EBackupUnitType.LocalUnit);
                    _cBlip.DisableRoute();
                }
                //Keybinds
                if (Game.IsKeyDown(Settings.EndCall)) End();
                if (Game.IsKeyDown(Settings.Interact))
                {
                    _mainMenu.Visible = !_mainMenu.Visible;
                }
                _interaction.ProcessMenus();
            }
            catch (Exception e)
            {
                Game.LogTrivial("Oops there was an error here. Please send this log to https://discord.gg/xsdAXJb");
                Game.LogTrivial("SuperCallouts Error Report Start");
                Game.LogTrivial("======================================================");
                Game.LogTrivial(e.ToString());
                Game.LogTrivial("======================================================");
                Game.LogTrivial("SuperCallouts Error Report End");
                End();
            }
            base.Process();
        }

        public override void End()
        {
            if (_cop1.Exists()) _cop1.Dismiss();
            if (_cop2.Exists()) _cop2.Dismiss();
            if (_bad1.Exists()) _bad1.Dismiss();
            if (_bad2.Exists()) _bad2.Dismiss();
            if (_cVehicle.Exists()) _cVehicle.Dismiss();
            if (_copVehicle.Exists()) _copVehicle.Dismiss();
            if (_cBlip.Exists()) _cBlip.Delete();
            _mainMenu.Visible = false;
                        BigMessageThread bigMessage = new BigMessageThread();
            bigMessage.MessageInstance.ShowColoredShard("Code 4", "Callout Ended", HudColor.Green, HudColor.Black,
                2);
            Game.DisplayHelp("Scene ~g~CODE 4", 5000);
            base.End();
        }
        //UI Items
        private void Interactions(UIMenu sender, UIMenuItem selItem, int index)
        {
            if (selItem == _endCall)
            {
                Game.DisplaySubtitle("~y~Callout Ended.");
                End();
            }
        }
    }
}
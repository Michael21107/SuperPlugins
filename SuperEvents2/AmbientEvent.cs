#region
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LSPD_First_Response;
using LSPD_First_Response.Mod.API;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using SuperEvents2.SimpleFunctions;
#endregion

namespace SuperEvents2
{
    public class AmbientEvent
    {
        public static bool EventRunning { get; set; }
        public static bool TimeStart { get; set; }
        public static List<Entity> EntitiesToClear { get; private set; }
        public static List<Blip> BlipsToClear { get; private set; }
        public GameFiber ProcessFiber { get; }
        public Ped Player => Game.LocalPlayer.Character;
        private Vector3 _checkDistance;
        
        //Main Menu
        internal readonly MenuPool Interaction = new MenuPool();
        internal readonly UIMenu MainMenu = new UIMenu("SuperEvents", "Choose an option.");
        internal readonly UIMenu ConvoMenu = new UIMenu("SuperEvents", "~y~Choose a subject to speak with.");
        internal readonly UIMenuItem Questioning = new UIMenuItem("Speak With Subjects");
        internal readonly UIMenuItem EndCall = new UIMenuItem("~y~End Event", "Ends the event.");

        protected AmbientEvent()
        {
            try
            {
                EntitiesToClear = new List<Entity>();
                BlipsToClear = new List<Blip>();
                ProcessFiber = new GameFiber(delegate
                {
                    while (EventRunning)
                    {
                        Process();
                        GameFiber.Yield();
                    }
                });
            }
            catch (Exception e)
            {
                Game.LogTrivial("Oops there was an error here. Please send this log to https://discord.gg/xsdAXJb");
                Game.LogTrivial("SuperEvents Error Report Start");
                Game.LogTrivial("======================================================");
                Game.LogTrivial(e.ToString());
                Game.LogTrivial("======================================================");
                Game.LogTrivial("SuperEvents Error Report End");
                // ReSharper disable once VirtualMemberCallInConstructor
                End(true);
            }
        }
        
        public virtual void StartEvent(Vector3 spawnPoint, float spawnPointH)
        {
            AmbientEvent.TimeStart = false;
            Interaction.Add(MainMenu);
            Interaction.Add(ConvoMenu);
            MainMenu.AddItem(Questioning);
            MainMenu.AddItem(EndCall);
            MainMenu.BindMenuToItem(ConvoMenu, Questioning);
            ConvoMenu.ParentMenu = MainMenu;
            Questioning.Enabled = false;
            MainMenu.RefreshIndex();
            ConvoMenu.RefreshIndex();
            MainMenu.OnItemSelect += Interactions;
            ConvoMenu.OnItemSelect += Conversations;
            if (Settings.ShowBlips)
            {
                var eventBlip = new Blip(spawnPoint, 15f);
                eventBlip.Color = Color.Red;
                eventBlip.Alpha /= 2;
                eventBlip.Name = "Event";
                eventBlip.Flash(500, 5000);
                BlipsToClear.Add(eventBlip);
            }
            _checkDistance = spawnPoint;
            EventRunning = true;
            ProcessFiber.Start();
        }

        protected virtual void Process()
        {
            if (Game.IsKeyDown(Settings.EndEvent)) End(false);
            if (Game.IsKeyDown(Settings.Interact)) MainMenu.Visible = !MainMenu.Visible;
            if (_checkDistance.DistanceTo(Player) > 200f)
            {
                End(true);
                Game.LogTrivial("SuperEvents: Cleaning up event due to player being too far.");
            }
            Interaction.ProcessMenus();
        }

        protected virtual void End(bool forceCleanup)
        {
            EventRunning = false;
            
            if (forceCleanup)
            {
                foreach (var entity in EntitiesToClear.Where(entity => entity))
                    entity.Delete();
                Game.LogTrivial("SuperEvents: Event has been forcefully cleaned up.");
            }
            else
            {
                foreach (var entity in EntitiesToClear.Where(entity => entity))
                    entity.Dismiss(); 
                Game.DisplayHelp("~y~Event Ended.");
            }
            
            foreach (var blip in BlipsToClear.Where(blip => blip))
                blip.Delete();
            
            Interaction.CloseAllMenus();
            Game.LogTrivial("SuperEvents: Ending Event.");
            ProcessFiber.Abort();
            EventTimer.TimerStart();
        }

        protected virtual void Interactions(UIMenu sender, UIMenuItem selItem, int index)
        {
            if (selItem == EndCall)
            {
                End(false);
            }
        }

        protected virtual void Conversations(UIMenu sender, UIMenuItem selItem, int index)
        {
        }
    }
}
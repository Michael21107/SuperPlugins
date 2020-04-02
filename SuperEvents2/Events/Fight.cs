using Rage;
using SuperEvents2.SimpleFunctions;

namespace SuperEvents2.Events
{
    public class Fight : AmbientEvent
    {
        private Vector3 _spawnPoint;
        private float _spawnPointH;
        private Ped _suspect;
        
        public override void StartEvent(Vector3 s, float f)
        {
            EFunctions.FindSideOfRoad(120, 45, out _spawnPoint, out _spawnPointH);
            _suspect = new Ped(_spawnPoint) {IsPersistent = true, BlockPermanentEvents = true};
            EntitiesToClear.Add(_suspect);
            base.StartEvent(_spawnPoint, _spawnPointH);
        }

        public override void Process()
        {
            switch (_tasks)
            {
                case Tasks.CheckDistance:
                    if (Game.LocalPlayer.Character.DistanceTo(_spawnPoint) < 20f)
                    {
                        _tasks = Tasks.OnScene;
                    }
                    break;
                case Tasks.OnScene:
                    if (Settings.ShowHints)
                    {
                        Game.DisplayNotification("IT WORKS");
                        _tasks = Tasks.End;
                    }
                    break;
                case Tasks.End:
                    if (_suspect.IsDead)
                    {
                        End(false);
                    }
                    break;
                default:
                    End(true);
                    break;
            }
        }

        private Tasks _tasks = Tasks.CheckDistance;
        private enum Tasks
        {
            CheckDistance,
            OnScene,
            End
        }
    }
}
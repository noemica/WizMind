using WizMind.LuigiAi;

namespace WizMind.Scripts
{
    public class QuarantineContentsScript : IScript
    {
        private State state = null!;
        private ScriptWorkspace ws = null!;

        public Type SerializableStateType => typeof(State);

        public IScriptState SerializableState => this.state;

        public void Initialize(ScriptWorkspace ws, object? state)
        {
            this.state = state as State ?? new State();
            this.ws = ws;
        }

        public bool ProcessRun()
        {
            // Go to the map and reveal the contents
            this.ws.WizardCommands.GotoMap(MapType.MAP_QUA);
            this.ws.WizardCommands.RevealMap();

            // Spawn and drop a MC data core so we can identify all AAs
            this.ws.WizardCommands.GiveItem("MAIN.C Data Core");
            this.ws.Inventory.DropItem(1);
            this.ws.Movement.Wait();

            foreach (var (item, count) in this.ws.ItemAnalysis.CalculateItemCounts())
            {
                this.state.ItemFrequencies[item] =
                    this.state.ItemFrequencies.GetValueOrDefault(item) + count;
            }

            foreach (var (prop, count) in this.ws.PropAnalysis.CalculatePropCounts())
            {
                this.state.PropFrequencies[prop] =
                    this.state.PropFrequencies.GetValueOrDefault(prop) + count;
            }

            return true;
        }

        private class State : IScriptState
        {
            public Dictionary<string, int> ItemFrequencies = [];

            public Dictionary<string, int> PropFrequencies = [];

            public bool Initialized { get; set; }

            public int NumRuns { get; set; }

            public void Initialize()
            {
                if (this.Initialized)
                {
                    return;
                }

                this.Initialized = true;
            }
        }
    }
}

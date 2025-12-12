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

            this.state.Initialize();
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

            // Count items
            var itemCounts = this.ws.ItemAnalysis.CalculateItemCounts();
            this.state.ItemCounts.Add(itemCounts);

            foreach (var (item, count) in itemCounts)
            {
                this.state.AllItemCounts[item] =
                    this.state.AllItemCounts.GetValueOrDefault(item) + count;
            }

            // Count props
            var propCounts = this.ws.PropAnalysis.CalculatePropCounts();
            this.state.PropCounts.Add(propCounts);

            foreach (var (prop, count) in propCounts)
            {
                this.state.AllPropCounts[prop] =
                    this.state.AllPropCounts.GetValueOrDefault(prop) + count;
            }

            return true;
        }

        private class State : IScriptState
        {
            public Dictionary<string, int> AllPropCounts { get; set; } = [];

            public Dictionary<string, int> AllItemCounts { get; set; } = [];

            public List<Dictionary<string, int>> ItemCounts { get; set; } = [];

            public bool Initialized { get; set; }

            public int NumRuns { get; set; }

            public List<Dictionary<string, int>> PropCounts { get; set; } = [];

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

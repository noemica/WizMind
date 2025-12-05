using System.Diagnostics;
using WizMind.Definitions;
using WizMind.Interaction;
using WizMind.LuigiAi;

namespace WizMind.Instances
{
    [DebuggerDisplay("MapTile ({X},{Y}): {Name}")]
    public class MapTile
    {
        private readonly CogmindProcess cogmindProcess;
        private readonly GameDefinitions definitions;
        private Entity? entity;
        private Item? item;
        private string? name;
        private readonly LuigiAiData luigiAiData;
        private Prop? prop;
        private readonly LuigiTileStruct tile;
        private readonly int lastAction;
        private readonly int x;
        private readonly int y;

        public MapTile(
            CogmindProcess cogmindProcess,
            GameDefinitions definitions,
            LuigiAiData luigiAiData,
            LuigiTileStruct tile,
            int x,
            int y
        )
        {
            this.cogmindProcess = cogmindProcess;
            this.definitions = definitions;
            this.luigiAiData = luigiAiData;
            this.tile = tile;
            this.x = x;
            this.y = y;

            this.lastAction = this.luigiAiData.LastAction;
        }

        public Entity? Entity
        {
            get
            {
                this.CheckLastAction();
                if (this.entity == null && this.tile.entity != IntPtr.Zero)
                {
                    this.entity = new Entity(
                        this.cogmindProcess,
                        this.definitions,
                        this.luigiAiData,
                        this.cogmindProcess.FetchStruct<LuigiEntityStruct>(this.tile.entity)
                    );
                }

                return this.entity;
            }
        }

        public Item? Item
        {
            get
            {
                this.CheckLastAction();
                if (this.entity == null && this.tile.item != IntPtr.Zero)
                {
                    this.item = new Item(
                        this.luigiAiData,
                        this.definitions,
                        this.cogmindProcess.FetchStruct<LuigiItemStruct>(this.tile.item)
                    );
                }

                return this.item;
            }
        }

        public string Name
        {
            get
            {
                this.CheckLastAction();

                this.name ??= definitions.CellIdToName[tile.cell];

                return this.name;
            }
        }

        public Prop? Prop
        {
            get
            {
                this.CheckLastAction();
                if (this.entity == null && this.tile.prop != IntPtr.Zero)
                {
                    this.prop = new Prop(
                        this.luigiAiData,
                        this.definitions,
                        this.cogmindProcess.FetchStruct<LuigiPropStruct>(this.tile.prop)
                    );
                }

                return this.prop;
            }
        }

        public int X
        {
            get
            {
                this.CheckLastAction();
                return x;
            }
        }

        public int Y
        {
            get
            {
                this.CheckLastAction();
                return y;
            }
        }

        private void CheckLastAction()
        {
            this.luigiAiData.CheckLastAction(this.lastAction);
        }
    }
}

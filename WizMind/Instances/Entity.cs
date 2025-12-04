using System.Diagnostics;
using WizMind.Definitions;
using WizMind.Interaction;
using WizMind.LuigiAi;

namespace WizMind.Instances
{
    [DebuggerDisplay("Entity: {Name}")]
    public class Entity
    {
        private readonly CogmindProcess cogmindProcess;
        private readonly GameDefinitions definitions;
        private readonly LuigiAiData luigiAiData;
        private readonly LuigiEntityStruct entity;
        private List<Item>? inventory;
        private readonly int lastAction;

        public Entity(
            CogmindProcess cogmindProcess,
            GameDefinitions definitions,
            LuigiAiData luigiAiData,
            LuigiEntityStruct entity)
        {
            this.cogmindProcess = cogmindProcess;
            this.definitions = definitions;
            this.luigiAiData = luigiAiData;
            this.entity = entity;

            this.Name = this.definitions.EntityIdToDefinition[entity.ID];

            this.lastAction = this.luigiAiData.LastAction;
        }

        public int ActiveState
        {
            get
            {
                this.CheckLastAction();
                return this.entity.activeState;
            }
        }

        public int Energy
        {
            get
            {
                this.CheckLastAction();
                return this.entity.energy;
            }
        }

        public int Exposure
        {
            get
            {
                this.CheckLastAction();
                return this.entity.exposure;
            }
        }

        public int Heat
        {
            get
            {
                this.CheckLastAction();
                return this.entity.heat;
            }
        }

        public int Id
        {
            get
            {
                this.CheckLastAction();
                return this.entity.ID;
            }
        }

        public int Integrity
        {
            get
            {
                this.CheckLastAction();
                return this.entity.integrity;
            }
        }

        public List<Item> Inventory
        {
            get
            {
                this.CheckLastAction();

                this.inventory ??= this.cogmindProcess.FetchList<LuigiItemStruct>(
                    this.entity.inventoryPointer, this.entity.inventorySize).Select(
                        item => new Item(this.luigiAiData, this.definitions, item)).ToList();

                return this.inventory;
            }
        }

        public int Matter
        {
            get
            {
                this.CheckLastAction();
                return this.entity.matter;
            }
        }

        public EntityDefinition Name { get; }

        public int Relation
        {
            get
            {
                this.CheckLastAction();
                return this.entity.relation;
            }
        }

        public int Speed
        {
            get
            {
                this.CheckLastAction();
                return this.entity.speed;
            }
        }

        public int SystemCorruption
        {
            get
            {
                this.CheckLastAction();
                return this.entity.systemCorruption;
            }
        }

        private void CheckLastAction()
        {
            this.luigiAiData.CheckLastAction(this.lastAction);
        }
    }
}

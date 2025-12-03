using System.Diagnostics;
using WizMind.Definitions;
using WizMind.LuigiAi;

namespace WizMind.Instances
{
    [DebuggerDisplay("Item: {Name}")]
    public class Item
    {
        private readonly LuigiItemStruct item;
        private readonly LuigiAiData luigiAiData;
        private readonly int lastAction;

        public Item(LuigiAiData luigiAiData, GameDefinitions definitions, LuigiItemStruct item)
        {
            this.luigiAiData = luigiAiData;
            this.item = item;
            this.Name = definitions.ItemIdToName[item.ID];

            this.lastAction = this.luigiAiData.LastAction;
        }

        public bool Equipped
        {
            get
            {
                this.CheckLastAction();
                return this.item.equipped;
            }
        }

        public int Id
        {
            get
            {
                this.CheckLastAction();
                return this.item.ID;
            }
        }

        public int Integrity
        {
            get
            {
                this.CheckLastAction();
                return this.item.integrity;
            }
        }

        public string Name { get; }

        private void CheckLastAction()
        {
            this.luigiAiData.CheckLastAction(this.lastAction);
        }
    }
}

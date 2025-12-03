using System.Diagnostics;
using WizMind.Definitions;
using WizMind.LuigiAi;

namespace WizMind.Instances
{
    [DebuggerDisplay("Prop: {Name}")]
    public class Prop
    {
        private readonly LuigiAiData luigiAiData;
        private readonly LuigiPropStruct prop;

        private readonly int lastAction;

        public Prop(LuigiAiData luigiAiData, GameDefinitions definitions, LuigiPropStruct prop)
        {
            this.luigiAiData = luigiAiData;
            this.prop = prop;
            this.Name = definitions.PropIdToDefinition[prop.ID];

            this.lastAction = this.luigiAiData.LastAction;
        }

        public int Id
        {
            get
            {
                this.CheckLastAction();
                return this.prop.ID;
            }
        }

        public bool InteractivePiece
        {
            get
            {
                this.CheckLastAction();
                return this.prop.interactivePiece;
            }
        }

        public PropDefinition Name { get; }

        private void CheckLastAction()
        {
            this.luigiAiData.CheckLastAction(this.lastAction);
        }
    }
}

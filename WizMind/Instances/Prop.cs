using System.Diagnostics;
using WizMind.Definitions;
using WizMind.LuigiAi;

namespace WizMind.Instances
{
    [DebuggerDisplay("Prop: {Name}{InteractiveDebugDisplay,nq}")]
    public class Prop
    {
        private PropDefinition? definition;
        private readonly GameDefinitions definitions;
        private readonly LuigiAiData luigiAiData;
        private string? name;
        private readonly LuigiPropStruct prop;

        private readonly int lastAction;

        public Prop(LuigiAiData luigiAiData, GameDefinitions definitions, LuigiPropStruct prop)
        {
            this.luigiAiData = luigiAiData;
            this.definitions = definitions;
            this.prop = prop;

            this.lastAction = this.luigiAiData.LastAction;
        }

        public PropDefinition Definition
        {
            get
            {
                this.CheckLastAction();

                this.definition ??= this.definitions.PropIdToDefinition[this.prop.ID];
                return this.definition;
            }
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

        public string Name
        {
            get
            {
                this.CheckLastAction();

                this.name ??= this.Definition.Name;
                return this.name;
            }
        }

        public string Tag
        {
            get
            {
                this.CheckLastAction();
                return this.Definition.Tag;
            }
        }

        private string InteractiveDebugDisplay =>
            this.InteractivePiece ? " (interactive)" : " (non-interactive)";

        private void CheckLastAction()
        {
            this.luigiAiData.CheckLastAction(this.lastAction);
        }
    }
}

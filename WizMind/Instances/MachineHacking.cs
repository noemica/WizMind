using WizMind.LuigiAi;

namespace WizMind.Instances
{
    public class MachineHacking
    {
        private readonly LuigiMachineHackingStruct machineHacking;
        private readonly LuigiAiData luigiAiData;
        private readonly int lastAction;

        public MachineHacking(LuigiAiData luigiAiData, LuigiMachineHackingStruct machineHacking)
        {
            this.luigiAiData = luigiAiData;
            this.machineHacking = machineHacking;

            this.lastAction = this.luigiAiData.LastAction;
        }

        public int DetectChance
        {
            get
            {
                this.CheckLastAction();
                return this.machineHacking.detectChance;
            }
        }

        public int LastAction
        {
            get
            {
                this.CheckLastAction();
                return this.machineHacking.actionReady;
            }
        }

        public int TraceProgress
        {
            get
            {
                this.CheckLastAction();
                return this.machineHacking.traceProgress;
            }
        }

        public bool LastHackSuccess
        {
            get
            {
                this.CheckLastAction();
                return this.machineHacking.lastHackSuccess;
            }
        }

        private void CheckLastAction()
        {
            this.luigiAiData.CheckLastAction(this.lastAction);
        }
    }
}

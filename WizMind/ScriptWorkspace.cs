using WizMind.Analysis;
using WizMind.Definitions;
using WizMind.Interaction;
using WizMind.LuigiAi;

namespace WizMind
{
    public class ScriptWorkspace(
        CogmindProcess cogmindProcess,
        GameDefinitions definitions,
        GameState gameState,
        Input input,
        ItemAnalysis itemAnalysis,
        LuigiAiData luigiAiData,
        MachineHacking machineHacking,
        Movement movement,
        PropAnalysis propAnalysis,
        TileAnalysis tileAnalysis,
        WizardCommands wizardCommands
    )
    {
        public CogmindProcess CogmindProcess { get; } = cogmindProcess;

        public GameDefinitions Definitions { get; } = definitions;

        public GameState GameState { get; } = gameState;

        public Input Input { get; } = input;

        public ItemAnalysis ItemAnalysis { get; } = itemAnalysis;

        public LuigiAiData LuigiAiData { get; } = luigiAiData;

        public MachineHacking MachineHacking { get; } = machineHacking;

        public Movement Movement { get; } = movement;

        public PropAnalysis PropAnalysis { get; } = propAnalysis;

        public TileAnalysis TileAnalysis { get; } = tileAnalysis;

        public WizardCommands WizardCommands { get; } = wizardCommands;
    }
}

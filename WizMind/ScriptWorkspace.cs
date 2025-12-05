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
        LuigiAiData luigiAiData,
        PropAnalysis propAnalysis,
        WizardCommands wizardCommands
    )
    {
        public CogmindProcess CogmindProcess { get; } = cogmindProcess;

        public GameDefinitions Definitions { get; } = definitions;

        public GameState GameState { get; } = gameState;

        public Input Input { get; } = input;

        public LuigiAiData LuigiAiData { get; } = luigiAiData;

        public PropAnalysis PropAnalysis { get; } = propAnalysis;

        public WizardCommands WizardCommands { get; } = wizardCommands;
    }
}

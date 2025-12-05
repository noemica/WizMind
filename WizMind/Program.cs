using System.Diagnostics;
using WizMind;
using WizMind.Analysis;
using WizMind.Definitions;
using WizMind.Interaction;
using WizMind.LuigiAi;
using WizMind.Scripts;

internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            // Get process
            Process[] processes = Process.GetProcessesByName("Cogmind");

            if (processes.Length == 0)
            {
                Console.WriteLine("Cogmind not open");
                return;
            }

            CogmindProcess? cogmindProcess = null;
            foreach (var process in processes)
            {
                cogmindProcess = CogmindProcess.TryCreate(process);
                if (cogmindProcess != null)
                {
                    break;
                }
            }

            if (cogmindProcess == null)
            {
                Console.WriteLine("Cogmind open but not run with -luigiai");
                return;
            }

            var definitions = new GameDefinitions(
                Directory.GetParent(cogmindProcess.Process.MainModule!.FileName)!.FullName
            );
            var input = new Input(cogmindProcess);
            var gameState = new GameState(input);
            var luigiAiData = new LuigiAiData(cogmindProcess, definitions);
            var propAnalysis = new PropAnalysis(luigiAiData);
            var wizardCommands = new WizardCommands(
                cogmindProcess,
                definitions,
                input,
                luigiAiData
            );

            var ws = new ScriptWorkspace(
                cogmindProcess,
                definitions,
                gameState,
                input,
                luigiAiData,
                propAnalysis,
                wizardCommands
            );

            var script = new GarrisonStatsScript();
            script.Initialize(ws);
            script.Run();

            Console.WriteLine("Done running");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unhandled exception");
            Console.WriteLine(ex.Message);

            Console.ReadLine();
        }
    }
}

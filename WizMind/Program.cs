using System.Diagnostics;
using System.Text.Json;
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

            // Create interaction objects and script workspace
            var definitions = new GameDefinitions(
                Directory.GetParent(cogmindProcess.Process.MainModule!.FileName)!.FullName
            );
            var input = new Input(cogmindProcess);
            var luigiAiData = new LuigiAiData(cogmindProcess, definitions);
            var gameState = new GameState(input, luigiAiData);
            var machineHacking = new MachineHacking(input, luigiAiData);
            var movement = new Movement(input, luigiAiData);

            var wizardCommands = new WizardCommands(
                cogmindProcess,
                definitions,
                input,
                luigiAiData
            );

            var itemAnalysis = new ItemAnalysis(luigiAiData);
            var propAnalysis = new PropAnalysis(luigiAiData);
            var tileAnalysis = new TileAnalysis(luigiAiData);

            var ws = new ScriptWorkspace(
                cogmindProcess,
                definitions,
                gameState,
                input,
                itemAnalysis,
                luigiAiData,
                machineHacking,
                movement,
                propAnalysis,
                tileAnalysis,
                wizardCommands
            );

            // In case there are any UI elements open, close them now
            input.SendKeystroke(Keys.Escape);
            input.SendKeystroke(Keys.Escape);
            Thread.Sleep(TimeDuration.UnknownEscapeSleep);

            // Run script (hardcoded for now)
            //var script = new GarrisonContentsScript();
            var script = new ECACountScript();

            var stateFile = $"{script.GetType().Name.Replace("Script", "State")}.json";

            object? state = null;

            // Try to deserialize the state file if it exists
            if (File.Exists(stateFile))
            {
                try
                {
                    using var stream = File.OpenRead(stateFile);
                    state = JsonSerializer.Deserialize(stream, script.SerializableStateType);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            script.Initialize(ws, state);

            var runNum = 1;

            while (true)
            {
                try
                {
                    // Try to process a run of the script
                    if (!script.ProcessRun(runNum))
                    {
                        break;
                    }

                    // Serialize the scruot state
                    using (var stream = File.OpenWrite(stateFile))
                    {
                        JsonSerializer.Serialize(stream, script.SerializableState);
                    }

                    // Self destruct before continuing again
                    gameState.SelfDestruct();
                    runNum += 1;
                }
                catch (Exception ex)
                {
                    // Encountered unknown state, just try to restart
                    Console.WriteLine(ex.ToString());

                    // In case there are any UI elements open, close them now
                    input.SendKeystroke(Keys.Escape);
                    input.SendKeystroke(Keys.Escape);
                    Thread.Sleep(TimeDuration.UnknownEscapeSleep);

                    gameState.SelfDestruct();
                    if (luigiAiData.MapType != MapType.MAP_YRD)
                    {
                        Console.WriteLine("Self destructing the run failed");
                        break;
                    }
                }
            }

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

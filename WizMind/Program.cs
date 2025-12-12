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
            var inventory = new Inventory(input, luigiAiData);
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
                inventory,
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
            //var script = new ECACountScript();
            //var script = new GarrisonContentsScript();
            var script = new QuarantineContentsScript();

            var stateFile = $"{script.GetType().Name.Replace("Script", "State")}.json";

            object? state = null;

            // Try to deserialize the state file if it exists
            state = DeserializeState(script, stateFile);

            script.Initialize(ws, state);

            while (true)
            {
                try
                {
                    script.SerializableState.NumRuns += 1;

                    // Try to process a run of the script
                    if (!script.ProcessRun())
                    {
                        break;
                    }

                    // Serialize the script state
                    using (var stream = File.OpenWrite(stateFile))
                    {
                        JsonSerializer.Serialize(
                            stream,
                            script.SerializableState,
                            script.SerializableStateType
                        );
                    }

                    // Self destruct before continuing again
                    gameState.SelfDestruct();
                }
                catch (Exception ex)
                {
                    // Encountered unknown state, try to self destruct and
                    // start a new clearn run
                    Console.WriteLine("Error during script run");
                    Console.WriteLine(ex.ToString());

                    // In case there are any UI elements open, close them now
                    input.SendKeystroke(Keys.Escape);
                    input.SendKeystroke(Keys.Escape);
                    Thread.Sleep(TimeDuration.UnknownEscapeSleep);

                    gameState.SelfDestruct();

                    // Try to reload the state from disk
                    state = DeserializeState(script, stateFile);
                    script.Initialize(ws, state);
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

    private static object? DeserializeState(QuarantineContentsScript script, string stateFile)
    {
        if (File.Exists(stateFile))
        {
            try
            {
                using var stream = File.OpenRead(stateFile);
                return JsonSerializer.Deserialize(stream, script.SerializableStateType);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deserializing state file:");
                Console.WriteLine(ex.Message);
            }
        }

        return null;
    }
}

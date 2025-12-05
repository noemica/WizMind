using System.Diagnostics;
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

            //var data = cogmindProcess.LuigiAiData;
            //var cogmind = data.Cogmind;
            //var inventory = cogmind.Inventory;
            //var tiles = data.AllTiles;
            //var machineHacking = data.MachineHacking;
            //var input = cogmindProcess.Input;
            //var wizardCommands = cogmindProcess.WizardCommands;

            //wizardCommands.GiveItem("Assault Rifle");
            //wizardCommands.GotoMap(MapType.MAP_MAT, 10);
            //wizardCommands.RevealMap();
            //wizardCommands.GotoMap(MapType.MAP_MAT, 9);
            //wizardCommands.RevealMap();
            //wizardCommands.GotoMap(MapType.MAP_MAT, 8);
            //wizardCommands.RevealMap();
            //wizardCommands.GotoMap(MapType.MAP_FAC, 7);
            //wizardCommands.RevealMap();
            //wizardCommands.GotoMap(MapType.MAP_FAC, 6);
            //wizardCommands.RevealMap();
            //wizardCommands.GotoMap(MapType.MAP_FAC, 5);
            //wizardCommands.RevealMap();
            //wizardCommands.GotoMap(MapType.MAP_FAC, 4);
            //wizardCommands.RevealMap();
            //wizardCommands.GotoMap(MapType.MAP_RES, 3);
            //wizardCommands.RevealMap();
            //wizardCommands.GotoMap(MapType.MAP_RES, 2);
            //wizardCommands.RevealMap();
            //wizardCommands.GotoMap(MapType.MAP_ACC);
            //wizardCommands.RevealMap();

            var script = new GarrisonStatsScript();
            script.Initialize(cogmindProcess);
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

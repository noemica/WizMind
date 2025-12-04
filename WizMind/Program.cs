using System.Diagnostics;
using WizMind.Interaction;

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

            var data = cogmindProcess.LuigiAiData;
            var cogmind = data.Cogmind;
            var inventory = cogmind.Inventory;
            var tiles = data.AllTiles;
            var machineHacking = data.MachineHacking;

            var input = new Input(cogmindProcess);

            input.SendKeystroke(Keys.D, KeyModifier.AltShift);
            Thread.Sleep(500);
            input.SendKeystroke(Keys.Escape);
            Thread.Sleep(500);

            input.SendKeystroke(Keys.D1);
            Thread.Sleep(500);

            input.SendKeystroke(Keys.D1, KeyModifier.Shift);
            Thread.Sleep(500);
            input.SendKeystroke(Keys.Escape);
            Thread.Sleep(500);

            input.SendKeystroke(Keys.D1, KeyModifier.Alt);
            Thread.Sleep(500);

            input.SendKeystroke(Keys.G);
            Thread.Sleep(500);

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
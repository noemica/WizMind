using WizMind.Definitions;
using WizMind.LuigiAi;

namespace WizMind.Interaction
{
    public class Commands
    {
        private readonly CogmindProcess cogmindProcess;
        private bool inWizardMode;

        public Commands(CogmindProcess cogmindProcess)
        {
            this.cogmindProcess = cogmindProcess;
        }

        private GameDefinitions Definitions => this.LuigiAiData.Definitions;

        private Input Input => this.cogmindProcess.Input;

        private LuigiAiData LuigiAiData => this.cogmindProcess.LuigiAiData;

        /// <summary>
        /// Enters a command in the wizard mode console.
        /// </summary>
        /// <param name="command">The command to enter.</param>
        public void EnterWizardModeCommand(string command)
        {
            this.EnsureWizardMode();

            // Open the wizard mode console
            this.Input.SendKeystroke(Keys.D, KeyModifier.AltShift, true);

            Thread.Sleep(SleepDuration.WizardConsole);

            this.Input.SendString(command);
            this.Input.SendKeystroke(Keys.Enter, waitForResponse: true);
        }

        /// <summary>
        /// Starts wizard mode if it is not already enabled.
        /// </summary>
        public void EnsureWizardMode()
        {
            if (this.inWizardMode)
            {
                return;
            }

            // Verify wizard mode key exists, if it doesn't then things won't work right
            var wizardKeyPath = Path.Combine(
                Directory.GetParent(this.cogmindProcess.Process.MainModule!.FileName)!.FullName,
                "wizard_access_private_key_do_not_share.txt");
            if (!Path.Exists(wizardKeyPath))
            {
                throw new Exception("Wizard mode key is missing");
            }

            // Send the wizard mode command twice to start it
            this.Input.SendKeystroke(Keys.D, KeyModifier.AltShift, true);
            this.Input.SendKeystroke(Keys.D, KeyModifier.AltShift, true);

            // If we are starting from a state with wizard mode active, we will
            // be in the console now. To exit the console without opening the
            // escape menu, attempt to open the console a second time. After
            // we are sure the console is open in both cases, then we are safe
            // to close it.
            this.Input.SendKeystroke(Keys.D, KeyModifier.AltShift, true);

            this.Input.SendKeystroke(Keys.Escape);

            this.inWizardMode = true;
        }

        /// <summary>
        /// Spawns an item in Cogmind's inventory.
        /// </summary>
        /// <param name="itemName">The name of the item to create.</param>
        public void GiveItem(string itemName)
        {
            if (!this.Definitions.ItemNameToId.ContainsKey(itemName))
            {
                throw new ArgumentException($"Item ${itemName} does not exist");
            }

            // Ideally invalidate data here but items aren't updated after wizard mode commands
            this.EnterWizardModeCommand($"g {itemName}");
        }

        private void InvalidateData()
        {
            this.LuigiAiData.InvalidateData();
        }
    }
}

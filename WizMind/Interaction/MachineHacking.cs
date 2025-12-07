using System.Diagnostics;
using WizMind.LuigiAi;

namespace WizMind.Interaction
{
    public class MachineHacking(Input input, LuigiAiData luigiAiData)
    {
        private readonly Input input = input;
        private readonly LuigiAiData luigiAiData = luigiAiData;

        /// <summary>
        /// Closes and waits for the hacking popup to fully close.
        /// </summary>
        public void CloseHackingPopup()
        {
            if (
                this.luigiAiData.MachineHackingState is null
                || this.luigiAiData.MachineHackingState.LastAction == 0
            )
            {
                throw new InvalidOperationException(
                    "Can't close the popup before the machine is ready"
                );
            }

            // Close the popup
            this.input.SendKeystroke(Keys.Escape);
            this.luigiAiData.InvalidateData(DataInvalidationType.NonadvancingAction);

            // Would be nice to refresh until the hacking state is null but it
            // gets cleared before the popup animation is finished. We have to
            // wait a fixed amount of time here instead.
            Thread.Sleep(TimeDuration.PostHackPopupLoadSleep);

            if (this.luigiAiData.MachineHackingState != null)
            {
                throw new Exception("Failed to close the hacking popup");
            }
        }

        /// <summary>
        /// Opens a machine hacking popup in the given direction and waits for
        /// the popup to fully open.
        /// </summary>
        /// <param name="direction">The direction the machine is in.</param>
        public void OpenHackingPopup(MovementDirection direction)
        {
            // TODO: Currently assumes you are standing next to the machine and
            // that the movement direction will open the popup. Would be nicer
            // to check this assumption or to just pass in the tile/prop and
            // figure it out ourselves.

            // First open the popup
            var key = Movement.DirectionToKey(direction);
            this.input.SendKeystroke(key);

            this.luigiAiData.InvalidateData(DataInvalidationType.NonadvancingAction);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (this.luigiAiData.MachineHackingState?.LastAction != 1)
            {
                if (stopwatch.ElapsedMilliseconds > TimeDuration.HackingPopupLoadTimeout)
                {
                    throw new Exception("Failed to open machine hacking popup");
                }

                Thread.Sleep(TimeDuration.HackPopupLoadSleep);
                this.luigiAiData.InvalidateData(DataInvalidationType.NonadvancingAction);
            }

            // The last action lies when the interface is really loaded so we
            // need an additional sleep here
            Thread.Sleep(TimeDuration.PostHackPopupLoadSleep);
        }

        /// <summary>
        /// Performs a fixed letter hack on a machine.
        /// </summary>
        /// <param name="key">The key associated with the direct hack.</param>
        public void PerformHack(Keys key)
        {
            if (
                this.luigiAiData.MachineHackingState is null
                || this.luigiAiData.MachineHackingState.LastAction == 0
            )
            {
                throw new InvalidOperationException("Can't hack before the machine is ready");
            }

            var lastAction = this.luigiAiData.MachineHackingState.LastAction;

            this.input.SendKeystroke(key);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (this.luigiAiData.MachineHackingState.LastAction == lastAction)
            {
                if (stopwatch.ElapsedMilliseconds > TimeDuration.HackingPopupLoadTimeout)
                {
                    throw new Exception("Failed to complete hack");
                }

                Thread.Sleep(TimeDuration.HackDataRefreshSleep);
                this.luigiAiData.InvalidateData(DataInvalidationType.NonadvancingAction);
            }
        }

        /// <summary>
        /// Performs a manual hack on a machine.
        /// </summary>
        /// <param name="hack">The string to enter into the hacking menu.</param
        public void PerformManualHack(string hack)
        {
            throw new NotImplementedException();
        }
    }
}

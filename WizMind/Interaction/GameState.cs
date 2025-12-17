using System.Diagnostics;
using WizMind.LuigiAi;

namespace WizMind.Interaction
{
    public class GameState(Input input, LuigiAiData luigiAiData)
    {
        private readonly Input input = input;
        private readonly LuigiAiData luigiAiData = luigiAiData;

        /// <summary>
        /// Loads the game.
        /// </summary>
        /// <param name="sameTurn">Whether we are loading a save on the same turn/map/depth.</param>
        public void LoadGame(bool sameTurn = false)
        {
            var depth = this.luigiAiData.Depth;
            var map = this.luigiAiData.MapType;
            var lastAction = this.luigiAiData.LastAction;

            // Send the load keypress
            this.input.SendKeystroke(Keys.F9, KeyModifier.Ctrl);
            this.luigiAiData.InvalidateData(DataInvalidationType.NonadvancingAction);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Wait for the load to complete
            while (
                !sameTurn
                && stopwatch.ElapsedMilliseconds < TimeDuration.MapLoadTime
                && depth == this.luigiAiData.Depth
                && map == this.luigiAiData.MapType
                && lastAction == this.luigiAiData.LastAction
            )
            {
                Thread.Sleep(TimeDuration.MapLoadSleep);
                this.luigiAiData.InvalidateData(DataInvalidationType.NonadvancingAction);
            }

            if (stopwatch.ElapsedMilliseconds >= TimeDuration.MapLoadTime)
            {
                throw new Exception("Failed to load save");
            }

            if (sameTurn)
            {
                // If we are loading the same turn, just double the timeout
                // because we can't tell when anything has changed
                Thread.Sleep(TimeDuration.PostMapLoadSleep);
                this.luigiAiData.InvalidateData(DataInvalidationType.NonadvancingAction);
            }

            // Wait for the map animation to finish loading
            Thread.Sleep(TimeDuration.PostMapLoadSleep);
        }

        /// <summary>
        /// Saves the game.
        /// </summary>
        public void SaveGame()
        {
            this.input.SendKeystroke(Keys.F8, KeyModifier.Ctrl);
            Thread.Sleep(TimeDuration.SaveGameSleep);
        }

        /// <summary>
        /// Self destructs and starts a new run.
        /// </summary>
        public void SelfDestruct()
        {
            // Send the self-destruct shortcut
            Thread.Sleep(TimeDuration.PreSelfDestructSleep);
            this.input.SendKeystroke(Keys.F10, KeyModifier.Alt);

            // On game over screen, space to restart with new run
            Thread.Sleep(TimeDuration.GameOverSleep);
            this.input.SendKeystroke(Keys.Space);
            Thread.Sleep(TimeDuration.NewGameSleep);

            // We should be in scrapyard at this point
            this.luigiAiData.InvalidateData(DataInvalidationType.NonadvancingAction);
            if (luigiAiData.MapType != MapType.MAP_YRD)
            {
                throw new Exception("Self destructing the run failed");
            }
        }
    }
}

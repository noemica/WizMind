using WizMind.LuigiAi;

namespace WizMind.Interaction
{
    public class GameState(Input input, LuigiAiData luigiAiData)
    {
        private readonly Input input = input;
        private readonly LuigiAiData luigiAiData = luigiAiData;

        public void SelfDestruct()
        {
            // Close any popups that might be active
            this.input.SendKeystroke(Keys.Escape);
            this.input.SendKeystroke(Keys.Escape);
            Thread.Sleep(TimeDuration.UnknownEscapeSleep);

            // Extra wait to make sure the ? is registered
            // Without this, the input was being ignored in some cases
            Thread.Sleep(TimeDuration.PreSelfDestructSleep);

            // Open the escape menu with ? since escape is disabled in settings
            this.input.SendKeystroke(Keys.OemQuestion, KeyModifier.Shift);
            Thread.Sleep(TimeDuration.EscapeMenuSleep);

            // Switch to save menu if not active
            this.input.SendKeystroke(Keys.D1);
            Thread.Sleep(TimeDuration.EscapeMenuSleep);

            // Self destruct buttons, long delay is required before second press is registered
            this.input.SendKeystroke(Keys.B);
            Thread.Sleep(TimeDuration.SelfDestructSleep);
            this.input.SendKeystroke(Keys.B);
            Thread.Sleep(TimeDuration.GameOverSleep);

            // On game over screen, space to restart with new run
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

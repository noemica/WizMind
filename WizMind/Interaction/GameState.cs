namespace WizMind.Interaction
{
    public class GameState(Input input)
    {
        private readonly Input input = input;

        public void SelfDestruct()
        {
            var input = this.input;

            // Extra wait to make sure the ? is registered
            // Without this, the input was being ignored in some cases
            Thread.Sleep(TimeDuration.PreSelfDestructSleep);

            // Open the escape menu with ? since escape is disabled in settings
            input.SendKeystroke(Keys.OemQuestion, KeyModifier.Shift);
            Thread.Sleep(TimeDuration.EscapeMenuSleep);

            // Switch to save menu if not active
            input.SendKeystroke(Keys.D1);
            Thread.Sleep(TimeDuration.EscapeMenuSleep);

            // Self destruct buttons, long delay is required before second press is registered
            input.SendKeystroke(Keys.B);
            Thread.Sleep(TimeDuration.SelfDestructSleep);
            input.SendKeystroke(Keys.B);
            Thread.Sleep(TimeDuration.GameOverSleep);

            // On game over screen, space to restart with new run
            input.SendKeystroke(Keys.Space);
            Thread.Sleep(TimeDuration.NewGameSleep);
        }
    }
}

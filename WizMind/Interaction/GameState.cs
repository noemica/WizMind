namespace WizMind.Interaction
{
    public class GameState
    {
        private readonly CogmindProcess cogmindProcess;

        public GameState(CogmindProcess cogmindProcess)
        {
            this.cogmindProcess = cogmindProcess;
        }

        public void SelfDestruct()
        {
            var input = this.cogmindProcess.Input;

            input.SendKeystroke(Keys.OemQuestion, KeyModifier.Shift);
            Thread.Sleep(TimeDuration.EscapeMenuSleep);

            input.SendKeystroke(Keys.D1);
            Thread.Sleep(TimeDuration.EscapeMenuSleep);

            input.SendKeystroke(Keys.B);
            Thread.Sleep(TimeDuration.SelfDestructSleep);

            input.SendKeystroke(Keys.B);
            Thread.Sleep(TimeDuration.GameOverSleep);

            input.SendKeystroke(Keys.Space);
            Thread.Sleep(TimeDuration.NewGameSleep);
        }
    }
}

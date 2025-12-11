namespace WizMind.Scripts
{
    public class RifInstallsScript : IScript
    {
        private ScriptWorkspace ws = null!;

        public IScriptState SerializableState => throw new NotImplementedException();

        public Type SerializableStateType => throw new NotImplementedException();

        public void Initialize(ScriptWorkspace ws, object? state)
        {
            this.ws = ws;
        }

        public bool ProcessRun()
        {
            throw new NotImplementedException();
        }
    }
}

namespace WizMind.Scripts
{
    public interface IScript
    {
        Type SerializableStateType { get; }

        object SerializableState { get; }

        void Initialize(ScriptWorkspace ws, object? state);

        bool ProcessRun(int runNum);
    }
}

namespace WizMind.Scripts
{
    public interface IScript
    {
        Type SerializableStateType { get; }

        IScriptState SerializableState { get; }

        void Initialize(ScriptWorkspace ws, object? state);

        bool ProcessRun();
    }
}

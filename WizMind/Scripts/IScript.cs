using WizMind.Interaction;

namespace WizMind.Scripts
{
    public interface IScript
    {
        void Initialize(ScriptWorkspace ws);

        void Run();
    }
}

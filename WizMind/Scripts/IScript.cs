using WizMind.Interaction;

namespace WizMind.Scripts
{
    public interface IScript
    {
        void Initialize(CogmindProcess cogmindProcess);

        void Run();
    }
}

using System.Diagnostics;

namespace WizMind.Definitions
{
    [DebuggerDisplay("PropDefinition: {Name}")]
    public class PropDefinition
    {
        public PropDefinition(int id, string tag, string name)
        {
            this.Id = id;
            this.Tag = tag;
            this.Name = name;
        }

        public int Id { get; }

        public string Name { get; }

        public string Tag { get; }
    }
}

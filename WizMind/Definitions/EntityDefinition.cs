using System.Diagnostics;

namespace WizMind.Definitions
{
    [DebuggerDisplay("EntityDefinition: {Name}")]
    public class EntityDefinition
    {
        public EntityDefinition(int id, string tag, string name)
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

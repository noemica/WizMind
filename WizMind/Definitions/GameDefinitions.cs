
using System.Text.RegularExpressions;

namespace WizMind.Definitions
{
    public class GameDefinitions
    {
        public GameDefinitions(string cogmindDirectory)
        {
            var luigiAiDirectory = Path.Combine(cogmindDirectory, "luigiAi");

            var cellIdPath = Path.Combine(luigiAiDirectory, "cellID.txt");
            this.ParseCellIds(cellIdPath);

            var entityIdPath = Path.Combine(luigiAiDirectory, "entityID.txt");
            this.ParseEntityIds(entityIdPath);

            var itemIdPath = Path.Combine(luigiAiDirectory, "itemID.txt");
            this.ParseItemIds(itemIdPath);

            var propIdPath = Path.Combine(luigiAiDirectory, "propID.txt");
            this.ParsePropIds(propIdPath);
        }

        public Dictionary<int, string> CellIdToName { get; } = new Dictionary<int, string>();

        public Dictionary<string, int> CellNameToId { get; } = new Dictionary<string, int>();

        public Dictionary<int, EntityDefinition> EntityIdToDefinition { get; } = new Dictionary<int, EntityDefinition>();

        public Dictionary<string, EntityDefinition> EntityNameDefinition { get; } = new Dictionary<string, EntityDefinition>();

        public Dictionary<int, string> ItemIdToName { get; } = new Dictionary<int, string>();

        public Dictionary<string, int> ItemNameToId { get; } = new Dictionary<string, int>();

        public Dictionary<int, PropDefinition> PropIdToDefinition { get; } = new Dictionary<int, PropDefinition>();

        public Dictionary<string, PropDefinition> PropNameToDefinition { get; } = new Dictionary<string, PropDefinition>();

        private void ParseCellIds(string cellIdPath)
        {
            var lines = File.ReadAllLines(cellIdPath);
            var regex = new Regex(@"^ *(\d+) (.*)$");

            foreach (var line in lines)
            {
                var match = regex.Match(line);

                var id = int.Parse(match.Groups[1].ValueSpan);
                var cellName = match.Groups[2].Value;

                this.CellIdToName[id] = cellName;
                this.CellNameToId[cellName] = id;
            }

            // Also add undefined unknown cell type
            this.CellIdToName[-1] = "UNKNOWN";
            this.CellNameToId["UNKNOWN"] = -1;
        }

        private void ParseEntityIds(string entityIdPath)
        {
            var lines = File.ReadAllLines(entityIdPath);
            var regex = new Regex(@"^ *(\d+) +(.+)  (.+)$");

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                var match = regex.Match(line);

                var entity = new EntityDefinition(
                    int.Parse(match.Groups[1].Value), match.Groups[2].Value, match.Groups[3].Value);

                this.EntityIdToDefinition[entity.Id] = entity;
                this.EntityNameDefinition[entity.Name] = entity;
            }
        }

        private void ParseItemIds(string itemIdPath)
        {
            var lines = File.ReadAllLines(itemIdPath);
            var regex = new Regex(@"^ *(\d+) (.*)$");

            foreach (var line in lines)
            {
                var match = regex.Match(line);

                var id = int.Parse(match.Groups[1].ValueSpan);
                var itemName = match.Groups[2].Value;

                this.ItemIdToName[id] = itemName;
                this.ItemNameToId[itemName] = id;
            }
        }

        private void ParsePropIds(string propIdPath)
        {
            var lines = File.ReadAllLines(propIdPath);
            var regex = new Regex(@"^ *(\d+) +(.+)  (.+)$");

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                var match = regex.Match(line);

                var prop = new PropDefinition(
                    int.Parse(match.Groups[1].Value), match.Groups[2].Value, match.Groups[3].Value);

                this.PropIdToDefinition[prop.Id] = prop;
                this.PropNameToDefinition[prop.Name] = prop;
            }
        }
    }
}

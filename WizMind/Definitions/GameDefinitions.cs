using System.Text.RegularExpressions;
using WizMind.LuigiAi;

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

            foreach (var map in MapDefinition.Maps)
            {
                this.MapNameToDefinition[map.name] = map;
                this.MapTypeToDefinition[map.type] = map;

                if (map.mainMap)
                {
                    for (var depth = map.firstDepth; depth >= map.lastDepth; depth--)
                    {
                        this.MainMaps[depth] = map;
                    }
                }
            }
        }

        public Dictionary<int, string> CellIdToName { get; } = [];

        public Dictionary<string, int> CellNameToId { get; } = [];

        public Dictionary<int, EntityDefinition> EntityIdToDefinition { get; } = [];

        public Dictionary<string, EntityDefinition> EntityNameDefinition { get; } = [];

        public Dictionary<int, string> ItemIdToName { get; } = [];

        public Dictionary<string, int> ItemNameToId { get; } = [];

        public Dictionary<int, MapDefinition> MainMaps { get; } = [];

        public Dictionary<MapType, MapDefinition> MapTypeToDefinition { get; } = [];

        public Dictionary<string, MapDefinition> MapNameToDefinition { get; } = [];

        public Dictionary<int, PropDefinition> PropIdToDefinition { get; } = [];

        public Dictionary<string, PropDefinition> PropNameToDefinition { get; } = [];

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

            // Also add undefined no cell type
            this.CellIdToName[-1] = "NO_CELL";
            this.CellNameToId["NO_CELL"] = -1;
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
                    int.Parse(match.Groups[1].Value),
                    match.Groups[2].Value,
                    match.Groups[3].Value
                );

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
                    int.Parse(match.Groups[1].Value),
                    match.Groups[2].Value,
                    match.Groups[3].Value
                );

                this.PropIdToDefinition[prop.Id] = prop;
                this.PropNameToDefinition[prop.Name] = prop;
            }
        }
    }
}

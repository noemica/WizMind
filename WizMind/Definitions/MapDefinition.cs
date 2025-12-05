using System.Diagnostics;
using WizMind.LuigiAi;

namespace WizMind.Definitions
{
    [DebuggerDisplay("MapDefinition: {name}")]
    public class MapDefinition
    {
        public const int NoMapDepth = 0;

        public static readonly List<MapDefinition> Maps =
        [
            // new MapDefinition("Scrapyard", MapType.MAP_YRD), Teleporting to scrapyard crashes the game
            new MapDefinition("Materials", MapType.MAP_MAT, 10, 8, true),
            new MapDefinition("Factory", MapType.MAP_FAC, 7, 4, true),
            new MapDefinition("Research", MapType.MAP_RES, 3, 2, true),
            new MapDefinition("Access", MapType.MAP_ACC, 1, 1, true),
            // new MapDefinition("Surface"), MapType.MAP_SUR, Teleporting to surface crashes the game
            new MapDefinition("Mines", MapType.MAP_MIN, 10, 9),
            new MapDefinition("Exiles", MapType.MAP_EXI),
            new MapDefinition("Storage", MapType.MAP_STO),
            new MapDefinition("Recycling", MapType.MAP_REC),
            new MapDefinition("Scraptown", MapType.MAP_SCR),
            new MapDefinition("Wastes", MapType.MAP_WAS, 7, 4, mainMapRequired: true),
            // This is missing the -9 Storage Garrison but it's only sometimes available
            new MapDefinition("Garrison", MapType.MAP_GAR, 8, 1, mainMapRequired: true),
            new MapDefinition("DSF", MapType.MAP_DSF, 7, 2),
            new MapDefinition("Subcaves", MapType.MAP_SUB, 7, 2),
            new MapDefinition("Lower Caves", MapType.MAP_LOW, 7, 6),
            new MapDefinition("Upper Caves", MapType.MAP_UPP, 5, 4),
            new MapDefinition("Proximity Caves", MapType.MAP_PRO, 6, 3),
            new MapDefinition("Deep Caves", MapType.MAP_DEE),
            new MapDefinition("Zion", MapType.MAP_ZIO),
            new MapDefinition("Data Miner", MapType.MAP_DAT),
            new MapDefinition("Zhirov", MapType.MAP_ZHI),
            new MapDefinition("Warlord", MapType.MAP_WAR),
            new MapDefinition("Extension", MapType.MAP_EXT),
            new MapDefinition("Cetus", MapType.MAP_CET),
            new MapDefinition("Archives", MapType.MAP_ARC),
            new MapDefinition("Hub_04(d)", MapType.MAP_HUB),
            new MapDefinition("Armory", MapType.MAP_ARM),
            new MapDefinition("Lab", MapType.MAP_LAB),
            new MapDefinition("Quarantine", MapType.MAP_QUA),
            new MapDefinition("Testing", MapType.MAP_TES),
            new MapDefinition("Section 7", MapType.MAP_SEC),
            new MapDefinition("Protoforge", "Frg", MapType.MAP_FRG),
            new MapDefinition("Command", MapType.MAP_COM),
            // new MapDefinition("Lair", MapType.MAP_LAI), Only available in abominations mode
            // new MapDefinition("Wartown", "Tow", MapType.MAP_TOW), Only available in battle royale mode (defunct)
        ];

        public readonly int firstDepth;
        public readonly int lastDepth;
        public readonly MapType type;
        public readonly bool mainMap;
        public readonly bool mainMapRequired;
        public readonly string name;
        public readonly string tag;

        public MapDefinition(
            string name,
            MapType type,
            int firstDepth = NoMapDepth,
            int lastDepth = NoMapDepth,
            bool mainMap = false,
            bool mainMapRequired = false,
            string? tag = null
        )
        {
            this.name = name;
            this.type = type;
            this.firstDepth = firstDepth;
            this.lastDepth = lastDepth;
            this.mainMap = mainMap;
            this.mainMapRequired = mainMapRequired;
            this.tag = tag ?? name[..3];
        }

        public MapDefinition(
            string name,
            string tag,
            MapType type,
            int firstDepth = NoMapDepth,
            int lastDepth = NoMapDepth,
            bool mainMap = false,
            bool mainMapRequired = false
        )
            : this(name, type, firstDepth, lastDepth, mainMap, mainMapRequired, tag) { }
    }
}

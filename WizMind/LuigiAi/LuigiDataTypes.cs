using System.Runtime.InteropServices;

namespace WizMind.LuigiAi
{
    // Cogmind is 32-bit so pointers are int-sized
    using CogmindPointer = int;

    public enum MapType : int
    {
        MAP_NONE = 0,
        MAP_SAN = -1,
        MAP_YRD = 1,
        MAP_MAT,
        MAP_FAC,
        MAP_RES,
        MAP_ACC,
        MAP_SUR,
        MAP_MIN,
        MAP_EXI,
        MAP_STO,
        MAP_REC,
        MAP_SCR,
        MAP_WAS,
        MAP_GAR,
        MAP_DSF,
        MAP_SUB,
        MAP_LOW,
        MAP_UPP,
        MAP_PRO,
        MAP_DEE,
        MAP_ZIO,
        MAP_DAT,
        MAP_ZHI,
        MAP_WAR,
        MAP_EXT,
        MAP_CET,
        MAP_ARC,
        MAP_HUB,
        MAP_ARM,
        MAP_LAB,
        MAP_QUA,
        MAP_TES,
        MAP_SEC,
        MAP_FRG,
        MAP_COM,
        MAP_AC0,
        MAP_LAI,
        MAP_TOW,
        MAP_W00 = 1000,
        MAP_W01,
        MAP_W02,
        MAP_W03,
        MAP_W04,
        MAP_W05,
        MAP_W06,
        MAP_W07,
        MAP_W08,
        MAP_W09,
    }

    // NOTE: Any types that had padding bytes at the end of them need to be
    // manually specified to marshal only the size of the struct member
    // Cogmind does not memset the full size of the struct and only assigns
    // the relevant struct members. Any bytes after the end are suceptible to
    // containing garbage data that can cause improper marshalling of any
    // type < 4 bytes (namely bools)
    [StructLayout(LayoutKind.Sequential)]
    public struct LuigiMachineHackingStruct
    {
        public int actionReady;
        public int detectChance;
        public int traceProgress;

        [MarshalAs(UnmanagedType.U1)]
        public bool lastHackSuccess;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LuigiPropStruct
    {
        public int ID;

        [MarshalAs(UnmanagedType.U1)]
        public bool interactivePiece;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LuigiItemStruct
    {
        public int ID;
        public int integrity;

        [MarshalAs(UnmanagedType.U1)]
        public bool equipped;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LuigiTileStruct
    {
        public int lastAction;
        public int lastFov;
        public int cell;
        public bool doorOpen;
        public CogmindPointer prop;
        public CogmindPointer entity;
        public CogmindPointer item;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LuigiEntityStruct
    {
        public int ID;
        public int integrity;
        public int relation;
        public int activeState;
        public int exposure;
        public int energy;
        public int matter;
        public int heat;
        public int systemCorruption;
        public int speed;
        public int inventorySize;
        public CogmindPointer inventoryPointer;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LuigiAiStruct
    {
        public int magic1;
        public int magic2;
        public int actionReady;
        public int mapWidth;
        public int mapHeight;
        public int locationDepth;
        public MapType locationMap;
        public CogmindPointer mapData;
        public int mapCursorIndex;
        public CogmindPointer player;
        public CogmindPointer machineHacking;
    }
}

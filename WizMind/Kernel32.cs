using System.Runtime.InteropServices;

namespace WizMind
{
    public static class Kernel32
    {
        [Flags]
        public enum AllocationProtectEnum : uint
        {
            PAGE_EXECUTE = 0x00000010,
            PAGE_EXECUTE_READ = 0x00000020,
            PAGE_EXECUTE_READWRITE = 0x00000040,
            PAGE_EXECUTE_WRITECOPY = 0x00000080,
            PAGE_NOACCESS = 0x00000001,
            PAGE_READONLY = 0x00000002,
            PAGE_READWRITE = 0x00000004,
            PAGE_WRITECOPY = 0x00000008,
            PAGE_GUARD = 0x00000100,
            PAGE_NOCACHE = 0x00000200,
            PAGE_WRITECOMBINE = 0x00000400
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            PROCESS_ALL_ACCESS = 0x001F0FFF,
            PROCESS_CREATE_PROCESS = 0x00080,
            PROCESS_CREATE_THREAD = 0x0002,
            PROCESS_DUP_HANDLE = 0x0040,
            PROCESS_QUERY_INFORMATION = 0x0400,
            PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,
            PROCESS_SET_INFORMATION = 0x0200,
            PROCESS_SET_QUOTA = 0x0100,
            PROCESS_SUSPEND_RESUME = 0x0800,
            PROCESS_TERMINATE = 0x0001,
            PROCESS_VM_OPERATION = 0x0008,
            PROCESS_VM_READ = 0x0010,
            PROCESS_VM_WRITE = 0x0020,
            PROCESS_SYNCHRONIZE = 0x00100000
        }

        [Flags]
        public enum MemoryBasicInformationState : uint
        {
            MEM_COMMIT = 0x1000,
            MEM_FREE = 0x10000,
            MEM_RESERVE = 0x2000
        }

        [Flags]
        public enum MemoryBasicInformationType : uint
        {
            MEM_IMAGE = 0x1000000,
            MEM_MAPPED = 0x40000,
            MEM_PRIVATE = 0x20000
        }

        public struct MemoryBasicInformation
        {
            public nint BaseAddress;
            public nint AllocationBase;
            public AllocationProtectEnum AllocationProtect;
            public nint RegionSize;
            public MemoryBasicInformationState State;
            public AllocationProtectEnum Protect;
            public MemoryBasicInformationType Type;
        }

        public struct SystemInfo
        {
            public ushort processorArchitecture;
            private readonly ushort reserved;
            public uint pageSize;
            public nint minimumApplicationAddress;
            public nint maximumApplicationAddress;
            public nint activeProcessorMask;
            public uint numberOfProcessors;
            public uint processorType;
            public uint allocationGranularity;
            public ushort processorLevel;
            public ushort processorRevision;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(nint hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern nint OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(nint hProcess, nint lpBaseAddress, byte[] buffer, nint size, out nint lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(nint hProcess, nint lpBaseAddress, nint buffer, nint size, out nint lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void GetSystemInfo(out SystemInfo lpSystemInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern nint VirtualQueryEx(nint hProcess, nint lpAddress, out MemoryBasicInformation lpBuffer, nint dwLength);

    }
}

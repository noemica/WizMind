using System.Diagnostics;
using System.Runtime.InteropServices;
using WizMind.LuigiAi;
using static WizMind.Kernel32;

namespace WizMind
{
    public class CogmindProcess
    {
        private readonly nint ptr;

        public CogmindProcess(Process process, nint ptr)
        {
            this.Process = process;
            this.ptr = ptr;

            this.LuigiAiData = new LuigiAiData(this);
        }

        public LuigiAiData LuigiAiData { get; }

        public Process Process { get; }

        public LuigiAiStruct FetchLuigiAiStruct()
        {
            return this.FetchStruct<LuigiAiStruct>(this.ptr);
        }

        public List<T> FetchList<T>(nint arrayPtr, int arraySize)
        {
            const int MaxReadSize = 0x1000;

            if (this.Process.HasExited)
            {
                throw new Exception("Cogmind.exe process exited");
            }

            // Copy the data from the process memory for each element, then marshal as a C# struct
            var list = new List<T>(arraySize);
            var size = Marshal.SizeOf<T>();

            // For better performance, read the memory in chunks of up to 4kb
            // Process all elements in that chunk before moving onto the next one
            var maxReadCount = MaxReadSize / size;
            var readIterations = Math.Ceiling((float)arraySize / maxReadCount);

            if (readIterations == 1)
            {
                // If we are only reading one chunk of data then allocate just
                // that chunk size instead of the full 4k
                maxReadCount = arraySize;
            }
            var buffer = Marshal.AllocHGlobal(size * maxReadCount);

            try
            {
                // Read each chunk
                for (var i = 0; i < readIterations; i++)
                {
                    var elementsLeft = arraySize - (i * maxReadCount);
                    var elementsToRead = Math.Min(elementsLeft, maxReadCount);
                    var sizeToRead = elementsToRead * size;

                    ReadProcessMemory(this.Process.Handle, arrayPtr + (i * size), buffer, sizeToRead, out var bytesRead);

                    if (bytesRead != sizeToRead)
                    {
                        throw new Exception("Couldn't read required number of bytes");
                    }

                    // Read each element in the chunk and marshal it to the struct type
                    for (var j = 0; j < elementsToRead; j++)
                    {
                        var val = Marshal.PtrToStructure<T>(buffer + (j * size));
                        if (val == null)
                        {
                            throw new Exception("Couldn't convert process data to structure");
                        }

                        list.Add(val);
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }

            return list;
        }

        public T FetchStruct<T>(nint ptr)
        {
            if (this.Process.HasExited)
            {
                throw new Exception("Cogmind.exe process exited");
            }

            // Copy the data from the process memory, then marshal as a C# struct
            var size = Marshal.SizeOf<T>();
            var buffer = Marshal.AllocHGlobal(size);
            try
            {
                ReadProcessMemory(this.Process.Handle, ptr, buffer, size, out var bytesRead);

                if (bytesRead == size)
                {
                    var val = Marshal.PtrToStructure<T>(buffer);
                    if (val == null)
                    {
                        throw new Exception("Couldn't convert process data to structure");
                    }

                    return val;
                }

                throw new Exception("Couldn't read Cogmind.exe process data");
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static nint? GetAddress(Process process)
        {
            // Cogmind defined magic numbers to locate LuigiAi struct
            const int Magic1 = 1689123404;
            const int Magic2 = 2035498713;

            // Determine range of memory addresses
            var sys_info = new SystemInfo();
            GetSystemInfo(out sys_info);
            var proc_min_address = sys_info.minimumApplicationAddress;
            var proc_max_address = sys_info.maximumApplicationAddress;

            var processHandle = process.Handle;

            while (proc_min_address < proc_max_address)
            {
                // Query process memory info
                var res = VirtualQueryEx(
                    processHandle, proc_min_address, out var mem_basic_info,
                    Marshal.SizeOf(typeof(MemoryBasicInformation)));

                if (res == 0)
                {
                    Console.WriteLine("VirtualQueryEx failed");
                    return null;
                }

                if (mem_basic_info.Protect == AllocationProtectEnum.PAGE_READWRITE &&
                    mem_basic_info.State == MemoryBasicInformationState.MEM_COMMIT)
                {
                    // Found mapped page, read memory now
                    var buffer = new byte[mem_basic_info.RegionSize];

                    ReadProcessMemory(
                        processHandle, mem_basic_info.BaseAddress, buffer, mem_basic_info.RegionSize, out var bytesRead);

                    nint i = 0;
                    while (i < mem_basic_info.RegionSize - 7)
                    {
                        // Search for magic1 and magic2 numbers every 4 bytes
                        if (BitConverter.ToUInt32(buffer.AsSpan((int)i, 4)) == Magic1
                            && BitConverter.ToUInt32(buffer.AsSpan((int)i + 4, 4)) == Magic2)
                        {
                            Console.WriteLine($"Found at {mem_basic_info.BaseAddress + i:x}");
                            return mem_basic_info.BaseAddress + i;
                        }

                        i += 4;
                    }
                }

                proc_min_address += mem_basic_info.RegionSize;
            }

            return null;
        }

        public static CogmindProcess? TryCreate(Process process)
        {
            var address = GetAddress(process);

            return address == null ? null : new CogmindProcess(process, address.Value);
        }
    }
}

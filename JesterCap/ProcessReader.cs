/**
* Responsible for reading memory from the Spelunky process.
*/

using System;
using System.Diagnostics;

namespace JesterCap
{
    public static class ProcessReader
    {
        private const int PROCESS_WM_READ = 0x0010;

        // These offsets need to be updated with each new version of Spelunky 2.
        // Current as of 1.17.0f (2020-11-18)
        private const int OFFSET_GAME_DATA = 0x221ABF60;
        private const int OFFSET_SCORE = 0x221BE664;
        private const int OFFSET_FRAME_COUNT = 0x9FC;      // the number of frames spent in the current level (unpaused)
        private const int OFFSET_NUM_ITEMS = 0x2DA;        // the number of items picked up
        private const int OFFSET_FIRST_ITEM_ID = 0x2E0;    // the first item ID in memory
        private const int SIZE_ITEM_STRUCT = 0x14;         // the space between each item ID

        private const int POINTER_SIZE = 8;

        public static Process SpelunkyProcess = null;
        public static IntPtr processHandle;
        private static IntPtr baseAddress;

        public static void LoadProcess(Process process)
        {
            SpelunkyProcess = process;
            baseAddress = process.MainModule.BaseAddress;
            processHandle = KernelDll.OpenProcess(PROCESS_WM_READ, false, process.Id);
        }

        private static byte[] ReadMemoryIntoBuffer(IntPtr address, int numBytes)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[numBytes];
            KernelDll.ReadProcessMemory(processHandle, address, buffer, numBytes, ref bytesRead);
            return buffer;
        }

        public static IntPtr GetGamePointer()
        {
            byte[] buffer = ReadMemoryIntoBuffer(baseAddress + OFFSET_GAME_DATA, POINTER_SIZE);
            return (IntPtr)BitConverter.ToInt64(buffer, 0);
        }

        public static int GetScore()
        {
            byte[] buffer = ReadMemoryIntoBuffer(baseAddress + OFFSET_SCORE, 4);
            return BitConverter.ToInt32(buffer, 0);
        }

        public static int GetFrameCount(IntPtr gamePointer)
        {
            byte[] buffer = ReadMemoryIntoBuffer(gamePointer + OFFSET_FRAME_COUNT, 4);
            return BitConverter.ToInt32(buffer, 0);
        }

        public static byte GetNumItems(IntPtr gamePointer)
        {
            byte[] buffer = ReadMemoryIntoBuffer(gamePointer + OFFSET_NUM_ITEMS, 1);
            return buffer[0];
        }

        public static int GetItemId(IntPtr gamePointer, int itemIndex)
        {
            int totalOffset = OFFSET_FIRST_ITEM_ID + itemIndex * SIZE_ITEM_STRUCT;
            byte[] buffer = ReadMemoryIntoBuffer(gamePointer + totalOffset, 4);
            return BitConverter.ToInt32(buffer, 0);
        }
    }
}

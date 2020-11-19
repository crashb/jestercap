/**
 * Responsible for wrapping kernel32.dll. This DLL provides functionality
 * that allows us to read memory from running processes.
 */

using System;
using System.Runtime.InteropServices;

namespace JesterCap
{
    public static class KernelDll
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

    }
}

/**
 * Responsible for locating the Spelunky 2 process and exposing a handle
 * to it.
 */

using System.Diagnostics;

namespace JesterCap
{
    public static class ProcessSearcher
    {
        public const string SPELUNKY_PROCESS_NAME = "Spel2";

        public static Process SearchForSpelunkyProcess()
        {
            Process[] matchingProcesses = Process.GetProcessesByName(SPELUNKY_PROCESS_NAME);
            if (matchingProcesses.Length > 0)
            {
                return matchingProcesses[0];
            }
            else
            {
                return null;
            }
        }
    }
}

/**
 * Responsible for locating the Spelunky 2 process and exposing a handle
 * to it.
 */

using System;
using System.Diagnostics;
using System.Management;
using System.Timers;

namespace JesterCap
{
    public static class ProcessSearcher
    {
        public const string SPELUNKY_PROCESS_NAME = "Spel2";
        private const int PROCESS_SEARCH_INTERVAL_MS = 500;

        public static void StartSearchingForSpelunkyProcess(Action<Process> processFound)
        {
            Process spelunkyProcess = GetSpelunkyProcess();
            if (spelunkyProcess != null)
            {
                processFound(spelunkyProcess);
                return;
            };

            try
            {
                ManagementEventWatcher processCreationWatcher = new ManagementEventWatcher("SELECT * FROM Win32_ProcessStartTrace");
                processCreationWatcher.EventArrived += new EventArrivedEventHandler((sender, e) =>
                {
                    string processName = (string)e.NewEvent.Properties["ProcessName"].Value;
                    if (processName == SPELUNKY_PROCESS_NAME + ".exe")
                    {
                        spelunkyProcess = GetSpelunkyProcess();
                        if (spelunkyProcess != null) // Should always be true
                        {
                            processCreationWatcher.Stop();
                            processFound(spelunkyProcess);
                        }
                    }
                });
                processCreationWatcher.Start();
            }
            catch (ManagementException) // Thrown if the application doesn't have administrative rights, revert to slower method
            {
                Timer processSearchTimer = new Timer(PROCESS_SEARCH_INTERVAL_MS);
                processSearchTimer.Elapsed += (sender, e) =>
                {
                    spelunkyProcess = GetSpelunkyProcess();
                    if (spelunkyProcess != null)
                    {
                        processSearchTimer.Stop();
                        processFound(spelunkyProcess);
                    }
                };
                processSearchTimer.Start();
            }
        }

        private static Process GetSpelunkyProcess()
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

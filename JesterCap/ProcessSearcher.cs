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

        private const int PROCESS_SEARCH_INTERVAL_MS = 5000;

        public static void StartSearchingForSpelunkyProcess(Action<Process> processFound)
        {
            Process spelunkyProcess;
            try
            {
                spelunkyProcess = TryGetSpelunkyProcess();
                if (spelunkyProcess != null)
                {
                    processFound(spelunkyProcess);
                    return;
                }

                ManagementEventWatcher processCreationWatcher = new ManagementEventWatcher("SELECT * FROM Win32_ProcessStartTrace");
                processCreationWatcher.EventArrived += new EventArrivedEventHandler((sender, e) =>
                {
                    if ((string)e.NewEvent.Properties["ProcessName"].Value == SPELUNKY_PROCESS_NAME + ".exe")
                    {
                        spelunkyProcess = TryGetSpelunkyProcess();
                        if (spelunkyProcess != null)// Should always be true
                        {
                            processFound(TryGetSpelunkyProcess());
                            processCreationWatcher.Stop();
                        }
                    }
                });
                processCreationWatcher.Start();
            }
            catch (ManagementException)
            // Thrown if the application doesn't have administrative rights, revert to slower method
            {
                Timer processSearchTimer = new Timer(PROCESS_SEARCH_INTERVAL_MS);
                processSearchTimer.Elapsed += (sender, e) =>
                {
                    spelunkyProcess = TryGetSpelunkyProcess();
                    if (spelunkyProcess != null)
                    {
                        processFound(spelunkyProcess);
                        processSearchTimer.Stop();
                    }
                };
                processSearchTimer.Start();
            }
        }

        private static Process TryGetSpelunkyProcess()
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

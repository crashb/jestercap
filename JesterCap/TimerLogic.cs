using System;
using System.Windows;
using System.Linq;
using System.Timers;
using System.Configuration;
using System.Media;
using System.Globalization;

namespace JesterCap
{
    public class TimerLogic
    {
        private const int POLL_INTERVAL_MS = 16;  // needs to be >= 60 times a second
        private const int TRUE_CROWN_ID = 559;
        private const int TELEPORT_FRAME_INTERVAL = 22 * 60;

        private Timer pollForCrown;
        private Timer pollForTime;

        private int crownTimerStartFrame;
        private int lastFramesUntilTeleport;
        private double[] warningTimes;
        private bool[] alreadyWarned;

        MainWindow mainWindow;

        public TimerLogic(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            pollForCrown = new Timer(POLL_INTERVAL_MS);
            pollForCrown.Stop();
            pollForCrown.Elapsed += PollForCrown;

            pollForTime = new Timer(POLL_INTERVAL_MS);
            pollForTime.Stop();
            pollForTime.Elapsed += PollForTime;
        }

        public void Start()
        {
            LoadWarningTimesFromConfig();
            pollForCrown.Start();
        }

        public void Stop()
        {
            pollForCrown.Stop();
            pollForTime.Stop();
        }

        private void LoadWarningTimesFromConfig()
        {
            try
            {
                string warningTimesSetting = ConfigurationManager.AppSettings["warningTimes"];
                if (warningTimesSetting != null)
                {
                    warningTimes = warningTimesSetting.Split(',').Select(s => {
                        string trimmedSetting = s.Trim();
                        return double.Parse(trimmedSetting, CultureInfo.InvariantCulture);
                    }).ToArray();
                }
            }
            catch (ConfigurationErrorsException)
            {
                // if an exception was thrown, we can continue on with the default settings
            }

            if (warningTimes == null || warningTimes.Length == 0)
            {
                MessageBox.Show("JesterCap.exe.config could not be loaded. Using default settings (timer at 5s, 2.5s, and 1s intervals).", "Configuration Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                warningTimes = new double[] { 5, 2.5, 1 };
            }
            alreadyWarned = Enumerable.Repeat(false, warningTimes.Length).ToArray();
        }

        private void PollForCrown(object sender, ElapsedEventArgs e)
        {
            if (ProcessReader.SpelunkyProcess == null || ProcessReader.SpelunkyProcess.HasExited)
            {
                pollForCrown.Stop();
                return;
            }
            int currentScore = ProcessReader.GetScore();
            mainWindow.Dispatcher.Invoke(() => {
                mainWindow.LabelReader.Content = string.Format("Score: {0}", currentScore);
            });

            IntPtr gamePointer = ProcessReader.GetGamePointer();
            int numItems = ProcessReader.GetNumItems(gamePointer);
            for (int i = 0; i < numItems; i++)
            {
                int itemId = ProcessReader.GetItemId(gamePointer, i);
                if (itemId == TRUE_CROWN_ID)
                {
                    crownTimerStartFrame = ProcessReader.GetFrameCount(gamePointer);
                    pollForCrown.Stop();
                    pollForTime.Start();
                    mainWindow.Dispatcher.Invoke(() => {
                        mainWindow.SetActivePanelReader(false);
                        mainWindow.SetActivePanelTimer(true);
                    });
                    return;
                }
            }
        }

        private void PollForTime(object sender, ElapsedEventArgs e)
        {
            if (ProcessReader.SpelunkyProcess == null || ProcessReader.SpelunkyProcess.HasExited)
            {
                pollForTime.Stop();
                return;
            }

            IntPtr gamePointer = ProcessReader.GetGamePointer();
            int numItems = ProcessReader.GetNumItems(gamePointer);
            if (numItems == 0)  // e.g. if the player has died or reset
            {
                pollForTime.Stop();
                pollForCrown.Start();
                mainWindow.Dispatcher.Invoke(() => {
                    mainWindow.SetActivePanelReader(true);
                    mainWindow.SetActivePanelTimer(false);
                });
                return;
            }

            int currentFrame = ProcessReader.GetFrameCount(gamePointer);
            if (currentFrame == 0)
            {
                crownTimerStartFrame = 0;
            }
            int framesUntilTeleport = TELEPORT_FRAME_INTERVAL - (currentFrame - crownTimerStartFrame) % TELEPORT_FRAME_INTERVAL;
            int numTeleportsThisLevel = (int)Math.Floor((currentFrame - crownTimerStartFrame) / (double)TELEPORT_FRAME_INTERVAL);
            int nextTeleportFrame = (numTeleportsThisLevel + 1) * TELEPORT_FRAME_INTERVAL + crownTimerStartFrame;
            double secondsUntilTeleport = framesUntilTeleport / 60.000;
            double nextTeleportSeconds = nextTeleportFrame / 60.000;
            for (int i = 0; i < warningTimes.Length; i++)
            {
                double warningTime = warningTimes[i];
                if (secondsUntilTeleport <= warningTime && !alreadyWarned[i])
                {
                    alreadyWarned[i] = true;
                    PlayNotificationSound();
                }
            }

            if (framesUntilTeleport > lastFramesUntilTeleport)
            {
                alreadyWarned = Enumerable.Repeat(false, warningTimes.Length).ToArray();
            }
            mainWindow.Dispatcher.Invoke(() =>
            {
                mainWindow.LabelTimer.Content = string.Format("{0:0.000}", secondsUntilTeleport);
                mainWindow.LabelNextTeleport.Content = string.Format("{0:0}:{1:00}", Math.Floor(nextTeleportSeconds / 60.0), nextTeleportSeconds % 60);
            });
            lastFramesUntilTeleport = framesUntilTeleport;
        }

        // https://docs.microsoft.com/en-us/dotnet/desktop/winforms/controls/how-to-play-a-sound-embedded-in-a-resource-from-a-windows-form
        private void PlayNotificationSound()
        {
            string uri = (new Uri("./Resources/notification.wav", UriKind.Relative)).ToString();
            SoundPlayer player = new SoundPlayer(uri);
            player.Play();
        }
    }
}

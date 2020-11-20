﻿using System;
using System.Windows;
using System.Diagnostics;
using System.Configuration;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Timers;
using System.Media;
using System.Linq;

namespace JesterCap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool activePanelAttach = true;
        private bool activePanelReader = false;
        private bool activePanelTimer = false;

        private Timer timerPollForCrown;
        private Timer timerPollForTime;

        private int crownTimerStartFrame;
        private int lastFramesUntilTeleport;
        private double[] warningTimes;
        private bool[] alreadyWarned;

        private const string COLOR_PANEL_BG_INACTIVE = "#88000000";
        private const string COLOR_PANEL_BG_ACTIVE = "#BB000000";
        private const string COLOR_LABEL_TIMER_INACTIVE = "#88000000";
        private const string COLOR_LABEL_TIMER_ACTIVE = "#FFFFFFFF";
        private const string ICON_ATTACH_ACTIVE = "Resources/spelunky_2_shadow.png";
        private const string ICON_ATTACH_INACTIVE = "Resources/spelunky_2_icon.png";
        private const string ICON_READER_ACTIVE = "Resources/true_crown_shadow.png";
        private const string ICON_READER_INACTIVE = "Resources/true_crown_icon.png";

        private const int POLL_INTERVAL_MS = 16;  // needs to be >= 60 times a second
        private const int TRUE_CROWN_ID = 552;
        private const int TELEPORT_FRAME_INTERVAL = 22 * 60;

        public MainWindow()
        {
            InitializeComponent();
            LoadWarningTimesFromConfig();

            timerPollForCrown = new Timer(POLL_INTERVAL_MS);
            timerPollForCrown.Stop();
            timerPollForCrown.Elapsed += PollForCrown;
            
            timerPollForTime = new Timer(POLL_INTERVAL_MS);
            timerPollForTime.Stop();
            timerPollForTime.Elapsed += PollForTime;

            SetActivePanelAttach(activePanelAttach);
            SetActivePanelReader(activePanelReader);
            SetActivePanelTimer(activePanelTimer);
        }

        private void LoadWarningTimesFromConfig()
        {
            warningTimes = ConfigurationManager.AppSettings["warningTimes"].Split(',').Select(s => double.Parse(s.Trim())).ToArray();
            if (warningTimes.Length == 0)
            {
                warningTimes = new double[] { 5 };
            }
            alreadyWarned = Enumerable.Repeat(false, warningTimes.Length).ToArray();
        }

        private void PollForCrown(object sender, ElapsedEventArgs e)
        {
            if (ProcessReader.SpelunkyProcess == null || ProcessReader.SpelunkyProcess.HasExited)
            {
                timerPollForCrown.Stop();
                return;
            }
            int currentScore = ProcessReader.GetScore();
            Dispatcher.Invoke(() => {
                LabelReader.Content = string.Format("Current Score: {0}", currentScore);
            });

            IntPtr gamePointer = ProcessReader.GetGamePointer();
            int numItems = ProcessReader.GetNumItems(gamePointer);
            for (int i = 0; i < numItems; i++)
            {
                int itemId = ProcessReader.GetItemId(gamePointer, i);
                if (itemId == TRUE_CROWN_ID)
                {
                    crownTimerStartFrame = ProcessReader.GetFrameCount(gamePointer);
                    timerPollForCrown.Stop();
                    timerPollForTime.Start();
                    Dispatcher.Invoke(() => {
                        SetActivePanelReader(false);
                        SetActivePanelTimer(true);
                    });
                    return;
                }
            }
        }

        private void PollForTime(object sender, ElapsedEventArgs e)
        {
            if (ProcessReader.SpelunkyProcess == null || ProcessReader.SpelunkyProcess.HasExited)
            {
                timerPollForTime.Stop();
                return;
            }

            IntPtr gamePointer = ProcessReader.GetGamePointer();
            int numItems = ProcessReader.GetNumItems(gamePointer);
            if (numItems == 0)  // e.g. if the player has died or reset
            {
                timerPollForTime.Stop();
                timerPollForCrown.Start();
                Dispatcher.Invoke(() => {
                    SetActivePanelReader(true);
                    SetActivePanelTimer(false);
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
            Dispatcher.Invoke(() =>
            {
                LabelTimer.Content = string.Format("{0:0.000}", secondsUntilTeleport);
                LabelNextTeleport.Content = string.Format("{0:0}:{1:00}", Math.Floor(nextTeleportSeconds / 60.0), nextTeleportSeconds % 60);
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

        private void ButtonAttach_Click(object sender, RoutedEventArgs e)
        {
            Process spelunkyProcess = ProcessSearcher.SearchForSpelunkyProcess();
            if (spelunkyProcess == null)
            {
                MessageBox.Show(string.Format("Process \"{0}\" not found. Please try again.", ProcessSearcher.SPELUNKY_PROCESS_NAME), "Process Not Found");
                return;
            }

            ProcessReader.LoadProcess(spelunkyProcess);
            spelunkyProcess.EnableRaisingEvents = true;
            spelunkyProcess.Exited += SpelunkyProcess_Exited;

            SetActivePanelAttach(false);
            SetActivePanelReader(true);
            timerPollForCrown.Start();
        }

        private void SpelunkyProcess_Exited(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => {
                SetActivePanelAttach(true);
                SetActivePanelReader(false);
                SetActivePanelTimer(false);
            });
        }

        private Brush CreateBrush(string colorCode)
        {
            BrushConverter converter = new BrushConverter();
            return (Brush)converter.ConvertFromString(colorCode);
        }

        private BitmapImage CreateImageSource(string path) {
            Uri uriSource = new Uri(string.Format("/JesterCap;component/{0}", path), UriKind.Relative);
            return new BitmapImage(uriSource);
        }

        private void SetActivePanelAttach(bool active)
        {
            activePanelAttach = active;

            PanelAttach.Background = CreateBrush(active ? COLOR_PANEL_BG_ACTIVE : COLOR_PANEL_BG_INACTIVE);
            IconAttach.Source = CreateImageSource(active ? ICON_ATTACH_ACTIVE : ICON_ATTACH_INACTIVE);
            ButtonAttach.IsEnabled = active;
            ButtonAttach.Content = active ? "Attach" : "Attached";
        }

        private void SetActivePanelReader(bool active)
        {
            activePanelReader = active;

            PanelReader.Background = CreateBrush(active ? COLOR_PANEL_BG_ACTIVE : COLOR_PANEL_BG_INACTIVE);
            IconReader.Source = CreateImageSource(active ? ICON_READER_ACTIVE : ICON_READER_INACTIVE);
            LabelReader.Visibility = active ? Visibility.Visible : Visibility.Hidden;
        }

        private void SetActivePanelTimer(bool active)
        {
            activePanelTimer = active;

            PanelTimer.Background = CreateBrush(active ? COLOR_PANEL_BG_ACTIVE : COLOR_PANEL_BG_INACTIVE);
            LabelTimer.Foreground = CreateBrush(active ? COLOR_LABEL_TIMER_ACTIVE : COLOR_LABEL_TIMER_INACTIVE);
            LabelNextTeleport.Foreground = CreateBrush(active ? COLOR_LABEL_TIMER_ACTIVE : COLOR_LABEL_TIMER_INACTIVE);
            if (!active)
            {
                LabelTimer.Content = "0.000";
                LabelNextTeleport.Content = "0:00";
            }
            BegImage.Opacity = active ? 1 : 0;
        }
    }
}

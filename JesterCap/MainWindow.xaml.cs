using System;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JesterCap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TimerLogic timer;

        private bool activePanelAttach = true;
        private bool activePanelReader = false;
        private bool activePanelTimer = false;

        private const string COLOR_PANEL_BG_INACTIVE = "#88000000";
        private const string COLOR_PANEL_BG_ACTIVE = "#BB000000";
        private const string COLOR_LABEL_TIMER_INACTIVE = "#88000000";
        private const string COLOR_LABEL_TIMER_ACTIVE = "#FFFFFFFF";
        private const string ICON_ATTACH_ACTIVE = "Resources/spelunky_2_shadow.png";
        private const string ICON_ATTACH_INACTIVE = "Resources/spelunky_2_icon.png";
        private const string ICON_READER_ACTIVE = "Resources/true_crown_shadow.png";
        private const string ICON_READER_INACTIVE = "Resources/true_crown_icon.png";

        public MainWindow()
        {
            InitializeComponent();
            timer = new TimerLogic(this);

            SetActivePanelAttach(activePanelAttach);
            SetActivePanelReader(activePanelReader);
            SetActivePanelTimer(activePanelTimer);
        }

        private void OnSpelunkyProcessFound(Process spelunkyProcess)
        {
            ProcessReader.LoadProcess(spelunkyProcess);
            spelunkyProcess.EnableRaisingEvents = true;
            spelunkyProcess.Exited += SpelunkyProcess_Exited;

            Dispatcher.Invoke(() => {
                SetActivePanelAttach(false);
                SetActivePanelReader(true);
            });
            timer.Start();
        }

        private void SpelunkyProcess_Exited(object sender, EventArgs e)
        {
            timer.Stop();
            Dispatcher.Invoke(() => {
                SetActivePanelReader(false);
                SetActivePanelTimer(false);
                SetActivePanelAttach(true);  // updating PanelAttach last avoids a race condition
            });
        }

        private Brush CreateBrush(string colorCode)
        {
            BrushConverter converter = new BrushConverter();
            return (Brush)converter.ConvertFromString(colorCode);
        }

        private BitmapImage CreateImageSource(string path)
        {
            Uri uriSource = new Uri(string.Format("/JesterCap;component/{0}", path), UriKind.Relative);
            return new BitmapImage(uriSource);
        }

        public void SetActivePanelAttach(bool active)
        {
            activePanelAttach = active;

            PanelAttach.Background = CreateBrush(active ? COLOR_PANEL_BG_ACTIVE : COLOR_PANEL_BG_INACTIVE);
            IconAttach.Source = CreateImageSource(active ? ICON_ATTACH_ACTIVE : ICON_ATTACH_INACTIVE);

            if (active)
            {
                ProcessSearcher.StartSearchingForSpelunkyProcess(OnSpelunkyProcessFound);
            }
        }

        public void SetActivePanelReader(bool active)
        {
            activePanelReader = active;

            PanelReader.Background = CreateBrush(active ? COLOR_PANEL_BG_ACTIVE : COLOR_PANEL_BG_INACTIVE);
            IconReader.Source = CreateImageSource(active ? ICON_READER_ACTIVE : ICON_READER_INACTIVE);
            LabelReader.Visibility = active ? Visibility.Visible : Visibility.Hidden;
        }

        public void SetActivePanelTimer(bool active)
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

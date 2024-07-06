using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AimAssist.Units.Implementation.ApplicationLog
{
    /// <summary>
    /// ApplicationLogControl.xaml の相互作用ロジック
    /// </summary>
    public partial class ApplicationLogControl : UserControl
    {
        public ObservableCollection<LogEntry> LogEntries { get; } = new ObservableCollection<LogEntry>();
        private string _logDirectoryPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AimAssist", "ApplicationLog");


        public ApplicationLogControl()
        {
            InitializeComponent();
            LoadLogFromFile();
            DrawTimeline();
            this.DataContext = this;
        }

        private void LoadLogFromFile()
        {
            string fileName = $"ActiveWindowLog_{DateTime.Now:yyyy_MM}.json";
            string filePath = System.IO.Path.Combine(_logDirectoryPath, fileName);

            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    var logEntry = JsonConvert.DeserializeObject<LogEntry>(line);
                    LogEntries.Add(logEntry);
                }
            }
        }

        private void DrawTimeline()
        {
            TimelineCanvas.Children.Clear();
            double canvasWidth = TimelineCanvas.ActualWidth;
            double canvasHeight = TimelineCanvas.ActualHeight;
            double pixelsPerMinute = canvasWidth / 1440; // 1日の分数(1440分)に基づく

            foreach (var entry in LogEntries)
            {
                if (DateTime.TryParse(entry.Time, out DateTime timestamp))
                {
                    double startX = timestamp.TimeOfDay.TotalMinutes * pixelsPerMinute;
                    double width = 5; // 固定幅、必要に応じて調整

                    Rectangle rect = new Rectangle
                    {
                        Fill = Brushes.Blue,
                        Width = width,
                        Height = canvasHeight
                    };

                    Canvas.SetLeft(rect, startX);
                    Canvas.SetTop(rect, 0);
                    TimelineCanvas.Children.Add(rect);
                }
            }
        }
    }

    public class LogEntry
    {
        public string Time { get; set; }
        public string Title { get; set; }
        public string App { get; set; }
    }

    public class ActiveWindowInfo
    {
        public string WindowTitle { get; set; }
        public string AppName { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

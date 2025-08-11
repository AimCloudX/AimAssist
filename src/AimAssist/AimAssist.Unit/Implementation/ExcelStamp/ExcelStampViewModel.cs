using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Web;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace AimAssist.Units.Implementation.ExcelStamp
{
    public enum StampType
    {
        Circle,      // 丸印
        Square,      // 角印
        DateStamp,   // 日付印
        Confidential,// 機密印
        Approved,    // 承認印
        Received,    // 受領印
        Paid         // 払済印
    }

    public class ExcelStampViewModel : INotifyPropertyChanged
    {
        private string _personalName = "";
        private StampType _stampType = StampType.Circle;
        private string _fontSize = "14";
        private Color _stampColor = Colors.Red;
        private double _stampSize = 60;
        private bool _isVerticalText = false;
        private DateTime _stampDate = DateTime.Today;

        public string PersonalName
        {
            get => _personalName;
            set
            {
                _personalName = value;
                OnPropertyChanged();
                UpdatePreview();
            }
        }

        public StampType StampType
        {
            get => _stampType;
            set
            {
                _stampType = value;
                OnPropertyChanged();
                UpdatePreview();
            }
        }

        public string FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                OnPropertyChanged();
                UpdatePreview();
            }
        }

        public Color StampColor
        {
            get => _stampColor;
            set
            {
                _stampColor = value;
                OnPropertyChanged();
                UpdatePreview();
            }
        }

        public double StampSize
        {
            get => _stampSize;
            set
            {
                _stampSize = value;
                OnPropertyChanged();
                UpdatePreview();
            }
        }


        public DateTime StampDate
        {
            get => _stampDate;
            set
            {
                _stampDate = value;
                OnPropertyChanged();
                UpdatePreview();
            }
        }

        public bool IsVerticalText
        {
            get => _isVerticalText;
            set
            {
                _isVerticalText = value;
                OnPropertyChanged();
                UpdatePreview();
            }
        }

        private BitmapSource _previewImage;
        public BitmapSource PreviewImage
        {
            get => _previewImage;
            set
            {
                _previewImage = value;
                OnPropertyChanged();
            }
        }

        public ICommand CopyToClipboardCommand { get; }
        public ICommand SetColorCommand { get; }

        public ExcelStampViewModel()
        {
            CopyToClipboardCommand = new RelayCommand(CopyToClipboard);
            SetColorCommand = new RelayCommand<string>(SetColor);
            UpdatePreview();
        }

        private void SetColor(string colorName)
        {
            switch (colorName)
            {
                case "Red": StampColor = Colors.Red; break;
                case "Blue": StampColor = Colors.Blue; break;
                case "Green": StampColor = Colors.Green; break;
                case "Black": StampColor = Colors.Black; break;
                default: StampColor = Colors.Red; break;
            }
        }

        private void UpdatePreview()
        {
            try
            {
                PreviewImage = CreateStampImage();
            }
            catch (Exception ex)
            {
                // エラー時は空の画像を設定
                PreviewImage = null;
            }
        }

        private BitmapSource CreateStampImage()
        {
            var size = (int)StampSize;
            var actualSize = size;
            var drawingVisual = new DrawingVisual();
            
            using (var context = drawingVisual.RenderOpen())
            {
                // 背景を透明に設定（PNG形式での透明性を確保）
                context.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, actualSize, actualSize));
                
                var brush = new SolidColorBrush(StampColor);
                var pen = new Pen(brush, 3); // 線を太くして本格的に
                pen.LineJoin = PenLineJoin.Round; // 線の接続部分を滑らかに
                pen.StartLineCap = PenLineCap.Round;
                pen.EndLineCap = PenLineCap.Round;
                
                var center = new Point(size / 2, size / 2);
                var radius = size / 2 - 3;
                
                // 印鑑の枠を描画
                if (StampType == StampType.Circle || StampType == StampType.DateStamp)
                {
                    // 二重枠で本格的な見た目に
                    context.DrawEllipse(null, pen, center, radius, radius);
                    
                    var innerPen = new Pen(brush, 1);
                    innerPen.LineJoin = PenLineJoin.Round;
                    context.DrawEllipse(null, innerPen, center, radius - 3, radius - 3);
                }
                else if (StampType == StampType.Square || 
                        StampType == StampType.Confidential || 
                        StampType == StampType.Approved || 
                        StampType == StampType.Received || 
                        StampType == StampType.Paid)
                {
                    var rect = new Rect(3, 3, size - 6, size - 6);
                    context.DrawRectangle(null, pen, rect);
                    
                    var innerRect = new Rect(6, 6, size - 12, size - 12);
                    var innerPen = new Pen(brush, 1);
                    innerPen.LineJoin = PenLineJoin.Round;
                    context.DrawRectangle(null, innerPen, innerRect);
                }
                
                // テキストを描画
                DrawStampText(context, size);
            }
            
            // より高品質なレンダリング設定
            var renderTargetBitmap = new RenderTargetBitmap(
                actualSize, actualSize, 96, 96, PixelFormats.Pbgra32);
                
            // アンチエイリアシングを有効にする
            RenderOptions.SetBitmapScalingMode(drawingVisual, BitmapScalingMode.HighQuality);
            RenderOptions.SetEdgeMode(drawingVisual, EdgeMode.Aliased);
            
            renderTargetBitmap.Render(drawingVisual);
            
            return renderTargetBitmap;
        }
        
        private void DrawStampText(DrawingContext context, int size)
        {
            var typeface = new Typeface(new FontFamily("Yu Gothic"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
            var brush = new SolidColorBrush(StampColor);
            
            
            switch (StampType)
            {
                case StampType.Circle:
                    DrawCircleText(context, typeface, brush, size);
                    break;
                case StampType.Square:
                    DrawSquareText(context, typeface, brush, size);
                    break;
                case StampType.DateStamp:
                    DrawDateStamp(context, typeface, brush, size);
                    break;
                case StampType.Confidential:
                    DrawBusinessStamp(context, typeface, brush, size, "機密");
                    break;
                case StampType.Approved:
                    DrawBusinessStamp(context, typeface, brush, size, "承認済");
                    break;
                case StampType.Received:
                    DrawBusinessStamp(context, typeface, brush, size, "受領");
                    break;
                case StampType.Paid:
                    DrawBusinessStamp(context, typeface, brush, size, "払済");
                    break;
            }
            
        }
        
        private void DrawCircleText(DrawingContext context, Typeface typeface, Brush brush, int size)
        {
            var center = new Point(size / 2, size / 2);
            
            if (!string.IsNullOrEmpty(PersonalName))
            {
                var fontSize = double.Parse(FontSize);
                
                if (IsVerticalText)
                {
                    // 縦書きで描画
                    DrawVerticalText(context, PersonalName, typeface, brush, center, fontSize);
                }
                else
                {
                    // 横書きで中央に描画
                    var formattedText = new FormattedText(PersonalName, 
                        System.Globalization.CultureInfo.CurrentCulture, 
                        FlowDirection.LeftToRight, typeface, fontSize, brush, 96);
                        
                    var textCenter = new Point(
                        center.X - formattedText.Width / 2,
                        center.Y - formattedText.Height / 2);
                        
                    context.DrawText(formattedText, textCenter);
                }
            }
        }
        
        private void DrawSquareText(DrawingContext context, Typeface typeface, Brush brush, int size)
        {
            var center = new Point(size / 2, size / 2);
            var fontSize = double.Parse(FontSize);
            
            if (!string.IsNullOrEmpty(PersonalName))
            {
                if (IsVerticalText)
                {
                    // 縦書きで描画
                    DrawVerticalText(context, PersonalName, typeface, brush, center, fontSize);
                }
                else
                {
                    // 横書きで中央に描画
                    var formattedText = new FormattedText(PersonalName,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight, typeface, fontSize, brush, 96);
                        
                    context.DrawText(formattedText, new Point(
                        center.X - formattedText.Width / 2,
                        center.Y - formattedText.Height / 2));
                }
            }
        }
        
        private void DrawVerticalText(DrawingContext context, string text, Typeface typeface, Brush brush, Point center, double fontSize)
        {
            if (string.IsNullOrEmpty(text)) return;
            
            var chars = text.ToCharArray();
            var charHeight = fontSize * 1.2; // 文字間隔
            var totalHeight = chars.Length * charHeight;
            var startY = center.Y - totalHeight / 2 + charHeight / 2;
            
            for (int i = 0; i < chars.Length; i++)
            {
                var formattedChar = new FormattedText(chars[i].ToString(),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, typeface, fontSize, brush, 96);
                
                var charPosition = new Point(
                    center.X - formattedChar.Width / 2,
                    startY + i * charHeight - formattedChar.Height / 2);
                    
                context.DrawText(formattedChar, charPosition);
            }
        }
        
        private void DrawTextOnCircle(DrawingContext context, string text, Typeface typeface, Brush brush, 
            Point center, double radius, double startAngle, bool clockwise)
        {
            var fontSize = double.Parse(FontSize) * 0.8;
            var angleStep = 360.0 / text.Length;
            
            for (int i = 0; i < text.Length; i++)
            {
                var angle = startAngle + (clockwise ? i * angleStep : -i * angleStep);
                var radians = angle * Math.PI / 180;
                
                var x = center.X + radius * Math.Cos(radians);
                var y = center.Y + radius * Math.Sin(radians);
                
                var formattedText = new FormattedText(text[i].ToString(),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, typeface, fontSize, brush, 96);
                    
                context.DrawText(formattedText, new Point(
                    x - formattedText.Width / 2,
                    y - formattedText.Height / 2));
            }
        }
        
        
        private void DrawDateStamp(DrawingContext context, Typeface typeface, Brush brush, int size)
        {
            var center = new Point(size / 2, size / 2);
            var fontSize = double.Parse(FontSize);
            
            // 年月日を分割して表示
            var year = StampDate.Year.ToString();
            var month = StampDate.Month.ToString();
            var day = StampDate.Day.ToString();
            
            var yearText = new FormattedText($"{year}年",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, typeface, fontSize * 0.6, brush, 96);
                
            var monthText = new FormattedText($"{month}月",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, typeface, fontSize * 0.8, brush, 96);
                
            var dayText = new FormattedText($"{day}日",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, typeface, fontSize, brush, 96);
            
            // 上から下へ配置
            context.DrawText(yearText, new Point(
                center.X - yearText.Width / 2,
                center.Y - fontSize * 1.2));
                
            context.DrawText(monthText, new Point(
                center.X - monthText.Width / 2,
                center.Y - fontSize * 0.3));
                
            context.DrawText(dayText, new Point(
                center.X - dayText.Width / 2,
                center.Y + fontSize * 0.5));
        }
        
        private void DrawBusinessStamp(DrawingContext context, Typeface typeface, Brush brush, int size, string text)
        {
            var center = new Point(size / 2, size / 2);
            var fontSize = double.Parse(FontSize);
            
            var formattedText = new FormattedText(text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, typeface, fontSize, brush, 96);
                
            context.DrawText(formattedText, new Point(
                center.X - formattedText.Width / 2,
                center.Y - formattedText.Height / 2));
        }

        private void CopyToClipboard()
        {
            if (PreviewImage == null)
            {
                MessageBox.Show("印鑑のプレビューが生成されていません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // PNG形式でクリップボードにコピー
                CopyImageToClipboardAsPng(PreviewImage);
                MessageBox.Show("印鑑画像をクリップボードにコピーしました。\nExcelでCtrl+Vで貼り付けできます。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"クリップボードへのコピーに失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyImageToClipboardAsPng(BitmapSource image)
        {
            // 透明な背景を持つPNGとしてエンコード
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var pngStream = new MemoryStream())
            {
                encoder.Save(pngStream);
                var pngData = pngStream.ToArray();

                // DataObjectを作成
                var dataObject = new DataObject();
                
                // PNG形式でデータを設定
                dataObject.SetData("PNG", new MemoryStream(pngData));
                
                // 標準的な画像形式としても設定
                dataObject.SetImage(image);
                
                // DIB形式での設定も試行（Officeアプリケーション向け）
                try
                {
                    // BMP形式にも変換してDIBとして設定
                    var bmpEncoder = new BmpBitmapEncoder();
                    bmpEncoder.Frames.Add(BitmapFrame.Create(image));
                    using (var bmpStream = new MemoryStream())
                    {
                        bmpEncoder.Save(bmpStream);
                        var bmpData = bmpStream.ToArray();
                        
                        // DIBヘッダーを除いたピクセルデータ部分を設定
                        if (bmpData.Length > 54) // BMPヘッダーサイズ
                        {
                            var dibData = new byte[bmpData.Length - 14]; // ファイルヘッダー(14バイト)を除く
                            Array.Copy(bmpData, 14, dibData, 0, dibData.Length);
                            dataObject.SetData(DataFormats.Dib, new MemoryStream(dibData));
                        }
                    }
                }
                catch
                {
                    // DIB設定に失敗しても続行
                }
                
                // クリップボードに設定（複数回試行）
                bool success = false;
                for (int i = 0; i < 3 && !success; i++)
                {
                    try
                    {
                        Clipboard.SetDataObject(dataObject, true);
                        success = true;
                    }
                    catch
                    {
                        System.Threading.Thread.Sleep(100); // 少し待機
                    }
                }
                
                if (!success)
                {
                    // 最後の手段として標準のSetImageを使用
                    Clipboard.SetImage(image);
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object? parameter)
        {
            _execute();
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object? parameter)
        {
            _execute((T)parameter);
        }
    }
}
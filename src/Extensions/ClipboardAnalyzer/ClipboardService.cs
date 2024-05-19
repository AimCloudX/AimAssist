using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ClipboardAnalyzer;

public class ClipboardService
{
    public static IEnumerable<IClipboardData> Load()
    {
        var ImageTypes = new List<string>{ "PNG"};
        var isVisibleList = new List<string>{ "HTML Format", "Text"};
        var dataObject = System.Windows.Clipboard.GetDataObject();
        var formats = dataObject.GetFormats();

        foreach (var format in formats)
        {
            if(ImageTypes.Contains(format))
            {
                var image = System.Windows.Clipboard.GetImage();
                yield return new ClipboardImage(format, image);
                continue;
            }

            var isVisible  = isVisibleList.Contains(format);
            var data = System.Windows.Clipboard.GetData(format);
            
            yield return new ClipboardData(format, data, isVisible);
        }
    }

    public interface IClipboardData
    {
        public string Format { get; }
        public object Data { get; }
        public bool IsDisabled { get; }
    }

    public class ClipboardData : IClipboardData
    {
        public ClipboardData(string format, object Data, bool isVisible)
        {
            this.Format = format;
            this.Data = Data;
            this.IsDisabled = !isVisible;
        }

        public string Format { get; }
        public object Data { get; }

        public bool IsDisabled { get; }
    }

    public class ClipboardImage : IClipboardData
    {
        public ClipboardImage(string format, BitmapSource bitmapsource)
        {
            this.Format = format;
            this.BitmapSource = bitmapsource;
        }

        public string Format { get; }

        public BitmapSource BitmapSource { get; }

        public bool IsDisabled => false;

        public object Data => BitmapSource;
    }
}
using System.Windows.Media.Imaging;

namespace ClipboardAnalyzer.DomainModels;

public class ClipboardImage : IClipboardData
{
    public ClipboardImage(string format, BitmapSource bitmapsource)
    {
        Format = format;
        BitmapSource = bitmapsource;
    }

    public string Format { get; }

    public BitmapSource BitmapSource { get; }

    public bool IsDisabled => false;

    public object Data => BitmapSource;
}
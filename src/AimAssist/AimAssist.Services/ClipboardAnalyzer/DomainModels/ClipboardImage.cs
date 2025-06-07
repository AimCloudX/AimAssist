using System.Windows.Media.Imaging;

namespace AimAssist.Services.ClipboardAnalyzer.DomainModels;

public class ClipboardImage(string format, BitmapSource bitmapsource) : IClipboardData
{
    public string Format { get; } = format;

    public BitmapSource BitmapSource { get; } = bitmapsource;

    public bool IsDisabled => false;

    public object Data => BitmapSource;
}
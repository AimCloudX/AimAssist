namespace AimAssist.Services.ClipboardAnalyzer.DomainModels;

public class ClipboardData : IClipboardData
{
    public ClipboardData(string format, object Data, bool isVisible)
    {
        Format = format;
        this.Data = Data;
        IsDisabled = !isVisible;
    }

    public string Format { get; }
    public object Data { get; }

    public bool IsDisabled { get; }
}
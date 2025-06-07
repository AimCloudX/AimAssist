namespace AimAssist.Services.ClipboardAnalyzer.DomainModels;

public class ClipboardData(string format, object data, bool isVisible) : IClipboardData
{
    public string Format { get; } = format;
    public object Data { get; } = data;

    public bool IsDisabled { get; } = !isVisible;
}
namespace ClipboardAnalyzer.DomainModels;

public interface IClipboardData
{
    public string Format { get; }
    public object Data { get; }
    public bool IsDisabled { get; }
}
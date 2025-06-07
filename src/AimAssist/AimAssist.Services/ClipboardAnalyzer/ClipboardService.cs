using AimAssist.Services.ClipboardAnalyzer.DomainModels;

namespace AimAssist.Services.ClipboardAnalyzer;

public partial class ClipboardService
{
    public static IEnumerable<IClipboardData> Load()
    {
        var ImageTypes = new List<string> { "PNG" };
        var isVisibleList = new List<string> { "HTML Format", "Text" };
        var dataObject = System.Windows.Clipboard.GetDataObject();
        var formats = dataObject.GetFormats();

        foreach (var format in formats)
        {
            if (ImageTypes.Contains(format))
            {
                var image = System.Windows.Clipboard.GetImage();
                yield return new ClipboardImage(format, image);
                continue;
            }

            var isVisible = isVisibleList.Contains(format);
            object data;
            try
            {
                data = System.Windows.Clipboard.GetData(format);
            }
            catch
            {
                continue;
            }

            yield return new ClipboardData(format, data, isVisible);
        }
    }
}
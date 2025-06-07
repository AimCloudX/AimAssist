using AimAssist.Services.ClipboardAnalyzer.DomainModels;

namespace AimAssist.Services.ClipboardAnalyzer;

public class ClipboardService
{
    public static IEnumerable<IClipboardData> Load()
    {
        var imageTypes = new List<string> { "PNG" };
        var isVisibleList = new List<string> { "HTML Format", "Text" };
        var dataObject = System.Windows.Clipboard.GetDataObject();
        if (dataObject != null)
        {
            var formats = dataObject.GetFormats();

            foreach (var format in formats)
            {
                if (imageTypes.Contains(format))
                {
                    var image = System.Windows.Clipboard.GetImage();
                    if(image != null)
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
}
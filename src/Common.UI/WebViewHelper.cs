using System;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace Common.UI
{

    public class WebViewHelper
    {
        private CoreWebView2 _coreWebView2;

        public WebViewHelper(CoreWebView2 coreWebView2)
        {
            _coreWebView2 = coreWebView2;
        }

        public async Task<string> WaitForWebMessageAsync()
        {
            var tcs = new TaskCompletionSource<string>();

            void MessageReceivedHandler(object sender, CoreWebView2WebMessageReceivedEventArgs e)
            {
                try
                {
                    var message = e.WebMessageAsJson; // もしくは e.TryGetWebMessageAsString() などで取得
                    tcs.SetResult(message);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
                finally
                {
                    _coreWebView2.WebMessageReceived -= MessageReceivedHandler;
                }
            }

            _coreWebView2.WebMessageReceived += MessageReceivedHandler;

            return await tcs.Task;
        }
    }
}

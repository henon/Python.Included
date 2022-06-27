using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Python.Deployment
{
    public static class Downloader
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task Download(
            string downloadUrl,
            string outputFilePath,
            Action<float> progress = null,
            CancellationToken token = default)
        {
            using (FileStream fileStream = new FileStream(outputFilePath, FileMode.Create))
            {
                await httpClient.DownloadWithProgressAsync(downloadUrl, fileStream, progress, token);
            }
        }
    }

    public static class HttpClientExtension
    {
        public static async Task DownloadWithProgressAsync(
            this HttpClient client,
            string requestUri,
            Stream destination,
            Action<float> progress = null,
            CancellationToken cancellationToken = default)
        {
            using (var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead))
            {
                var contentLength = response.Content.Headers.ContentLength;

                using (var download = await response.Content.ReadAsStreamAsync())
                {
                    int bufferSize = 81920;

                    if (progress == null || !contentLength.HasValue)
                    {
                        await download.CopyToAsync(destination, bufferSize, cancellationToken);
                        return;
                    }

                    var buffer = new byte[bufferSize];
                    long totalBytesRead = 0;
                    int bytesRead;
                    while ((bytesRead = await download.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) != 0)
                    {
                        await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                        totalBytesRead += bytesRead;
                        var progressPercentage = ((float)totalBytesRead / contentLength.Value) * 100;
                        progress.Invoke(progressPercentage);
                    }
                }
            }
        }
    }
}

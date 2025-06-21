using NUnit.Framework;
using Python.Deployment;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Python.Tests
{
    /// <summary>
    /// These tests are not strictly unit tests. Because of the difficulties of unit testing
    /// with HttpClient in a static class, these tests rely on using the HttpClient class and actually 
    /// downloading the resource at <see cref="TestResources.DownloadValidUrl"/>. These tests will fail if 
    /// there are network errors or if that resource is otherwise no longer available.
    /// </summary>
    public class DownloaderTests
    {
        [Test]
        public void DownloaderCanCancel()
        {
            var outputFile = new TemporaryFile(TestResources.DownloadFilename);
            var cts = new CancellationTokenSource();

            var downloadTask = Downloader.Download(
                TestResources.DownloadValidUrl,
                outputFile,
                null,
                cts.Token);

            cts.Cancel();

            Assert.CatchAsync<OperationCanceledException>(async () => await Task.WhenAll(downloadTask));
            Assert.IsFalse(File.Exists(outputFile));

            outputFile.Dispose();
        }

        [Test]
        public async Task DownloaderDownloadsFile()
        {
            var outputFile = new TemporaryFile(TestResources.DownloadFilename);

            await Downloader.Download(TestResources.DownloadValidUrl, outputFile);

            Assert.IsTrue(File.Exists(outputFile));

            outputFile.Dispose();
        }

        [Test]
        public async Task DownloaderReportsProgress()
        {
            float percentProgress = 0;
            void OnPercentageProgess(float percentage) => percentProgress = percentage;

            await Downloader.Download(
                TestResources.DownloadValidUrl,
                TestResources.DownloadFilename,
                OnPercentageProgess);

            Assert.AreEqual(100, percentProgress);
        }

        [Test]
        public void DownloaderThrowsIfUnsuccessfulStatusCode()
        {
            var outputFile = new TemporaryFile(TestResources.DownloadFilename);

            var downloadTask = Downloader.Download(
                TestResources.DownloadNotFoundUrl,
                outputFile);

            //Using the message hack to determine unsuccessful status code since netstandard does not include
            //the status code in the HttpRequestException. If this project is updated to target .Net 5 or higher,
            //this can directly access the status code and determine that the status code was an unsuccessful one.
            var ex = Assert.CatchAsync<HttpRequestException>(async () => await Task.WhenAll(downloadTask));
            Assert.IsTrue(ex.Message.Contains("Response status code does not indicate success"));
            Assert.IsFalse(File.Exists(outputFile));

            outputFile.Dispose();
        }
    }
}

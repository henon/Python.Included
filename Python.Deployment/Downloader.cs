using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Python.Deployment
{
    public static class Downloader
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task Download(string downloadUrl, string outputFileName)
        {
            Stream responseStream = await httpClient.GetStreamAsync(downloadUrl);
            using (FileStream fileStream = new FileStream(outputFileName, FileMode.Create))
            {
                responseStream.CopyTo(fileStream);
            }
        }
    }
}

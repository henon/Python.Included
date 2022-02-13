using System;
using System.IO;
using System.Threading.Tasks;
using Python.Runtime;

namespace Python.Deployment.Download
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // ================================================
            // This example demonstrates how to download a Python distribution (v2.7.9) and install it locally 
            // ================================================

            // set the download source
            Python.Deployment.Installer.Source = new Deployment.Installer.DownloadInstallationSource()
            {
                DownloadUrl = @"https://www.python.org/ftp/python/3.7.3/python-3.7.3-embed-amd64.zip",
            };

            // install in local directory. if you don't set it will install in local app data of your user account
            Python.Deployment.Installer.InstallPath = Path.GetFullPath(".");

            // see what the installer is doing
            Python.Deployment.Installer.LogMessage += Console.WriteLine;

            // install from the given source
            await Python.Deployment.Installer.SetupPython();

            // ok, now use pythonnet from that installation
            PythonEngine.Initialize();

            // call Python's sys.version to prove we are executing the right version
            dynamic sys=Py.Import("sys");
            Console.WriteLine("### Python version:\n\t" + sys.version);

            // call os.getcwd() to prove we are executing the locally installed embedded python distribution
            dynamic os = Py.Import("os");
            Console.WriteLine("### Current working directory:\n\t" + os.getcwd());
            Console.WriteLine("### PythonPath:\n\t" + PythonEngine.PythonPath);

        }
    }
}

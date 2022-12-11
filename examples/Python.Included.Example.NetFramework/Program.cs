using System;
using System.IO;
using System.Threading.Tasks;
using Python.Runtime;

namespace Python.Included.Example.NetFramework
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // This example demonstrates how Python.Included is able to automatically install a minimal Python
            // environment which it includes as an embedded resource in its .NET assembly file
            // Python.Included is currently fixed to Python 3.7.3 amd64 for windows
            // If you need a different Python version or platform check out the Python.Installer examples!

            // install in local directory
            Installer.InstallPath = Path.GetFullPath(".");

            // see what the installer is doing
            Installer.LogMessage += Console.WriteLine;

            // install the embedded python distribution
            await Installer.SetupPython();

            // install pip3 for package installation
            await Installer.TryInstallPip();

            // download and install Spacy from the internet
            await Installer.PipInstallModule("spacy");

            // ok, now use pythonnet from that installation
            PythonEngine.Initialize();

            // call Python's sys.version to prove we are executing the right version
            dynamic sys = Py.Import("sys");
            Console.WriteLine("### Python version:\n\t" + sys.version);

            // call os.getcwd() to prove we are executing the locally installed embedded python distribution
            dynamic os = Py.Import("os");
            Console.WriteLine("### Current working directory:\n\t" + os.getcwd());
            Console.WriteLine("### PythonPath:\n\t" + PythonEngine.PythonPath);

            // call spacy
            dynamic spacy = Py.Import("spacy");
            Console.WriteLine("### Spacy version:\n\t" + spacy.__version__);

#if NETFRAMEWORK
            Console.WriteLine("Hit any key to exit.");
            Console.ReadKey();
#endif
        }
    }
}

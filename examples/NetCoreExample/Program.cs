using System;
using System.Threading.Tasks;
using Python.Included;
using Python.Runtime;

namespace NetCoreExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Installer.SetupPython();
            PythonEngine.Initialize();
            dynamic sys=PythonEngine.ImportModule("sys");
            Console.WriteLine("Python version: " + sys.version);
        }
    }
}

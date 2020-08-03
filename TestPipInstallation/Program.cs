using Python.Included;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TestPipInstallation
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // send installation logs to std out
            Installer.LogMessage += Console.WriteLine;
            try
            {
                await Installer.SetupPython();
                Installer.TryInstallPip();
                Installer.PipInstallModule("spacy");
                PythonEngine.Initialize();
                dynamic spacy = Py.Import("spacy");
                Console.WriteLine("Spacy version: " + spacy.__version__);
            }
            finally
            {
                Console.ReadKey();
            }
        }
    }
}

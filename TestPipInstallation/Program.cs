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
            await Installer.SetupPython();
            Installer.TryInstallPip();
            try
            {
                if (!Installer.IsPipInstalled())
                {
                    Console.WriteLine("Error installing Pip");
                    return;
                }
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

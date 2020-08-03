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
            bool isDone = Installer.TryInstallPip();

            if (isDone | Installer.IsPipInstalled())
            {
                PythonEngine.Initialize();
                Installer.PipInstallModule("spacy");
                dynamic spacy = Py.Import("spacy");
                Console.WriteLine("Spacy version: " + spacy.__version__);
            }
            else
            {
                Console.WriteLine("Error installing Pip");
            }

            Console.ReadKey();
        }
    }
}

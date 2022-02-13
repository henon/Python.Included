using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Python.Deployment;
using Python.Runtime;

namespace Python.Tests
{
    public class InstallationSourceTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task GetPythonVersion()
        {
            await Python.Included.Installer.SetupPython();
            //Runtime.Runtime.PythonDLL = "python310.dll";
            PythonEngine.Initialize();
            dynamic sys = Py.Import("sys");
            Console.WriteLine("Python version: " + sys.version);
        }
    }
}
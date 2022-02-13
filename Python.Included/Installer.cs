/*

Copyright (c) 2020 by Meinrad Recheis (meinrad.recheis@gmail.com)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

 */

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Python.Runtime;

namespace Python.Included
{
    public static class Installer
    {
        public const string PYTHON_VERSION = "python310";

        /// <summary>
        /// Path to install python. If needed, set before calling SetupPython().
        /// <para>Default is: Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)</para>
        /// </summary>
        public static string InstallPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        /// <summary>
        /// Name of the python directory. If needed, set before calling SetupPython().
        /// Defaults to python-3.7.3-embed-amd64
        /// </summary>
        public static string InstallDirectory { get; set; } = "python-3.10.0-embed-amd64";

        /// <summary>
        /// The full path to the Python directory. Customize this by setting InstallPath and InstallDirectory
        /// </summary>
        public static string EmbeddedPythonHome => Path.Combine(InstallPath, InstallDirectory);

        /// <summary>
        /// Subscribe to this event to get installation log messages 
        /// </summary>
        public static event Action<string> LogMessage;

        private static void Log(string message)
        {
            LogMessage?.Invoke(message);
        }

        public static async Task SetupPython(bool force = false)
        {
            if (!PythonEnv.DeployEmbeddedPython)
                return;
            Runtime.Runtime.PythonDLL = "python310.dll";

            try
            {
                Python.Deployment.Installer.LogMessage += Log;
                Python.Deployment.Installer.Source = GetInstallationSource();
                Python.Deployment.Installer.PythonDirectoryName = InstallDirectory;
                Python.Deployment.Installer.InstallPath = InstallPath;
                await Python.Deployment.Installer.SetupPython(force);
            }
            finally
            {
                Python.Deployment.Installer.LogMessage -= Log;
            }
        }

        private static Python.Deployment.Installer.InstallationSource GetInstallationSource()
        {
            return new Python.Deployment.Installer.EmbeddedResourceInstallationSource()
            {
                Assembly = typeof(PythonEnv).Assembly,
                ResourceName = "python-3.10.0-embed-amd64.zip",
            };
        }

        /// <summary>
        /// Install a python library (.whl file) in the embedded python installation of Python.Included
        ///
        /// Note: Installing python packages using a custom wheel may result in an invalid python environment if the packages don't match the python version.
        /// To be safe, use pip by calling Installer.PipInstallModule.
        /// </summary>
        /// <param name="assembly">The assembly containing the embedded wheel</param>
        /// <param name="resource_name">Name of the embedded wheel file i.e. "numpy-1.16.3-cp37-cp37m-win_amd64.whl"</param>
        /// <param name="force"></param>
        /// <returns></returns>
        public static async Task InstallWheel(Assembly assembly, string resource_name, bool force = false)
        {
            try
            {
                Python.Deployment.Installer.LogMessage += Log;
                Python.Deployment.Installer.Source = GetInstallationSource();
                Python.Deployment.Installer.PythonDirectoryName = InstallDirectory;
                Python.Deployment.Installer.InstallPath = InstallPath;
                await Python.Deployment.Installer.InstallWheel(assembly, resource_name, force);
            }
            finally
            {
                Python.Deployment.Installer.LogMessage -= Log;
            }
        }

        /// <summary>
        /// Uses the local python-embedded pip module to install a python library (.whl file) in the embedded python installation of Python.Included
        ///
        /// Note: Installing python packages using a custom wheel may result in an invalid python environment if the packages don't match the python version.
        /// To be safe, use pip by calling Installer.PipInstallModule.
        /// </summary>
        /// <param name="assembly">The assembly containing the embedded wheel</param>
        /// <param name="resource_name">Name of the embedded wheel file i.e. "numpy-1.16.3-cp37-cp37m-win_amd64.whl"</param>
        /// <param name="force"></param>
        /// <returns></returns>
        public static void PipInstallWheel(Assembly assembly, string resource_name, bool force = false)
        {
            try
            {
                Python.Deployment.Installer.LogMessage += Log;
                Python.Deployment.Installer.Source = GetInstallationSource();
                Python.Deployment.Installer.PythonDirectoryName = InstallDirectory;
                Python.Deployment.Installer.InstallPath = InstallPath;
                Python.Deployment.Installer.PipInstallWheel(assembly, resource_name, force);
            }
            finally
            {
                Python.Deployment.Installer.LogMessage -= Log;
            }
        }

        /// <summary>
        /// Uses pip to find and install the specified package.
        /// </summary>
        /// <param name="module_name">The module/package to install </param>
        /// <param name="force">When true, reinstall the packages even if it is already up-to-date.</param>
        public static void PipInstallModule(string module_name, string version = "", bool force = false)
        {
            try
            {
                Python.Deployment.Installer.LogMessage += Log;
                Python.Deployment.Installer.Source = GetInstallationSource();
                Python.Deployment.Installer.PythonDirectoryName = InstallDirectory;
                Python.Deployment.Installer.InstallPath = InstallPath;
                Python.Deployment.Installer.PipInstallModule(module_name, version, force);
            }
            finally
            {
                Python.Deployment.Installer.LogMessage -= Log;
            }
        }

        /// <summary>
        /// Download and install pip.
        /// </summary>
        /// <remarks>
        /// Creates the lib folder under <see cref="EmbeddedPythonHome"/> if it does not exist.
        /// </remarks>
        public static void InstallPip()
        {
            try
            {
                Python.Deployment.Installer.LogMessage += Log;
                Python.Deployment.Installer.Source = GetInstallationSource();
                Python.Deployment.Installer.PythonDirectoryName = InstallDirectory;
                Python.Deployment.Installer.InstallPath = InstallPath;
                Python.Deployment.Installer.InstallPip();
            }
            finally
            {
                Python.Deployment.Installer.LogMessage -= Log;
            }
        }

        public static bool TryInstallPip(bool force = false)
        {
            if (!IsPipInstalled() || force)
            {
                try
                {
                    InstallPip();
                    return true;
                }
                catch
                {
                    throw new FileNotFoundException("pip is not installed");
                }
            }
            return false;
        }

        public static bool IsPythonInstalled()
        {
            return File.Exists(Path.Combine(EmbeddedPythonHome, "python.exe"));
        }

        public static bool IsPipInstalled()
        {
            return File.Exists(Path.Combine(EmbeddedPythonHome, "Scripts", "pip.exe"));
        }

        public static bool IsModuleInstalled(string module)
        {
            if (!IsPythonInstalled())
                return false;
            string moduleDir = Path.Combine(EmbeddedPythonHome, "Lib", "site-packages", module);
            return Directory.Exists(moduleDir) && File.Exists(Path.Combine(moduleDir, "__init__.py"));
        }
    }
}

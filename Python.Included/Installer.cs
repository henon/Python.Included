using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Python.Runtime;

namespace Python.Included
{
    public static class Installer
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        public const string EMBEDDED_PYTHON = "python-3.7.3-embed-amd64";

        public const string PYTHON_VERSION = "python37";
        /// <summary>
        /// Path to install python. If needed set it before calling SetupPython().
        /// <para>Default is: Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)</para>
        /// </summary>
        public static string INSTALL_PATH { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static string EmbeddedPythonHome
        {
            get
            {
                var install_dir = Path.Combine(INSTALL_PATH, EMBEDDED_PYTHON);
                return install_dir;
            }
        }

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static async Task SetupPython(bool force = false)
        {
            if (!PythonEnv.DeployEmbeddedPython)
                return;
            if (Runtime.Runtime.pyversion != "3.7")
                throw new InvalidOperationException("You must compile Python.Runtime with PYTHON37 flag! Runtime version: " + Runtime.Runtime.pyversion);
            Environment.SetEnvironmentVariable("PATH", $"{EmbeddedPythonHome};" + Environment.GetEnvironmentVariable("PATH"));
            if (!force && Directory.Exists(EmbeddedPythonHome) && File.Exists(Path.Combine(EmbeddedPythonHome, "python.exe"))) // python seems installed, so exit
                return;
            await Task.Run(() =>
            {
                var assembly = typeof(Installer).Assembly;
                var zip = Path.Combine(INSTALL_PATH, $"{EMBEDDED_PYTHON}.zip");
                var resource_name = EMBEDDED_PYTHON;
                CopyEmbeddedResourceToFile(assembly, resource_name, zip, force);
                try
                {
                    ZipFile.ExtractToDirectory(zip, zip.Replace(".zip", ""));

                    // allow pip on embedded python installation
                    // see https://github.com/pypa/pip/issues/4207#issuecomment-281236055
                    var pth = Path.Combine(EmbeddedPythonHome, PYTHON_VERSION + "._pth");
                    string lines = File.ReadAllText(pth);
                    lines = lines.Replace("#import site", "import site");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error extracting zip file: " + zip);
                }
            });
        }

        /***************************************************/

        public static string GetResourceKey(Assembly assembly, string embedded_file)
        {
            return assembly.GetManifestResourceNames().FirstOrDefault(x => x.Contains(embedded_file));
        }

        /***************************************************/

        /// <summary>
        /// Install a python library (.whl file) in the embedded python installation of Python.Included
        /// </summary>
        /// <param name="assembly">The assembly containing the embedded wheel</param>
        /// <param name="resource_name">Name of the embedded wheel file i.e. "numpy-1.16.3-cp37-cp37m-win_amd64.whl"</param>
        /// <param name="force"></param>
        /// <returns></returns>
        public static async Task InstallWheel(Assembly assembly, string resource_name, bool force = false)
        {
            var key = GetResourceKey(assembly, resource_name);
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException($"The resource '{resource_name}' was not found in assembly '{assembly.FullName}'");

            var module_name = resource_name.Split('-').FirstOrDefault();
            if (string.IsNullOrWhiteSpace(module_name))
                throw new ArgumentException($"The resource name '{resource_name}' did not contain a valid module name");

            var lib = Path.Combine(EmbeddedPythonHome, "Lib");
            if (!Directory.Exists(lib))
                Directory.CreateDirectory(lib);

            var module_path = Path.Combine(lib, module_name);
            if (!force && Directory.Exists(module_path))
                return;

            var wheelPath = Path.Combine(lib, key);
            await Task.Run(() =>
            {
                CopyEmbeddedResourceToFile(assembly, key, wheelPath, force);
                try
                {
                    ZipFile.ExtractToDirectory(wheelPath, lib);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error extracting zip file: " + wheelPath);
                }

                // modify _pth file
                var pth = Path.Combine(EmbeddedPythonHome, PYTHON_VERSION + "._pth");
                if (!File.ReadAllLines(pth).Contains("./Lib"))
                    File.AppendAllLines(pth, new[] { "./Lib" });
            });
        }

        /// <summary>
        /// Uses the local python-embedded pip module to install a python library (.whl file) in the embedded python installation of Python.Included
        /// </summary>
        /// <param name="assembly">The assembly containing the embedded wheel</param>
        /// <param name="resource_name">Name of the embedded wheel file i.e. "numpy-1.16.3-cp37-cp37m-win_amd64.whl"</param>
        /// <param name="force"></param>
        /// <returns></returns>
        public static void PipInstallWheel(Assembly assembly, string resource_name, bool force = false)
        {
            string key = GetResourceKey(assembly, resource_name);
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException($"The resource '{resource_name}' was not found in assembly '{assembly.FullName}'");

            string module_name = resource_name.Split('-').FirstOrDefault();
            if (string.IsNullOrWhiteSpace(module_name))
                throw new ArgumentException($"The resource name '{resource_name}' did not contain a valid module name");

            string libDir = Path.Combine(EmbeddedPythonHome, "Lib");
            if (!Directory.Exists(libDir))
                Directory.CreateDirectory(libDir);

            string module_path = Path.Combine(libDir, module_name);
            if (!force && Directory.Exists(module_path))
                return;

            string wheelPath = Path.Combine(libDir, key);
            string pipPath = Path.Combine(EmbeddedPythonHome, "Scripts", "pip3");

            CopyEmbeddedResourceToFile(assembly, key, wheelPath, force);

            TryInstallPip();

            RunCommand($"{pipPath} install {wheelPath}");
        }

        /***************************************************/

        public static void PipInstallModule(string module_name, bool force = false)
        {
            TryInstallPip();

            if (IsModuleInstalled(module_name) && !force)
                return;

            string pipPath = Path.Combine(EmbeddedPythonHome, "Scripts", "pip3");
            string forceInstall = force ? " --force-reinstall" : "";
            RunCommand($"{pipPath} install {module_name}{forceInstall}");
        }

        /***************************************************/

        public static void InstallPip()
        {
            string libDir = Path.Combine(EmbeddedPythonHome, "Lib");
            RunCommand($"cd {libDir} && curl https://bootstrap.pypa.io/get-pip.py -o get-pip.py");
            RunCommand($"cd {EmbeddedPythonHome} && python.exe Lib\\get-pip.py");
        }

        /***************************************************/

        public static bool TryInstallPip()
        {
            if (!IsPipInstalled())
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

        /***************************************************/

        public static bool IsPythonInstalled()
        {
            return File.Exists(Path.Combine(EmbeddedPythonHome, "python.exe"));

        }

        /***************************************************/

        public static bool IsPipInstalled()
        {
            return File.Exists(Path.Combine(EmbeddedPythonHome, "Scripts", "pip.exe"));
        }

        /***************************************************/

        public static bool IsModuleInstalled(string module)
        {
            if (!IsPythonInstalled())
                return false;

            string moduleDir = Path.Combine(EmbeddedPythonHome, "Lib", "site-packages", module);
            return Directory.Exists(moduleDir) && File.Exists(Path.Combine(moduleDir, "__init__.py"));
        }

        /***************************************************/

        public static void RunCommand(string command, bool runInBackground = false)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            if (runInBackground)
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            string commandMode = runInBackground ? "/C" : "/K";
            commandMode = "";
            startInfo.WorkingDirectory = EmbeddedPythonHome;
            startInfo.Arguments = $"{commandMode} {command}";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static void CopyEmbeddedResourceToFile(Assembly assembly, string resourceName, string filePath, bool force = false)
        {
            if (force || !File.Exists(filePath))
            {
                var key = GetResourceKey(assembly, resourceName);
                using (Stream stream = assembly.GetManifestResourceStream(key))
                using (var file = new FileStream(filePath, FileMode.Create))
                {
                    if (stream == null)
                        throw new ArgumentException($"Resource name '{resourceName}' not found!");
                    stream.CopyTo(file);
                }
            }
        }

        /***************************************************/
    }
}

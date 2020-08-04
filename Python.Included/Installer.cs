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
        public const string PYTHON_VERSION = "python37";

        /// <summary>
        /// Path to install python. If needed, set before calling SetupPython().
        /// <para>Default is: Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)</para>
        /// </summary>
        public static string InstallPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        /// <summary>
        /// Name of the python directory. If needed, set before calling SetupPython().
        /// Defaults to python-3.7.3-embed-amd64
        /// </summary>
        public static string InstallDirectory { get; set; } = "python-3.7.3-embed-amd64";

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
            if (Runtime.Runtime.pyversion != "3.7")
            {
                Log("SetupPython: You must compile Python.Runtime with PYTHON37 flag! Runtime version: " + Runtime.Runtime.pyversion);
                throw new InvalidOperationException("You must compile Python.Runtime with PYTHON37 flag! Runtime version: " + Runtime.Runtime.pyversion);
            }
            Environment.SetEnvironmentVariable("PATH", $"{EmbeddedPythonHome};" + Environment.GetEnvironmentVariable("PATH"));
            if (!force && Directory.Exists(EmbeddedPythonHome) && File.Exists(Path.Combine(EmbeddedPythonHome, "python.exe"))) // python seems installed, so exit
                return;
            await Task.Run(() =>
            {
                var assembly = typeof(Installer).Assembly;
                var zip = Path.Combine(InstallPath, $"{InstallDirectory}.zip");
                var resource_name = InstallDirectory;
                CopyEmbeddedResourceToFile(assembly, resource_name, zip, force);
                try
                {
                    ZipFile.ExtractToDirectory(zip, zip.Replace(".zip", ""));

                    // allow pip on embedded python installation
                    // see https://github.com/pypa/pip/issues/4207#issuecomment-281236055
                    var pth = Path.Combine(EmbeddedPythonHome, PYTHON_VERSION + "._pth");
                    File.Delete(pth);
                }
                catch (Exception e)
                {
                    Log("SetupPython: Error extracting zip file: " + zip);
                }
            });
        }

        private static void CopyEmbeddedResourceToFile(Assembly assembly, string resourceName, string filePath, bool force = false)
        {
            if (force || !File.Exists(filePath))
            {
                var key = GetResourceKey(assembly, resourceName);
                using (Stream stream = assembly.GetManifestResourceStream(key))
                using (var file = new FileStream(filePath, FileMode.Create))
                {
                    if (stream == null)
                    {
                        Log($"CopyEmbeddedResourceToFile: Resource name '{resourceName}' not found!");
                        throw new ArgumentException($"Resource name '{resourceName}' not found!");
                    }
                    stream.CopyTo(file);
                }
            }
        }

        public static string GetResourceKey(Assembly assembly, string embedded_file)
        {
            return assembly.GetManifestResourceNames().FirstOrDefault(x => x.Contains(embedded_file));
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

        /// <summary>
        /// Uses pip to find and install the specified package.
        /// </summary>
        /// <param name="module_name">The module/package to install </param>
        /// <param name="force">When true, reinstall the packages even if it is already up-to-date.</param>
        /// <param name="runInBackground">
        /// Indicates that no command windows will be visible and the process will automatically
        /// terminate when complete. When true, the command window must be manually closed before
        /// processing will continue.
        /// </param>
        public static void PipInstallModule(string module_name, string version = "", bool force = false)
        {
            TryInstallPip();

            if (IsModuleInstalled(module_name) && !force)
                return;

            string pipPath = Path.Combine(EmbeddedPythonHome, "Scripts", "pip");
            string forceInstall = force ? " --force-reinstall" : "";
            if (version.Length > 0)
                version = $"=={version}";

            RunCommand($"{pipPath} install {module_name}{version} {forceInstall}");
        }

        /// <summary>
        /// Download and install pip.
        /// </summary>
        /// <remarks>
        /// Creates the lib folder under <see cref="EmbeddedPythonHome"/> if it does not exist.
        /// </remarks>
        /// <param name="runInBackground">
        /// Indicates that no command windows will be visible and the process will automatically
        /// terminate when complete. When true, the command window must be manually closed before
        /// processing will continue.
        /// </param>
        public static void InstallPip()
        {
            string libDir = Path.Combine(EmbeddedPythonHome, "Lib");

            if (!Directory.Exists(libDir))
                Directory.CreateDirectory(libDir);

            RunCommand($"cd {libDir} && curl https://bootstrap.pypa.io/get-pip.py -o get-pip.py");
            RunCommand($"cd {EmbeddedPythonHome} && python.exe Lib\\get-pip.py");
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

        /// <summary>
        /// Runs the specified command as a local system cmd processes.
        /// </summary>
        /// <param name="command">The arguments passed to cmd.</param>
        /// <param name="runInBackground">
        /// Indicates that no command windows will be visible and the process will automatically
        /// terminate when complete. When true, the command window must be manually closed before
        /// processing will continue.
        /// </param>
        public static void RunCommand(string command) =>
            RunCommand(command, CancellationToken.None).Wait();

        public static async Task RunCommand(string command, CancellationToken token)
        {
            Process process = new Process();
            try
            {
                string args = null;
                string filename = null;
                ProcessStartInfo startInfo = new ProcessStartInfo();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // Unix/Linux/macOS specific command execution
                    filename = "/bin/bash";
                    args = $"-c {command}";
                }
                else
                {
                    // Windows specific command execution
                    filename = "cmd.exe";
                    args = $"/C {command}";
                }
                Log($"> {filename} {args}");
                startInfo = new ProcessStartInfo
                {
                    FileName = filename,
                    WorkingDirectory = EmbeddedPythonHome,
                    Arguments = args,

                    // If the UseShellExecute property is true, the CreateNoWindow property value is ignored and a new window is created.
                    // .NET Core does not support creating windows directly on Unix/Linux/macOS and the property is ignored.

                    CreateNoWindow = true,
                    UseShellExecute = false, // necessary for stdout redirection
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                };
                process.StartInfo = startInfo;
                process.OutputDataReceived += (x, y) => Log(y.Data);
                process.ErrorDataReceived += (x, y) => Log(y.Data);
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                token.Register(() =>
                {
                    try
                    {
                        if (!process.HasExited)
                            process.Kill();
                    }
                    catch (Exception) { /* ignore */ }
                });
                await Task.Run(() => { process.WaitForExit(); }, token);
                if (process.ExitCode != 0)
                    Log(" => exit code " + process.ExitCode);
            }
            catch (Exception e)
            {
                Log($"RunCommand: Error with command: '{command}'\r\n{e.Message}");
            }
            finally
            {
                process?.Dispose();
            }
        }

    }
}

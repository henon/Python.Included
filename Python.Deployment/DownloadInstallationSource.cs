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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Python.Deployment
{
    public static partial class Installer
    {

        /// <summary>
        /// Installs Python from an embedded resource of a .NET assembly
        /// </summary>
        public class DownloadInstallationSource : InstallationSource
        {
            /// <summary>
            /// The location on the web where to download the python distribution, for instance https://www.python.org/ftp/python/3.7.3/python-3.7.3-embed-amd64.zip
            /// </summary>
            public string DownloadUrl { get; set; }

            public override async Task<string> RetrievePythonZip(string destinationDirectory)
            {
                var zipFile = Path.Combine(destinationDirectory, GetPythonZipFileName());
                if (!Force && File.Exists(zipFile))
                    return zipFile;
                await RunCommand($"curl {DownloadUrl} -o {zipFile}", CancellationToken.None);
                return zipFile;
            }

            public override string GetPythonZipFileName()
            {
                Uri uri = new Uri(DownloadUrl);
                return System.IO.Path.GetFileName(uri.LocalPath);
            }

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
                        WorkingDirectory = Directory.GetCurrentDirectory(),
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
}

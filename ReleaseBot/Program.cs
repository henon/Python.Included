using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using HtmlAgilityPack;

namespace ReleaseBot
{




    // Note: the Release bot is not working yet.  It needs to embed a different python-x.y.z-embed....zip for every version it builds.
    //      That must be implemented before it can work. 





    class Program
    {
        private const string V = "2"; // <--- lib version, to be appended after python version!
        public static string PythonNetVersion = "1";

        private const string ProjectPath = "../Python.Included";
        private const string ProjectName = "Python.Included.csproj";

        private const string Description = "Python.Included is an automatic deployment mechanism for .NET packages which depend on the embedded Python distribution. This allows libraries depending on Python and/or Python packages to be deployed via Nuget without having to worry about any local Python installations.";
        private const string Tags = "Python, pythonnet, embedded Python";

#error TODO: change the embedded python version dynamically!

        static void Main(string[] args)
        {
            var specs = new ReleaseSpec[]
            {
                //// linux                
                //new ReleaseSpec() { CPythonVersion = "2.7", Platform="Linux",   },
                //new ReleaseSpec() { CPythonVersion = "3.5", Platform="Linux",   },
                //new ReleaseSpec() { CPythonVersion = "3.6", Platform="Linux",   },
                //new ReleaseSpec() { CPythonVersion = "3.7", Platform="Linux",   },
                //// mac
                //new ReleaseSpec() { CPythonVersion = "2.7", Platform="OSX",  },
                //new ReleaseSpec() { CPythonVersion = "3.5", Platform="OSX",  },
                //new ReleaseSpec() { CPythonVersion = "3.6", Platform="OSX",  },
                //new ReleaseSpec() { CPythonVersion = "3.7", Platform="OSX",  },
                // win
                new ReleaseSpec() { CPythonVersion = "2.7", Platform="Win64",   },
                new ReleaseSpec() { CPythonVersion = "3.5", Platform="Win64",   },
                new ReleaseSpec() { CPythonVersion = "3.6", Platform="Win64",   },
                new ReleaseSpec() { CPythonVersion = "3.7", Platform="Win64",   },

            };
            foreach (var spec in specs)
            {
                spec.Version = $"{spec.CPythonVersion}.{V}";
                spec.PythonNetVersion = $"{spec.CPythonVersion}.{PythonNetVersion}";
                spec.Description = string.Format(Description, spec.Platform, spec.CPythonVersion);
                spec.PackageTags = Tags;
                spec.RelativeProjectPath = ProjectPath;
                spec.ProjectName = ProjectName;
                switch (spec.Platform)
                {
                    //case "Linux":
                    //    spec.PackageId = "Python.Included.Mono";
                    //    spec.PythonNet = "Python.Runtime.Mono";
                    //    break;
                    //case "OSX":
                    //    spec.PackageId = "Python.Included.OSX";
                    //    spec.PythonNet = "Python.Runtime.OSX";
                    //    break;
                    case "Win64":
                        spec.PackageId = "Python.Included";
                        spec.PythonNet = "Python.Runtime.NETStandard";
                        break;
                }
                spec.Process();
            }

            var key = File.ReadAllText("../../nuget.key").Trim();
            foreach (var nuget in Directory.GetFiles(Path.Combine(ProjectPath, "bin", "Release"), "*.nupkg"))
            {
                Console.WriteLine("Push " + nuget);
                var arg = $"push -Source https://api.nuget.org/v3/index.json -ApiKey {key} {nuget}";
                var p = new Process() { StartInfo = new ProcessStartInfo("nuget.exe", arg) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false } };
                p.OutputDataReceived += (x, data) => Console.WriteLine(data.Data);
                p.ErrorDataReceived += (x, data) => Console.WriteLine("Error: " + data.Data);
                p.Start();
                p.WaitForExit();
                Console.WriteLine("... pushed");
            }
            Thread.Sleep(3000);
        }
    }

    public class ReleaseSpec
    {
        /// <summary>
        /// The assembly / nuget package version
        /// </summary>
        public string Version;

        public string CPythonVersion;
        public string Platform;

        /// <summary>
        /// Project description
        /// </summary>
        public string Description;

        /// <summary>
        /// Project description
        /// </summary>
        public string PackageTags;

        /// <summary>
        /// Nuget package id
        /// </summary>
        public string PackageId;

        /// <summary>
        /// PythonNet package name
        /// </summary>
        public string PythonNet;

        /// <summary>
        /// PythonNet Version
        /// </summary>
        public string PythonNetVersion;

        /// <summary>
        /// Name of the csproj file
        /// </summary>
        public string ProjectName;

        /// <summary>
        /// Path to the csproj file, relative to the execution directory of ReleaseBot
        /// </summary>
        public string RelativeProjectPath;

        public string FullProjectPath => Path.Combine(RelativeProjectPath, ProjectName);

        public void Process()
        {
            if (!File.Exists(FullProjectPath))
                throw new InvalidOperationException("Project not found at: " + FullProjectPath);
            // modify csproj
            var doc = new HtmlDocument() { OptionOutputOriginalCase = true, OptionWriteEmptyNodes = true };
            doc.Load(FullProjectPath);
            var group0 = doc.DocumentNode.Descendants("propertygroup").FirstOrDefault();
            SetInnerText(group0.Element("version"), Version);
            Console.WriteLine("Version: " + group0.Element("version").InnerText);
            SetInnerText(group0.Element("description"), Description);
            Console.WriteLine("Description: " + group0.Element("description").InnerText);
            SetInnerText(group0.Element("packageid"), PackageId);
            var group1 = doc.DocumentNode.Descendants("itemgroup").FirstOrDefault(g => g.Element("packagereference") != null);
            var reference = group1.Descendants("packagereference").ToArray()[1];
            reference.Attributes["Include"].Value = PythonNet;
            reference.Attributes["Version"].Value = PythonNetVersion;
            doc.Save(FullProjectPath);
            // now build in release mode
            RestoreNugetDependencies();
            Build();
        }

        private void RestoreNugetDependencies()
        {
            Console.WriteLine("Fetch Nugets " + Description);
            var p = new Process()
            {
                StartInfo = new ProcessStartInfo("dotnet", "restore")
                { WorkingDirectory = Path.GetFullPath(RelativeProjectPath) }
            };
            p.Start();
            p.WaitForExit();
        }

        private void Build()
        {
            Console.WriteLine("Build " + Description);
            var p = new Process()
            {
                StartInfo = new ProcessStartInfo("dotnet", "build -c Release")
                { WorkingDirectory = Path.GetFullPath(RelativeProjectPath) }
            };
            p.Start();
            p.WaitForExit();
        }

        private void SetInnerText(HtmlNode node, string text)
        {
            node.ReplaceChild(HtmlTextNode.CreateNode(text), node.FirstChild);
        }
    }
}

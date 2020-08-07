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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Python.Deployment
{
    public static partial class Installer
    {

        /// <summary>
        /// Installs Python from an embedded resource of a .NET assembly
        /// </summary>
        public class EmbeddedResourceInstallationSource : InstallationSource
        {
            /// <summary>
            /// The .NET assembly that includes a python zip as embedded resource.
            /// Note: you can get that by using <code>typeof(AnyTypeInYourAssembly).Assembly</code>
            /// </summary>
            public Assembly Assembly { get; set; }

            /// <summary>
            /// The name of the zip file that has been included in the given assembly as embedded resource, i.e. "python-3.7.3-embed-amd64.zip". 
            /// </summary>
            public string ResourceName { get; set; }

            public override async Task<string> RetrievePythonZip(string destinationDirectory)
            {
                var filePath = Path.Combine(destinationDirectory, ResourceName);
                if (!Force && File.Exists(filePath))
                    return filePath;
                CopyEmbeddedResourceToFile(Assembly, GetPythonDistributionName(), filePath);
                return filePath;
            }

            public override string GetPythonZipFileName()
            {
                return Path.GetFileName( ResourceName);
            }

        }


    }
}

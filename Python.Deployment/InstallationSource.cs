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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Python.Deployment
{
    public static partial class Installer
    {
        /// <summary>
        /// 
        /// </summary>
        public abstract class InstallationSource
        {
            /// <summary>
            /// Retrieves (extracts or downloads etc) the python distribution zip file, saves it at the given destination path and returns the full path of the retrieved zip file.
            /// Note: if Force==false and the file already exists this will simply return the path to the zip file.
            /// </summary>
            /// <param name="destinationDirectory">The directory location where the retrieved zip file should be placed</param>
            /// <returns></returns>
            public abstract Task<string> RetrievePythonZip(string destinationDirectory);

            /// <summary>
            /// If true, retrieve the python file again even if it already exists at the destination path
            /// </summary>
            public bool Force { get; set; } = false;

            public virtual string GetPythonDistributionName()
            {
                var zip = GetPythonZipFileName();
                if (zip == null)
                    return null;
                return Path.GetFileNameWithoutExtension(zip);
            }

            public abstract string GetPythonZipFileName();

            public virtual string GetPythonVersion()
            {
                var dist = GetPythonDistributionName();
                var m=Regex.Match(dist, @"python-(?<major>\d)\.(?<minor>\d+)");
                if (!m.Success)
                {
                    Log("Unable to get python version from distribution name.");
                    return null;
                }
                return $"python{m.Groups["major"]}{m.Groups["minor"]}";
            }

        }


    }
}

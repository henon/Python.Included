using System;
using System.IO;
using System.Threading.Tasks;
using Python.Runtime;

namespace Python.Deployment.InstallWheel
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // ================================================
            // This example demonstrates how to install wheel files from the assembly's resources
            // into the embedded Python distribution (v3.7.3)
            // ================================================

            // set the installation source to be the embedded python zip from our resources
            Python.Deployment.Installer.Source = new Deployment.Installer.EmbeddedResourceInstallationSource()
            {
                Assembly = typeof(Program).Assembly,
                ResourceName = "python-3.7.3-embed-amd64.zip",
            };

            // install in local directory. if you don't set it will install in local app data of your user account
            Python.Deployment.Installer.InstallPath = Path.GetFullPath(".");

            // see what the installer is doing
            Python.Deployment.Installer.LogMessage += Console.WriteLine;


            // install from the given source
            await Python.Deployment.Installer.SetupPython(force:true);

            await Python.Deployment.Installer.InstallWheel(typeof(Program).Assembly,
                "numpy-1.16.3-cp37-cp37m-win_amd64.whl");

            // The following two wheels are picked because they have non-standard naming conventions.
            // Normally, "foobar.whl" will unzip to a folder "foobar". These two don't.
            await Python.Deployment.Installer.InstallWheel(typeof(Program).Assembly,
                "python_dateutil-2.8.1-py2.py3-none-any.whl"); // Unzips to "dateutils"
            await Python.Deployment.Installer.InstallWheel(typeof(Program).Assembly,
                "six-1.15.0-py2.py3-none-any.whl"); // Unzips to root of Libs

            // Install wheel present in local file system
            await Python.Deployment.Installer.InstallWheel(@".\pytz-2020.1-py2.py3-none-any.whl");

            Runtime.Runtime.PythonDLL = "python37.dll"; // set the python dll to use, this is the one we just installed
            // ok, now use pythonnet from that installation
            PythonEngine.Initialize();

            // call Python's sys.version to prove we are executing the right version
            dynamic sys=Py.Import("sys");
            Console.WriteLine("### Python version:\n\t" + sys.version);

            // call os.getcwd() to prove we are executing the locally installed embedded python distribution
            dynamic os = Py.Import("os");
            Console.WriteLine("### Current working directory:\n\t" + os.getcwd());
            Console.WriteLine("### PythonPath:\n\t" + PythonEngine.PythonPath);

            PythonEngine.Exec(@"
import sys
import math
import numpy as np

print ('Hello world!')
print ('version:' + sys.version)

np.arange(1) # check if numpy is properly loaded

a1 = np.arange(60000).reshape(300, 200)
a2 = np.arange(80000).reshape(200, 400)
result = np.matmul(a1, a2)

print('result: ' + str(result))

# do some dateutils stuff
from dateutil.relativedelta import *
from dateutil.easter import *
from dateutil.rrule import *
from dateutil.parser import *
from datetime import *
now = parse('Sat Oct 11 17:13:46 UTC 2003')
today = now.date()
year = rrule(YEARLY, dtstart = now, bymonth = 8, bymonthday = 13, byweekday = FR)[0].year
rdelta = relativedelta(easter(year), today)
print('Today is: %s (no, it isn\'t)' % today)
print('Year with next Aug 13th on a Friday is: %s' % year)
print('How far is the Easter of that year: %s' % rdelta)
print('And the Easter of that year is: %s' % (today + rdelta))

# do some pytz stuff
import pytz
print ('UTC time zone: %s' % pytz.utc);

from pytz import timezone
print ('Eastern time zone: %s' % timezone('US/Eastern'));
");
        }
    }
}

![logo](art/python_included_nuget.png)

# Python.Included

Python.Included is an automatic deployment mechanism for .NET packages which depend on the embedded Python distribution. This allows  libraries depending on Python and/or Python packages to be deployed via Nuget without having to worry about any local Python installations. 

It packages embedded Python (python-3.7.3-embed-amd64.zip) in its .NET assembly and automatically deploys it in the user's home directory upon first execution. On subsequent runs, it will find Python already deployed and therefor doesn't install it again. It also features a Python package (\*.whl) installer which Numpy.NET uses to install the NumPy wheel into the embedded Python installation. Last but not least, for packages that are too big to package into .NET nugets, there is support for installing those with `pip`.

Note: Python.Included only implements deployment on top of [pythonnet_netstandard](https://github.com/henon/pythonnet_netstandard). If you do not need (or want) the automatic deployment of embedded Python you should use that.

### Getting Started

Just reference Python.Included via NuGet, call `await Installer.SetupPython();` and you are ready to interop with [Python.NET](http://pythonnet.github.io/). It is completely irrelevant wether or not you have any local Python installations of any kind.

This short example initializes Python.Included and prints the version of the included Python installation by calling Python's `sys.version`:
```c#
using System;
using System.Threading.Tasks;
using Python.Included;
using Python.Runtime;

namespace NetCoreExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Installer.SetupPython();
            PythonEngine.Initialize();
            dynamic sys=PythonEngine.ImportModule("sys");
            Console.WriteLine("Python version: " + sys.version);
        }
    }
}
```

Output:
```
Python version: 3.7.3 (v3.7.3:ef4ec6ed12, Mar 25 2019, 22:22:05) [MSC v.1916 64 bit (AMD64)]
```

### Installing Python libraries (*.whl files) from embedded resources
An example of this usage is [Numpy.NET](https://github.com/SciSharp/Numpy.NET) which in `np.module.gen.cs` installs the numpy binary which it includes as embedded resource:

```c#
await Installer.SetupPython(force)
await Installer.InstallWheel(typeof(np).Assembly, "numpy-1.16.3-cp37-cp37m-win_amd64.whl")
PythonEngine.Initialize();
var numpy = Py.Import("numpy");
```

`Installer.InstallWheel(Assembly assembly, string resource_name)` loads the wheel from the given assembly and installs it into the embedded python installation. 

NOTE: Make sure the wheel file is compatible to the version of Python which Python.Included installs (v3.7.3 x64). Also, while it works fine with numpy, not every python package can easily be embedded due to its size. Some might require extra installation routines which are not supported by `InstallWheel`. In that case, use pip to download and install such packages as described below.

### Installing Python libraries with pip

You can install a current version of `pip3` and use that to install any python package:

```c#
await Installer.SetupPython();
Installer.TryInstallPip();
Installer.PipInstallModule("spacy");
PythonEngine.Initialize();
dynamic spacy = Py.Import("spacy");
Console.WriteLine("Spacy version: " + spacy.__version__);
```

Check out the example project `TestPipInstallation` to try it out.

Downloading PIP requires [`curl`](https://curl.se/). Please make sure it is installed and available on systen PATH.

### Project Status

The code is stable for Windows. For testing on other platforms help is wanted.

### Projects using Python.Included
* [Numpy.NET](https://github.com/SciSharp/Numpy.NET)
* [BHoM Python Toolkit](https://github.com/BHoM/Python_Toolkit) is a modified version of Python.Included

### License
Since Python.Included distributes Python, it is licensed under the [Python Software Foundation License (PSF)](https://docs.python.org/3/license.html) like Python itself. 

### Links
* [Project Announcement](https://henon.wordpress.com/2019/06/05/using-python-libraries-in-net-without-a-python-installation/)


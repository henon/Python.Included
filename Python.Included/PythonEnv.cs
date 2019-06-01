using System;
using System.Collections.Generic;
using System.Text;

namespace Python.Included
{
    public static class PythonEnv
    {
        /// <summary>
        /// Set PythonEnv.DeployEmbeddedPython to false to use system Python installation
        /// </summary>
        public static bool DeployEmbeddedPython { get; set; } = true;
    }
}

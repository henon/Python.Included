using System;
using NUnit.Framework;
using Python.Deployment;
using Python.Runtime;

namespace Python.Tests
{
    public class PythonIncludedTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GetPythonDistributionProperties()
        {
            Python.Deployment.Installer.InstallationSource src = new Python.Deployment.Installer.EmbeddedResourceInstallationSource()
            {
                Assembly = typeof(InstallationSourceTests).Assembly,
                ResourceName = "python-3.7.3-embed-amd64.zip",
            };
            Assert.AreEqual("python-3.7.3-embed-amd64.zip", src.GetPythonZipFileName());
            Assert.AreEqual("python-3.7.3-embed-amd64", src.GetPythonDistributionName());
            Assert.AreEqual("python37", src.GetPythonVersion());
            src = new Python.Deployment.Installer.DownloadInstallationSource()
            {
                DownloadUrl = @"https://www.python.org/ftp/python/3.7.3/python-3.7.3-embed-amd64.zip",
            };
            Assert.AreEqual("python-3.7.3-embed-amd64.zip", src.GetPythonZipFileName());
            Assert.AreEqual("python-3.7.3-embed-amd64", src.GetPythonDistributionName());
            Assert.AreEqual("python37", src.GetPythonVersion());
        }
    }
}
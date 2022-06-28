using System;
using System.IO;

namespace Python.Tests
{
    public class TemporaryFile : IDisposable
    {
        private readonly string path;

        public TemporaryFile(string filename)
        {
            path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}-{filename}");
        }

        public static implicit operator string(TemporaryFile temporaryFile) => temporaryFile.path;

        public void Dispose()
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}

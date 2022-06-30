using System;
using System.IO;

namespace AxeptiaExporter.ConsoleApp
{
    public class Lockfile
    {
        const string lockFileName = "program.lock";
        private string _lockFileFullPath;
        private readonly string _programDirectory;


        public Lockfile(string programDirectory)
        {
            _programDirectory = programDirectory;
            _lockFileFullPath = $"{_programDirectory}/{lockFileName}";
        }

        public void CreateLockFile()
        {
            if (!File.Exists(_lockFileFullPath))
            {
                using var stream = File.Create(_lockFileFullPath);
            }
        }

        public bool DoLockfileExists()
        {
            return File.Exists(_lockFileFullPath);
        }

        public void RemoveLockFile()
        {
            if (File.Exists(_lockFileFullPath))
            {
                File.Delete(_lockFileFullPath);
            }
        }

        public bool WasLockFileCreatedBefore(DateTime dateTime)
        {
            if (File.Exists(_lockFileFullPath))
            {
                var lockFileCreated = File.GetCreationTimeUtc(_lockFileFullPath);
                return lockFileCreated < dateTime;
            }
            else
            {
                return false;
            }
        }

    }
}

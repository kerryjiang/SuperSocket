using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace SuperSocket.SocketEngine
{
    class ProcessLocker
    {
        private string m_LockFilePath;

        public ProcessLocker(string workDir, string lockFileName)
        {
            m_LockFilePath = Path.Combine(workDir, lockFileName);
        }

        public Process GetLockedProcess()
        {
            if (!File.Exists(m_LockFilePath))
                return null;

            int processId;

            if (!int.TryParse(File.ReadAllText(m_LockFilePath), out processId))
            {
                File.Delete(m_LockFilePath);
                return null;
            }

            try
            {
                return Process.GetProcessById(processId);
            }
            catch
            {
                File.Delete(m_LockFilePath);
                return null;
            }
        }

        public void SaveLock(Process process)
        {
            File.WriteAllText(m_LockFilePath, process.Id.ToString());
        }

        public void CleanLock()
        {
            if (File.Exists(m_LockFilePath))
                File.Delete(m_LockFilePath);
        }

        ~ProcessLocker()
        {
            CleanLock();
        }
    }
}

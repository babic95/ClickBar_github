using ClickBar_Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public class ProcessManager
{
    private Process? _process;
    private string _processName;
    private string _processPath;
    public ProcessManager(string processName, string processPath)
    {
        _processName = processName;
        _processPath = processPath;
    }

    public bool StartProcess(string arguments = "", string workingDirectory = "")
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = _processPath,
                Arguments = arguments,
                UseShellExecute = true,
                CreateNoWindow = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas",
                WorkingDirectory = workingDirectory
            };

            _process = Process.Start(startInfo);

            if(_process == null)
            {
                Log.Error("ProcessManager -> StartProcess -> Proces nije uspešno pokrenut: " + _processName);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.Error("ProcessManager -> StartProcess -> Greška prilikom pokretanja procesa: " + _processName, ex);
            return false;
        }
    }

    public void KillProcess()
    {
        if (_process != null)
        {
            if (!_process.HasExited)
            {
                _process.Kill();
            }
        }
    }

    public bool IsProcessRunning()
    {
        if (_process == null)
        {
            var existingProcesses = Process.GetProcessesByName(_processName);

            foreach (var process in existingProcesses)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        _process = process;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("ProcessManager -> IsProcessRunning -> Greška prilikom provere da li je proces pokrenut: " + _processName, ex);
                    _process = process;
                    return true;
                }
            }
        }
        else
        {
            return !_process.HasExited;
        }
        return false;
    }
}
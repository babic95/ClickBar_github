using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace ClickBar_SqlServerInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Do you want to install SQL Server? (y/n)");
            string input = Console.ReadLine();
            if (input.ToLower() != "y")
            {
                Console.WriteLine("SQL Server installation aborted.");
                return;
            }

            try
            {
                string currentUser = Environment.UserDomainName + "\\" + Environment.UserName;
                if (!GrantUserRights(currentUser))
                {
                    Console.WriteLine("Failed to grant user rights.");
                    return;
                }

                if (!IsSqlServerInstalled())
                {
                    Console.WriteLine("SQL Server is not installed. Starting installation...");
                    string installationPath = @"C:\CCS SQLServer2022";
                    CreateRequiredDirectories(installationPath);
                    if (!InstallSqlServer(installationPath))
                    {
                        Console.WriteLine("SQL Server installation failed.");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("SQL Server is already installed.");
                }

                ConfigureSqlServerForRemoteAccess();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void CreateRequiredDirectories(string baseDir)
        {
            Directory.CreateDirectory(baseDir);
            Directory.CreateDirectory(Path.Combine(baseDir, "UserDatabases"));
            Directory.CreateDirectory(Path.Combine(baseDir, "UserDatabases", "Logs"));
            Directory.CreateDirectory(Path.Combine(baseDir, "TempDB"));
            Directory.CreateDirectory(Path.Combine(baseDir, "TempDB", "Logs"));
        }

        private static bool GrantUserRights(string account)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string scriptPath = Path.Combine(baseDirectory, "GrantRights.ps1");

            if (!File.Exists(scriptPath))
            {
                Console.WriteLine("PowerShell script file not found: " + scriptPath);
                return false;
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" -account \"{account}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    Verb = "runas" // Run as administrator
                }
            };

            try
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine("User rights granted successfully.");
                    Console.WriteLine(output);
                    return true;
                }
                else
                {
                    Console.WriteLine("Failed to grant user rights with exit code: " + process.ExitCode);
                    Console.WriteLine(error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to grant user rights: {ex.Message}");
                return false;
            }
        }

        private static bool IsSqlServerInstalled()
        {
            const string sqlServerRegKey = @"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(sqlServerRegKey))
            {
                if (key != null)
                {
                    Console.WriteLine("SQL Server registry key found.");
                    return true;
                }
                else
                {
                    Console.WriteLine("SQL Server registry key not found.");
                    return false;
                }
            }
        }

        private static bool InstallSqlServer(string installationPath)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string setupPath = Path.Combine(baseDirectory, "SQLEXPRADV_x64_ENU.exe");
            string configFilePath = Path.Combine(baseDirectory, "ConfigurationFile.ini");

            if (!File.Exists(setupPath))
            {
                Console.WriteLine("SQL Server setup file not found: " + setupPath);
                return false;
            }

            if (!File.Exists(configFilePath))
            {
                Console.WriteLine("Configuration file not found: " + configFilePath);
                return false;
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = setupPath,
                    Arguments = $"/ConfigurationFile=\"{configFilePath}\" /QUIETSIMPLE",
                    UseShellExecute = true,
                    Verb = "runas" // Run as administrator
                }
            };

            try
            {
                process.Start();
                process.WaitForExit();
                if (process.ExitCode == 0)
                {
                    Console.WriteLine("SQL Server installation completed successfully.");
                    return true;
                }
                else
                {
                    Console.WriteLine("SQL Server installation failed with exit code: " + process.ExitCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SQL Server installation failed: {ex.Message}");
                return false;
            }
        }

        private static void ConfigureSqlServerForRemoteAccess()
        {
            // Enabling TCP/IP protocol and setting port to 1433
            const string sqlServerRegKey = @"SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQLServer\SuperSocketNetLib\Tcp\IPAll";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(sqlServerRegKey, true))
            {
                if (key != null)
                {
                    key.SetValue("TcpPort", "1433");
                    key.SetValue("Enabled", 1);
                    Console.WriteLine("TCP/IP protocol enabled and port set to 1433.");
                }
                else
                {
                    Console.WriteLine("Failed to open registry key for TCP/IP configuration.");
                }
            }

            // Restarting SQL Server service to apply changes
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "net",
                    Arguments = "stop MSSQLSERVER && net start MSSQLSERVER",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            try
            {
                process.Start();
                process.WaitForExit();
                Console.WriteLine("SQL Server service restarted to apply changes.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to restart SQL Server service: {ex.Message}");
            }
        }
    }
}
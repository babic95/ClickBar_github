using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;

namespace SqlServerInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!IsAdministrator())
            {
                Console.WriteLine("Ovaj program mora biti pokrenut kao administrator.");
                return;
            }

            try
            {
                if (IsSqlServerInstalled())
                {
                    Console.WriteLine("SQL Server je već instaliran. Instalacija je preskočena.");
                }
                else
                {
                    Console.WriteLine("Instalacija SQL Servera je u toku...");
                    InstallSqlServer();
                    Console.WriteLine("Instalacija SQL Servera je završena.");
                }

                Console.WriteLine("Pritisnite Enter za kraj.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Došlo je do greške: {ex.Message}");
                Console.WriteLine("Pritisnite Enter za kraj.");
                Console.ReadLine();
            }
        }

        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static bool IsSqlServerInstalled()
        {
            string instanceName = "MSSQL$CLICKBAR_SSMS";
            Process process = new Process();
            process.StartInfo.FileName = "net";
            process.StartInfo.Arguments = "start";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Contains(instanceName);
        }

        private static void InstallSqlServer()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string sqlSetupPath = Path.Combine(baseDirectory, "SQLEXPRADV_x64_ENU.exe");
            string configFilePath = Path.Combine(baseDirectory, "ConfigurationFile.ini");

            if (!File.Exists(sqlSetupPath))
            {
                throw new FileNotFoundException("SQL Server setup file not found.", sqlSetupPath);
            }

            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException("Configuration file not found.", configFilePath);
            }

            Process process = new Process();
            process.StartInfo.FileName = sqlSetupPath;
            process.StartInfo.Arguments = $"/ConfigurationFile=\"{configFilePath}\"";
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.Verb = "runas"; // Pokreni kao administrator
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"SQL Server setup failed with exit code: {process.ExitCode}");
            }
        }
    }
}
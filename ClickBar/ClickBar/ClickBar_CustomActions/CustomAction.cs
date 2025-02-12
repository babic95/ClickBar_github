using System;
using System.Diagnostics;
using System.IO;

namespace ClickBar_CustomActions
{
    public class CustomAction
    {
        public static void RunSQLInstaller(string targetDir)
        {
            string sqlExpressInstallerPath = Path.Combine(targetDir, "SQLEXPRADV_x64_ENU.exe");
            string configIniPath = Path.Combine(targetDir, "ConfigurationFile.ini");
            string arguments = $"/ConfigurationFile={configIniPath} /IACCEPTSQLSERVERLICENSETERMS";

            if (File.Exists(sqlExpressInstallerPath))
            {
                Process.Start(sqlExpressInstallerPath, arguments);
            }
            else
            {
                throw new FileNotFoundException("SQL Server Express installer not found.");
            }
        }
    }
}
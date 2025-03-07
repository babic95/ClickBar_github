using ClickBar_Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Settings
{
    public class SettingsFile
    {
        public string UrlToLPFR { get; set; }
        public string EsirId { get; set; }
        public string ActivationCode { get; set; }
        public bool EnableCCS_Server { get; set; }
        public string UrlToCCS_Server { get; set; }
        public DateTime? ValidTo { get; set; }
        public string InDirectory { get; set; }
        public string OutDirectory { get; set; }
        public string PrinterName { get; set; }
        public PrinterFormatEnumeration PrinterFormat { get; set; }
        public bool EnableFileSystemWatcher { get; set; }
        public bool EnableTableOverview { get; set; }
        public bool EnableSmartCard { get; set; }
        public bool CancelOrderFromTable { get; set; }
        public bool EnableSuperGroup { get; set; }
        public int NumberCopy { get; set; }
        public string? PrinterNameKuhinja { get; set; }
        public string? PrinterNameSank1 { get; set; }
        public string? CopPort { get; set; }
        public string? EfakturaDirectory { get; set; }
        public string? PathToDrljaKuhinjaDB { get; set; }
        public string? PathToMainDB { get; set; }
        public string? HostPC_IP { get; set; }
        public bool RunPorudzbineServis { get; set; }
        public TypeAppEnumeration TypeApp { get; set; }
    }
}

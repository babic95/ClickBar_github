using ClickBar_Logging;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Invoice.Verification
{
    public class VerificationQRcode
    {
        /// <summary>
        /// in millimeter
        /// </summary>
        private readonly int _minimalSize = 40;
        private readonly int _fixedModuleSize = 4;
        private string _verificationURL;

        public VerificationQRcode(string verificationURL)
        {
            _verificationURL = verificationURL;
        }

        public string GetQRcode()
        {
            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(_verificationURL, QRCodeGenerator.ECCLevel.L);
                Base64QRCode qrCode = new Base64QRCode(qrCodeData);

                string qrCodeString = qrCode.GetGraphic(_fixedModuleSize, Color.Black, Color.White, false, Base64QRCode.ImageType.Gif);

                return qrCodeString;
            }
            catch (Exception ex)
            {

                Log.Error(string.Format("VerificationQRcode - GetQRcode - "), ex);
                return null;
            }
        }
    }
}

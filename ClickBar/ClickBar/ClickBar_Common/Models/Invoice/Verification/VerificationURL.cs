using ClickBar_Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Invoice.Verification
{
    public class VerificationURL
    {
        public int Version { get; set; }
        public string RequestedBy { get; set; }
        public string SignedBy { get; set; }
        public int? TotalCounter { get; set; }
        public int? TransactionTypeCounter { get; set; }
        public decimal? TotalAmount { get; set; }
        public DateTime DateAndTime { get; set; }
        public InvoiceTypeEenumeration InvoiceType { get; set; }
        public TransactionTypeEnumeration TransactionType { get; set; }
        public string BuyerId { get; set; }
        public byte[] EncryptedInternalData { get; set; }
        public byte[] Signature { get; set; }

        public string GetVerificationURL()
        {
            int buyerIdLength = string.IsNullOrEmpty(BuyerId) == true ? 20 : BuyerId.Length;
            int verificationURLforMD5Length = 44 + buyerIdLength + EncryptedInternalData.Length + 256;
            var verificationURLforMD5 = new byte[verificationURLforMD5Length];
            var verificationURL = new byte[verificationURLforMD5.Length + 16];

            var version = new byte[1] { Convert.ToByte(Version) };
            var requestedBy = Encoding.ASCII.GetBytes(RequestedBy);
            var signedBy = Encoding.ASCII.GetBytes(SignedBy);
            var totalCounter = BitConverter.GetBytes(TotalCounter.Value);
            var transactionTypeCounter = BitConverter.GetBytes(TransactionTypeCounter.Value);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(totalCounter);
                Array.Reverse(transactionTypeCounter);
            }
            var totalAmount = ConvertAmountToUInt64_8ByteLittleEndianArray(TotalAmount.Value);

            UInt64 unixTimestamp = (UInt64)(DateAndTime.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            var dateAndTime = ConvertUInt64To8ByteBigEndianArray(unixTimestamp);
            var invoiceType = new byte[1] { Convert.ToByte(InvoiceType) };
            var transactionType = new byte[1] { Convert.ToByte(TransactionType) };
            var buyerIDlength = new byte[1] { Convert.ToByte(buyerIdLength) };

            var buyerId = string.IsNullOrEmpty(BuyerId) == true ? new byte[20] : Encoding.ASCII.GetBytes(BuyerId);
            var encryptedInternalData = EncryptedInternalData;

            version.CopyTo(verificationURLforMD5, 0);
            requestedBy.CopyTo(verificationURLforMD5, 1);
            signedBy.CopyTo(verificationURLforMD5, 9);
            totalCounter.CopyTo(verificationURLforMD5, 17);
            transactionTypeCounter.CopyTo(verificationURLforMD5, 21);
            totalAmount.CopyTo(verificationURLforMD5, 25);
            dateAndTime.CopyTo(verificationURLforMD5, 33);
            invoiceType.CopyTo(verificationURLforMD5, 41);
            transactionType.CopyTo(verificationURLforMD5, 42);
            buyerIDlength.CopyTo(verificationURLforMD5, 43);
            buyerId.CopyTo(verificationURLforMD5, 44);

            int encryptedInternalDataStart = 44 + buyerIdLength;
            encryptedInternalData.CopyTo(verificationURLforMD5, encryptedInternalDataStart);
            Signature.CopyTo(verificationURLforMD5, encryptedInternalDataStart + encryptedInternalData.Length);

            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(verificationURLforMD5);

                verificationURLforMD5.CopyTo(verificationURL, 0);
                hashBytes.CopyTo(verificationURL, verificationURLforMD5.Length);
            }

            return Convert.ToBase64String(verificationURL);
        }
        private byte[] ConvertUInt64To8ByteBigEndianArray(UInt64 counter)
        {
            return BitConverter.GetBytes(counter).Reverse().ToArray();
        }
        private byte[] ConvertAmountToUInt64_8ByteLittleEndianArray(decimal amount)
        {
            var littleEndian = BitConverter.GetBytes((ulong)(HalfRoundUp4(amount * 10000))).Reverse();
            if (BitConverter.IsLittleEndian)
            {
                littleEndian = littleEndian.Reverse();
            }
            //littleEndian = littleEndian.Skip(1).Take(8);
            return littleEndian.ToArray();
        }
        private decimal HalfRoundUp4(decimal value)
        {
            return Math.Round(value, 4, MidpointRounding.AwayFromZero);
        }
    }
}

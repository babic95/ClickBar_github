using ClickBar_Common.Models.Invoice.Tax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_Common.Models.Invoice.Helpers
{
    public class JournalHelper
    {
        private static InvoiceResult _invoiceResult;

        public static string CrteateClosingAdvandeJournal(InvoiceRequest invoiceRequest,
            InvoiceResult invoiceResult,
            InvoiceResult advanceRefund)
        {
            _invoiceResult = invoiceResult;

            string journal = NormalSale(invoiceRequest, advanceRefund);

            return journal;
        }

        public static string CreateJournal(InvoiceRequest invoiceRequest, InvoiceResult invoiceResult)
        {
            _invoiceResult = invoiceResult;
            string journal = string.Empty;

            switch (invoiceRequest.InvoiceType)
            {
                case Enums.InvoiceTypeEenumeration.Normal:
                    if (invoiceRequest.TransactionType == Enums.TransactionTypeEnumeration.Sale)
                    {
                        journal = NormalSale(invoiceRequest);
                    }
                    else if (invoiceRequest.TransactionType == Enums.TransactionTypeEnumeration.Refund)
                    {
                        journal = NormalRefund(invoiceRequest);
                    }
                    break;
                case Enums.InvoiceTypeEenumeration.Training:
                    if (invoiceRequest.TransactionType == Enums.TransactionTypeEnumeration.Sale)
                    {
                        journal = TrainingSale(invoiceRequest);
                    }
                    else if (invoiceRequest.TransactionType == Enums.TransactionTypeEnumeration.Refund)
                    {
                        journal = TrainingRefund(invoiceRequest);
                    }
                    break;
                case Enums.InvoiceTypeEenumeration.Copy:
                    if (invoiceRequest.TransactionType == Enums.TransactionTypeEnumeration.Sale)
                    {
                        journal = CopySale(invoiceRequest);
                    }
                    else if (invoiceRequest.TransactionType == Enums.TransactionTypeEnumeration.Refund)
                    {
                        journal = CopyRefund(invoiceRequest);
                    }
                    break;
                case Enums.InvoiceTypeEenumeration.Proforma:
                    if (invoiceRequest.TransactionType == Enums.TransactionTypeEnumeration.Sale)
                    {
                        journal = ProformaSale(invoiceRequest);
                    }
                    else if (invoiceRequest.TransactionType == Enums.TransactionTypeEnumeration.Refund)
                    {
                        journal = ProformaRefund(invoiceRequest);
                    }
                    break;
                case Enums.InvoiceTypeEenumeration.Advance:
                    if (invoiceRequest.TransactionType == Enums.TransactionTypeEnumeration.Sale)
                    {
                        journal = AdvanceSale(invoiceRequest);
                    }
                    else if (invoiceRequest.TransactionType == Enums.TransactionTypeEnumeration.Refund)
                    {
                        journal = AdvanceRefund(invoiceRequest);
                    }
                    break;
            }

            return journal;
        }

        private static string NormalSale(InvoiceRequest invoiceRequest, InvoiceResult advanceRefund = null)
        {
            string normalSale = "============ ФИСКАЛНИ РАЧУН ============\r\n";

            normalSale += GetTaxpayerInformationForJournal();
            normalSale += GetPOSinformationForJournal(invoiceRequest);

            normalSale += "-------------ПРОМЕТ ПРОДАЈА-------------\r\n";

            normalSale += GetItemsForJournal(invoiceRequest);

            normalSale += "----------------------------------------\r\n";
            if (_invoiceResult.TotalAmount.HasValue)
            {
                if (advanceRefund is not null)
                {
                    normalSale += string.Format("Укупан износ:{0}\r\n", _invoiceResult.TotalAmount.Value.ToString("0.00").Replace('.', ',').PadLeft(40 - "Укупан износ:".Length));

                    normalSale += string.Format("Плаћено авансом:{0}\r\n", advanceRefund.TotalAmount.Value.ToString("0.00").Replace('.', ',').PadLeft(40 - "Плаћено авансом:".Length));

                    decimal avansTotalTax = CalculateTotalTax(advanceRefund);
                    normalSale += string.Format("ПДВ на аванс:{0}\r\n", avansTotalTax.ToString("0.00").Replace('.', ',').PadLeft(40 - "ПДВ на аванс:".Length));
                }
                else
                {
                    normalSale += string.Format("За уплату:{0}\r\n", _invoiceResult.TotalAmount.Value.ToString("0.00").Replace('.', ',').PadLeft(40 - "За уплату:".Length));
                }
            }

            decimal totalPayment;
            normalSale += GetPaymentForJournal(invoiceRequest, out totalPayment);

            if (advanceRefund is not null)
            {
                normalSale += string.Format("Преостало за плаћанје:{0}\r\n", "0,00".PadLeft(40 - "Преостало за плаћанје:".Length));
            }

            if (_invoiceResult.TotalAmount.HasValue)
            {
                decimal change = totalPayment - _invoiceResult.TotalAmount.Value;
                if (change > 0)
                {
                    normalSale += string.Format("Повраћај:{0}\r\n", change.ToString("0.00").Replace('.', ',').PadLeft(40 - "Повраћај:".Length));
                }
            }

            normalSale += "========================================\r\n";

            normalSale += GetLabelsForJournal();

            normalSale += GetSDCinformationForJournal();

            normalSale += "======== КРАЈ ФИСКАЛНОГ РАЧУНА =========\r\n";
            if (advanceRefund is not null)
            {
                normalSale += SplitInParts(advanceRefund.AdvanceAddition, "Последњи авансни рачун:", 40);//  string.Format("Последњи авансни рачун:{0}\r\n", advanceRefund.AdvanceAddition.PadLeft(40 - "Последњи авансни рачун:".Length));sdfsdf
            }

            return normalSale;
        }
        private static string NormalRefund(InvoiceRequest invoiceRequest)
        {
            string normalRefund = string.Empty;
            normalRefund = "============ ФИСКАЛНИ РАЧУН ============\r\n";

            normalRefund += GetTaxpayerInformationForJournal();
            normalRefund += GetPOSinformationForJournal(invoiceRequest);

            normalRefund += "-----------ПРОМЕТ РЕФУНДАЦИЈА-----------\r\n";

            normalRefund += GetItemsForJournal(invoiceRequest);

            normalRefund += "----------------------------------------\r\n";
            if (_invoiceResult.TotalAmount.HasValue)
            {
                normalRefund += string.Format("За уплату:{0}\r\n", _invoiceResult.TotalAmount.Value.ToString("0.00").PadLeft(40 - "За уплату:".Length));
            }

            decimal totalPayment;
            normalRefund += GetPaymentForJournal(invoiceRequest, out totalPayment);
            normalRefund += "========================================\r\n";

            normalRefund += GetLabelsForJournal();

            normalRefund += GetSDCinformationForJournal();

            normalRefund += "======== КРАЈ ФИСКАЛНОГ РАЧУНА =========\r\n";

            return normalRefund;
        }
        private static string AdvanceSale(InvoiceRequest invoiceRequest)
        {
            string advanceSale = "============ ФИСКАЛНИ РАЧУН ============\r\n";

            advanceSale += GetTaxpayerInformationForJournal();
            advanceSale += GetPOSinformationForJournal(invoiceRequest);

            advanceSale += "-------------АВАНС ПРОДАЈА--------------\r\n";

            advanceSale += GetItemsForJournal(invoiceRequest);

            advanceSale += "----------------------------------------\r\n";
            if (_invoiceResult.TotalAmount.HasValue)
            {
                advanceSale += string.Format("За уплату:{0}\r\n", _invoiceResult.TotalAmount.Value.ToString("0.00").Replace('.', ',').PadLeft(40 - "За уплату:".Length));
            }

            decimal totalPayment;
            advanceSale += GetPaymentForJournal(invoiceRequest, out totalPayment);
            if (_invoiceResult.TotalAmount.HasValue)
            {
                decimal change = totalPayment - _invoiceResult.TotalAmount.Value;
                if (change > 0)
                {
                    advanceSale += string.Format("Повраћај:{0}\r\n", change.ToString("0.00").Replace('.', ',').PadLeft(40 - "Повраћај:".Length));
                }
            }
            advanceSale += "========================================\r\n";

            advanceSale += GetLabelsForJournal();

            advanceSale += GetSDCinformationForJournal();

            advanceSale += "======== КРАЈ ФИСКАЛНОГ РАЧУНА =========\r\n";

            return advanceSale;
        }
        private static string AdvanceRefund(InvoiceRequest invoiceRequest)
        {
            string advanceRefund = "============ ФИСКАЛНИ РАЧУН ============\r\n";

            advanceRefund += GetTaxpayerInformationForJournal();
            advanceRefund += GetPOSinformationForJournal(invoiceRequest);

            advanceRefund += "-----------АВАНС РЕФУНДАЦИЈА------------\r\n";

            advanceRefund += GetItemsForJournal(invoiceRequest);

            advanceRefund += "----------------------------------------\r\n";
            if (_invoiceResult.TotalAmount.HasValue)
            {
                advanceRefund += string.Format("За уплату:{0}\r\n", _invoiceResult.TotalAmount.Value.ToString("0.00").PadLeft(40 - "За уплату:".Length));
            }

            decimal totalPayment;
            advanceRefund += GetPaymentForJournal(invoiceRequest, out totalPayment);
            advanceRefund += "========================================\r\n";

            advanceRefund += GetLabelsForJournal();

            advanceRefund += GetSDCinformationForJournal();

            advanceRefund += "======== КРАЈ ФИСКАЛНОГ РАЧУНА =========\r\n";

            return advanceRefund;
        }
        private static string CopySale(InvoiceRequest invoiceRequest)
        {
            string copySale = "======== ОВО НИЈЕ ФИСКАЛНИ РАЧУН =======\r\n";

            copySale += GetTaxpayerInformationForJournal();
            copySale += GetPOSinformationForJournal(invoiceRequest);

            copySale += "-------------КОПИЈА ПРОДАЈА-------------\r\n";

            copySale += GetItemsForJournal(invoiceRequest);

            copySale += "----------------------------------------\r\n";
            if (_invoiceResult.TotalAmount.HasValue)
            {
                copySale += string.Format("За уплату:{0}\r\n", _invoiceResult.TotalAmount.Value.ToString("0.00").Replace('.', ',').PadLeft(40 - "За уплату:".Length));
            }

            decimal totalPayment;
            copySale += GetPaymentForJournal(invoiceRequest, out totalPayment);
            if (_invoiceResult.TotalAmount.HasValue)
            {
                decimal change = totalPayment - _invoiceResult.TotalAmount.Value;
                if (change > 0)
                {
                    copySale += string.Format("Повраћај:{0}\r\n", change.ToString("0.00").Replace('.', ',').PadLeft(40 - "Повраћај:".Length));
                }
            }
            copySale += "========================================\r\n";

            copySale += "ОВО НИЈЕ ФИСКАЛНИ РАЧУН\r\n";
            copySale += "========================================\r\n";

            copySale += GetLabelsForJournal();

            copySale += GetSDCinformationForJournal();
            copySale += "======== ОВО НИЈЕ ФИСКАЛНИ РАЧУН =======\r\n";

            return copySale;
        }
        private static string CopyRefund(InvoiceRequest invoiceRequest)
        {
            string copyRefund = "======== ОВО НИЈЕ ФИСКАЛНИ РАЧУН =======\r\n";

            copyRefund += GetTaxpayerInformationForJournal();
            copyRefund += GetPOSinformationForJournal(invoiceRequest);

            copyRefund += "-----------КОПИЈА РЕФУНДАЦИЈА-----------\r\n";

            copyRefund += GetItemsForJournal(invoiceRequest);

            copyRefund += "----------------------------------------\r\n";
            if (_invoiceResult.TotalAmount.HasValue)
            {
                copyRefund += string.Format("За уплату:{0}\r\n", _invoiceResult.TotalAmount.Value.ToString("0.00").PadLeft(40 - "За уплату:".Length));
            }

            decimal totalPayment;
            copyRefund += GetPaymentForJournal(invoiceRequest, out totalPayment);
            copyRefund += "========================================\r\n";

            copyRefund += "ОВО НИЈЕ ФИСКАЛНИ РАЧУН\r\n";

            copyRefund += "========================================\r\n";

            copyRefund += GetLabelsForJournal();

            copyRefund += GetSDCinformationForJournal();

            copyRefund += "                                        \r\n";
            copyRefund += "                                        \r\n";
            copyRefund += "                                        \r\n";
            copyRefund += "                                        \r\n";
            copyRefund += "                                        \r\n";
            copyRefund += "Потпис купца: __________________________\r\n";
            copyRefund += "                                        \r\n";
            copyRefund += "                                        \r\n";

            copyRefund += "======== ОВО НИЈЕ ФИСКАЛНИ РАЧУН =======\r\n";

            return copyRefund;
        }
        private static string ProformaSale(InvoiceRequest invoiceRequest)
        {
            string proformaSale = "======== ОВО НИЈЕ ФИСКАЛНИ РАЧУН =======\r\n";

            proformaSale += GetTaxpayerInformationForJournal();
            proformaSale += GetPOSinformationForJournal(invoiceRequest);

            proformaSale += "-----------ПРЕДРАЧУН ПРОДАЈА------------\r\n";

            proformaSale += GetItemsForJournal(invoiceRequest);

            proformaSale += "----------------------------------------\r\n";
            if (_invoiceResult.TotalAmount.HasValue)
            {
                proformaSale += string.Format("За уплату:{0}\r\n", _invoiceResult.TotalAmount.Value.ToString("0.00").Replace('.', ',').PadLeft(40 - "За уплату:".Length));
            }

            decimal totalPayment;
            proformaSale += GetPaymentForJournal(invoiceRequest, out totalPayment);
            if (_invoiceResult.TotalAmount.HasValue)
            {
                decimal change = totalPayment - _invoiceResult.TotalAmount.Value;
                if (change > 0)
                {
                    proformaSale += string.Format("Повраћај:{0}\r\n", change.ToString("0.00").Replace('.', ',').PadLeft(40 - "Повраћај:".Length));
                }
            }
            proformaSale += "========================================\r\n";

            proformaSale += "ОВО НИЈЕ ФИСКАЛНИ РАЧУН\r\n";

            proformaSale += "========================================\r\n";

            proformaSale += GetLabelsForJournal();

            proformaSale += GetSDCinformationForJournal();

            proformaSale += "======== ОВО НИЈЕ ФИСКАЛНИ РАЧУН =======\r\n";

            return proformaSale;
        }
        private static string ProformaRefund(InvoiceRequest invoiceRequest)
        {
            string proformaRefund = "======== ОВО НИЈЕ ФИСКАЛНИ РАЧУН =======\r\n";

            proformaRefund += GetTaxpayerInformationForJournal();
            proformaRefund += GetPOSinformationForJournal(invoiceRequest);

            proformaRefund += "---------ПРЕДРАЧУН РЕФУНДАЦИЈА----------\r\n";

            proformaRefund += GetItemsForJournal(invoiceRequest);

            proformaRefund += "----------------------------------------\r\n";

            if (_invoiceResult.TotalAmount.HasValue)
            {
                proformaRefund += string.Format("За уплату:{0}\r\n", _invoiceResult.TotalAmount.Value.ToString("0.00").PadLeft(40 - "За уплату:".Length));
            }

            decimal totalPayment;
            proformaRefund += GetPaymentForJournal(invoiceRequest, out totalPayment);
            proformaRefund += "========================================\r\n";

            proformaRefund += "ОВО НИЈЕ ФИСКАЛНИ РАЧУН\r\n";

            proformaRefund += "========================================\r\n";

            proformaRefund += GetLabelsForJournal();

            proformaRefund += GetSDCinformationForJournal();

            proformaRefund += "======== ОВО НИЈЕ ФИСКАЛНИ РАЧУН =======\r\n";

            return proformaRefund;
        }
        private static string TrainingSale(InvoiceRequest invoiceRequest)
        {
            string trainingSale = "======== ОВО НИЈЕ ФИСКАЛНИ РАЧУН =======\r\n";

            trainingSale += GetTaxpayerInformationForJournal();
            trainingSale += GetPOSinformationForJournal(invoiceRequest);

            trainingSale += "-------------ОБУКА ПРОДАЈА--------------\r\n";

            trainingSale += GetItemsForJournal(invoiceRequest);

            trainingSale += "----------------------------------------\r\n";
            if (_invoiceResult.TotalAmount.HasValue)
            {
                trainingSale += string.Format("За уплату:{0}\r\n", _invoiceResult.TotalAmount.Value.ToString("0.00").Replace('.', ',').PadLeft(40 - "За уплату:".Length));
            }

            decimal totalPayment;
            trainingSale += GetPaymentForJournal(invoiceRequest, out totalPayment);
            if (_invoiceResult.TotalAmount.HasValue)
            {
                decimal change = totalPayment - _invoiceResult.TotalAmount.Value;
                if (change > 0)
                {
                    trainingSale += string.Format("Повраћај:{0}\r\n", change.ToString("0.00").Replace('.', ',').PadLeft(40 - "Повраћај:".Length));
                }
            }
            trainingSale += "========================================\r\n";

            trainingSale += "ОВО НИЈЕ ФИСКАЛНИ РАЧУН\r\n";

            trainingSale += "========================================\r\n";

            trainingSale += GetLabelsForJournal();

            trainingSale += GetSDCinformationForJournal();

            trainingSale += "======== ОВО НИЈЕ ФИСКАЛНИ РАЧУН =======\r\n";

            return trainingSale;
        }
        private static string TrainingRefund(InvoiceRequest invoiceRequest)
        {
            string trainingRefund = "======== ОВО НИЈЕ ФИСКАЛНИ РАЧУН =======\r\n";

            trainingRefund += GetTaxpayerInformationForJournal();
            trainingRefund += GetPOSinformationForJournal(invoiceRequest);

            trainingRefund += "-----------ОБУКА РЕФУНДАЦИЈА------------\r\n";

            trainingRefund += GetItemsForJournal(invoiceRequest);

            trainingRefund += "----------------------------------------\r\n";
            if (_invoiceResult.TotalAmount.HasValue)
            {
                trainingRefund += string.Format("За уплату:{0}\r\n", _invoiceResult.TotalAmount.Value.ToString("0.00").PadLeft(40 - "За уплату:".Length));
            }

            decimal totalPayment;
            trainingRefund += GetPaymentForJournal(invoiceRequest, out totalPayment);
            trainingRefund += "========================================\r\n";

            trainingRefund += "ОВО НИЈЕ ФИСКАЛНИ РАЧУН\r\n";

            trainingRefund += "========================================\r\n";

            trainingRefund += GetLabelsForJournal();

            trainingRefund += GetSDCinformationForJournal();

            trainingRefund += "======== ОВО НИЈЕ ФИСКАЛНИ РАЧУН =======\r\n";

            return trainingRefund;
        }

        private static string GetTaxpayerInformationForJournal()
        {
            string taxpayerInformation = string.Empty;
            taxpayerInformation += CenterString(_invoiceResult.Tin, 40);
            taxpayerInformation += CenterString(_invoiceResult.BusinessName, 40);
            taxpayerInformation += CenterString(_invoiceResult.LocationName, 40);
            taxpayerInformation += CenterString(_invoiceResult.Address, 40);
            taxpayerInformation += CenterString(_invoiceResult.District, 40);

            return taxpayerInformation;
        }
        private static string GetPOSinformationForJournal(InvoiceRequest invoiceRequest)
        {
            string posInformation = SplitInParts(invoiceRequest.Cashier, "Касир: ", 40);

            if (!string.IsNullOrEmpty(invoiceRequest.BuyerId))
            {
                posInformation += SplitInParts(invoiceRequest.BuyerId, "ИД купца: ", 40);
            }

            if (!string.IsNullOrEmpty(invoiceRequest.BuyerCostCenterId))
            {
                posInformation += SplitInParts(invoiceRequest.BuyerCostCenterId, "Опционо поље купца: ", 40);
            }

            if (!string.IsNullOrEmpty(invoiceRequest.BuyerName))
            {
                posInformation += SplitInParts(invoiceRequest.BuyerName, "Назив купца: ", 50);
            }

            if (!string.IsNullOrEmpty(invoiceRequest.BuyerAddress))
            {
                posInformation += SplitInParts(invoiceRequest.BuyerAddress, "Адреса купца: ", 50);
            }

            if (!string.IsNullOrEmpty(invoiceRequest.InvoiceNumber))
            {
                posInformation += SplitInParts(invoiceRequest.InvoiceNumber, "ЕСИР број: ", 40);
            }

            if (invoiceRequest.DateAndTimeOfIssue.HasValue &&
                invoiceRequest.InvoiceType == Enums.InvoiceTypeEenumeration.Advance &&
                invoiceRequest.DateAndTimeOfIssue.Value.Date < DateTime.Now.Date)
            {
                posInformation += SplitInParts(invoiceRequest.DateAndTimeOfIssue.Value.ToString("dd.MM.yyyy HH:mm:ss"), "ЕСИР време: ", 40);
            }

            if (!string.IsNullOrEmpty(invoiceRequest.ReferentDocumentNumber))
            {
                posInformation += SplitInParts(invoiceRequest.ReferentDocumentNumber, "Реф. број: ", 40);
            }

            if (invoiceRequest.ReferentDocumentDT != null)
            {
                posInformation += SplitInParts(invoiceRequest.ReferentDocumentDT.ToString(), "Реф. време: ", 40);
            }

            return posInformation;
        }
        private static string GetItemsForJournal(InvoiceRequest invoiceRequest)
        {
            string items = "Артикли\r\n";
            items += "========================================\r\n";
            items += string.Format("{0}{1}{2}{3}\r\n", "Назив".PadRight(10), "Јед. цена".PadRight(10), "Кол.".PadLeft(9), "Укупно".PadLeft(11));

            foreach (Item item in invoiceRequest.Items)
            {
                string i = string.Format("{0} (", item.Name);
                foreach (string label in item.Labels)
                {
                    i += string.Format("{0}", label);
                    if (item.Labels.Last() != label)
                    {
                        i += string.Format(", ");
                    }
                }
                i += string.Format($")/{item.Jm}");

                decimal price = item.TotalAmount / item.Quantity;

                items += SplitInParts(i, "", 40, 1);
                items += string.Format("{0}{1}{2}{3}\r\n", string.Empty.PadRight(10),
                    price.ToString("0.00").Replace('.', ',').PadRight(10),
                    item.Quantity.ToString("0.00").Replace('.', ',').PadLeft(9),
                    invoiceRequest.TransactionType == Enums.TransactionTypeEnumeration.Sale ?
                    item.TotalAmount.ToString("0.00").Replace('.', ',').PadLeft(11) : item.TotalAmount.ToString("-0.00").Replace('.', ',').PadLeft(11));
            }

            return items;
        }
        private static string GetPaymentForJournal(InvoiceRequest invoiceRequest, out decimal totalPayment)
        {
            string paymentString = string.Empty;

            totalPayment = 0;
            foreach (Payment payment in invoiceRequest.Payment)
            {
                string paymentType = string.Empty;
                switch (payment.PaymentType)
                {
                    case Enums.PaymentTypeEnumeration.Other:
                        paymentType = "Уплаћено - друго безготовинско плаћање:";
                        break;
                    case Enums.PaymentTypeEnumeration.Cash:
                        paymentType = "Уплаћено - готовина:";
                        break;
                    case Enums.PaymentTypeEnumeration.Card:
                        paymentType = "Уплаћено - платна картица:";
                        break;
                    case Enums.PaymentTypeEnumeration.Check:
                        paymentType = "Уплаћено - чек:";
                        break;
                    case Enums.PaymentTypeEnumeration.WireTransfer:
                        paymentType = "Уплаћено - пренос на рачун:";
                        break;
                    case Enums.PaymentTypeEnumeration.Voucher:
                        paymentType = "Уплаћено - ваучер:";
                        break;
                    case Enums.PaymentTypeEnumeration.MobileMoney:
                        paymentType = "Уплаћено - инстант плаћање:";
                        break;
                }

                totalPayment += payment.Amount;
                if (payment.PaymentType != Enums.PaymentTypeEnumeration.Other)
                {
                    paymentString += string.Format("{0}{1}\r\n", paymentType.PadRight(29),
                        payment.Amount.ToString("0.00").Replace('.', ',').PadLeft(11));
                }
                else
                {
                    paymentString += string.Format("{0}\n{1}\r\n", paymentType.PadRight(40),
                        payment.Amount.ToString("0.00").Replace('.', ',').PadLeft(40));
                }
            }

            return paymentString;
        }
        private static decimal CalculateTotalTax(InvoiceResult invoice)
        {
            decimal totalTax = 0;

            invoice.TaxItems.ToList().ForEach(x =>
            {
                totalTax += Convert.ToDecimal(x.Amount);
            });

            return totalTax;
        }
        private static string GetLabelsForJournal()
        {
            if (_invoiceResult.TaxItems != null)
            {
                string labels = string.Format("{0}{1}{2}{3}\r\n", "Ознака".PadRight(8), "Име".PadRight(12), "Стопа".PadRight(6), "Порез".PadLeft(14));

                decimal totalTax = 0;

                foreach (TaxItem taxItem in _invoiceResult.TaxItems)
                {

                    string label = taxItem.Label.PadRight(8);
                    string categoryName = taxItem.CategoryName.PadRight(12);
                    string rate = taxItem.Rate.ToString("0.00").Replace('.', ',');
                    rate += taxItem.CategoryType.Value == Enums.CategoryTypeEnumeration.AmountPerQuantity ?
                        "$" :
                        "%";
                    rate = rate.PadRight(6);
                    string amount = taxItem.Amount.ToString("0.00").Replace('.', ',').PadLeft(14);

                    labels += $"{label}{categoryName}{rate}{amount}\r\n";

                    totalTax += Convert.ToDecimal(taxItem.Amount);
                }
                labels += "----------------------------------------\r\n";

                labels += string.Format("Укупан износ пореза:{0}\r\n", totalTax.ToString("0.00").Replace('.', ',').PadLeft(40 - "Укупан износ пореза:".Length));

                labels += "========================================\r\n";
                return labels;
            }
            return string.Empty;
        }
        private static string GetSDCinformationForJournal()
        {
            string sdcInformation = string.Format("ПФР време:{0}\r\n", _invoiceResult.SdcDateTime.ToString("dd.MM.yyyy HH:mm:ss").PadLeft(40 - "ПФР време:".Length));
            sdcInformation += string.Format("ПФР број рачуна:{0}\r\n", _invoiceResult.InvoiceNumber.PadLeft(40 - "ПФР број рачуна:".Length));
            sdcInformation += string.Format("Бројач рачуна:{0}\r\n", _invoiceResult.InvoiceCounter.PadLeft(40 - "Бројач рачуна:".Length));

            sdcInformation += "========================================\r\n";
            return sdcInformation;
        }

        private static string CenterString(string value, int length)
        {
            string journal = string.Empty;
            if (string.IsNullOrEmpty(value))
            {
                value = string.Empty;
            }

            if (value.Length < length)
            {
                int spaces = length - value.Length;
                int padLeft = spaces / 2 + value.Length;

                return $"{value.PadLeft(padLeft).PadRight(length)}\r\n";
            }

            string str = value;
            int journalLength = value.Length;

            int counter = 0;

            while (journalLength > 0)
            {
                int len = 0;
                if (journalLength > length)
                {
                    len = length;
                }
                else
                {
                    len = journalLength;
                }
                string s = str.Substring(counter * length, len);

                int spaces = length - s.Length;
                int padLeft = spaces / 2 + s.Length;

                journal += $"{s.PadLeft(padLeft).PadRight(length)}\r\n";

                journalLength -= s.Length;
                counter++;
            }

            return journal;
        }
        private static string SplitInParts(string value, string fixedPart, int length, int pad = 0)
        {
            string journal = string.Empty;

            if (string.IsNullOrEmpty(value))
            {
                value = string.Empty;
            }

            if (fixedPart.Length + value.Length <= length)
            {
                if (pad == 0)
                {
                    journal = string.Format("{0}{1}\r\n", fixedPart, value.PadLeft(length - fixedPart.Length));
                }
                else
                {
                    journal = string.Format("{0}{1}\r\n", fixedPart, value.PadRight(length));
                }
                return journal;
            }

            string str = fixedPart + value;

            int journalLength = str.Length;

            int counter = 0;

            while (journalLength > 0)
            {
                int len = 0;
                if (journalLength > length)
                {
                    len = length;
                }
                else
                {
                    len = journalLength;
                }
                string s = str.Substring(counter * length, len);

                journal += string.Format("{0}\r\n", s.PadRight(length));

                journalLength -= s.Length;
                counter++;
            }

            return journal;
        }
    }
}

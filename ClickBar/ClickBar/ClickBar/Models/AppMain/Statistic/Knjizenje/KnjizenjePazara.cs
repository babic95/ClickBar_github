using ClickBar_Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Statistic.Knjizenje
{
    public class KnjizenjePazara : ObservableObject
    {
        private string _id;
        private string _description;
        private DateTime _issueDateTime;
        private decimal _normalSaleCash;
        private decimal _normalSaleCard;
        private decimal _normalSaleWireTransfer;
        private decimal _normalRefundCash;
        private decimal _normalRefundCard;
        private decimal _normalRefundWireTransfer;
        private decimal _total;

        public KnjizenjePazara(KnjizenjePazaraDB knjizenjePazaraDB)
        {
            Id = knjizenjePazaraDB.Id;
            Description = knjizenjePazaraDB.Description;
            IssueDateTime = knjizenjePazaraDB.IssueDateTime;
            NormalSaleCash = knjizenjePazaraDB.NormalSaleCash;
            NormalSaleCard = knjizenjePazaraDB.NormalSaleCard;
            NormalSaleWireTransfer = knjizenjePazaraDB.NormalSaleWireTransfer;
            NormalRefundCash = knjizenjePazaraDB.NormalRefundCash;
            NormalRefundCard = knjizenjePazaraDB.NormalRefundCard;
            NormalRefundWireTransfer = knjizenjePazaraDB.NormalRefundWireTransfer;

            Total = NormalSaleCash + NormalSaleCard + NormalSaleWireTransfer - NormalRefundCash - NormalRefundCard - NormalRefundWireTransfer;
        }

        public KnjizenjePazara(DateTime issueDateTime)
        {
            Id = Guid.NewGuid().ToString();
            IssueDateTime = issueDateTime;
            Description = $"Knjiženje pazara na dan {issueDateTime.ToString("dd.MM.yyyy")}";
            NormalSaleCash = 0;
            NormalSaleCard = 0;
            NormalSaleWireTransfer = 0;
            NormalRefundCash = 0;
            NormalRefundCard = 0;
            NormalRefundWireTransfer = 0;
            Total = 0;
        }

        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChange(nameof(Id));
            }
        }
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChange(nameof(Description));
            }
        }
        public DateTime IssueDateTime
        {
            get { return _issueDateTime; }
            set
            {
                _issueDateTime = value;
                OnPropertyChange(nameof(IssueDateTime));
            }
        }
        public decimal NormalSaleCash
        {
            get { return _normalSaleCash; }
            set
            {
                _normalSaleCash = value;
                OnPropertyChange(nameof(NormalSaleCash));
            }
        }
        public decimal NormalSaleCard
        {
            get { return _normalSaleCard; }
            set
            {
                _normalSaleCard = value;
                OnPropertyChange(nameof(NormalSaleCard));
            }
        }
        public decimal NormalSaleWireTransfer
        {
            get { return _normalSaleWireTransfer; }
            set
            {
                _normalSaleWireTransfer = value;
                OnPropertyChange(nameof(NormalSaleWireTransfer));
            }
        }
        public decimal NormalRefundCash
        {
            get { return _normalRefundCash; }
            set
            {
                _normalRefundCash = value;
                OnPropertyChange(nameof(NormalRefundCash));
            }
        }
        public decimal NormalRefundCard
        {
            get { return _normalRefundCard; }
            set
            {
                _normalRefundCard = value;
                OnPropertyChange(nameof(NormalRefundCard));
            }
        }
        public decimal NormalRefundWireTransfer
        {
            get { return _normalRefundWireTransfer; }
            set
            {
                _normalRefundWireTransfer = value;
                OnPropertyChange(nameof(NormalRefundWireTransfer));
            }
        }
        public decimal Total
        {
            get { return _total; }
            set
            {
                _total = value;
                OnPropertyChange(nameof(Total));
            }
        }
    }
}

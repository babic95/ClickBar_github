using ClickBar.Models.Sale;
using ClickBar_DatabaseSQLManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.AppMain.Statistic.Radnici
{
    public class WhatDidWorkerSell : ObservableObject
    {
        private ItemInvoiceDB _item;
        private InvoiceDB _invoice;
        private string _itemName;
        private string _itemJm;

        public ItemInvoiceDB Item
        {
            get { return _item; }
            set
            {
                _item = value;
                OnPropertyChange(nameof(Item));
            }
        }
        public InvoiceDB Invoice
        {
            get { return _invoice; }
            set
            {
                _invoice = value;
                OnPropertyChange(nameof(Invoice));
            }
        }
        public string ItemName
        {
            get { return _itemName; }
            set
            {
                _itemName = value;
                OnPropertyChange(nameof(ItemName));
            }
        }
        public string ItemJm
        {
            get { return _itemJm; }
            set
            {
                _itemJm = value;
                OnPropertyChange(nameof(ItemJm));
            }
        }
    }
}

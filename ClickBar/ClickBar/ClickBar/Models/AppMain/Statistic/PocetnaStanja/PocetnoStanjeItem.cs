using System;
using ClickBar.Models.Sale;

namespace ClickBar.Models.AppMain.Statistic.PocetnaStanja
{
    public class PocetnoStanjeItem : ObservableObject
    {
        private Invertory _item;
        private decimal _newQuantity;
        private string _newQuantityText;
        private decimal _newInputPrice;
        private string _newInputPriceText;

        public PocetnoStanjeItem(Invertory item)
        {
            Item = item;
            NewQuantityText = item.Quantity.ToString();
            NewInputPriceText = item.InputPrice.ToString();
        }
        public Invertory Item
        {
            get { return _item; }
            set
            {
                _item = value;
                OnPropertyChange(nameof(Item));
            }
        }
        public decimal NewQuantity
        {
            get { return _newQuantity; }
            set
            {
                _newQuantity = value;
                OnPropertyChange(nameof(NewQuantity));
            }
        }
        public string NewQuantityText
        {
            get { return _newQuantityText; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _newQuantityText = value.Replace(',', '.');
                }
                else
                {
                    _newQuantityText = value;
                }
                OnPropertyChange(nameof(NewQuantityText));

                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        NewQuantity = decimal.Round(Convert.ToDecimal(_newQuantityText), 3);
                    }
                    catch
                    {
                        NewQuantityText = "0";
                    }
                }
            }
        }
        public decimal NewInputPrice
        {
            get { return _newInputPrice; }
            set
            {
                _newInputPrice = value;
                OnPropertyChange(nameof(NewInputPrice));
            }
        }
        public string NewInputPriceText
        {
            get { return _newInputPriceText; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _newInputPriceText = value.Replace(',', '.');
                }
                else
                {
                    _newInputPriceText = value;
                }
                OnPropertyChange(nameof(NewInputPriceText));

                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        NewInputPrice = decimal.Round(Convert.ToDecimal(_newInputPriceText), 2);
                    }
                    catch
                    {
                        NewInputPriceText = "0";
                    }
                }
            }
        }
    }
}

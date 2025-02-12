using ClickBar.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Logging;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_DatabaseSQLManager;
using ClickBar.Models.Sale;
using System.Collections.ObjectModel;
using ClickBar.Models.AppMain.Statistic.Otpis;
using ClickBar.Enums.AppMain.Statistic;
using ClickBar.Models.AppMain.Statistic;

namespace ClickBar.Commands.AppMain.Statistic.Otpis
{
    public class CreateOtpisCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private OtpisViewModel _currentViewModel;

        public CreateOtpisCommand(OtpisViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                var result = MessageBox.Show("Da li ste sigurni da želite da kreirate otpis?",
                    "Kreiranje otpisa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (_currentViewModel.ItemsInOtpis.Any())
                    {
                        int counter = 1;

                        DateTime odDatuma = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0);
                        DateTime doDatuma = new DateTime(DateTime.Now.Year, 12, 31, 23, 59, 59);

                        var otpisiDB = _currentViewModel.DbContext.Otpisi.Where(o => o.OtpisDate.Date >= odDatuma &&
                        o.OtpisDate.Date <= doDatuma);

                        if (otpisiDB != null &&
                            otpisiDB.Any())
                        {
                            counter = otpisiDB.Max(o => o.Counter);
                            counter++;
                        }

                        DateTime otpisDate = DateTime.Now;
                        decimal totalOtpis = 0;

                        OtpisDB otpisDB = new OtpisDB()
                        {
                            Id = Guid.NewGuid().ToString(),
                            OtpisDate = otpisDate,
                            Counter = counter,
                            Name = $"Otpis robe {counter}/{otpisDate.ToString("yy")}",
                            CashierId = _currentViewModel.LoggedCashier.Id
                        };

                        _currentViewModel.DbContext.Otpisi.Add(otpisDB);
                        _currentViewModel.DbContext.SaveChanges();

                        _currentViewModel.ItemsInOtpis.ToList().ForEach(item =>
                        {
                            var itemDB = _currentViewModel.DbContext.Items.Find(item.ItemInOtpis.Id);

                            if (itemDB != null)
                            {
                                itemDB.TotalQuantity -= item.Quantity;
                                _currentViewModel.DbContext.Items.Update(itemDB);

                                OtpisItemDB otpisItemDB = new OtpisItemDB()
                                {
                                    OtpisId = otpisDB.Id,
                                    ItemId = item.ItemInOtpis.Id,
                                    Quantity = item.Quantity,
                                    TotalPrice = Decimal.Round(-1 * item.Quantity * item.ItemInOtpis.SellingUnitPrice, 2),
                                };

                                _currentViewModel.DbContext.OtpisItems.Add(otpisItemDB);

                                totalOtpis += otpisItemDB.TotalPrice;
                            }
                        });

                        KepDB kepOtpisDB = new KepDB()
                        {
                            Id = Guid.NewGuid().ToString(),
                            KepDate = otpisDate,
                            Type = (int)KepStateEnumeration.Otpis,
                            Razduzenje = 0,
                            Zaduzenje = totalOtpis,
                            Description = otpisDB.Name
                        };
                        _currentViewModel.DbContext.Kep.Add(kepOtpisDB);

                        _currentViewModel.DbContext.SaveChanges();

                        _currentViewModel.CurrentItem = null;
                        _currentViewModel.TextSearch = string.Empty;
                        _currentViewModel.SelectedItem = null;
                        _currentViewModel.QuantityString = "0";
                        _currentViewModel.ItemsInOtpis = new ObservableCollection<OtpisItem>();
                    }
                    MessageBox.Show("Uspešno kreiran otpis",
                        "Uspešno",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Log.Error("CreateOtpisCommand -> Execute -> Greska prilikom kreiranja otpisa", ex);
                MessageBox.Show("Greška prilikom kreiranja otpisa!\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
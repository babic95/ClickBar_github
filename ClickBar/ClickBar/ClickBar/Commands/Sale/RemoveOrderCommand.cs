using ClickBar.ViewModels;
using ClickBar_DatabaseSQLManager;
using ClickBar_Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClickBar_Logging;
using ClickBar_DatabaseSQLManager.Models;
using System.Windows.Navigation;
using ClickBar.Enums;
using ClickBar.ViewModels.Sale;
using ClickBar_Common.Enums;
using ClickBar.Enums.Kuhinja;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore;

namespace ClickBar.Commands.Sale
{
    public class RemoveOrderCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SaleViewModel _viewModel;

        public RemoveOrderCommand(SaleViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var result = MessageBox.Show("Da li ste sigurni da želite da obrišete trenutnu porudžbinu?", "Brisanje porudžbine",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (_viewModel.TableId > 0 &&
                        SettingsManager.Instance.CancelOrderFromTable())
                    {
                        if (_viewModel.OldOrders.Count == 0)
                        {
                            MessageBox.Show("Nema ništa za brisanje!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        using (var dbContext = _viewModel.DbContextFactory.CreateDbContext())
                        {
                            var order = dbContext.UnprocessedOrders.FirstOrDefault(order => order.PaymentPlaceId == _viewModel.TableId);

                            if (order != null)
                            {
                                var ordersToday = dbContext.OrdersToday.Where(o => o.UnprocessedOrderId == order.Id).ToList();

                                if (ordersToday != null &&
                                    ordersToday.Any())
                                {
                                    foreach (var orderToday in ordersToday)
                                    {
                                        orderToday.Faza = (int)FazaKuhinjeEnumeration.Obrisana;
                                        dbContext.OrdersToday.Update(orderToday);
                                    }
                                }
                                dbContext.UnprocessedOrders.Remove(order);
                            }
                            dbContext.SaveChanges();

                            Log.Debug($"RemoveOrderCommand -> Execute -> Porudžbina sa stola {_viewModel.TableId} je uspešno obrisana!");
                        }

                        int tableId = _viewModel.TableId;

                        _viewModel.Reset();

                        var typeApp = SettingsManager.Instance.GetTypeApp();

                        if (typeApp == TypeAppEnumeration.Sale)
                        {
                            AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.Sale,
                            _viewModel.LoggedCashier,
                            tableId,
                            _viewModel);
                            _viewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                        }
                        else
                        {
                            AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.TableOverview,
                            _viewModel.LoggedCashier,
                            tableId,
                            _viewModel);
                            _viewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Greška prilikom brisanja porudžbine!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

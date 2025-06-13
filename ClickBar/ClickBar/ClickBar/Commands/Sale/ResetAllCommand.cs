using ClickBar.Enums;
using ClickBar.ViewModels;
using ClickBar_Common.Enums;
using ClickBar_DatabaseSQLManager;
using ClickBar_Logging;
using ClickBar_Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.Commands.Sale
{
    public class ResetAllCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SaleViewModel _viewModel;

        public ResetAllCommand(SaleViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var result = MessageBox.Show("Da li ste sigurni da želite da poništite trenutni račun?", "Poništi račun",
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    //if (_viewModel.TableId > 0 &&
                    //    SettingsManager.Instance.CancelOrderFromTable())
                    //{
                    //    SqlServerDbContext sqliteDbContext = new SqlServerDbContext();

                    //    var order = sqliteDbContext.UnprocessedOrders.FirstOrDefault(order => order.PaymentPlaceId == _viewModel.TableId);

                    //    if (order != null)
                    //    {
                    //        var itemsInOrder = sqliteDbContext.ItemsInUnprocessedOrder.Where(item => item.UnprocessedOrderId == order.Id);

                    //        if (itemsInOrder != null &&
                    //            itemsInOrder.Any())
                    //        {
                    //            itemsInOrder.ToList().ForEach(item =>
                    //            {
                    //                sqliteDbContext.ItemsInUnprocessedOrder.Remove(item);
                    //            });
                    //        }
                    //        sqliteDbContext.UnprocessedOrders.Remove(order);
                    //        sqliteDbContext.SaveChanges();
                    //    }
                    //}

                    var idStola = _viewModel.TableId;

                    _viewModel.Reset();

                    var typeApp = SettingsManager.Instance.GetTypeApp();

                    if (typeApp == TypeAppEnumeration.Sale)
                    {
                        AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.Sale,
                        _viewModel.LoggedCashier,
                        idStola,
                        _viewModel);
                        _viewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                    }
                    else
                    {
                        AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.TableOverview,
                        _viewModel.LoggedCashier,
                        idStola,
                        _viewModel);
                        _viewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                    }
                    Log.Debug($"Uspesno ponisten pregled stola {idStola}");
                }
                catch (Exception ex) 
                {
                    Log.Error($"ResetAllCommand -> Greška prilikom poništavanja porudžbine sa stola {_viewModel.TableId}: ", ex);
                    MessageBox.Show("Greška prilikom poništavanja porudžbine!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

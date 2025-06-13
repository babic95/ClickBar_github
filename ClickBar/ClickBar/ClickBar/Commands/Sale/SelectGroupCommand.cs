using ClickBar.Models.Sale;
using ClickBar.ViewModels;
using ClickBar_DatabaseSQLManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClickBar.Commands.Sale
{
    public class SelectGroupCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SaleViewModel _viewModel;

        public SelectGroupCommand(SaleViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            int idGroup = Convert.ToInt32(parameter);

            var group = _viewModel.Groups.Where(g => g.Id == idGroup).ToList().FirstOrDefault();

            if (group != null && !group.Focusable)
            {
                if (_viewModel.CurrentGroup != null)
                {
                    _viewModel.CurrentGroup.Focusable = false;
                }

                group.Focusable = true;
                _viewModel.CurrentGroup = group;
                _viewModel.Items = new ObservableCollection<Item>();

                var items = _viewModel.AllItems.Where(item => item.IdItemGroup == idGroup)
                    .OrderBy(i => i.Rb)
                    .Select(i => new Item(i))
                    .ToList();

                if (items.Any())
                {
                    items.ForEach(i => _viewModel.Items.Add(i));
                }
            }
        }
    }
}

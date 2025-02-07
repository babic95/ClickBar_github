using ClickBar.Models.Sale;
using ClickBar.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.Sale
{
    public class SelectSupergroupCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SaleViewModel _viewModel;

        public SelectSupergroupCommand(SaleViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            int idSupergroup = Convert.ToInt32(parameter);

            var supergroup = _viewModel.Supergroups.Where(g => g.Id == idSupergroup).ToList().FirstOrDefault();

            if (supergroup != null && !supergroup.Focusable)
            {
                if (_viewModel.CurrentSupergroup != null)
                {
                    _viewModel.CurrentSupergroup.Focusable = false;
                }

                supergroup.Focusable = true;
                _viewModel.CurrentSupergroup = supergroup;
                _viewModel.Groups = new ObservableCollection<GroupItems>();
                _viewModel.Items = new ObservableCollection<Item>();

                var groups = _viewModel.AllGroups.Where(group => group.IdSupergroup == idSupergroup).ToList();

                if (groups.Any())
                {
                    groups.ForEach(group =>
                    {
                        GroupItems g = new GroupItems(group.Id, group.IdSupergroup, group.Name);

                        if (!g.Name.ToLower().Contains("sirovine"))
                        {
                            _viewModel.Groups.Add(g);
                        }
                        
                    });
                }
            }
        }
    }
}

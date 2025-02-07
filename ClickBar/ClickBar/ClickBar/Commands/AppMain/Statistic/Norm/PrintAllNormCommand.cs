using ClickBar.ViewModels.AppMain.Statistic;
using ClickBar_Common.Models.Statistic.Nivelacija;
using ClickBar_Common.Models.Statistic.Norm;
using ClickBar_Database;
using ClickBar_Database.Models;
using ClickBar_Printer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Statistic.Norm
{
    public class PrintAllNormCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private NormViewModel _currentViewModel;

        public PrintAllNormCommand(NormViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            using (SqliteDbContext sqliteDbContext = new SqliteDbContext())
            {

                //var allNorms = sqliteDbContext.Norms.Join(sqliteDbContext.Items, 
                //    norm => norm.Id,
                //    item => item.IdNorm,
                //    (norm, item) => new { Norm = norm, Item = item })
                //    .Join(sqliteDbContext.ItemsInNorm,
                //    norm => norm.Norm.Id,
                //    itemInNorm => itemInNorm.IdNorm,
                //    (norm, itemInNorm) => new { Norm = norm, ItemInNorm = itemInNorm }).
                //    Join(sqliteDbContext.Items,
                //    norm => norm.ItemInNorm.IdItem,
                //    item => item.Id,
                //    (norm, item) => new { Norm = norm, Item = item });

                var allNorms = sqliteDbContext.Items.Where(item => item.IdNorm != null);

                if (allNorms != null &&
                    allNorms.Any())
                {
                    Dictionary<string, Dictionary<string, List<NormGlobal>>> norms = new Dictionary<string, Dictionary<string, List<NormGlobal>>>();

                    allNorms.ForEachAsync(norm =>
                    {
                        var groupNorm = sqliteDbContext.ItemGroups.Find(norm.IdItemGroup);

                        if (groupNorm != null)
                        {
                            var supergroup = sqliteDbContext.Supergroups.Find(groupNorm.IdSupergroup);

                            if (supergroup != null)
                            {
                                NormGlobal normGlobal = new NormGlobal()
                                {
                                    Id = norm.Id,
                                    Name = norm.Name,
                                    Items = new List<NormItemGlobal>()
                                };

                                var normItems = sqliteDbContext.ItemsInNorm.Where(itemInNorm => itemInNorm.IdNorm == norm.IdNorm);

                                if (normItems != null &&
                                normItems.Any())
                                {
                                    normItems.ForEachAsync(itemInNorm =>
                                    {
                                        var itemDB = sqliteDbContext.Items.Find(itemInNorm.IdItem);

                                        if (itemDB != null)
                                        {
                                            normGlobal.Items.Add(new NormItemGlobal()
                                            {
                                                Id = itemInNorm.IdItem,
                                                Quantity = string.Format("{0:#,##0.000}", itemInNorm.Quantity).Replace(',', '#').Replace('.', ',').Replace('#', '.'),
                                                Name = itemDB.Name,
                                                JM = itemDB.Jm
                                            });
                                        }
                                    });

                                    if (norms.ContainsKey(supergroup.Name))
                                    {
                                        if (norms[supergroup.Name].ContainsKey(groupNorm.Name))
                                        {
                                            norms[supergroup.Name][groupNorm.Name].Add(normGlobal);
                                        }
                                        else
                                        {
                                            norms[supergroup.Name].Add(groupNorm.Name, new List<NormGlobal>() { normGlobal });
                                        }
                                    }
                                    else
                                    {
                                        norms.Add(supergroup.Name, new Dictionary<string, List<NormGlobal>>());
                                        norms[supergroup.Name].Add(groupNorm.Name, new List<NormGlobal>() { normGlobal });
                                    }
                                }
                            }
                        }
                    });

                    PrinterManager.Instance.PrintNorms(norms);
                }
            }
        }
    }
}
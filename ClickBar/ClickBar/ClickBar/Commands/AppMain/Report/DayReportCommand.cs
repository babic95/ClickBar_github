using ClickBar.ViewModels.AppMain;
using ClickBar.Views.AppMain.AuxiliaryWindows.ReportWindows;
using ClickBar_Printer.PaperFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Report
{
    public class DayReportCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ReportViewModel _currentViewModel;

        public DayReportCommand(ReportViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _currentViewModel.Title = "DNEVNI IZVEŠTAJ";

            _currentViewModel.AuxiliaryWindow = new DayReportWindow(_currentViewModel);
            _currentViewModel.AuxiliaryWindow.ShowDialog();

            _currentViewModel.StartReport = new DateTime(_currentViewModel.StartReport.Date.Year,
                _currentViewModel.StartReport.Date.Month,
                _currentViewModel.StartReport.Date.Day, 5, 0, 0);
            _currentViewModel.EndReport = _currentViewModel.StartReport.AddDays(1).AddSeconds(-1);

            ClickBar_Report.Report report;
            if (string.IsNullOrEmpty(_currentViewModel.SmartCard))
            {
                report = new ClickBar_Report.Report(_currentViewModel.DbContext, 
                    _currentViewModel.StartReport,
                    _currentViewModel.EndReport,
                    _currentViewModel.Items);
            }
            else
            {
                report = new ClickBar_Report.Report(_currentViewModel.DbContext, 
                    _currentViewModel.StartReport,
                    _currentViewModel.EndReport,
                    _currentViewModel.Items,
                    _currentViewModel.SmartCard);
            }

            _currentViewModel.CurrentReport = report;

#if CRNO
            _currentViewModel.Report = FormatPos.CreateReportBlack(report);
#else
            _currentViewModel.Report = FormatPos.CreateReport(report);
#endif
        }
    }
}
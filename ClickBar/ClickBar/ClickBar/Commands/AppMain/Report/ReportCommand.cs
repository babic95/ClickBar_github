using ClickBar.ViewModels.AppMain;
using ClickBar_Printer.PaperFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Report
{
    public class ReportCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ReportViewModel _currentViewModel;

        public ReportCommand(ReportViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _currentViewModel.Title = "PRESEK STANJA";

            DateTime end = DateTime.Now;
            DateTime start;

            if (end.Hour < 5)
            {
                start = end.Date;
            }
            else
            {
                start = new DateTime(end.Year, end.Month, end.Day, 5, 0, 0);
            }

            _currentViewModel.StartReport = start;
            _currentViewModel.EndReport = end;

            ClickBar_Report.Report report;
            if (string.IsNullOrEmpty(_currentViewModel.SmartCard))
            {
                report = new ClickBar_Report.Report(_currentViewModel.StartReport,
                    _currentViewModel.EndReport,
                    _currentViewModel.Items);
            }
            else
            {
                report = new ClickBar_Report.Report(_currentViewModel.StartReport,
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

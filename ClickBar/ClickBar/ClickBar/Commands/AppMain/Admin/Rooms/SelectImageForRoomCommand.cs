using ClickBar.ViewModels.AppMain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace ClickBar.Commands.AppMain.Admin.Rooms
{
    public class SelectImageForRoomCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private AdminViewModel _currentViewModel;

        public SelectImageForRoomCommand(AdminViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Image Files (*.bmp;*.png;*.jpg)|*.bmp;*.png;*.jpg"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                var filePath = ofd.FileName;

                if (string.IsNullOrEmpty(filePath))
                {
                    _currentViewModel.NewRoom.Image = null;
                }
                else
                {
                    _currentViewModel.NewRoom.Image = filePath;
                }
            }
        }
    }
}
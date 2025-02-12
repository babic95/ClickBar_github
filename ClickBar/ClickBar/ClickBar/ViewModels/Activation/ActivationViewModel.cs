using ClickBar.Commands.Activation;
using ClickBar_API;
using ClickBar_DatabaseSQLManager;
using ClickBar_Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Input;

namespace ClickBar.ViewModels.Activation
{
    public enum ActivationCodePartEnumeration
    {
        FirstPart = 0,
        SecondPart = 1,
        ThirdPart = 2,
        FourPart = 3,
        FivePart = 4
    }

    public class ActivationViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly MainViewModel _mainViewModel;
        private string _firstPart;
        private string _secondPart;
        private string _thirdPart;
        private string _fourPart;
        private string _fivePart;
        private string _activationCodeNumber;
        private bool _isEnable;
        private ActivationCodePartEnumeration _activationCode;

        public ActivationViewModel(IServiceProvider serviceProvider, MainViewModel mainViewModel)
        {
            _serviceProvider = serviceProvider;
            _mainViewModel = mainViewModel;
            ActivationCommand = _serviceProvider.GetRequiredService<ActivationCommand>();

            IsEnable = false;
            ActivationCode = ActivationCodePartEnumeration.FirstPart;

            if (SettingsManager.Instance.GetEnableCCS_Server())
            {
                bool initializationCCS_Server = CCS_Fiscalization_ApiManager.Instance.Initialization().Result;

                if (!initializationCCS_Server)
                {
                    MessageBox.Show("Putanja do CCS SERVER-a ne postoji! Obratite se proizvođaču.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        App.Current.Shutdown();
                    });
                }
            }
        }

        #region Properties
        public ActivationCodePartEnumeration ActivationCode
        {
            get { return _activationCode; }
            set
            {
                _activationCode = value;
                OnPropertyChange(nameof(ActivationCode));
            }
        }

        public string FirstPart
        {
            get { return _firstPart; }
            set
            {
                _firstPart = value.ToUpper();
                OnPropertyChange(nameof(FirstPart));
                if (_firstPart.Length == 4)
                {
                    ActivationCode = ActivationCodePartEnumeration.SecondPart;
                }
                SetActivationCodeNumber();
            }
        }

        public string SecondPart
        {
            get { return _secondPart; }
            set
            {
                _secondPart = value.ToUpper();
                OnPropertyChange(nameof(SecondPart));
                if (_secondPart.Length == 4)
                {
                    ActivationCode = ActivationCodePartEnumeration.ThirdPart;
                }
                else if (_secondPart.Length == 0)
                {
                    ActivationCode = ActivationCodePartEnumeration.FirstPart;
                }
                SetActivationCodeNumber();
            }
        }

        public string ThirdPart
        {
            get { return _thirdPart; }
            set
            {
                _thirdPart = value.ToUpper();
                OnPropertyChange(nameof(ThirdPart));
                if (_thirdPart.Length == 4)
                {
                    ActivationCode = ActivationCodePartEnumeration.FourPart;
                }
                else if (_thirdPart.Length == 0)
                {
                    ActivationCode = ActivationCodePartEnumeration.SecondPart;
                }
                SetActivationCodeNumber();
            }
        }

        public string FourPart
        {
            get { return _fourPart; }
            set
            {
                _fourPart = value.ToUpper();
                OnPropertyChange(nameof(FourPart));
                if (_fourPart.Length == 4)
                {
                    ActivationCode = ActivationCodePartEnumeration.FivePart;
                }
                else if (_fourPart.Length == 0)
                {
                    ActivationCode = ActivationCodePartEnumeration.ThirdPart;
                }
                SetActivationCodeNumber();
            }
        }

        public string FivePart
        {
            get { return _fivePart; }
            set
            {
                _fivePart = value.ToUpper();
                OnPropertyChange(nameof(FivePart));
                if (_fivePart.Length == 0)
                {
                    ActivationCode = ActivationCodePartEnumeration.FourPart;
                }
                SetActivationCodeNumber();
            }
        }

        public string ActivationCodeNumber
        {
            get { return _activationCodeNumber; }
            set
            {
                _activationCodeNumber = value;
                OnPropertyChange(nameof(ActivationCodeNumber));

                if (_activationCodeNumber.Length == 24)
                {
                    IsEnable = true;
                }
                else
                {
                    IsEnable = false;
                }
            }
        }

        private void SetActivationCodeNumber()
        {
            ActivationCodeNumber = $"{FirstPart}-{SecondPart}-{ThirdPart}-{FourPart}-{FivePart}";
        }

        public bool IsEnable
        {
            get { return _isEnable; }
            set
            {
                _isEnable = value;
                OnPropertyChange(nameof(IsEnable));
            }
        }
        #endregion Properties

        public ICommand ActivationCommand { get; }
    }
}
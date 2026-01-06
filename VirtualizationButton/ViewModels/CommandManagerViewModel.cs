using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VirtualizationButton.Models;

namespace VirtualizationButton.ViewModels
{
    public class CommandManagerViewModel : INotifyPropertyChanged
    {
        private Models.Timer _timer = new Models.Timer();

        public string VirtualizationStatus => _isToggleActive ? "Virtualization on" : "Virtualization off";
        public HorizontalAlignment ToggleAlignment => _isToggleActive ? HorizontalAlignment.Right : HorizontalAlignment.Left;

        private bool _isToggleActive ;
        public bool IsToggleActive
        {
            get { return _isToggleActive; }
            set
            {
                _isToggleActive = value;
                OnPropertyChanged(nameof(IsToggleActive));
                OnPropertyChanged(nameof(VirtualizationStatus));
                OnPropertyChanged(nameof(ToggleAlignment));
            }
        }

        private bool _isLogOn;
        public bool IsLogOn
        {
            get { return _isLogOn; }
            set
            {
                _isLogOn = value;
                OnPropertyChanged(nameof(IsLogOn));
            }
        }
        private static void AnalysisVirtualization(Dictionary<string, bool?> values, CommandManagerViewModel obj)
        {
            if (values.ContainsKey("HyperVisorPresent"))
            {
                obj._isToggleActive = values["HyperVisorPresent"]?? false;
                return;
            }
            obj._isToggleActive = false;
            if(values.ContainsKey("HyperVRequirementVirtualizationFirmwareEnabled") && values["HyperVRequirementVirtualizationFirmwareEnabled"] == false)
            {
                MessageBox.Show("Virtualization is disabled in the bios, the program will be closed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            if (values.ContainsKey("HyperVRequirementDataExecutionPreventionAvailable") && values["HyperVRequirementDataExecutionPreventionAvailable"] == false)
            {
                MessageBox.Show("Your PC does not support DEP/NX Bit.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if (values.ContainsKey("HyperVRequirementSecondLevelAddressTranslation") && values["HyperVRequirementSecondLevelAddressTranslation"] == false)
            {
                MessageBox.Show("Your PC does not support SLAT.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if (values.ContainsKey("HyperVRequirementVMMonitorModeExtensions") && values["HyperVRequirementVMMonitorModeExtensions"] == false)
            {
                MessageBox.Show("The virtualization extension is not supported on your computer.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        public CommandManagerViewModel()
        {
            CommandManagerViewModel.AnalysisVirtualization(CommandManager.GetVirtualizationStatus(), this);
        }

        private RelayCommand virtualizationCommand;
        public RelayCommand VirtualizationCommand
        {
            get
            {
                return virtualizationCommand ??
                  (virtualizationCommand = new RelayCommand(obj =>
                  {
                      const int CLICK_DELAY_SECONDS = 10;
                      if (!_timer.IsDelayOver())
                      {
                          IsToggleActive = !IsToggleActive;
                          TimeSpan timeLeft = TimeSpan.FromSeconds(CLICK_DELAY_SECONDS) - (DateTime.Now - _timer.LastClickTime);
                          MessageBox.Show($"Wait {timeLeft.Seconds} second(s)", "Too fast!", MessageBoxButton.OK, MessageBoxImage.Error);
                          return;
                      }
                      _timer.LastClickTime = DateTime.Now;
                      if (IsToggleActive)
                      {
                          CommandManager.EnableVirtualization(_isLogOn);
                      }
                      else
                      {
                          CommandManager.DisableVirtualization(_isLogOn);
                      }

                      while (Logger.IsRunning != false)
                      {
                          TimeSpan timeLeft = TimeSpan.FromSeconds(CLICK_DELAY_SECONDS) - (DateTime.Now - _timer.LastClickTime);
                          MessageBox.Show($"Wait {timeLeft.Seconds} second(s) to complete the operation.", "Wait");
                      }
                      var answer = MessageBox.Show("Do you want to reboot now?", "Reboot", MessageBoxButton.YesNo, MessageBoxImage.Question);
                      if (answer == MessageBoxResult.Yes)
                      {
                          CommandManager.Reboot();
                          Application.Current.Shutdown();
                      }

                  }));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}

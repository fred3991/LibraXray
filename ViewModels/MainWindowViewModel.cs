using LibraXray.Commands;
using LibraXray.Commands.Libra;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LibraXray.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {

        private ObservableCollection<string> _logStrings = new ObservableCollection<string>() { "Log strings" };
        public ObservableCollection<string> LogStrings
        {
            get => LibraXRaySource.XraySettings.LogStrings;
            set
            {
                Set(ref _logStrings, value);
            }

        }

        public ICommand _closeApplicationCommand;
        public ICommand CloseApplicationCommand => _closeApplicationCommand ??= new RelayCommand(parameter =>
        {
            try
            {
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            CommandManager.InvalidateRequerySuggested();
        });

        public SerialPort sp232 = new SerialPort();

        public ICommand _ConnectXrayCommand;
        public ICommand ConnectXrayCommand => _ConnectXrayCommand ??= new RelayCommand(parameter =>
        {
            try
            {
                sp232 = new SerialPort();

                sp232.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                sp232.PortName = "COM3";
                sp232.BaudRate = 38400;
                sp232.DataBits = 8;
                sp232.Parity = System.IO.Ports.Parity.None;
                sp232.StopBits = System.IO.Ports.StopBits.One;
                sp232.NewLine = "\r";
                sp232.ReadBufferSize = 8;
                sp232.Open();

                sp232.WriteTimeout = 1000;
                sp232.ReadTimeout = 1000;

                sp232.WriteLine("STS");
                var str = sp232.ReadLine();


                //LibraXRaySource.Connect();
                //LogStrings = LibraXRaySource.XraySettings.LogStrings;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            CommandManager.InvalidateRequerySuggested();
        });

        public void LogMessageString(string m)
        {
            LogStrings.Insert(0, DateTime.Now.ToString("[MM/dd/yyyy HH:mm:ss] ") + RecivedCommand);
            if (LogStrings.Count > 100)
                LogStrings.RemoveAt(LogStrings.Count - 1);
        }
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadLine();
            RecivedCommand = indata;

            try
            {
                LogMessageString(RecivedCommand);
            }
            catch
            {

            }
            sp.DiscardOutBuffer();
            sp.DiscardInBuffer();

        }


        public ICommand _DisconnectXrayCommand;
        public ICommand DisconnectXrayCommand => _DisconnectXrayCommand ??= new RelayCommand(parameter =>
        {
            try
            {
                sp232.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            CommandManager.InvalidateRequerySuggested();
        });


        public LibraDeviceModel LibraXRaySource { get; set; }
        public LibraSettings LibraXRaySettings { get; set; }
        public XraySettings XraySettings { get; set; }


        //public XraySettings XraySettings { get; set; }
        //public LibraSettings Settings { get; set; }
        private string _SendCommand = "TYP";
        public string SendCommand
        {
            get => _SendCommand;
            set => Set(ref _SendCommand, value);
        }
        private string _RecivedCommand = "---";
        public string RecivedCommand
        {
            get => _RecivedCommand;
            set => Set(ref _RecivedCommand, value);
        }

        public ICommand _SendXrayCommand;
        public ICommand SendXrayCommand => _SendXrayCommand ??= new RelayCommand(parameter =>
        {
            try
            {
                sp232.WriteLine(SendCommand);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            CommandManager.InvalidateRequerySuggested();
        });


        public ICommand _BeepXrayCommand;
        public ICommand BeepXrayCommand => _BeepXrayCommand ??= new RelayCommand(parameter =>
        {
            try
            {
                Console.Beep(800, 500);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            CommandManager.InvalidateRequerySuggested();
        });

        #region VoltageControl

        private string _AcitveVoltage = "0";
        public string AcitveVoltage
        {
            get => _AcitveVoltage;
            set => Set(ref _AcitveVoltage, value);
        }

        private string _TubeVoltage = "0";
        public string TubeVoltage
        {
            get => _TubeVoltage;
            set => Set(ref _TubeVoltage, value);
        }

        private string _SetVoltage = "40";
        public string SetVoltage
        {
            get => _SetVoltage;
            set => Set(ref _SetVoltage, value);
        }

        public ICommand _UpVoltageCommand;
        public ICommand UpVoltageCommand => _UpVoltageCommand ??= new RelayCommand(parameter =>
        {
            try
            {
                var currentVoltage = Convert.ToInt32(SetVoltage);
                currentVoltage++;

                if (currentVoltage >= 150)
                {
                    currentVoltage = 150;
                }

                SetVoltage = currentVoltage.ToString();
                UpdateXraySetting();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            CommandManager.InvalidateRequerySuggested();
        });

        public ICommand _DownVoltageCommand;
        public ICommand DownVoltageCommand => _DownVoltageCommand ??= new RelayCommand(parameter =>
        {
            try
            {
                var currentVoltage = Convert.ToInt32(SetVoltage);
                currentVoltage--;

                if (currentVoltage <= 40)
                {
                    currentVoltage = 40;
                }

                SetVoltage = currentVoltage.ToString();
                UpdateXraySetting();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            CommandManager.InvalidateRequerySuggested();
        });

        #endregion

        #region CurrentControl

        private string _AcitveCurrent = "0";
        public string AcitveCurrent
        {
            get => _AcitveCurrent;
            set => Set(ref _AcitveCurrent, value);
        }

        private string _TubeCurrent = "0";
        public string TubeCurrent
        {
            get => _TubeCurrent;
            set => Set(ref _TubeCurrent, value);
        }

        private string _SetCurrent = "10";
        public string SetCurrent
        {
            get => _SetCurrent;
            set => Set(ref _SetCurrent, value);
        }

        public ICommand _UpCurrentCommand;
        public ICommand UpCurrentCommand => _UpCurrentCommand ??= new RelayCommand(parameter =>
        {
            try
            {
                var currentCurrent = Convert.ToInt32(SetCurrent);
                currentCurrent++;

                if (currentCurrent >= 500)
                {
                    currentCurrent = 500;
                }

                SetCurrent = currentCurrent.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            CommandManager.InvalidateRequerySuggested();
        });

        public ICommand _DownCurrentCommand;
        public ICommand DownCurrentCommand => _DownCurrentCommand ??= new RelayCommand(parameter =>
        {
            try
            {
                var currentCurrent = Convert.ToInt32(SetCurrent);
                currentCurrent--;

                if (currentCurrent <= 10)
                {
                    currentCurrent = 10;
                }

                SetCurrent = currentCurrent.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            CommandManager.InvalidateRequerySuggested();
        });

        #endregion

        public void UpdateXraySetting()
        {
            //Set Voltage
            var strVoltage = LibraXrayCommands.HIV + String.Format(" {0:000}", SetVoltage);
            sp232.WriteLine(strVoltage);

            var strCurrent = LibraXrayCommands.CUR + String.Format(" {0:000}", SetCurrent);
            sp232.WriteLine(strCurrent);

            LogMessageString(RecivedCommand);
            //sp232.WriteLine(LibraXrayCommands.HIV + String.Format("%02d", SetVoltage));
        }


        public MainWindowViewModel()
        {
            XraySettings xraySettings = new XraySettings();
            LibraXRaySource = new LibraDeviceModel();

            LibraXRaySettings = new LibraSettings();
            XraySettings = new XraySettings();

            LibraXRaySource.XraySettings = XraySettings;
            LibraXRaySource.Settings = LibraXRaySettings;

            LibraXRaySettings.XrayCOM = "COM3";
            LibraXRaySettings.XrayBaudRate = 38400;
            LibraXRaySource.Settings = LibraXRaySettings;

        }

    }

}

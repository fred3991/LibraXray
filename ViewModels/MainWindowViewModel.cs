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

        private ObservableCollection<string> _logStrings = new ObservableCollection<string>() { "Log strings"};
        public ObservableCollection<string> LogStrings 
        {   get => LibraXRaySource.XraySettings.LogStrings;
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

                sp232.WriteLine("STS");
                var str =  sp232.ReadLine();


                sp232.ReadBufferSize = 8;
                //LibraXRaySource.Connect();
                //LogStrings = LibraXRaySource.XraySettings.LogStrings;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            CommandManager.InvalidateRequerySuggested();
        });

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            RecivedCommand = indata.Trim();

            try
            {
                LogStrings.Insert(0, DateTime.Now.ToString("[MM/dd/yyyy HH:mm:ss] ") + RecivedCommand);
                if (LogStrings.Count > 100)
                    LogStrings.RemoveAt(LogStrings.Count - 1);
            }
            catch
            {

            }

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
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            CommandManager.InvalidateRequerySuggested();
        });
       

        public LibraDeviceModel LibraXRaySource {get;set;}
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
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            CommandManager.InvalidateRequerySuggested();
        });


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

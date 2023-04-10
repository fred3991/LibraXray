using LibraXray.Commands;
using LibraXray.Commands.Libra;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LibraXray.ViewModels
{
    internal class LibraDeviceModel : ViewModel
    {
		public enum DeviceStatuses
		{
			Disconnected = 0,
			Ready,
			Emission,
			Fault,
		}

		public enum MainStatuses
		{
			Disconnected = 0,
			Standby,
			Emitted,
			EmittedPulse,
			NotReady
		}

		public enum PortStatuses
		{
			Closed = 0,
			Open,
			Connected,
			Error
		}

		public XraySettings XraySettings { get; set; }
		public LibraSettings Settings { get; set; }

		#region Properties

		public int PortConnectInterval = 5000;
		public int DeviceConnectTimeout = 1000;
		public int DeviceConnectInterval = 5000;
		public int ReadStatusInterval = 500;
		public int ChangeValueBeforeSetInterval = 1000;

		public const int WatchdogMax = 255;
		public const int PulseMax = 65535;
		public const double LVMax = 32.55;
		public const double IVMax = 15;
		public const double RangeOfValues = 4095;

		public double kVMin = 0;
		public double kVMax = 88.89;
		public double mAMin = 0;
		public double mAMax = 13.88;
		public double tempMax = 70.036;


		private MainStatuses deviceStatus;
		public MainStatuses DeviceStatus
		{
			get { return deviceStatus; }
			set
			{
				Set(ref deviceStatus, value);
				XrayState = deviceStatus == MainStatuses.Emitted || deviceStatus == MainStatuses.EmittedPulse;
				ConnectCommand.RaiseCanExecuteChanged();
				DisconnectCommand.RaiseCanExecuteChanged();
				XRayOnCommand.RaiseCanExecuteChanged();
				XRayOffCommand.RaiseCanExecuteChanged();
			}
		}

		private PortStatuses portStatus;
		public PortStatuses PortStatus
		{
			get { return portStatus; }
			set { Set(ref portStatus, value); }
		}

		private string rsStatusString;
		public string RSStatusString
		{
			get { return rsStatusString; }
			set { Set(ref rsStatusString, value); }
		}

		private double kv = 0;
		public double KV
		{
			get { return kv; }
			set
			{
				Set(ref kv, value);
				XraySettings.XrayInfokVp = kv;
			}
		}

		private double kvBindable = 60;
		public double KVBindable
		{
			get { return kvBindable; }
			set
			{
				if (value < kVMin)
					kvBindable = kVMin;
				else if (value > kVMax)
					kvBindable = kVMax;
				else
					kvBindable = value;

				Set(ref kvBindable, value);
				//
				//OnPropertyChanged(() => KVBindable);
				tmrSetkV.Change(ChangeValueBeforeSetInterval, System.Threading.Timeout.Infinite);
				XraySettings.XrayInfokVp = kvBindable;
			}
		}

		private double ma = 0d;
		public double MA
		{
			get { return ma; }
			set
			{
				Set(ref ma, value);
				XraySettings.XrayInfomA = ma;
			}
		}

		private double maBindable = 0.5d;
		public double MABindable
		{
			get { return maBindable; }
			set
			{
				if (value < mAMin)
					maBindable = mAMin;
				else if (value > mAMax)
					maBindable = mAMax;
				else
					maBindable = value;
				OnPropertyChanged("MABindable");
				tmrSetua.Change(ChangeValueBeforeSetInterval, System.Threading.Timeout.Infinite);
				XraySettings.XrayInfomA = maBindable;
			}
		}


		private bool xrayState = true;
		public bool XrayState
		{
			get { return xrayState; }
			set
			{
				Set(ref xrayState, value);
				XRayOnCommand.RaiseCanExecuteChanged();
				XRayOffCommand.RaiseCanExecuteChanged();
			}
		}


		public bool ControlsEnabled
		{
			get { return (PortStatus == PortStatuses.Connected) && (DeviceStatus == MainStatuses.Standby || DeviceStatus == MainStatuses.Emitted || DeviceStatus == MainStatuses.EmittedPulse); }
		}

		public bool ResetFaultsEnabled
		{
			get { return (PortStatus == PortStatuses.Connected) && (Arc || Interlock || OverTemp || OverVoltage || UnderVoltage || OverCurrent || UnderCurrent); }
		}


		private bool emitted;
		public bool Emitted
		{
			get { return emitted; }
			set { Set(ref emitted, value); }
		}

		private bool arc;
		public bool Arc
		{
			get { return arc; }
			set { Set(ref arc, value); }
		}

		private bool interlock;
		public bool Interlock
		{
			get { return interlock; }
			set { Set(ref interlock, value); }
		}

		private bool overTemp;
		public bool OverTemp
		{
			get { return overTemp; }
			set { Set(ref overTemp, value); }
		}

		private bool overVoltage;
		public bool OverVoltage
		{
			get { return overVoltage; }
			set { Set(ref overVoltage, value); }
		}

		private bool underVoltage;
		public bool UnderVoltage
		{
			get { return underVoltage; }
			set { Set(ref underVoltage, value); }
		}

		private bool overCurrent;
		public bool OverCurrent
		{
			get { return overCurrent; }
			set { Set(ref overCurrent, value); }
		}

		private bool underCurrent;
		public bool UnderCurrent
		{
			get { return underCurrent; }
			set { Set(ref underCurrent, value); }
		}

		private bool inTimeout;
		public bool InTimeout
		{
			get { return inTimeout; }
			set { Set(ref inTimeout, value); }
		}


		private bool watchdog;
		public bool Watchdog
		{
			get { return watchdog; }
			set
			{
				Set(ref watchdog, value);
				OnPropertyChanged(nameof(WatchdogBindable));
			}
		}

		public bool WatchdogBindable
		{
			get { return watchdog; }
			set
			{
				Watchdog = value;
				if (value)
					SendToPort(WatchdogOff);
				else
					SendToPort(WatchdogOn);
			}
		}


		#endregion

		#region Libra
		public const string WatchdogOff = "WDTE 0";
		public const string WatchdogOn = "WDTE 1";
		public const string XrayOff = "ENBL 0";
		public const string XrayOn = "ENBL 1";
		public const string KVSet = "VREF";
		public const string MASet = "IREF";
		public const string ResetFaults = "CLR";

		public const string msgCommandError = "Command error.";

		private RS232 sp232;

		public bool PortBusy;
		public bool WaitResponse;
		public string LastCommand;
		public string LastError;
		public Queue<string> Commands;

		public void ConnectToDevice()
		{
			Commands.Clear();
			WaitResponse = false;
			Commands.Enqueue("HWVR");
			Commands.Enqueue("FREV");
			Commands.Enqueue("SOFT");
			Commands.Enqueue("SLVR");
			Commands.Enqueue("SLIR");
			Commands.Enqueue(KVSet + " " + Convert.ToInt32(kvBindable * 4095.0 / kVMax).ToString());
			Commands.Enqueue(MASet + " " + Convert.ToInt32(maBindable * 4095.0 / mAMax).ToString());

			bool b = SendToPort("MODR");
			if (!b)
			{
				XraySettings.Log(LastError);
				tmrConnectionTimeout.Change(100, Timeout.Infinite);
			}
		}

		public bool SendToPort(string Command, bool Urgent = false)
		{
			if (sp232 == null)
				return false;

			int i = 0;
			LastError = "";
			if (!PortBusy && !WaitResponse)
			{
				PortBusy = true;
				LastCommand = Command;
				WaitResponse = true;
				i = sp232.SendToPort("\x02" + Command + ";" + Calculate(Command + ";"));
				if (i != 1)
				{
					LastError = RS232.GetTextException(i);
				}
				tmrDeviceReconnection.Change(100, Timeout.Infinite);
				PortBusy = false;
			}
			else
			{
				Commands.Enqueue(Command);
				if (Urgent)
				{
					int n = Commands.Count - 1;
					for (int j = 0; j < n; j++)
					{
						Commands.Enqueue(Commands.Dequeue());
					}
				}
			}
			return i == 1;
		}


		private bool OpenPort()
		{
			PortBusy = true;
			int i = sp232.OpenPort();
			if (i != 1)
			{
				LastError = RS232.GetTextException(i);
			}
			PortBusy = false;
			return i == 1;
		}

		private void ClosePort()
		{
			tmrDeviceReconnection.Change(Timeout.Infinite, Timeout.Infinite);
			tmrPortReconnection.Change(Timeout.Infinite, Timeout.Infinite);
			PortBusy = true;
			sp232.ClosePort();
			PortBusy = false;
			PortStatus = PortStatuses.Closed;
		}

		public void ConnectToPort()
		{
			bool b = OpenPort();
			if (b)
			{
				PortStatus = PortStatuses.Open;
				ConnectToDevice();
			}
			else
			{
				PortStatus = PortStatuses.Error;
				XraySettings.Log(LastError);
				tmrPortReconnection.Change(100, Timeout.Infinite);
			}
			EnableControls();
		}


		private void ClearValues()
		{
			WaitResponse = false;
			LastCommand = "";
			LastError = "";
			Commands.Clear();

			DeviceStatus = MainStatuses.Disconnected;
			fil = 0.0;
			temperature = 0.0;

			Emitted = false;
			Arc = false;
			Interlock = false;
			OverTemp = false;
			OverVoltage = false;
			UnderVoltage = false;
			OverCurrent = false;
			UnderCurrent = false;
			Watchdog = false;
			InTimeout = false;
		}

		public void Connect()
		{
			sp232 = new RS232();
			sp232.DataReceived += ReceivedFromPort;
			var sps = sp232.GetSettings();

			sps.PortName = Settings.XrayCOM;
			sps.BaudRate = Settings.XrayBaudRate;
			sps.DataBits = 8;
			sps.Parity = System.IO.Ports.Parity.None;
			sps.StopBits = System.IO.Ports.StopBits.One;
			sps.NewLine = "\r";
			sp232.SetSettings(sps);
			sp232.ReadingMode = ReadingModes.Text;

			RSStatusString = sp232.PortSettingsToStringShort();
			PortStatus = PortStatuses.Closed;

			PortBusy = false;
			Commands = new Queue<string>();

			ClearValues();

			EnableControls();

			ConnectToPort();

			//SetkV(null);
			//SetuA(null);
		}

		public void Disconnect()
		{
			PortStatus = PortStatuses.Open;
			ClearValues();
			DisposeRS232();
		}

		public void DisposeRS232()
		{
			if (DeviceStatus != MainStatuses.Disconnected)
				StopEmission();
			if (PortStatus == PortStatuses.Connected || PortStatus == PortStatuses.Open)
				ClosePort();
			sp232.DataReceived -= ReceivedFromPort;
		}

		public void StartEmission()
		{
			SendToPort(MASet + " " + Convert.ToInt32(maBindable * 4095.0 / mAMax).ToString());
			if (DeviceStatus == MainStatuses.Standby)
				SendToPort(XrayOn, true);
			tmrReadStatus.Change(500, Timeout.Infinite);
		}

		public void StopEmission()
		{
			if (DeviceStatus == MainStatuses.Emitted || DeviceStatus == MainStatuses.EmittedPulse)
				SendToPort(XrayOff, true);
			tmrReadStatus.Change(500, Timeout.Infinite);
		}

		public void RumpUpDelay()
		{
			var warmUpTime = XraySettings.XrayRumpUpTime;
			Thread.Sleep(warmUpTime);
		}


		public Color clrDisconnected = Colors.DarkGray;
		public Color clrMainStatus0 = Colors.DarkGray;
		public Color clrMainStatus1 = Colors.Green;
		public Color clrMainStatus2 = Colors.Lime;
		public Color clrMainStatus3 = Colors.Lime;
		public Color clrMainStatus4 = Colors.Red;
		public Color clrStatusInfo = Colors.White;
		public Color clrStatusError = Colors.Red;
		public Color clrTubeControl = Colors.DarkGreen;
		public Color clrTubePreset = Colors.Gray;


		public double fil;
		public double temperature;



		public void EnableControls()
		{
			OnPropertyChanged(nameof(ControlsEnabled));
			OnPropertyChanged(nameof(ResetFaultsEnabled));
		}


		private char Calculate(string text)
		{
			int x = 0;
			for (int i = 0; i < text.Length; i++)
			{
				x += text[i];
			}
			x = x ^ 0xFFFF;
			x += 1;
			return (char)((x & 0x7F) | 0x40);
		}

		private bool CheckResponse(ref string text)
		{
			bool rs = false;
			if (text.Length > 1 && text[0] == '\x02')
			{
				text = text.Substring(1);
				int p = text.IndexOf(';');
				if (p >= 0 && text.Length > p + 1)
				{
					rs = text[p + 1] == Calculate(text.Substring(0, p + 1));
					text = text.Substring(0, p);
				}
			}
			return rs;
		}


		public void ReceivedFromPort(string Message)
		{
			tmrDeviceReconnection.Change(Timeout.Infinite, Timeout.Infinite);
			if (WaitResponse)
			{
				if (CheckResponse(ref Message))
				{
					if (LastCommand == "STAT" && Message.Length > 0)
					{
						DeviceStatus = Message == "1" ? MainStatuses.Emitted : MainStatuses.Standby;
					}
					else if (LastCommand == "FLT" && Message.Length == 8)
					{
						Arc = Message[0] == '1';
						OverTemp = Message[1] == '1';
						OverVoltage = Message[2] == '1';
						UnderVoltage = Message[3] == '1';
						OverCurrent = Message[4] == '1';
						UnderCurrent = Message[5] == '1';
						InTimeout = Message[6] == '1';
						Interlock = Message[7] == '1';
					}
					else if (LastCommand == "VMON" && Message.Length > 0)
					{
						double d = Convert.ToDouble(Message);
						KV = d * kVMax / 4095.0;
					}
					else if (LastCommand == "IMON" && Message.Length > 0)
					{
						double d = Convert.ToDouble(Message);
						MA = d * mAMax / 4095.0;
					}
					else if (LastCommand == "FMON" && Message.Length > 0)
					{
						fil = Convert.ToDouble(Message);
					}
					else if (LastCommand == "TEMP" && Message.Length > 0)
					{
						double d = Convert.ToDouble(Message);
						temperature = d * mAMax / 956.0;
					}
					else if (LastCommand == "VSET" && Message.Length > 0)
					{
						double d = Convert.ToDouble(Message);
						//kVPreset = d * kVMax / 4095.0;
					}
					else if (LastCommand == "ISET" && Message.Length > 0)
					{
						double d = Convert.ToDouble(Message);
						//uAPreset = d * uAMax / 4095.0;
					}
					else if (LastCommand.StartsWith("ENBL") && Message == "")
					{
						DeviceStatus = LastCommand[5] == '1' ? MainStatuses.Emitted : MainStatuses.Standby;
					}
					else if (LastCommand.StartsWith("WDTE") && Message == "")
					{
						Watchdog = LastCommand[5] == '1';
					}
					else if (LastCommand.StartsWith("VREF") && Message == "")
					{
						double d = Convert.ToDouble(LastCommand.Substring(5));
						//kVPreset = d * kVMax / 4095.0;
					}
					else if (LastCommand.StartsWith("IREF") && Message == "")
					{
						double d = Convert.ToDouble(LastCommand.Substring(5));
						//uAPreset = d * uAMax / 4095.0;
					}
					else if (LastCommand == "SLVR" && Message.Length > 0)
					{
						kVMax = Convert.ToDouble(Message) / 100.0;
						//nudkVSet.Maximum = Convert.ToDecimal(kVMax);
					}
					else if (LastCommand == "SLIR" && Message.Length > 0)
					{
						mAMax = Convert.ToDouble(Message) / 1000;
						//nuduASet.Maximum = Convert.ToDecimal(uAMax);
					}
					else if (LastCommand == "MODR")
					{
						//Device = Message;
						PortStatus = PortStatuses.Connected;
						DeviceStatus = MainStatuses.Standby;
					}
					else if (LastCommand == "FREV")
					{
						XraySettings.Log("Software version: " + Message);
					}
					else if (LastCommand == "HWVR")
					{
						XraySettings.Log("Hardware version: " + Message);
					}
					else if (LastCommand == "SOFT")
					{
						XraySettings.Log("Software build version: " + Message);
					}
				}
				WaitResponse = false;
			}
			EnableControls();
			if (Commands.Count > 0)
			{
				SendToPort(Commands.Dequeue());
			}
			else if (PortStatus == PortStatuses.Connected)
			{
				tmrReadStatus.Change(500, Timeout.Infinite);
			}
		}



		#endregion



		#region Timers


		private Timer tmrBeep;
		private Timer tmrConnectionTimeout;
		private Timer tmrDeviceReconnection;
		private Timer tmrPortReconnection;
		private Timer tmrReadStatus;
		private Timer tmrSetkV;
		private Timer tmrSetua;
		private void InitializeTimers()
		{
			tmrBeep = new Timer(BeepCallback, null, 0, 1000);
			tmrConnectionTimeout = new Timer(ConnectionTimeoutCallback);
			tmrDeviceReconnection = new Timer(DeviceReconnectionCallback);
			tmrPortReconnection = new Timer(PortReconnectionCallback);
			tmrReadStatus = new Timer(ReadStatusCallback, null, 1000, Timeout.Infinite);
			tmrSetkV = new Timer(SetkV);
			tmrSetua = new Timer(SetuA);
		}

		private void BeepCallback(object state)
		{
			if (DeviceStatus == MainStatuses.Emitted || DeviceStatus == MainStatuses.EmittedPulse)
				ThreadPool.QueueUserWorkItem(o => Console.Beep(800, 100));
		}


		private void ConnectionTimeoutCallback(object state)
		{
			Disconnect();
			EnableControls();
			ConnectToDevice();
		}

		private void PortReconnectionCallback(object state)
		{
			ConnectToPort();
		}

		private void DeviceReconnectionCallback(object state)
		{
			ConnectToDevice();
		}

		private void ReadStatusCallback(object state)
		{
			if (PortStatus != PortStatuses.Connected)
				return;
			Commands.Enqueue("FLT");
			Commands.Enqueue("VMON");
			Commands.Enqueue("IMON");
			Commands.Enqueue("FMON");
			Commands.Enqueue("TEMP");
			Commands.Enqueue("VSET");
			Commands.Enqueue("ISET");

			if (Watchdog)
				Commands.Enqueue("WDTT");
			SendToPort("STAT");
		}


		public void SetkV(object state)
		{
			int i = Convert.ToInt32(kvBindable * 4095.0 / kVMax);
			string s = i.ToString();
			SendToPort(KVSet + " " + s);
		}

		public void SetuA(object state)
		{
			int i = Convert.ToInt32(maBindable * 4095.0 / mAMax);
			string s = i.ToString();
			SendToPort(MASet + " " + s);
		}

		#endregion



		#region Commands
		public DelegateCommand ConnectCommand { get; private set; }
		public DelegateCommand DisconnectCommand { get; private set; }
		public DelegateCommand XRayOnCommand { get; private set; }
		public DelegateCommand XRayOffCommand { get; private set; }
		public DelegateCommand ResetCommand { get; private set; }
		public DelegateCommand ResetFaultsCommand { get; private set; }
		public DelegateCommand SettingsCommand { get; private set; }


		public void InitializeCommands()
		{
			//SettingsCommand = new DelegateCommand(OnSettings);

			ConnectCommand = new DelegateCommand(Connect, () => DeviceStatus == MainStatuses.Disconnected);
			DisconnectCommand = new DelegateCommand(Disconnect, () => DeviceStatus != MainStatuses.Disconnected);

			XRayOnCommand = new DelegateCommand(StartEmission);
			XRayOffCommand = new DelegateCommand(StopEmission);

			ResetCommand = new DelegateCommand(StartLibraXRay, () => true);
			ResetFaultsCommand = new DelegateCommand(OnResetFaults, () => true);
		}

		public void OnResetFaults()
		{
			SendToPort(ResetFaults);
		}

		//private bool xrayReset;
		public void StartLibraXRay()
		{
			if (DeviceStatus != MainStatuses.Disconnected)
				Disconnect();
			Connect();
		}



		#endregion
		public LibraDeviceModel()
		{
			InitializeCommands();
			InitializeTimers();
			//XraySettings =  new XraySettings();
			//kvBindable = XraySettings.XrayInfoSet.kVp;
			//maBindable = XraySettings.XrayInfoSet.mA;

			//Service.GetEvent<ApplicationSessionEnded>().Subscribe(o => StopEmission());
		}
	}

	public class XraySettings
    {
		private const int minRumpUpTime = 1;
		private const int MaxRumpUpTime = 60000;

		public double XrayInfokVp;
		public double XrayInfomA;

		private int xrayRumpUpTime = 1500;
		public int XrayRumpUpTime
		{
			get { return xrayRumpUpTime; }
			set
			{

				xrayRumpUpTime = value;
				if (xrayRumpUpTime > MaxRumpUpTime)
					xrayRumpUpTime = MaxRumpUpTime;
				else if (xrayRumpUpTime < minRumpUpTime)
					xrayRumpUpTime = minRumpUpTime;
				//OnPropertyChanged(() => XrayRumpUpTime);
				xrayRumpUpTime = value;
			}
		}

		public ObservableCollection<string> LogStrings { get; set; } = new ObservableCollection<string>();

		public void Log(string message)
		{
			try
			{
				LogStrings.Insert(0, DateTime.Now.ToString("[MM/dd/yyyy HH:mm:ss] ") + message);
				if (LogStrings.Count > 100)
					LogStrings.RemoveAt(LogStrings.Count - 1);
			}
			catch
			{

			}

		}
	}
}

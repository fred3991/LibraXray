using System;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Windows;

namespace LibraXray.Commands.Libra
{
	public enum ReadingModes
	{
		Text = 0,
		Byte
	}

	public struct SerialPortSettings
	{
		public string PortName;
		public int BaudRate;
		public int DataBits;
		public Parity Parity;
		public StopBits StopBits;
		public Handshake Handshake;
		public Encoding Encoding;
		public string NewLine;
		public bool DtrEnable;
		public bool RtsEnable;
		public bool DiscardNull;
		public byte ParityReplace;
		public int ReadTimeout;
		public int ReceivedBytesThreshold;
		public int WriteBufferSize;
		public int WriteTimeout;

	}

	public class RS232
	{
		private const string Ex1InvalidOperation = "COM Port is already open.";
		private const string Ex2ArgumentOutOfRange = "One or more of the properties are invalid.";
		private const string Ex3Argument = "The port name does not begin with 'COM'.";
		private const string Ex4IO = "The port is in an invalid state.";
		private const string Ex5UnauthorizedAccess = "Access is denied to the port.";
		private const string Ex6ArgumentNull = "The message parameter is null.";
		private const string Ex7InvalidOperation = "The port is not open.";
		private const string Ex8Timeout = "Message could not write to the port.";
		private const string Ex9ArgumentOutOfRangeException = "The parameters are outside a valid region.";
		private const string Ex10ArgumentException = "The parameters are invalid.";
		//private const string  = "";

		private const int ReadingBufferMaxSize = 10000;


		private SerialPort f_sp;
		private SerialPortSettings f_sps;
		private ReadingModes f_rm;
		public event DataReceivedFunc DataReceived;


		//public int[] BaudRates = { 50, 75, 110, 150, 300, 600, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 };
		public delegate void DataReceivedFunc(string text);


		public RS232()
		{
			f_sp = new SerialPort();
			f_sp.DataReceived += sp_DataReceived;

			f_sps = new SerialPortSettings();
			f_sps.PortName = f_sp.PortName;
			f_sps.BaudRate = f_sp.BaudRate;
			f_sps.DataBits = f_sp.DataBits;
			f_sps.Parity = f_sp.Parity;
			f_sps.StopBits = f_sp.StopBits;
			f_sps.Handshake = f_sp.Handshake;
			f_sps.Encoding = f_sp.Encoding;
			f_sps.NewLine = f_sp.NewLine;
			f_sps.DtrEnable = f_sp.DtrEnable;
			f_sps.RtsEnable = f_sp.RtsEnable;
			f_sps.DiscardNull = f_sp.DiscardNull;
			f_sps.ParityReplace = f_sp.ParityReplace;
			f_sps.ReadTimeout = f_sp.ReadTimeout;
			f_sps.ReceivedBytesThreshold = f_sp.ReceivedBytesThreshold;
			f_sps.WriteBufferSize = f_sp.WriteBufferSize;
			f_sps.WriteTimeout = f_sp.WriteTimeout;

			f_rm = ReadingModes.Text;
		}

		public ReadingModes ReadingMode
		{
			get { return f_rm; }
			set { f_rm = value; }
		}

		public SerialPortSettings GetSettings()
		{
			SerialPortSettings rs = f_sps;
			return rs;
		}

		public void SetSettings(SerialPortSettings Settings)
		{
			f_sps = Settings;
			if (!f_sp.IsOpen)
			{
				f_sp.PortName = f_sps.PortName;
				f_sp.BaudRate = f_sps.BaudRate;
				f_sp.DataBits = f_sps.DataBits;
				f_sp.Parity = f_sps.Parity;
				f_sp.StopBits = f_sps.StopBits;
				f_sp.Handshake = f_sps.Handshake;
				f_sp.Encoding = f_sps.Encoding;
				f_sp.NewLine = f_sps.NewLine != "" ? f_sps.NewLine : "\r";
				f_sp.DtrEnable = f_sps.DtrEnable;
				f_sp.RtsEnable = f_sps.RtsEnable;
				f_sp.DiscardNull = f_sps.DiscardNull;
				f_sp.ParityReplace = f_sps.ParityReplace;
				f_sp.ReadTimeout = f_sps.ReadTimeout;
				f_sp.ReceivedBytesThreshold = f_sps.ReceivedBytesThreshold;
				f_sp.WriteBufferSize = f_sps.WriteBufferSize;
				f_sp.WriteTimeout = f_sps.WriteTimeout;
			}
			else
			{
				ClosePort();
				f_sp.PortName = f_sps.PortName;
				f_sp.BaudRate = f_sps.BaudRate;
				f_sp.DataBits = f_sps.DataBits;
				f_sp.Parity = f_sps.Parity;
				f_sp.StopBits = f_sps.StopBits;
				f_sp.Handshake = f_sps.Handshake;
				f_sp.Encoding = f_sps.Encoding;
				f_sp.NewLine = f_sps.NewLine != "" ? f_sps.NewLine : "\r";
				f_sp.DtrEnable = f_sps.DtrEnable;
				f_sp.RtsEnable = f_sps.RtsEnable;
				f_sp.DiscardNull = f_sps.DiscardNull;
				f_sp.ParityReplace = f_sps.ParityReplace;
				f_sp.ReadTimeout = f_sps.ReadTimeout;
				f_sp.ReceivedBytesThreshold = f_sps.ReceivedBytesThreshold;
				f_sp.WriteBufferSize = f_sps.WriteBufferSize;
				f_sp.WriteTimeout = f_sps.WriteTimeout;
				OpenPort();
			}
		}

		public void ShowSettings()
		{
			if (!f_sp.IsOpen)
			{
				//RS232SettingsWF SettingsForm = new RS232SettingsWF();
				//SettingsForm.f_sps = f_sps;
				//if (SettingsForm.ShowDialog() == DialogResult.OK)
				//{
				//	f_sps = SettingsForm.f_sps;
				//}
				//SettingsForm.Close();

				f_sp.PortName = f_sps.PortName;
				f_sp.BaudRate = f_sps.BaudRate;
				f_sp.DataBits = f_sps.DataBits;
				f_sp.Parity = f_sps.Parity;
				f_sp.StopBits = f_sps.StopBits;
				f_sp.Handshake = f_sps.Handshake;
				f_sp.Encoding = f_sps.Encoding;
				f_sp.NewLine = f_sps.NewLine != "" ? f_sps.NewLine : "\r";
				f_sp.DtrEnable = f_sps.DtrEnable;
				f_sp.RtsEnable = f_sps.RtsEnable;
				f_sp.DiscardNull = f_sps.DiscardNull;
				f_sp.ParityReplace = f_sps.ParityReplace;
				f_sp.ReadTimeout = f_sps.ReadTimeout;
				f_sp.ReceivedBytesThreshold = f_sps.ReceivedBytesThreshold;
				f_sp.WriteBufferSize = f_sps.WriteBufferSize;
				f_sp.WriteTimeout = f_sps.WriteTimeout;
			}
			else
			{
				MessageBox.Show("Port is open.");
			}
		}

		public void ShowSettingsWhite()
		{
			if (!f_sp.IsOpen)
			{
				//RS232SettingsWFWhite SettingsForm = new RS232SettingsWFWhite();
				//SettingsForm.f_sps = f_sps;
				//if (SettingsForm.ShowDialog() == DialogResult.OK)
				//{
				//	f_sps = SettingsForm.f_sps;
				//}
				//SettingsForm.Close();

				f_sp.PortName = f_sps.PortName;
				f_sp.BaudRate = f_sps.BaudRate;
				f_sp.DataBits = f_sps.DataBits;
				f_sp.Parity = f_sps.Parity;
				f_sp.StopBits = f_sps.StopBits;
				f_sp.Handshake = f_sps.Handshake;
				f_sp.Encoding = f_sps.Encoding;
				f_sp.NewLine = f_sps.NewLine != "" ? f_sps.NewLine : "\r";
				f_sp.DtrEnable = f_sps.DtrEnable;
				f_sp.RtsEnable = f_sps.RtsEnable;
				f_sp.DiscardNull = f_sps.DiscardNull;
				f_sp.ParityReplace = f_sps.ParityReplace;
				f_sp.ReadTimeout = f_sps.ReadTimeout;
				f_sp.ReceivedBytesThreshold = f_sps.ReceivedBytesThreshold;
				f_sp.WriteBufferSize = f_sps.WriteBufferSize;
				f_sp.WriteTimeout = f_sps.WriteTimeout;
			}
			else
			{
				MessageBox.Show("Port is open.");
			}
		}

		public int OpenPort()
		{
			int rs = 1;
			try
			{
				ClosePort();
				f_sp.Open();
			}
			catch (InvalidOperationException)
			{
				rs = -1;
			}
			catch (ArgumentOutOfRangeException)
			{
				rs = -2;
			}
			catch (ArgumentException)
			{
				rs = -3;
			}
			catch (IOException)
			{
				rs = -4;
			}
			catch (UnauthorizedAccessException)
			{
				rs = -5;
			}
			return rs;
		}

		public void ClosePort()
		{
			if (f_sp.IsOpen)
			{
				f_sp.DiscardOutBuffer();
				f_sp.DiscardInBuffer();
				f_sp.Close();
				int i = 0;
				while (f_sp.IsOpen && (i < 50))
				{
					Thread.Sleep(100);
					i++;
				}
			}
		}

		public int SendToPort(string Message)
		{
			int rs = 1;
			try
			{
				f_sp.WriteLine(Message);
			}
			catch (ArgumentNullException)
			{
				rs = -6;
			}
			catch (InvalidOperationException)
			{
				rs = -7;
			}
			catch (TimeoutException)
			{
				rs = -8;
			}
			return rs;
		}

		public int SendToPort(byte[] buffer)
		{
			int rs = 1;
			try
			{
				f_sp.Write(buffer, 0, buffer.Length);
			}
			catch (ArgumentNullException)
			{
				rs = -6;
			}
			catch (InvalidOperationException)
			{
				rs = -7;
			}
			catch (TimeoutException)
			{
				rs = -8;
			}
			catch (ArgumentOutOfRangeException)
			{
				rs = -9;
			}
			catch (ArgumentException)
			{
				rs = -10;
			}
			return rs;
		}

		public static string GetTextException(int i)
		{
			string rs = "";
			switch (i)
			{
				case -1:
					rs = Ex1InvalidOperation;
					break;
				case -2:
					rs = Ex2ArgumentOutOfRange;
					break;
				case -3:
					rs = Ex3Argument;
					break;
				case -4:
					rs = Ex4IO;
					break;
				case -5:
					rs = Ex5UnauthorizedAccess;
					break;
				case -6:
					rs = Ex6ArgumentNull;
					break;
				case -7:
					rs = Ex7InvalidOperation;
					break;
				case -8:
					rs = Ex8Timeout;
					break;
				case -9:
					rs = Ex9ArgumentOutOfRangeException;
					break;
				case -10:
					rs = Ex10ArgumentException;
					break;
			}
			return rs;
		}

		public static string[] PortNames()
		{
			return SerialPort.GetPortNames();
		}

		public string PortSettingsToString()
		{
			string rs = "";
			string pt = "N";
			if (f_sps.Parity == Parity.Even)
			{
				pt = "E";
			}
			else if (f_sps.Parity == Parity.Mark)
			{
				pt = "M";
			}
			else if (f_sps.Parity == Parity.Odd)
			{
				pt = "O";
			}
			else if (f_sps.Parity == Parity.Space)
			{
				pt = "S";
			}
			else
			{
				pt = "N";
			}
			string sb = "1";
			if (f_sps.StopBits == StopBits.One)
			{
				sb = "1";
			}
			else if (f_sps.StopBits == StopBits.OnePointFive)
			{
				sb = "1.5";
			}
			else if (f_sps.StopBits == StopBits.Two)
			{
				sb = "2";
			}
			else
			{
				sb = "N";
			}
			string hs = "N";
			if (f_sps.Handshake == Handshake.RequestToSend)
			{
				hs = "R";
			}
			else if (f_sps.Handshake == Handshake.RequestToSendXOnXOff)
			{
				hs = "RX";
			}
			else if (f_sps.Handshake == Handshake.XOnXOff)
			{
				hs = "X";
			}
			else
			{
				hs = "N";
			}
			rs = f_sps.PortName + ";" + f_sps.BaudRate.ToString() + ";" + f_sps.DataBits.ToString() + ";" + pt + ";" + sb + ";" + hs;
			return rs;
		}

		public string PortSettingsToStringShort()
		{
			string rs = "";
			string pt = "N";
			if (f_sps.Parity == Parity.Even)
			{
				pt = "E";
			}
			else if (f_sps.Parity == Parity.Mark)
			{
				pt = "M";
			}
			else if (f_sps.Parity == Parity.Odd)
			{
				pt = "O";
			}
			else if (f_sps.Parity == Parity.Space)
			{
				pt = "S";
			}
			else
			{
				pt = "N";
			}
			string sb = "1";
			if (f_sps.StopBits == StopBits.One)
			{
				sb = "1";
			}
			else if (f_sps.StopBits == StopBits.OnePointFive)
			{
				sb = "1.5";
			}
			else if (f_sps.StopBits == StopBits.Two)
			{
				sb = "2";
			}
			else
			{
				sb = "N";
			}
			rs = f_sps.PortName + "  " + f_sps.BaudRate.ToString() + ";" + f_sps.DataBits.ToString() + ";" + pt + ";" + sb;
			return rs;
		}

		public string PortName
		{
			get { return f_sp.PortName; }
		}

		private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			if (DataReceived != null)
			{
				if (f_rm == ReadingModes.Text)
				{
					Thread.Sleep(5);
					try
					{
						string data = f_sp.ReadLine();
						DataReceived(data);
					}
					catch (TimeoutException)
					{ }
					catch (InvalidOperationException)
					{ }
				}
				else
				{
					byte[] buffer = new byte[ReadingBufferMaxSize];
					int i = 0;
					while (i < ReadingBufferMaxSize && f_sp.BytesToRead > 0)
					{
						int l = f_sp.BytesToRead;
						try
						{
							int n = f_sp.Read(buffer, i, l);
							i += n;
						}
						catch (ArgumentNullException)
						{ }
						catch (InvalidOperationException)
						{ }
						catch (ArgumentOutOfRangeException)
						{ }
						catch (ArgumentException)
						{ }
						catch (TimeoutException)
						{ }
						Thread.Sleep(10);
					}
					string data = "";
					for (int j = 0; j < i; j++)
					{
						data += (char)buffer[j];
					}
					DataReceived(data);
				}
			}
		}

	}
}

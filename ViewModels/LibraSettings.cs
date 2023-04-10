using System.ComponentModel;
using System.IO;


namespace LibraXray.ViewModels
{
    internal class LibraSettings : ViewModel
    {
		private readonly string settingsPath = Path.Combine(/*App.GetPublicDir(),*/ "SpellmanSettings.xml");
		private static bool isLoaded;
		//private bool isLoading;

		private int hvDuration = 1000;
		public int HVDuration
		{
			get { return hvDuration; }
			set { Set(ref hvDuration, value); }
		}

		private string xrayCOM = "COM1";
		public string XrayCOM
		{
			get { return xrayCOM; }
			set { Set(ref xrayCOM, value); }
		}

		private int xrayBaudRate = 115200;
		public int XrayBaudRate
		{
			get { return xrayBaudRate; }
			set { Set(ref xrayBaudRate, value); }
		}

		private void Load()
		{
			if (isLoaded)
				return;
			isLoaded = true;
			//isLoading = true;
			if (File.Exists(settingsPath))
			{
				////var settings = XmlHelper.FromXDocument<SpellmanSettings>(settingsPath);
				//XrayCOM = settings.XrayCOM;
				//XrayBaudRate = settings.XrayBaudRate;
				//HVDuration = settings.HVDuration;
			}
			//isLoading = false;
			PropertyChanged += (s, e) => Save();
		}

		public void Save()
		{
			//this.ToXDocument(settingsPath);
		}

		public LibraSettings()
		{
			Load();
		}
	}
}

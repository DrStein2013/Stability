using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Stability.Model.Port;

namespace Stability
{
    /// <summary>
    /// Реализует обработку файла конфигурации приложения
    /// </summary>
    public static class MainConfig
    {
        //private static CPortConfig _portConfig;
        public static CPortConfig PortConfig { get; private set; }

        static MainConfig()
        {
            PortConfig = new CPortConfig();
           if (ConfigurationManager.AppSettings.Count == 0)
                Init();

            Load();
        }

        public static void Update(CPortConfig config)
        {
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            currentConfig.AppSettings.Settings["PortName"].Value = config.PortName;
            currentConfig.AppSettings.Settings["Baud"].Value = config.Baud.ToString();
            currentConfig.AppSettings.Settings["AutoConnect"].Value = config.AutoConnect.ToString();
            currentConfig.AppSettings.Settings["UseSLIP"].Value = config.UseSLIP.ToString();
            currentConfig.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

        }

        private static void Load()
        {
            PortConfig.PortName = ConfigurationManager.AppSettings["PortName"];
            PortConfig.Baud = Convert.ToInt32(ConfigurationManager.AppSettings["Baud"]);
            PortConfig.AutoConnect = Convert.ToBoolean(ConfigurationManager.AppSettings["AutoConnect"]);
            PortConfig.UseSLIP = Convert.ToBoolean(ConfigurationManager.AppSettings["UseSLIP"]);
        }

        private static void Init()
        {
            string name;
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (!CComPort.FindPort("Stabilometric Device", out name))
                name = "COM1";

            currentConfig.AppSettings.Settings.Add("PortName", name);
            currentConfig.AppSettings.Settings.Add("Baud", 9600.ToString());
            currentConfig.AppSettings.Settings.Add("AutoConnect", true.ToString());
            currentConfig.AppSettings.Settings.Add("UseSLIP", true.ToString());

            currentConfig.Save(ConfigurationSaveMode.Full);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}

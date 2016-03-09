using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
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
        public static double[] WeightKoefs { get; private set; }
        public static double[] ZeroAdcVals { get; private set; }
        static MainConfig()
        {
            PortConfig = new CPortConfig();
            WeightKoefs = new double[4];
            ZeroAdcVals = new double[4];
           if (ConfigurationManager.AppSettings.Count == 0)
                Init();

            Load();
        }

        public static void Update(CPortConfig config, double[] weightKoefs, double [] zeroAdcVals)
        {
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config != null)
            {
                currentConfig.AppSettings.Settings["PortName"].Value = config.PortName;
                currentConfig.AppSettings.Settings["Baud"].Value = config.Baud.ToString();
                currentConfig.AppSettings.Settings["AutoConnect"].Value = config.AutoConnect.ToString();
                currentConfig.AppSettings.Settings["UseSLIP"].Value = config.UseSLIP.ToString();
            }
            if (weightKoefs != null)
                currentConfig.AppSettings.Settings["WeightKoefs"].Value = weightKoefs[0].ToString(CultureInfo.InvariantCulture) + "," +
                                                                          weightKoefs[1].ToString(CultureInfo.InvariantCulture) + "," +
                                                                          weightKoefs[2].ToString(CultureInfo.InvariantCulture) + "," +
                                                                          weightKoefs[3].ToString(CultureInfo.InvariantCulture);
            if (zeroAdcVals != null)
                currentConfig.AppSettings.Settings["ZeroAdcVals"].Value = zeroAdcVals[0].ToString(CultureInfo.InvariantCulture) + "," +
                                                                          zeroAdcVals[1].ToString(CultureInfo.InvariantCulture) + "," +
                                                                          zeroAdcVals[2].ToString(CultureInfo.InvariantCulture) + "," +
                                                                          zeroAdcVals[3].ToString(CultureInfo.InvariantCulture) ;

            currentConfig.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            Load();
        }

        private static void Load()
        {
            PortConfig.PortName = ConfigurationManager.AppSettings["PortName"];
            PortConfig.Baud = Convert.ToInt32(ConfigurationManager.AppSettings["Baud"]);
            PortConfig.AutoConnect = Convert.ToBoolean(ConfigurationManager.AppSettings["AutoConnect"]);
            PortConfig.UseSLIP = Convert.ToBoolean(ConfigurationManager.AppSettings["UseSLIP"]);


            var s = ConfigurationManager.AppSettings["WeightKoefs"].Split(',');
            var s1 = ConfigurationManager.AppSettings["ZeroAdcVals"].Split(',');

            for (int i = 0; i < WeightKoefs.Count(); i++)
            {
                WeightKoefs[i] = Convert.ToDouble(s[i],CultureInfo.InvariantCulture);
                ZeroAdcVals[i] = Convert.ToDouble(s1[i], CultureInfo.InvariantCulture);
            }
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

            currentConfig.AppSettings.Settings.Add("ZeroAdcVals","0,0,0,0");
            currentConfig.AppSettings.Settings.Add("WeightKoefs", "1.0,1.0,1.0,1.0");
            
            currentConfig.Save(ConfigurationSaveMode.Full);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}

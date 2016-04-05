using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Stability.Enums;
using Stability.Model.Device;
using Stability.Model.Port;

namespace Stability.Model
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
        public static StabilityExchangeConfig ExchangeConfig { get; private set; }
        //public static double[] AlphaBetaKoefs { get; private set; }
       // public static InputFilterType FilterType { get; private set; }

        static MainConfig()
        {
            PortConfig = new CPortConfig();
            WeightKoefs = new double[4];
            ZeroAdcVals = new double[4];
            ExchangeConfig = new StabilityExchangeConfig();
            //AlphaBetaKoefs = new double[4];

           if (ConfigurationManager.AppSettings.Count == 0)
                Init();

            Load();
        }

        public static void Update(CPortConfig config)
        {
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config != null)
            {
                currentConfig.AppSettings.Settings["PortName"].Value = config.PortName;
                currentConfig.AppSettings.Settings["Baud"].Value = config.Baud.ToString(CultureInfo.InvariantCulture);
                currentConfig.AppSettings.Settings["AutoConnect"].Value = config.AutoConnect.ToString();
                currentConfig.AppSettings.Settings["UseSLIP"].Value = config.UseSLIP.ToString();
            }

            currentConfig.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            Load();
        }

        public static void Update(StabilityExchangeConfig config)
        {
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config != null)
            {
                currentConfig.AppSettings.Settings["FilterType"].Value = config.FilterType.ToString();
                currentConfig.AppSettings.Settings["Period"].Value = config.Period.ToString(CultureInfo.InvariantCulture);
                currentConfig.AppSettings.Settings["SavePureADCs"].Value = config.SavePureADCs.ToString();
                currentConfig.AppSettings.Settings["CorrectRxMistakes"].Value = config.CorrectRxMistakes.ToString();

                currentConfig.AppSettings.Settings["AlphaBetaKoefs"].Value = config.AlphaBetaKoefs[0].ToString(CultureInfo.CreateSpecificCulture("en-GB")) + "," +
                                                                           config.AlphaBetaKoefs[1].ToString(CultureInfo.CreateSpecificCulture("en-GB")) + "," +
                                                                           config.AlphaBetaKoefs[2].ToString(CultureInfo.CreateSpecificCulture("en-GB")) + "," +
                                                                           config.AlphaBetaKoefs[3].ToString(CultureInfo.CreateSpecificCulture("en-GB"));
            }

            currentConfig.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            Load();            
        }

        public static void Update(/*CPortConfig config,*/ double[] weightKoefs, double [] zeroAdcVals)
        {
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            /*if (config != null)
            {
                currentConfig.AppSettings.Settings["PortName"].Value = config.PortName;
                currentConfig.AppSettings.Settings["Baud"].Value = config.Baud.ToString();
                currentConfig.AppSettings.Settings["AutoConnect"].Value = config.AutoConnect.ToString();
                currentConfig.AppSettings.Settings["UseSLIP"].Value = config.UseSLIP.ToString();
            }*/

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

            InputFilterType res;
            Enum.TryParse(ConfigurationManager.AppSettings["FilterType"],out res);
            ExchangeConfig.FilterType = res;
            ExchangeConfig.Period = Convert.ToInt32(ConfigurationManager.AppSettings["Period"]);
            ExchangeConfig.SavePureADCs = Convert.ToBoolean(ConfigurationManager.AppSettings["SavePureADCs"]);
            ExchangeConfig.CorrectRxMistakes = Convert.ToBoolean(ConfigurationManager.AppSettings["CorrectRxMistakes"]);

            var s = ConfigurationManager.AppSettings["WeightKoefs"].Split(',');
            var s1 = ConfigurationManager.AppSettings["ZeroAdcVals"].Split(',');
            var s2 = ConfigurationManager.AppSettings["AlphaBetaKoefs"].Split(',');
            for (int i = 0; i < WeightKoefs.Count(); i++)
            {
                WeightKoefs[i] = Convert.ToDouble(s[i],CultureInfo.InvariantCulture);
                ZeroAdcVals[i] = Convert.ToDouble(s1[i], CultureInfo.InvariantCulture);
                ExchangeConfig.AlphaBetaKoefs[i] = Convert.ToDouble(s2[i], CultureInfo.InvariantCulture);
            }
        }

        private static void Init()
        {
            string name;
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (!CComPort.FindPort("Stabilometric Device", out name))
                name = "COM1";

            currentConfig.AppSettings.Settings.Add("PortName", name);
            currentConfig.AppSettings.Settings.Add("Baud", 19200.ToString(CultureInfo.InvariantCulture));
            currentConfig.AppSettings.Settings.Add("AutoConnect", true.ToString());
            currentConfig.AppSettings.Settings.Add("UseSLIP", true.ToString());

            currentConfig.AppSettings.Settings.Add("ZeroAdcVals","0,0,0,0");
            currentConfig.AppSettings.Settings.Add("WeightKoefs", "1.0,1.0,1.0,1.0");
           
            currentConfig.AppSettings.Settings.Add("FilterType", InputFilterType.NoFilter.ToString());
            currentConfig.AppSettings.Settings.Add("Period", 50.ToString(CultureInfo.InvariantCulture));
            currentConfig.AppSettings.Settings.Add("SavePureADCs", false.ToString());
            currentConfig.AppSettings.Settings.Add("CorrectRxMistakes", false.ToString());
            currentConfig.AppSettings.Settings.Add("AlphaBetaKoefs", "1.0,1.0,1.0,1.0");
            
            currentConfig.Save(ConfigurationSaveMode.Full);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigSet.Configs
{
    public class Config : Configuration<Config>
    {
        public const string ConfFileName = "config.json";

        public bool IsNeedUpdate { get; set; }
        public bool SaveWindowSettings { get; set; }

        public string CertFullPath { get; set; }

        public string AbtDataBaseIpAddress { get; set; }
        public string AbtDataBaseSid { get; set; }

        public string EdiDataBaseIpAddress { get; set; }
        public string EdiDataBaseSid { get; set; }

        public DateTime? EdoLastDocDateTime { get; set; }
        public int EdoDocCount { get; set; }

        public bool ProxyEnabled { get; set; }
        public string ProxyAddress { get; set; }
        public string ProxyUserName { get; set; }
        public string ProxyUserPassword { get; set; }

        public string MailSmtpServerAddress { get; set; }
        public string MailUserLogin { get; set; }
        public string MailUserPassword { get; set; }
        public string MailUserEmailAddress { get; set; }
        public string MailErrorSubject { get; set; }

        [NonSerialized]
        private string _password = null;

        [NonSerialized]
        private static volatile Config _instance;

        [NonSerialized]
        private static readonly object syncRoot = new object();

        private Config() { }

        public static Config GetInstance()
        {
            // тут страндартный дабл-чек с блокировкой
            // для создания инстанса синглтона безопасно для многопоточности
            if (_instance == null)
            {
                lock (syncRoot)
                {
                    if (_instance == null)
                    {
                        // а тут паттерн пошёл по пизде.
                        // зато коротко и красиво
                        _instance = new Config().Load(ConfFileName);
                    }
                }
            }

            return _instance;
        }

        public string GetDataBaseUser()
        {
            return Properties.Settings.Default.DataBaseUser;
        }

        public string GetDataBasePassword()
        {
            if (_password == null)
            {
                var skitalaBytes = System.Text.Encoding.ASCII.GetBytes(Properties.Settings.Default.CipherDataBasePassword);
                var saltData = System.Text.Encoding.ASCII.GetBytes("uc*nwex^wgx#kriior&gcier+irerzqqp?wiqavb");

                int position = 6, shift = 7;
                var bytes = new byte[40];

                for (int j = 0; j < 8; j++)
                {
                    for (int k = 0; k < 5; k++)
                    {
                        bytes[j * 5 + k] = skitalaBytes[j + k * 8];
                    }
                }

                var passData = new System.Collections.Generic.List<byte>();
                int i = 0;
                while (saltData[(position + i * shift) % bytes.Length] != bytes[(position + i * shift) % bytes.Length])
                {
                    byte b;
                    if (saltData[(position + i * shift) % bytes.Length] > bytes[(position + i * shift) % bytes.Length])
                    {
                        b = (byte)(128 + (int)bytes[(position + i * shift) % bytes.Length] - (int)saltData[(position + i * shift) % bytes.Length]);
                    }
                    else
                    {
                        b = (byte)(bytes[(position + i * shift) % bytes.Length] - saltData[(position + i * shift) % bytes.Length]);
                    }
                    passData.Add(b);
                    i++;
                }
                _password = System.Text.Encoding.ASCII.GetString(passData.ToArray());
            }
            return _password;
        }
    }
}

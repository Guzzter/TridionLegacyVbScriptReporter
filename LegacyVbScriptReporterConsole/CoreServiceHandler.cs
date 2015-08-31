using System;
using System.IO;
using System.Reflection;
using System.Configuration;
using LegacyVbScriptReporterConsole.CoreService;
using System.ServiceModel;
using System.Xml;  // through adding the 'CoreService' Service Reference...
using Tridion.ContentManager.CoreService.Client;

// Handler code from:
// http://code.google.com/p/tridion-practice/wiki/GetCoreServiceClientWithoutConfigFile

namespace LegacyVbScriptReporterConsole
{
    public class CoreServiceHandler
    {
        private Configuration _Config;
        private CoreServiceClient _Tcmclient;
        private ICoreService2010 _Factory;

        public CoreServiceHandler(string configFilename)
        {
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            String filePath = Assembly.GetExecutingAssembly().CodeBase;
            filePath = filePath.Replace("file:///", String.Empty);
            filePath = filePath.Replace("/", "\\");

            fileMap.ExeConfigFilename = string.Format("{0}\\{1}", Path.GetDirectoryName(filePath), configFilename);
            _Config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
        }

        public string GetAppSetting(string key)
        {
            if (_Config != null && _Config.AppSettings != null && _Config.AppSettings.Settings[key] != null)
                return _Config.AppSettings.Settings[key].Value;

            return string.Empty;
        }

        public ICoreService2010 GetNewClient()
        {
            if (_Factory != null)
                return _Factory;

            if (_Config.AppSettings.Settings.Count == 0)
                throw new ArgumentException("Config file not found.  Pelase set property to 'Copy Always' and confirm name.");

            string hostname = GetAppSetting("hostname");
            string username = GetAppSetting("impersonation_user");
            string password = GetAppSetting("impersonation_password");
            
            var binding = new BasicHttpBinding()
            {
                MaxBufferSize = 4194304, // 4MB
                MaxBufferPoolSize = 4194304,
                MaxReceivedMessageSize = 4194304,
                ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas()
                {
                    MaxStringContentLength = 4194304, // 4MB
                    MaxArrayLength = 4194304,
                },
                Security = new BasicHttpSecurity()
                {
                    Mode = BasicHttpSecurityMode.TransportCredentialOnly,
                    Transport = new HttpTransportSecurity()
                    {
                        ClientCredentialType = HttpClientCredentialType.Windows                        
                    }
                }
            };
            hostname = string.Format("{0}{1}{2}", hostname.StartsWith("http") ? "" : "http://", hostname, hostname.EndsWith("/") ? "" : "/");
            var endpoint = new EndpointAddress(hostname + "/webservices/CoreService.svc/basicHttp_2010");
            ChannelFactory<ICoreService2010> factory = new ChannelFactory<ICoreService2010>(binding, endpoint);
            factory.Credentials.Windows.ClientCredential = new System.Net.NetworkCredential(username, password);
            _Factory = factory.CreateChannel();

            return _Factory;
        }

        public CoreServiceClient GetNewNetTcpClient()
        {
            string username = _Config.AppSettings.Settings["impersonation_user"].Value;
            string password = _Config.AppSettings.Settings["impersonation_password"].Value;
            string address = _Config.AppSettings.Settings["nettcpaddress"].Value;

            var binding = new NetTcpBinding
            {
                MaxReceivedMessageSize = 2147483647,
                ReaderQuotas = new XmlDictionaryReaderQuotas
                {
                    MaxStringContentLength = 2147483647,
                    MaxArrayLength = 2147483647
                }
            };
            var endpoint = new EndpointAddress(address);

            var client = new CoreServiceClient(binding, endpoint);
            client.ChannelFactory.Credentials.Windows.ClientCredential = new System.Net.NetworkCredential(username, password);
            try
            {
                client.GetApiVersion();
            }
            catch {
                return null;
            }

            return client;
        }

        /// <summary>
        /// Initializes the core service client.
        /// </summary>
        private void InitializeCoreServiceClient()
        {
            _Tcmclient = GetNewNetTcpClient();
        }
    }
}

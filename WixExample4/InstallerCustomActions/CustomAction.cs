using System;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Deployment.WindowsInstaller;

namespace InstallerCustomActions
{
    public class CustomActions
    {
        private const string InstallLocation = "INSTALLLOCATION";
        private const string ConfigFile = "ApplicationWindowsService.exe.config";
        private const string ServerPort = "SERVER_PORT";

        private const string DataServer = "DATA_SERVER";
        private const string DataDb = "DATA_DB";
        private const string DataLogin = "DATA_LOGIN";
        private const string DataPassword = "DATA_PASSWORD";

        private const int DefaultPort = 8888;


        [CustomAction]
        public static ActionResult ReadServerConfig(Session session)
        {
            session.Log("Begin ReadServerConfig");

            try
            {
                string installLocation = session[InstallLocation];
                session.Log("InstallLocation: [{0}]", installLocation);

                var root = ReadConfigRoot(session, installLocation);
                if (root == null)
                    return ActionResult.Success;

                int port = LoadPort(root);
                session[ServerPort] = port.ToString();
            }
            catch (Exception ex)
            {
                session.Log("Error occured on ReadServerConfig: {0}", ex.Message);
                return ActionResult.Failure;
            }

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult CheckDataConnection(Session session)
        {
            var connString = new ConnectionStringParts
            {
                Server = session[DataServer],
                Database = session[DataDb],
                User = session[DataLogin],
                Password = session[DataPassword]
            };

            session["DATACHECKED"] = CheckDbConnection(session, connString)
                ? "Проверка подключения прошла успешно."
                : "Не удалось подключиться к базе данных.";

            return ActionResult.Success;
        }

        private static bool CheckDbConnection(Session session, ConnectionStringParts db)
        {
            var connectionString = db.FormConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (Exception ex)
                {
                    session.Log("Error ocured during connection string checking: {0}", ex.Message);
                    return false;
                }
            }
        }

        #region Методы парсинга файла конфигурации

        private static XmlElement ReadConfigRoot(Session session, string installLocation)
        {
            if (!Directory.Exists(installLocation))
            {
                session.Log("InstallLocation not foud");
                return null;
            }

            string configFile = Path.Combine(installLocation, ConfigFile);
            if (!File.Exists(configFile))
            {
                session.Log("Config file [{0}] not found", configFile);
                return null;
            }

            var doc = new XmlDocument();
            using (var reader = new StreamReader(configFile))
            {
                doc.Load(reader);
            }

            return doc.DocumentElement;
        }

        private static int LoadPort(XmlElement root)
        {
            const string path = "descendant::service[@name='ApplicationService.Common.ApplicationService']/host/baseAddresses/add[@baseAddress]/@baseAddress";
            const string portPattern = @"localhost:(?<port>\d*)";

            string address = LoadAttribute(root, path);
            if (string.IsNullOrEmpty(address))
                return DefaultPort;

            var regex = new Regex(portPattern);
            var matches = regex.Match(address);
            if (!matches.Success)
                return DefaultPort;

            var portString = matches.Groups["port"].Value;

            int result;
            if (!int.TryParse(portString, out result))
                return DefaultPort;

            return result;
        }


        private static string LoadAttribute(XmlElement root, string path)
        {
            var node = root.SelectSingleNode(path);
            return node != null ? node.Value : null;
        }
        #endregion

        #region Дополнительные вложенные классы
        private class ConnectionStringParts
        {
            public string Server { get; set; }
            public string Database { get; set; }
            public string User { get; set; }
            public string Password { get; set; }

            public string FormConnectionString()
            {
                var connString = new SqlConnectionStringBuilder
                {
                    DataSource = Server,
                    InitialCatalog = Database,
                    UserID = User,
                    Password = Password,
                    ConnectTimeout = 10,
                    //PersistSecurityInfo = true
                };

                return connString.ConnectionString;
            }
        }
        #endregion
    }


}

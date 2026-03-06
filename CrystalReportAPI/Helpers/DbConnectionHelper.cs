using CrystalDecisions.Shared;
using System;
using System.Configuration;

namespace CrystalReportAPI.Helpers
{
    public static class DbConnectionHelper
    {
        private static string DSN_NAME => ConfigurationManager.AppSettings["DSN_NAME"];
        private static string DB_USER => ConfigurationManager.AppSettings["DB_USER"];
        private static string DB_PASSWORD => ConfigurationManager.AppSettings["DB_PASSWORD"];

        public static ConnectionInfo GetHanaConnection(string schema)
        {
            return new ConnectionInfo
            {
                ServerName = DSN_NAME,
                DatabaseName = schema,
                UserID = DB_USER,
                Password = DB_PASSWORD
            };
        }
    }
}

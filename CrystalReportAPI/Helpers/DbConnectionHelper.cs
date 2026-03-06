using CrystalDecisions.Shared;
using System;

namespace IDU_APP.Helpers
{
    public static class DbConnectionHelper
    {
        public const string DSN_NAME = "UDMW32";
        public const string DB_USER = "B1ADMIN";
        public const string DB_PASSWORD = "Password#01";

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

using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IDU_APP.Controllers
{
    public class CheckConnectionController : Controller
    {
        // GET: CheckConnection
        public ActionResult Index()
        {
            string result = "";
            string status = "";

            try
            {
                // GUNAKAN DSN yang sudah dibuat (UDMW32)
                string connectionString = "DSN=UDMW32;UID=B1ADMIN;PWD=Password#01";

                using (OdbcConnection conn = new OdbcConnection(connectionString))
                {
                    conn.Open();

                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        status = "SUCCESS";
                        result = "Koneksi ke database HANA berhasil!";

                        // Test query sederhana
                        using (OdbcCommand cmd = new OdbcCommand("SELECT CURRENT_USER, CURRENT_SCHEMA FROM DUMMY", conn))
                        {
                            using (OdbcDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    result += "\n\nUser: " + reader[0].ToString();
                                    result += "\nSchema: " + reader[1].ToString();
                                }
                            }
                        }
                    }
                    else
                    {
                        status = "FAILED";
                        result = "Koneksi gagal dibuka";
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                status = "ERROR";
                result = "Error: " + ex.Message;

                if (ex.InnerException != null)
                {
                    result += "\n\nInner Exception: " + ex.InnerException.Message;
                }
            }

            ViewBag.Status = status;
            ViewBag.Result = result;
            ViewBag.ConnectionString = "DSN=UDMW32;UID=B1ADMIN;PWD=****";

            return View();
        }
    }
}
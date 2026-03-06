using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IDU_APP.Controllers
{
    public class PdfToBase64ApiController : ApiController
    {
        public HttpResponseMessage GetInvoice(int docEntry)
        {
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            ReportDocument report = new ReportDocument();

            try
            {
                string pathDoc = @"D:\Lab\C#\IDU_APP\IDU_APP\Layouts\Invoice.rpt";

                if (!File.Exists(pathDoc))
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent("Error: Report file not found")
                    };
                }

                // ==================================================
                // LOAD REPORT
                // ==================================================
                report.Load(pathDoc);

                // ==================================================
                // SET PARAMETER - MAIN REPORT
                // ==================================================
                report.SetParameterValue("DOCKEY@", docEntry);

                // ==================================================
                // SET PARAMETER - SUBREPORT
                // ==================================================
                foreach (ReportDocument sub in report.Subreports)
                {
                    sub.SetParameterValue("DOCKEY@", docEntry);
                }

                // ==================================================
                // DEBUG PARAMETER (MAIN)
                // ==================================================
                foreach (ParameterFieldDefinition p in report.DataDefinition.ParameterFields)
                {
                    if (!p.HasCurrentValue)
                        throw new Exception($"Missing parameter (Main Report): {p.Name}");
                }

                // ==================================================
                // DEBUG PARAMETER (SUBREPORT)
                // ==================================================
                foreach (ReportDocument sub in report.Subreports)
                {
                    foreach (ParameterFieldDefinition p in sub.DataDefinition.ParameterFields)
                    {
                        if (!p.HasCurrentValue)
                            throw new Exception(
                                $"Missing parameter (Subreport): {sub.Name} -> {p.Name}"
                            );
                    }
                }

                // ==================================================
                // CONNECTION INFO (HANA DSN & SCHEMA)
                // ==================================================
                ConnectionInfo connectionInfo = new ConnectionInfo
                {
                    ServerName = "UDMW32", 
                    DatabaseName = "SIMULASI_NEW_UD", 
                    UserID = "B1ADMIN",
                    Password = "Password#01"
                };

                // ==================================================
                // APPLY LOGIN & SCHEMA TO TABLES
                // ==================================================
                ApplyLogOnAndSchema(report, connectionInfo, "SIMULASI_NEW_UD");

                foreach (ReportDocument sub in report.Subreports)
                {
                    ApplyLogOnAndSchema(sub, connectionInfo, "SIMULASI_NEW_UD");
                }

                // ==================================================
                // ==================================================
                // SET PARAMETERS
                // ==================================================
                SetParametersAggressively(report, docEntry);

                // ==================================================
                // EXPORT (DO NOT call Refresh() manually before export)
                // ==================================================
                using (Stream stream = report.ExportToStream(ExportFormatType.PortableDocFormat))
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    byte[] bytes = ms.ToArray();
                    string base64 = Convert.ToBase64String(bytes);

                    if (bytes.Length < 100)
                        throw new Exception($"PDF too small ({bytes.Length} bytes). Connection/Data issue?");

                    return Request.CreateResponse(HttpStatusCode.OK, new { base64 = base64, size = bytes.Length });
                }
            }
            catch (Exception ex)
            {
                string pStates = "";
                try {
                    foreach (ParameterFieldDefinition p in report.DataDefinition.ParameterFields)
                        pStates += $"[{p.Name}:HasValue={p.HasCurrentValue}] ";
                } catch { }

                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                    errorMessage += " | Inner: " + ex.InnerException.Message;
                
                errorMessage += " | States: " + pStates;

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { error = errorMessage });
            }
            finally
            {
                report.Close();
                report.Dispose();
            }
        }

        private void ApplyLogOnAndSchema(ReportDocument doc, ConnectionInfo connectionInfo, string schema)
        {
            foreach (Table table in doc.Database.Tables)
            {
                TableLogOnInfo logonInfo = table.LogOnInfo;
                logonInfo.ConnectionInfo = connectionInfo;
                table.ApplyLogOnInfo(logonInfo);
                
                // For HANA, sometimes location needs to be Schema.TableName
                if (!string.IsNullOrEmpty(schema) && !table.Location.Contains("."))
                {
                    table.Location = schema + "." + table.Location;
                }
            }
        }

        private void SetParametersAggressively(ReportDocument doc, int val)
        {
            // 1. Try by common names directly on the document
            try { doc.SetParameterValue("DOCKEY@", val); } catch { }
            try { doc.SetParameterValue("DocEntry@", val); } catch { }

            // 2. Iterate and try multiple ways for each parameter
            foreach (ParameterFieldDefinition p in doc.DataDefinition.ParameterFields)
            {
                try { doc.SetParameterValue(p.Name, val); } catch { }
                
                try {
                    ParameterValues currentValues = new ParameterValues();
                    ParameterDiscreteValue discreteValue = new ParameterDiscreteValue();
                    discreteValue.Value = val;
                    currentValues.Add(discreteValue);
                    p.ApplyCurrentValues(currentValues);
                } catch { }
            }

            // 3. Recursive for subreports
            foreach (ReportDocument sub in doc.Subreports)
            {
                SetParametersAggressively(sub, val);
            }
        }
    }
}

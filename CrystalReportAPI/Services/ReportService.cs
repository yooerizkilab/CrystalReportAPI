using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using IDU_APP.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Web;


namespace IDU_APP.Services
{
    public class ReportService : IReportService
    {
        public string GenerateReportBase64(string schema, string folder, string rptFile, int docEntry)
        {
            ReportDocument report = new ReportDocument();
            try
            {
                // Constuct path: ~/Layouts/{folder}/{rptFile}.rpt
                string rootPath = HttpContext.Current.Server.MapPath("~/Layouts");
                string pathDoc = Path.Combine(rootPath, folder, rptFile + ".rpt");

                if (!File.Exists(pathDoc))
                {
                    throw new FileNotFoundException($"Report template not found: {pathDoc}");
                }

                report.Load(pathDoc);

                ConnectionInfo connInfo = DbConnectionHelper.GetHanaConnection(schema);

                // Apply connection to main report and subreports
                ApplyLogOnAndSchema(report, connInfo, schema);
                foreach (ReportDocument sub in report.Subreports)
                {
                    ApplyLogOnAndSchema(sub, connInfo, schema);
                }

                // Set parameters robustly
                SetParametersAggressively(report, docEntry);

                // Export to PDF
                using (Stream stream = report.ExportToStream(ExportFormatType.PortableDocFormat))
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    byte[] bytes = ms.ToArray();
                    
                    if (bytes.Length < 100)
                        throw new Exception("Generated PDF is empty or corrupted.");

                    return Convert.ToBase64String(bytes);
                }
            }
            finally
            {
                report.Close();
                report.Dispose();
            }
        }

        public string[] ListTemplates(string folder)
        {
            try
            {
                string rootPath = HttpContext.Current.Server.MapPath("~/Layouts");
                string categoryPath = Path.Combine(rootPath, folder);

                if (!Directory.Exists(categoryPath))
                {
                    // Return empty if directory not found to avoid crashing the listing API
                    return new string[0];
                }

                // Get all .rpt files and return just the name without extension
                return Directory.GetFiles(categoryPath, "*.rpt")
                                .Select(Path.GetFileNameWithoutExtension)
                                .ToArray();
            }
            catch
            {
                return new string[0];
            }
        }

        private void ApplyLogOnAndSchema(ReportDocument doc, ConnectionInfo connectionInfo, string schema)
        {
            foreach (Table table in doc.Database.Tables)
            {
                TableLogOnInfo logonInfo = table.LogOnInfo;
                logonInfo.ConnectionInfo = connectionInfo;
                table.ApplyLogOnInfo(logonInfo);

                if (!string.IsNullOrEmpty(schema) && !table.Location.Contains("."))
                {
                    table.Location = schema + "." + table.Location;
                }
            }
        }

        private void SetParametersAggressively(ReportDocument doc, int val)
        {
            // Try common names directly
            try { doc.SetParameterValue("DOCKEY@", val); } catch { }
            try { doc.SetParameterValue("DocEntry@", val); } catch { }

            foreach (ParameterFieldDefinition p in doc.DataDefinition.ParameterFields)
            {
                try { doc.SetParameterValue(p.Name, val); } catch { }

                try
                {
                    ParameterValues currentValues = new ParameterValues();
                    ParameterDiscreteValue discreteValue = new ParameterDiscreteValue();
                    discreteValue.Value = val;
                    currentValues.Add(discreteValue);
                    p.ApplyCurrentValues(currentValues);
                }
                catch { }
            }

            foreach (ReportDocument sub in doc.Subreports)
            {
                SetParametersAggressively(sub, val);
            }
        }
    }
}

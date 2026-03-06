using CrystalReportAPI.Services;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CrystalReportAPI.Controllers
{
    [RoutePrefix("api/report")]
    public class ReportController : ApiController
    {
        private readonly IReportService _reportService;

        public ReportController()
        {
            // Note: In a real senior-level project, we would use Dependency Injection (Unity/Autofac).
            // For now, we'll manually instantiate it for simplicity unless DI is requested.
            _reportService = new ReportService();
        }

        [HttpGet]
        [Route("{schema}/{folder}/{rptFile}/{docEntry}")]
        public HttpResponseMessage GetReport(string schema, string folder, string rptFile, int docEntry)
        {
            try
            {
                string base64 = _reportService.GenerateReportBase64(schema, folder, rptFile, docEntry);
                
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = "Success",
                    schema = schema,
                    folder = folder,
                    file = rptFile,
                    docEntry = docEntry,
                    base64 = base64
                });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " | Inner: " + ex.InnerException.Message;
                }

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "Error",
                    message = errorMessage
                });
            }
        }
    }
}

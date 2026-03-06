using CrystalReportAPI.Services;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CrystalReportAPI.Controllers
{
    [RoutePrefix("api/template")]
    public class TemplateController : ApiController
    {
        private readonly IReportService _reportService;

        public TemplateController()
        {
            _reportService = new ReportService();
        }

        [HttpGet]
        [Route("{schemadb}/{folderName}")]
        public HttpResponseMessage ListTemplates(string schemadb, string folderName)
        {
            try
            {
                var templates = _reportService.ListTemplates(schemadb, folderName);
                
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = "Success",
                    schema = schemadb,
                    category = folderName,
                    templates = templates,
                    count = templates.Length
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "Error",
                    message = ex.Message
                });
            }
        }
    }
}

using IDU_APP.Services;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IDU_APP.Controllers
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
        [Route("{folderName}")]
        public HttpResponseMessage ListTemplates(string folderName)
        {
            try
            {
                var templates = _reportService.ListTemplates(folderName);
                
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = "Success",
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

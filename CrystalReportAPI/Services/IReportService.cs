using System.Threading.Tasks;

namespace IDU_APP.Services
{
    public interface IReportService
    {
        string GenerateReportBase64(string schema, string folder, string rptFile, int docEntry);
        string[] ListTemplates(string folder);
    }
}

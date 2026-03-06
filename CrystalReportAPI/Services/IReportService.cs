using System.Threading.Tasks;

namespace CrystalReportAPI.Services
{
    public interface IReportService
    {
        string GenerateReportBase64(string schema, string folder, string rptFile, int docEntry);
        string[] ListTemplates(string schema, string folder);
    }
}

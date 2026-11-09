using STOM.API.Models;

namespace STOM.API.Services;

public class PlayerImportService
{
    public async Task<ImportResult> ImportAsync(IFormFile file)
    {
        var result = new ImportResult();
        
        if (file == null || file.Length == 0)
        {
            result.Errors.Add("File is empty");
            return result;
        }

        return result;
    }
}

using GoogleSheetsWrapper;
using PterodactylPavlovServerController.Exceptions;

namespace PterodactylPavlovServerController.Services
{
    public class GoogleSheetService
    {
        private readonly IConfiguration configuration;

        public GoogleSheetService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public T[] GetDocumentRows<T>(Type repositoryType, string spreadsheetId, string tabName) where T : BaseRecord
        {
            SheetHelper<T> sheetHelper = new SheetHelper<T>(spreadsheetId, configuration["google_serviceaccountemail"], tabName);
            sheetHelper.Init(File.ReadAllText(configuration["google_jsoncredentialpath"]));

            BaseRepository<T> repository = (BaseRepository<T>)Activator.CreateInstance(repositoryType, sheetHelper)!;

            if (!repository.ValidateSchema().IsValid)
            {
                throw new GoogleSheetsHeaderMismatch();
            }

            return repository.GetAllRecords().ToArray();
        }
    }
}

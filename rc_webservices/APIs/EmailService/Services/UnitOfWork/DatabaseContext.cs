using EmailService.Code;
using EmailService;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using CommonLibrary.Entities;

namespace EmailService.Services.UnitOfWork
{
    public class DatabaseContext
    {
        private readonly IMongoDatabase _database = null;

        public DatabaseContext(IOptions<DbConfiguration> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            if (client != null)
            {
                _database = client.GetDatabase(settings.Value.Database);
            }
        }
        
        public IMongoCollection<Model.Setting> Settings
        {
            get
            {
                return _database.GetCollection<Model.Setting>("Setting");
            }
        }

        public IMongoCollection<Model.EmailTemplate> EmailTemplates
        {
            get
            {
                return _database.GetCollection<Model.EmailTemplate>("EmailTemplate");
            }
        }

    }
}

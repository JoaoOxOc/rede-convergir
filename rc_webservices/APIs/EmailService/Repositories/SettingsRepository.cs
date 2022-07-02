using CommonLibrary.Entities;
using EmailService;
using EmailService.Services.UnitOfWork;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EmailService.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly DatabaseContext context = null;
        
        public SettingsRepository(IOptions<DbConfiguration> configs)
        {
            this.context = new DatabaseContext(configs);
        }

        public async Task<IEnumerable<Model.Setting>> Get()
        {
            try
            {
                return await context.Settings.Find(_ => true).ToListAsync();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            //return new List<TEntity>();
        }

        public IEnumerable<Model.Setting> GetFiltered(string searchText, int? offset, int? limit, string sort, string sortDirection, string ignoreId = null)
        {
            try
            {
                searchText = searchText == null ? string.Empty : searchText.Trim().ToLower();

                IQueryable<Model.Setting> query = context.Settings.AsQueryable();

                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(x => x.Description.ToLower().Contains(searchText) || x.Key.ToLower().Contains(searchText));
                }

                if (!string.IsNullOrEmpty(ignoreId))
                {
                    query = query.Where(x => x.Id != ignoreId);
                }

                //SortDefinition<Log> sortFilter;

                switch (sort)
                {
                    case "Description":
                        query = sortDirection == "asc" ? query.OrderBy(x => x.Description) : query.OrderByDescending(x => x.Description);
                        break;
                    case "Key":
                        query = sortDirection == "asc" ? query.OrderBy(x => x.Key) : query.OrderByDescending(x => x.Key);
                        break;
                    default:
                        query = sortDirection == "asc" ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt);
                        break;
                }



                if (offset.HasValue && limit.HasValue)
                {
                    query = query.Skip((offset.Value - 1) * limit.Value).Take(limit.Value);
                }

                List<Model.Setting> logsList = query.ToList();

                return logsList;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int Count(string searchText, int? offset, int? limit, string sort)
        {
            try
            {
                searchText = searchText == null ? string.Empty : searchText.Trim();

                IQueryable<Model.Setting> query = context.Settings.AsQueryable();

                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(x => x.Description.ToLower().Contains(searchText) || x.Key.ToLower().Contains(searchText));
                }

                List<Model.Setting> logsList = query.ToList();

                return logsList.Count();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Model.Setting> GetById(object id)
        {
            var filter = Builders<Model.Setting>.Filter.Eq("Id", id);

            try
            {
                return await context.Settings.Find(filter).FirstOrDefaultAsync();
            }
            catch(Exception ex)
            {
                throw ex;
            }
       }

       public async Task Add(Model.Setting entity)
        {
            try
            {
                await this.context.Settings.InsertOneAsync(entity);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Model.Setting> UpdateSetting(Model.Setting editedSetting)
        {
            try
            {
                var filter = Builders<Model.Setting>.Filter.Eq("Id", editedSetting.Id);
                var setting = this.context.Settings.Find(filter).FirstOrDefaultAsync();
                if (setting.Result != null && setting.Result.SettingType == editedSetting.SettingType)
                {
                    var update = Builders<Model.Setting>.Update
                                          .Set(x => x.Description, editedSetting.Description)
                                          .Set(x => x.DefaultValue, editedSetting.DefaultValue)
                                          .Set(x => x.Value, editedSetting.Value)
                                          .Set(x => x.UpdatedBy, editedSetting.UpdatedBy)
                                          .Set(x => x.UpdatedAt, editedSetting.UpdatedAt);

                    var result = await this.context.Settings.UpdateOneAsync(filter, update);
                    if (result.IsAcknowledged)
                    {
                        return setting.Result;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> Delete(string id)
        {
            try
            {
                DeleteResult actionResult = await this.context.Settings.DeleteOneAsync(Builders<Model.Setting>.Filter.Eq("Id", id));

                return actionResult.IsAcknowledged && actionResult.DeletedCount > 0;
            }
            catch(Exception ex)
            {
                throw ex;
            } 

        }
        
    }
}

using ContentManageService.Entities;
using ContentManageService.Services.Database;

namespace ContentManageService.Migrations.Config
{
    public class DbInitializer
    {
        public static void Initialize(IServiceProvider services)
        {
            //IUnitOfWork uow = services.GetRequiredService<IUnitOfWork>();

            //#region Banners

            //var banners = new Banner[]
            //{
            //    new ClientCredential { Name = "tpbackoffice", ClientId= "tpbackoffice", Description="transição portugal backoffice", ClientSecret = _tokenManager.GetClientToken().Token },
            //    new ClientCredential { Name = "tphome", ClientId = "tphome", Description = "transição portugal homepage", ClientSecret = _tokenManager.GetClientToken().Token }
            //};

            //foreach (ClientCredential obj in clientCredentials)
            //{
            //    if (context.ClientCredential.Where(x => x.ClientId == obj.ClientId).FirstOrDefault() == null)
            //    {
            //        context.ClientCredential.Add(obj);
            //    }
            //}

            //#endregion

            //uow.BannerRepository.Add(banners.ToList());

            //uow.Save();
        }

    }
}

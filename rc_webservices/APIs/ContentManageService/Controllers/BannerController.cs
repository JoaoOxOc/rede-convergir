using CommonLibrary.Entities.ViewModel;
using CommonLibrary.Enums;
using CommonLibrary.Extensions;
using ContentManageService.Entities;
using ContentManageService.Models;
using ContentManageService.Services.Database;
using ContentManageService.Services.RabbitMQ;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Text.Json;

namespace ContentManageService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BannerController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IRabbitMQSender _rabbitSender;
        public BannerController(IUnitOfWork uow, IRabbitMQSender rabbitMQsender)
        {
            _uow = uow;
            _rabbitSender = rabbitMQsender;
        }

        private ObjectResult ValidateBanner(Banner banner)
        {
            if (banner.PageKey == null || string.IsNullOrWhiteSpace(banner.PageKey.Trim()))
            {
                return UnprocessableEntity("MandatoryFields");
            }

            Expression<Func<Banner, bool>> filter = (x => x.Id != banner.Id && x.PageKey.Equals(banner.PageKey)
            && (string.IsNullOrEmpty(banner.ComponentKey) || x.ComponentKey.Contains(banner.ComponentKey)) && (!banner.OrderPosition.HasValue || x.OrderPosition == banner.OrderPosition));

            var existsBanner = this._uow.BannerRepository.Get(null, null, filter, string.Empty, SortDirection.Ascending).FirstOrDefault();
            if (existsBanner != null)
            {
                ConflictModel conflict = new ConflictModel();
                conflict.ConflictType = ConflictType.DuplicateValue;
                conflict.Entity = typeof(Banner).Name;

                if (existsBanner.PageKey == banner.PageKey)
                {
                    var propertyInfo = typeof(Banner).GetProperty("PageKey");
                    conflict.Field = propertyInfo != null ? propertyInfo.Name : "Unknown";
                }
                if (existsBanner.ComponentKey == banner.ComponentKey)
                {
                    var propertyInfo = typeof(Banner).GetProperty("ComponentKey");
                    conflict.Field = propertyInfo != null ? propertyInfo.Name : "Unknown";
                }
                if (existsBanner.OrderPosition == banner.OrderPosition)
                {
                    var propertyInfo = typeof(Banner).GetProperty("OrderPosition");
                    conflict.Field = propertyInfo != null ? propertyInfo.Name : "Unknown";
                }

                return Conflict(conflict);
            }

            return Ok(null);
        }

        private ObjectResult ValidateBannerTranslation(BannerTranslation bannerTranslation)
        {
            if (bannerTranslation.LangKey == null || string.IsNullOrWhiteSpace(bannerTranslation.LangKey.Trim()))
            {
                return UnprocessableEntity("MandatoryFields");
            }

            Expression<Func<BannerTranslation, bool>> filter = (x => x.LangKey == bannerTranslation.LangKey && x.BannerId == bannerTranslation.BannerId);

            var existsBanner = this._uow.BannerTranslationRepository.Get(null, null, filter, string.Empty, SortDirection.Ascending).FirstOrDefault();
            if (existsBanner != null)
            {
                ConflictModel conflict = new ConflictModel();
                conflict.ConflictType = ConflictType.DuplicateValue;
                conflict.Entity = typeof(BannerTranslation).Name;

                if (existsBanner.LangKey == bannerTranslation.LangKey)
                {
                    var propertyInfo = typeof(BannerTranslation).GetProperty("LangKey");
                    conflict.Field = propertyInfo != null ? propertyInfo.Name : "Unknown";
                }

                return Conflict(conflict);
            }

            return Ok(null);
        }

        private BannerModel? ParseEntityToModel(BannerModel? bannerData, BannerTranslation bannerTranslationData)
        {
            BannerModel? model = null;
            if (bannerTranslationData != null)
            {
                if (bannerData == null)
                {
                    model = new BannerModel();
                    model.Id = bannerTranslationData.BannerId;
                    model.IsDraft = bannerTranslationData.Banner?.IsDraft;
                    model.ComponentKey = bannerTranslationData.Banner?.ComponentKey;
                    model.PageKey = bannerTranslationData.Banner?.PageKey;
                    model.OrderPosition = bannerTranslationData.Banner?.OrderPosition;
                    model.ParentBannerId = bannerTranslationData.Banner?.ParentBannerId;
                    model.ParentBannerPath = bannerTranslationData.Banner?.ParentPath;
                    model.BannerLanguages = new List<BannerModel.BannerDataModel>();
                    model.BannerLanguages.Add(new BannerModel.BannerDataModel
                    {
                        LangCode = bannerTranslationData.LangKey,
                        BannerData = bannerTranslationData.BannerDataJson,
                        BannerDataHtml = bannerTranslationData.BannerDataHtml
                    });
                }
                else
                {
                    if (bannerData.BannerLanguages != null && !bannerData.BannerLanguages.Any(x => x.LangCode == bannerTranslationData.LangKey))
                    {
                        bannerData.BannerLanguages.Add(new BannerModel.BannerDataModel
                        {
                            LangCode = bannerTranslationData.LangKey,
                            BannerData = bannerTranslationData.BannerDataJson,
                            BannerDataHtml = bannerTranslationData.BannerDataHtml
                        });
                    }
                }
            }
            return model;
        }

        private BannerModel? ParseEntityToModel(BannerModel? bannerData, Banner banner)
        {
            BannerModel? model = null;
            if (banner != null)
            {
                if (bannerData == null)
                {
                    model = new BannerModel();
                    model.Id = banner.Id;
                    model.IsDraft = banner.IsDraft;
                    model.ComponentKey = banner.ComponentKey;
                    model.PageKey = banner.PageKey;
                    model.OrderPosition = banner.OrderPosition;
                    model.ParentBannerId = banner.ParentBannerId;
                    model.ParentBannerPath = banner.ParentPath;
                    model.ChildElements = _uow.BannerRepository.Count((x => x.ParentBannerId == banner.Id));
                    model.BannerLanguages = new List<BannerModel.BannerDataModel>();

                    if (banner.BannerTranslations != null)
                    {
                        foreach(var translation in banner.BannerTranslations)
                        {
                            model.BannerLanguages.Add(new BannerModel.BannerDataModel
                            {
                                LangCode = translation.LangKey,
                                BannerData = translation.BannerDataJson,
                                BannerDataHtml = translation.BannerDataHtml
                            });
                        }
                    }
                }
            }
            return model;
        }

        private List<BannerModel> ParseEntitiesToModel(List<BannerTranslation> bannersTranslationData)
        {
            List<BannerModel> models = new List<BannerModel>();
            foreach (var bannerTranslation in bannersTranslationData)
            {
                var model = ParseEntityToModel(models.Find(x => x.Id == bannerTranslation.BannerId), bannerTranslation);
                if (model != null)
                {
                    models.Add(model);
                }
            }
            return models;
        }

        private List<BannerModel> ParseEntitiesToModel(List<Banner> bannersData)
        {
            List<BannerModel> models = new List<BannerModel>();
            foreach (var banner in bannersData)
            {
                var model = ParseEntityToModel(models.Find(x => x.Id == banner.Id), banner);
                if (model != null)
                {
                    models.Add(model);
                }
            }
            return models;
        }

        private KeyValuePair<List<BannerTranslation>, List<BannerTranslation>> ProcessBannerTranslations(bool isCreate, Banner bannerData, List<BannerModel.BannerDataModel>? translationsModel)
        {
            KeyValuePair<List<BannerTranslation>, List<BannerTranslation>> translationsProcessed = new KeyValuePair<List<BannerTranslation>, List<BannerTranslation>>();
            if (translationsModel != null)
            {
                List<BannerTranslation> toUpdate = new List<BannerTranslation>();
                List<BannerTranslation> toCreate = new List<BannerTranslation>();
                foreach (var translation in translationsModel)
                {
                    BannerTranslation? bannerTranslationFound = null;
                    if (!isCreate)
                    {
                        Expression<Func<BannerTranslation, bool>> filterTranslation = (x => x.LangKey == translation.LangCode && x.BannerId == bannerData.Id);
                        bannerTranslationFound = this._uow.BannerTranslationRepository.Get(null, null, filterTranslation, "LangKey", SortDirection.Ascending).FirstOrDefault();
                    }
                    if (bannerTranslationFound != null)
                    {
                        bannerTranslationFound.BannerDataJson = translation.BannerData;
                        toUpdate.Add(bannerTranslationFound);
                    }
                    else
                    {
                        BannerTranslation newTranslation = new BannerTranslation();
                        if (!isCreate && bannerData.Id.HasValue)
                        {
                            newTranslation.BannerId = bannerData.Id.Value;
                        }

                        newTranslation.LangKey = translation.LangCode;
                        newTranslation.PageKey = bannerData.PageKey;
                        newTranslation.BannerDataHtml = translation.BannerDataHtml;
                        newTranslation.BannerDataJson = translation.BannerData;

                        toCreate.Add(newTranslation);
                    }
                }
                translationsProcessed = new KeyValuePair<List<BannerTranslation>, List<BannerTranslation>>(toUpdate, toCreate);
            }
            return translationsProcessed;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string? searchText, string? langCode, string? pageKey, string? componentKey, int? orderPosition, int? parentBannerId, bool? isActive, int? offset, int? limit, string sort, string sortDirection)
        {
            string userId = HttpContext.Request.Headers["UserId"];
            string userRole = HttpContext.Request.Headers["UserRole"];
            List<string>? scopes = !string.IsNullOrEmpty(HttpContext.Request.Headers["UserClaims"]) ? JsonSerializer.Deserialize<List<string>>(HttpContext.Request.Headers["UserClaims"]) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userRole, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "cms.read", "cms.write" }))
            {
                try
                {
                    offset = offset ?? 1;

                    string parsedLangKey = !string.IsNullOrEmpty(langCode) ? langCode.ToLower() : "en-us";
                    pageKey = pageKey ?? "";
                    componentKey = componentKey ?? "";
                    searchText = searchText == null ? string.Empty : searchText.Trim().ToLower();
                    sort = sort ?? "PageKey";
                    SortDirection direction = sortDirection == "desc" ? SortDirection.Descending : SortDirection.Ascending;

                    //(string.IsNullOrEmpty(langCode) || x.LangKey.Contains(parsedLangKey)) && 
                    Expression<Func<Banner, bool>> filter = (x => (!isActive.HasValue || (isActive.HasValue && x.IsDraft == !isActive))
                    && (string.IsNullOrEmpty(pageKey) || x.PageKey.ToLower().Contains(pageKey.ToLower()))
                    && (string.IsNullOrEmpty(componentKey) || x.ComponentKey.ToLower().Contains(componentKey.ToLower()))
                    && (!orderPosition.HasValue || (orderPosition.HasValue && x.OrderPosition == orderPosition))
                    && ((!parentBannerId.HasValue && !x.ParentBannerId.HasValue) || (parentBannerId.HasValue && x.ParentBannerId == parentBannerId)));

                    var _bannerData = _uow.BannerRepository.Get(offset, limit, filter, sort, direction, "BannerTranslations");

                    int totalCount = _uow.BannerRepository.Count(filter);

                    Request.HttpContext.Response.Headers.Add("X-Total-Count", totalCount.ToString());

                    return _bannerData != null ? Ok(new
                    {
                        banners = ParseEntitiesToModel(_bannerData),
                        isActive = isActive
                    })
                    : NotFound(new List<BannerModel>());
                }
                catch (Exception ex)
                {
                    CommonLibrary.Entities.ViewModel.ExceptionModel exceptionModel = new CommonLibrary.Entities.ViewModel.ExceptionModel();
                    exceptionModel.Message = ex.Message;
                    exceptionModel.StackTrace = ex.StackTrace;
                    exceptionModel.DateLogging = DateTime.UtcNow;
                    exceptionModel.AdminRole = "Admin";
                    exceptionModel.InnerException = ex.InnerException;
                    exceptionModel.InputDataJson = JsonSerializer.Serialize(new
                    {
                        searchText = searchText,
                        langCode = langCode,
                        pageKey = pageKey,
                        componentKey = componentKey,
                        orderPosition = orderPosition,
                        isActive = isActive,
                        offset = offset,
                        limit = limit,
                        sort = sort,
                        sortDirection = sortDirection
                    });
                    exceptionModel.UserId = userId;

                    bool success = await _rabbitSender.PublishExceptionMessage(exceptionModel);
                    return StatusCode(StatusCodes.Status500InternalServerError, ex.Message + "| stacktrace: " + ex.StackTrace);
                }
            }
            return StatusCode(StatusCodes.Status403Forbidden, null);
        }

        [HttpGet]
        [Route("public")]
        public async Task<IActionResult> GetPublicBanners(string? pageKey, string? componentKey, int? orderPosition, string? langCode)
        {
            try
            {
                string parsedLangKey = !string.IsNullOrEmpty(langCode) ? langCode.ToLower() : "en-us";
                pageKey = pageKey ?? "aboutheadline";
                componentKey = componentKey ?? "";

                Expression<Func<BannerTranslation, bool>> filter = (x => x.Banner.PageKey.ToLower().Contains(pageKey) && x.LangKey.Contains(parsedLangKey) && x.Banner.IsDraft == false
                && (string.IsNullOrEmpty(componentKey) || x.Banner.ComponentKey.Contains(componentKey)) && (!orderPosition.HasValue || x.Banner.OrderPosition == orderPosition));

                var _bannerData = _uow.BannerTranslationRepository.Get(null, null, filter, "LangKey", SortDirection.Ascending, "Banner").FirstOrDefault();
                if (_bannerData == null)
                {
                    //try to get any without the language key
                    filter = (x => x.Banner.PageKey.ToLower().Contains(pageKey) && x.Banner.IsDraft == false
                    && (string.IsNullOrEmpty(componentKey) || x.Banner.ComponentKey.Contains(componentKey)) && (!orderPosition.HasValue || x.Banner.OrderPosition == orderPosition));

                    _bannerData = _uow.BannerTranslationRepository.Get(null, null, filter, "LangKey", SortDirection.Ascending, "Banner").FirstOrDefault();
                }

                int totalCount = _uow.BannerTranslationRepository.Count(filter);

                Request.HttpContext.Response.Headers.Add("X-Total-Count", totalCount.ToString());

                return _bannerData != null ? Ok(new
                {
                    banners = ParseEntitiesToModel(new List<BannerTranslation> { _bannerData })
                })
                : NotFound(new List<BannerModel>());
            }
            catch (Exception ex)
            {
                CommonLibrary.Entities.ViewModel.ExceptionModel exceptionModel = new CommonLibrary.Entities.ViewModel.ExceptionModel();
                exceptionModel.Message = ex.Message;
                exceptionModel.StackTrace = ex.StackTrace;
                exceptionModel.DateLogging = DateTime.UtcNow;
                exceptionModel.AdminRole = "Admin";
                exceptionModel.InnerException = ex.InnerException;
                exceptionModel.InputDataJson = JsonSerializer.Serialize(new
                {
                    langCode = langCode,
                    pageKey = pageKey,
                    componentKey = componentKey,
                    orderPosition = orderPosition,
                });
                exceptionModel.UserId = string.Empty;

                bool success = await _rabbitSender.PublishExceptionMessage(exceptionModel);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message + "| stacktrace: " + ex.StackTrace);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            string userId = HttpContext.Request.Headers["UserId"];
            string userRole = HttpContext.Request.Headers["UserRole"];
            List<string>? scopes = !string.IsNullOrEmpty(HttpContext.Request.Headers["UserClaims"]) ? JsonSerializer.Deserialize<List<string>>(HttpContext.Request.Headers["UserClaims"]) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userRole, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "cms.read", "cms.write" }))
            {
                try
                {
                    Expression<Func<Banner, bool>> filter = (x => x.Id == id);

                    var _bannerData = _uow.BannerRepository.Get(null, null, filter, "PageKey", SortDirection.Ascending, "BannerTranslations");

                    return _bannerData != null && _bannerData.Count > 0 ? Ok(new
                    {
                        banner = ParseEntitiesToModel(_bannerData)[0]
                    })
                    : NotFound(null);
                }
                catch (Exception ex)
                {
                    CommonLibrary.Entities.ViewModel.ExceptionModel exceptionModel = new CommonLibrary.Entities.ViewModel.ExceptionModel();
                    exceptionModel.Message = ex.Message;
                    exceptionModel.StackTrace = ex.StackTrace;
                    exceptionModel.DateLogging = DateTime.UtcNow;
                    exceptionModel.AdminRole = "Admin";
                    exceptionModel.InnerException = ex.InnerException;
                    exceptionModel.InputDataJson = JsonSerializer.Serialize(new
                    {
                        bannerId = id
                    });
                    exceptionModel.UserId = HttpContext.Request.Headers["UserId"];

                    bool success = await _rabbitSender.PublishExceptionMessage(exceptionModel);
                    return StatusCode(StatusCodes.Status500InternalServerError, null);
                }
            }
            return StatusCode(StatusCodes.Status403Forbidden, null);
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Put([FromBody] BannerModel model)
        {
            List<string>? scopes = !string.IsNullOrEmpty(HttpContext.Request.Headers["UserClaims"]) ? JsonSerializer.Deserialize<List<string>>(HttpContext.Request.Headers["UserClaims"]) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(HttpContext.Request.Headers["UserRole"], new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "cms.write" }))
            {
                Expression<Func<Banner, bool>> filter = (x => x.Id == model.Id);

                var banner = this._uow.BannerRepository.Get(null, null, filter, string.Empty, SortDirection.Ascending).FirstOrDefault();
                if (banner != null)
                {
                    banner.PageKey = model.PageKey;
                    banner.ComponentKey = model.ComponentKey;
                    banner.OrderPosition = model.OrderPosition;
                    banner.ParentBannerId = model.ParentBannerId;
                    banner.ParentPath = model.ParentBannerPath;
                    banner.IsDraft = model.IsDraft;
                    banner.UpdatedAt = DateTime.UtcNow;
                    banner.UpdatedBy = HttpContext.Request.Headers["UserId"];

                    ObjectResult _validate = this.ValidateBanner(banner);
                    if (_validate.StatusCode != StatusCodes.Status200OK)
                    {
                        return _validate;
                    }

                    try
                    {
                        _uow.BannerRepository.Update(banner);
                        _uow.Save();

                        var updateCreatePairs = ProcessBannerTranslations(false, banner, model.BannerLanguages);

                        if (updateCreatePairs.Key != null && updateCreatePairs.Key.Count > 0)
                        {
                            _uow.BannerTranslationRepository.Update(updateCreatePairs.Key);
                        }
                        if (updateCreatePairs.Value != null && updateCreatePairs.Value.Count > 0)
                        {
                            _uow.BannerTranslationRepository.Add(updateCreatePairs.Value);
                        }
                        _uow.Save();

                        return Ok(new
                        {
                            bannerPageKey = banner.PageKey,
                            bannerComponentKey = banner.ComponentKey,
                            bannerOrderPosition = banner.OrderPosition,
                            bannerId = banner.Id
                        });
                    }
                    catch (Exception ex)
                    {
                        CommonLibrary.Entities.ViewModel.ExceptionModel exceptionModel = new CommonLibrary.Entities.ViewModel.ExceptionModel();
                        exceptionModel.Message = ex.Message;
                        exceptionModel.StackTrace = ex.StackTrace;
                        exceptionModel.DateLogging = DateTime.UtcNow;
                        exceptionModel.AdminRole = "Admin";
                        exceptionModel.InnerException = ex.InnerException;
                        exceptionModel.InputDataJson = JsonSerializer.Serialize(model);
                        exceptionModel.UserId = HttpContext.Request.Headers["UserId"];

                        bool success = await _rabbitSender.PublishExceptionMessage(exceptionModel);
                        return StatusCode(StatusCodes.Status500InternalServerError, null);
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            return StatusCode(StatusCodes.Status403Forbidden, null);
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> RegisterBanner([FromBody] BannerModel model)
        {
            List<string>? scopes = !string.IsNullOrEmpty(HttpContext.Request.Headers["UserClaims"]) ? JsonSerializer.Deserialize<List<string>>(HttpContext.Request.Headers["UserClaims"]) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(HttpContext.Request.Headers["UserRole"], new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "cms.write" }))
            {
                Banner banner = new Banner();

                banner.PageKey = model.PageKey;
                banner.ComponentKey = model.ComponentKey;
                banner.OrderPosition = model.OrderPosition;
                if (model.ParentBannerId.HasValue)
                {
                    var parentBanner = _uow.BannerRepository.GetById(model.ParentBannerId);
                    banner.ParentBannerId = parentBanner.Id;
                    banner.ParentPath = (!string.IsNullOrEmpty(parentBanner.ParentPath) ? parentBanner.ParentPath : "|") + parentBanner.Id + "|";
                }
                banner.IsDraft = model.IsDraft;
                banner.CreatedAt = DateTime.UtcNow;
                banner.CreatedBy = HttpContext.Request.Headers["UserId"];

                ObjectResult _validate = this.ValidateBanner(banner);
                if (_validate.StatusCode != StatusCodes.Status200OK)
                {
                    return _validate;
                }

                try
                {
                    var updateCreatePairs = ProcessBannerTranslations(true, banner, model.BannerLanguages);
                    if (updateCreatePairs.Value != null && updateCreatePairs.Value.Count > 0)
                    {
                        banner.BannerTranslations = updateCreatePairs.Value;
                    }
                    _uow.BannerRepository.Add(banner);
                    _uow.Save();

                    return Ok(new
                    {
                        bannerPageKey = banner.PageKey,
                        bannerComponentKey = banner.ComponentKey,
                        bannerOrderPosition = banner.OrderPosition,
                        bannerId = banner.Id
                    });
                }
                catch (Exception ex)
                {
                    CommonLibrary.Entities.ViewModel.ExceptionModel exceptionModel = new CommonLibrary.Entities.ViewModel.ExceptionModel();
                    exceptionModel.Message = ex.Message;
                    exceptionModel.StackTrace = ex.StackTrace;
                    exceptionModel.DateLogging = DateTime.UtcNow;
                    exceptionModel.AdminRole = "Admin";
                    exceptionModel.InnerException = ex.InnerException;
                    exceptionModel.InputDataJson = JsonSerializer.Serialize(model);
                    exceptionModel.UserId = HttpContext.Request.Headers["UserId"];

                    bool success = await _rabbitSender.PublishExceptionMessage(exceptionModel);
                    return StatusCode(StatusCodes.Status500InternalServerError, null);
                }
            }
            return StatusCode(StatusCodes.Status403Forbidden, null);
        }
    }
}

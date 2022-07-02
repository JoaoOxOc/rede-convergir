using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Services.Database;
using UserService.Services.Email;
using UserService.Entities;
using UserService.Models;
using System.Text.Json;
using System.Linq.Expressions;
using CommonLibrary.Enums;
using CommonLibrary.Entities.ViewModel;
using Microsoft.AspNetCore.Authorization;
using CommonLibrary.Extensions;
using UserService.Services.TermsManager;
using UserService.Services.RabbitMQ;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TermsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _uow;
        private readonly IEmailSender _emailSender;
        private readonly ITermsManager _termsManager;
        private readonly IRabbitMQSender _rabbitSender;

        public TermsController(IUnitOfWork uow, IConfiguration configuration, IEmailSender emailSender, ITermsManager termsManager, IRabbitMQSender rabbitMQSender)
        {
            _uow = uow;
            _configuration = configuration;
            _emailSender = emailSender;
            _termsManager = termsManager;
            _rabbitSender = rabbitMQSender;
        }

        private ObjectResult ValidateTerms(TermsConditions terms)
        {
            if (terms.Version <= 0)
            {
                return UnprocessableEntity("MandatoryFields");
            }

            Expression<Func<TermsConditions, bool>> filter = (x => x.Id != terms.Id && (x.Version == terms.Version));

            var existsTermsRecord = this._uow.TermsConditionsRepository.Get(null, null, filter, string.Empty, SortDirection.Ascending).FirstOrDefault();
            if (existsTermsRecord != null)
            {
                ConflictModel conflict = new ConflictModel();
                conflict.ConflictType = ConflictType.DuplicateValue;
                conflict.Entity = typeof(Scope).Name;

                if (existsTermsRecord.Version == terms.Version)
                {
                    var propertyInfo = typeof(TermsConditions).GetProperty("Version");
                    conflict.Field = propertyInfo != null ? propertyInfo.Name : "Unknown";
                }

                return Conflict(conflict);
            }

            return Ok(null);
        }

        private ObjectResult ValidateTermsTranslation(TermsConditionsTranslation termsTranslation)
        {
            if (termsTranslation.LangKey == null || string.IsNullOrWhiteSpace(termsTranslation.LangKey.Trim())
                || termsTranslation.DataBlocksJson == null || string.IsNullOrWhiteSpace(termsTranslation.DataBlocksJson.ToString().Trim()))
            {
                return UnprocessableEntity("MandatoryFields");
            }

            Expression<Func<TermsConditionsTranslation, bool>> filter = (x => x.LangKey == termsTranslation.LangKey && x.TermsConditionsId == termsTranslation.TermsConditionsId);

            var existsTranslation = this._uow.TermsConditionsTranslationRepository.Get(null, null, filter, string.Empty, SortDirection.Ascending).FirstOrDefault();
            if (existsTranslation != null)
            {
                ConflictModel conflict = new ConflictModel();
                conflict.ConflictType = ConflictType.DuplicateValue;
                conflict.Entity = typeof(TermsConditionsTranslation).Name;

                if (existsTranslation.LangKey == termsTranslation.LangKey)
                {
                    var propertyInfo = typeof(TermsConditionsTranslation).GetProperty("LangKey");
                    conflict.Field = propertyInfo != null ? propertyInfo.Name : "Unknown";
                }

                return Conflict(conflict);
            }

            return Ok(null);
        }

        private TermsModel? ParseEntityToModel(TermsModel? termsData, TermsConditionsTranslation termsTranslation, List<User>? users)
        {
            TermsModel? model = null;
            if (termsTranslation != null)
            {
                if (termsData == null)
                {
                    model = new TermsModel();
                    model.Id = termsTranslation.TermsConditionsId;
                    model.IsActive = termsTranslation.TermsConditions?.IsActive;
                    model.Version = termsTranslation.TermsConditions?.Version;
                    model.BeenActive = termsTranslation.TermsConditions?.BeenActive;
                    model.TermsLanguages = new List<TermsModel.TermsDataModel>();
                    model.TermsLanguages.Add(new TermsModel.TermsDataModel
                    {
                        LangCode = termsTranslation.LangKey,
                        TermsData = termsTranslation.DataBlocksJson//JsonSerializer.Serialize(termsTranslation.DataBlocksJson)
                    });
                }
                else
                {
                    if (termsData.TermsLanguages != null && !termsData.TermsLanguages.Any(x => x.LangCode == termsTranslation.LangKey))
                    {
                        termsData.TermsLanguages.Add(new TermsModel.TermsDataModel
                        {
                            LangCode = termsTranslation.LangKey,
                            TermsData = termsTranslation.DataBlocksJson//JsonSerializer.Serialize(termsTranslation.DataBlocksJson)
                        });
                    }
                }
            }

            return model;
        }

        private List<TermsModel> ParseEntitiesToModel(List<TermsConditionsTranslation> termsRecords)
        {
            List<TermsModel> models = new List<TermsModel>();
            foreach (var termsTranslation in termsRecords)
            {
                var model = ParseEntityToModel(models.Find(x => x.Id == termsTranslation.TermsConditionsId), termsTranslation, null);
                if (model != null)
                {
                    models.Add(model);
                }
            }
            return models;
        }

        private KeyValuePair<List<TermsConditionsTranslation>, List<TermsConditionsTranslation>> ProcessTermsTranslations(bool isCreate, TermsConditions termsData, List<TermsModel.TermsDataModel>? translationsModel)
        {
            KeyValuePair<List<TermsConditionsTranslation>, List<TermsConditionsTranslation>> translationsProcessed = new KeyValuePair<List<TermsConditionsTranslation>, List<TermsConditionsTranslation>>();
            if (translationsModel != null)
            {
                List<TermsConditionsTranslation> toUpdate = new List<TermsConditionsTranslation>();
                List<TermsConditionsTranslation> toCreate = new List<TermsConditionsTranslation>();
                foreach (var translation in translationsModel)
                {
                    TermsConditionsTranslation? termsTranslationFound = null;
                    if (!isCreate)
                    {
                        Expression<Func<TermsConditionsTranslation, bool>> filterTranslation = (x => x.LangKey == translation.LangCode && x.TermsConditionsId == termsData.Id);
                        termsTranslationFound = this._uow.TermsConditionsTranslationRepository.Get(null, null, filterTranslation, "LangKey", SortDirection.Ascending).FirstOrDefault();
                    }
                    if (termsTranslationFound != null)
                    {
                        termsTranslationFound.DataBlocksJson = translation.TermsData;// JsonDocument.Parse(translation.TermsData, new System.Text.Json.JsonDocumentOptions { AllowTrailingCommas = true });
                        toUpdate.Add(termsTranslationFound);
                    }
                    else
                    {
                        TermsConditionsTranslation newTranslation = new TermsConditionsTranslation();
                        if (!isCreate)
                        {
                            newTranslation.TermsConditionsId = termsData.Id;
                        }
                        newTranslation.TermsConditionsVersion = termsData.Version;
                        newTranslation.LangKey = translation.LangCode;
                        newTranslation.DataBlocksJson = translation.TermsData;// JsonDocument.Parse(translation.TermsData, new System.Text.Json.JsonDocumentOptions { AllowTrailingCommas = true });

                        toCreate.Add(newTranslation);
                    }
                }
                translationsProcessed = new KeyValuePair<List<TermsConditionsTranslation>, List<TermsConditionsTranslation>>(toUpdate, toCreate);
            }
            return translationsProcessed;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get(string? searchText, bool? isActive, decimal? version, int? offset, int? limit, string sort, string sortDirection)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "terms.admin" }))
            {
                try
                {
                    offset = offset ?? 1;

                    searchText = searchText == null ? string.Empty : searchText.Trim().ToLower();
                    sort = sort ?? "Version";
                    SortDirection direction = sortDirection == "desc" ? SortDirection.Descending : SortDirection.Ascending;

                    Expression<Func<TermsConditions, bool>> filter = (x => (!version.HasValue || x.Version == version.Value) 
                    && (!isActive.HasValue || x.IsActive == isActive.Value));


                    var _terms = _uow.TermsConditionsRepository.Get(offset, limit, filter, sort, direction, string.Empty);

                    int totalCount = _uow.TermsConditionsRepository.Count(filter);

                    Request.HttpContext.Response.Headers.Add("X-Total-Count", totalCount.ToString());

                    return _terms != null ? Ok(new
                    {
                        termsRecords = _terms
                    })
                    : NotFound(new List<TermsConditions>());
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
                        isActive = isActive,
                        version = version,
                        offset = offset,
                        limit = limit,
                        sort = sort,
                        sortDirection = sortDirection
                    });
                    var claimUserId = userClaims.Where(x => x.Claim == "userId").FirstOrDefault();
                    exceptionModel.UserId = claimUserId != null ? claimUserId.Value : "";

                    bool success = await _rabbitSender.PublishExceptionMessage(exceptionModel);

                    return StatusCode(StatusCodes.Status500InternalServerError, null);
                }
            }
            return Forbid();
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "terms.admin" }))
            {
                try
                {
                    Expression<Func<TermsConditionsTranslation, bool>> filter = (x => x.TermsConditionsId == id);

                    var terms = _uow.TermsConditionsTranslationRepository.Get(null, null, filter, "LangKey", SortDirection.Ascending, "TermsConditions");

                    if (terms != null)
                    {
                        return Ok(new
                        {
                            termsRecord = ParseEntitiesToModel(terms)[0]
                        });
                    }
                    else
                    {
                        return NotFound(null);
                    }
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
                        termsId = id
                    });
                    var claimUserId = userClaims.Where(x => x.Claim == "userId").FirstOrDefault();
                    exceptionModel.UserId = claimUserId != null ? claimUserId.Value : "";

                    bool success = await _rabbitSender.PublishExceptionMessage(exceptionModel);

                    return StatusCode(StatusCodes.Status500InternalServerError, null);
                }
            }
            return Forbid();
        }

        [HttpGet]
        [Route("public")]
        public async Task<IActionResult> GetActivePublic(string langCode)
        {
            try
            {
                Expression<Func<TermsConditionsTranslation, bool>> filter = (x => x.TermsConditions.IsActive == true && x.LangKey == langCode);

                var terms = _uow.TermsConditionsTranslationRepository.Get(1, 1, filter, "TermsConditionsVersion", SortDirection.Descending, "TermsConditions");

                if (terms != null)
                {
                    return Ok(new
                    {
                        termsRecord = ParseEntitiesToModel(terms)[0]
                    });
                }
                else
                {
                    return NotFound(null);
                }
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
                    langCode = langCode
                });
                exceptionModel.UserId = "";

                bool success = await _rabbitSender.PublishExceptionMessage(exceptionModel);

                return StatusCode(StatusCodes.Status500InternalServerError, null);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] TermsModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "terms.admin" }))
            {

                TermsConditions newTerms = new TermsConditions();
                newTerms.Version = _termsManager.GetNewTermsConditionsVersion();
                newTerms.CreatedAt = DateTime.UtcNow;
                newTerms.CreatedBy = userClaims.Where(x => x.Claim == "userId").Single().Value;

                ObjectResult _validate = this.ValidateTerms(newTerms);
                if (_validate.StatusCode != StatusCodes.Status200OK)
                {
                    return _validate;
                }

                var updateCreatePairs = ProcessTermsTranslations(true, newTerms, model.TermsLanguages);
                if (updateCreatePairs.Value != null && updateCreatePairs.Value.Count > 0)
                {
                    newTerms.TermsConditionsTranslations = updateCreatePairs.Value;
                }
                _uow.TermsConditionsRepository.Add(newTerms);
                _uow.Save();

                return Ok(new
                {
                    termsId = newTerms.Id,
                    termsVersion = newTerms.Version
                });
            }
            return Forbid();
        }

        [Authorize]
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Put([FromBody] TermsModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "terms.admin" }))
            {
                Expression<Func<TermsConditions, bool>> filter = (x => x.Id == model.Id);

                var terms = this._uow.TermsConditionsRepository.Get(null, null, filter, string.Empty, SortDirection.Ascending).FirstOrDefault();
                if (terms != null)
                {
                    if (!terms.BeenActive.HasValue || !terms.BeenActive.Value)
                    {
                        terms.UpdatedAt = DateTime.UtcNow;
                        terms.UpdatedBy = userClaims.Where(x => x.Claim == "userId").Single().Value;

                        ObjectResult _validate = this.ValidateTerms(terms);
                        if (_validate.StatusCode != StatusCodes.Status200OK)
                        {
                            return _validate;
                        }

                        if (model.IsActive.HasValue)
                        {
                            if (terms.IsActive == false && model.IsActive.Value == true)
                            {
                                _termsManager.DeactivateTermsConditions(terms);
                                terms.BeenActive = true;
                            }
                            terms.IsActive = model.IsActive.Value;
                        }

                        try
                        {
                            _uow.TermsConditionsRepository.Update(terms);
                            _uow.Save();

                            var updateCreatePairs = ProcessTermsTranslations(false, terms, model.TermsLanguages);

                            if (updateCreatePairs.Key != null && updateCreatePairs.Key.Count > 0)
                            {
                                _uow.TermsConditionsTranslationRepository.Update(updateCreatePairs.Key);
                            }
                            if (updateCreatePairs.Value != null && updateCreatePairs.Value.Count > 0)
                            {
                                _uow.TermsConditionsTranslationRepository.Add(updateCreatePairs.Value);
                            }
                            _uow.Save();

                            return Ok(new
                            {
                                termsId = terms.Id,
                                termsVersion = terms.Version
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
                            var claimUserId = userClaims.Where(x => x.Claim == "userId").FirstOrDefault();
                            exceptionModel.UserId = claimUserId != null ? claimUserId.Value : "";

                            bool success = await _rabbitSender.PublishExceptionMessage(exceptionModel);

                            return StatusCode(StatusCodes.Status500InternalServerError, null);
                        }
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status403Forbidden, new
                        {
                            message = "been_active",
                            termsVersion = terms.Version
                        });
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            return Forbid();
        }

        [Authorize]
        [HttpPut]
        [Route("activate")]
        public async Task<IActionResult> Activate([FromBody] TermsModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "terms.admin" }))
            {
                Expression<Func<TermsConditions, bool>> filter = (x => x.Id == model.Id);

                var terms = this._uow.TermsConditionsRepository.Get(null, null, filter, string.Empty, SortDirection.Ascending).FirstOrDefault();
                if (terms != null)
                {
                    terms.UpdatedAt = DateTime.UtcNow;
                    terms.UpdatedBy = userClaims.Where(x => x.Claim == "userId").Single().Value;

                    _termsManager.DeactivateTermsConditions(terms);
                    terms.BeenActive = true;
                    terms.IsActive = true;

                    try
                    {
                        _uow.TermsConditionsRepository.Update(terms);
                        _uow.Save();

                        return Ok(new
                        {
                            termsId = terms.Id,
                            termsVersion = terms.Version
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
                        var claimUserId = userClaims.Where(x => x.Claim == "userId").FirstOrDefault();
                        exceptionModel.UserId = claimUserId != null ? claimUserId.Value : "";

                        bool success = await _rabbitSender.PublishExceptionMessage(exceptionModel);

                        return StatusCode(StatusCodes.Status500InternalServerError, null);
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            return Forbid();
        }

        [Authorize]
        [HttpPost]
        [Route("clone")]
        public async Task<IActionResult> Clone([FromBody] TermsModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "terms.admin" }))
            {
                Expression<Func<TermsConditions, bool>> filter = (x => x.Id == model.Id);
                var terms = this._uow.TermsConditionsRepository.Get(null, null, filter, string.Empty, SortDirection.Ascending).FirstOrDefault();

                Expression<Func<TermsConditionsTranslation, bool>> filterTranslation = (x => x.TermsConditionsId == model.Id);
                var termsTranslations = this._uow.TermsConditionsTranslationRepository.Get(null, null, filterTranslation, "LangKey", SortDirection.Ascending);
                if (terms != null)
                {
                    TermsConditions clonedTerms = terms.ShallowCopy();
                    clonedTerms.Id = null;
                    clonedTerms.IsActive = false;
                    clonedTerms.BeenActive = false;
                    clonedTerms.Version = _termsManager.GetNewTermsConditionsVersion();
                    clonedTerms.CreatedAt = DateTime.UtcNow;
                    clonedTerms.CreatedBy = userClaims.Where(x => x.Claim == "userId").Single().Value;
                    clonedTerms.UpdatedAt = null;
                    clonedTerms.UpdatedBy = null;

                    try
                    {
                        if (termsTranslations != null && termsTranslations.Count > 0)
                        {
                            List<TermsConditionsTranslation> clonedTranslations = new List<TermsConditionsTranslation>();
                            foreach(var translation in termsTranslations)
                            {
                                var clonedTranslation = translation.ShallowCopy();
                                clonedTranslation.TermsConditionsVersion = clonedTerms.Version;
                                clonedTranslation.TermsConditionsId = null;
                                clonedTranslations.Add(clonedTranslation);
                            }
                            clonedTerms.TermsConditionsTranslations = clonedTranslations;
                        }
                        _uow.TermsConditionsRepository.Add(clonedTerms);
                        _uow.Save();

                        return Ok(new
                        {
                            termsId = clonedTerms.Id,
                            termsVersion = clonedTerms.Version
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
                        var claimUserId = userClaims.Where(x => x.Claim == "userId").FirstOrDefault();
                        exceptionModel.UserId = claimUserId != null ? claimUserId.Value : "";

                        bool success = await _rabbitSender.PublishExceptionMessage(exceptionModel);

                        return StatusCode(StatusCodes.Status500InternalServerError, ex.Message + "| " + ex.StackTrace);
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            return Forbid();
        }
    }
}

using CommonLibrary.Entities.ViewModel;
using CommonLibrary.Enums;
using CommonLibrary.Extensions;
using MicroservicesLibrary.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Text.Json;
using UserService.Entities;
using UserService.Models;
using UserService.Services.Database;
using UserService.Services.Email;
using UserService.Services.RabbitMQ;
using UserService.Services.UserManager;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssociationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _uow;
        private readonly ITPUserManager _userManager;
        private readonly ITokenManager _tokenManager;
        private readonly IEmailSender _emailSender;
        private readonly IRabbitMQSender _rabbitSender;
        public AssociationController(IUnitOfWork uow, IConfiguration configuration, ITPUserManager userManager, ITokenManager tokenManager, IEmailSender emailSender, IRabbitMQSender rabbitMQSender)
        {
            _uow = uow;
            _configuration = configuration;
            _userManager = userManager;
            _tokenManager = tokenManager;
            _emailSender = emailSender;
            _rabbitSender = rabbitMQSender;
        }

        private ObjectResult ValidateAssociation(Association association)
        {
            if (association.Email == null || string.IsNullOrWhiteSpace(association.Email.Trim()))
            {
                return UnprocessableEntity("MandatoryFields");
            }

            Expression<Func<Association, bool>> filter = (x => x.Id != association.Id && (x.Email.ToLower() == association.Email.ToLower() || (!string.IsNullOrEmpty(association.Vat) && x.Vat.ToLower() == association.Vat.ToLower()))
            );

            var existsAssociation = this._uow.AssociationRepository.Get(null, null, filter, string.Empty, SortDirection.Ascending).FirstOrDefault();
            if (existsAssociation != null)
            {
                ConflictModel conflict = new ConflictModel();
                conflict.ConflictType = ConflictType.DuplicateValue;
                conflict.Entity = typeof(Scope).Name;

                if (existsAssociation.Email.ToLower() == association.Email.ToLower())
                {
                    var propertyInfo = typeof(Association).GetProperty("Email");
                    conflict.Field = propertyInfo != null ? propertyInfo.Name : "Unknown";
                }

                if (existsAssociation.Vat.ToLower() == association.Vat.ToLower())
                {
                    var propertyInfo = typeof(Association).GetProperty("Vat");
                    conflict.Field = propertyInfo != null ? propertyInfo.Name : "Unknown";
                }

                return Conflict(conflict);
            }

            return Ok(null);
        }

        private AssociationModel ParseEntityToModel(Association association, List<AssociationProfileTranslation>? profileDataTranslations, List<User>? users)
        {
            AssociationModel model = new AssociationModel();
            model.Id = association.Id;
            model.Name = association.Name;
            model.Email = association.Email;
            model.Phone = association.Phone;
            model.Address = association.Address;
            model.Town = association.Town;
            model.PostalCode = association.PostalCode;
            model.DistrictCode = association.DistrictCode;
            model.MunicipalityCode = association.MunicipalityCode;
            model.Vat = association.Vat;
            model.CoverImage = association.CoverImage;
            model.LogoImage = association.LogoImage;
            model.Filename = association.Filename;
            model.Description = association.Description;
            model.Website = association.Website;
            model.Latitude = association.Latitude;
            model.Longitude = association.Longitude;
            model.ContractStartDate = association.ContractStartDate;
            model.ContractEndDate = association.ContractEndDate;
            model.IsActive = association.IsActive;
            model.IsVerified = association.IsVerified;
            model.IsEmailVerified = association.IsEmailVerified;

            if (users != null)
            {
                model.AssociationUsers = new List<UserReadModel>();
                foreach (var user in users)
                {
                    model.AssociationUsers.Add(new UserReadModel
                    {
                        Id = user.Id,
                        Name = user.Name,
                        UserName = user.UserName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt,
                        IsVerified = user.IsVerified,
                        IsActive = user.IsActive,
                        AssociationName = association.Name,
                        AssociationId = association.Id
                    });
                }
            }

            if (profileDataTranslations != null)
            {
                model.AssociationProfileDataList = new List<AssociationModel.ProfileDataModel>();
                foreach (var translation in profileDataTranslations)
                {
                    if (model.AssociationProfileDataList != null)
                    {
                        var profileDataExists = model.AssociationProfileDataList.Find(x => x.PageWidgetKey == translation.PageContentKey);
                        if (profileDataExists != null)
                        {
                            profileDataExists.DataLanguages.Add(
                            new AssociationModel.DataLanguageModel
                            {
                                LangCode = translation.LangKey,
                                PageWidgetData = translation.DataBlocksJson
                            });
                        }
                        else
                        {
                            model.AssociationProfileDataList.Add(new AssociationModel.ProfileDataModel
                            {
                                PageWidgetKey = translation.PageContentKey,
                                DataLanguages = new List<AssociationModel.DataLanguageModel>{
                                new AssociationModel.DataLanguageModel
                                {
                                    LangCode = translation.LangKey,
                                    PageWidgetData = translation.DataBlocksJson
                                }
                            }
                            });
                        }
                    }
                }
            }

            return model;
        }

        private List<AssociationModel> ParseEntitiesToModel(List<Association> associations, List<AssociationProfileTranslation>? profileDataTranslations)
        {
            List<AssociationModel> models = new List<AssociationModel>();
            foreach (var association in associations)
            {
                models.Add(ParseEntityToModel(association, profileDataTranslations, null));
            }
            return models;
        }

        private KeyValuePair<List<AssociationProfileTranslation>, List<AssociationProfileTranslation>> MapModelTranslationsToEntities(bool isCreate, Association association, AssociationModel model)
        {
            KeyValuePair<List<AssociationProfileTranslation>, List<AssociationProfileTranslation>> translationsProcessed = new KeyValuePair<List<AssociationProfileTranslation>, List<AssociationProfileTranslation>>();
            if (model != null)
            {
                List<AssociationProfileTranslation> toUpdate = new List<AssociationProfileTranslation>();
                List<AssociationProfileTranslation> toCreate = new List<AssociationProfileTranslation>();

                List<AssociationProfileTranslation> transformedBlocks = new List<AssociationProfileTranslation>();
                foreach (var dataBlock in model.AssociationProfileDataList)
                {
                    foreach(var dataTranslation in dataBlock.DataLanguages)
                    {
                        var transformedBlock = new AssociationProfileTranslation();
                        if (!isCreate && association.Id.HasValue)
                        {
                            transformedBlock.AssociationId = association.Id.Value;
                        }
                        transformedBlock.PageContentKey = dataBlock.PageWidgetKey;
                        transformedBlock.LangKey = dataTranslation.LangCode;
                        transformedBlock.DataBlocksJson = dataTranslation.PageWidgetData;
                        transformedBlocks.Add(transformedBlock);
                    }
                }
                foreach (var dataBlock in transformedBlocks)
                {
                    AssociationProfileTranslation? associationProfileTranslationFound = null;
                    if (!isCreate)
                    {
                        Expression<Func<AssociationProfileTranslation, bool>> filterTranslation = (x => x.LangKey == dataBlock.LangKey && x.AssociationId == association.Id);
                        associationProfileTranslationFound = this._uow.AssociationProfileTranslationRepository.Get(null, null, filterTranslation, "LangKey", SortDirection.Ascending).FirstOrDefault();
                    }
                    if (associationProfileTranslationFound != null)
                    {
                        associationProfileTranslationFound.DataBlocksJson = dataBlock.DataBlocksJson;
                        toUpdate.Add(associationProfileTranslationFound);
                    }
                    else
                    {
                        toCreate.Add(dataBlock);
                    }
                }
                translationsProcessed = new KeyValuePair<List<AssociationProfileTranslation>, List<AssociationProfileTranslation>>(toUpdate, toCreate);
            }


            return translationsProcessed;
        }

        private Association MapModelToEntity(Association association, AssociationModel model, bool isProfileUpdate)
        {
            if (!string.IsNullOrEmpty(model.Name)) association.Name = model.Name;
            if (!string.IsNullOrEmpty(model.Email)) association.Email = model.Email;
            if (!string.IsNullOrEmpty(model.Phone)) association.Phone = model.Phone;
            if (!string.IsNullOrEmpty(model.Address)) association.Address = model.Address;
            if (!string.IsNullOrEmpty(model.Town)) association.Town = model.Town;
            if (!string.IsNullOrEmpty(model.PostalCode)) association.PostalCode = model.PostalCode;
            if (!string.IsNullOrEmpty(model.DistrictCode)) association.PostalCode = model.DistrictCode;
            if (!string.IsNullOrEmpty(model.MunicipalityCode)) association.PostalCode = model.MunicipalityCode;
            if (!string.IsNullOrEmpty(model.Vat)) association.Vat = model.Vat;
            if (!string.IsNullOrEmpty(model.CoverImage)) association.LogoImage = model.CoverImage;
            if (!string.IsNullOrEmpty(model.LogoImage)) association.LogoImage = model.LogoImage;
            if (!string.IsNullOrEmpty(model.Filename)) association.Filename = model.Filename;
            if (!string.IsNullOrEmpty(model.Description)) association.Description = model.Description;
            if (!string.IsNullOrEmpty(model.Website)) association.Website = model.Website;
            if (model.Latitude.HasValue) association.Latitude = model.Latitude;
            if (model.Longitude.HasValue) association.Longitude = model.Longitude;
            if (model.ContractStartDate.HasValue) association.ContractStartDate = model.ContractStartDate;
            if (model.ContractEndDate.HasValue) association.ContractEndDate = model.ContractEndDate;
            if (!isProfileUpdate)
            {
                if (model.IsActive.HasValue)
                {
                    association.IsActive = model.IsActive;
                }
                else if (!association.Id.HasValue)
                {
                    association.IsActive = model.IsActive;
                }
                if (model.IsVerified.HasValue)
                {
                    association.IsVerified = model.IsVerified;
                }
                else if (!association.Id.HasValue)
                {
                    association.IsVerified = model.IsVerified;
                }
                if (model.IsEmailVerified.HasValue)
                {
                    association.IsEmailVerified = model.IsEmailVerified;
                }
                else if (!association.Id.HasValue)
                {
                    association.IsEmailVerified = model.IsEmailVerified;
                }
            }

            return association;
        }

        [HttpPost]
        [Route("search-association")]
        public async Task<IActionResult> SearchAssociation([FromBody] AssociationValidationModel model)
        {
            var association = _uow.AssociationRepository.Get(null, null, (x => x.Email == model.Email || x.Vat == model.Vat), "Name").FirstOrDefault();
            if (association != null)
            {
                return Ok(new
                {
                    message = "association_exists"
                });
            }
            return NotFound();
        }

        [HttpPost]
        [Route("validate-vat")]
        public async Task<IActionResult> ValidateVat([FromBody] AssociationValidationModel model)
        {
            var association = _uow.AssociationRepository.Get(null, null, (x => x.Vat == model.Vat), "Name").FirstOrDefault();
            if (association != null)
            {
                // TODO: remote validation of VAT number
                return Ok(new
                {
                    message = "association_exists"
                });
            }
            return NotFound();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get(string? searchText, bool? isActive, bool? isVerified, int? offset, int? limit, string sort, string sortDirection)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "users.write", "associationusers.write" }))
            {
                try
                {
                    offset = offset ?? 1;

                    searchText = searchText == null ? string.Empty : searchText.Trim().ToLower();
                    sort = sort ?? "Name";
                    SortDirection direction = sortDirection == "desc" ? SortDirection.Descending : SortDirection.Ascending;

                    Expression<Func<Association, bool>> filter = (x => (x.Name.ToLower().Contains(searchText.ToLower()) || x.Email.ToLower().Contains(searchText.ToLower()))
                    && (!isActive.HasValue || x.IsActive == isActive.Value) && (!isVerified.HasValue || x.IsEmailVerified == isVerified.Value));


                    var _associations = _uow.AssociationRepository.Get(offset, limit, filter, sort, direction, string.Empty);

                    int totalCount = _uow.AssociationRepository.Count(filter);

                    Request.HttpContext.Response.Headers.Add("X-Total-Count", totalCount.ToString());

                    return _associations != null ? Ok(new
                    {
                        associations = _associations
                    })
                    : NotFound(new List<Association>());
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
                        isVerified = isVerified,
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

        private async Task<IActionResult> GetAssociationData(int id)
        {
            var association = _uow.AssociationRepository.GetById(id);

            if (association != null)
            {
                Expression<Func<User, bool>> filter = (x => x.AssociationId == association.Id);
                var users = _uow.UserRepository.Get(null, null, filter, "Id", SortDirection.Ascending, String.Empty);

                Expression<Func<AssociationProfileTranslation, bool>> filterProfileData = (x => x.AssociationId == association.Id);
                var profileDataTranslations = _uow.AssociationProfileTranslationRepository.Get(null, null, filterProfileData, "PageContentKey", SortDirection.Ascending, String.Empty);

                return Ok(new
                {
                    associationData = ParseEntityToModel(association, profileDataTranslations, users)
                });
            }
            else
            {
                return NotFound(null);
            }
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
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "users.write", "associationusers.write" }))
            {
                try
                {
                    return await GetAssociationData(id);
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
                        associationId = id
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
        [HttpGet]
        [Route("profile")]
        public async Task<IActionResult> Profile()
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", "associationId", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            var associationClaim = userClaims.Where(x => x.Claim == "associationId").FirstOrDefault();
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;
            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "AssociationAdmin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "association.admin" })
                && associationClaim != null)
            {
                int associationId = 0;
                int.TryParse(associationClaim.Value, out associationId);
                return await GetAssociationData(associationId);
            }
            return Unauthorized();
        }

        [Authorize]
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] AssociationModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "users.write" }))
            {
                Association association = MapModelToEntity(new Association(), model, false);

                ObjectResult _validate = this.ValidateAssociation(association);
                if (_validate.StatusCode != StatusCodes.Status200OK)
                {
                    return _validate;
                }
                association.CanonicalNameAlias = association.Name.ToLower().Replace(" ", "-");
                association.CanonicalNameAlias = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(association.CanonicalNameAlias));

                _uow.AssociationRepository.Add(association);
                _uow.Save();

                return Ok(new
                {
                    associationId = association.Id
                });
            }
            return Forbid();
        }

        private async Task<IActionResult> updateAssociationData(int? associationId, AssociationModel model, string? whoUpdated, bool isProfileUpdate)
        {
            Expression<Func<Association, bool>> filter = (x => x.Id == associationId);

            var association = this._uow.AssociationRepository.Get(null, null, filter, string.Empty, SortDirection.Ascending).FirstOrDefault();
            if (association != null)
            {
                association = MapModelToEntity(association, model, isProfileUpdate);

                ObjectResult _validate = this.ValidateAssociation(association);
                if (_validate.StatusCode != StatusCodes.Status200OK)
                {
                    return _validate;
                }

                try
                {
                    _uow.AssociationRepository.Update(association);
                    _uow.Save();

                    var updateCreatePairsBlocks = MapModelTranslationsToEntities(false, association, model);

                    if (updateCreatePairsBlocks.Key != null && updateCreatePairsBlocks.Key.Count > 0)
                    {
                        _uow.AssociationProfileTranslationRepository.Update(updateCreatePairsBlocks.Key);
                    }
                    if (updateCreatePairsBlocks.Value != null && updateCreatePairsBlocks.Value.Count > 0)
                    {
                        _uow.AssociationProfileTranslationRepository.Add(updateCreatePairsBlocks.Value);
                    }
                    _uow.Save();

                    return Ok(new
                    {
                        associationId = association.Id
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
                    exceptionModel.InputDataJson = JsonSerializer.Serialize(new
                    {
                        associationId = associationId,
                        associationModel = model,
                        whoUpdated = whoUpdated,
                        isProfileUpdate = isProfileUpdate
                    });
                    exceptionModel.UserId = whoUpdated;

                    bool success = await _rabbitSender.PublishExceptionMessage(exceptionModel);

                    return StatusCode(StatusCodes.Status500InternalServerError, null);
                }
            }
            else
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Put([FromBody] AssociationModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "users.write" }))
            {
                return await updateAssociationData(model.Id,model, userClaims.Where(x => x.Claim == "userId").First().Value, false);
            }
            return Forbid();
        }

        [Authorize]
        [HttpPut]
        [Route("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] AssociationModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", "associationId", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            var associationClaim = userClaims.Where(x => x.Claim == "associationId").FirstOrDefault();
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;
            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "AssociationAdmin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "association.admin" })
                && associationClaim != null)
            {
                int associationId = 0;
                int.TryParse(associationClaim.Value, out associationId);
                return await updateAssociationData(associationId, model, userClaims.Where(x => x.Claim == "userId").First().Value, true);
            }
            return Unauthorized();
        }

        [Authorize]
        [HttpPost]
        [Route("resend")]
        public async Task<IActionResult> Resend([FromBody] AssociationsActionsModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "users.write" })
                && model.AssociationIds != null && model.AssociationIds.Count > 0)
            {
                Expression<Func<Association, bool>> filter = (x => model.AssociationIds.Contains(x.Id.Value) && x.IsEmailVerified == false);
                var associationsToApprove = _uow.AssociationRepository.Get(null, null, filter, "Name", SortDirection.Ascending);
                var associationsEmailError = new List<int>();
                foreach (var association in associationsToApprove)
                {
                    var associationClaims = await _userManager.GetAssociationClaimsConfirmEmail(association);

                    var associationTokenData = _tokenManager.GetToken(associationClaims, 1440, null);

                    var associationEmailLink = _configuration["ApplicationSettings:RecoverPasswordBaseUrl"] + _configuration["ApplicationSettings:ConfirmEmailUri"] + "?t=" + associationTokenData.Token;
                    bool associationEmailSuccess = await _emailSender.SendActivateAssociationEmail(association.Email, "pt-PT", association, associationEmailLink);

                    if (!associationEmailSuccess)
                    {
                        associationsEmailError.Add(association.Id.Value);
                    }
                }

                return Ok(new
                {
                    associations = model.AssociationIds,
                    associationsEmailError = associationsEmailError
                });
            }
            return Forbid();
        }

        [Authorize]
        [HttpPost]
        [Route("approve")]
        public async Task<IActionResult> Approve([FromBody] AssociationsActionsModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "users.write" })
                && model.AssociationIds != null && model.AssociationIds.Count > 0)
            {
                Expression<Func<Association, bool>> filter = (x => model.AssociationIds.Contains(x.Id.Value) && (x.IsActive == false || x.IsVerified == false));
                var associationsToApprove = _uow.AssociationRepository.Get(null, null, filter, "Name", SortDirection.Ascending);
                List<string> approvedEmails = new List<string>();
                foreach (var association in associationsToApprove)
                {
                    association.IsActive = true;
                    association.IsVerified = true;
                    association.UpdatedAt = DateTime.UtcNow;
                    approvedEmails.Add(association.Email);
                }

                _uow.AssociationRepository.Update(associationsToApprove);
                _uow.Save();

                var associationEmailLink = _configuration["ApplicationSettings:RecoverPasswordBaseUrl"] + "/auth/login/cover";
                bool associationEmailSuccess = await _emailSender.SendBulkAssociationActivatedEmail(approvedEmails, "pt-PT", associationsToApprove, associationEmailLink);

                return Ok(new
                {
                    associations = model.AssociationIds
                });
            }
            return Forbid();
        }

        [Authorize]
        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> Delete([FromBody] AssociationsActionsModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "users.write" })
                && model.AssociationIds != null && model.AssociationIds.Count > 0)
            {
                // TODO: implement delete logic for users

                return Ok(new
                {
                    associations = model.AssociationIds
                });
            }
            return Forbid();
        }
    }
}

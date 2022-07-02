using CommonLibrary.Entities.ViewModel;
using CommonLibrary.Enums;
using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Text.Json;
using UserService.Entities;
using UserService.Models.Users;
using UserService.Services.Database;
using UserService.Services.RabbitMQ;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssociationTypeController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IRabbitMQSender _rabbitSender;
        private readonly IConfiguration _configuration;
        public AssociationTypeController(IUnitOfWork uow, IConfiguration configuration, IRabbitMQSender rabbitMQSender)
        {
            _uow = uow;
            _configuration = configuration;
            _rabbitSender = rabbitMQSender;
        }

        private ObjectResult ValidateAssociationType(AssociationType associationType)
        {
            if (associationType.Code == null || string.IsNullOrWhiteSpace(associationType.Code.Trim()))
            {
                return UnprocessableEntity("MandatoryFields");
            }

            Expression<Func<AssociationType, bool>> filter = (x => x.Id != associationType.Id && (x.Code.ToLower().Equals(associationType.Code.ToLower()))
            );

            var existsType = this._uow.AssociationTypeRepository.Get(null, null, filter, string.Empty, SortDirection.Ascending).FirstOrDefault();
            if (existsType != null)
            {
                ConflictModel conflict = new ConflictModel();
                conflict.ConflictType = ConflictType.DuplicateValue;
                conflict.Entity = typeof(AssociationType).Name;

                if (existsType.Code.ToLower() == associationType.Code.ToLower())
                {
                    var propertyInfo = typeof(AssociationType).GetProperty("Code");
                    conflict.Field = propertyInfo != null ? propertyInfo.Name : "Unknown";
                }

                return Conflict(conflict);
            }

            return Ok(null);
        }

        private AssociationTypeModel ParseEntityToModel(AssociationType associationType, bool isSysAdmin)
        {
            AssociationTypeModel model = new AssociationTypeModel();
            if (isSysAdmin)
            {
                model.AssociationTypeId = associationType.Id;
            }
            model.Code = associationType.Code;
            model.Label = associationType.Label;
            model.Description = associationType.Description;
            model.VatRequired = associationType.VatRequired;

            return model;
        }

        private List<AssociationTypeModel> ParseEntitiesToModel(List<AssociationType> associationTypes, bool isSysAdmin)
        {
            List<AssociationTypeModel> models = new List<AssociationTypeModel>();
            foreach (var associationType in associationTypes)
            {
                models.Add(ParseEntityToModel(associationType, isSysAdmin));
            }
            return models;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get(string? searchText, int? offset, int? limit, string sort, string sortDirection)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;
            string claimUserRole = PermissionsHelper.GetUserRoleFromClaim(userClaims);

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAny(scopes, new List<string> { "users.write" }))
            {
                try
                {
                    offset = offset ?? 1;

                    searchText = searchText == null ? string.Empty : searchText.Trim().ToLower();
                    sort = sort ?? "Code";
                    SortDirection direction = sortDirection == "desc" ? SortDirection.Descending : SortDirection.Ascending;

                    Expression<Func<AssociationType, bool>> filter = (x => (x.Code.ToLower().Contains(searchText.ToLower()) || x.Label.ToLower().Contains(searchText.ToLower()) || x.Description.ToLower().Contains(searchText.ToLower())));

                    var _roles = _uow.AssociationTypeRepository.Get(offset, limit, filter, sort, direction, string.Empty);

                    int totalCount = _uow.AssociationTypeRepository.Count(filter);

                    Request.HttpContext.Response.Headers.Add("X-Total-Count", totalCount.ToString());

                    return _roles != null ? Ok(new
                    {
                        associationTypes = ParseEntitiesToModel(_roles, claimUserRole == "Admin" ? true : false)
                    })
                    : NotFound(new List<AssociationTypeModel>());
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

        [HttpGet]
        [Route("public")]
        public async Task<IActionResult> GetPublic()
        {
            try
            {
                Expression<Func<AssociationType, bool>> filter = (x => x.Id.HasValue);
                var associationTypes = _uow.AssociationTypeRepository.Get(null, null, filter, "Label", SortDirection.Ascending);

                if (associationTypes != null)
                {
                    int totalCount = _uow.AssociationTypeRepository.Count(filter);

                    Request.HttpContext.Response.Headers.Add("X-Total-Count", totalCount.ToString());

                    return Ok(new
                    {
                        associationTypes = ParseEntitiesToModel(associationTypes, false)
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
                exceptionModel.UserId = "";

                bool success = await _rabbitSender.PublishExceptionMessage(exceptionModel);

                return StatusCode(StatusCodes.Status500InternalServerError, null);
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
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "users.write" }))
            {
                try
                {
                    var associationType = _uow.AssociationTypeRepository.GetById(id);
                    if (associationType != null)
                    {
                        return Ok(new
                        {
                            role = ParseEntityToModel(associationType, true)
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
                        associationTypeId = id
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
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] AssociationTypeModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;
            var claimUserId = userClaims.Where(x => x.Claim == "userId").FirstOrDefault();
            string userId = claimUserId != null ? claimUserId.Value : "";

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "users.write" }))
            {
                AssociationType associationType = new()
                {
                    Code = model.Code,
                    Label = model.Label,
                    Description = model.Description,
                    VatRequired = model.VatRequired,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId
                };

                ObjectResult _validate = this.ValidateAssociationType(associationType);
                if (_validate.StatusCode != StatusCodes.Status200OK)
                {
                    return _validate;
                }

                _uow.AssociationTypeRepository.Add(associationType);
                _uow.Save();

                return Ok(new
                {
                    associationTypeId = associationType.Id,
                });
            }
            return Forbid();
        }

        [Authorize]
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Put([FromBody] AssociationTypeModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;
            var claimUserId = userClaims.Where(x => x.Claim == "userId").FirstOrDefault();
            string userId = claimUserId != null ? claimUserId.Value : "";

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "users.write" }))
            {
                Expression<Func<AssociationType, bool>> filter = (x => x.Id == model.AssociationTypeId);

                var associationType = this._uow.AssociationTypeRepository.Get(null, null, filter, string.Empty, SortDirection.Ascending).FirstOrDefault();
                if (associationType != null)
                {
                    associationType.Code = model.Code;
                    associationType.Label = model.Label;
                    associationType.Description = model.Description;
                    associationType.VatRequired = model.VatRequired;
                    associationType.UpdatedAt = DateTime.Now;
                    associationType.UpdatedBy = userId;

                    ObjectResult _validate = this.ValidateAssociationType(associationType);
                    if (_validate.StatusCode != StatusCodes.Status200OK)
                    {
                        return _validate;
                    }

                    try
                    {
                        _uow.AssociationTypeRepository.Update(associationType);
                        _uow.Save();

                        return Ok(new
                        {
                            associationTypeId = associationType.Id
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
                        exceptionModel.UserId = userId;

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
    }
}

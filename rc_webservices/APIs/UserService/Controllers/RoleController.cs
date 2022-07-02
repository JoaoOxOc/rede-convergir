using CommonLibrary.Entities.ViewModel;
using CommonLibrary.Enums;
using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Text.Json;
using UserService.Entities;
using UserService.Models;
using UserService.Services.Database;
using UserService.Services.RabbitMQ;
using UserService.Services.UserManager;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IRoleScopeManager _roleScopeManager;
        private readonly IRabbitMQSender _rabbitSender;

        public RoleController(IUnitOfWork uow, IConfiguration configuration, RoleManager<IdentityRole> roleManager, IRoleScopeManager roleScopeManager, IRabbitMQSender rabbitMQSender)
        {
            _uow = uow;
            _configuration = configuration;
            _roleManager = roleManager;
            _roleScopeManager = roleScopeManager;
            _rabbitSender = rabbitMQSender;
        }

        private ObjectResult ValidateRole(IdentityRole role)
        {
            if (role.Name == null || string.IsNullOrWhiteSpace(role.Name.Trim()))
            {
                return UnprocessableEntity("MandatoryFields");
            }

            Expression<Func<IdentityRole, bool>> filter = (x => x.Id != role.Id && (x.Name.ToLower().Equals(role.Name.ToLower()))
            );

            var existsRole = this._uow.IdentityRoleRepository.Get(null, null, filter, string.Empty, SortDirection.Ascending).FirstOrDefault();
            if (existsRole != null)
            {
                ConflictModel conflict = new ConflictModel();
                conflict.ConflictType = ConflictType.DuplicateValue;
                conflict.Entity = typeof(IdentityRole).Name;

                if (existsRole.Name.ToLower() == role.Name.ToLower())
                {
                    var propertyInfo = typeof(IdentityRole).GetProperty("Name");
                    conflict.Field = propertyInfo != null ? propertyInfo.Name : "Unknown";
                }

                return Conflict(conflict);
            }

            return Ok(null);
        }

        private RoleModel ParseEntityToModel(IdentityRole role, List<RoleScope>? scopes, bool isSysAdmin)
        {
            RoleModel model = new RoleModel();
            if (isSysAdmin)
            {
                model.RoleId = role.Id;
            }
            model.NormalizedRoleName = role.NormalizedName;
            model.RoleName = role.Name;

            if (scopes != null)
            {
                model.Scopes = new List<ScopeModel>();
                foreach (var scope in scopes)
                {
                    var scopeModel = new ScopeModel();
                    if (isSysAdmin)
                    {
                        scopeModel.ScopeId = scope.Scope.Id;
                    }
                    scopeModel.ScopeIdentifier = scope.Scope.ScopeName;
                    scopeModel.Description = scope.Scope.Description;
                    model.Scopes.Add(scopeModel);
                }
            }

            return model;
        }

        private List<RoleModel> ParseEntitiesToModel(List<IdentityRole> roles, bool isSysAdmin)
        {
            List<RoleModel> models = new List<RoleModel>();
            foreach (var role in roles)
            {
                models.Add(ParseEntityToModel(role, null, isSysAdmin));
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
            var associationClaim = userClaims.Where(x => x.Claim == "associationId").FirstOrDefault();
            int claimAssociationId = 0;
            if (associationClaim != null)
            {
                int.TryParse(associationClaim.Value, out claimAssociationId);
            }

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin", "AssociationAdmin" })
                && PermissionsHelper.ValidateUserScopesPermissionAny(scopes, new List<string> { "users.write", "associationusers.write" }))
            {
                try
                {
                    offset = offset ?? 1;

                    searchText = searchText == null ? string.Empty : searchText.Trim().ToLower();
                    sort = sort ?? "Name";
                    SortDirection direction = sortDirection == "desc" ? SortDirection.Descending : SortDirection.Ascending;

                    Expression<Func<IdentityRole, bool>> filter = (x => (claimUserRole == "Admin" || (claimUserRole == "AssociationAdmin" && (x.Name.ToLower().Contains("association"))))
                        && x.Name.ToLower().Contains(searchText.ToLower()));

                    var _roles = _uow.IdentityRoleRepository.Get(offset, limit, filter, sort, direction, string.Empty);

                    int totalCount = _uow.IdentityRoleRepository.Count(filter);

                    Request.HttpContext.Response.Headers.Add("X-Total-Count", totalCount.ToString());

                    return _roles != null ? Ok(new
                    {
                        roles = ParseEntitiesToModel(_roles, claimUserRole == "Admin" ? true : false)
                    })
                    : NotFound(new List<RoleModel>());
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

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
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
                    var role = _uow.IdentityRoleRepository.GetById(id);
                    if (role != null)
                    {
                        Expression<Func<RoleScope, bool>> scopeFilter = (x => x.RoleId == role.Id);
                        var roleScopes = _uow.RoleScopeRepository.Get(null,null,scopeFilter, "Scope.ScopeName", SortDirection.Ascending, "Scope");

                        return Ok(new
                        {
                            role = ParseEntityToModel(role, roleScopes, true)
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
                        roleId = id
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
        public async Task<IActionResult> Register([FromBody] RoleModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "users.write" }))
            {
                IdentityRole role = new()
                {
                    Name = model.RoleName,
                    NormalizedName = !string.IsNullOrEmpty(model.NormalizedRoleName) ? model.NormalizedRoleName : model.RoleName.ToUpper(),
                };

                ObjectResult _validate = this.ValidateRole(role);
                if (_validate.StatusCode != StatusCodes.Status200OK)
                {
                    return _validate;
                }

                var creationResult = await _roleManager.CreateAsync(role);
                if (creationResult == null || !creationResult.Succeeded)
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Role creation failed! Please check role details and try again." });

                var roleScopes = _roleScopeManager.ReplaceRoleScopes(role.Id, model.Scopes);

                return Ok(new
                {
                    roleId = role.Id,
                    scopes = roleScopes != null ? roleScopes.Select(x => x.ScopeId) : null
                });
            }
            return Forbid();
        }

        [Authorize]
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Put([FromBody] RoleModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "users.write" }))
            {
                Expression<Func<IdentityRole, bool>> filter = (x => x.Id == model.RoleId);

                var role = this._uow.IdentityRoleRepository.Get(null, null, filter, string.Empty, SortDirection.Ascending).FirstOrDefault();
                if (role != null)
                {
                    role.Name = model.RoleName;
                    role.NormalizedName = !string.IsNullOrEmpty(model.NormalizedRoleName) ? model.NormalizedRoleName : model.RoleName.ToUpper();

                    ObjectResult _validate = this.ValidateRole(role);
                    if (_validate.StatusCode != StatusCodes.Status200OK)
                    {
                        return _validate;
                    }

                    try
                    {
                        var updateResult = await _roleManager.UpdateAsync(role);
                        if (updateResult == null || !updateResult.Succeeded)
                            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Role update failed! Please check role details and try again." });

                        var roleScopes = _roleScopeManager.ReplaceRoleScopes(role.Id, model.Scopes);

                        return Ok(new
                        {
                            roleId = role.Id,
                            scopes = roleScopes != null ? roleScopes.Select(x => x.ScopeId) : null
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
    }
}

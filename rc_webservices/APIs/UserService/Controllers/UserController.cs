using CommonLibrary.Entities.ViewModel;
using CommonLibrary.Enums;
using CommonLibrary.Extensions;
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
using UserService.Services.TermsManager;
using UserService.Services.UserManager;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _configuration;
        private readonly ITPUserManager _userManager;
        private readonly IUserRoleManager _userRoleManager;
        private readonly ITokenManager _tokenManager;
        private readonly IEmailSender _emailSender;
        private readonly ITermsManager _termsManager;
        private readonly IRabbitMQSender _rabbitSender;

        public UserController(IUnitOfWork uow, IConfiguration configuration, ITPUserManager userManager, IUserRoleManager userRoleManager, ITokenManager tokenManager, IEmailSender emailSender, ITermsManager termsManager, IRabbitMQSender rabbitMQSender)
        {
            _uow = uow;
            _configuration = configuration;
            _userManager = userManager;
            _userRoleManager = userRoleManager;
            _tokenManager = tokenManager;
            _emailSender = emailSender;
            _termsManager = termsManager;
            _rabbitSender = rabbitMQSender;
        }

        private ObjectResult ValidateUser(User user)
        {
            if (user.Name == null || string.IsNullOrWhiteSpace(user.Name.Trim()) ||
                user.UserName == null || string.IsNullOrWhiteSpace(user.UserName.Trim())
                || user.Email == null || string.IsNullOrWhiteSpace(user.Email.Trim()))
            {
                return UnprocessableEntity("MandatoryFields");
            }

            Expression<Func<User, bool>> filter = (x => x.Id != user.Id && (x.UserName.Equals(user.UserName) || x.Email.Equals(user.Email))
            );

            var existsUser = this._uow.UserRepository.Get(null, null, filter, string.Empty, SortDirection.Ascending).FirstOrDefault();
            if (existsUser != null)
            {
                ConflictModel conflict = new ConflictModel();
                conflict.ConflictType = ConflictType.DuplicateValue;
                conflict.Entity = typeof(User).Name;

                if (existsUser.Name == user.Name)
                {
                    var propertyInfo = typeof(User).GetProperty("Name");
                    conflict.Field = propertyInfo != null ? propertyInfo.Name : "Unknown";
                }

                if (existsUser.UserName == user.UserName)
                {
                    var propertyInfo = typeof(User).GetProperty("UserName");
                    conflict.Field = propertyInfo != null ? propertyInfo.Name : "Unknown";
                }

                if (existsUser.Email == user.Email)
                {
                    var propertyInfo = typeof(User).GetProperty("Email");
                    conflict.Field = propertyInfo != null ? propertyInfo.Name : "Unknown";
                }

                return Conflict(conflict);
            }

            return Ok(null);
        }

        private UserReadModel ParseEntityToModel(User user)
        {
            UserReadModel model = new UserReadModel();

            var userRole = _userRoleManager.GetUserRoleByUserId(user.Id);
            //var userRoles = await _userManager.GetRolesAsync(user);

            if (user != null)
            {

                model.Id = user.Id;
                model.Name = user.Name;
                model.UserName = user.UserName;
                model.Email = user.Email;
                model.PhoneNumber = user.PhoneNumber;
                model.CreatedAt = user.CreatedAt;
                model.UpdatedAt = user.UpdatedAt;
                model.IsVerified = user.IsVerified;
                model.IsActive = user.IsActive;
                model.IsEmailVerified = user.IsEmailVerified;
                model.AssociationName = user.Association?.Name;
                model.AssociationId = user.Association?.Id;
                model.AssociationLogoImage = user.Association?.LogoImage;
                model.UserRole = userRole?.Name;
            }

            return model;
        }

        private List<UserReadModel> ParseEntitiesToModel(List<User> users)
        {
            List<UserReadModel> models = new List<UserReadModel>();
            foreach (var user in users)
            {
                models.Add(ParseEntityToModel(user));
            }
            return models;
        }

        private User MapModelToEntity(User user, UserReadModel model)
        {
            if (!string.IsNullOrEmpty(model.Name))
            {
                user.Name = model.Name;
            }
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.UserName = model.UserName;
            if (model.IsActive.HasValue)
            {
                user.IsActive = model.IsActive.Value;
            }
            else if (!string.IsNullOrEmpty(user.Id))
            {
                user.IsActive = model.IsActive.HasValue ? model.IsActive.Value : false;
            }
            if (model.IsVerified.HasValue)
            {
                user.IsVerified = model.IsVerified.Value;
            }
            else if (!string.IsNullOrEmpty(user.Id))
            {
                user.IsVerified = model.IsVerified.HasValue ? model.IsVerified.Value : false;
            }
            if (model.IsEmailVerified.HasValue)
            {
                user.IsEmailVerified = model.IsEmailVerified;
                user.EmailConfirmed = model.IsEmailVerified.Value;
            }
            else if (!string.IsNullOrEmpty(user.Id))
            {
                user.IsEmailVerified = model.IsEmailVerified;
                user.EmailConfirmed = model.IsEmailVerified.HasValue ? model.IsEmailVerified.Value : false;
            }

            return user;
        }

        

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get(string? searchText, int? associationId, string? userRole, bool? isActive, bool? isVerified, int? offset, int? limit, string sort, string sortDirection)
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

                    var filterbyUserIds = _userRoleManager.GetUserIdsByRole(userRole);

                    Expression<Func<User, bool>> filter = (x => (claimUserRole == "AssociationAdmin" || string.IsNullOrEmpty(userRole) || userRole == "all" || filterbyUserIds.Contains(x.Id))
                    && (x.Name.ToLower().Contains(searchText.ToLower()) || x.UserName.ToLower().Contains(searchText.ToLower()))
                    && (claimUserRole == "AssociationAdmin" && (claimAssociationId > 0 && x.AssociationId == claimAssociationId) || (!associationId.HasValue || x.AssociationId == associationId.Value))
                    && (!isActive.HasValue || x.IsActive == isActive.Value)
                    && (!isVerified.HasValue || (x.IsEmailVerified == isVerified.Value && x.EmailConfirmed == isVerified.Value)));

                    var _users = _uow.UserRepository.Get(offset, limit, filter, sort, direction, "Association");

                    int totalCount = _uow.UserRepository.Count(filter);

                    Request.HttpContext.Response.Headers.Add("X-Total-Count", totalCount.ToString());

                    return _users != null ? Ok(new
                    {
                        users = ParseEntitiesToModel(_users)
                    })
                    : NotFound(new List<User>());
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
                        associationId = associationId,
                        userRole = userRole,
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
                    var user = await _userManager.SearchUserById(id);

                    if (user != null && user.AssociationId.HasValue)
                    {
                        user.Association = _uow.AssociationRepository.GetById(user.AssociationId.Value);
                    }

                    return user != null ? Ok(new
                    {
                        user = ParseEntityToModel(user)
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
                        userId = id
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
        public async Task<IActionResult> RegisterUser([FromBody] RegisterModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "users.write" }))
            {
                User user = new()
                {
                    Name = model.FirstName + " " + model.LastName,
                    Email = model.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.Now,
                    UserName = model.Username,
                    NormalizedUserName = !string.IsNullOrEmpty(model.Username) ? model.Username.ToUpper() : "",
                    IsVerified = false,
                    IsActive = false,
                    IsEmailVerified = false,
                    EmailConfirmed = false,
                    TermsConsent = model.TermsConfirmed.HasValue ? model.TermsConfirmed.Value : true,
                    TermsConsentVersion = _termsManager.GetActiveTermsConditionsVersion()
                };

                ObjectResult _validate = this.ValidateUser(user);
                if (_validate.StatusCode != StatusCodes.Status200OK)
                {
                    return _validate;
                }

                var result = await _userManager.CreateUser(user, model.Password, model.UserRole);
                if (result == null || !result.Succeeded)
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

                return Ok(new
                {
                    userId = user.Id
                });
            }
            return Forbid();
        }

        [Authorize]
        [HttpGet]
        [Route("profile")]
        public async Task<IActionResult> Profile()
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", "associationId" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            if (userClaims != null && userClaims.Count > 0)
            {
                //int requesterId = Convert.ToInt32(jwtClaims.Where(x => x.Claim == "userId").Single().Value);
                string userId = userClaims.Where(x => x.Claim == "userId").Single().Value;
                var userProfile = await _userManager.GetUserProfileById(userId);

                var userRole = _userRoleManager.GetUserRoleByUserId(userId);
                if (userRole != null)
                {
                    userProfile.UserRole = userRole.Name;
                }

                return Ok(new
                {
                    userProfile = userProfile
                });
            }
            return Unauthorized();
        }

        private async Task<IActionResult> updateUserData(string? userId, string whoUpdated, ProfileModel model)
        {
            Expression<Func<User, bool>> filter = (x => x.Id == userId);

            var user = this._uow.UserRepository.Get(null, null, filter, string.Empty, SortDirection.Ascending).FirstOrDefault();
            if (user != null)
            {
                if (!string.IsNullOrEmpty(model.Name))
                {
                    user.Name = model.Name;
                }
                user.Email = model.Email;
                if (!string.IsNullOrEmpty(model.Phone))
                {
                    user.PhoneNumber = model.Phone;
                }

                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = !string.IsNullOrEmpty(whoUpdated) ? whoUpdated : userId;

                ObjectResult _validate = this.ValidateUser(user);
                if (_validate.StatusCode != StatusCodes.Status200OK)
                {
                    return _validate;
                }

                try
                {
                    _uow.UserRepository.Update(user);
                    _uow.Save();

                    return Ok(new
                    {
                        userId = user.Id
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
                        userId = userId,
                        whoUpdated = whoUpdated,
                        profileModel = model
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
        public async Task<IActionResult> Put([FromBody] ProfileModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "users.write" }))
            {
                return await updateUserData(model.UserId, userClaims.Where(x => x.Claim == "userId").Single().Value, model);
            }
            return Forbid();
        }

        [Authorize]
        [HttpPut]
        [Route("updaterole")]
        public async Task<IActionResult> UpdateRole([FromBody] UserReadModel model)
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
                Expression<Func<User, bool>> filter = (x => (claimUserRole == "Admin" || (claimUserRole == "AssociationAdmin" && claimAssociationId > 0 && x.AssociationId == claimAssociationId))
                    && x.Id == model.Id);
                var userToUpdate = _uow.UserRepository.Get(null, null, filter, "UserName", SortDirection.Ascending).FirstOrDefault();
                if (userToUpdate != null && !string.IsNullOrEmpty(model.UserRole))
                {
                    if (claimUserRole == "Admin" || model.UserRole.ToLower().Contains("association"))
                    {
                        var currentUserRole = _userRoleManager.GetUserRoleByUserId(userToUpdate.Id);
                        var removeResult = await _userManager.RemoveFromRoleAsync(userToUpdate, currentUserRole.Name);
                        if (removeResult != null && removeResult.Succeeded)
                        {
                            var addResult = await _userManager.AddUserToRole(userToUpdate, model.UserRole);
                            if (addResult != null && addResult.Succeeded)
                            {
                                userToUpdate.RefreshToken = null;
                                userToUpdate.UpdatedAt = DateTime.UtcNow;
                                userToUpdate.UpdatedBy = userClaims.Where(x => x.Claim == "userId").Single().Value;
                                _uow.UserRepository.Update(userToUpdate);
                                _uow.Save();
                                return Ok();
                            }
                            else
                            {
                                return NotFound("add_user_role");
                            }
                        }
                        else
                        {
                            return NotFound("current_user_role");
                        }
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
        [Route("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", "associationId" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            if (userClaims != null && userClaims.Count > 0)
            {
                //int requesterId = Convert.ToInt32(jwtClaims.Where(x => x.Claim == "userId").Single().Value);
                string userId = userClaims.Where(x => x.Claim == "userId").Single().Value;
                return await updateUserData(userId, userId, model);
            }
            return Unauthorized();
        }

        [Authorize]
        [HttpPost]
        [Route("resend")]
        public async Task<IActionResult> Resend([FromBody] UsersActionsModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;
            string userRole = PermissionsHelper.GetUserRoleFromClaim(userClaims);
            var associationClaim = userClaims.Where(x => x.Claim == "associationId").FirstOrDefault();
            int associationId = 0;
            if (associationClaim != null)
            {
                int.TryParse(associationClaim.Value, out associationId);
            }

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin", "AssociationAdmin" })
                && PermissionsHelper.ValidateUserScopesPermissionAny(scopes, new List<string> { "users.write", "associationusers.write" })
                && model.UserIds != null && model.UserIds.Count > 0)
            {
                Expression<Func<User, bool>> filter = (x => model.UserIds.Contains(x.Id) && x.IsEmailVerified == false && x.EmailConfirmed == false
                    && (userRole == "Admin" || (userRole == "AssociationAdmin" && associationId > 0 && x.AssociationId == associationId)));
                var usersToApprove = _uow.UserRepository.Get(null, null, filter, "UserName", SortDirection.Ascending);
                var usersEmailError = new List<string>();
                foreach (var user in usersToApprove)
                {
                    var userToApproveClaims = await _userManager.GetUserClaimsPasswordRecovery(user);

                    var userTokenData = _tokenManager.GetToken(userToApproveClaims, 1440, null);

                    var userEmailLink = _configuration["ApplicationSettings:RecoverPasswordBaseUrl"] + _configuration["ApplicationSettings:ConfirmEmailUri"] + "?t=" + userTokenData.Token;
                    bool userEmailSuccess = await _emailSender.SendActivateUserEmail(user.Email, "pt-PT", user, userEmailLink);

                    if (!userEmailSuccess)
                    {
                        usersEmailError.Add(user.Id);
                    }
                }

                return Ok(new
                {
                    users = model.UserIds,
                    usersEmailError = usersEmailError
                });
            }
            return Forbid();
        }

        [Authorize]
        [HttpPost]
        [Route("approve")]
        public async Task<IActionResult> Approve([FromBody] UsersActionsModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;
            string userRole = PermissionsHelper.GetUserRoleFromClaim(userClaims);
            var associationClaim = userClaims.Where(x => x.Claim == "associationId").FirstOrDefault();
            int associationId = 0;
            if (associationClaim != null)
            {
                int.TryParse(associationClaim.Value, out associationId);
            }

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin", "AssociationAdmin" })
                && PermissionsHelper.ValidateUserScopesPermissionAny(scopes, new List<string> { "users.write", "associationusers.write" })
                && model.UserIds != null && model.UserIds.Count > 0)
            {
                Expression<Func<User, bool>> filter = (x => model.UserIds.Contains(x.Id) && (x.IsActive == false || x.IsVerified == false)
                    && (userRole == "Admin" || (userRole == "AssociationAdmin" && associationId > 0 && x.AssociationId == associationId)));
                var usersToApprove = _uow.UserRepository.Get(null, null, filter, "UserName", SortDirection.Ascending);
                List<string> approvedEmails = new List<string>();
                foreach (var user in usersToApprove)
                {
                    user.IsActive = true;
                    user.IsVerified = true;
                    user.UpdatedAt = DateTime.UtcNow;
                    user.UpdatedBy = userClaims.Where(x => x.Claim == "userId").Single().Value;
                    approvedEmails.Add(user.Email);

                    if (userRole == "Admin" && !user.AssociationId.HasValue)
                    {
                        await _userManager.AddUserToRole(user, "User");
                    }
                    else if (userRole == "AssociationAdmin")
                    {
                        await _userManager.AddUserToRole(user, "AssociationUser");
                    }
                }

                _uow.UserRepository.Update(usersToApprove);
                _uow.Save();

                var userEmailLink = _configuration["ApplicationSettings:RecoverPasswordBaseUrl"] + _configuration["ApplicationSettings:AuthPageUri"];
                bool userEmailSuccess = await _emailSender.SendBulkUserActivatedEmail(approvedEmails, "pt-PT", usersToApprove, userEmailLink);

                return Ok(new
                {
                    users = model.UserIds
                });
            }
            return Forbid();
        }

        [Authorize]
        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> Delete([FromBody] UsersActionsModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "users.write" })
                && model.UserIds != null && model.UserIds.Count > 0)
            {
                // TODO: implement delete logic for users

                return Ok(new
                {
                    users = model.UserIds
                });
            }
            return Forbid();
        }
    }
}

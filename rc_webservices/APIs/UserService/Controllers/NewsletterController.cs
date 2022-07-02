using CommonLibrary.Entities.ViewModel;
using CommonLibrary.Enums;
using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using UserService.Entities;
using UserService.Models;
using UserService.Services.Database;
using UserService.Services.Mailchimp;
using UserService.Services.RabbitMQ;
using UserService.Services.UserManager;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsletterController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _configuration;
        private readonly ITPUserManager _userManager;
        private readonly ITokenManager _tokenManager;
        private readonly IMailchimpRepository _mailchimpRepository;
        private readonly IRabbitMQSender _rabbitSender;

        public NewsletterController(IUnitOfWork uow, IConfiguration configuration, ITPUserManager userManager, ITokenManager tokenManager, IMailchimpRepository mailchimpRepository, IRabbitMQSender rabbitMQSender)
        {
            _uow = uow;
            _configuration = configuration;
            _userManager = userManager;
            _tokenManager = tokenManager;
            _mailchimpRepository = mailchimpRepository;
            _rabbitSender = rabbitMQSender;
        }

        private ObjectResult ValidateNewsletter(NewsletterSubscription newsletter)
        {
            if (newsletter.Email == null || string.IsNullOrWhiteSpace(newsletter.Email.Trim()))
            {
                return UnprocessableEntity("MandatoryFields");
            }

            Expression<Func<NewsletterSubscription, bool>> filter = (x => x.Id != newsletter.Id && (x.Email.ToLower().Equals(newsletter.Email.ToLower()))
            );

            var existsNewsletter = this._uow.NewsletterSubscriptionRepository.Get(null, null, filter, string.Empty, SortDirection.Ascending).FirstOrDefault();
            if (existsNewsletter != null)
            {
                ConflictModel conflict = new ConflictModel();
                conflict.ConflictType = ConflictType.DuplicateValue;
                conflict.Entity = typeof(NewsletterSubscription).Name;

                if (existsNewsletter.Email.ToLower() == newsletter.Email.ToLower())
                {
                    var propertyInfo = typeof(NewsletterSubscription).GetProperty("Email");
                    conflict.Field = propertyInfo != null ? propertyInfo.Name : "Unknown";
                }

                return Conflict(conflict);
            }

            return Ok(null);
        }

        private NewsletterSubscriptionModel ParseEntityToModel(NewsletterSubscription subscription)
        {
            NewsletterSubscriptionModel model = new NewsletterSubscriptionModel();
            model.Email = subscription.Email;

            if (subscription.User != null)
            {
                model.UserId = subscription.UserId;
                model.UserFullName = subscription.User.Name;
            }

            return model;
        }

        private List<NewsletterSubscriptionModel> ParseEntitiesToModel(List<NewsletterSubscription> subscriptions)
        {
            List<NewsletterSubscriptionModel> models = new List<NewsletterSubscriptionModel>();
            foreach (var subs in subscriptions)
            {
                models.Add(ParseEntityToModel(subs));
            }
            return models;
        }

        [Authorize]
        [HttpGet("lists")]
        public async Task<IActionResult> GetLists(string? searchText, int? offset, int? limit, string? sort, string? sortDirection)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "newsletter.admin" }))
            {
                try
                {
                    offset = offset ?? 1;

                    searchText = searchText == null ? string.Empty : searchText.Trim().ToLower();
                    sort = sort ?? "Name";
                    SortDirection direction = sortDirection == "desc" ? SortDirection.Descending : SortDirection.Ascending;

                    var mailingLists = this._mailchimpRepository.GetAllMailingLists();

                    Request.HttpContext.Response.Headers.Add("X-Total-Count", mailingLists != null ? mailingLists.Count.ToString() : "0");

                    return mailingLists != null ? Ok(new
                    {
                        mailingLists = mailingLists
                    })
                    : NotFound(new List<MailChimp.Net.Models.List>());
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
        [HttpGet]
        public async Task<IActionResult> Get(string? searchText, string mailingListId, int? offset, int? limit, string? sort, string? sortDirection)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "newsletter.admin" }))
            {
                try
                {
                    offset = offset.HasValue ? offset - 1 : 0;
                    limit = limit ?? 10;

                    searchText = searchText == null ? string.Empty : searchText.Trim().ToLower();
                    sort = sort ?? "EmailAddress";
                    MailChimp.Net.Core.MemberSortOrder direction = sortDirection == "desc" ? MailChimp.Net.Core.MemberSortOrder.DESC : MailChimp.Net.Core.MemberSortOrder.ASC;

                    var listMembers = this._mailchimpRepository.GetAllMembers(mailingListId, null, offset.Value, limit.Value, direction);

                    Request.HttpContext.Response.Headers.Add("X-Total-Count", this._mailchimpRepository.GetAllMembersCount(mailingListId, null).ToString());

                    return listMembers != null ? Ok(new
                    {
                        listMembers = listMembers
                    })
                    : NotFound(new List<MailChimp.Net.Models.Member>());
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
                        mailingListId = mailingListId,
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
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "newsletter.admin" }))
            {
                try
                {
                    Expression<Func<NewsletterSubscription, bool>> filter = (x => x.Id == id);
                    var _subscription = _uow.NewsletterSubscriptionRepository.Get(1, 1, filter, "Id", SortDirection.Ascending, "User").FirstOrDefault();

                    if (_subscription != null)
                    {
                        return Ok(new
                        {
                            subscription = ParseEntityToModel(_subscription)
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
                        newsletterSubscriptionId = id
                    });
                    var claimUserId = userClaims.Where(x => x.Claim == "userId").FirstOrDefault();
                    exceptionModel.UserId = claimUserId != null ? claimUserId.Value : "";

                    bool success = await _rabbitSender.PublishExceptionMessage(exceptionModel);

                    return StatusCode(StatusCodes.Status500InternalServerError, null);
                }
            }
            return Forbid();
        }

        private async Task<IActionResult> registNewsletter(string email, string? userId)
        {
            NewsletterSubscription subscription = new()
            {
                Email = email
            };

            subscription.SubscriptionToken = _tokenManager.GetClientToken(await _userManager.GetNewsletterSubscriptionClaims()).Token;

            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.SearchUserById(userId);
                if (user != null)
                {
                    subscription.UserId = user.Id;
                }
            }

            ObjectResult _validate = this.ValidateNewsletter(subscription);
            if (_validate.StatusCode != StatusCodes.Status200OK)
            {
                return _validate;
            }

            _uow.NewsletterSubscriptionRepository.Add(subscription);
            _uow.Save();

            return Ok(new
            {
                subscriptionId = subscription.Id
            });
        }

        [Authorize]
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody]NewsletterSubscriptionModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "newsletter.admin" }))
            {
                return await registNewsletter(model.Email, model.UserId);
            }
            return Forbid();
        }

        [HttpPost]
        [Route("public/register")]
        public async Task<IActionResult> PublicRegister([FromBody] NewsletterSubscriptionModel model)
        {
            return await registNewsletter(model.Email, null);
        }

        [Authorize]
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Put([FromBody] NewsletterSubscriptionModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "newsletter.admin" }))
            {
                Expression<Func<NewsletterSubscription, bool>> filter = (x => x.Email == model.Email);

                var _subscription = this._uow.NewsletterSubscriptionRepository.Get(null, null, filter, string.Empty, SortDirection.Ascending).FirstOrDefault();
                if (_subscription != null)
                {
                    if (!string.IsNullOrEmpty(model.UserId))
                    {
                        var user = await _userManager.SearchUserById(model.UserId);
                        if (user != null)
                        {
                            _subscription.UserId = user.Id;
                        }
                    }

                    ObjectResult _validate = this.ValidateNewsletter(_subscription);
                    if (_validate.StatusCode != StatusCodes.Status200OK)
                    {
                        return _validate;
                    }

                    try
                    {
                        _uow.NewsletterSubscriptionRepository.Update(_subscription);
                        _uow.Save();

                        return Ok(new
                        {
                            subscriptionId = _subscription.Id
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

        private async Task<IActionResult> removeSubscription(string email, bool isAdmin, string? validationToken)
        {
            Expression<Func<NewsletterSubscription, bool>> filter = (x => (isAdmin && x.Email == email) || (!string.IsNullOrEmpty(validationToken) && x.SubscriptionToken == validationToken && x.Email == email));

            var _subscription = this._uow.NewsletterSubscriptionRepository.Get(null, null, filter, string.Empty, SortDirection.Ascending).FirstOrDefault();
            if (_subscription != null)
            {
                this._uow.NewsletterSubscriptionRepository.Delete(_subscription);

                return Ok(new
                {
                    subscriptionEmailRemoved = email
                });
            }
            else
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> Delete([FromBody] NewsletterSubscriptionModel model)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "newsletter.admin" }))
            {
                return await removeSubscription(model.Email, true, model.ValidationToken);
            }
            return Forbid();
        }

        [HttpDelete]
        [Route("public/delete")]
        public async Task<IActionResult> PublicDelete([FromBody] NewsletterSubscriptionModel model)
        {
            if (!string.IsNullOrEmpty(model.ValidationToken))
            {
                List<JwtClaim> validateClaims = JwtHelper.ValidateToken(model.ValidationToken, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], new string[1] { JwtRegisteredClaimNames.Jti });
                if (validateClaims != null && validateClaims.Count > 0)
                {
                    return await removeSubscription(model.Email, false, model.ValidationToken);
                }
                else
                {
                    return Unauthorized();
                }
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpGet]
        [Route("export/csv")]
        public async Task<IActionResult> Csv()
        {
            string header = HttpContext.Request.Headers["Authorization"];
            string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope" };
            List<JwtClaim> userClaims = JwtHelper.ValidateToken(header, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
            string userScopesString = userClaims.Where(x => x.Claim == "scope").Single().Value;
            List<string>? scopes = !string.IsNullOrEmpty(userScopesString) ? JsonSerializer.Deserialize<List<string>>(userScopesString) : null;

            if (PermissionsHelper.ValidateRoleClaimPermission(userClaims, new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "newsletter.admin" }))
            {
                var builder = new StringBuilder();
                builder.AppendLine("Id,Username");
                //foreach (var user in users)
                //{
                //    builder.AppendLine($"{user.Id},{user.Username}");
                //}

                return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "users.csv");
            }
            return Forbid();
        }
    }
}

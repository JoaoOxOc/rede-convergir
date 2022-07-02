using CommonLibrary.Entities.ViewModel;
using CommonLibrary.Extensions;
using EmailService.Model;
using EmailService.Repositories;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using System.Linq.Expressions;
using System.Net;
using System.Text.Json;

namespace EmailService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailTemplatesController : ControllerBase
    {
        private readonly IEmailTemplatesRepository _emailTemplatesRepository;
        private readonly IConfiguration _configuration;

        public EmailTemplatesController(IEmailTemplatesRepository emailTemplatesRepository, IConfiguration configuration)
        {
            _emailTemplatesRepository = emailTemplatesRepository;
            _configuration = configuration;
        }

        private ObjectResult ValidateEmailTemplate(EmailTemplate template)
        {
            if (template.Key == null || string.IsNullOrWhiteSpace(template.Key.Trim())
                || string.IsNullOrEmpty(template.Subject) || string.IsNullOrEmpty(template.Language))
            {
                return UnprocessableEntity("MandatoryFields");
            }

            var existsEmailTemplate = this._emailTemplatesRepository.GetFiltered(template.Key, template.Language, null, null, string.Empty, string.Empty, template.Id).FirstOrDefault();
            if (existsEmailTemplate != null)
            {
                ConflictModel conflict = new ConflictModel();
                conflict.ConflictType = ConflictType.DuplicateValue;
                conflict.Entity = typeof(Setting).Name;

                if (existsEmailTemplate.Key == template.Key)
                {
                    conflict.Field = typeof(EmailTemplate).GetProperty("Key").Name;
                }

                return Conflict(conflict);
            }

            return Ok(null);
        }

        private EmailTemplateVM ParseEntityToModel(EmailTemplate templateData, bool isList)
        {
            EmailTemplateVM templateVM = new EmailTemplateVM();
            if (!isList)
            {
                templateVM.TemplateDataJson = System.Text.Json.JsonDocument.Parse(templateData.BodyJson.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson }));
                templateVM.BodyHtml = templateData.BodyHtml;
            }

            templateVM.Id = templateData.Id;
            templateVM.Description = templateData.Description;
            templateVM.Key = templateData.Key;
            templateVM.Language = templateData.Language;
            templateVM.Subject = templateData.Subject;
            templateVM.CreatedAt = templateData.CreatedAt;
            templateVM.UpdatedAt = templateData.UpdatedAt;

            return templateVM;
        }

        private List<EmailTemplateVM> ParseEntitiesToModel(List<EmailTemplate> templates)
        {
            List<EmailTemplateVM> models = new List<EmailTemplateVM>();
            foreach (var template in templates)
            {
                var model = ParseEntityToModel(template, true);
                if (model != null)
                {
                    models.Add(model);
                }
            }
            return models;
        }

        private EmailTemplate ProcessModelToEntity(EmailTemplateVM templateVM, EmailTemplate template)
        {
            if (templateVM != null && template != null)
            {
                if (!string.IsNullOrEmpty(templateVM.Description))
                    template.Description = templateVM.Description;

                if (!string.IsNullOrEmpty(templateVM.Subject))
                    template.Subject = templateVM.Subject;

                if (templateVM.TemplateDataJson != null)
                    template.BodyJson = BsonDocument.Parse(JsonSerializer.Serialize(templateVM.TemplateDataJson));

                if (!string.IsNullOrEmpty(templateVM.BodyHtml))
                    template.BodyHtml = templateVM.BodyHtml;
            }
            return template;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string? searchText, string? language, int? offset, int? limit, string sort, string sortDirection)
        {
            try
            {
                List<string> scopes = !string.IsNullOrEmpty(HttpContext.Request.Headers["UserClaims"]) ? JsonSerializer.Deserialize<List<string>>(HttpContext.Request.Headers["UserClaims"]) : null;
                if (PermissionsHelper.ValidateRoleClaimPermission(HttpContext.Request.Headers["UserRole"], new List<string> { "Admin" })
                    && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "email.admin" }))
                {
                    searchText = string.IsNullOrEmpty(searchText) ? string.Empty : searchText;

                    List<EmailTemplate> result = this._emailTemplatesRepository.GetFiltered(searchText, language, offset, limit, sort, sortDirection).ToList();

                    var totalElements = this._emailTemplatesRepository.Count(searchText, language, 1, null, sort);

                    Request.HttpContext.Response.Headers.Add("X-Total-Count", totalElements.ToString());

                    if (result.Count() > 0)
                    {
                        return Ok(new
                        {
                            templates = ParseEntitiesToModel(result)
                        });
                    }
                    else
                    {
                        return NotFound(new List<EmailTemplate>());
                    }
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, null);
                }

            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, null);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                List<string> scopes = !string.IsNullOrEmpty(HttpContext.Request.Headers["UserClaims"]) ? JsonSerializer.Deserialize<List<string>>(HttpContext.Request.Headers["UserClaims"]) : null;
                if (PermissionsHelper.ValidateRoleClaimPermission(HttpContext.Request.Headers["UserRole"], new List<string> { "Admin" })
                    && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "email.admin" }))
                {
                    var _template = await this._emailTemplatesRepository.GetById(id);

                    return _template != null ? Ok(new
                    {
                        template = ParseEntityToModel(_template, false)
                    })
                    : NotFound(null);
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, null);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message + "| " + ex.StackTrace);
            }
        }

        [HttpPut]
        [Route("edit")]
        public async Task<IActionResult> Edit([FromBody] EmailTemplateVM model)
        {
            List<string> scopes = !string.IsNullOrEmpty(HttpContext.Request.Headers["UserClaims"]) ? JsonSerializer.Deserialize<List<string>>(HttpContext.Request.Headers["UserClaims"]) : null;
            if (PermissionsHelper.ValidateRoleClaimPermission(HttpContext.Request.Headers["UserRole"], new List<string> { "Admin" })
                && PermissionsHelper.ValidateUserScopesPermissionAll(scopes, new List<string> { "email.admin" }))
            {
                var _template = this._emailTemplatesRepository.GetFiltered(model.Key, model.Language, null, null, string.Empty, string.Empty).FirstOrDefault();
                if (_template != null)
                {
                    _template = ProcessModelToEntity(model, _template);

                    _template.UpdatedAt = DateTime.UtcNow;
                    _template.UpdatedBy = HttpContext.Request.Headers["UserId"];

                    ObjectResult _validate = this.ValidateEmailTemplate(_template);
                    if (_validate.StatusCode != (int)HttpStatusCode.OK)
                    {
                        return _validate;
                    }

                    var result = this._emailTemplatesRepository.Update(_template);
                    if (result.Result != null)
                    {
                        return Ok(new
                        {
                            templateId = result.Result.Id
                    });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, result.Exception);
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            return StatusCode((int)HttpStatusCode.Forbidden, null);
        }
    }
}

using CommonLibrary.Entities;
using CommonLibrary.Extensions;
using System.Text.Json;
using CommonLibrary.Enums;

namespace EmailService.Code
{
    public static class ErrorLog
    {
        public static string ErrorLogGenerator(Exception ex, int? entityId, string entityName, int userId, string userName, string companyId, string extraInfo = null)
        {
            Tuple<string, string> errorData = ExceptionHelper.GetLogMessageFromException(ex);

            LogEvent log = new LogEvent();
            log.CompanyId = companyId;
            log.Date = DateTime.Now;
            log.EntityId = entityId;
            log.EntityName = entityName;
            if (!string.IsNullOrEmpty(extraInfo))
            {
                log.Message = extraInfo + "\n";
            }

            log.Message += errorData.Item2 + "\n" + errorData.Item1;
            log.Timezone = "UTC";
            log.UserId = userId;
            log.Username = userName;
            log.Type = (int)LogType.Error;
            log.ActivityType = (int)LogActivityType.ErrorAction;

            string serializedLog = JsonSerializer.Serialize(log);

            return serializedLog;// Encoding.UTF8.GetBytes(serializedLog);
        }
    }
}

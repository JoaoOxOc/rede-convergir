using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Code
{
    public class SMTPConfigurations
    {
        public static string Server;
        public static int Port;
        public static string Username;
        public static string Password;
        public static string From;
        public static string FromName;
        public static bool EnableSSL;
    }
}

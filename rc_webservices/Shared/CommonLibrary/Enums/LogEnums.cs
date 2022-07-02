using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLibrary.Enums
{
    public enum LogType
    {
        Error = 1,
        Debug = 2,
        Information = 3,
        Warning = 4
    }

    public enum LogActivityType
    {
        Login = 1,
        CreateAction = 2,
        DeleteAction = 3,
        UpdateAction = 4,
        ErrorAction = 5,
        UserActivation = 6,
        PasswordRecovery = 7,
        UserReactivation = 8,
        PublishAction = 9,
        FinishAction = 10,
        ReassignAction = 11,

        SystemActiviy = 99
    }
}

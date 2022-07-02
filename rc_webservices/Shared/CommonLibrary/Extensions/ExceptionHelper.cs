using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CommonLibrary.Extensions
{
    public static class ExceptionHelper
    {
        /// <summary>
        /// Static method for building exception details into a tuple of exception message and exception location
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static Tuple<string, string> GetLogMessageFromException(Exception exception)
        {
            // Get stack trace for the exception with source file information
            var st = new StackTrace(exception, true);
            int frameQuant = st.FrameCount;
            int lineNumber = 0;
            string objectName = string.Empty;
            string methodName = string.Empty;
            if (frameQuant > 0)
            {
                //get the top stack frame
                var frame = st.GetFrame(frameQuant - 1);
                // Get the line number from the stack frame
                lineNumber = frame.GetFileLineNumber();
                objectName = frame.GetFileName();
                methodName = frame.GetMethod().Name;
            }
            else
            {
                objectName = exception.TargetSite != null ? exception.TargetSite.DeclaringType.Name : string.Empty;
                methodName = exception.TargetSite != null ? exception.TargetSite.Name : string.Empty;
            }

            Exception innerException = exception.InnerException != null ? (exception.InnerException.InnerException != null ? exception.InnerException.InnerException : exception.InnerException) : null;

            string errorUrl = string.Empty;

            if (innerException != null)
            {
                var innerSt = new StackTrace(innerException, true);
                frameQuant = innerSt.FrameCount;
                int innerLineNumber = 0;
                string innerObjectName = string.Empty;
                string innerMethodName = string.Empty;
                if (frameQuant > 0)
                {
                    //get the top stack frame
                    var frame = innerSt.GetFrame(frameQuant - 1);
                    // Get the line number from the stack frame
                    innerLineNumber = frame.GetFileLineNumber();
                    innerObjectName = frame.GetFileName();
                    innerMethodName = frame.GetMethod().Name;
                }
                else
                {
                    innerObjectName = innerException.TargetSite != null ? innerException.TargetSite.DeclaringType.Name : string.Empty;
                    innerMethodName = innerException.TargetSite != null ? innerException.TargetSite.Name : string.Empty;
                }

                errorUrl = string.Format("{0} at method {1} - line {2}. InnerException: {3} at method {4} - line {5}.", objectName, methodName, lineNumber, innerObjectName, innerMethodName, innerLineNumber);

                return Tuple.Create<string, string>(innerException.Message, errorUrl);
            }
            else
            {
                errorUrl = string.Format("{0} at method {1} - line {2}.", objectName, methodName, lineNumber);

                return Tuple.Create<string, string>(exception.Message, errorUrl);
            }
        }

        public static string Substring(string text, int charNumber)
        {
            return text.Substring(0, charNumber) + "...";
        }
    }
}

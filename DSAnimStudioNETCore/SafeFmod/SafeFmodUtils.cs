using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.SafeFmod
{
    public class SafeFmodUtils
    {
        public enum SafeFmodErrorReportTypes
        {
            ThrowException = 0,
            PushNotification = 1,
            CompletelyIgnore = 2,
        }

        public static SafeFmodErrorReportTypes ErrorReportType = SafeFmodErrorReportTypes.CompletelyIgnore;

        public static void ShowErrorForResult(FMOD.RESULT result, string errorStringIfApplicable)
        {
            switch (ErrorReportType)
            {
                case SafeFmodErrorReportTypes.PushNotification:
                    string e = SafeFmodResultException.GetMessageString(errorStringIfApplicable, result);
                    zzz_NotificationManagerIns.PushNotificationError(
                    $"{e}\n\n" +
                    $"STACK TRACE:\n{System.Environment.StackTrace}");
                    break;
                case SafeFmodErrorReportTypes.ThrowException:
                    throw new SafeFmodResultException(errorStringIfApplicable, result);
                    break;
                case SafeFmodErrorReportTypes.CompletelyIgnore:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public static bool AssertResultOK(FMOD.RESULT result, string errorStringIfApplicable)
        {
            if (result == FMOD.RESULT.OK)
            {
                return true;
            }
            else
            {
                ShowErrorForResult(result, errorStringIfApplicable);
                return false;
            }
        }

    }
}

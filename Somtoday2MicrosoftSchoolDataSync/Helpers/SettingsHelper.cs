using Somtoday2MicrosoftSchoolDataSync.UmService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Somtoday2MicrosoftSchoolDataSync.Helpers
{

    class SettingsHelper
    {
        public static readonly string OutputFormatUsernameTeacher = (ConfigurationManager.AppSettings["OutputFormatUsernameTeacher"]).StartsWith("{user.") && (ConfigurationManager.AppSettings["OutputFormatUsernameTeacher"]).EndsWith("}") ? ConfigurationManager.AppSettings["OutputFormatUsernameTeacher"] : "{user." + ConfigurationManager.AppSettings["OutputFormatUsernameTeacher"] + "}";
        public static readonly string OutputFormatUsernameStudent = (ConfigurationManager.AppSettings["OutputFormatUsernameStudent"]).StartsWith("{user.") && (ConfigurationManager.AppSettings["OutputFormatUsernameStudent"]).EndsWith("}") ? ConfigurationManager.AppSettings["OutputFormatUsernameStudent"] : "{user." + ConfigurationManager.AppSettings["OutputFormatUsernameStudent"] + "}";


        EventLogHelper eh = Program.eh;

        internal bool ValidateUsernameFormat()
        {
            bool success = true;
            webserviceUmObject dummyUser = new webserviceUmObject() { medewerkerUsername = "testnaam" };
            try
            {
                ReplaceUserProperty(OutputFormatUsernameTeacher, dummyUser);
            }
            catch (Exception ex)
            {
                success = false;
                eh.WriteLog(string.Format("OutputFormatUsernameTeacher onjuist: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error, 500);
            }

            try
            {
                ReplaceUserProperty(OutputFormatUsernameStudent, dummyUser);
            }
            catch (Exception ex)
            {
                success = false;
                eh.WriteLog(string.Format("OutputFormatUsernameStudent onjuist: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error, 500);
            }
            return success;
        }

        internal string ReplaceTeacherProperty(string format, webserviceUmObject userobj)
        {
            return ReplaceUserProperty(format, userobj);
        }

        internal string ReplaceStudentProperty(string format, webserviceUmObject userobj)
        {
            return ReplaceUserProperty(format, userobj);
        }


        private static string ReplaceUserProperty(string value, webserviceUmObject userobj)
        {
            return Regex.Replace(value, @"{(?<exp>[^}]+)}", match =>
            {
                var p = Expression.Parameter(typeof(webserviceUmObject), "user");
                var e = System.Linq.Dynamic.DynamicExpression.ParseLambda(new[] { p }, null, match.Groups["exp"].Value);
                return (e.Compile().DynamicInvoke(userobj) ?? "").ToString();
            });
        }
    }

}

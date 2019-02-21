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
        public static readonly string OutputFormatUsernameTeacher = ConfigurationManager.AppSettings["OutputFormatUsernameTeacher"];
        //public static readonly string OutputFormatFirstnameTeacher = ConfigurationManager.AppSettings["OutputFormatFirstnameTeacher"];
        //public static readonly string OutputFormatLastnameTeacher = ConfigurationManager.AppSettings["OutputFormatLastnameTeacher"];
        public static readonly string OutputFormatUsernameStudent = ConfigurationManager.AppSettings["OutputFormatUsernameStudent"];
        //public static readonly string OutputFormatFirstnameStudent = ConfigurationManager.AppSettings["OutputFormatFirstnameStudent"];
        //public static readonly string OutputFormatLastnameStudent = ConfigurationManager.AppSettings["OutputFormatLastnameStudent"];

        EventLogHelper eh = new EventLogHelper();

        internal bool ValidateUsernameFormat()
        {
            bool success = true;
            webserviceUmObject dummyUser = new webserviceUmObject() { medewerkerUsername = "testnaam" };
            try
            {
                ReplaceUserProperty(OutputFormatUsernameTeacher, dummyUser);
                //ReplaceUserProperty(OutputFormatFirstnameTeacher, dummyUser);
                //ReplaceUserProperty(OutputFormatLastnameTeacher, dummyUser);

            }
            catch (Exception ex)
            {
                success = false;
                eh.WriteLog(string.Format("OutputFormatUsernameTeacher onjuist: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error, 500);
            }

            try
            {
                ReplaceUserProperty(OutputFormatUsernameStudent, dummyUser);
                //ReplaceUserProperty(OutputFormatFirstnameStudent, dummyUser);
                //ReplaceUserProperty(OutputFormatLastnameStudent, dummyUser);
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

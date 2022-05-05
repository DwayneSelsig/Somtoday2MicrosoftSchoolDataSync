using CsvHelper;
using Somtoday2MicrosoftSchoolDataSync.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Somtoday2MicrosoftSchoolDataSync.Helpers
{
    class FileHelper
    {

        internal bool WriteSDStoFiles(string path, SDScsv sdscsv)
        {
            EventLogHelper eh = Program.eh;
            if (sdscsv.Schools.Count() > 0 &&
                                   sdscsv.Sections.Count() > 0 &&
                                   sdscsv.Teachers.Count() > 0 &&
                                   sdscsv.Students.Count() > 0 &&
                                   sdscsv.TeacherRosters.Count() > 0 &&
                                   sdscsv.StudentEnrollments.Count > 0)
            {
                if (!Directory.Exists(path))
                {
                    eh.WriteLog(String.Format("Output directory bestaat niet, maar wordt nu aangemaakt: {0} ", path), EventLogEntryType.Information, 100);
                    Directory.CreateDirectory(path);
                }
                Console.Write(string.Format("CSV-bestanden schrijven... "));

                using (TextWriter writer = new StreamWriter(path + @"School.csv"))
                {
                    var csv = new CsvWriter(writer);
                    csv.Configuration.RegisterClassMap<SchoolCSVMap>();
                    csv.WriteRecords(sdscsv.Schools); // where values implements IEnumerable
                }

                using (TextWriter writer = new StreamWriter(path + @"Section.csv"))
                {
                    var csv = new CsvWriter(writer);
                    csv.Configuration.RegisterClassMap<SectionCSVMap>();
                    csv.WriteRecords(sdscsv.Sections); // where values implements IEnumerable
                }

                using (TextWriter writer = new StreamWriter(path + @"Student.csv"))
                {
                    var csv = new CsvWriter(writer);
                    csv.Configuration.RegisterClassMap<StudentCSVMap>();
                    csv.WriteRecords(sdscsv.Students); // where values implements IEnumerable
                }

                using (TextWriter writer = new StreamWriter(path + @"Teacher.csv"))
                {
                    var csv = new CsvWriter(writer);
                    csv.Configuration.RegisterClassMap<TeacherCSVMap>();
                    csv.WriteRecords(sdscsv.Teachers); // where values implements IEnumerable
                }

                using (TextWriter writer = new StreamWriter(path + @"TeacherRoster.csv"))
                {
                    var csv = new CsvWriter(writer);
                    csv.Configuration.RegisterClassMap<TeacherRosterCSVMap>();
                    csv.WriteRecords(sdscsv.TeacherRosters); // where values implements IEnumerable
                }

                using (TextWriter writer = new StreamWriter(path + @"StudentEnrollment.csv"))
                {
                    var csv = new CsvWriter(writer);
                    csv.Configuration.RegisterClassMap<StudentEnrollmentCSVMap>();
                    csv.WriteRecords(sdscsv.StudentEnrollments); // where values implements IEnumerable
                }
                if (sdscsv.User.Count() > 0 && sdscsv.Guardianrelationship.Count() > 0)
                {
                    using (TextWriter writer = new StreamWriter(path + @"user.csv"))
                    {
                        var csv = new CsvWriter(writer);
                        csv.Configuration.RegisterClassMap<GuardianCSVMap>();
                        csv.WriteRecords(sdscsv.User); // where values implements IEnumerable
                    }
                    using (TextWriter writer = new StreamWriter(path + @"guardianrelationship.csv"))
                    {
                        var csv = new CsvWriter(writer);
                        csv.Configuration.RegisterClassMap<GuardianRelationshipCSVMap>();
                        csv.WriteRecords(sdscsv.Guardianrelationship); // where values implements IEnumerable
                    }
                }

                if (sdscsv.Schools.Count == 1)
                {
                    eh.WriteLog(String.Format("CSV-bestanden geschreven: {0}, {1} Lesgroepen, {2} Docenten, {3} Leerlingen, {4} Docentlesgroepen, {5} Leerlinglesgroepen, naar {6}",
                    sdscsv.Schools[0].Name, sdscsv.Sections.Count, sdscsv.Teachers.Count, sdscsv.Students.Count, sdscsv.TeacherRosters.Count, sdscsv.StudentEnrollments.Count, path), EventLogEntryType.Information, 200);
                }
                else
                {
                    eh.WriteLog(String.Format("CSV-bestanden geschreven: {0} Scholen, {1} Lesgroepen, {2} Docenten, {3} Leerlingen, {4} Docentlesgroepen, {5} Leerlinglesgroepen, naar {6}",
                       sdscsv.Schools.Count, sdscsv.Sections.Count, sdscsv.Teachers.Count, sdscsv.Students.Count, sdscsv.TeacherRosters.Count, sdscsv.StudentEnrollments.Count, path), EventLogEntryType.Information, 200);
                }
            }
            else
            {
                if (sdscsv.Schools.Count == 1)
                {
                    eh.WriteLog(String.Format("Te weinig informatie om naar CSV te schrijven: {0}, {1} Lesgroepen, {2} Docenten, {3} Leerlingen, {4} Docentlesgroepen, {5} Leerlinglesgroepen",
                    sdscsv.Schools[0].Name, sdscsv.Sections.Count, sdscsv.Teachers.Count, sdscsv.Students.Count, sdscsv.TeacherRosters.Count, sdscsv.StudentEnrollments.Count), EventLogEntryType.Warning, 100);
                }
                else
                {
                    eh.WriteLog(String.Format("Te weinig informatie om naar CSV te schrijven: {0} Scholen, {1} Lesgroepen, {2} Docenten, {3} Leerlingen, {4} Docentlesgroepen, {5} Leerlinglesgroepen",
                        sdscsv.Schools.Count, sdscsv.Sections.Count, sdscsv.Teachers.Count, sdscsv.Students.Count, sdscsv.TeacherRosters.Count, sdscsv.StudentEnrollments.Count), EventLogEntryType.Warning, 100);
                }

            }
            return true;
        }
    }
}

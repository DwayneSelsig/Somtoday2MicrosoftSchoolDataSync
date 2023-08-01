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

        internal List<VestigingSDSModel> GetVestigingSDSModels(string path)
        {
            List<VestigingSDSModel> vestigingSDSList = new List<VestigingSDSModel>();
            string[] dirs;
            try
            {
                dirs = Directory.GetDirectories(path);
            }
            catch
            {
                return null;
            }

            if (dirs.Length == 0)
            {
                SDScsv sds = ReadCSVsFromDisk(path);
                if (sds != null)
                {
                    vestigingSDSList.Add(new VestigingSDSModel
                    {
                        Vestigingsafkorting = null,
                        SDS = sds
                    });
                }
            }
            if (dirs.Length >= 1)
            {
                foreach (string folder in dirs)
                {
                    SDScsv sds = ReadCSVsFromDisk(folder);
                    if (sds != null)
                    {
                        vestigingSDSList.Add(new VestigingSDSModel
                        {
                            Vestigingsafkorting = (folder.Remove(0, path.Length)).Trim(new Char[] { '\\' }),
                            SDS = sds
                        });
                    }
                }
            }
            return vestigingSDSList;
        }

        private static SDScsv ReadCSVsFromDisk(string path)
        {
            SDScsv sds = new SDScsv();
            try
            {
                using (TextReader reader = new StreamReader(path + @"School.csv"))
                {
                    var csv = new CsvReader(reader);
                    csv.Configuration.RegisterClassMap<SchoolCSVMap>();
                    var schools = csv.GetRecords<School>(); // where values implements IEnumerable
                    sds.Schools = schools.ToList();
                }
            }
            catch
            {
                return null;
            }
            try
            {

                using (TextReader reader = new StreamReader(path + @"Section.csv"))
                {
                    var csv = new CsvReader(reader);
                    csv.Configuration.RegisterClassMap<SectionCSVMap>();
                    var sections = csv.GetRecords<Section>(); // where values implements IEnumerable
                    sds.Sections = sections.ToList();
                }
            }
            catch
            {
                return null;
            }

            using (TextReader reader = new StreamReader(path + @"Student.csv"))
            {
                var csv = new CsvReader(reader);
                csv.Configuration.RegisterClassMap<StudentCSVMap>();
                var students = csv.GetRecords<Student>(); // where values implements IEnumerable
                sds.Students = students.ToList();
            }
            try
            {

                using (TextReader reader = new StreamReader(path + @"Teacher.csv"))
                {
                    var csv = new CsvReader(reader);
                    csv.Configuration.RegisterClassMap<TeacherCSVMap>();
                    var teachers = csv.GetRecords<Teacher>(); // where values implements IEnumerable
                    sds.Teachers = teachers.ToList();
                }
            }
            catch
            {
                return null;
            }
            try
            {

                using (TextReader reader = new StreamReader(path + @"TeacherRoster.csv"))
                {
                    var csv = new CsvReader(reader);
                    csv.Configuration.RegisterClassMap<TeacherRosterCSVMap>();
                    var teacherRosters = csv.GetRecords<TeacherRoster>(); // where values implements IEnumerable
                    sds.TeacherRosters = teacherRosters.ToList();
                }
            }
            catch
            {
                return null;
            }
            try
            {

                using (TextReader reader = new StreamReader(path + @"StudentEnrollment.csv"))
                {
                    var csv = new CsvReader(reader);
                    csv.Configuration.RegisterClassMap<StudentEnrollmentCSVMap>();
                    var studentEnrollments = csv.GetRecords<StudentEnrollment>(); // where values implements IEnumerable
                    sds.StudentEnrollments = studentEnrollments.ToList();
                }
            }
            catch
            {
                return null;
            }
            try
            {

                using (TextReader reader = new StreamReader(path + @"user.csv"))
                {
                    var csv = new CsvReader(reader);
                    csv.Configuration.RegisterClassMap<GuardianCSVMap>();
                    var guardians = csv.GetRecords<Guardian>(); // where values implements IEnumerable
                    sds.User = guardians.ToList();
                }
            }
            catch
            {
            }
            try
            {
                using (TextReader reader = new StreamReader(path + @"guardianrelationship.csv"))
                {
                    var csv = new CsvReader(reader);
                    csv.Configuration.RegisterClassMap<GuardianRelationshipCSVMap>();
                    var guardianRelationships = csv.GetRecords<GuardianRelationship>(); // where values implements IEnumerable
                    sds.Guardianrelationship = guardianRelationships.ToList();
                }
            }
            catch
            {
            }
            return sds;
        }
    }
}

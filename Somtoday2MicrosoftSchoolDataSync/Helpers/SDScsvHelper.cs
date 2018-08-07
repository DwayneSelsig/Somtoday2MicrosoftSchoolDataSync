using Somtoday2MicrosoftSchoolDataSync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Somtoday2MicrosoftSchoolDataSync.Helpers
{
    class SDScsvHelper
    {
        List<VestigingLesgroepModel> vestigingLesgroepen;
        List<UserLesgroepModel> docentLesgroepen;
        List<UserLesgroepModel> leerlingLesgroepen;

        public SDScsvHelper(List<VestigingLesgroepModel> vestigingLesgroepenModel, List<UserLesgroepModel> docentLesgroepenModel, List<UserLesgroepModel> leerlingLesgroepenModel)
        {
            vestigingLesgroepen = vestigingLesgroepenModel;
            docentLesgroepen = docentLesgroepenModel;
            leerlingLesgroepen = leerlingLesgroepenModel;
        }

        internal SDScsv GetSDScsv()
        {
            Console.WriteLine(string.Format("School Data Sync CSV samenstellen..."));
            // https://support.office.com/nl-nl/article/CSV-bestanden-voor-synchronisatie-van-schoolgegevens-9f3c3c2b-7364-4f6e-a959-e8538feead70

            SDScsv _sdscsv = new SDScsv();

            List<School> Schools = GetSchools().ToList();
            List<Section> Sections = GetSections().ToList();
            List<Teacher> Teachers = GetTeachers().ToList();
            List<Student> Students = GetStudents().ToList();
            List<TeacherRoster> TeacherRosters = GetTeacherRosters();
            List<StudentEnrollment> StudentEnrollments = GetStudentEnrollments(Sections);


            _sdscsv.Schools = Schools;
            _sdscsv.Sections = Sections;
            _sdscsv.Teachers = Teachers;
            _sdscsv.Students = Students;
            _sdscsv.TeacherRosters = TeacherRosters;
            _sdscsv.StudentEnrollments = StudentEnrollments;
            Console.WriteLine();

            return _sdscsv;
        }

        private List<School> GetSchools()
        {
            Console.Write("Scholen samenstellen");
            List<School> schools = new List<School>();
            foreach (var school in vestigingLesgroepen)
            {
                schools.Add(new School { SISid = school.Vestiging.id.ToString(), Name = school.Vestiging.naam });
            }
            Console.Write(". ");
            Console.WriteLine(schools.Count);
            return schools;
        }

        private List<Section> GetSections()
        {
            Console.Write("Lesgroepen samenstellen");
            List<Section> _sections = new List<Section>();

            foreach (var vestigingLesgroep in vestigingLesgroepen)
            {
                string vestigingsAfkorting = vestigingLesgroep.Vestiging.afkorting;
                foreach (var lesgroep in vestigingLesgroep.Lesgroepen)
                {
                    if (lesgroep.docenten?.Count() > 0)
                    {
                        if (lesgroep.naam != null)
                        {
                            string sectieNaam = GetFilteredName(lesgroep.naam);

                            var leerlingen = leerlingLesgroepen.Where(vl => vl.VestigingLesgroep == vestigingLesgroep)
                                .Select(l => l.Users.Where(o => o.leerlingLesgroepen.Split(',').Any(lg => lg == lesgroep.naam)))
                                .SelectMany(l => l.OrderByDescending(o => o.leerlingLeerjaar)).ToList();

                            if (leerlingen.Count > 0)
                            {
                                string leerjaar = leerlingen.GroupBy(x => x.leerlingLeerjaar).OrderByDescending(g => g.Count()).FirstOrDefault().Key;

                                _sections.Add(new Section
                                {
                                    SISSchoolid = vestigingLesgroep.Vestiging.id.ToString(),
                                    SISid = lesgroep.naam.ToLower().StartsWith(vestigingsAfkorting.ToLower()) ? sectieNaam : vestigingsAfkorting.ToLower() + sectieNaam,
                                    Name = sectieNaam,
                                    Number = lesgroep.id.ToString(),
                                    CourseSISID = lesgroep.vak.id.ToString(),
                                    CourseName = lesgroep.vak.naam,
                                    CourseSubject = lesgroep.vak.naam,
                                    Periods = leerjaar,
                                    TermSISID = DateTime.Now.Month < 8 ? (DateTime.Now.Year - 1).ToString() + DateTime.Now.Year.ToString() : DateTime.Now.Year.ToString() + (DateTime.Now.Year + 1).ToString(),
                                    TermName = DateTime.Now.Month < 8 ? (DateTime.Now.Year - 1) + "/" + DateTime.Now.Year : DateTime.Now.Year + "/" + (DateTime.Now.Year + 1),
                                    TermStartDate = DateTime.Now.Month < 8 ? "8/1/" + (DateTime.Now.Year - 1) : "8/1/" + DateTime.Now.Year,
                                    TermEndDate = DateTime.Now.Month < 8 ? "7/31/" + DateTime.Now.Year : "7/31/" + (DateTime.Now.Year + 1)
                                });
                            }
                        }
                    }
                }
            }
            Console.Write(". ");
            Console.WriteLine(_sections.Count);
            return _sections;
        }

        private List<Teacher> GetTeachers()
        {
            Console.Write("Docenten samenstellen");
            List<Teacher> teachers = new List<Teacher>();

            List<UmService.webserviceUmObject> docenten = docentLesgroepen.SelectMany(d => d.Users).Distinct().ToList();
            List<UmService.webserviceUmObject> docentenActief = docenten.Where(d => d.medewerkerActiefOmschrijving.ToLower() == "in dienst").ToList();

            foreach (var employee in docentenActief)
            {
                var lesgevendDocent = vestigingLesgroepen.SelectMany(d => d.Lesgroepen).ToList();
                var docentenLijst = lesgevendDocent.Where(d => d.docenten != null).ToList();
                var lesgroepje = docentenLijst.Where(d => d.docenten.Any(o => o.docentUUID == employee.medewerkerUUID)).ToList();

                if (lesgroepje != null && lesgroepje.Count > 0)
                {
                    string sISSchoolid = vestigingLesgroepen.Where(v => v.Vestiging.afkorting == employee.medewerkerDefaultVestiging).FirstOrDefault()?.Vestiging.id.ToString();

                    //Als de hoofdvestiging van deze docent is gefilterd, maar hij/zij geeft wel les op een gesynchroniseerde vestiging
                    //Zoek dan het ID van een gesynchroniseerde vestiging waar hij/zij les geeft.

                    if (string.IsNullOrEmpty(sISSchoolid))
                    {
                        List<VestigingLesgroepModel> alleVestigingenVanDocent = vestigingLesgroepen.Where(v => employee.medewerkerVestigingen.Split(':').Any(ev => ev == v.Vestiging.afkorting)).ToList();
                        if (alleVestigingenVanDocent.Count > 0)
                        {
                            sISSchoolid = alleVestigingenVanDocent.FirstOrDefault().Vestiging.id.ToString();
                        }
                    }
                    //Ga nu verder met samenstellen.

                    if (!string.IsNullOrEmpty(sISSchoolid))
                    {
                        teachers.Add(new Teacher
                        {
                            //Firstname = employeecsv.Firstname,
                            //Lastname = employeecsv.Lastname,
                            SISid = employee.medewerkerUsername,
                            SISSchoolid = sISSchoolid,
                            Username = employee.medewerkerUsername,
                            //Password = "Welkom" + DateTime.Now.Year
                        });
                    }
                }
            }
            Console.Write(". ");
            Console.WriteLine(teachers.Count);
            return teachers;
        }

        private List<Student> GetStudents()
        {
            Console.Write("Leerlingen samenstellen");
            List<Student> students = new List<Student>();

            foreach (UserLesgroepModel userLesgroep in leerlingLesgroepen)
            {
                List<UmService.webserviceUmObject> leerlingenActief = userLesgroep.Users.Where(d => d.leerlingActief.ToLower() == "actief").Distinct().ToList();

                foreach (var leerling in leerlingenActief)
                {
                    students.Add(new Student
                    {
                        //Firstname = studentCsv.Firstname,
                        //Lastname = studentCsv.Lastname,
                        SISid = leerling.leerlingNummer.ToString(),
                        SISSchoolid = userLesgroep.VestigingLesgroep.Vestiging.id.ToString(),
                        Username = leerling.leerlingNummer.ToString(),
                        //Password = "Welkom" + DateTime.Now.Year
                    });
                }
            }
            students = students.GroupBy(o => new { o.Firstname, o.Lastname, o.Password, o.SISid, o.SISSchoolid, o.Username }).Select(o => o.FirstOrDefault()).ToList();
            Console.Write(". ");
            Console.WriteLine(students.Count);
            return students;
        }

        private List<TeacherRoster> GetTeacherRosters()
        {
            Console.Write("Lesgroepen voor docenten samenstellen");
            int i = 0;

            List<TeacherRoster> _teacherRosters = new List<TeacherRoster>();

            foreach (var docentLesgroep in docentLesgroepen)
            {
                foreach (var lesgroep in docentLesgroep.VestigingLesgroep.Lesgroepen)

                {
                    i++;
                    if (i > 100)
                    {
                        Console.Write(".");
                        i = 0;
                    }
                    if (lesgroep.docenten != null && lesgroep.docenten.Count() > 0)
                    {
                        foreach (var lesgroepdocent in lesgroep.docenten)
                        {
                            var docent = docentLesgroepen.SelectMany(u => u.Users.Where(m => m.medewerkerUUID == lesgroepdocent.docentUUID && m.medewerkerActiefOmschrijving.ToLower() == "in dienst")).FirstOrDefault();

                            if (docent != null)
                            {
                                var leerlingen = leerlingLesgroepen.Where(vl => vl.VestigingLesgroep.Lesgroepen.Any(l => l == lesgroep))
                                    .Select(l => l.Users.Where(o => o.leerlingLesgroepen.Split(',').Any(lg => lg == lesgroep.naam)))
                                    .SelectMany(l => l.OrderByDescending(o => o.leerlingLeerjaar)).ToList();
                                if (leerlingen.Count > 0)
                                {
                                    string vestigingsAfkorting = docentLesgroep.VestigingLesgroep.Vestiging.afkorting;
                                    string sectieNaam = GetFilteredName(lesgroep.naam);

                                    _teacherRosters.Add(new TeacherRoster
                                    {
                                        SISSectionid = lesgroep.naam.ToLower().StartsWith(vestigingsAfkorting.ToLower()) ? sectieNaam : vestigingsAfkorting.ToLower() + sectieNaam,
                                        SISTeacherid = docent.medewerkerUsername
                                    });
                                }
                            }

                        }
                    }
                }
            }
            Console.Write(". ");
            Console.WriteLine(_teacherRosters.Count);
            return _teacherRosters;
        }


        private List<StudentEnrollment> GetStudentEnrollments(List<Section> sections)
        {
            Console.Write("Lesgroepen voor leerlingen samenstellen");
            List<StudentEnrollment> _studentEnrollments = new List<StudentEnrollment>();
            int i = 0;
            foreach (Section sec in sections)
            {
                i++;
                if (i > 100)
                {
                    Console.Write(".");
                    i = 0;
                }

                var huidigeLeerlingen = leerlingLesgroepen.SelectMany(v => v.Users.Where(l => l.leerlingLesgroepen.Split(',').Any(lg => lg == sec.Name) && v.VestigingLesgroep.Vestiging.id.ToString() == sec.SISSchoolid)).ToList();
                foreach (var student in huidigeLeerlingen)
                {
                    _studentEnrollments.Add(new StudentEnrollment
                    {
                        SISSectionid = sec.SISid,
                        SISStudentid = student.leerlingUsername
                    });
                }
            }
            Console.Write(". ");
            Console.WriteLine(_studentEnrollments.Count);
            return _studentEnrollments;
        }

        private string GetFilteredName(string input)
        {
            //Alles met een spatie of verboden teken voor OneDrive wordt omgezet naar _
            return Regex.Replace(input, @"[^\S]|[\~\""\#\%\&\*\:\<\>\?\/\\{\|}\.]", "_");
        }
    }
}

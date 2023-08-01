using Somtoday2MicrosoftSchoolDataSync.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        List<UserLesgroepModel> ouderLesgroepen;
        SettingsHelper sh = new SettingsHelper();

        public SDScsvHelper(List<VestigingLesgroepModel> vestigingLesgroepenModel, List<UserLesgroepModel> docentLesgroepenModel, List<UserLesgroepModel> leerlingLesgroepenModel, List<UserLesgroepModel> ouderLesgroepenModel)
        {
            vestigingLesgroepen = vestigingLesgroepenModel;
            docentLesgroepen = docentLesgroepenModel;
            leerlingLesgroepen = leerlingLesgroepenModel;
            ouderLesgroepen = ouderLesgroepenModel;
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

            Tuple<List<Guardian>, List<GuardianRelationship>> ouderData = GetGuardianData();
            List<Guardian> Guardians = ouderData.Item1;
            List<GuardianRelationship> GuardianRelationships = ouderData.Item2;

            _sdscsv.Schools = Schools;
            _sdscsv.Sections = Sections;
            _sdscsv.Teachers = Teachers;
            _sdscsv.Students = Students;
            _sdscsv.TeacherRosters = TeacherRosters;
            _sdscsv.StudentEnrollments = StudentEnrollments;
            _sdscsv.User = Guardians;
            _sdscsv.Guardianrelationship = GuardianRelationships;
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
            string currentSchoolyear = DateTime.Now.Month < 8 ? (DateTime.Now.Year - 1) + "-" + DateTime.Now.Year : DateTime.Now.Year + "-" + (DateTime.Now.Year + 1);

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
                                .Select(l => l.Users.Where(o => !string.IsNullOrEmpty(o.leerlingUsername) && o.leerlingActief.ToLower() == "actief" && o.leerlingLesgroepen.Split(',').Any(lg => lg == lesgroep.naam)))
                                .SelectMany(l => l.OrderByDescending(o => o.leerlingLeerjaar)).ToList();

                            if (leerlingen.Count > 0)
                            {
                                string leerjaar = leerlingen.GroupBy(x => x.leerlingLeerjaar).OrderByDescending(g => g.Count()).FirstOrDefault().Key;

                                _sections.Add(new Section
                                {
                                    SISSchoolid = vestigingLesgroep.Vestiging.id.ToString(),
                                    SISid = (lesgroep.naam.ToLower().StartsWith(vestigingsAfkorting.ToLower()) ? sectieNaam : vestigingsAfkorting.ToLower() + sectieNaam) + currentSchoolyear,
                                    Name = sectieNaam,
                                    Number = lesgroep.id.ToString(),
                                    CourseSISID = lesgroep.vak.id.ToString(),
                                    CourseName = lesgroep.vak.naam,
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
                var lesgroep = docentenLijst.Where(d => d.docenten.Any(o => o.docentUUID == employee.medewerkerUUID)).ToList();

                if (lesgroep != null && lesgroep.Count > 0)
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

                        if (!string.IsNullOrEmpty(sh.ReplaceTeacherProperty(SettingsHelper.OutputFormatUsernameTeacher, employee)))
                        {
                            teachers.Add(new Teacher
                            {
                                SISid = employee.medewerkerNummer,
                                SISSchoolid = sISSchoolid,
                                Username = sh.ReplaceTeacherProperty(SettingsHelper.OutputFormatUsernameTeacher, employee),
                            });
                        }
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
                List<UmService.webserviceUmObject> leerlingenActief = userLesgroep.Users.Where(d => d.leerlingActief.ToLower() == "actief" && !string.IsNullOrEmpty(d.leerlingUsername)).Distinct().ToList();

                foreach (var student in leerlingenActief)
                {
                    if (!string.IsNullOrEmpty(sh.ReplaceStudentProperty(SettingsHelper.OutputFormatUsernameStudent, student)))
                    {
                        students.Add(new Student
                        {
                            SISid = student.leerlingNummer.ToString(),
                            SISSchoolid = userLesgroep.VestigingLesgroep.Vestiging.id.ToString(),
                            Username = sh.ReplaceStudentProperty(SettingsHelper.OutputFormatUsernameStudent, student),
                        });
                    }
                }
            }
            students = students.GroupBy(o => new { o.Firstname, o.Lastname, o.SISid, o.SISSchoolid, o.Username }).Select(o => o.FirstOrDefault()).ToList();
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
                        string currentSchoolyear = DateTime.Now.Month < 8 ? (DateTime.Now.Year - 1) + "-" + DateTime.Now.Year : DateTime.Now.Year + "-" + (DateTime.Now.Year + 1);

                        foreach (var lesgroepdocent in lesgroep.docenten)
                        {
                            var docent = docentLesgroepen.SelectMany(u => u.Users.Where(m => m.medewerkerUUID == lesgroepdocent.docentUUID && m.medewerkerActiefOmschrijving.ToLower() == "in dienst")).FirstOrDefault();

                            if (docent != null)
                            {
                                var leerlingen = leerlingLesgroepen.Where(vl => vl.VestigingLesgroep.Lesgroepen.Any(l => l == lesgroep))
                                    .Select(l => l.Users.Where(o => !string.IsNullOrEmpty(o.leerlingUsername) && o.leerlingActief.ToLower() == "actief" && o.leerlingLesgroepen.Split(',').Any(lg => lg == lesgroep.naam)))
                                    .SelectMany(l => l.OrderByDescending(o => o.leerlingLeerjaar)).ToList();
                                if (leerlingen.Count > 0)
                                {
                                    string vestigingsAfkorting = docentLesgroep.VestigingLesgroep.Vestiging.afkorting;
                                    string sectieNaam = GetFilteredName(lesgroep.naam);

                                    if (!string.IsNullOrEmpty(docent.medewerkerNummer))
                                    {
                                        _teacherRosters.Add(new TeacherRoster
                                        {
                                            SISSectionid = (lesgroep.naam.ToLower().StartsWith(vestigingsAfkorting.ToLower()) ? sectieNaam : vestigingsAfkorting.ToLower() + sectieNaam) + currentSchoolyear,
                                            SISTeacherid = docent.medewerkerNummer,
                                        });
                                    }
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

                var huidigeLeerlingen = leerlingLesgroepen.SelectMany(v => v.Users.Where(l => !string.IsNullOrEmpty(l.leerlingUsername) && l.leerlingActief.ToLower() == "actief" && l.leerlingLesgroepen.Split(',').Any(lg => GetFilteredName(lg) == sec.Name) && v.VestigingLesgroep.Vestiging.id.ToString() == sec.SISSchoolid)).ToList();
                foreach (var student in huidigeLeerlingen)
                {
                    if (student.leerlingNummer != null)
                    {
                        _studentEnrollments.Add(new StudentEnrollment
                        {
                            SISSectionid = sec.SISid,
                            SISStudentid = student.leerlingNummer.ToString()
                        });
                    }
                }
            }
            Console.Write(". ");
            Console.WriteLine(_studentEnrollments.Count);
            return _studentEnrollments;
        }

        private Tuple<List<Guardian>, List<GuardianRelationship>> GetGuardianData()
        {
            // https://docs.microsoft.com/en-us/schooldatasync/parent-contact-sync-file-format
            Console.Write("Oudergegevens samenstellen");
            int i = 0;

            List<Guardian> ouders = new List<Guardian>();
            List<GuardianRelationship> ouderRelaties = new List<GuardianRelationship>();
            foreach (UserLesgroepModel ouderLesgroep in ouderLesgroepen)
            {
                List<UmService.webserviceUmObject> leerlingenVanDeVestiging = leerlingLesgroepen.Where(vl => vl.VestigingLesgroep == ouderLesgroep.VestigingLesgroep).Select(l => l.Users.Where(o => !string.IsNullOrEmpty(o.leerlingUsername) && o.leerlingActief.ToLower() == "actief" && !string.IsNullOrEmpty(o.leerlingUsername)).Distinct().ToList()).FirstOrDefault();
                foreach (var ouderObject in ouderLesgroep.Users)
                {
                    foreach (var leerlingData in ouderObject.verzorgerLeerlingRelaties.Split(':'))
                    {
                        i++;
                        if (i > 100)
                        {
                            Console.Write(".");
                            i = 0;
                        }
                        string[] ouderGegevens = leerlingData.Split(',');
                        /* De opbouw van de verzorgerLeerlingRelaties is verder als volgt:
                           [0]BurgerServiceNummer leerling, [1]Onderwijsnummmer leerling, [2]leerlingnummer, [3]relatie type, [4]Wettelijk, [5]Factuur, [6]Post, [7]extern debiteurnummer.

                           Als er geen BSN bekend is, gebruik je het onderwijsnummer (deze wordt dan door DUO verstrekt). Wettelijk, factuur en post zijn kenmerken in Somtoday. Een school kan deze vrij gebruiken. Indien aangevinkt geeft dit de waarde ja, anders een nee. 
                           Indien factuur ja is, wordt er een extern debiteurnummer meegegeven. 
                           Indien de verzorger meerdere kinderen als leerlingen heeft, worden deze achter elkaar getoond met een dubbele punt (:) als scheidingsteken. 
                         */
                        if (ouderGegevens[4].ToLower() == "ja") //Indien dit de wettelijke ouder/voogd is.
                        {
                            if (!string.IsNullOrEmpty(ouderObject.verzorgerEmail)) //Als de ouder een e-mailadres heeft
                            {
                                UmService.webserviceUmObject gevondenLeerling = leerlingenVanDeVestiging.Where(l => l.leerlingNummer.ToString() == ouderGegevens[2]).FirstOrDefault();
                                if (gevondenLeerling != null)
                                {
                                    ouders.Add(new Guardian
                                    {
                                        Email = ouderObject.verzorgerEmail,
                                        FirstName = ouderObject.verzorgerVoorletters,
                                        LastName = ouderObject.verzorgerAchternaam,
                                        Phone = ouderObject.verzorgerMobielNummerGeheim || String.IsNullOrEmpty(ouderObject.verzorgerMobielNummer) ? (ouderObject.verzorgerAdresTelefoonNummerGeheim || String.IsNullOrEmpty(ouderObject.verzorgerAdresTelefoonNummer) ? (ouderObject.verzorgerMobielWerkNummerGeheim || String.IsNullOrEmpty(ouderObject.verzorgerMobielWerkNummer) ? "" : ouderObject.verzorgerMobielWerkNummer) : ouderObject.verzorgerAdresTelefoonNummer) : ouderObject.verzorgerMobielNummer,
                                        //Mobielnummer. Als dit geheim is of niet ingevuld, dan thuisnummer gebruiken.
                                        //Thuisnummer.  Als dit geheim is of niet ingevuld, dan werknummer gebruiken.
                                        //Werknummer.   Als dit geheim is, geen nummer invullen.
                                        SISid = ouderObject.verzorgerUUID
                                    });
                                    ouderRelaties.Add(new GuardianRelationship
                                    {
                                        Email = ouderObject.verzorgerEmail,
                                        SISid = gevondenLeerling.leerlingNummer.ToString()
                                    });
                                }
                            }
                        }
                    }
                }
            }
            Console.Write(". ");
            Console.WriteLine(ouders.Count);
            return Tuple.Create<List<Guardian>, List<GuardianRelationship>>(ouders.GroupBy(o => o.SISid).Select(o => o.FirstOrDefault()).ToList(), ouderRelaties);
        }


        private string GetFilteredName(string input)
        {
            //Alles met een spatie of verboden teken voor OneDrive wordt omgezet naar _
            string _temp = Regex.Replace(input, @"[^\S]|[\~\""\#\%\&\*\:\<\>\?\/\\{\|}\.\[\]]", "_");
            return RemoveDiacritics(_temp);
        }

        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}

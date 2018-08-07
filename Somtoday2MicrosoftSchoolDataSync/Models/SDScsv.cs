using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Somtoday2MicrosoftSchoolDataSync.Models
{
    class SDScsv
    {
        public List<School> Schools { get; set; }
        public List<Section> Sections { get; set; }
        public List<Student> Students { get; set; }
        public List<Teacher> Teachers { get; set; }
        public List<StudentEnrollment> StudentEnrollments { get; set; }
        public List<TeacherRoster> TeacherRosters { get; set; }
    }


    public class TeacherRoster
    {
        [DisplayName("Section SIS ID")]
        public string SISSectionid { get; set; }
        [DisplayName("SIS ID")]
        public string SISTeacherid { get; set; }
    }
    public sealed class TeacherRosterCSVMap : ClassMap<TeacherRoster>
    {
        public TeacherRosterCSVMap()
        {
            AutoMap();
            Map(m => m.SISSectionid).Name("Section SIS ID");
            Map(m => m.SISTeacherid).Name("SIS ID");
        }
    }


    public class StudentEnrollment
    {
        public string SISSectionid { get; set; }
        public string SISStudentid { get; set; }
    }
    public sealed class StudentEnrollmentCSVMap : ClassMap<StudentEnrollment>
    {
        public StudentEnrollmentCSVMap()
        {
            AutoMap();
            Map(m => m.SISSectionid).Name("Section SIS ID");
            Map(m => m.SISStudentid).Name("SIS ID");
        }
    }


    public class Teacher
    {
        public string SISid { get; set; }
        public string SISSchoolid { get; set; }
        public string Username { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Password { get; set; }
    }
    public sealed class TeacherCSVMap : ClassMap<Teacher>
    {
        public TeacherCSVMap()
        {
            //AutoMap();
            Map(m => m.SISid).Name("SIS ID");
            Map(m => m.SISSchoolid).Name("School SIS ID");
            Map(m => m.Username).Name("Username");
            //Map(m => m.Firstname).Name("First Name");
            //Map(m => m.Lastname).Name("Last Name");
            //Map(m => m.Password).Name("Password");
        }
    }


    public class Student
    {
        public string SISid { get; set; }
        public string SISSchoolid { get; set; }
        public string Username { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Password { get; set; }
    }
    public sealed class StudentCSVMap : ClassMap<Student>
    {
        public StudentCSVMap()
        {
            //AutoMap();
            Map(m => m.SISid).Name("SIS ID");
            Map(m => m.SISSchoolid).Name("School SIS ID");
            Map(m => m.Username).Name("Username");
            //Map(m => m.Firstname).Name("First Name");
            //Map(m => m.Lastname).Name("Last Name");
            //Map(m => m.Password).Name("Password");
        }
    }


    public class Section
    {
        public string SISid { get; set; }
        public string SISSchoolid { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string CourseSISID { get; set; }
        public string CourseName { get; set; }
        public string CourseSubject { get; set; }
        public string Periods { get; set; }
        public string TermSISID { get; set; }
        public string TermName { get; set; }
        public string TermStartDate { get; set; }
        public string TermEndDate { get; set; }
    }
    public sealed class SectionCSVMap : ClassMap<Section>
    {
        public SectionCSVMap()
        {
            AutoMap();
            Map(m => m.SISid).Name("SIS ID");
            Map(m => m.SISSchoolid).Name("School SIS ID");
            Map(m => m.Name).Name("Section Name");
            Map(m => m.Number).Name("Section Number");
            Map(m => m.CourseSISID).Name("Course SIS ID");
            Map(m => m.CourseName).Name("Course Name");
            Map(m => m.CourseSubject).Name("Course Subject");
            Map(m => m.Periods).Name("Periods");
            Map(m => m.TermSISID).Name("Term SIS ID");
            Map(m => m.TermName).Name("Term Name");
            Map(m => m.TermStartDate).Name("Term StartDate");
            Map(m => m.TermEndDate).Name("Term EndDate");
        }
    }


    public class School
    {
        public string SISid { get; set; }
        public string Name { get; set; }
    }
    public sealed class SchoolCSVMap : ClassMap<School>
    {
        public SchoolCSVMap()
        {
            AutoMap();
            Map(m => m.SISid).Name("SIS ID");
            Map(m => m.Name).Name("Name");
        }
    }
}

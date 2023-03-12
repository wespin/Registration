namespace Registration.Migrations
{
    using Registration.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Collections.Generic;
    using System.Linq;
    using Registration.Data;

    internal sealed class Configuration : DbMigrationsConfiguration<Registration.Data.RegistrationContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Registration.Data.RegistrationContext context)
        {
            var students = new List<Student>
            {
                new Student { FirstMidName = "Andrea",   LastName = "Valenzuela",
                    EnrollmentDate = DateTime.Parse("2023-03-11") },
                new Student { FirstMidName = "Moises", LastName = "Perez",
                    EnrollmentDate = DateTime.Parse("2023-03-11") },
                new Student { FirstMidName = "Yasmin",   LastName = "Torres",
                    EnrollmentDate = DateTime.Parse("2023-03-11") },
            };
            students.ForEach(s => context.Students.AddOrUpdate(p => p.LastName, s));
            context.SaveChanges();


            var courses = new List<Course>
            {
                new Course{CourseID=1000,Title="Algebra",Credits=3,},
                new Course{CourseID=1050,Title="Matematica",Credits=3,},
                new Course{CourseID=1045,Title="Educacion Fisica",Credits=3,},
                new Course{CourseID=3141,Title="Religion",Credits=3,},
                new Course{CourseID=4022,Title="Castellano",Credits=3,},
                new Course{CourseID=4041,Title="Sociales",Credits=3,},                
                new Course{CourseID=5022,Title="Geografia",Credits=3,},
                new Course{CourseID=6012,Title="Tecnologia",Credits=3,},
                new Course{CourseID=7084,Title="Ingles",Credits=3,},
                new Course{CourseID=8956,Title="Musica",Credits=3,},
                new Course{CourseID=9060,Title="Dibujo",Credits=3,},
                
            };
            courses.ForEach(s => context.Courses.AddOrUpdate(p => p.Title, s));
            context.SaveChanges();


            var enrollments = new List<Enrollment>
            {
                new Enrollment {
                    StudentID = students.Single(s => s.LastName == "Valenzuela").ID,
                    CourseID = courses.Single(c => c.Title == "Algebra" ).CourseID,
                    Grade = Grade.A
                },
                 new Enrollment {
                    StudentID = students.Single(s => s.LastName == "Valenzuela").ID,
                    CourseID = courses.Single(c => c.Title == "Educacion Fisica" ).CourseID,
                    Grade = Grade.C
                 },
                 new Enrollment {
                     StudentID = students.Single(s => s.LastName == "Torres").ID,
                    CourseID = courses.Single(c => c.Title == "Algebra" ).CourseID,
                    Grade = Grade.B
                 }
            };

            foreach (Enrollment e in enrollments)
            {
                var enrollmentInDataBase = context.Enrollments.Where(
                    s =>
                         s.Student.ID == e.StudentID &&
                         s.Course.CourseID == e.CourseID).SingleOrDefault();
                if (enrollmentInDataBase == null)
                {
                    context.Enrollments.Add(e);
                }
            }
            context.SaveChanges();


            var teachers = new List<Teacher>
            {
                new Teacher { FirstMidName = "Alan",     LastName = "Cadena",
                    HireDate = DateTime.Parse("1995-03-11") },
                new Teacher { FirstMidName = "Alejandro",    LastName = "Garnica",
                    HireDate = DateTime.Parse("2002-07-06") },
                new Teacher { FirstMidName = "Minerva",   LastName = "Gonzalez",
                    HireDate = DateTime.Parse("2004-02-12") },
                new Teacher { FirstMidName = "Silvia",   LastName = "Prado",
                    HireDate = DateTime.Parse("1998-07-01") },
                new Teacher { FirstMidName = "Hernan", LastName = "Valero",
                    HireDate = DateTime.Parse("2001-01-15") },

            };
            teachers.ForEach(s => context.Teachers.AddOrUpdate(p => p.LastName, s));
            context.SaveChanges();

            AddOrUpdateTeacher(context, "Algebra", "Alan");
            AddOrUpdateTeacher(context, "Religion", "Alan");

            AddOrUpdateTeacher(context, "Geografia", "Alejandro");
            AddOrUpdateTeacher(context, "Musica", "Alejandro");

            AddOrUpdateTeacher(context, "Educacion Fisica", "Minerva");
            AddOrUpdateTeacher(context, "Castellano", "Minerva");

            AddOrUpdateTeacher(context, "Tecnologia", "Hernan");
            AddOrUpdateTeacher(context, "Dibujo", "Hernan");

            AddOrUpdateTeacher(context, "Matematica", "Silvia");
            AddOrUpdateTeacher(context, "Sociales", "Silvia");

            context.SaveChanges();
        }

        void AddOrUpdateTeacher(RegistrationContext context, string courseTitle, string teacherName)
        {
            var crs = context.Courses.SingleOrDefault(c => c.Title == courseTitle);
            var inst = crs.Teachers.SingleOrDefault(i => i.FirstMidName == teacherName);
            if (inst == null)
                crs.Teachers.Add(context.Teachers.Single(i => i.FirstMidName == teacherName));
        }
    }
}

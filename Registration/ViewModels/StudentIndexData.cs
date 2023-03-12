
using Registration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Registration.ViewModels
{
    public class StudentIndexData
    {
        public IEnumerable<Student> Classmates { get; set; }
        public IEnumerable<Course> Courses { get; set; }
        public IEnumerable<Enrollment> Enrollments { get; set; }
        public IEnumerable<Student> Students { get; set; }
    }
}
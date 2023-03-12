using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Registration.ViewModels
{
    public class TeacherAssignedCourseData
    {
        public int CourseID { get; set; }
        public string Title { get; set; }
        public bool Assigned { get; set; }
    }
}
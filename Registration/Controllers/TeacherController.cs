using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Registration.Data;
using Registration.Models;
using Registration.ViewModels;

namespace Registration.Controllers
{
    public class TeacherController : Controller
    {
        private RegistrationContext db = new RegistrationContext();

        //// GET: Teacher
        //public ActionResult Index()
        //{
        //    return View(db.Teachers.ToList());
        //}

        public ActionResult Index(int? id, int? courseID)
        {
            var viewModel = new TeacherIndexData();
            viewModel.Teachers = db.Teachers
                .OrderBy(i => i.LastName);

            if (id != null)
            {
                ViewBag.TeacherID = id.Value;
                viewModel.Courses = viewModel.Teachers.Where(
                    i => i.ID == id.Value).Single().Courses;
            }

            if (courseID != null)
            {
                ViewBag.CourseID = courseID.Value;
                // Lazy loading
                //viewModel.Enrollments = viewModel.Courses.Where(
                //    x => x.CourseID == courseID).Single().Enrollments;
                // Explicit loading
                var selectedCourse = viewModel.Courses.Where(x => x.CourseID == courseID).Single();
                db.Entry(selectedCourse).Collection(x => x.Enrollments).Load();
                foreach (Enrollment enrollment in selectedCourse.Enrollments)
                {
                    db.Entry(enrollment).Reference(x => x.Student).Load();
                }

                viewModel.Enrollments = selectedCourse.Enrollments;
            }

            return View(viewModel);
        }

        // GET: Teacher/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teacher teacher = db.Teachers.Find(id);
            if (teacher == null)
            {
                return HttpNotFound();
            }
            return View(teacher);
        }

        public ActionResult Create()
        {
            var teacher = new Teacher();
            teacher.Courses = new List<Course>();
            PopulateAssignedCourseData(teacher);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,LastName,FirstMidName,HireDate")] Teacher teacher, string[] selectedCourses)
        {
      
            if (selectedCourses.Count() > 2)
            {
                ModelState.AddModelError("", "Superó el máximo de materias permitidas");
            }

            foreach (var courseid in selectedCourses)
            {
                var results = db.Database.SqlQuery<int>($"SELECT count(*) FROM CourseTeacher WHERE CourseID = @Param1", new SqlParameter("@Param1", int.Parse(courseid))).Single();

                if (results == 1)
                {
                    ModelState.AddModelError("", "Ya existe un profesor con esa materia");
                    break;
                }
            }


            if (selectedCourses != null)
            {
                teacher.Courses = new List<Course>();
                foreach (var course in selectedCourses)
                {
                    var courseToAdd = db.Courses.Find(int.Parse(course));
                    teacher.Courses.Add(courseToAdd);
                }
            }

            if (ModelState.IsValid)
            {
                db.Teachers.Add(teacher);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            PopulateAssignedCourseData(teacher);

            return View(teacher);
        }

        // GET: Teacher/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teacher teacher = db.Teachers
                  .Include(i => i.Courses)
                  .Where(i => i.ID == id)
                  .Single();


            if (teacher == null)
            {
                return HttpNotFound();
            }

            PopulateAssignedCourseData(teacher);

            return View(teacher);
        }

        private void PopulateAssignedCourseData(Teacher teacher)
        {
            var allCourses = db.Courses;
            var instructorCourses = new HashSet<int>(teacher.Courses.Select(c => c.CourseID));
            var viewModel = new List<TeacherAssignedCourseData>();

            foreach (var course in allCourses)
            {
                viewModel.Add(new TeacherAssignedCourseData
                {
                    CourseID = course.CourseID,
                    Title = course.Title,
                    Assigned = instructorCourses.Contains(course.CourseID)
                });
            }

            ViewBag.Courses = viewModel;
        }

        // POST: Teacher/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, string[] selectedCourses)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (selectedCourses.Count() > 2)
            {
                ModelState.AddModelError("", "Superó el máximo de materias permitidas");
            }

            foreach (var courseid in selectedCourses)
            {
                var results = db.Database.SqlQuery<int>($"SELECT count(*) FROM CourseTeacher WHERE CourseID = @Param1", new SqlParameter("@Param1", int.Parse(courseid))).Single();

                if (results == 1)
                {
                    ModelState.AddModelError("", "Ya existe un profesor con esa materia");
                    break;
                }
            }


            var teacherToUpdate = db.Teachers
               .Include(i => i.Courses)
               .Where(i => i.ID == id)
               .Single();

            if (TryUpdateModel(teacherToUpdate, "",
               new string[] { "LastName", "FirstMidName", "HireDate" }))
            {
                try
                {
                    UpdateInstructorCourses(selectedCourses, teacherToUpdate);

                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            PopulateAssignedCourseData(teacherToUpdate);
            return View(teacherToUpdate);
        }

        private void UpdateInstructorCourses(string[] selectedCourses, Teacher teacherToUpdate)
        {
            if (selectedCourses == null)
            {
                teacherToUpdate.Courses = new List<Course>();
                return;
            }

            var selectedCoursesHS = new HashSet<string>(selectedCourses);
            var instructorCourses = new HashSet<int>
                (teacherToUpdate.Courses.Select(c => c.CourseID));
            foreach (var course in db.Courses)
            {
                if (selectedCoursesHS.Contains(course.CourseID.ToString()))
                {
                    if (!instructorCourses.Contains(course.CourseID))
                    {
                        teacherToUpdate.Courses.Add(course);
                    }
                }
                else
                {
                    if (instructorCourses.Contains(course.CourseID))
                    {
                        teacherToUpdate.Courses.Remove(course);
                    }
                }
            }
        }

        // GET: Teacher/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teacher teacher = db.Teachers.Find(id);
            if (teacher == null)
            {
                return HttpNotFound();
            }
            return View(teacher);
        }

        // POST: Teacher/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Teacher teacher = db.Teachers
                   .Where(i => i.ID == id)
                   .Single();

            db.Teachers.Remove(teacher);

            db.SaveChanges();
            
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

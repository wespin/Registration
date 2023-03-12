using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Registration.Data;
using Registration.Models;

namespace Registration.Controllers
{
    public class EnrollmentController : Controller
    {
        private RegistrationContext db = new RegistrationContext();

        // GET: Enrollment
        public ActionResult Index(string sortOrder, string searchString)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            var enrollments = db.Enrollments.Include(e => e.Course).Include(e => e.Student);

            if (!String.IsNullOrEmpty(searchString))
            {
                enrollments = enrollments.Where(s => s.Student.LastName.Contains(searchString)
                                       || s.Student.FirstMidName.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    enrollments = enrollments.OrderByDescending(s => s.Student.LastName);
                    break;
                default:
                    enrollments = enrollments.OrderBy(s => s.Student.LastName);
                    break;
            }


            return View(enrollments.ToList());
        }

        // GET: Enrollment/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Enrollment enrollment = db.Enrollments.Find(id);
            if (enrollment == null)
            {
                return HttpNotFound();
            }
            return View(enrollment);
        }

        // GET: Enrollment/Create
        public ActionResult Create()
        {
            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title");
            ViewBag.StudentID = new SelectList(db.Students, "ID", "LastName");


            return View();
        }

        // POST: Enrollment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EnrollmentID,CourseID,StudentID,Grade")] Enrollment enrollment)
        {

            try
            {
                if (enrollment != null)
                {
                    var enrollmentsByStudent = db.Enrollments.Include(p => p.Course).Where(x => x.StudentID == enrollment.StudentID).ToList();

                    if (enrollmentsByStudent != null)
                    {
                        int creditCourse = enrollmentsByStudent.Sum(x => x.Course.Credits);
                        bool assignedCourse = enrollmentsByStudent.Any(x => x.CourseID == enrollment.CourseID);

                        if (creditCourse >= 9)
                        {
                            ModelState.AddModelError("", "Superó el máximo de creditos permitidos");
                        }
                        if (assignedCourse)
                        {
                            ModelState.AddModelError("", "Ya tiene inscrita esta materia");
                        }

                        var teacherIdEnrollmentNew = db.Database.SqlQuery<int>($"SELECT TeacherId FROM CourseTeacher WHERE CourseID = @Param1", new SqlParameter("@Param1", enrollment.CourseID)).Single();
                        var existTeacherCOurse = enrollmentsByStudent.Exists(x => x.Course.Teachers.Any(c => c.ID == teacherIdEnrollmentNew));
                        if (existTeacherCOurse)
                        {
                            ModelState.AddModelError("", "No puedes tener 2 materias con el mismo profesor");
                        }


                    }
                }

            }
            catch (Exception e)
            { 
                
            }

            if (ModelState.IsValid)
            {
                db.Enrollments.Add(enrollment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title", enrollment.CourseID);
            ViewBag.StudentID = new SelectList(db.Students, "ID", "LastName", enrollment.StudentID);
            return View(enrollment);
        }

        // GET: Enrollment/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Enrollment enrollment = db.Enrollments.Find(id);
            if (enrollment == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title", enrollment.CourseID);
            ViewBag.StudentID = new SelectList(db.Students, "ID", "LastName", enrollment.StudentID);
            return View(enrollment);
        }

        // POST: Enrollment/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EnrollmentID,CourseID,StudentID,Grade")] Enrollment enrollment)
        {
            if (enrollment != null)
            {
                var enrollmentsByStudent = db.Enrollments.Include(p => p.Course).Where(x => x.StudentID == enrollment.StudentID).ToList();

                if (enrollmentsByStudent != null)
                {
                    int creditCourse = enrollmentsByStudent.Sum(x => x.Course.Credits);
                    bool assignedCourse = enrollmentsByStudent.Any(x => x.CourseID == enrollment.CourseID);

                    if (creditCourse >= 9)
                    {
                        ModelState.AddModelError("", "Superó el máximo de creditos permitidos");
                    }
                    if (assignedCourse)
                    {
                        ModelState.AddModelError("", "Ya tiene inscrita esta materia");
                    }

                    var teacherIdEnrollmentNew = db.Database.SqlQuery<int>($"SELECT TeacherId FROM CourseTeacher WHERE CourseID = @Param1", new SqlParameter("@Param1", enrollment.CourseID)).Single();
                    var existTeacherCOurse = enrollmentsByStudent.Exists(x => x.Course.Teachers.Any(c => c.ID == teacherIdEnrollmentNew));
                    if (existTeacherCOurse)
                    {
                        ModelState.AddModelError("", "No puedes tener 2 materias con el mismo profesor");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                db.Entry(enrollment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title", enrollment.CourseID);
            ViewBag.StudentID = new SelectList(db.Students, "ID", "LastName", enrollment.StudentID);
            return View(enrollment);
        }

        // GET: Enrollment/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Enrollment enrollment = db.Enrollments.Find(id);
            if (enrollment == null)
            {
                return HttpNotFound();
            }
            return View(enrollment);
        }

        // POST: Enrollment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Enrollment enrollment = db.Enrollments.Find(id);
            db.Enrollments.Remove(enrollment);
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

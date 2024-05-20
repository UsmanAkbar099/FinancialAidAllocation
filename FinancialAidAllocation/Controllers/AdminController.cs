using FinancialAidAllocation.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Http;

namespace FinancialAidAllocation.Controllers
{
    public class AdminController : ApiController
    {
        FAAToolEntities db = new FAAToolEntities();


        [HttpGet]
        public HttpResponseMessage getAdminInfo(int id)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, db.Admins.Where(a => a.adminId == id).FirstOrDefault());
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage AddBudget(int amount)
        {
            try
            {
                if (amount > 0)
                {
                    var paisa = db.Budgets.OrderByDescending(bd => bd.budgetID).FirstOrDefault();
                    Budget b;
                    if (paisa != null)
                    {
                        b = new Budget();
                        b.budgetAmount = amount;
                        b.remainingAmount = paisa.remainingAmount + amount;
                    }
                    else
                    {
                        b = new Budget();
                        b.budgetAmount = amount;
                        b.remainingAmount = amount;
                    }
                    db.Budgets.Add(b);
                    db.SaveChanges();
                    var Remainbalance = db.Budgets.OrderByDescending(bd => bd.budgetID).FirstOrDefault();
                    return Request.CreateResponse(HttpStatusCode.OK, Remainbalance.remainingAmount);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, "Add Some Ammount");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }
        [HttpGet]
        public HttpResponseMessage FacultyMembers()
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, db.Faculties);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        [HttpPost]
        public HttpResponseMessage AcceptApplication(int amount, int applicationid)
        {
            try
            {
                var remainingamount = db.Budgets.OrderByDescending(bd => bd.budgetID).FirstOrDefault();

                if (amount > 0 && remainingamount.remainingAmount >= amount)
                {
                    var application = db.FinancialAids.Where(f => f.applicationId == applicationid).FirstOrDefault();
                    if (application.applicationStatus.ToLower() == "pending")
                    {
                        application.amount = amount.ToString();
                        application.applicationStatus = "Accepted";
                        var paisa = db.Budgets.OrderByDescending(bd => bd.budgetID).FirstOrDefault();
                        paisa.remainingAmount -= amount;
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, paisa.remainingAmount + "\n" + application.applicationStatus);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, application.applicationStatus);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, "InSufficient Funds");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }
        [HttpPost]
        public HttpResponseMessage RejectApplication(int applicationid)
        {
            try
            {
                var application = db.FinancialAids.Where(f => f.applicationId == applicationid).FirstOrDefault();
                if (application.applicationStatus.ToLower() == "pending")
                {
                    application.applicationStatus = "Rejected";
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, application.applicationStatus);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Already : " + application.applicationStatus);
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        /* [HttpPost]
         public HttpResponseMessage AddPolicies(String policyfor, String session, String Description, String deadline)
         {
             try
             {
                 Policy p = new Policy();
                 p.policyFor = policyfor;
                 p.session = session;
                 p.deadline = deadline;
                 p.description = Description;
                 db.Policies.Add(p);
                 db.SaveChanges();
                 return Request.CreateResponse(HttpStatusCode.OK);
             }
             catch (Exception ex)
             {
                 return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
             }
         }*/

        [HttpGet]
        public HttpResponseMessage MeritBaseShortListing()
        {
            try
            {
                var meritbaselist = db.MeritBases.Join
                    (
                    db.Students,
                    m => m.studentId,
                    s => s.student_id,
                    (m, s) => new
                    {
                        m,
                        s
                    }
                    );

                return Request.CreateResponse(HttpStatusCode.OK, meritbaselist);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        public HttpResponseMessage AcceptedApplication()
        {
            try
            {
                var application = db.Applications.Join
                    (
                    db.FinancialAids.Where(f => f.applicationStatus.ToLower().Equals("accepted")),
                    a => a.applicationID,
                    f => f.applicationId,
                    (a, f) => new
                    {
                        a,
                        f,
                    }
                    );
                var result = application.Join
                    (
                        db.Students,
                        ap => ap.a.studentId,
                        s => s.student_id
                        ,
                        (ap, s) => new
                        {
                            ap,
                            s
                        }

                    );
                return Request.CreateResponse(HttpStatusCode.OK, result);
                //                return Request.CreateResponse(HttpStatusCode.OK,db.FinancialAids.Where(f=>f.ApplicationStatus.ToLower()=="accepted"));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        public HttpResponseMessage RejectedApplication()
        {
            try
            {
                var application = db.Students.Join
                    (
                        db.Applications,
                        s => s.student_id,
                        a => a.studentId,
                        (s, a) => new
                        {
                            s,
                            a,
                        }
                    );
                var result = application.Join
                    (
                    db.FinancialAids.Where(f => f.applicationStatus.ToLower() == "rejected"),
                    ap => ap.a.applicationID,
                    f => f.id,
                    (ap, f) => new
                    {
                        ap,
                        f,
                    }
                    );
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }
        [HttpGet]
        public HttpResponseMessage CommitteeMembers()
        {
            try
            {
                var members = db.Committees.Join
                    (
                    db.Faculties,
                    c => c.facultyId,
                    f => f.facultyId,
                    (c, f) => new
                    {
                        c.committeeId,
                        f.name,
                        f.contactNo,
                        f.profilePic,
                    }
                    );
                return Request.CreateResponse(HttpStatusCode.OK, members);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage AddUser(String username, String password, int role, int? profileId)
        {
            /*
               1 student
               2 faculty
               3 committee
               4 Admin
             */
            try
            {
                var user = db.Users.Where(us => us.userName == username & us.password == password).FirstOrDefault();

                if (user == null)
                {
                    User u = new User();
                    u.userName = username;
                    u.password = password;
                    u.role = role;
                    u.profileId = profileId;
                    db.Users.Add(u);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Found, "Already Exist");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage AddFacultyMember()
        {
            try
            {
                var request = HttpContext.Current.Request;
                String name = request["name"];
                String contact = request["contact"];
                String password = request["password"];
                int Role = 3;
                var photo = request.Files["pic"];
                var provider = new MultipartMemoryStreamProvider();
                String picpath = name + "." + photo.FileName.Split('.')[1];
                photo.SaveAs(HttpContext.Current.Server.MapPath("~/Content/ProfileImages/" + picpath));
                Faculty f = new Faculty();
                f.name = name;
                f.contactNo = contact;
                f.profilePic = picpath;
                db.Faculties.Add(f);
                db.SaveChanges();
                var facultyid = db.Faculties.Where(fa => fa.name == name & fa.contactNo == contact).FirstOrDefault();
                AddUser(name, password, Role, facultyid.facultyId);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Added");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage AddCommitteeMember(int id)
        {
            try
            {
                if (db.Committees.Where(c => c.facultyId == id).FirstOrDefault() == null)
                {
                    Committee committee = new Committee();
                    committee.facultyId = id;
                    committee.status = "1";
                    db.Committees.Add(committee);
                    db.SaveChanges();
                    var user = db.Users.Where(u => u.profileId == id).FirstOrDefault();
                    var comm = db.Committees.Where(cm => cm.facultyId == id).FirstOrDefault();
                    user.role = 2;
                    user.profileId = comm.committeeId;
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Found, "Already Exist");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public HttpResponseMessage BudgetHistory()
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, db.Budgets);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public HttpResponseMessage getStudentApplicationStatus(int id)
        {
            try
            {
                var count = db.Applications.Where(c => c.studentId == 1d).FirstOrDefault();
                if (count != null)
                {
                    var result = db.Applications.Where(ap => ap.studentId == id).Join(
                    db.FinancialAids,
                    a => a.applicationID,
                    f => f.applicationId,
                    (a, f) => new
                    {
                        //                        a.applicationID,
                        f.applicationStatus,
                    }
                    );
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Submitted");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage AssignGrader(int facultyId, int StudentId, String session)
        {
            try
            {
                var student = db.Graders.Where(gr => gr.studentId == StudentId & gr.session == session).FirstOrDefault();
                if (student == null)
                {
                    Grader g = new Grader();
                    g.studentId = StudentId;
                    g.facultyId = facultyId;
                    g.session = session;
                    db.Graders.Add(g);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Found, "Alredy Assigned ");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public HttpResponseMessage gradersInformation()
        {
            try
            {
                var graders = db.Faculties.Join
                    (
                        db.Graders,
                        f => f.facultyId,
                        g => g.facultyId,
                        (f, g) => new
                        {
                            g,
                            f
                        }
                    );
                return Request.CreateResponse(HttpStatusCode.OK, graders);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public HttpResponseMessage ApplicationSuggestions()
        {
            try
            {
                var applications = db.Applications
                            .Join(db.Suggestions,
                                application => application.applicationID,
                                suggestion => suggestion.applicationId,
                                (application, suggestion) => new
                                {
                                    application,
                                    suggestion
                                });
                var result = applications.Join(db.Students,
                    ap => ap.application.studentId,
                    s => s.student_id,
                    (appplication, student) => new
                    {
                        student.arid_no,
                        student.name,
                        student.student_id,
                        student.father_name,
                        student.gender,
                        student.degree,
                        student.cgpa,
                        student.semester,
                        student.section,
                        student.profile_image,
                        appplication.application.applicationDate,
                        appplication.application.reason,
                        appplication.application.requiredAmount,
                        appplication.application.EvidenceDocuments,
                        appplication.application.applicationID,
                        appplication.application.session,
                        appplication.application.father_status,
                        appplication.application.jobtitle,
                        appplication.application.salary,
                        appplication.application.guardian_contact,
                        appplication.application.house,
                        appplication.application.guardian_name,
                        appplication.suggestion.comment,
                        appplication.suggestion.status
                    });

                var pendingapplication = result.Join(
                    db.FinancialAids,
                    re => re.applicationID,
                    f => f.applicationId,
                    (re, f) => new
                    {
                        re,
                        f.applicationStatus
                    }
                    );

                return Request.CreateResponse(HttpStatusCode.OK, pendingapplication.Where(p => p.applicationStatus.ToLower().Equals("pending")));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }




        [HttpPost]
        public HttpResponseMessage UpdatePassword(int id, String username, String password)
        {
            try
            {
                var userprofile = db.Users.Where(u => u.userName == username).FirstOrDefault();

                if (userprofile == null)
                {
                    User user = new User();
                    user.role = 4;
                    user.password = password;
                    user.userName = username;
                    user.profileId = id;
                    db.Users.Add(user);
                    db.SaveChanges();
                }
                else
                {
                    userprofile.password = password;
                    db.SaveChanges();
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }



        [HttpGet]
        public HttpResponseMessage getAllStudent()
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, db.Students);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public HttpResponseMessage getAllBudget()
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, db.Budgets);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        [HttpGet]
        public HttpResponseMessage ToperStudents(double cgpa)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, db.Students.Where(s => s.cgpa >= cgpa));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        [HttpPost]
        public HttpResponseMessage AddStudent()
        {
            try
            {
                var request = HttpContext.Current.Request;
                String name = request["name"];
                String cgpa = request["cgpa"];
                String semester = request["semester"];
                String aridno = request["aridno"];
                String gender = request["gender"];
                String fathername = request["fathername"];
                String degree = request["degree"];
                String section = request["section"];
                String password = request["password"];
                int Role = 1;
                var photo = request.Files["pic"];
                var provider = new MultipartMemoryStreamProvider();
                String picpath = name + "." + photo.FileName.Split('.')[1];
                photo.SaveAs(HttpContext.Current.Server.MapPath("~/Content/ProfileImages/" + picpath));
                Student s = new Student();
                s.section = section;
                s.name = name;
                s.gender = gender;
                s.arid_no = aridno;
                s.degree = degree;
                s.father_name = fathername;
                s.semester = int.Parse(semester);
                s.cgpa = double.Parse(cgpa);
                s.profile_image = picpath;
                db.Students.Add(s);
                db.SaveChanges();
                var studentId = db.Students.Where(sa => sa.arid_no == aridno).FirstOrDefault();
                AddUser(aridno, password, Role, studentId.student_id);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }
        [HttpGet]
        public HttpResponseMessage ApplicationWithSuggestion()
        {
            try
            {


                var applications = db.Applications
                            .Join(db.Suggestions,
                                application => application.applicationID,
                                suggestion => suggestion.applicationId,
                                (application, suggestion) => new
                                {
                                    application,
                                    suggestion
                                });
                var result = applications.Join(db.Students,
                    ap => ap.application.studentId,
                    s => s.student_id,
                    (appplication, student) => new
                    {
                        student.arid_no,
                        student.name,
                        student.student_id,
                        student.father_name,
                        student.gender,
                        student.degree,
                        student.cgpa,
                        student.semester,
                        student.section,
                        student.profile_image,
                        appplication.application.applicationDate,
                        appplication.application.reason,
                        appplication.application.requiredAmount,
                        appplication.application.EvidenceDocuments,
                        appplication.application.applicationID,
                        appplication.application.session,
                        appplication.application.father_status,
                        appplication.application.jobtitle,
                        appplication.application.salary,
                        appplication.application.guardian_contact,
                        appplication.application.house,
                        appplication.application.guardian_name,
                        appplication.suggestion.comment,
                        appplication.suggestion.status
                    });

                var pendingapplication = result.Join(
                    db.FinancialAids,
                    re => re.applicationID,
                    f => f.applicationId,
                    (re, f) => new
                    {
                        re,
                        f.applicationStatus
                    }
                    );

                return Request.CreateResponse(HttpStatusCode.OK, pendingapplication.Where(p => p.applicationStatus.ToLower().Equals("pending")));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}

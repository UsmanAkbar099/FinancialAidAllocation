﻿using FinancialAidAllocation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Web.Http;

namespace FinancialAidAllocation.Controllers
{
    public class FacultyController : ApiController
    {
        FAAToolEntities db = new FAAToolEntities();

        [HttpGet]

        public HttpResponseMessage FacultyInfo(int id)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, db.Faculties.Where(f => f.facultyId == id).FirstOrDefault());
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public HttpResponseMessage TeachersGraders(int id)
        {
            try
            {
                var result = db.Graders.Where(gr => gr.facultyId == id).Join
                    (
                    db.Students,
                    gr => gr.studentId,
                    s => s.student_id,
                    (gr, s) => new
                    {
                        s
                    }
                    );
                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        [HttpPost]
        public HttpResponseMessage RateGraderPerformance(int facultyId, int graderId, int rate, String session)
        {
            try
            {
                var result = db.Graders.Where(f => f.facultyId == facultyId && f.studentId == graderId && f.session == session && f.feedback == null).FirstOrDefault();

                if (result != null)
                {
                    Grader g = new Grader();
                    g.feedback = rate;
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, g);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Found, "Already Rated");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}

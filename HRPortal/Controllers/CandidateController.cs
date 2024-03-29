﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
using HRPortal.Helper;
using HRPortal.Models;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using System.Net.Mail;
using System.Net.Mime;
using HRPortal.Common;
using HRPortal.Common.Enums;
using PagedList;
using System.Web.Http.Controllers;
using System.Threading;

namespace HRPortal.Controllers
{
    [LogActionFilter]
    public class CandidateController : Controller
    {
        private HRPortalEntities db = new HRPortalEntities();
        private CandidateViewModels vmodel = new CandidateViewModels();
        private LoginViewModel logvmodel = new LoginViewModel();
        JobAndCandidateViewModels jobCanObj = new JobAndCandidateViewModels();
        private AppointmentViewModels appointmentVM = new AppointmentViewModels();
        const int pageSize = 10;

        // GET: Candidate
        public ActionResult Index(string sOdr, int? page)
        {
            try
            {
                jobCanObj = GetCandidateSearchResults();
                ViewBag.StatusList = vmodel.GetStatusList();
                ViewBag.VendorList = vmodel.GetVendorList();
                ViewBag.PositionList = vmodel.GetPositionForPartner();
                var jobCanObjl = jobCanObj.CandidateItems != null? GetPagination(jobCanObj.CandidateItems, sOdr, page): jobCanObj.CandidateItems;
                return View(jobCanObjl);
            }
            catch(Exception ex) { throw ex; }
        }

        // GET: Squads Candidates
        [Authorize(Roles = "SuperUser")]
        public async Task<ActionResult> SquadJobs(string sOdr, int? page)
        {
            try
            {
                var uid = HelperFuntions.HasValue(CookieStore.GetCookie(CacheKey.Uid.ToString()));
                var vendorId = Guid.Parse(HelperFuntions.HasValue(CookieStore.GetCookie(CacheKey.VendorId.ToString())));
                var squadsLst = await db.CANDIDATES.ToListAsync();
                var usrs = await db.AspNetUsers.Where(i => i.Vendor_Id == vendorId && i.Id != uid).Select(s => s.Id).ToListAsync();
                var canLst = squadsLst.Where(c => usrs.Contains(c.CREATED_BY) && c.ISACTIVE == true).Select(i => new CandidateViewModels
                {
                    CANDIDATE_ID = i.CANDIDATE_ID,
                    CANDIDATE_NAME = i.CANDIDATE_NAME,
                    MOBILE_NO = i.MOBILE_NO,
                    EMAIL = i.EMAIL,
                    CURRENT_COMPANY = i.CURRENT_COMPANY,
                    NOTICE_PERIOD = i.NOTICE_PERIOD,
                    YEARS_OF_EXP_TOTAL = i.YEARS_OF_EXP_TOTAL,
                    LAST_WORKING_DATE = i.LAST_WORKING_DATE,
                    STATUS_ID = i.STATUS,
                    STATUS = vmodel.GetStatusNameById(i.CANDIDATE_ID),
                    CREATED_ON = i.CREATED_ON,
                    CREATED_BY = logvmodel.GetUserNameById(i.CREATED_BY),
                }).ToList();
                ViewBag.TotalRecord = canLst.Count();
                canLst = squadsLst != null ? GetPagination(canLst, sOdr, page) : canLst;

                int pSize = ViewBag.PageSize == null ? 0 : ViewBag.PageSize;
                int pNo = ViewBag.PageNo == null ? 0 : ViewBag.PageNo;

                return View(canLst.ToPagedList(pNo, pSize));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        // GET: Candidate/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            try {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                CANDIDATE cANDIDATE = await db.CANDIDATES.FindAsync(id);
                var userLst = db.AspNetUsers.ToList();
                cANDIDATE.CREATED_BY = userLst.Where(u => u.Id == cANDIDATE.CREATED_BY).Select(um => um.FirstName + " " + um.LastName).FirstOrDefault();
                cANDIDATE.MODIFIED_BY = cANDIDATE.MODIFIED_BY == null ? string.Empty : userLst.Where(u => u.Id.ToUpper() == cANDIDATE.MODIFIED_BY.ToUpper()).Select(um => um.FirstName + " " + um.LastName).FirstOrDefault();
                var stsCmnts = db.STATUS_HISTORY.Where(i => i.CANDIDATE_ID == id).OrderByDescending(j => j.MODIFIED_ON).FirstOrDefault().COMMENTS;
                ViewBag.StatusComments = stsCmnts;
                if (cANDIDATE == null)
                {
                    return HttpNotFound();
                }
                return View(cANDIDATE);
            }
            catch (Exception ex) { throw ex; }
        }

        // GET: Candidate/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Candidate/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "CANDIDATE_NAME,YEARS_OF_EXP_TOTAL,YEARS_OF_EXP_RELEVANT,MOBILE_NO,ALTERNATE_MOBILE_NO,EMAIL,ALTERNATE_EMAIL_ID,DOB,CURRENT_COMPANY,CURRENT_LOCATION,NOTICE_PERIOD,COMMENTS,ISINNOTICEPERIOD,LAST_WORKING_DATE")] CANDIDATE cANDIDATE, HttpPostedFileBase file,FormCollection frm)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            //System.Web.Helpers.AntiForgery.Validate();

            string _qryString = Request.UrlReferrer.Query.ToString();
            string _uid = HelperFuntions.HasValue(CookieStore.GetCookie(CacheKey.Uid.ToString()));
            var jid = _qryString.Split('&').Count() > 1 ? _qryString.Split('&')[1] : cANDIDATE.JOB_ID.ToString();
            cANDIDATE.JOB_ID = new Guid(jid.Replace("JID=", ""));
            if (IsProfileDuplicate(cANDIDATE.MOBILE_NO, cANDIDATE.DOB.ToShortDateString())) //Additional check
            {
                ViewBag.IsExist = "Candidate is already exist.";
            }
            else
            {
                if (ModelState.IsValid && file != null && _uid != string.Empty)
                {
                    cANDIDATE.CANDIDATE_ID = Guid.NewGuid();
                    cANDIDATE.CANDIDATE_NAME = cANDIDATE.CANDIDATE_NAME.Trim();
                    cANDIDATE.ISACTIVE = true;
                    cANDIDATE.ISINNOTICEPERIOD = (!string.IsNullOrEmpty(frm["IsNP"]) && frm["IsNP"] == "Yes") ? true : false;
                    cANDIDATE.NOTICE_PERIOD = string.IsNullOrEmpty(frm["ddlNoticePeriod"]) ? "0" : frm["ddlNoticePeriod"].ToString();
                    cANDIDATE.MODIFIED_BY = (_uid == string.Empty ? User.Identity.Name : _uid);
                    cANDIDATE.MODIFIED_ON = HelperFuntions.GetDateTime();
                    cANDIDATE.NO_OF_TIMES_APPEARED = 1;
                    cANDIDATE.CREATED_BY = (_uid == string.Empty ? User.Identity.Name : _uid);
                    cANDIDATE.CREATED_ON = HelperFuntions.GetDateTime();
                    cANDIDATE.STATUS = db.STATUS_MASTER.Where(i => i.STATUS_ORDER == 1).FirstOrDefault().STATUS_ID.ToString();
                    cANDIDATE.RESUME_FILE_PATH = FileUpload(file, cANDIDATE.CANDIDATE_NAME);
                    db.CANDIDATES.Add(cANDIDATE);
                    await db.SaveChangesAsync();

                    vmodel.UpdateStatus(Guid.Empty, cANDIDATE.CANDIDATE_ID, string.Empty,1);
                     bool isSuperAdmin = HelperFuntions.HasValue(CookieStore.GetCookie(CacheKey.RoleName.ToString())).ToUpper().Contains("ADMIN") ? true : false;
                     if (isSuperAdmin) //For redirecting to Admin's job list page
                        return RedirectToAction("Index","Job");

                        return RedirectToAction("Index");
                }
            }
            ModelState.AddModelError("FAILED", "Failed to Insert");
            return View(cANDIDATE);
        }
        
        public async Task<ActionResult> ScheduleCandidate(string id, string date,string length,string sendTo, string comments,string statusId,string reSch)
        {
            try
            {
                var uid = HelperFuntions.HasValue(CookieStore.GetCookie(CacheKey.Uid.ToString()));
                Guid canId = (string.IsNullOrEmpty(id) ? Guid.Empty : Guid.Parse(id));
                string stsDesc = string.Empty;
                STATUS_HISTORY sHist = new STATUS_HISTORY();
                //BEGIN:SET TO INACTIVE BEFORE SETTING NEW STATUS in STATUS HISTORY TABLE
                var oHist = await db.STATUS_HISTORY.Where(c => c.CANDIDATE_ID == canId).ToListAsync();
                oHist.ForEach(a => a.ISACTIVE = false);
                await db.SaveChangesAsync();
                //END:SET TO INACTIVE BEFORE SETTING NEW STATUS

                if (reSch=="true") //For re-scheduling
                {
                    stsDesc = date+"--"+length+" MINS";
                    sHist.STATUS_ID = (string.IsNullOrEmpty(statusId) ? Guid.Empty : Guid.Parse(statusId));
                }
                else { 
                    var stsLst = db.STATUS_MASTER.ToList();
                    var sOrdr = stsLst.Where(i => i.STATUS_ID == Guid.Parse(statusId)).FirstOrDefault();
                    int stsOrdr = sOrdr.STATUS_NAME.Contains("TBS-F") ? 2 : 1;
                    var stsId = stsLst.Where(i => i.STATUS_ORDER == sOrdr.STATUS_ORDER + stsOrdr).FirstOrDefault();
                    stsDesc = stsId.STATUS_DESCRIPTION.ToString();
                    sHist.STATUS_ID = stsId.STATUS_ID;
                }
                
                sHist.CANDIDATE_ID = canId;
                sHist.ISACTIVE = true;
                sHist.SCHEDULED_TO = Convert.ToDateTime(date);
                sHist.SCHEDULED_FOR = sendTo;
                sHist.SCHEDULE_LENGTH_MINS = int.Parse(length);
                sHist.COMMENTS = comments;
                sHist.MODIFIED_BY = HelperFuntions.HasValue(CookieStore.GetCookie(CacheKey.Uid.ToString()));
                sHist.MODIFIED_ON = HelperFuntions.GetDateTime();
                db.STATUS_HISTORY.Add(sHist);
                await db.SaveChangesAsync();

                CANDIDATE cANDIDATE = db.CANDIDATES.Where(i => i.CANDIDATE_ID == sHist.CANDIDATE_ID).FirstOrDefault();
                cANDIDATE.STATUS = sHist.STATUS_ID.ToString();
                cANDIDATE.MODIFIED_BY = uid;
                cANDIDATE.MODIFIED_ON = HelperFuntions.GetDateTime();
                db.Entry(cANDIDATE).State = EntityState.Modified;
                await db.SaveChangesAsync();
                if (System.Configuration.ConfigurationManager.AppSettings["IsAppointmentMail"] == "true")
                {
                    bool reSche = reSch == "true" ? true : false;
                    await appointmentVM.SendInvite(date, length, sendTo, Guid.Parse(id), comments,reSche);
                }

                return Json(stsDesc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { throw ex; }
        }
        
        private string FileUpload(HttpPostedFileBase file,string canName)
        {
            string filename = string.Empty;
            if (file != null && file.ContentLength > 0)
                try
                {
                    filename = Path.GetFileName(file.FileName);
                    DateTime dtNow = HelperFuntions.GetDateTime();
                    filename = canName.Trim().Replace(' ', '_') + "_" + dtNow.Day + dtNow.ToString("MMM") + "_" + dtNow.Hour + dtNow.Minute + Path.GetExtension(filename);
                    string path = Path.Combine(Server.MapPath("~/UploadDocument"), filename);
                    file.SaveAs(path);
                    ViewBag.FileMessage = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.FileMessage = "ERROR:" + ex.Message.ToString();
                    throw ex;
                }
            else
            {
                ViewBag.FileMessage = "You have not specified a file.";
            }
            return filename;
        }

        // GET: Candidate/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CANDIDATE cANDIDATE = await db.CANDIDATES.FindAsync(id);
            ViewBag.PositionList = vmodel.GetAllPositions();
            if (cANDIDATE == null)
            {
                return HttpNotFound();
            }
            return View(cANDIDATE);
        }

        // POST: Candidate/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "CANDIDATE_ID,JOB_ID,CANDIDATE_NAME,YEARS_OF_EXP_TOTAL,YEARS_OF_EXP_RELEVANT,MOBILE_NO,ALTERNATE_MOBILE_NO,EMAIL,ALTERNATE_EMAIL_ID,DOB,CURRENT_COMPANY,CURRENT_LOCATION,NOTICE_PERIOD,COMMENTS,ISINNOTICEPERIOD,STATUS,ISACTIVE,MODIFIED_BY,CREATED_ON,CREATED_BY")] CANDIDATE cANDIDATE, HttpPostedFileBase file, FormCollection frm)
        {
                try
                {
                    if (!User.Identity.IsAuthenticated)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    if (ModelState.IsValid)
                    {
                        cANDIDATE.ISINNOTICEPERIOD = (!string.IsNullOrEmpty(frm["IsNP"]) && frm["IsNP"] == "Yes") ? true : false;
                        cANDIDATE.NOTICE_PERIOD = string.IsNullOrEmpty(frm["ddlNoticePeriod"]) ? "0" : frm["ddlNoticePeriod"].ToString();
                        if (!string.IsNullOrEmpty(frm["ddlCriteriaPosition"]))
                        {
                            cANDIDATE.JOB_ID = Guid.Parse(frm["ddlCriteriaPosition"].ToString());
                        }
                        cANDIDATE.MODIFIED_BY = HelperFuntions.HasValue(CookieStore.GetCookie(CacheKey.Uid.ToString()));
                        cANDIDATE.MODIFIED_ON = HelperFuntions.GetDateTime();
                        cANDIDATE.LAST_WORKING_DATE = string.IsNullOrEmpty(frm["LAST_WORKING_DATE"]) ? cANDIDATE.LAST_WORKING_DATE : DateTime.Parse(frm["LAST_WORKING_DATE"]);
                        cANDIDATE.RESUME_FILE_PATH = (file == null ? frm["RESUME_FILE_PATH"] : FileUpload(file, cANDIDATE.CANDIDATE_NAME));

                        //BEGIN:Re-Submitting the canddate's profile, after cooling period over
                        if (Request.UrlReferrer.Query.ToString().Contains("cTyp=2"))
                        {
                            var canId = cANDIDATE.CANDIDATE_ID;
                            cANDIDATE.CREATED_BY = HelperFuntions.HasValue(CookieStore.GetCookie(CacheKey.Uid.ToString()));
                            cANDIDATE.CREATED_ON = HelperFuntions.GetDateTime();
                            cANDIDATE.NO_OF_TIMES_APPEARED = 2;
                            cANDIDATE.STATUS = db.STATUS_MASTER.Where(i => i.STATUS_ORDER == 1).FirstOrDefault().STATUS_ID.ToString();

                            //Insert corresponding data in Status History table
                            vmodel.UpdateStatus(Guid.Empty, canId, string.Empty, 2);
                        }
                        //END:Re-Submitting the candidate's profile, after cooling period over

                        db.Entry(cANDIDATE).State = EntityState.Modified;
                        await db.SaveChangesAsync();

                        if (Request.UrlReferrer.Query.ToString().Contains("?styp=S"))
                            return RedirectToAction("SquadJobs");
                        else if (Request.UrlReferrer.Query.ToString().Contains("?styp=H") || Request.UrlReferrer.Query.ToString().Contains("cTyp=2"))
                            return RedirectToAction("Index", "Home");
                        else
                            return RedirectToAction("Index");
                    }

                    ModelState.AddModelError("FAILED", "Failed to update");
                    return View(cANDIDATE);
                }
                catch (Exception ex) { throw ex; }
        }

        // GET: Candidate/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            try { 
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CANDIDATE cANDIDATE = await db.CANDIDATES.FindAsync(id);
            var userLst = db.AspNetUsers.ToList();
            cANDIDATE.CREATED_BY = userLst.Where(u => u.Id == cANDIDATE.CREATED_BY).Select(um => um.FirstName + " " + um.LastName).FirstOrDefault();
            cANDIDATE.MODIFIED_BY = userLst.Where(u => u.Id == cANDIDATE.MODIFIED_BY).Select(um => um.FirstName + " " + um.LastName).FirstOrDefault();

            if (cANDIDATE == null)
            {
                return HttpNotFound();
            }
            return View(cANDIDATE);
            }
            catch (Exception ex) { throw ex; }
        }

        // POST: Candidate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id, CANDIDATE model)
        {
            try { 

                CANDIDATE cANDIDATE = await db.CANDIDATES.FindAsync(id);
                //db.CANDIDATES.Remove(cANDIDATE);
                cANDIDATE.COMMENTS = cANDIDATE.COMMENTS+ " || "+ model.COMMENTS;
                cANDIDATE.ISACTIVE = false;
                cANDIDATE.MODIFIED_BY = (HelperFuntions.HasValue(CookieStore.GetCookie(CacheKey.Uid.ToString())) == string.Empty ? User.Identity.Name : CookieStore.GetCookie(CacheKey.Uid.ToString()).ToString());
                cANDIDATE.MODIFIED_ON = HelperFuntions.GetDateTime();
                db.Entry(cANDIDATE).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index","Home");
            }
            catch (Exception ex) { throw ex; }
        }

        /// <summary>
        /// Check if Candidates mobile number and DOB are exist.
        /// </summary>
        /// <param name="mob"></param>
        /// <param name="dob"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult IsCandidateDuplicate(string mob, string dob)
        {
            try
            {
                bool isDupli = true;
                double InDays = 0;
                Guid canId = new Guid();
                var dupli = db.CANDIDATES.Where(u => u.MOBILE_NO == mob).ToList();
                if (dupli.Count() == 0)
                {
                    DateTime oDob = string.IsNullOrEmpty(dob) ? HelperFuntions.GetDateTime() : DateTime.Parse(dob);
                    isDupli = (db.CANDIDATES.Where(u => u.DOB == oDob && u.MOBILE_NO == mob).Count() > 0 ? true : false);
                }

                if (isDupli)
                {
                    DateTime daysOlder = dupli.Select(c => c.CREATED_ON).FirstOrDefault();
                    InDays = (HelperFuntions.GetDateTime() - daysOlder).TotalDays;
                    canId = dupli.Select(c => c.CANDIDATE_ID).FirstOrDefault();
                }

                var model = new
                {
                    IsDuplicate = IsProfileDuplicate(mob, dob),
                    InDays=InDays,
                    CandidateId=canId
                };
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ActionResult> SearchCriteria(string name, string vendor, string position, string status, string stdt, string edt, string flag)
        {
            try
            {
                var dbJobs = await db.JOBPOSTINGs.ToListAsync();
                var uid = HelperFuntions.HasValue(CookieStore.GetCookie(CacheKey.Uid.ToString()));
                CookieStore.SetCookie(CacheKey.CANSearch.ToString(), name + "|" + vendor + "|" + position + "|" + status + "|" + stdt + "|" + edt + "|" + flag, TimeSpan.FromHours(4));
                var dbCan = await db.CANDIDATES.Where(c => c.CREATED_BY == uid && c.ISACTIVE == true).ToListAsync();
                if (dbCan != null && dbCan.Count > 0)
                {
                    jobCanObj = GetCandidateSearchResults();
                }

                ViewBag.StatusList = vmodel.GetStatusList();
                ViewBag.VendorList = vmodel.GetVendorList();
                ViewBag.PositionList = vmodel.GetPositionForPartner();

                if (jobCanObj != null && jobCanObj.CandidateItems != null && jobCanObj.CandidateItems.Count > 0)
                {
                    jobCanObj.CandidateItems = GetPagination(jobCanObj.CandidateItems, string.Empty, 1);
                    return PartialView("_CandidateList", jobCanObj.CandidateItems);
                }
                else
                    return PartialView("_CandidateList");
               
            }
            catch (Exception ex) { throw ex; }
        }

        public async Task<ActionResult> ClearSearch()
        {
            CookieStore.ClearCookie(CacheKey.CANSearch.ToString());
            return await SearchCriteria(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,string.Empty);
        }

        public ActionResult _TimeLines(Guid Id)
        {
            List<STATUS_HISTORY> lstCan = db.STATUS_HISTORY.Where(c=>c.CANDIDATE_ID==Id).ToList();
            var canTimeLine = (from sh in lstCan
                               join sm in db.STATUS_MASTER on sh.STATUS_ID equals sm.STATUS_ID
                               join usr in db.AspNetUsers on sh.MODIFIED_BY.ToLower() equals usr.Id.ToLower()
                               select new CandidateViewModels
                               {
                                   COMMENTS = sh.COMMENTS,
                                   MODIFIED_BY=usr.FirstName+" "+usr.LastName,
                                   MODIFIED_ON=sh.MODIFIED_ON,
                                   SCHEDULED_TO=sh.SCHEDULED_TO,
                                   STATUS=sm.STATUS_DESCRIPTION,
                                   STATUS_ID=sm.STATUS_NAME
                               }).OrderByDescending(s=>s.MODIFIED_ON).ToList();
                               //select new {sh.COMMENTS,sm.STATUS_DESCRIPTION,usr.FirstName,usr.LastName,sh.MODIFIED_BY,sh.MODIFIED_ON,sh.SCHEDULED_TO}).ToList();

            return PartialView(canTimeLine);
        }

        /// <summary>
        /// Check the profile duplicate
        /// </summary>
        /// <param name="mob"></param>
        /// <param name="dob"></param>
        /// <returns></returns>
        private bool IsProfileDuplicate(string mob, string dob)
        {
            bool isDupli = true;
            var dupli = db.CANDIDATES.Where(u => u.MOBILE_NO == mob).ToList();
            if(dupli.Count()==0)
            {
                DateTime oDob = string.IsNullOrEmpty(dob) ? HelperFuntions.GetDateTime() : DateTime.Parse(dob);
                isDupli = (db.CANDIDATES.Where(u=>u.DOB == oDob && u.MOBILE_NO == mob).Count()>0 ? true : false);
            }
            return isDupli;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private List<CandidateViewModels> GetPagination(List<CandidateViewModels> jobCanObj, string sOdr, int? page)
        {
            ViewBag.CurrentSort = sOdr;

            if (jobCanObj != null && jobCanObj.Count > 0)
            {
                ViewBag.CNameSort = string.IsNullOrEmpty(sOdr) ? "Name_desc" : "";
                ViewBag.EmailSort = sOdr == "EMail_desc" ? "EMail_asc" : "EMail_desc";
                ViewBag.StatusSort = sOdr == "Sts_desc" ? "Sts_asc" : "Sts_desc";

                switch (sOdr)
                {
                    case "Name_desc":
                        jobCanObj = jobCanObj.OrderByDescending(s => s.CANDIDATE_NAME).ToList();
                        break;
                    case "EMail_desc":
                        jobCanObj = jobCanObj.OrderByDescending(s => s.EMAIL).ToList();
                        break;
                    case "EMail_asc":
                        jobCanObj = jobCanObj.OrderBy(s => s.EMAIL).ToList();
                        break;
                    case "Sts_desc":
                        jobCanObj = jobCanObj.OrderByDescending(s => s.STATUS).ToList();
                        break;
                    case "Sts_asc":
                        jobCanObj = jobCanObj.OrderBy(s => s.STATUS).ToList();
                        break;
                    default:
                        jobCanObj = jobCanObj.OrderBy(s => s.CANDIDATE_NAME).ToList();
                        break;
                }
            }

            ViewBag.PageSize = pageSize;
            ViewBag.PageNo = (page ?? 1);
            return jobCanObj;
        }

        private string GetModifiedById(CANDIDATE objCan)
        {
            string strUser = string.Empty;
            if (!string.IsNullOrEmpty(objCan.STATUS)) //is to check first time candidates-to show admin modified as empty
            {
                var stsSrc = db.STATUS_HISTORY.Where(i => i.CANDIDATE_ID == objCan.CANDIDATE_ID && i.ISACTIVE == true).OrderByDescending(j => j.MODIFIED_ON).FirstOrDefault();
                if (stsSrc != null && stsSrc.MODIFIED_BY != null && stsSrc.MODIFIED_ON != null)
                {
                    strUser = db.AspNetUsers.Where(x => x.Id == stsSrc.MODIFIED_BY).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault();
                    strUser = strUser + " / " + stsSrc.MODIFIED_ON.Value.ToString("dd-MMM");
                }
            }
            return strUser;
        }

        private JobAndCandidateViewModels GetCandidateSearchResults()
        {
            try
            {
                var cookie = CookieStore.GetCookie(CacheKey.CANSearch.ToString());
                string Uid = string.Empty;
                string name = string.Empty, vendor = string.Empty, position = string.Empty, status = string.Empty, stdt = string.Empty, edt = string.Empty, flag = string.Empty;
                if (!string.IsNullOrEmpty(cookie))
                {
                    string[] val = cookie.Split('|');
                    name = val[0]; vendor = val[1]; position = val[2]; status = val[3]; stdt = val[4]; edt = val[5]; flag = val[6];
                }
                if (!HelperFuntions.HasValue(CookieStore.GetCookie(CacheKey.RoleName.ToString())).ToUpper().Contains("ADMIN"))
                    Uid = CookieStore.GetCookie(CacheKey.Uid.ToString());

                var canlist = db.getSearchResults(position, name, status, vendor, stdt, edt, Uid, flag).ToList();
                jobCanObj.CandidateItems = canlist.Select(i => new CandidateViewModels
                {
                    CANDIDATE_ID = i.CANDIDATE_ID,
                    CANDIDATE_NAME = i.CANDIDATE_NAME,
                    MOBILE_NO = i.MOBILE_NO,
                    EMAIL = i.EMAIL,
                    POSITION = i.POSITION,
                    RESUME_FILE_PATH = (string.IsNullOrEmpty(i.RESUME_FILE_PATH) ? string.Empty : i.RESUME_FILE_PATH),
                    NOTICE_PERIOD = i.NOTICE_PERIOD,
                    CURRENT_COMPANY = i.CURRENT_COMPANY,
                    YEARS_OF_EXP_TOTAL = i.YEARS_OF_EXP_TOTAL,
                    LAST_WORKING_DATE = i.LAST_WORKING_DATE,
                    VENDOR_NAME = i.VENDOR_NAME,
                    STATUS = i.STATUS,
                    STATUS_ID = i.STATUS_ID,
                    CREATED_ON = i.CREATED_ON,
                    MODIFIED_ON = i.MODIFIED_ON,
                    MODIFIED_BY = i.MODIFIED_BY,
                    NO_OF_TIMES_APPEARED=i.NO_OF_TIMES_APPEARED
                }).ToList();
                return jobCanObj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetPartnerName(string pId)
        {
            var vendor = (from u in db.AspNetUsers.Where(i => i.Id == pId)
                          join v in db.VENDOR_MASTER on u.Vendor_Id equals v.VENDOR_ID
                          select v.VENDOR_NAME).FirstOrDefault();
            return vendor.ToString();
        }

        /// <summary>
        /// Archive the original candidate data when cooling period is over
        /// </summary>
        /// <param name="canId">Candidate Id</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> ArchiveCandidate(Guid Id)
        {
            CANDIDATES_ARCHIVE cArch = new CANDIDATES_ARCHIVE();
            CANDIDATE oCan = await db.CANDIDATES.Where(c => c.CANDIDATE_ID == Id).SingleAsync();
            cArch.CANDIDATE_ID = oCan.CANDIDATE_ID;
            cArch.CANDIDATE_NAME = oCan.CANDIDATE_NAME.Trim();
            cArch.ALTERNATE_EMAIL_ID = oCan.ALTERNATE_EMAIL_ID;
            cArch.ALTERNATE_MOBILE_NO = oCan.ALTERNATE_MOBILE_NO;
            cArch.ANY_OTHER_OFFER = oCan.ANY_OTHER_OFFER;
            cArch.COMMENTS = oCan.COMMENTS;
            cArch.CURRENT_COMPANY = oCan.CURRENT_COMPANY;
            cArch.CURRENT_LOCATION = oCan.CURRENT_LOCATION;
            cArch.DOB = oCan.DOB;
            cArch.EMAIL = oCan.EMAIL;
            cArch.JOB_ID = oCan.JOB_ID;
            cArch.LAST_WORKING_DATE = oCan.LAST_WORKING_DATE;
            cArch.MOBILE_NO = oCan.MOBILE_NO;
            cArch.NO_OF_TIMES_APPEARED = oCan.NO_OF_TIMES_APPEARED;
            cArch.RESUME_SOURCE = oCan.RESUME_SOURCE;
            cArch.YEARS_OF_EXP_RELEVANT = oCan.YEARS_OF_EXP_RELEVANT;
            cArch.YEARS_OF_EXP_TOTAL = oCan.YEARS_OF_EXP_TOTAL;
            cArch.ISACTIVE = oCan.ISACTIVE;
            cArch.ISINNOTICEPERIOD = oCan.ISINNOTICEPERIOD;
            cArch.NOTICE_PERIOD = oCan.NOTICE_PERIOD;
            cArch.MODIFIED_BY = oCan.MODIFIED_BY;
            cArch.MODIFIED_ON = oCan.MODIFIED_ON;
            cArch.CREATED_BY = oCan.CREATED_BY;
            cArch.CREATED_ON = oCan.CREATED_ON;
            cArch.STATUS = oCan.STATUS;
            cArch.RESUME_FILE_PATH = oCan.RESUME_FILE_PATH;
            db.CANDIDATES_ARCHIVE.Add(cArch);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<ActionResult> ExportToExcel()
        {
            try
            {
                System.Web.UI.WebControls.GridView gv = new System.Web.UI.WebControls.GridView();
                List<CANDIDATE> canDb = new List<CANDIDATE>();
                var uid = HelperFuntions.HasValue(CookieStore.GetCookie(CacheKey.Uid.ToString()));
                canDb = await db.CANDIDATES.Where(c => c.CREATED_BY == uid && c.ISACTIVE == true).ToListAsync();

                var canLst = canDb.Select(i => new 
                {
                    CANDIDATE_NAME = i.CANDIDATE_NAME,
                    MOBILE_NO = i.MOBILE_NO,
                    EMAIL = i.EMAIL,
                    CURRENT_COMPANY = i.CURRENT_COMPANY,
                    NOTICE_PERIOD = i.NOTICE_PERIOD,
                    YEARS_OF_EXP_TOTAL = i.YEARS_OF_EXP_TOTAL,
                    LAST_WORKING_DATE = i.LAST_WORKING_DATE,
                    STATUS = vmodel.GetStatusNameById(i.CANDIDATE_ID),
                }).ToList();

                gv.DataSource = canLst;
                gv.DataBind();
                Response.ClearContent();
                Response.Buffer = true;
                string fileName = "Candidates_" + DateTime.Now.Day + DateTime.Now.ToString("MMM") + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second + ".xls";
                Response.AddHeader("content-disposition", "attachment; filename=" + fileName);
                Response.ContentType = "application/ms-excel";
                Response.Charset = "";
                StringWriter sw = new StringWriter();
                System.Web.UI.HtmlTextWriter htw = new System.Web.UI.HtmlTextWriter(sw);
                gv.RenderControl(htw);
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex) { throw ex; }
        }
        
        protected override void OnException(ExceptionContext filterContext)
        {
            Exception e = filterContext.Exception;
            //Log Exception e to DB.
            if (!filterContext.ExceptionHandled)
            {
                LoggingUtil.LogException(e, errorLevel: ErrorLevel.Critical);
            }
            //filterContext.Result = new ViewResult()
            //{
            //    ViewName = "Error"
            //};
        }
    }
}

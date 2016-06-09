﻿using HRPortal.Helper;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HRPortal.Models
{
    public class CandidateViewModels
    {
        HRPortalEntities dbContext = new HRPortalEntities();

            public Guid CANDIDATE_ID { get; set; }
            public string CANDIDATE_NAME { get; set; }
            public string VENDOR_NAME { get; set; }
            public Guid JOB_ID { get; set; }
            public string POSITION { get; set; }
            public int YEARS_OF_EXP_TOTAL { get; set; }
            public Nullable<int> YEARS_OF_EXP_RELEVANT { get; set; }
            public string MOBILE_NO { get; set; }
            public string ALTERNATE_MOBILE_NO { get; set; }
            public string EMAIL { get; set; }
            public string ALTERNATE_EMAIL_ID { get; set; }
            public DateTime DOB { get; set; }
            public string CURRENT_COMPANY { get; set; }
            public string NOTICE_PERIOD { get; set; }
            public Nullable<bool> ANY_OTHER_OFFER { get; set; }
            public Nullable<System.DateTime> LAST_WORKING_DATE { get; set; }
            public string RESUME_FILE_PATH { get; set; }
            public string COMMENTS { get; set; }
            public Nullable<bool> ISINNOTICEPERIOD { get; set; }
            public bool ISACTIVE { get; set; }
            public string MODIFIED_BY { get; set; }
            public Nullable<System.DateTime> MODIFIED_ON { get; set; }
            public string CREATED_BY { get; set; }
            public DateTime CREATED_ON { get; set; }
            public string STATUS { get; set; }
            public string STATUS_ID { get; set; }

        /// <summary>
        /// Update the candidate status on creating the canidate profile
        /// </summary>
        /// <param name="stid"></param>
        /// <param name="cId"></param>
        /// <param name="cmnts"></param>
        /// <returns></returns>
        public string UpdateStatus(Guid stid, Guid cId, string cmnts)
        {
            STATUS_HISTORY stsHist = new STATUS_HISTORY();
            var stsId = dbContext.STATUS_MASTER.Where(i => i.STATUS_ORDER == 1).FirstOrDefault().STATUS_ID;
            var uid = HttpRuntime.Cache.Get(CacheKey.Uid.ToString()) == null ? Guid.NewGuid() : HttpRuntime.Cache.Get(CacheKey.Uid.ToString());
            stsHist = new STATUS_HISTORY();
            stsHist.STATUS_ID = ((stid == null || stid == Guid.Empty) ? stsId : stid);
            stsHist.CANDIDATE_ID = cId;
            stsHist.COMMENTS = string.IsNullOrEmpty(cmnts) ? "Initial Status - SCR-SBM" : cmnts;
            stsHist.ISACTIVE = true;
            stsHist.MODIFIED_BY = uid.ToString();
            stsHist.MODIFIED_ON = DateTime.Now;
            dbContext.STATUS_HISTORY.Add(stsHist);
            dbContext.SaveChanges();
            return "OK";
        }

        /// <summary>
        /// Update the candidate status on creating the canidate profile
        /// </summary>
        public void AutoUpdateStatus()
        {
            STATUS_HISTORY stsHist = new STATUS_HISTORY();
            var sHist = dbContext.STATUS_HISTORY.Where(i => i.SCHEDULED_TO != null && i.SCHEDULED_TO <= DateTime.Now && i.ISACTIVE==true).ToList();
            if (sHist.Count > 0)
            { 
                var uid = HttpRuntime.Cache.Get(CacheKey.Uid.ToString()) == null ? Guid.NewGuid() : HttpRuntime.Cache.Get(CacheKey.Uid.ToString());
                var stsLst = dbContext.STATUS_MASTER.Where(i => i.ISACTIVE == true).ToList();

                foreach (var item in sHist)
                {
                    int stsOrdr = stsLst.Where(i => i.STATUS_ID == item.STATUS_ID).FirstOrDefault().STATUS_ORDER.GetValueOrDefault();
                    stsHist = new STATUS_HISTORY();
                    stsHist.STATUS_ID = stsLst.Where(i => i.STATUS_ORDER == stsOrdr+1).FirstOrDefault().STATUS_ID;
                    stsHist.CANDIDATE_ID = item.CANDIDATE_ID;
                    stsHist.COMMENTS = "Auto updated the status to Feedback Pending for passed the due date.";
                    stsHist.ISACTIVE = true;
                    stsHist.MODIFIED_BY = uid.ToString();
                    stsHist.MODIFIED_ON = DateTime.Now;
                    dbContext.STATUS_HISTORY.Add(stsHist);
                    dbContext.SaveChanges();
                }
            }
        }

        public SelectList GetStatusList()
        {
            var sts = dbContext.STATUS_MASTER.Where(i => i.ISACTIVE == true).Select(s => new { s.STATUS_ID, s.STATUS_NAME, s.STATUS_DESCRIPTION, s.STATUS_ORDER }).OrderBy(v => v.STATUS_ORDER).ToList();
           return new SelectList(sts.AsEnumerable(), "STATUS_ID", "STATUS_DESCRIPTION", 1);
        }

        public string GetStatusNameById(Guid id)
        {
            string stsName = "Screening–Submitted";// "SCR -SBM";
            var stsSrc = dbContext.STATUS_HISTORY.Where(i => i.CANDIDATE_ID == id).ToList();
            if (stsSrc.Count > 0)
            {
                var stsH = stsSrc.OrderByDescending(j => j.MODIFIED_ON).FirstOrDefault().STATUS_ID;
                stsName = dbContext.STATUS_MASTER.Where(i => i.STATUS_ID == stsH).FirstOrDefault().STATUS_DESCRIPTION;
            }
            return stsName;
        }

    }
}
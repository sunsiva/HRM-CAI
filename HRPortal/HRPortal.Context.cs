﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HRPortal
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class HRPortalEntities : DbContext
    {
        public HRPortalEntities()
            : base("name=HRPortalEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<C__MigrationHistory> C__MigrationHistory { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<CANDIDATE> CANDIDATES { get; set; }
        public virtual DbSet<ERROR_LOG> ERROR_LOG { get; set; }
        public virtual DbSet<EVENTSCHEDULE> EVENTSCHEDULEs { get; set; }
        public virtual DbSet<EXCEPTION_LOGGING> EXCEPTION_LOGGING { get; set; }
        public virtual DbSet<JOB_HISTORY> JOB_HISTORY { get; set; }
        public virtual DbSet<JOBPOSTING> JOBPOSTINGs { get; set; }
        public virtual DbSet<STATUS_HISTORY> STATUS_HISTORY { get; set; }
        public virtual DbSet<STATUS_MASTER> STATUS_MASTER { get; set; }
        public virtual DbSet<UserLog> UserLogs { get; set; }
        public virtual DbSet<UserXRole> UserXRoles { get; set; }
        public virtual DbSet<VENDOR_MASTER> VENDOR_MASTER { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
    
        public virtual ObjectResult<getSearchResults_Result> getSearchResults(string positionName, string candidateName, string statusIds, string partner, string startDate, string endDate, string flag)
        {
            var positionNameParameter = positionName != null ?
                new ObjectParameter("PositionName", positionName) :
                new ObjectParameter("PositionName", typeof(string));
    
            var candidateNameParameter = candidateName != null ?
                new ObjectParameter("CandidateName", candidateName) :
                new ObjectParameter("CandidateName", typeof(string));
    
            var statusIdsParameter = statusIds != null ?
                new ObjectParameter("StatusIds", statusIds) :
                new ObjectParameter("StatusIds", typeof(string));
    
            var partnerParameter = partner != null ?
                new ObjectParameter("Partner", partner) :
                new ObjectParameter("Partner", typeof(string));
    
            var startDateParameter = startDate != null ?
                new ObjectParameter("StartDate", startDate) :
                new ObjectParameter("StartDate", typeof(string));
    
            var endDateParameter = endDate != null ?
                new ObjectParameter("EndDate", endDate) :
                new ObjectParameter("EndDate", typeof(string));
    
            var flagParameter = flag != null ?
                new ObjectParameter("flag", flag) :
                new ObjectParameter("flag", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<getSearchResults_Result>("getSearchResults", positionNameParameter, candidateNameParameter, statusIdsParameter, partnerParameter, startDateParameter, endDateParameter, flagParameter);
        }
    }
}

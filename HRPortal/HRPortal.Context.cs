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
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<CANDIDATE> CANDIDATES { get; set; }
        public virtual DbSet<ERROR_LOG> ERROR_LOG { get; set; }
        public virtual DbSet<EVENTSCHEDULE> EVENTSCHEDULEs { get; set; }
        public virtual DbSet<EXCEPTION_LOGGING> EXCEPTION_LOGGING { get; set; }
        public virtual DbSet<JOB_HISTORY> JOB_HISTORY { get; set; }
        public virtual DbSet<JOBPOSTING> JOBPOSTINGs { get; set; }
        public virtual DbSet<Number> Numbers { get; set; }
        public virtual DbSet<STATUS_HISTORY> STATUS_HISTORY { get; set; }
        public virtual DbSet<STATUS_MASTER> STATUS_MASTER { get; set; }
        public virtual DbSet<UserLog> UserLogs { get; set; }
        public virtual DbSet<UserXRole> UserXRoles { get; set; }
        public virtual DbSet<VENDOR_MASTER> VENDOR_MASTER { get; set; }
        public virtual DbSet<JOBXVENDOR> JOBXVENDORs { get; set; }
    
        [DbFunction("HRPortalEntities", "SplitString")]
        public virtual IQueryable<SplitString_Result> SplitString(string input, string character)
        {
            var inputParameter = input != null ?
                new ObjectParameter("Input", input) :
                new ObjectParameter("Input", typeof(string));
    
            var characterParameter = character != null ?
                new ObjectParameter("Character", character) :
                new ObjectParameter("Character", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<SplitString_Result>("[HRPortalEntities].[SplitString](@Input, @Character)", inputParameter, characterParameter);
        }
    
        [DbFunction("HRPortalEntities", "fnSplitString")]
        public virtual IQueryable<fnSplitString_Result> fnSplitString(string @string, string delimiter)
        {
            var stringParameter = @string != null ?
                new ObjectParameter("string", @string) :
                new ObjectParameter("string", typeof(string));
    
            var delimiterParameter = delimiter != null ?
                new ObjectParameter("delimiter", delimiter) :
                new ObjectParameter("delimiter", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnSplitString_Result>("[HRPortalEntities].[fnSplitString](@string, @delimiter)", stringParameter, delimiterParameter);
        }
    
        [DbFunction("HRPortalEntities", "funcSplitValue")]
        public virtual IQueryable<funcSplitValue_Result> funcSplitValue(string splitOn, string list)
        {
            var splitOnParameter = splitOn != null ?
                new ObjectParameter("SplitOn", splitOn) :
                new ObjectParameter("SplitOn", typeof(string));
    
            var listParameter = list != null ?
                new ObjectParameter("List", list) :
                new ObjectParameter("List", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<funcSplitValue_Result>("[HRPortalEntities].[funcSplitValue](@SplitOn, @List)", splitOnParameter, listParameter);
        }
    
        public virtual ObjectResult<GetCandidateDetailsByLastworkingdate_Result> GetCandidateDetailsByLastworkingdate()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetCandidateDetailsByLastworkingdate_Result>("GetCandidateDetailsByLastworkingdate");
        }
    
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
    
        public virtual ObjectResult<rptGetCandidatesIdleTimeByWeek_Result> rptGetCandidatesIdleTimeByWeek(string week)
        {
            var weekParameter = week != null ?
                new ObjectParameter("Week", week) :
                new ObjectParameter("Week", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<rptGetCandidatesIdleTimeByWeek_Result>("rptGetCandidatesIdleTimeByWeek", weekParameter);
        }
    
        public virtual ObjectResult<rptGetCandidatesIdleTimeDetailsByWeek_Result> rptGetCandidatesIdleTimeDetailsByWeek(string week)
        {
            var weekParameter = week != null ?
                new ObjectParameter("Week", week) :
                new ObjectParameter("Week", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<rptGetCandidatesIdleTimeDetailsByWeek_Result>("rptGetCandidatesIdleTimeDetailsByWeek", weekParameter);
        }
    
        public virtual ObjectResult<rptGetCandidatesStaging_Result> rptGetCandidatesStaging(string partner)
        {
            var partnerParameter = partner != null ?
                new ObjectParameter("Partner", partner) :
                new ObjectParameter("Partner", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<rptGetCandidatesStaging_Result>("rptGetCandidatesStaging", partnerParameter);
        }
    
        public virtual ObjectResult<rptGetCandidatesStagingByPartner_Result> rptGetCandidatesStagingByPartner(string partner)
        {
            var partnerParameter = partner != null ?
                new ObjectParameter("Partner", partner) :
                new ObjectParameter("Partner", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<rptGetCandidatesStagingByPartner_Result>("rptGetCandidatesStagingByPartner", partnerParameter);
        }
    
        public virtual ObjectResult<getCandidatesSchedules_Result> getCandidatesSchedules(string navigDate)
        {
            var navigDateParameter = navigDate != null ?
                new ObjectParameter("NavigDate", navigDate) :
                new ObjectParameter("NavigDate", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<getCandidatesSchedules_Result>("getCandidatesSchedules", navigDateParameter);
        }
    }
}

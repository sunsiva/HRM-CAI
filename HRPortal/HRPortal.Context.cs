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
        public virtual DbSet<ERROR_LOG> ERROR_LOG { get; set; }
        public virtual DbSet<VENDOR_MASTER> VENDOR_MASTER { get; set; }
        public virtual DbSet<UserLog> UserLogs { get; set; }
        public virtual DbSet<STATUS_MASTER> STATUS_MASTER { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<JOBPOSTING> JOBPOSTINGs { get; set; }
        public virtual DbSet<UserXRole> UserXRoles { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<STATUS_HISTORY> STATUS_HISTORY { get; set; }
        public virtual DbSet<EVENTSCHEDULE> EVENTSCHEDULEs { get; set; }
        public virtual DbSet<EXCEPTION_LOGGING> EXCEPTION_LOGGING { get; set; }
        public virtual DbSet<CANDIDATE> CANDIDATES { get; set; }
    }
}

//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class UserLog
    {
        public int UserLogId { get; set; }
        public string UserLogName { get; set; }
        public string UserLogDesc { get; set; }
        public string LoggedInBy { get; set; }
        public Nullable<System.DateTime> LoggedInOn { get; set; }
        public Nullable<System.DateTime> LoggedOutOn { get; set; }
        public string UserIP { get; set; }
        public string UserMAC { get; set; }
        public Nullable<bool> IsOnline { get; set; }
    }
}

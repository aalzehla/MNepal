using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models
{
    //[PetaPoco.TableName("MNTraceIDHelper")]
    //[PetaPoco.PrimaryKey("id")]
    public class MNTraceID
    {
        public int id { get; set; }
        public string TraceID { get; set; }
    }
}
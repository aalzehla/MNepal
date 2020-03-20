using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalProject.Models
{
    public class MNError
    {
        public bool status { get; private set; }
        public string message { get; private set; }
        public MNError(bool status, string message)
        {
            this.status = status;
            this.message = message;
        }
    }
}
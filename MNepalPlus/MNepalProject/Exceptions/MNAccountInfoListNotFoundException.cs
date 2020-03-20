using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace MNepalProject.CustomExceptions
{
    public class MNAccountInfoListNotFoundException : System.Exception
    {
        public MNAccountInfoListNotFoundException()
        {
        }

        public MNAccountInfoListNotFoundException(string message)
            : base(message)
        {
        }

        public MNAccountInfoListNotFoundException(string message, System.Exception inner)
            : base(message, inner)
        {
        }
    }
}
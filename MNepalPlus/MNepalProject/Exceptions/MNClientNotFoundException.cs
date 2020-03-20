using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalProject.CustomExceptions
{
    public class MNClientNotFoundException:System.Exception
    {
         public MNClientNotFoundException()
        {
        }

        public MNClientNotFoundException(string message)
            : base(message)
        {
        }

        public MNClientNotFoundException(string message, System.Exception inner)
            : base(message, inner)
        {
        }
    }
}
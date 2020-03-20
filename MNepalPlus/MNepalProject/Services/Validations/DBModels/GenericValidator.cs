using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentValidation;

namespace MNepalProject.Services.Validations.DBModels
{
    public class GenericValidator
    {
        public bool BeAValidInteger(Type value)
        {
            Type intType = typeof(Int16);
            if (value.IsInstanceOfType(intType)){return true;}
            else {return false;}
        }
        public bool BeAValidString(Type value)
        {
            Type stringType = typeof(String);
            if (value.IsInstanceOfType(stringType)) { return true; }
            else { return false; }
        }
    }
}
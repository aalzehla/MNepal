using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models
{
    public class CustomerSetupInfo
    {
        /**************>Personal Information*******************/
        public string CustomerId
        {
            get;
            set;
        }

        public string CustomerCbsId
        {
            get;
            set;
        }

        public string MobileNumber
        {
            get;
            set;
        }

        public string ServiceProvider
        {
            get;
            set;
        }

        public string CustomerName
        {
            get;
            set;
        }

        public string Gender
        {
            get;
            set;
        }

        public string Address
        {
            get;
            set;
        }

        public string ContactNumber
        {
            get;
            set;
        }

        public string CustomerType
        {
            get;
            set;
        }

        public string CreatedBy
        {
            get;
            set;
        }

        public string CreatedDate
        {
            get;
            set;
        }

        public string CreatorBranch
        {
            get;
            set;
        }

        public string ActiveBlockedStatus
        {
            get;
            set;
        }

        public string Status
        {
            get;
            set;
        }

        public string PinReset
        {
            get;
            set;
        }

        public string ServiceBlocked
        {
            get;
            set;
        }

        /******************Profile Information*****************************/

        public string ProfileName
        {
            get;
            set;
        }

        public string ProfileStatus
        {
            get;
            set;
        }

        /**************Profile Charge*******************/

        public string RegistrationHasCharge
        {
            get;
            set;
        }

        public string RegistrationAmount
        {
            get;
            set;
        }

        public string RenewalHasCharge
        {
            get;
            set;
        }

        public string RenewalAmount
        {
            get;
            set;
        }

        public string PinResetHasCharge
        {
            get;
            set;
        }

        public string PinResetAmount
        {
            get;
            set;
        }

        public string TrialPeriod
        {
            get;
            set;
        }

        public string RenewalPeriod
        {
            get;
            set;
        }


        /*********Assigned Profile Features******************/

        //public string PinResetAmount
        //{
        //    get;
        //    set;
        //}

        //public string TrialPeriod
        //{
        //    get;
        //    set;
        //}

        //public string RenewalPeriod
        //{
        //    get;
        //    set;
        //}

        public string Mode
        {
            get;
            set;
        }
    }
}
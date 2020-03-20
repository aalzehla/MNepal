using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models
{
    public class NTTopUp
    {


        public string recordId { get; set; }
        public string sourceDealer { get; set; }
        public string targetDealer { get; set; }
        public string sourceNumber { get; set; }
        public string targetNumber { get; set; }
        public string phoneNumber { get; set; }
        public string operationTime { get; set; }
        public string @operator { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string doneCode { get; set; }
        public string optType { get; set; }
        public string dealerName { get; set; }
        public string hold1 { get; set; }
        public string hold2 { get; set; }
        public string parentId { get; set; }
        public string channel { get; set; }
        public string transferType { get; set; }
        public string operationType { get; set; }
        public string phoneType { get; set; }
        public string amount { get; set; }
    }
}
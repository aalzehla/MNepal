using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MNepalProject.Models
{
    [PetaPoco.TableName("MNPinLog")]
    public class MNPinLog
    {
        [Column(TypeName = "VARCHAR")]
        [StringLength(8)]
        public string ClientCode { get; set; }

        public string UserName { get; set; }
        public string PinChanged { get; set; }
        public string PassChanged { get; set; }
        public string IsFirstLogin { get; set; }
        public MNPinLog() { }
        public MNPinLog(string ClientCode, string UserName, string PinChanged, string PassChanged)
        {
            this.ClientCode = ClientCode;
            this.UserName = UserName;
            this.PinChanged = PinChanged;
            this.PassChanged = PassChanged;

        }

        public MNPinLog(string ClientCode, string UserName, string PinChanged)
        {
            this.ClientCode = ClientCode;
            this.UserName = UserName;
            this.PinChanged = PinChanged;
        }
    }
}
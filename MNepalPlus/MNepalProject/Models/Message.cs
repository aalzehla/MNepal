using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace MNepalProject.Models
{
    public class Message
    {
        public string jsonMessage { get; set; }
        public Message createMessage(object T)
        {
            this.jsonMessage = JsonConvert.SerializeObject(T);
            return this;
        }
    }
}
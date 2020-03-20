using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace WCF.MNepal.Models
{
    public class Message<T>
    {
        public string message { get; set; }

        public Message(T item)
        {
            /*DataContractJsonSerializer serializer = new DataContractJsonSerializer(item.GetType());
            MemoryStream memoryStream = new MemoryStream();
            serializer.WriteObject(memoryStream, item) ;

            // Return the results serialized as JSON
            this.message = Encoding.Default.GetString(memoryStream.ToArray());
            */
            string json = JsonConvert.SerializeObject(item);
            this.message = json;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using MNepalProject;
using MNepalProject.Models;
using MNepalProject.Controllers;
using MNepalProject.DAL;

namespace WCF.MNepal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class PinChangeService
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
                   RequestFormat = WebMessageFormat.Json,
                   ResponseFormat = WebMessageFormat.Json
                   )]

        public string IN(string FocusOneParams)
        {
            string focusOneParams = FocusOneParams;
            

          /*  Bank bank = new Bank();
            bank.BIN = "ASDF";
            bank.CreatedDate = DateTime.Now;
            bank.Name = "ASDF ASDF ASDF";
            BanksController controller = new BanksController();
            try {
                controller.CreateBank(bank);
            }catch(Exception ex)
            {
                string error = ex.ToString();
            }
            */
            
            // Serialize the results as JSON
            List<Bank> results = new List<Bank>();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(results.GetType());
            MemoryStream memoryStream = new MemoryStream();
            serializer.WriteObject(memoryStream, results);

            // Return the results serialized as JSON
            string json = Encoding.Default.GetString(memoryStream.ToArray());
            return json;


            var objString =;

            

            return (T)DeserializeObject<T>(objString);

            //return string.Format("You entered: {0}", focusOneParams);
        }

        public static T DeserializeObject<T>(string objString)
        {
            using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(objString)))
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                return (T)serializer.ReadObject(stream);
            }
        }

       
    }
}

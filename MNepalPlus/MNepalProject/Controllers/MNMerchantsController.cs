using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MNepalProject.Models;
using MNepalProject.IRepository;
using MNepalProject.Repository;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Data;
using MNepalProject.DAL;

namespace MNepalProject.Controllers
{
    public class MNMerchantsController : Controller
    {
        public IMNMerchantsRepository mnMerchantsRepository;


        public MNMerchantsController()
        {
            this.mnMerchantsRepository = new MNMerchantsRepository(new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection()));
        }
        // GET: MNMerchants
        public ActionResult Index()
        {
            return View();
        }

        string constrmerchants = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;



        public string GETNewJSONMerchant(string mobile, string catid)
        {
            JArray finalmerchants = new JArray();

            Dictionary<string, JArray> merchantsarray = new Dictionary<string, JArray>();
            string jsonversion = @"['1.1.1']";
            JArray jsonversionarray = JArray.Parse(jsonversion);
            merchantsarray.Add("version", jsonversionarray);

            JArray merchants = new JArray();
            
            string sqlmerchants = "select * from MNMerchant (NOLOCK) order by Id asc";
            
            try
            {
                using (System.Data.SqlClient.SqlConnection conmerchants = new System.Data.SqlClient.SqlConnection(constrmerchants))
                {
                    conmerchants.Open();
                    using (System.Data.SqlClient.SqlCommand commandMerchants = new System.Data.SqlClient.SqlCommand(sqlmerchants, conmerchants))
                    {
                        using (System.Data.SqlClient.SqlDataReader rdrmerchants = commandMerchants.ExecuteReader())
                        {
                            while (rdrmerchants.Read())
                            {
                                JObject merchantDynamicControl = new JObject();
                                int merchantID = int.Parse(rdrmerchants["Id"].ToString());
                                string merchantName = rdrmerchants["Name"].ToString();
                                //string merchantIcon = rdrmerchants["IconUrl"].ToString();
                                int ParentId = int.Parse(rdrmerchants["Parentid"].ToString());
                                
                                string constrmerchantDynamicField = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;
                                string sqlmerchantDynamicField = "";
                                if (merchantID != 0)
                                {
                                    sqlmerchantDynamicField = "select d.Id,d.Label,d.DataName,d.FieldType,d.DataType,d.Required,d.Regex,d.Datalist,d.Range,d.IsDefault,d.ErrorMessage from " +
                                        "MNMerchantDynamicField d (NOLOCK) inner join MNMerchantForm f (NOLOCK) on d.Id= f.LId where f.MId = '" + merchantID + "' order by d.Id asc";
                                }
                                
                                JArray merchantDynamicFieldarray = new JArray();
                                
                                try
                                {
                                    using (System.Data.SqlClient.SqlConnection conmerchantsDynamicField = new System.Data.SqlClient.SqlConnection(constrmerchantDynamicField))
                                    {
                                        conmerchantsDynamicField.Open();
                                        using (System.Data.SqlClient.SqlCommand commandmerchantDynamicField = new System.Data.SqlClient.SqlCommand(sqlmerchantDynamicField, conmerchantsDynamicField))
                                        using (System.Data.SqlClient.SqlDataReader rdrmerchantDynamicField = commandmerchantDynamicField.ExecuteReader())
                                        {
                                            while (rdrmerchantDynamicField.Read())
                                            {
                                                string controlid = rdrmerchantDynamicField["Id"].ToString();
                                                JObject dynamicFields = new JObject();
                                                dynamicFields["Id"] = controlid;
                                                dynamicFields["Label"] = rdrmerchantDynamicField["Label"].ToString();
                                                dynamicFields["DataName"] = rdrmerchantDynamicField["DataName"].ToString();
                                                dynamicFields["FieldType"] = rdrmerchantDynamicField["FieldType"].ToString();
                                                dynamicFields["DataType"] = rdrmerchantDynamicField["DataType"].ToString();
                                                dynamicFields["Required"] = bool.Parse(rdrmerchantDynamicField["Required"].ToString());
                                                dynamicFields["Regex"] = rdrmerchantDynamicField["Regex"].ToString();
                                                dynamicFields["Datalist"] = rdrmerchantDynamicField["Datalist"].ToString();
                                                dynamicFields["Range"] = rdrmerchantDynamicField["Range"].ToString();
                                                dynamicFields["IsDefault"] = bool.Parse(rdrmerchantDynamicField["IsDefault"].ToString());
                                                dynamicFields["ErrorMessage"] = rdrmerchantDynamicField["ErrorMessage"].ToString();

                                                merchantDynamicFieldarray.Add(dynamicFields);
                                            }
                                           
                                        }
                                    }
                              }
                              catch (Exception ex)
                              {

                              }

                              
                              merchantDynamicControl["FormID"] = merchantID;
                              merchantDynamicControl["FormTitle"] = merchantName;
                              merchantDynamicControl["Controls"] = merchantDynamicFieldarray;

                              merchants.Add(merchantDynamicControl);
                            }

                            
                        }
                    }
                }
            }

            catch (Exception Ex)
            {
                string aa = Ex.ToString();
            }

            merchantsarray.Add("merchants", merchants);
            return JsonConvert.SerializeObject(merchantsarray, Formatting.Indented);
        }




        public string GETJSONMerchant(string mobile, string catid)
        {
            Dictionary<string, JArray> merchantsarray = new Dictionary<string, JArray>();
            string jsonversion = @"['1.1.1']";
            JArray jsonversionarray = JArray.Parse(jsonversion);
            merchantsarray.Add("version", jsonversionarray);
            string sqlmerchants = "select * from MNMerchantType (NOLOCK)";
            try
            {
                using (System.Data.SqlClient.SqlConnection conmerchants = new System.Data.SqlClient.SqlConnection(constrmerchants))
                {
                    conmerchants.Open();
                    using (System.Data.SqlClient.SqlCommand commandMerchants = new System.Data.SqlClient.SqlCommand(sqlmerchants, conmerchants))
                    {
                        using (System.Data.SqlClient.SqlDataReader rdrmerchants = commandMerchants.ExecuteReader())
                        {
                            while (rdrmerchants.Read())
                            {
                                string merchantsTypeID = rdrmerchants["ID"].ToString();
                                string merchantName = rdrmerchants["Name"].ToString();
                                //string merchantIcon = rdrmerchants["IconUrl"].ToString();

                                JObject merchantType = new JObject();
                                JArray merchantTypearray = new JArray();

                                string constrmerchantType = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;
                                string sqlmerchantType = "";
                                if (!String.IsNullOrEmpty(catid))
                                {
                                    sqlmerchantType = "select * from MNMerchantCategory (NOLOCK) where ID='" + catid + "' and Type = '" + merchantsTypeID + "'";
                                }
                                else
                                {
                                    sqlmerchantType = "select * from MNMerchantCategory (NOLOCK) where Type = '" + merchantsTypeID + "'";
                                }

                                try
                                {
                                    using (System.Data.SqlClient.SqlConnection conmerchantsType = new System.Data.SqlClient.SqlConnection(constrmerchantType))
                                    {
                                        conmerchantsType.Open();
                                        using (System.Data.SqlClient.SqlCommand commandmerchantType = new System.Data.SqlClient.SqlCommand(sqlmerchantType, conmerchantsType))
                                        using (System.Data.SqlClient.SqlDataReader rdrmerchantType = commandmerchantType.ExecuteReader())
                                        {
                                            while (rdrmerchantType.Read())
                                            {
                                                string categoryid = rdrmerchantType["ID"].ToString();
                                                JObject category = new JObject();
                                                string constrout = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;
                                                string sqlouter = "select * from MNMerchantCategory (NOLOCK) where ID = '" + categoryid + "'";
                                                try
                                                {
                                                    using (System.Data.SqlClient.SqlConnection conout = new System.Data.SqlClient.SqlConnection(constrout))
                                                    {
                                                        conout.Open();
                                                        using (System.Data.SqlClient.SqlCommand commandout = new System.Data.SqlClient.SqlCommand(sqlouter, conout))
                                                        using (System.Data.SqlClient.SqlDataReader rdrout = commandout.ExecuteReader())
                                                        {
                                                            while (rdrout.Read())
                                                            {
                                                                category["catid"] = categoryid;
                                                                category["catname"] = rdrout["Name"].ToString();
                                                                category["IconUrl"] = rdrout["IconUrl"].ToString();

                                                                string constr1 = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;
                                                                string sql1 = "select * from MNMerchants (NOLOCK) where catid = '" + categoryid + "'";
                                                                try
                                                                {
                                                                    using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(constr1))
                                                                    {
                                                                        con.Open();
                                                                        JArray categoryarray = new JArray();
                                                                        using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql1, con))
                                                                        using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                                                                            while (rdr.Read())
                                                                            {
                                                                                string denomiation = rdr["denomination"].ToString();
                                                                                JObject o = new JObject();
                                                                                o["mid"] = rdr["mid"].ToString(); ;
                                                                                o["mname"] = rdr["mname"].ToString();
                                                                                o["formtype"] = rdr["formtype"].ToString();
                                                                                o["dynamiclabel"] = rdr["dynamiclabel"].ToString();
                                                                                o["amount"] = rdr["amount"].ToString();
                                                                                o["IconUrl"] = rdr["IconUrl"].ToString();

                                                                                if (denomiation != "")
                                                                                {
                                                                                    JArray array = new JArray();
                                                                                    string[] denomiationSplit = denomiation.Split(',');
                                                                                    for (int i = 0; i < denomiationSplit.Length; i++)
                                                                                    {
                                                                                        array.Add(int.Parse(denomiationSplit[i]));
                                                                                    }
                                                                                    o["denomiation"] = array;
                                                                                }
                                                                                categoryarray.Add(o);
                                                                            }
                                                                        category["merchants"] = categoryarray;

                                                                    }
                                                                }
                                                                catch (Exception ex)
                                                                {

                                                                }
                                                                merchantTypearray.Add(category);
                                                            }
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {

                                                }

                                            }

                                        }
                                    }
                                }
                                catch (Exception ex)
                                {

                                }

                                merchantsarray.Add(merchantName, merchantTypearray);
                            }

                        }
                    }
                }
            }

            catch (Exception Ex)
            {
                string aa = Ex.ToString();
            }

            return JsonConvert.SerializeObject(merchantsarray, Formatting.Indented);
        }

        public string PassVidToGetMerchantDetail(string VendorId)
        {
            string merchants = mnMerchantsRepository.GetMerchantInfo(VendorId);
            if (merchants == null || merchants == "")
            {
                return "";
            }
            else
            {
                return merchants;
            }
        }

        public string PassVIdToGetMerchantName(string vid)
        {
            string getMerchantName = "select * from MNMerchants (NOLOCK) where mid= @mid";
            string mname = "";
            using (System.Data.SqlClient.SqlConnection conmerchants = new System.Data.SqlClient.SqlConnection(constrmerchants))
            {
                conmerchants.Open();
                using (System.Data.SqlClient.SqlCommand commandMerchants = new System.Data.SqlClient.SqlCommand(getMerchantName, conmerchants))
                {
                    commandMerchants.Parameters.Add("mid", vid);
                    using (System.Data.SqlClient.SqlDataReader rdrmerchants = commandMerchants.ExecuteReader())
                    {
                        while (rdrmerchants.Read())
                        {
                            mname = rdrmerchants["mname"].ToString();

                        }
                    }
                }
            }

            return mname;
        }




    }
}
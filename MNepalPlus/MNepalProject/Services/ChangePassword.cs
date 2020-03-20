using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using MNepalProject.DAL;

namespace MNepalProject.Services
{
    public class ChangePassword : ReplyMessage
    {
        string PasswordChanged = "";

        public string PasswordChange(string mobile, string opwd, string npwd)
        {
            var dataContext = new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection());

            try
            {
                MNClientExt mn = new MNClientExt();
                mn.UserName = mobile;
                ClientsDetails clientdetails = new ClientsDetails(mn);
                if (clientdetails.client != null && clientdetails.clientContact != null)
                {
                    if (clientdetails.clientExt.Password == opwd && clientdetails.client.Status == "Active")
                    {
                        dataContext.Update<MNClientExt>("SET Password=@0 WHERE ClientCode=@1 and Password=@2", npwd, clientdetails.client.ClientCode, opwd);
                        PasswordChanged = "true";
                    }
                    else
                    {
                        PasswordChanged = "false";
                    }
                }
                else
                {
                    PasswordChanged = "false";
                }
            }
            catch (Exception ex)
            {
                PasswordChanged = "false";
            }
            return PasswordChanged;
        }


        public string PasswordForgot(string mobile, string npwd)
        {
            var dataContext = new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection());

            try
            {
                MNClientExt mn = new MNClientExt();
                mn.UserName = mobile;
                ClientsDetails clientdetails = new ClientsDetails(mn);
                if (clientdetails.client != null && clientdetails.clientContact != null)
                {
                    if (clientdetails.client.Status == "Active")
                    {
                        dataContext.Update<MNClientExt>("SET Password=@0 WHERE ClientCode=@1 ", npwd, clientdetails.client.ClientCode);
                        PasswordChanged = "true";
                    }
                    else
                    {
                        PasswordChanged = "false";
                    }
                }
                else
                {
                    PasswordChanged = "false";
                }
            }
            catch (Exception ex)
            {
                PasswordChanged = "false";
            }
            return PasswordChanged;
        }

        public string PasswordReset(string mobile, string npwd)
        {
            var dataContext = new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection());

            try
            {
                MNClientExt mn = new MNClientExt();
                mn.UserName = mobile;
                ClientsDetails clientdetails = new ClientsDetails(mn);
                if (clientdetails.client != null && clientdetails.clientContact != null)
                {
                    if (clientdetails.client.Status == "Active")
                    {
                        dataContext.Update<MNClientExt>("SET Password=@0 WHERE ClientCode=@1 ", npwd, clientdetails.client.ClientCode);
                        dataContext.Update<MNPinLog>("SET PassChanged='T' WHERE ClientCode=@0 ", clientdetails.client.ClientCode);
                        PasswordChanged = "true";
                    }
                    else
                    {
                        PasswordChanged = "false";
                    }
                }
                else
                {
                    PasswordChanged = "false";
                }
            }
            catch (Exception ex)
            {
                PasswordChanged = "false";
            }
            return PasswordChanged;
        }

    }
}
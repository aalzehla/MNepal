using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace MNSuperadmin.Utilities
{
    public class EMailUtil
    {
        public void SendMail(string DestinationAddress, string Subject, string Message) //Single Mail
        {
            try {


                if (string.IsNullOrEmpty(DestinationAddress))
                {
                    return;
                }

                if (string.IsNullOrEmpty(Subject))
                {
                    return;
                }
                if (string.IsNullOrEmpty(Message))
                {
                    return;
                }
                using (SmtpClient client = new SmtpClient())
                {
                    MailMessage mail = new MailMessage("donotreply@mnepal.com", DestinationAddress);
                    client.Port = 25;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Host = "smtp.mos.com.np";
                    mail.Subject = Subject;
                    mail.Body = Message;
                    mail.IsBodyHtml = true;
                    try {
                        client.Send(mail);
                    }
                    catch(SmtpException exception)
                    {
                       // throw new Exception(exception.Message);
                    }
                }
            }
            catch(Exception ex)
            {
               
            }
              
           
        
        }


        //public void SendMail(List<string> DestinationAddress, string Subject, string Message) //bulk Message
        //{
           
        //    try
        //    {
        //        if (DestinationAddress.Count <= 0)
        //        {
        //            return;
        //        }
        //        if (string.IsNullOrEmpty(Subject))
        //        {
        //            return;

        //        }
        //        if (string.IsNullOrEmpty(Message))
        //        {
        //            return;

        //        }
        //        using (SmtpClient client = new SmtpClient())
        //        {
        //            MailMessage mail = new MailMessage();
                    
        //            mail.From = new MailAddress("donotreply@mnepal.com.", "MNepal");
        //            foreach (string address in DestinationAddress)
        //            {
        //                mail.To.Add(address);
        //            }
                    
        //            client.Port = 25;
        //            client.DeliveryMethod = SmtpDeliveryMethod.Network;
        //            client.UseDefaultCredentials = false;
        //            client.Host = "smtp.mos.com.np";
        //            mail.Subject = Subject;
        //            mail.Body = Message;
        //            mail.IsBodyHtml = true;
        //            try
        //            {
        //                client.Send(mail);
        //            }
        //            catch (SmtpException exception)
        //            {
        //                throw new Exception(exception.Message);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }



        //}

    }
}
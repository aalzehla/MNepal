using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace MNepalWeb.Utilities
{
    public class SMSUtils
    {
        public void SendSMS(string Message,string mobile)
        {
            string messagereply = Message;
            var client = new WebClient();

            if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
            {
                //FOR NCELL
                var content = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                + "977" + mobile + "&Text=" + messagereply + "");
            }
            else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                || (mobile.Substring(0, 3) == "986"))
            {
                //FOR NTC
                var content = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                    + "977" + mobile + "&Text=" + messagereply + "");
            }
        }


     
    }
}
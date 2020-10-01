using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalAPI.Models
{
    public class Notifications
    {
        public class Notificationsobject
        {
            public string to { get; set; }
            public string imageUrl { get; set; }
            public Data data { get; set; }
            public Notification notification { get; set; }
        }

        public class Data
        {
            public string extra_information { get; set; }
            public string redirectUrl { get; set; }
        }

        public class Notification
        {
            public string title { get; set; }
            public string text { get; set; }
            public string click_action { get; set; }

        }

        public class Response
        {
            public string message_id { get; set; }
        }

        public class NotificationModel
        {
            public string imageName { get; set; }
            public string title { get; set; }
            public string text { get; set; }
            public string messageId { get; set; }
            public string redirectUrl { get; set; }
            public DateTime pushNotificationDate { get; set; }
        }

        public class BulkSMSModel
        {
            public string customerNumber { get; set; }
            public string message { get; set; }
            public DateTime smsDateTime { get; set; }

        }
        public class RootObject
        {
            public List<BulkSMSModel> notificationsList { get; set; }
        }
    }
}
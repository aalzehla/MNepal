using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Models
{
    public class Notifications
    {
        public class Notificationsobject
        {
            public string to { get; set; }
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
        }

        public class NotificationModel
        {
            public string title { get; set; }
            public string text { get; set; }
            public string NotificationsImage { get; set; }
            public string imageName { get; set; }
            public string pushNotificationDate { get; set; }
            public string messageId { get; set; }
            public string redirectUrl { get; set; }
            public string CustomerNumber { get; set; }
            public string Message { get; set; }

        }

        public class RootObject
        {
            public List<NotificationModel> notificationsList { get; set; }
        }

    }
}
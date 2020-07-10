using MNepalAPI.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static MNepalAPI.Models.Notifications;

namespace MNepalAPI.Utilities
{
    public class NotificationUtilities
    {
        public static int Notification(NotificationModel resNotificationInfo)
        {
            var objresNotificationModel = new NotificationUserModel();
            var objresPaypointNotificationInfo = new NotificationModel
            {

                extraInformation = resNotificationInfo.extraInformation,
                title = resNotificationInfo.title,
                text = resNotificationInfo.text,
                messageId = resNotificationInfo.messageId


            };
            return objresNotificationModel.ResponseNotificationInfo(objresPaypointNotificationInfo);
        }
    }
}
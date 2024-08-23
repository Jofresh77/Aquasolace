using System;
using Code.Scripts.Enums;
using UnityEngine;

namespace Code.Scripts.UI.Notification
{
    public static class Notification
    {
        public static NotificationWindow Create(NotificationType notificationType, string text, float lifetimeInSeconds = 3.0f, Texture2D image = null)
        {
            // check if image needs to be set and is set properly
            if ((notificationType == NotificationType.Biome || notificationType == NotificationType.Event) &&
                !image)
            {
                throw new Exception("ERROR: an image should be set for this type of event (event type: " + notificationType + ")");
            }

            var notificationWindow = new NotificationWindow()
                .SetMsgText(text)
                .SetLifetimeInSeconds(lifetimeInSeconds);

            switch (notificationType)
            {
                case NotificationType.Event:
                    notificationWindow.ShowImage(true);
                    notificationWindow.SetImage(image);
                    break;
                case NotificationType.Restriction:
                    notificationWindow.IsRestriction();
                    notificationWindow.ShowImage(false);
                    break;
                case NotificationType.QuestAchievement:
                    notificationWindow.ShowImage(true);
                    notificationWindow.IsAchievement();
                    notificationWindow.SetImage(image);
                    break;
                case NotificationType.QuestFailure:
                    notificationWindow.ShowImage(true);
                    notificationWindow.IsRestriction();
                    notificationWindow.SetImage(image);
                    break;
                default:
                    notificationWindow.ShowImage(false);
                    break;
            }

            notificationWindow.Build();
            
            return notificationWindow;
        }
    }
}
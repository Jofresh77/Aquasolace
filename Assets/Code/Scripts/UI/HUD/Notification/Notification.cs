using System;
using Code.Scripts.Enums;
using Code.Scripts.Singletons;
using UnityEngine;

namespace Code.Scripts.UI.HUD.Notification
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
                    SoundManager.Instance.PlaySound(SoundType.NotificationEvent);
                    
                    notificationWindow.ShowImage(true);
                    notificationWindow.SetImage(image);
                    break;
                case NotificationType.Restriction:
                    SoundManager.Instance.PlaySound(SoundType.NotificationRestriction);
                    
                    notificationWindow.Restriction();
                    notificationWindow.ShowImage(false);
                    break;
                case NotificationType.QuestAchievement:
                    SoundManager.Instance.PlaySound(SoundType.NotificationAchievement);
                    
                    notificationWindow.ShowImage(true);
                    notificationWindow.Achievement();
                    notificationWindow.SetImage(image);
                    break;
                case NotificationType.QuestFailure:
                    SoundManager.Instance.PlaySound(SoundType.NotificationFailure);
                    
                    notificationWindow.ShowImage(true);
                    notificationWindow.Restriction();
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
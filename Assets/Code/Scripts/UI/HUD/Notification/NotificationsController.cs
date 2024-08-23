using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.UI.HUD.Notification
{
    public class NotificationsController : MonoBehaviour
    {
        public bool useQueueSystem;
        public int maxNumbersOfShownNotifications = 4;
        private VisualElement _notificationsList;
        private Queue<NotificationWindow> _notificationsQueue;
        
        void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _notificationsList = root.Q<VisualElement>("NotificationsList");
            _notificationsQueue = new Queue<NotificationWindow>();

            if (useQueueSystem)
            {
                StartCoroutine(WorkOffQueue());
            }
        }

        public void AddNotification(NotificationWindow notificationWindow)
        {
            if (useQueueSystem)
            {
                _notificationsQueue.Enqueue(notificationWindow);
            }
            else
            {
                _notificationsList.Add(notificationWindow);
                StartCoroutine(FadeOutAndDestroyAfterTime(notificationWindow));
            }
            
        }

        private IEnumerator WorkOffQueue()
        {
            while (true)
            {
                // if we have no notifications in the queue or our list currently contains the max number to be shown -> continue
                if (_notificationsQueue.Count > 0 && _notificationsList.childCount < maxNumbersOfShownNotifications)
                {
                    var notificationWindow = _notificationsQueue.Dequeue();
                    _notificationsList.Add(notificationWindow);
                    StartCoroutine(FadeOutAndDestroyAfterTime(notificationWindow));
                }

                yield return null;
            }
        }

        private IEnumerator FadeOutAndDestroyAfterTime(NotificationWindow notificationWindow)
        {
            // wait x seconds before the oldest notification is being deleted
            yield return new WaitForSeconds(notificationWindow.GetLifeTimeInSeconds());
            
            // slowly fade out the notification
            notificationWindow.AddToClassList("fade-out");
            yield return new WaitForSeconds(1.1f);

            // remove the notification
            _notificationsList.Remove(notificationWindow);
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace Code.Scripts.UI
{
    public class TutorialUI : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDoc;
        [SerializeField] private UIDocument[] otherDocs;

        [SerializeField] private VideoPlayer videoIdle;
        [SerializeField] private VideoPlayer videoQuest;
        [SerializeField] private VideoPlayer videoInteraction;
        [SerializeField] private VideoPlayer videoCondition;

        private VisualElement _videoBackground;

        private Label _textWelcome;

        private void Start()
        {
            GameManager.Instance.IsGameInTutorial = true;
            GameManager.Instance.SetIsGamePaused(true);

            foreach (UIDocument doc in otherDocs)
            {
                doc.rootVisualElement.style.display = DisplayStyle.None;
            }
            
            VisualElement root = uiDoc.rootVisualElement;

            _videoBackground = root.Q<VisualElement>("VideoIdle");
            //_videoBackground.AddToClassList("video-idle-de");

            videoIdle.Prepare();
            videoIdle.Stop();
            videoIdle.Play();
        }
    }
}

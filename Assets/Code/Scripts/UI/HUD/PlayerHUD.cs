using System;
using Code.Scripts.Managers;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.UI.HUD
{
    [Obsolete("This script doesn't seem to be used anywhere.")]
    public class PlayerHUD : MonoBehaviour
    {
        [SerializeField] private UIDocument document;

        private Label timer;

        private void Start()
        {
            VisualElement root = document.rootVisualElement;

            timer = root.Q<Label>("Time");
        }

        private void Update()
        {
            if (!GameManager.Instance || !GameManager.Instance.IsGameStarted) return;

            // timer.text = TimeManager.instance.GetElapsedTime();
        }
    }
}

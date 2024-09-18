using Code.Scripts.Singletons;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace Code.Scripts.UI.GameEnd
{
    public class GameStateUI : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private UIDocument gameHud;
        [SerializeField] private UIDocument questUI;

        [SerializeField] private Canvas emailCollectorUI;

        [SerializeField] private EmailCollector emailCollector;

        private VisualElement _root;

        private void Start()
        {
            _root = document.rootVisualElement;
            _root.style.display = DisplayStyle.None;
            Generate();
        }

        private void Generate()
        {
            _root.Q<Button>("ContinueRestart").clicked += ContinueOrRestart;
            _root.Q<Button>("BackTo").clicked += BackTo;
        }

        private void ContinueOrRestart()
        {
            emailCollector.state = GameManager.Instance.IsGameWon
                ? EmailCollector.GameState.Continue
                : EmailCollector.GameState.Restart;

            HideUiAndShowEmailCollection();
        }

        private void BackTo()
        {
            emailCollector.state = EmailCollector.GameState.Back;

            HideUiAndShowEmailCollection();
        }

        private void HideUiAndShowEmailCollection()
        {
            _root.style.display = DisplayStyle.None;
            emailCollectorUI.enabled = true;
        }

        public void DisplayGameEnd(bool gwlLoss = true)
        {
            GameManager.Instance.IsGameEndStateOpen = true;
            
            gameHud.rootVisualElement.style.display = DisplayStyle.None;
            questUI.rootVisualElement.style.display = DisplayStyle.None;

            Label stateLabel = _root.Q<Label>("State");
            Label infoLabel = _root.Q<Label>("Info");

            Button continueOrRestartBtn = _root.Q<Button>("ContinueRestart");
            Button backToBtn = _root.Q<Button>("BackTo");

            backToBtn.text =
                LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "game_state_main_menu_btn");


            if (GameManager.Instance.IsGameWon)
            {
                // only possible to win by gwl
                stateLabel.text =
                    LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "game_state_won_headline");
                infoLabel.text =
                    LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "game_state_won_text");

                continueOrRestartBtn.text =
                    LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "game_state_continue_btn");
            }
            else
            {
                // headline is the same for gwl and temp
                stateLabel.text =
                    LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "game_state_lost_headline");

                infoLabel.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable",
                    gwlLoss ? "game_state_lost_gwl_text" : "game_state_lost_temp_text");

                continueOrRestartBtn.text =
                    LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "game_state_restart_btn");
                
                continueOrRestartBtn.style.backgroundColor = Color.red;
            }

            _root.style.display = DisplayStyle.Flex;
        }
    }
}
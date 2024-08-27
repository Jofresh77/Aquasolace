using System;
using System.Collections;
using System.Net.Mail;
using Code.Scripts.QuestSystem;
using Code.Scripts.Singletons;
using Code.Scripts.Tile;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Code.Scripts.UI.GameEnd
{
    public class EmailCollector : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private TMPro.TextMeshProUGUI txtData;
        [SerializeField] private UnityEngine.UI.Button btnSubmit;
        [SerializeField] private UnityEngine.UI.Button btnSkip;
        [SerializeField] private TMPro.TextMeshProUGUI invalidText;
        [SerializeField] private TMPro.TextMeshProUGUI tocText;
        [SerializeField] private UnityEngine.UI.Toggle checkbox;

        [SerializeField] private UIDocument gameHudUI;
        [SerializeField] private UIDocument questUI;

        // private float fadeOutDuration = 5f; // Adjust the duration of the fade-out

        //private const string kReceiverEmailAddress = "me@gmail.com";

        private const string kGFormBaseURL =
            "https://docs.google.com/forms/d/e/1FAIpQLSeamoa9Y5zvGoUJiwtSXiqSzpUcH1KTvehMPr9bSVfGFRoDRw/";

        private const string kGFormEntryID = "entry.745963594";

        public GameState state;

        public enum GameState
        {
            Restart,
            Continue,
            Back
        }

        // Start is called before the first frame update
        private void Start()
        {
            canvas.enabled = false;
            // Ensure the Text component is assigned
            if (invalidText == null)
            {
                Debug.LogError("Text component not assigned.");
                enabled = false; // Disable the script if Text component is missing
            }

            // Ensure the Toggle component is assigned
            if (checkbox == null)
            {
                Debug.LogError("Toggle component not assigned.");
                enabled = false; // Disable the script if Toggle component is missing
            }

            UnityEngine.Assertions.Assert.IsNotNull(txtData);
            UnityEngine.Assertions.Assert.IsNotNull(btnSubmit);
            UnityEngine.Assertions.Assert.IsNotNull(btnSkip);
            btnSubmit.onClick.AddListener(ValidateEmail);
            btnSkip.onClick.AddListener(delegate { StateCycle(state); });
        }

        private void ValidateEmail()
        {
            // string email = emailInputField.text;
            Debug.Log("Attempting to validate email: " + txtData.text);
            string emailAddress = txtData.text;
            if (IsValid(emailAddress) && IsValidRegex(emailAddress))
            {
                Debug.Log("Email is valid");
                Debug.Log("Checkbox status before validation: " + checkbox.isOn);
                if (!checkbox.isOn)
                {
                    tocText.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "email_toc_invalid");
                    //StartFadeOut();
                    Debug.Log("Not agreeing the TOC");
                }
                else
                {
                    tocText.text = "";
                    invalidText.enabled = false;
                    StartCoroutine(SendGFormData(txtData.text));
                    StateCycle(state);
                    Debug.Log("Response sent");
                }

                invalidText.text = "";
                Debug.Log("Checkbox status after validation: " + checkbox.isOn);
            }
            else
            {
                invalidText.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "email_input_invalid");
                Debug.Log("Invalid email");
            }
        }

        private static IEnumerator SendGFormData<T>(T dataContainer)
        {
            bool isString = dataContainer is string;
            string jsonData = isString ? dataContainer.ToString() : JsonUtility.ToJson(dataContainer);

            WWWForm form = new WWWForm();
            form.AddField(kGFormEntryID, jsonData);
            string urlGFormResponse = kGFormBaseURL + "formResponse";
            using (UnityWebRequest www = UnityWebRequest.Post(urlGFormResponse, form))
            {
                yield return www.SendWebRequest();
            }
        }

        // We cannot have spaces in links for iOS
        /*public static void OpenLink(string link)
        {
            bool googleSearch = link.Contains("google.com/search");
            string linkNoSpaces = link.Replace(" ", googleSearch ? "+" : "%20");
            Application.OpenURL(linkNoSpaces);
        }*/

        private bool IsValid(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private bool IsValidRegex(string email)
        {
            if (email == null)
            {
                return false;
            }

            int atSymbolPosition = email.IndexOf("@");

            //checks if the @ symbol is not found, at the start or end of the address.
            //That is, the following are not valid:
            //somewhere.com
            //@somewhere.com
            //someone@
            //someone@somewhere.com@
            if (atSymbolPosition < 1 || email.EndsWith("@"))
            {
                return false;
            }

            int periodSymbolPosition = email.IndexOf(".", atSymbolPosition);

            //checks if the period is not found, and that it's not beside the @ symbol, and it's not at the end.  
            //That is, the following are not valid:
            //someone@somewhere
            //someone@.somewhere.com
            if (periodSymbolPosition > (atSymbolPosition + 1))
            {
                return true;
            }

            // A trailing period is actually legal, but validity is defined by the mail providers.
            return false;
        }

        private void StateCycle(GameState selectedState)
        {
            GameManager.Instance.SetIsGamePaused(false);
            GameManager.Instance.IsGameContinue = true;
            
            switch (selectedState)
            {
                case GameState.Continue:
                    canvas.enabled = false;
                    gameHudUI.rootVisualElement.style.display = DisplayStyle.Flex;
                    questUI.rootVisualElement.style.display = DisplayStyle.Flex;
                    GameManager.Instance.IsGameEndStateOpened = false;
                    break;
                case GameState.Restart:
                    CleanUpLevel();
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    break;
                case GameState.Back:
                    CleanUpLevel();
                    SceneManager.LoadScene("MainMenu");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(selectedState), selectedState, null);
            }
        }

        private void CleanUpLevel()
        {
            Destroy(TileHelper.Instance);
            Destroy(GridHelper.Instance);
            Destroy(QuestManager.Instance);
            Destroy(TimeManager.Instance);
            Destroy(SealedAreaManager.Instance);
            Destroy(GameManager.Instance);
        }

        /*public void StartFadeOut()
        {
            StartCoroutine(FadeOut());
        }

        private IEnumerator FadeOut()
        {
            // Ensure the Text component is assigned
            if (invalidText != null)
            {
                Color startColor = invalidText.color;
                float elapsedTime = 0f;

                while (elapsedTime < fadeOutDuration)
                {
                    // Calculate the new alpha value using Lerp
                    float newAlpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / fadeOutDuration);

                    // Set the text color with the new alpha value
                    invalidText.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);

                    // Increment the elapsed time
                    elapsedTime += Time.deltaTime;

                    // Yield until the next frame
                    yield return null;
                }

                // Ensure the final alpha is set to 0
                invalidText.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
            }
        }*/

        public void Update()
        {
            if (invalidText.text !=
                LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "email_input_invalid") && invalidText.text != "")
            {
                invalidText.text =
                    LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "email_input_invalid");
            }
            
            if (tocText.text !=
                LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "email_toc_invalid") && tocText.text != "")
            {
                tocText.text =
                    LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "email_toc_invalid");
            }
        }
    }
}
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Code.Scripts.Managers
{
    public class LanguageManager : MonoBehaviour
    {
        #region Properties
        
        public static LanguageManager Instance;

        private Locale _currentLocale;

        #endregion

        private void Awake()
        {
            Instance = this;

            _currentLocale = LocalizationSettings.SelectedLocale;
        }
        
        #region Getter and Setter
        
        public Locale GetCurrentLocale()
        {
            return _currentLocale;
        }

        public void SetCurrentLocale(Locale locale)
        {
            _currentLocale = locale;
        }
        
        #endregion
    }
}
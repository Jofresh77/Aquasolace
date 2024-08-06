using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Code.Scripts.UI
{
    public class LocaleSelector : MonoBehaviour
    {
        private bool _active = false;

        public void ChangeLocale(Locale locale)
        {
            if (_active) return;
            StartCoroutine(SetLocale(locale));
        }

        IEnumerator SetLocale(Locale locale)
        {
            _active = true;
            yield return LocalizationSettings.InitializationOperation;
            var localesId = LocalizationSettings.AvailableLocales.Locales.IndexOf(locale);
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localesId];
            _active = false;
        }
    }
}
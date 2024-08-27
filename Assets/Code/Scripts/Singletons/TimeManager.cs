using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;

namespace Code.Scripts.Singletons
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        private float _elapsedTime;
        [SerializeField] private float secondsPerMonth = 12.73f; // With 08/2024 -> 05/2027 you need this value for 7min and 18.18f for 10min
        private readonly DateTime _startDate = new(2024, 8, 1);
        private readonly DateTime _endDate = new(2027, 5, 1);
        public DateTime CurrentDate { get; private set; }

        [Serializable]
        public class UpdateMethodCall
        {
            public int id;
            public DateTime NextUpdate;
            public int monthInterval;
            public UnityEvent methodToCall;
        }

        [SerializeField] private List<UpdateMethodCall> updateMethodCallList = new();
        [SerializeField] private UnityEvent endMethodCall = new();

        private void Awake()
        {
            Instance = this;
            CurrentDate = _startDate;

            foreach (var method in updateMethodCallList)
                method.NextUpdate = CurrentDate.AddMonths(method.monthInterval);

            StartCoroutine(UpdateHabitat());
            StartCoroutine(UpdateDateTime());
        }

        private IEnumerator UpdateHabitat()
        {
            while (true)
            {
                yield return new WaitForSeconds(secondsPerMonth / 2.5f);
                
                foreach (var method in updateMethodCallList)
                {
                    if (method.id != 0 || CurrentDate != method.NextUpdate) continue;
                    
                    method.NextUpdate = CurrentDate.AddMonths(method.monthInterval);
                    method.methodToCall?.Invoke();
                }
            }
        }

        private IEnumerator UpdateDateTime()
        {
            while (true)
            {
                yield return new WaitForSeconds(secondsPerMonth);
                IncreaseMonth();

                // call the method to expand the sealed area & the method to check if species in habitat still fulfill the condition to live
                foreach (var method in updateMethodCallList)
                {
                    if (method.id != 1 || CurrentDate != method.NextUpdate) continue;
                    
                    method.NextUpdate = CurrentDate.AddMonths(method.monthInterval);
                    method.methodToCall?.Invoke();
                }

                if (IsEndDateReached())
                    endMethodCall?.Invoke();
            }
        }

        private void IncreaseMonth()
        {
            CurrentDate = CurrentDate.AddMonths(1);
        }

        private bool IsEndDateReached()
        {
            return CurrentDate.Equals(_endDate);
        }

        public string FormatDateTime(DateTime dateTime, string format)
        {
            var currentLocale = LocalizationSettings.SelectedLocale;
            if (currentLocale != null)
            {
                var cultureInfo = new CultureInfo(currentLocale.Identifier.Code);
                return dateTime.ToString(format, cultureInfo);
            }

            return dateTime.ToString(format);
        }
    }
}

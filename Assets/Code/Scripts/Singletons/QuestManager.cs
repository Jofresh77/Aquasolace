using System.Collections.Generic;
using System.Linq;
using Code.Scripts.QuestSystem;
using UnityEngine;

namespace Code.Scripts.Singletons
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        public QuestList questList;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDisable()
        {
            questList = null;
        }

        public void CheckQuestList()
        {
            QuestBoard.Instance.CheckAchievement();
        }

        public bool AreRequiredQuestsAchieved()
        {
            foreach (Quest questSo in questList.quests)
            {
                if (questSo is Quest { isEndRequired: true, IsAchieved: false })
                    
                    return false;
            }

            return true;
        }
        
        public List<ReviveSpecies> GetReviveSpeciesQuests()
        {
            return questList.quests.OfType<ReviveSpecies>().ToList();
        }
    }
}
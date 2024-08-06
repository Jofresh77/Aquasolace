using System.Collections.Generic;
using UnityEngine;

namespace Code.Scripts.QuestSystem
{
    [CreateAssetMenu(fileName = "QuestList", menuName = "System Quest/Quest List", order = 1)]
    public class QuestList : ScriptableObject
    {
        public List<Quest> quests = new();
    }
}
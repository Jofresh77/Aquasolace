using System.Collections.Generic;
using Code.Scripts.QuestSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.UI.QuestUI
{
    public class QuestLogScrollController : MonoBehaviour
    {
        private ListView _questLogList;
        private List<QuestBoard.QuestInfo> _questLogEntries;
        
        private void Awake()
        {
            _questLogEntries = QuestBoard.Instance.GetQuestInfoList();
            
            var root = GetComponent<UIDocument>().rootVisualElement;
            _questLogList = root.Q<ListView>("ScrollLogList");

            _questLogList.itemsSource = _questLogEntries;
            _questLogList.makeItem = MakeItem;
            _questLogList.bindItem = BindItem;
            _questLogList.selectionType = SelectionType.None;
            
            // used to set the item height after the listview is built
            _questLogList.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            _questLogList.Rebuild();
        }


        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            float listViewHeight = _questLogList.resolvedStyle.height;
            Debug.Log("Die aktuelle Höhe der ListView ist: " + listViewHeight);

            if (!float.IsNaN(listViewHeight) && listViewHeight > 0)
            {
                // setting the fixed item height to half the height of the listview
                _questLogList.fixedItemHeight = listViewHeight / 2;
                Debug.Log("Die fixedItemHeight der ListView wurde auf: " + _questLogList.fixedItemHeight + " gesetzt");
                
                // unregister after the first time setting the height
                _questLogList.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                _questLogList.Rebuild();
            }
        }
        
        private VisualElement MakeItem()
        {
            return new QuestLogEntry();
        }

        private void BindItem(VisualElement element, int index)
        {
            var questLogEntry = element as QuestLogEntry;
            var item = _questLogEntries[index];

            questLogEntry?.SetName(item.questName)
                .SetDescription(item.description)
                .SetCountCurrent(0)
                .SetCountToReach(50)
                .SetRewardLabel("100")
                .SetTipLabel("This is a temporary tip text.")
                .Build();
        }
    }
}
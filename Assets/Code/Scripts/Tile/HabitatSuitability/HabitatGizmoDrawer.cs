using System.Collections.Generic;
using Code.Scripts.QuestSystem;
using Code.Scripts.Singletons;
using UnityEngine;

namespace Code.Scripts.Tile.HabitatSuitability
{
    public class HabitatGizmoDrawer : MonoBehaviour
    {
        public ReviveSpecies reviveSpeciesQuest;
        public GridHelper gridHelper;

        private List<Transform> _sealedBorders = new ();
        public Color gizmoColor = Color.red;
        public float gizmoRadius = 0.5f;

        private void OnDrawGizmos()
        {
            if (reviveSpeciesQuest != null)
            {
                reviveSpeciesQuest.DrawHabitatGizmos();
            }

            if (gridHelper != null)
            {
                gridHelper.DrawSealedBorderGizmos();
            }
        }
    }
}

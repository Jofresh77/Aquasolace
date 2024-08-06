using Code.Scripts.QuestSystem;
using UnityEngine;

namespace Code.Scripts.Tile.HabitatSuitability
{
    public class HabitatGizmoDrawer : MonoBehaviour
    {
        public ReviveSpecies reviveSpeciesQuest;

        private void OnDrawGizmos()
        {
            if (reviveSpeciesQuest != null)
            {
                reviveSpeciesQuest.DrawHabitatGizmos();
            }
        }
    }
}

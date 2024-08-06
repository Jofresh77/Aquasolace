using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Scripts.Tile
{
    public class TileRandomEnable : MonoBehaviour
    {
        private void OnEnable()
        {
            int randomTreeIndex = Random.Range(0, transform.GetChild(1).childCount - 1);
            transform.GetChild(1).GetChild(randomTreeIndex).gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            foreach (Transform tree in transform.GetChild(1))
            {
                tree.gameObject.SetActive(false);
            }
        }
    }
}

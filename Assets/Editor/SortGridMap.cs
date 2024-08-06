using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Editor
{
    public class SortGridMap
    {
        [MenuItem("Tools/SortGridMap")]
        private static void Sort()
        {
            Transform tileMap = Selection.activeGameObject.transform;

            Transform[] children = new Transform[tileMap.childCount];
            for (int i = 0; i < children.Length; i++)
            {
                children[i] = tileMap.GetChild(i);
            }
            
            System.Array.Sort(children, (a, b) => ComparePositions(a.position, b.position));

            for (int i = 0; i < children.Length; i++)
            {
                children[i].SetSiblingIndex(i);
            }
        }

        [MenuItem("Tools/SortGridMap", true)]
        private static bool SortActionValidation()
        {
            return Selection.activeGameObject.TryGetComponent(out Tilemap tilemap);
        }

        private static int ComparePositions(Vector3 pos1, Vector3 pos2)
        {
            if (pos1.z < pos2.z)
            {
                return -1;
            }

            if (pos1.z > pos2.z)
            {
                return 1;
            }
            
            return pos1.x > pos2.x ? 1 : -1;
        }
    }
}
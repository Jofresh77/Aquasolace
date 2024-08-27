using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Editor
{
    public class FixNonPrefabTiles : EditorWindow
    {
        private Tilemap targetTilemap;
        private GameObject originalPrefab;

        [MenuItem("Tools/Fix Non-Prefab Tiles")]
        private static void ShowWindow()
        {
            GetWindow<FixNonPrefabTiles>("Fix Tilemap");
        }

        private void OnGUI()
        {
            // Tilemap Input Field
            EditorGUILayout.LabelField("Select Target Tilemap:");
            targetTilemap = (Tilemap)EditorGUILayout.ObjectField(targetTilemap, typeof(Tilemap), true);

            // Prefab Input Field
            EditorGUILayout.LabelField("Select Original Prefab:");
            originalPrefab = (GameObject)EditorGUILayout.ObjectField(originalPrefab, typeof(GameObject), false);

            // Replace Button
            if (GUILayout.Button("Fix Non-Prefab Tiles"))
            {
                if (targetTilemap != null && originalPrefab != null)
                {
                    FixTiles();
                }
                else
                {
                    Debug.LogError("Please select both a Tilemap and a Prefab.");
                }
            }

            if (GUILayout.Button("Fix missing sealed-surface"))
            {
                if (targetTilemap != null)
                {
                    FixMissingSealed();
                }
                else
                {
                    Debug.LogError("Please select a Tilemap.");
                }
            }
        }

        private void FixTiles()
        {
            Transform tilemapTransform = targetTilemap.transform;
            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();

            for (int i = tilemapTransform.childCount - 1; i >= 0; i--)
            {
                Transform child = tilemapTransform.GetChild(i);

                if (PrefabUtility.GetPrefabAssetType(child) == PrefabAssetType.Regular) continue;

                GameObject newPrefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(originalPrefab);
                if (newPrefabInstance == null)
                {
                    Debug.LogError($"Failed to instantiate prefab: {originalPrefab.name}");
                    continue;
                }

                newPrefabInstance.transform.SetParent(tilemapTransform);
                newPrefabInstance.transform.position = child.position;
                newPrefabInstance.transform.rotation = child.rotation;

                MatchChildStates(child.gameObject, newPrefabInstance);

                if (stage != null)
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(newPrefabInstance.transform);
                }

                Undo.DestroyObjectImmediate(child.gameObject);
            }
        }

        //!!!!!!!!!!!!!!!!!!!!!!!
        //!!!   NOT WORKING   !!!
        //!!!!!!!!!!!!!!!!!!!!!!!
        private void FixMissingSealed()
        {
            Transform tilemapTransform = targetTilemap.transform;
            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
            
            for (int i = tilemapTransform.childCount - 1; i >= 0; i--)
            {
                Transform child = tilemapTransform.GetChild(i);
                bool anyActive = false;

                for (int j = 0; j < child.childCount; j++)
                {
                    if (!child.GetChild(j).gameObject.activeSelf) continue;
                    
                    anyActive = true;
                    break;
                }
                
                if (anyActive) return;
                
                for (int j = 0; j < child.childCount; j++)
                {
                    Transform childChild = child.GetChild(j);
                    
                    if (childChild.CompareTag("Sealed"))
                    {
                        childChild.gameObject.SetActive(true);
                        
                        if (stage != null)
                        {
                            PrefabUtility.RecordPrefabInstancePropertyModifications(childChild);
                        }
                    }
                }
            }
        }

        private static void MatchChildStates(GameObject source, GameObject target)
        {
            Transform[] sourceChildren = source.GetComponentsInChildren<Transform>(true);
            Transform[] targetChildren = target.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < sourceChildren.Length; i++)
            {
                if (i < targetChildren.Length)
                {
                    targetChildren[i].gameObject.SetActive(sourceChildren[i].gameObject.activeSelf);
                }
            }
        }
    }
}
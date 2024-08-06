using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace Editor
{
    public class BiomeTileStateFixer : EditorWindow
    {
        private Tilemap targetTilemap;
        private Dictionary<string, GameObject> biomePrefabs = new Dictionary<string, GameObject>();

        private string[] biomeTypes = new string[]
        {
            "Deciduous", "Farmland", "Meadow", "Mixed", "Pine",
            "RiverCorner", "RiverCross", "RiverEnd", "RiverSplit", "RiverStraight", "Sealed"
        };

        [MenuItem("Tools/Biome Tile Fixer")]
        private static void ShowWindow()
        {
            GetWindow<BiomeTileStateFixer>("Biome Tile Fixer");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Select Target Tilemap:");
            targetTilemap = (Tilemap)EditorGUILayout.ObjectField(targetTilemap, typeof(Tilemap), true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Select Biome Prefabs:", EditorStyles.boldLabel);

            foreach (string biomeType in biomeTypes)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"BiomeTile_{biomeType}:");
                biomePrefabs[biomeType] = (GameObject)EditorGUILayout.ObjectField(
                    biomePrefabs.ContainsKey(biomeType) ? biomePrefabs[biomeType] : null,
                    typeof(GameObject), false
                );
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Fix Biome Tiles"))
            {
                if (targetTilemap != null && AreAllPrefabsAssigned())
                {
                    FixBiomeTiles();
                }
                else
                {
                    Debug.LogError("Please select a Tilemap and assign all Biome Prefabs.");
                }
            }
        }

        private bool AreAllPrefabsAssigned()
        {
            foreach (string biomeType in biomeTypes)
            {
                if (!biomePrefabs.ContainsKey(biomeType) || biomePrefabs[biomeType] == null)
                {
                    return false;
                }
            }

            return true;
        }

        private void FixBiomeTiles()
        {
            Transform tilemapTransform = targetTilemap.transform;
            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();

            Undo.IncrementCurrentGroup();
            int undoGroupIndex = Undo.GetCurrentGroup();

            List<GameObject> objectsToDestroy = new List<GameObject>();

            for (int i = tilemapTransform.childCount - 1; i >= 0; i--)
            {
                Transform child = tilemapTransform.GetChild(i);
                string biomeType = DetermineBiomeType(child.gameObject);

                if (string.IsNullOrEmpty(biomeType))
                {
                    Debug.LogWarning($"Could not determine biome type for {child.name}. Skipping.");
                    continue;
                }

                GameObject newPrefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(biomePrefabs[biomeType]);
                if (newPrefabInstance == null)
                {
                    Debug.LogError($"Failed to instantiate prefab: {biomePrefabs[biomeType].name}");
                    continue;
                }

                Undo.RegisterCreatedObjectUndo(newPrefabInstance, "Create new biome tile");

                newPrefabInstance.transform.SetParent(tilemapTransform);
                newPrefabInstance.transform.position = child.position;
                newPrefabInstance.transform.rotation = child.rotation;

                MatchChildStates(child.gameObject, newPrefabInstance);

                if (stage != null)
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(newPrefabInstance.transform);
                }

                objectsToDestroy.Add(child.gameObject);
            }

            foreach (GameObject obj in objectsToDestroy)
            {
                Undo.DestroyObjectImmediate(obj);
            }

            Undo.CollapseUndoOperations(undoGroupIndex);
        }

        private string DetermineBiomeType(GameObject prefabInstance)
        {
            foreach (Transform child in prefabInstance.transform)
            {
                if (child.gameObject.activeSelf)
                {
                    string tag = "";
                    if (child.CompareTag("River"))
                    {
                        foreach (Transform childChild in child.transform)
                        {
                            if (childChild.gameObject.activeSelf)
                                tag = childChild.gameObject.tag;
                        }
                    }
                    else
                    {
                        tag = child.gameObject.tag;
                    }

                    if (tag == "") continue;
                    
                    if (System.Array.IndexOf(biomeTypes, tag) != -1)
                        return tag;
                }
            }

            return null;
        }

        private static void MatchChildStates(GameObject source, GameObject target)
        {
            for (int i = 0; i < source.transform.childCount; i++)
            {
                if (i < target.transform.childCount)
                {
                    target.transform.GetChild(i).gameObject
                        .SetActive(source.transform.GetChild(i).gameObject.activeSelf);
                }
            }
        }
    }
}
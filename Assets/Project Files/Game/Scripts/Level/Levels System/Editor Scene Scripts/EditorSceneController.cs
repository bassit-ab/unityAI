#pragma warning disable 649

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    public class EditorSceneController : MonoBehaviour
    {

#if UNITY_EDITOR
        private static EditorSceneController instance;
        public static EditorSceneController Instance { get => instance; }

        [SerializeField] GameObject content;
        [SerializeField] GameObject background;
        [SerializeField] GameObject playerSpawn;

        [Header("Snap & Handles Configuration")]
        [SerializeField] float snapDistance;
        [SerializeField] float lineThickness;
        [SerializeField] float discThickness;
        [SerializeField] float discRadius;
        [SerializeField] Color handlesColor;

        private List<SavableObstacle> obstacles;
        private List<SavableBooster> boosters;
        private List<SavableGate> gates;
        private Color backupColor;
        private bool levelLoaded = false;


        public GameObject Content { get => content; set => content = value; }
        public GameObject Background { get => background; set => background = value; }
        public bool LevelLoaded { get => levelLoaded; set => levelLoaded = value; }
        public GameObject PlayerSpawn { get => playerSpawn; set => playerSpawn = value; }

        public EditorSceneController()
        {
            instance = this;
            obstacles = new List<SavableObstacle>();
            boosters = new List<SavableBooster>();
            gates = new List<SavableGate>();
            SceneView.duringSceneGui += SceneGUI;
            EditorApplication.hierarchyChanged += HierarchyChanged;
        }

        public GameObject SpawnPrefab(GameObject prefab, Vector3 position, bool isContent = true, bool isSelected = false)
        {
            GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            if (isContent)
            {
                gameObject.transform.SetParent(content.transform);
            }
            else
            {
                gameObject.transform.SetParent(background.transform);
            }
            
            gameObject.transform.position = position;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.name = prefab.name + " ( Child # " + content.transform.childCount + ")";

            if (isSelected)
            {
                Selection.activeGameObject = gameObject;
            }

            return gameObject;
        }

        public SavableGate AddGate(GameObject gameObject, string id)
        {
            SavableGate savable = gameObject.AddComponent<SavableGate>();
            savable.Id = id;
            return savable;
        }

        public SavableBooster AddBooster(GameObject gameObject, string id)
        {
            SavableBooster savable = gameObject.AddComponent<SavableBooster>();
            savable.Id = id;
            return savable;
        }

        public SavableObstacle AddObstacle(GameObject gameObject, string id)
        {
            SavableObstacle savable = gameObject.AddComponent<SavableObstacle>();
            savable.Id = id;
            return savable;
        }

        public SavableEnemy AddEnemy(GameObject gameObject, string id)
        {
            SavableEnemy savable = gameObject.AddComponent<SavableEnemy>();
            savable.Id = id;
            return savable;
        }

        public SavableRoad AddRoad(GameObject gameObject, string id)
        {
            SavableRoad savable = gameObject.AddComponent<SavableRoad>();
            savable.Id = id;
            return savable;
        }

        public SavableFinish AddFinish(GameObject gameObject, string id)
        {
            SavableFinish savable = gameObject.AddComponent<SavableFinish>();
            savable.Id = id;
            return savable;
        }

        public SavableEnvironment AddEnvironment(GameObject gameObject, string id)
        {
            SavableEnvironment savable = gameObject.AddComponent<SavableEnvironment>();
            savable.Id = id;
            return savable;
        }


        public void Clear()
        {
            obstacles.Clear();
            boosters.Clear();

            for (int i = content.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(content.transform.GetChild(i).gameObject);
            }

            for (int i = background.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(background.transform.GetChild(i).gameObject);
            }
        }

        public void ClearBackground()
        {
            for (int i = background.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(background.transform.GetChild(i).gameObject);
            }
        }

        private void SceneGUI(SceneView view)
        {
            try
            {
                //Update visuals
                foreach (SavableGate item in gates)
                {
                    if (item.needToUpdateVisuals)
                    {
                        item.UpdateGateVisuals();
                    }
                }

                foreach (SavableBooster item in boosters)
                {
                    if (item.needToUpdateVisuals)
                    {
                        item.UpdateBoosterVisuals();
                    }
                }


                //handle snap
                if (Selection.activeGameObject != null)
                {
                    SavableBooster booster = Selection.activeGameObject.GetComponent<SavableBooster>();

                    if (booster != null)
                    {
                        Vector2 boosterVector = new Vector2(booster.Position.x, booster.Position.z);
                        Vector2 obstacleVector = Vector2.zero;
                        float distance;
                        float minDistance = float.MaxValue;
                        int index = -1;

                        for (int i = 0; i < obstacles.Count; i++)
                        {
                            obstacleVector.Set(obstacles[i].Position.x, obstacles[i].Position.z);
                            distance = Vector2.Distance(boosterVector, obstacleVector);

                            if (Vector2.Distance(boosterVector, obstacleVector) <= snapDistance)
                            {
                                if(distance < minDistance)
                                {
                                    minDistance = distance;
                                    index = i;
                                }

                                
                            }
                        }

                        if(index != -1)
                        {
                            booster.transform.position = new Vector3(obstacles[index].Position.x, obstacles[index].BoosterHeight, obstacles[index].Position.z);
                        }
                    }
                }


                //Visualize snapped 
                backupColor = Handles.color;
                Handles.color = handlesColor;
                bool customParent;
                bool snapped;

                for (int i = 0; i < boosters.Count; i++)
                {
                    customParent = (boosters[i].transform.parent != content.transform);
                    snapped = false;

                    for (int j = 0; j < obstacles.Count; j++)
                    {
                        if (Mathf.Approximately(boosters[i].Position.x, obstacles[j].Position.x) && Mathf.Approximately(boosters[i].Position.z, obstacles[j].Position.z))
                        {
                            if (!Mathf.Approximately(boosters[i].Position.y, obstacles[j].BoosterHeight)) // handle booster height change
                            {
                                boosters[i].transform.position = boosters[i].transform.position.SetY(obstacles[j].BoosterHeight);
                            }

                            Handles.DrawLine(boosters[i].Position, obstacles[j].Position, lineThickness);
                            Handles.DrawWireDisc(obstacles[j].Position, Vector3.up, discRadius, discThickness);
                            snapped = true;

                            if (!customParent)
                            {
                                boosters[i].transform.SetParent(obstacles[j].transform);
                            }

                            break;
                        }
                    }

                    if (customParent && (!snapped))
                    {
                        boosters[i].transform.position = boosters[i].transform.position.SetY(0);
                        boosters[i].transform.SetParent(content.transform);
                    }
                }
            }
            catch
            {

            }

            Handles.color = backupColor;
        }

        public void HierarchyChanged()
        {
            try
            {
                if (levelLoaded)
                {
                    obstacles.Clear();
                    boosters.Clear();
                    gates.Clear();
                    obstacles.AddRange(content.GetComponentsInChildren<SavableObstacle>());
                    boosters.AddRange(content.GetComponentsInChildren<SavableBooster>());
                    gates.AddRange(content.GetComponentsInChildren<SavableGate>());
                    SortHierarchyByZ(content.transform);
                    SortHierarchyByZ(background.transform);
                }
            }
            catch
            {

            }
        }

        private void SortHierarchyByZ(Transform target)
        {
            float currentZ;
            float maxZ;
            int maxZIndex;

            for (int i = 0; i < target.transform.childCount - 1; i++)
            {
                currentZ = target.transform.GetChild(i).position.z;
                maxZIndex = -1;

                for (int j = i + 1; j < target.transform.childCount; j++)
                {
                    maxZ = target.transform.GetChild(j).position.z;

                    if (maxZ > currentZ)
                    {
                        currentZ = maxZ;
                        maxZIndex = j;
                    }
                }

                if (maxZIndex != -1)
                {
                    target.transform.GetChild(maxZIndex).SetSiblingIndex(i);
                }
            }
        }

        private void RecursiveCheck(GameObject activeGameObject)
        {

            if(activeGameObject == null)
            {
                return;
            }

            if(activeGameObject.GetComponent<AbstractSavable>() != null)
            {
                Selection.activeGameObject = activeGameObject;
            }
            else
            {
                Selection.activeGameObject = null;
                RecursiveCheck(activeGameObject.transform.parent.gameObject);
            }
        }

        public void FocusCameraPosition(Vector3 pos, float boundsSize)
        {
            SceneView.lastActiveSceneView.Frame(new Bounds(pos, Vector3.one * boundsSize), false);
        }

        public void Unsubscribe()
        {
            SceneView.duringSceneGui -= SceneGUI;
            EditorApplication.hierarchyChanged -= HierarchyChanged;
        }

        

        public void OnDestroy()
        {
            Unsubscribe();
        }

#endif
    }
}

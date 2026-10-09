using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarStrike.Gameplay
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        private Dictionary<string, Queue<GameObject>> poolDictionary;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                poolDictionary = new Dictionary<string, Queue<GameObject>>();
            }
        }

        public GameObject Spawn(string tag, Vector3 position, Quaternion rotation, Func<GameObject> factory)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                poolDictionary.Add(tag, new Queue<GameObject>());
            }

            Queue<GameObject> queue = poolDictionary[tag];
            GameObject objectToSpawn = null;

            if (queue.Count > 0)
            {
                // Find an inactive one (just in case active ones got enqueued somehow)
                while (queue.Count > 0)
                {
                    GameObject obj = queue.Dequeue();
                    if (obj == null) continue;
                    
                    if (!obj.activeInHierarchy)
                    {
                        objectToSpawn = obj;
                        break;
                    }
                    else
                    {
                        // Enqueue it back? No, it's round-robin, so we enqueue it at the end
                        queue.Enqueue(obj);
                        if (queue.Peek() == obj) break; // looped around completely
                    }
                }
            }

            if (objectToSpawn == null)
            {
                objectToSpawn = factory();
                objectToSpawn.transform.SetParent(transform);
            }

            if (objectToSpawn != null)
            {
                objectToSpawn.transform.position = position;
                objectToSpawn.transform.rotation = rotation;
                objectToSpawn.SetActive(true);
                queue.Enqueue(objectToSpawn);
                return objectToSpawn;
            }

            return null;
        }
    }
}

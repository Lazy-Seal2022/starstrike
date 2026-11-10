using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarStrike.Gameplay
{
    public class PoolManager : MonoBehaviour
    {
        private static PoolManager _instance;
        public static PoolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<PoolManager>();
                    if (_instance == null && Application.isPlaying)
                    {
                        var go = new GameObject("[PoolManager]");
                        _instance = go.AddComponent<PoolManager>();
                        _instance.Initialize();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        private Dictionary<string, Queue<GameObject>> poolDictionary;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            Initialize();
        }

        public void Initialize()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            if (poolDictionary == null)
            {
                poolDictionary = new Dictionary<string, Queue<GameObject>>();
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public void Prewarm(string tag, Func<GameObject> factory, int count)
        {
            if (string.IsNullOrEmpty(tag) || factory == null || count <= 0) return;

            Initialize();

            if (!poolDictionary.ContainsKey(tag))
            {
                poolDictionary.Add(tag, new Queue<GameObject>());
            }

            Queue<GameObject> queue = poolDictionary[tag];
            for (int i = 0; i < count; i++)
            {
                GameObject obj = factory();
                if (obj != null)
                {
                    obj.transform.SetParent(transform);
                    obj.SetActive(false);
                    queue.Enqueue(obj);
                }
            }
        }

        public void Despawn(GameObject obj)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        public GameObject Spawn(string tag, Vector3 position, Quaternion rotation, Func<GameObject> factory)
        {
            Initialize();

            if (!poolDictionary.ContainsKey(tag))
            {
                poolDictionary.Add(tag, new Queue<GameObject>());
            }

            Queue<GameObject> queue = poolDictionary[tag];
            GameObject objectToSpawn = null;

            if (queue.Count > 0)
            {
                int count = queue.Count;
                for (int i = 0; i < count; i++)
                {
                    GameObject obj = queue.Dequeue();
                    if (obj == null) continue;

                    if (!obj.activeInHierarchy)
                    {
                        objectToSpawn = obj;
                        break;
                    }
                    queue.Enqueue(obj);
                }
            }

            if (objectToSpawn == null)
            {
                objectToSpawn = factory();
                if (objectToSpawn != null)
                {
                    objectToSpawn.transform.SetParent(transform);
                }
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

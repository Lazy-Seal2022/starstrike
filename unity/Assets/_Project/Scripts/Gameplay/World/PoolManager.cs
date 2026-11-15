using System;
using System.Collections.Generic;
using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    /// <summary>
    /// High-performance zero-allocation object pool manager.
    /// Uses O(1) stack-based free lists for instant acquisition and recycling.
    /// Interoperates with PooledObjectTracker to guarantee automatic return to the pool
    /// even when objects are deactivated via SetActive(false).
    /// </summary>
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

        private class ObjectPool
        {
            public readonly string Tag;
            public readonly Stack<GameObject> Inactive = new Stack<GameObject>();
            public readonly HashSet<GameObject> Active = new HashSet<GameObject>();

            public ObjectPool(string tag)
            {
                Tag = tag;
            }
        }

        private readonly Dictionary<string, ObjectPool> pools = new Dictionary<string, ObjectPool>();
        private readonly Dictionary<GameObject, string> objectToTag = new Dictionary<GameObject, string>();

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
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private ObjectPool GetOrCreatePool(string tag)
        {
            if (!pools.TryGetValue(tag, out var pool))
            {
                pool = new ObjectPool(tag);
                pools.Add(tag, pool);
            }
            return pool;
        }

        public void Prewarm(string tag, Func<GameObject> factory, int count)
        {
            if (string.IsNullOrEmpty(tag) || factory == null || count <= 0) return;
            Initialize();

            var pool = GetOrCreatePool(tag);
            for (int i = 0; i < count; i++)
            {
                GameObject obj = factory();
                if (obj != null)
                {
                    var tracker = obj.GetComponent<PooledObjectTracker>();
                    if (tracker == null) tracker = obj.AddComponent<PooledObjectTracker>();
                    tracker.poolManager = this;
                    tracker.poolTag = tag;
                    tracker.isSpawned = false;

                    obj.transform.SetParent(transform);
                    obj.SetActive(false);
                    pool.Inactive.Push(obj);
                    objectToTag[obj] = tag;
                }
            }
        }

        public GameObject Spawn(string tag, Vector3 position, Quaternion rotation, Func<GameObject> factory)
        {
            Initialize();
            var pool = GetOrCreatePool(tag);
            GameObject obj = null;

            // O(1) acquisition from inactive stack
            while (pool.Inactive.Count > 0)
            {
                var candidate = pool.Inactive.Pop();
                if (candidate != null)
                {
                    obj = candidate;
                    break;
                }
            }

            // Fallback for EditMode or immediate SetActive(false) without lifecycle callbacks
            if (obj == null && pool.Active.Count > 0)
            {
                GameObject inactiveCandidate = null;
                foreach (var activeObj in pool.Active)
                {
                    if (activeObj != null && !activeObj.activeSelf)
                    {
                        inactiveCandidate = activeObj;
                        break;
                    }
                }

                if (inactiveCandidate != null)
                {
                    pool.Active.Remove(inactiveCandidate);
                    obj = inactiveCandidate;
                }
            }

            if (obj == null)
            {
                if (factory != null)
                {
                    obj = factory();
                }
                if (obj != null)
                {
                    obj.transform.SetParent(transform);
                }
            }

            if (obj != null)
            {
                var tracker = obj.GetComponent<PooledObjectTracker>();
                if (tracker == null) tracker = obj.AddComponent<PooledObjectTracker>();
                tracker.poolManager = this;
                tracker.poolTag = tag;
                tracker.isSpawned = true;

                objectToTag[obj] = tag;
                pool.Active.Add(obj);

                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);

                var poolable = obj.GetComponent<IPoolable>();
                poolable?.OnSpawned();
                return obj;
            }

            return null;
        }

        public void Despawn(GameObject obj)
        {
            if (obj == null) return;

            string tag = null;
            var tracker = obj.GetComponent<PooledObjectTracker>();
            if (tracker != null)
            {
                tag = tracker.poolTag;
                tracker.isSpawned = false;
            }
            if (string.IsNullOrEmpty(tag) && objectToTag.TryGetValue(obj, out string mappedTag))
            {
                tag = mappedTag;
            }

            if (!string.IsNullOrEmpty(tag) && pools.TryGetValue(tag, out var pool))
            {
                pool.Active.Remove(obj);
                
                var poolable = obj.GetComponent<IPoolable>();
                poolable?.OnDespawned();

                obj.SetActive(false);
                obj.transform.SetParent(transform);
                pool.Inactive.Push(obj);
            }
            else
            {
                var poolable = obj.GetComponent<IPoolable>();
                poolable?.OnDespawned();
                obj.SetActive(false);
            }
        }

        /// <summary>
        /// Callback invoked by PooledObjectTracker when a pooled object is deactivated via SetActive(false).
        /// </summary>
        public void NotifyDeactivated(GameObject obj, string tag)
        {
            if (obj == null) return;

            var tracker = obj.GetComponent<PooledObjectTracker>();
            if (tracker != null)
            {
                tracker.isSpawned = false;
            }

            if (string.IsNullOrEmpty(tag) && objectToTag.TryGetValue(obj, out string mappedTag))
            {
                tag = mappedTag;
            }

            if (!string.IsNullOrEmpty(tag) && pools.TryGetValue(tag, out var pool))
            {
                pool.Active.Remove(obj);

                var poolable = obj.GetComponent<IPoolable>();
                poolable?.OnDespawned();

                obj.transform.SetParent(transform);
                pool.Inactive.Push(obj);
            }
        }

        public int GetActiveCount(string tag)
        {
            return pools.TryGetValue(tag, out var pool) ? pool.Active.Count : 0;
        }

        public int GetInactiveCount(string tag)
        {
            return pools.TryGetValue(tag, out var pool) ? pool.Inactive.Count : 0;
        }

        public void Clear()
        {
            foreach (var kvp in pools)
            {
                foreach (var obj in kvp.Value.Inactive)
                {
                    if (obj != null) Destroy(obj);
                }
                foreach (var obj in kvp.Value.Active)
                {
                    if (obj != null) Destroy(obj);
                }
            }
            pools.Clear();
            objectToTag.Clear();
        }
    }
}

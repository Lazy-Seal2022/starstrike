using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StarStrike.Core.Interfaces;
using UnityEngine;

namespace StarStrike.Platform.Assets
{
    /// <summary>
    /// Synchronous and Resources-backed asset provider implementation.
    /// Provides zero-dependency asset loading for EditMode unit testing, offline scenarios, or instant fallbacks.
    /// </summary>
    public class DirectAssetProvider : IAssetProvider
    {
        private readonly Dictionary<string, UnityEngine.Object> _mockRegistry = new();
        private readonly HashSet<string> _loadedAddresses = new();

        public void RegisterMockAsset(string address, UnityEngine.Object asset)
        {
            _mockRegistry[address] = asset;
            _loadedAddresses.Add(address);
        }

        public void ClearMocks()
        {
            _mockRegistry.Clear();
            _loadedAddresses.Clear();
        }

        public Task<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object
        {
            return Task.FromResult(LoadAsset<T>(address));
        }

        public Task<IList<T>> LoadAssetsByLabelAsync<T>(string label) where T : UnityEngine.Object
        {
            var list = new List<T>();
            foreach (var kvp in _mockRegistry)
            {
                if (kvp.Value is T typed)
                {
                    list.Add(typed);
                }
            }
            return Task.FromResult<IList<T>>(list);
        }

        public Task<GameObject> InstantiateAsync(string address, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
        {
            var prefab = LoadAsset<GameObject>(address);
            if (prefab == null)
            {
                Debug.LogWarning($"[DirectAssetProvider] Failed to find prefab to instantiate at address '{address}'");
                return Task.FromResult<GameObject>(null);
            }

            var instance = UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
            return Task.FromResult(instance);
        }

        public T LoadAsset<T>(string address) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(address)) return null;

            if (_mockRegistry.TryGetValue(address, out var mock) && mock is T typedMock)
            {
                _loadedAddresses.Add(address);
                return typedMock;
            }

            var asset = Resources.Load<T>(address);
            if (asset != null)
            {
                _loadedAddresses.Add(address);
            }
            return asset;
        }

        public void ReleaseAsset<T>(T asset) where T : UnityEngine.Object
        {
            if (asset == null) return;

            string addressToRemove = null;
            foreach (var kvp in _mockRegistry)
            {
                if (kvp.Value == asset)
                {
                    addressToRemove = kvp.Key;
                    break;
                }
            }

            if (addressToRemove != null)
            {
                _mockRegistry.Remove(addressToRemove);
                _loadedAddresses.Remove(addressToRemove);
            }
        }

        public void ReleaseInstance(GameObject instance)
        {
            if (instance == null) return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEngine.Object.DestroyImmediate(instance);
                return;
            }
#endif
            UnityEngine.Object.Destroy(instance);
        }

        public bool IsAssetLoaded(string address)
        {
            return _loadedAddresses.Contains(address);
        }
    }
}

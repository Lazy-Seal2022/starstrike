using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StarStrike.Core.Interfaces;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace StarStrike.Platform.Assets
{
    /// <summary>
    /// Production asynchronous asset provider using Unity Addressables.
    /// Handles reference counting, VRAM/RAM release, and seamless fallback to Resources.
    /// </summary>
    public class AddressableAssetProvider : IAssetProvider
    {
        private readonly Dictionary<string, AsyncOperationHandle> _addressHandles = new();
        private readonly Dictionary<object, AsyncOperationHandle> _assetHandles = new();
        private readonly Dictionary<GameObject, AsyncOperationHandle<GameObject>> _instanceHandles = new();
        private readonly DirectAssetProvider _fallbackProvider = new();

        public async Task<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(address)) return null;

            if (_addressHandles.TryGetValue(address, out var existingHandle) && existingHandle.IsValid())
            {
                if (existingHandle.IsDone)
                {
                    return existingHandle.Result as T;
                }
                var waitResult = await existingHandle.Task;
                return waitResult as T;
            }

            try
            {
                var handle = Addressables.LoadAssetAsync<T>(address);
                var result = await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded && result != null)
                {
                    _addressHandles[address] = handle;
                    _assetHandles[result] = handle;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AddressableAssetProvider] Addressables load failed for '{address}': {ex.Message}. Falling back to DirectAssetProvider.");
            }

            return await _fallbackProvider.LoadAssetAsync<T>(address);
        }

        public async Task<IList<T>> LoadAssetsByLabelAsync<T>(string label) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(label)) return new List<T>();

            try
            {
                var handle = Addressables.LoadAssetsAsync<T>(label, null);
                var result = await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded && result != null)
                {
                    _assetHandles[result] = handle;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AddressableAssetProvider] Failed to load label '{label}': {ex.Message}");
            }

            return await _fallbackProvider.LoadAssetsByLabelAsync<T>(label);
        }

        public async Task<GameObject> InstantiateAsync(string address, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
        {
            if (string.IsNullOrEmpty(address)) return null;

            try
            {
                var handle = Addressables.InstantiateAsync(address, position, rotation, parent);
                var result = await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded && result != null)
                {
                    _instanceHandles[result] = handle;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AddressableAssetProvider] Addressables instantiate failed for '{address}': {ex.Message}. Falling back to DirectAssetProvider.");
            }

            return await _fallbackProvider.InstantiateAsync(address, position, rotation, parent);
        }

        public T LoadAsset<T>(string address) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(address)) return null;

            if (_addressHandles.TryGetValue(address, out var existingHandle) && existingHandle.IsValid() && existingHandle.IsDone)
            {
                return existingHandle.Result as T;
            }

            try
            {
                var handle = Addressables.LoadAssetAsync<T>(address);
                var result = handle.WaitForCompletion();

                if (handle.Status == AsyncOperationStatus.Succeeded && result != null)
                {
                    _addressHandles[address] = handle;
                    _assetHandles[result] = handle;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AddressableAssetProvider] Sync load failed for '{address}': {ex.Message}. Using Direct fallback.");
            }

            return _fallbackProvider.LoadAsset<T>(address);
        }

        public void ReleaseAsset<T>(T asset) where T : UnityEngine.Object
        {
            if (asset == null) return;

            if (_assetHandles.TryGetValue(asset, out var handle))
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
                _assetHandles.Remove(asset);

                string addressToRemove = null;
                foreach (var kvp in _addressHandles)
                {
                    if (!kvp.Value.IsValid() || Equals(kvp.Value.Result, asset))
                    {
                        addressToRemove = kvp.Key;
                        break;
                    }
                }
                if (addressToRemove != null)
                {
                    _addressHandles.Remove(addressToRemove);
                }
            }
            else
            {
                _fallbackProvider.ReleaseAsset(asset);
            }
        }

        public void ReleaseInstance(GameObject instance)
        {
            if (instance == null) return;

            if (_instanceHandles.TryGetValue(instance, out var handle))
            {
                if (handle.IsValid())
                {
                    Addressables.ReleaseInstance(instance);
                }
                _instanceHandles.Remove(instance);
            }
            else
            {
                _fallbackProvider.ReleaseInstance(instance);
            }
        }

        public bool IsAssetLoaded(string address)
        {
            if (_addressHandles.TryGetValue(address, out var handle) && handle.IsValid() && handle.IsDone)
            {
                return true;
            }
            return _fallbackProvider.IsAssetLoaded(address);
        }
    }
}

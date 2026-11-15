using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace StarStrike.Core.Interfaces
{
    /// <summary>
    /// Contract for decoupled asset loading, lifecycle management, and VRAM/RAM release.
    /// Decouples gameplay logic from concrete asset pipelines (Addressables, Resources, AssetBundles, or Test Mocks).
    /// </summary>
    public interface IAssetProvider
    {
        /// <summary>
        /// Loads an asset asynchronously by address/key.
        /// </summary>
        Task<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object;

        /// <summary>
        /// Loads all assets assigned with a given label asynchronously.
        /// </summary>
        Task<IList<T>> LoadAssetsByLabelAsync<T>(string label) where T : UnityEngine.Object;

        /// <summary>
        /// Instantiates a prefab asynchronously by address/key.
        /// </summary>
        Task<GameObject> InstantiateAsync(string address, Vector3 position = default, Quaternion rotation = default, Transform parent = null);

        /// <summary>
        /// Synchronously loads an asset. Useful for immediate fallbacks or EditMode unit tests.
        /// </summary>
        T LoadAsset<T>(string address) where T : UnityEngine.Object;

        /// <summary>
        /// Releases a loaded asset, decrementing its reference count and freeing memory/VRAM if 0.
        /// </summary>
        void ReleaseAsset<T>(T asset) where T : UnityEngine.Object;

        /// <summary>
        /// Releases an instantiated GameObject (Addressables instance or pooled object).
        /// </summary>
        void ReleaseInstance(GameObject instance);

        /// <summary>
        /// Checks whether an asset address is currently cached/loaded.
        /// </summary>
        bool IsAssetLoaded(string address);
    }
}

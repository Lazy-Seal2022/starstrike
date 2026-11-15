using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StarStrike.Core.Interfaces;
using UnityEngine;

namespace StarStrike.Platform.Assets
{
    /// <summary>
    /// Centralized asset management facade. Provides high-level async loading,
    /// synchronous fallback for EditMode testing, and automatic lifecycle management.
    /// </summary>
    public static class AssetService
    {
        private static IAssetProvider _provider;

        public static IAssetProvider Provider
        {
            get
            {
                if (_provider == null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        _provider = new DirectAssetProvider();
                        return _provider;
                    }
#endif
                    _provider = new AddressableAssetProvider();
                }
                return _provider;
            }
            set => _provider = value;
        }

        public static void SetProvider(IAssetProvider provider)
        {
            _provider = provider;
        }

        public static void ResetToDefault()
        {
            _provider = null;
        }

        public static Task<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object
        {
            return Provider.LoadAssetAsync<T>(address);
        }

        public static Task<IList<T>> LoadAssetsByLabelAsync<T>(string label) where T : UnityEngine.Object
        {
            return Provider.LoadAssetsByLabelAsync<T>(label);
        }

        public static Task<GameObject> InstantiateAsync(string address, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
        {
            return Provider.InstantiateAsync(address, position, rotation, parent);
        }

        public static T LoadAsset<T>(string address) where T : UnityEngine.Object
        {
            return Provider.LoadAsset<T>(address);
        }

        public static void ReleaseAsset<T>(T asset) where T : UnityEngine.Object
        {
            Provider.ReleaseAsset(asset);
        }

        public static void ReleaseInstance(GameObject instance)
        {
            Provider.ReleaseInstance(instance);
        }

        public static bool IsAssetLoaded(string address)
        {
            return Provider.IsAssetLoaded(address);
        }
    }
}

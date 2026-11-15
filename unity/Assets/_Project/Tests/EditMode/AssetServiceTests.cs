using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using StarStrike.Core;
using StarStrike.Platform.Assets;
using UnityEngine;
using UnityEngine.TestTools;

namespace StarStrike.Tests.EditMode
{
    [TestFixture]
    public class AssetServiceTests
    {
        [SetUp]
        public void SetUp()
        {
            AssetService.ResetToDefault();
            AssetService.SetProvider(new DirectAssetProvider());
        }

        [TearDown]
        public void TearDown()
        {
            AssetService.ResetToDefault();
        }

        [Test]
        public void AssetService_DirectProvider_LoadsResourcesConfigSynchronously()
        {
            var config = AssetService.LoadAsset<GameConfig>("Data/GameConfig");
            Assert.IsNotNull(config, "GameConfig must be loaded via AssetService synchronous path");
            Assert.Greater(config.worldWidth, 0f, "GameConfig must have valid worldWidth");
            Assert.IsTrue(AssetService.IsAssetLoaded("Data/GameConfig"));
        }

        [Test]
        public async Task AssetService_DirectProvider_LoadsAssetAsync()
        {
            var audioConfig = await AssetService.LoadAssetAsync<AudioConfig>("Data/AudioConfig");
            Assert.IsNotNull(audioConfig, "AudioConfig must load asynchronously through IAssetProvider");
        }

        [Test]
        public void AssetService_MockProvider_ResolvesMockAsset()
        {
            var direct = new DirectAssetProvider();
            var mockConfig = ScriptableObject.CreateInstance<GameConfig>();
            mockConfig.worldWidth = 999f;
            direct.RegisterMockAsset("Test/MockGameConfig", mockConfig);

            AssetService.SetProvider(direct);

            var retrieved = AssetService.LoadAsset<GameConfig>("Test/MockGameConfig");
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(999f, retrieved.worldWidth);

            AssetService.ReleaseAsset(retrieved);
            Assert.IsFalse(AssetService.IsAssetLoaded("Test/MockGameConfig"));

            Object.DestroyImmediate(mockConfig);
        }

        [Test]
        public async Task AssetService_InstantiateAsync_CreatesAndDestroysInstance()
        {
            var instance = await AssetService.InstantiateAsync("Prefabs/Environment/GemCrystal");
            Assert.IsNotNull(instance, "Prefab should instantiate through AssetService");

            AssetService.ReleaseInstance(instance);
            Assert.IsTrue(instance == null, "GameObject should be destroyed after ReleaseInstance");
        }

        [Test]
        public void AssetService_ReleaseAsset_GracefullyHandlesNull()
        {
            Assert.DoesNotThrow(() => AssetService.ReleaseAsset<GameConfig>(null));
            Assert.DoesNotThrow(() => AssetService.ReleaseInstance(null));
        }

        [Test]
        public void AddressableAssetProvider_GracefullyFallsBackToResources()
        {
            var provider = new AddressableAssetProvider();
            var config = provider.LoadAsset<GameConfig>("Data/GameConfig");
            Assert.IsNotNull(config, "AddressableAssetProvider should fall back to Resources when Addressables runtime is uninitialized");
        }
    }
}

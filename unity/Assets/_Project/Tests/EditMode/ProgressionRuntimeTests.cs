using NUnit.Framework;
using UnityEngine;
using StarStrike.Core;
using StarStrike.Gameplay;

namespace StarStrike.Tests.EditMode
{
    public class ProgressionRuntimeTests
    {
        private GameObject playerGo;
        private PlayerProgression progression;
        private ShipDefinition scoutShip;
        private ShipDefinition fighterShip;

        [SetUp]
        public void SetUp()
        {
            playerGo = new GameObject("TestPlayerProgression");
            playerGo.AddComponent<Rigidbody2D>();
            playerGo.AddComponent<ShipHealth>();
            playerGo.AddComponent<ShipMovement>();
            playerGo.AddComponent<ShipWeapon>();

            scoutShip = ScriptableObject.CreateInstance<ShipDefinition>();
            scoutShip.id = "scout";
            scoutShip.shipName = "Delta Scout";
            scoutShip.tier = 1;
            scoutShip.maxUpgradeLevel = 2;
            scoutShip.baseStats = new StatValues
            {
                shieldCapacity = 100f,
                shieldRegen = 10f,
                energyCapacity = 80f,
                energyRegen = 15f,
                pulseDamage = 25f,
                pulseSpeed = 16f,
                speed = 8f,
                agility = 180f
            };

            fighterShip = ScriptableObject.CreateInstance<ShipDefinition>();
            fighterShip.id = "fighter";
            fighterShip.shipName = "Star Fighter";
            fighterShip.tier = 2;
            fighterShip.maxUpgradeLevel = 5;
            fighterShip.weaponSlots = 2;
            fighterShip.baseStats = new StatValues
            {
                shieldCapacity = 150f,
                shieldRegen = 15f,
                energyCapacity = 100f,
                energyRegen = 20f,
                pulseDamage = 40f,
                pulseSpeed = 20f,
                speed = 10f,
                agility = 220f
            };
            scoutShip.evolvesTo = new ShipDefinition[] { fighterShip };

            progression = playerGo.AddComponent<PlayerProgression>();
            progression.currentShip = scoutShip;

            GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
            config.upgradeBaseCost = 10;
            config.upgradeCostMultiplier = 1.5f;
            progression.gameConfig = config;

            progression.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            if (playerGo != null)
            {
                Object.DestroyImmediate(playerGo);
            }
            if (scoutShip != null) Object.DestroyImmediate(scoutShip);
            if (fighterShip != null) Object.DestroyImmediate(fighterShip);
        }

        [Test]
        public void AddGems_IncreasesGemCount()
        {
            Assert.AreEqual(0, progression.gems);
            progression.AddGems(50);
            Assert.AreEqual(50, progression.gems);
        }

        [Test]
        public void AddGems_FiresGemCollectedEvent_WithExactAmountAndTotal()
        {
            int receivedAmount = -1;
            int receivedTotal = -1;
            System.Action<int, int> handler = (amt, tot) =>
            {
                receivedAmount = amt;
                receivedTotal = tot;
            };

            GameEvents.OnGemCollected += handler;
            progression.AddGems(42);
            GameEvents.OnGemCollected -= handler;

            Assert.AreEqual(42, receivedAmount, "OnGemCollected must deliver the non-zero amount collected");
            Assert.AreEqual(42, receivedTotal, "OnGemCollected must deliver the running total");
        }

        [Test]
        public void CreateDefaultShipTree_ProvidesFullThreeTierHierarchy()
        {
            ShipDefinition root = PlayerProgression.CreateDefaultShipTree();
            Assert.IsNotNull(root);
            Assert.AreEqual("scout", root.id);
            Assert.AreEqual(2, root.evolvesTo.Length);

            ShipDefinition fighter = root.evolvesTo[0];
            ShipDefinition miner = root.evolvesTo[1];
            Assert.AreEqual("fighter", fighter.id);
            Assert.AreEqual("miner", miner.id);

            Assert.AreEqual(1, fighter.evolvesTo.Length);
            Assert.AreEqual("interceptor", fighter.evolvesTo[0].id);

            Assert.AreEqual(1, miner.evolvesTo.Length);
            Assert.AreEqual("juggernaut", miner.evolvesTo[0].id);
        }

        [Test]
        public void ApplyShipVisuals_SetsScaleForTier3Ships()
        {
            ShipDefinition root = PlayerProgression.CreateDefaultShipTree();
            ShipDefinition fighter = root.evolvesTo[0];
            ShipDefinition interceptor = fighter.evolvesTo[0];

            progression.currentShip = interceptor;
            progression.ApplyShipVisuals();
            Assert.AreEqual(1.2f, playerGo.transform.localScale.x, 0.01f);

            ShipDefinition miner = root.evolvesTo[1];
            ShipDefinition juggernaut = miner.evolvesTo[0];
            progression.currentShip = juggernaut;
            progression.ApplyShipVisuals();
            Assert.AreEqual(1.6f, playerGo.transform.localScale.x, 0.01f);
        }

        [Test]
        public void UpgradeStat_DeductsGemsAndIncrementsLevel()
        {
            progression.AddGems(100);
            int cost = progression.GetUpgradeCost("damage");
            Assert.AreEqual(10, cost);

            bool success = progression.UpgradeStat("damage");
            Assert.IsTrue(success);
            Assert.AreEqual(1, progression.upgradeState.pulseDamageLevel);
            Assert.AreEqual(90, progression.gems);
        }

        [Test]
        public void UpgradeStat_FailsWhenInsufficientGems()
        {
            progression.AddGems(5); // cost is 10
            bool success = progression.UpgradeStat("damage");

            Assert.IsFalse(success);
            Assert.AreEqual(0, progression.upgradeState.pulseDamageLevel);
            Assert.AreEqual(5, progression.gems);
        }

        [Test]
        public void CanEvolve_ReturnsTrueOnlyWhenAllStatsMaxed()
        {
            Assert.IsFalse(progression.CanEvolve(), "Cannot evolve when stats are 0");

            progression.upgradeState.shieldCapacityLevel = 2;
            progression.upgradeState.shieldRegenLevel = 2;
            progression.upgradeState.energyCapacityLevel = 2;
            progression.upgradeState.energyRegenLevel = 2;
            progression.upgradeState.pulseDamageLevel = 2;
            progression.upgradeState.pulseSpeedLevel = 2;
            progression.upgradeState.speedLevel = 2;
            progression.upgradeState.agilityLevel = 1; // 1 not maxed!

            Assert.IsFalse(progression.CanEvolve(), "Cannot evolve when 1 stat is still below max");

            progression.upgradeState.agilityLevel = 2; // All 8 maxed!
            Assert.IsTrue(progression.CanEvolve(), "Must be able to evolve when all stats maxed");
        }

        [Test]
        public void Evolve_AppliesNewShipBaseStatsAndResetsUpgradeLevels()
        {
            progression.upgradeState.pulseDamageLevel = 2;
            progression.Evolve(fighterShip);

            Assert.AreEqual("fighter", progression.currentShip.id);
            Assert.AreEqual(0, progression.upgradeState.pulseDamageLevel, "Upgrade levels must reset on evolution");
            Assert.AreEqual(2, progression.weapon.weaponSlots, "Evolving to fighter should grant 2 weapon slots");
        }
    }
}

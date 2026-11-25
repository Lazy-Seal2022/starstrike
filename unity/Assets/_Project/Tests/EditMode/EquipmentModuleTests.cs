using NUnit.Framework;
using UnityEngine;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.Platform.Storage;
using StarStrike.UI;

namespace StarStrike.Tests.EditMode
{
    public class EquipmentModuleTests
    {
        [SetUp]
        public void Setup()
        {
            StorageService.LoadProfile();
        }

        [Test]
        public void EquipmentModuleCatalog_HasAllFourModuleCategories_WithValidStats()
        {
            var defenseMods = EquipmentModuleCatalog.GetModulesByCategory(EquipmentSlotCategory.Defense);
            var engineMods = EquipmentModuleCatalog.GetModulesByCategory(EquipmentSlotCategory.Engine);
            var reactorMods = EquipmentModuleCatalog.GetModulesByCategory(EquipmentSlotCategory.Reactor);
            var auxMods = EquipmentModuleCatalog.GetModulesByCategory(EquipmentSlotCategory.Auxiliary);

            Assert.GreaterOrEqual(defenseMods.Count, 3, "Expected at least 3 defense modules");
            Assert.GreaterOrEqual(engineMods.Count, 3, "Expected at least 3 engine modules");
            Assert.GreaterOrEqual(reactorMods.Count, 3, "Expected at least 3 reactor modules");
            Assert.GreaterOrEqual(auxMods.Count, 3, "Expected at least 3 auxiliary modules");

            foreach (var mod in EquipmentModuleCatalog.AllModules)
            {
                Assert.IsFalse(string.IsNullOrEmpty(mod.id));
                Assert.IsFalse(string.IsNullOrEmpty(mod.name));
                Assert.IsFalse(string.IsNullOrEmpty(mod.role));
                Assert.IsFalse(string.IsNullOrEmpty(mod.description));
                Assert.Greater(mod.gemCost, 0);
            }
        }

        [Test]
        public void ShipArchetypes_HaveValidEquipmentSlots_MatchingRoles()
        {
            var fighter = ShipArchetypeFactory.CreateArchetype(ShipClass.Fighter);
            var frigate = ShipArchetypeFactory.CreateArchetype(ShipClass.Frigate);
            var destroyer = ShipArchetypeFactory.CreateArchetype(ShipClass.Destroyer);
            var cruiser = ShipArchetypeFactory.CreateArchetype(ShipClass.Cruiser);
            var carrier = ShipArchetypeFactory.CreateArchetype(ShipClass.Carrier);
            var battleship = ShipArchetypeFactory.CreateArchetype(ShipClass.Battleship);

            Assert.AreEqual(4, fighter.equipmentSlots.Length, "Fighter should have 4 equipment slots");
            Assert.AreEqual(6, frigate.equipmentSlots.Length, "Frigate should have 6 equipment slots");
            Assert.AreEqual(7, destroyer.equipmentSlots.Length, "Destroyer should have 7 equipment slots");
            Assert.AreEqual(9, cruiser.equipmentSlots.Length, "Cruiser should have 9 equipment slots");
            Assert.AreEqual(9, carrier.equipmentSlots.Length, "Carrier should have 9 equipment slots");
            Assert.AreEqual(11, battleship.equipmentSlots.Length, "Battleship should have 11 equipment slots");

            // Verify Battleship slot composition
            int bWeapons = 0, bDefense = 0, bReactor = 0, bAux = 0;
            foreach (var s in battleship.equipmentSlots)
            {
                if (s.category == EquipmentSlotCategory.Weapon) bWeapons++;
                else if (s.category == EquipmentSlotCategory.Defense) bDefense++;
                else if (s.category == EquipmentSlotCategory.Reactor) bReactor++;
                else if (s.category == EquipmentSlotCategory.Auxiliary) bAux++;
            }
            Assert.AreEqual(6, bWeapons);
            Assert.AreEqual(2, bDefense);
            Assert.AreEqual(2, bReactor);
            Assert.AreEqual(1, bAux);
        }

        [Test]
        public void StorageService_CanSaveAndRetrieveEquippedModules()
        {
            StorageService.SaveEquippedModule("frigate", "def_0", "def_shield_booster");
            StorageService.SaveEquippedModule("frigate", "eng_0", "eng_vector_thrusters");

            Assert.AreEqual("def_shield_booster", StorageService.GetEquippedModule("frigate", "def_0"));
            Assert.AreEqual("eng_vector_thrusters", StorageService.GetEquippedModule("frigate", "eng_0"));
            Assert.AreEqual("", StorageService.GetEquippedModule("frigate", "non_existent_slot"));

            var allFrigate = StorageService.GetAllEquippedModules("frigate");
            Assert.IsTrue(allFrigate.ContainsKey("def_0"));
            Assert.AreEqual("def_shield_booster", allFrigate["def_0"]);
        }

        [Test]
        public void ProceduralSpriteHelper_GeneratesSpritesForNewShipClassesAndModuleIcons()
        {
            var frigateSprite = ProceduralSpriteHelper.GenerateShipSprite("frigate");
            var destroyerSprite = ProceduralSpriteHelper.GenerateShipSprite("destroyer");
            var cruiserSprite = ProceduralSpriteHelper.GenerateShipSprite("cruiser");
            var battleshipSprite = ProceduralSpriteHelper.GenerateShipSprite("battleship");

            Assert.IsNotNull(frigateSprite);
            Assert.IsNotNull(destroyerSprite);
            Assert.IsNotNull(cruiserSprite);
            Assert.IsNotNull(battleshipSprite);

            var defIcon = ProceduralSpriteHelper.GetCategoryIcon(EquipmentSlotCategory.Defense);
            var engIcon = ProceduralSpriteHelper.GetCategoryIcon(EquipmentSlotCategory.Engine);
            var reaIcon = ProceduralSpriteHelper.GetCategoryIcon(EquipmentSlotCategory.Reactor);
            var auxIcon = ProceduralSpriteHelper.GetCategoryIcon(EquipmentSlotCategory.Auxiliary);

            Assert.IsNotNull(defIcon);
            Assert.IsNotNull(engIcon);
            Assert.IsNotNull(reaIcon);
            Assert.IsNotNull(auxIcon);
        }

        [Test]
        public void ShipDefinition_GetEquipmentSlots_ReturnsFallbackWhenUnconfigured()
        {
            var legacyShip = ScriptableObject.CreateInstance<ShipDefinition>();
            legacyShip.weaponSockets = new[] { new Vector2(1f, 0.5f), new Vector2(1f, -0.5f) };

            var slots = legacyShip.GetEquipmentSlots();
            Assert.IsNotNull(slots);
            Assert.GreaterOrEqual(slots.Length, 5); // 2 weapons + def + eng + rea
            Assert.AreEqual(EquipmentSlotCategory.Weapon, slots[0].category);
            Assert.AreEqual(EquipmentSlotCategory.Weapon, slots[1].category);
            Assert.AreEqual(EquipmentSlotCategory.Defense, slots[2].category);
            Assert.AreEqual(EquipmentSlotCategory.Engine, slots[3].category);
            Assert.AreEqual(EquipmentSlotCategory.Reactor, slots[4].category);
        }

        [Test]
        public void ShipHealth_ArmorDamageReduction_MitigatesHullDamage()
        {
            var go = new GameObject("TestShip_Armor");
            var health = go.AddComponent<ShipHealth>();
            health.maxShield = 0f;
            health.currentShield = 0f;
            health.maxHull = 200f;
            health.currentHull = 200f;
            health.armorDamageReduction = 0.25f; // 25% reduction

            health.TakeDamage(100f);
            // 100 * (1 - 0.25) = 75 damage applied
            Assert.AreEqual(125f, health.CurrentHull, 0.01f);

            Object.DestroyImmediate(go);
        }
    }
}

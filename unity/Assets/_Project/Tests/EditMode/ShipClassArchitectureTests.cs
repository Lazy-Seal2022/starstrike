using NUnit.Framework;
using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Tests.EditMode
{
    public class ShipClassArchitectureTests
    {
        [Test]
        public void AllSixShipClasses_CanBeInstantiated_WithValidArchetypes()
        {
            var archetypes = ShipArchetypeFactory.CreateAllArchetypes();
            Assert.AreEqual(6, archetypes.Count, "Expected exactly 6 ship class archetypes");

            foreach (var ship in archetypes)
            {
                Assert.IsNotNull(ship);
                Assert.IsFalse(string.IsNullOrEmpty(ship.id));
                Assert.IsFalse(string.IsNullOrEmpty(ship.shipName));
                Assert.Greater(ship.cameraOrthoSize, 5f);
                Assert.Greater(ship.hullMass, 0.5f);
                Assert.Greater(ship.baseStats.hullCapacity, 0f);
                Assert.Greater(ship.baseStats.shieldCapacity, 0f);
                Assert.Greater(ship.baseStats.energyCapacity, 0f);
                Assert.IsNotNull(ship.hardpointSockets);
                Assert.GreaterOrEqual(ship.hardpointSockets.Length, 2);
            }
        }

        [Test]
        public void CameraAndMass_EscalateProgressivelyByHullClass()
        {
            var fighter = ShipArchetypeFactory.CreateArchetype(ShipClass.Fighter);
            var frigate = ShipArchetypeFactory.CreateArchetype(ShipClass.Frigate);
            var destroyer = ShipArchetypeFactory.CreateArchetype(ShipClass.Destroyer);
            var cruiser = ShipArchetypeFactory.CreateArchetype(ShipClass.Cruiser);
            var carrier = ShipArchetypeFactory.CreateArchetype(ShipClass.Carrier);
            var battleship = ShipArchetypeFactory.CreateArchetype(ShipClass.Battleship);

            // Progressive camera zoom-out
            Assert.Less(fighter.cameraOrthoSize, frigate.cameraOrthoSize);
            Assert.Less(frigate.cameraOrthoSize, destroyer.cameraOrthoSize);
            Assert.Less(destroyer.cameraOrthoSize, cruiser.cameraOrthoSize);
            Assert.Less(cruiser.cameraOrthoSize, carrier.cameraOrthoSize);
            Assert.Less(carrier.cameraOrthoSize, battleship.cameraOrthoSize);

            // Progressive hull mass and inertia
            Assert.Less(fighter.hullMass, frigate.hullMass);
            Assert.Less(frigate.hullMass, destroyer.hullMass);
            Assert.Less(destroyer.hullMass, cruiser.hullMass);
            Assert.Less(cruiser.hullMass, carrier.hullMass);
            Assert.Less(carrier.hullMass, battleship.hullMass);
        }

        [Test]
        public void HardpointSockets_MatchArchetypeRolesAndSizes()
        {
            var fighter = ShipArchetypeFactory.CreateArchetype(ShipClass.Fighter);
            Assert.AreEqual(2, fighter.hardpointSockets.Length);
            Assert.AreEqual(SocketSize.Light, fighter.hardpointSockets[0].size);
            Assert.AreEqual(SocketSize.Light, fighter.hardpointSockets[1].size);

            var frigate = ShipArchetypeFactory.CreateArchetype(ShipClass.Frigate);
            Assert.AreEqual(3, frigate.hardpointSockets.Length);

            var destroyer = ShipArchetypeFactory.CreateArchetype(ShipClass.Destroyer);
            Assert.AreEqual(4, destroyer.hardpointSockets.Length);
            Assert.AreEqual(SocketSize.Heavy, destroyer.hardpointSockets[2].size);
            Assert.AreEqual(SocketSize.Heavy, destroyer.hardpointSockets[3].size);

            var cruiser = ShipArchetypeFactory.CreateArchetype(ShipClass.Cruiser);
            Assert.AreEqual(5, cruiser.hardpointSockets.Length);
            bool hasTurret = false;
            foreach (var s in cruiser.hardpointSockets)
            {
                if (s.mountType == SocketMountType.Turret) hasTurret = true;
            }
            Assert.IsTrue(hasTurret, "Cruiser must have auto-tracking turret mounts");

            var carrier = ShipArchetypeFactory.CreateArchetype(ShipClass.Carrier);
            Assert.AreEqual(4, carrier.hardpointSockets.Length);
            Assert.AreEqual(4, carrier.droneHangarCapacity);
            bool hasHangar = false;
            foreach (var s in carrier.hardpointSockets)
            {
                if (s.size == SocketSize.Hangar && s.mountType == SocketMountType.HangarBay) hasHangar = true;
            }
            Assert.IsTrue(hasHangar, "Carrier must have dedicated HangarBay sockets");

            var battleship = ShipArchetypeFactory.CreateArchetype(ShipClass.Battleship);
            Assert.AreEqual(6, battleship.hardpointSockets.Length);
            Assert.AreEqual(SocketSize.Spinal, battleship.hardpointSockets[0].size);
            Assert.AreEqual(SocketMountType.Spinal, battleship.hardpointSockets[0].mountType);
            Assert.GreaterOrEqual(battleship.collisionArmorReduction, 0.35f, "Battleship must have high collision armor reduction");
        }

        [Test]
        public void LegacySocketInterop_SeamlesslyReturnsPositions()
        {
            var battleship = ShipArchetypeFactory.CreateArchetype(ShipClass.Battleship);
            Vector2[] positions = battleship.GetWeaponSocketPositions();

            Assert.IsNotNull(positions);
            Assert.AreEqual(6, positions.Length);
            Assert.AreEqual(battleship.hardpointSockets[0].localPosition, positions[0]);
            Assert.AreEqual(battleship.hardpointSockets[5].localPosition, positions[5]);
            Assert.AreEqual(6, battleship.SocketCount);
        }

        [Test]
        public void ModularClassBuilders_ProperlyConfigureDedicatedShipVariants()
        {
            var scout = ScriptableObject.CreateInstance<ShipDefinition>();
            FighterClass.ConfigureScout(scout);
            Assert.AreEqual("scout", scout.id);
            Assert.AreEqual(ShipClass.Fighter, scout.shipClass);
            Assert.AreEqual("Delta Scout", scout.shipName);

            var fighter = ScriptableObject.CreateInstance<ShipDefinition>();
            FighterClass.ConfigureFighter(fighter);
            Assert.AreEqual("fighter", fighter.id);
            Assert.AreEqual(ShipClass.Fighter, fighter.shipClass);

            var frigate = ScriptableObject.CreateInstance<ShipDefinition>();
            FrigateClass.ConfigureFrigate(frigate);
            Assert.AreEqual("frigate", frigate.id);
            Assert.AreEqual(ShipClass.Frigate, frigate.shipClass);

            var destroyer = ScriptableObject.CreateInstance<ShipDefinition>();
            DestroyerClass.ConfigureDestroyer(destroyer);
            Assert.AreEqual("destroyer", destroyer.id);
            Assert.AreEqual(ShipClass.Destroyer, destroyer.shipClass);

            var cruiser = ScriptableObject.CreateInstance<ShipDefinition>();
            CruiserClass.ConfigureCruiser(cruiser);
            Assert.AreEqual("cruiser", cruiser.id);
            Assert.AreEqual(ShipClass.Cruiser, cruiser.shipClass);

            var carrier = ScriptableObject.CreateInstance<ShipDefinition>();
            CarrierClass.ConfigureCarrier(carrier);
            Assert.AreEqual("carrier", carrier.id);
            Assert.AreEqual(ShipClass.Carrier, carrier.shipClass);

            var battleship = ScriptableObject.CreateInstance<ShipDefinition>();
            BattleshipClass.ConfigureBattleship(battleship);
            Assert.AreEqual("battleship", battleship.id);
            Assert.AreEqual(ShipClass.Battleship, battleship.shipClass);
        }

        [Test]
        public void ShipArchetypeFactory_CreateShipById_ConfiguresAllVariantsCorrectly()
        {
            string[] shipIds = new[] { "scout", "fighter", "interceptor", "phantom", "miner", "frigate", "destroyer", "cruiser", "carrier", "vanguard", "juggernaut", "battleship" };

            foreach (var id in shipIds)
            {
                var ship = ShipArchetypeFactory.CreateShipById(id);
                Assert.IsNotNull(ship, $"Ship '{id}' must not be null");
                Assert.AreEqual(id, ship.id, $"Ship id mismatch for '{id}'");
                Assert.IsNotNull(ship.baseStats);
                Assert.Greater(ship.baseStats.hullCapacity, 0f);
                Assert.Greater(ship.baseStats.shieldCapacity, 0f);
                Assert.IsNotNull(ship.weaponSockets);
                Assert.GreaterOrEqual(ship.weaponSockets.Length, 1);
            }
        }
    }
}

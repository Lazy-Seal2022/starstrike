using System.Collections.Generic;
using UnityEngine;

namespace StarStrike.Core
{
    /// <summary>
    /// Factory for generating and querying the 6 naval ship class archetypes and fleet configurations.
    /// Delegates specific ship class configuration to modular class builders under Classes/.
    /// </summary>
    public static class ShipArchetypeFactory
    {
        public static ShipDefinition CreateArchetype(ShipClass shipClass)
        {
            var def = ScriptableObject.CreateInstance<ShipDefinition>();
            def.shipClass = shipClass;
            def.classRoleDescription = shipClass.GetClassRoleDescription();
            def.cameraOrthoSize = shipClass.GetDefaultCameraSize();
            def.hullMass = shipClass.GetDefaultHullMass();
            def.collisionArmorReduction = shipClass.GetCollisionArmorReduction();

            switch (shipClass)
            {
                case ShipClass.Fighter:
                    FighterClass.ConfigureFighter(def);
                    break;
                case ShipClass.Frigate:
                    FrigateClass.ConfigureFrigate(def);
                    break;
                case ShipClass.Destroyer:
                    DestroyerClass.ConfigureDestroyer(def);
                    break;
                case ShipClass.Cruiser:
                    CruiserClass.ConfigureCruiser(def);
                    break;
                case ShipClass.Carrier:
                    CarrierClass.ConfigureCarrier(def);
                    break;
                case ShipClass.Battleship:
                    BattleshipClass.ConfigureBattleship(def);
                    break;
            }

            // Sync legacy weaponSockets for 100% backward compatibility
            def.weaponSockets = def.GetWeaponSocketPositions();
            def.weaponSlots = def.hardpointSockets != null ? def.hardpointSockets.Length : 1;
            return def;
        }

        public static List<ShipDefinition> CreateAllArchetypes()
        {
            var list = new List<ShipDefinition>();
            foreach (ShipClass sc in System.Enum.GetValues(typeof(ShipClass)))
            {
                list.Add(CreateArchetype(sc));
            }
            return list;
        }

        public static ShipDefinition CreateShipById(string id)
        {
            if (string.IsNullOrEmpty(id)) id = "scout";
            id = id.ToLowerInvariant();

            var def = ScriptableObject.CreateInstance<ShipDefinition>();
            switch (id)
            {
                case "scout":
                    FighterClass.ConfigureScout(def);
                    break;
                case "fighter":
                    FighterClass.ConfigureFighter(def);
                    break;
                case "interceptor":
                    FighterClass.ConfigureInterceptor(def);
                    break;
                case "phantom":
                    FighterClass.ConfigurePhantom(def);
                    break;
                case "miner":
                    FrigateClass.ConfigureMiner(def);
                    break;
                case "frigate":
                    FrigateClass.ConfigureFrigate(def);
                    break;
                case "destroyer":
                    DestroyerClass.ConfigureDestroyer(def);
                    break;
                case "cruiser":
                    CruiserClass.ConfigureCruiser(def);
                    break;
                case "carrier":
                    CarrierClass.ConfigureCarrier(def);
                    break;
                case "vanguard":
                    CarrierClass.ConfigureVanguard(def);
                    break;
                case "juggernaut":
                    BattleshipClass.ConfigureJuggernaut(def);
                    break;
                case "battleship":
                    BattleshipClass.ConfigureBattleship(def);
                    break;
                default:
                    FighterClass.ConfigureScout(def);
                    break;
            }

            def.classRoleDescription = def.shipClass.GetClassRoleDescription();
            if (def.cameraOrthoSize <= 0f) def.cameraOrthoSize = def.shipClass.GetDefaultCameraSize();
            if (def.hullMass <= 0f) def.hullMass = def.shipClass.GetDefaultHullMass();
            if (def.collisionArmorReduction <= 0f) def.collisionArmorReduction = def.shipClass.GetCollisionArmorReduction();

            // Sync legacy weaponSockets for 100% backward compatibility
            def.weaponSockets = def.GetWeaponSocketPositions();
            def.weaponSlots = def.hardpointSockets != null ? def.hardpointSockets.Length : 1;
            return def;
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using StarStrike.Core;
using StarStrike.Gameplay;

namespace StarStrike.UI
{
    public class HUDManager : MonoBehaviour
    {
        [Tooltip("When enabled, migrates automatically to modern uGUI CanvasHUD.")]
        public bool useCanvasUI = true;

        private ShipController player;
        private float shieldCurrent = 100f;
        private float shieldMax = 100f;
        private float energyCurrent = 80f;
        private float energyMax = 80f;
        private int totalGems = 0;
        private string shipName = "T1 • DELTA SCOUT";

        private bool showUpgradeDrawer = false;
        private Texture2D texWhite;

        private void Awake()
        {
            if (useCanvasUI)
            {
                if (GetComponent<CanvasHUD>() == null && FindAnyObjectByType<CanvasHUD>() == null)
                {
                    gameObject.AddComponent<CanvasHUD>();
                }
                enabled = false;
            }
        }

        private void OnEnable()
        {
            GameEvents.OnShieldChanged += HandleShield;
            GameEvents.OnEnergyChanged += HandleEnergy;
            GameEvents.OnGemCollected += HandleGem;
            GameEvents.OnPlayerEvolved += HandleEvolved;
        }

        private void OnDisable()
        {
            GameEvents.OnShieldChanged -= HandleShield;
            GameEvents.OnEnergyChanged -= HandleEnergy;
            GameEvents.OnGemCollected -= HandleGem;
            GameEvents.OnPlayerEvolved -= HandleEvolved;
        }

        private void Start()
        {
            texWhite = new Texture2D(1, 1);
            texWhite.SetPixel(0, 0, Color.white);
            texWhite.Apply();

            FindPlayer();
        }

        private void FindPlayer()
        {
            if (player == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null)
                {
                    player = p.GetComponent<ShipController>();
                    if (player != null && player.stats != null)
                    {
                        shipName = $"T{player.stats.tier} • {player.stats.shipName.ToUpper()}";
                        shieldMax = player.stats.shieldCapacity;
                        shieldCurrent = shieldMax;
                        energyMax = player.stats.energyCapacity;
                        energyCurrent = energyMax;
                    }
                }
            }
        }

        private void HandleShield(float cur, float max) { shieldCurrent = cur; shieldMax = max; }
        private void HandleEnergy(float cur, float max) { energyCurrent = cur; energyMax = max; }
        private void HandleGem(int added, int total) { totalGems = total; }
        private void HandleEvolved(string shipId)
        {
            if (player != null && player.stats != null)
            {
                shipName = $"T{player.stats.tier} • {player.stats.shipName.ToUpper()}";
            }
        }

        private void OnGUI()
        {
            FindPlayer();

            int screenW = Screen.width;
            int screenH = Screen.height;

            // GUI Skin styling
            GUI.skin.label.fontSize = 14;
            GUI.skin.button.fontSize = 13;

            // 1. Top Bar: Ship Info & Gems
            GUI.Box(new Rect(10, 10, 320, 95), "");
            GUI.Label(new Rect(20, 14, 280, 24), $"<b><color=#38bdf8>{shipName}</color></b>");
            GUI.Label(new Rect(20, 36, 280, 24), $"💎 <color=#4ade80><b>{totalGems}</b> Gems</color>");

            // Shield Bar (Blue)
            float shieldRatio = Mathf.Clamp01(shieldCurrent / Mathf.Max(1f, shieldMax));
            GUI.Label(new Rect(20, 58, 60, 20), "<size=11>SHIELD</size>");
            DrawBar(new Rect(75, 62, 170, 12), shieldRatio, new Color(0.22f, 0.74f, 0.97f));
            GUI.Label(new Rect(250, 58, 70, 20), $"<size=11>{Mathf.RoundToInt(shieldCurrent)}/{Mathf.RoundToInt(shieldMax)}</size>");

            // Energy Bar (Orange)
            float energyRatio = Mathf.Clamp01(energyCurrent / Mathf.Max(1f, energyMax));
            GUI.Label(new Rect(20, 78, 60, 20), "<size=11>ENERGY</size>");
            DrawBar(new Rect(75, 82, 170, 12), energyRatio, new Color(0.96f, 0.62f, 0.04f));
            GUI.Label(new Rect(250, 78, 70, 20), $"<size=11>{Mathf.RoundToInt(energyCurrent)}/{Mathf.RoundToInt(energyMax)}</size>");

            // 2. Toggle Upgrades Button
            if (GUI.Button(new Rect(10, 115, 140, 32), showUpgradeDrawer ? "▲ HIDE UPGRADES" : "▼ UPGRADES [U]"))
            {
                showUpgradeDrawer = !showUpgradeDrawer;
            }

            // Keyboard shortcut via InputSystem
            if (Keyboard.current != null && Keyboard.current.uKey.wasPressedThisFrame)
            {
                showUpgradeDrawer = !showUpgradeDrawer;
            }

            // 3. Upgrades Drawer
            if (showUpgradeDrawer && player != null)
            {
                GUI.Box(new Rect(10, 155, 340, 280), "<b>SHIP UPGRADES</b>");
                DrawUpgradeButton("shieldCap", "1. Shield Capacity", 185);
                DrawUpgradeButton("shieldRegen", "2. Shield Regen", 215);
                DrawUpgradeButton("energyCap", "3. Energy Capacity", 245);
                DrawUpgradeButton("energyRegen", "4. Energy Regen", 275);
                DrawUpgradeButton("damage", "5. Pulse Damage", 305);
                DrawUpgradeButton("pulseSpeed", "6. Pulse Speed", 335);
                DrawUpgradeButton("speed", "7. Engine Speed", 365);
                DrawUpgradeButton("agility", "8. Turn Agility", 395);
            }

            // 4. Evolution Banner
            if (player != null && player.CanEvolve())
            {
                GUI.Box(new Rect(screenW / 2 - 160, 20, 320, 80), "<b>★ EVOLUTION AVAILABLE ★</b>");
                if (GUI.Button(new Rect(screenW / 2 - 150, 50, 140, 38), "⚔ ARES FIGHTER"))
                {
                    player.Evolve("fighter");
                }
                if (GUI.Button(new Rect(screenW / 2 + 10, 50, 140, 38), "⛏ GOLIATH MINER"))
                {
                    player.Evolve("miner");
                }
            }

            // 5. On-Screen Touch Controls (Mobile / Tablet / Responsive)
            bool isMobile = Application.isMobilePlatform || Touchscreen.current != null;
            if (isMobile)
            {
                int btnSize = 85;
                // Virtual Fire Button (Bottom Right)
                Rect fireRect = new Rect(screenW - btnSize - 25, screenH - btnSize - 25, btnSize, btnSize);
                if (GUI.RepeatButton(fireRect, "<b><size=18>FIRE</size></b>"))
                {
                    if (player != null) player.inputFire = true;
                }

                // Virtual Thrust Button
                Rect thrustRect = new Rect(screenW - btnSize * 2 - 45, screenH - btnSize - 25, btnSize, btnSize);
                if (GUI.RepeatButton(thrustRect, "<b><size=16>THRUST</size></b>"))
                {
                    if (player != null) player.inputThrust = true;
                }
            }
        }

        private void DrawUpgradeButton(string statKey, string label, float y)
        {
            int cost = player.GetUpgradeCost(statKey);
            bool canAfford = player.gems >= cost;

            GUI.Label(new Rect(20, y, 160, 24), label);

            string btnText = $"{cost} 💎";
            GUI.color = canAfford ? Color.white : new Color(0.6f, 0.6f, 0.6f);
            if (GUI.Button(new Rect(230, y, 105, 26), btnText) && canAfford)
            {
                player.UpgradeStat(statKey);
            }
            GUI.color = Color.white;
        }

        private void DrawBar(Rect rect, float ratio, Color barColor)
        {
            GUI.color = new Color(0.1f, 0.15f, 0.2f, 0.8f);
            GUI.DrawTexture(rect, texWhite);

            GUI.color = barColor;
            Rect fillRect = new Rect(rect.x, rect.y, rect.width * ratio, rect.height);
            GUI.DrawTexture(fillRect, texWhite);

            GUI.color = Color.white;
        }
    }
}

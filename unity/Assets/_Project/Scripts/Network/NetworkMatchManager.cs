using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.Gameplay.Audio;

namespace StarStrike.Network
{
    public class NetworkMatchManager : NetworkBehaviour
    {
        public static NetworkMatchManager Instance { get; private set; }

        public static event Action<string, string> OnKillFeed;
        public static event Action<string> OnMatchOver;
        public static event Action OnScoreboardUpdated;

        [Header("Match Settings")]
        public bool enableMatchEnd = false;
        public int killsToWin = 0;
        public float spawnRadius = 35f;
        public int maxSlots = 8;

        [Header("Prefabs")]
        public GameObject networkPlayerShipPrefab;

        public NetworkList<NetworkScoreEntry> Scoreboard { get; private set; }

        public NetworkVariable<bool> MatchEnded = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<FixedString32Bytes> WinnerName = new NetworkVariable<FixedString32Bytes>(
            new FixedString32Bytes(""),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            Scoreboard = new NetworkList<NetworkScoreEntry>(
                null,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            Scoreboard.OnListChanged += HandleScoreboardListChanged;

            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += HandleServerClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += HandleServerClientDisconnected;

                // Register host/server as first player
                RegisterPlayer(NetworkManager.Singleton.LocalClientId, "HostPilot", "scout");
            }
        }

        public override void OnNetworkDespawn()
        {
            Scoreboard.OnListChanged -= HandleScoreboardListChanged;

            if (IsServer && NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleServerClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleServerClientDisconnected;
            }

            base.OnNetworkDespawn();
        }

        private void HandleScoreboardListChanged(NetworkListEvent<NetworkScoreEntry> changeEvent)
        {
            OnScoreboardUpdated?.Invoke();
        }

        private void HandleServerClientConnected(ulong clientId)
        {
            if (!IsServer) return;

            // Prevent duplicate entries
            bool exists = false;
            for (int i = 0; i < Scoreboard.Count; i++)
            {
                if (Scoreboard[i].ClientId == clientId)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                string defaultName = clientId == 0 ? "HostPilot" : $"Pilot_{clientId}";
                RegisterPlayer(clientId, defaultName, "scout");
            }
        }

        private void HandleServerClientDisconnected(ulong clientId)
        {
            if (!IsServer) return;

            for (int i = 0; i < Scoreboard.Count; i++)
            {
                if (Scoreboard[i].ClientId == clientId)
                {
                    Scoreboard.RemoveAt(i);
                    break;
                }
            }
        }

        public void RegisterPlayer(ulong clientId, string playerName, string shipId)
        {
            if (!IsServer) return;

            NetworkScoreEntry entry = new NetworkScoreEntry
            {
                ClientId = clientId,
                PlayerName = new FixedString32Bytes(playerName),
                ShipId = new FixedString32Bytes(shipId),
                Kills = 0,
                Deaths = 0,
                Score = 0
            };

            Scoreboard.Add(entry);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SubmitPilotProfileServerRpc(ulong clientId, FixedString32Bytes pilotName, FixedString32Bytes shipId)
        {
            if (!IsServer) return;

            for (int i = 0; i < Scoreboard.Count; i++)
            {
                if (Scoreboard[i].ClientId == clientId)
                {
                    var entry = Scoreboard[i];
                    entry.PlayerName = pilotName;
                    entry.ShipId = shipId;
                    Scoreboard[i] = entry;
                    break;
                }
            }

            // Also update the spawned player ship if present
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                if (client.PlayerObject != null)
                {
                    var playerShip = client.PlayerObject.GetComponent<NetworkPlayerShip>();
                    if (playerShip != null)
                    {
                        playerShip.NetworkPlayerName.Value = pilotName;
                        playerShip.NetworkShipId.Value = shipId;
                    }
                }
            }
        }

        public Vector2 GetSpawnPoint(ulong clientId, int totalSlots = 8, float radius = 35f)
        {
            int slots = totalSlots > 0 ? totalSlots : maxSlots;
            float r = radius > 0 ? radius : spawnRadius;
            float angle = (clientId % (ulong)Mathf.Max(1, slots)) * (2f * Mathf.PI / slots);
            return new Vector2(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r);
        }

        public void RecordKill(ulong killerClientId, ulong victimClientId)
        {
            if (!IsServer || (enableMatchEnd && MatchEnded.Value)) return;

            FixedString32Bytes killerName = "Unknown";
            FixedString32Bytes victimName = "Unknown";
            int killerKills = 0;

            // Update victim
            for (int i = 0; i < Scoreboard.Count; i++)
            {
                if (Scoreboard[i].ClientId == victimClientId)
                {
                    var entry = Scoreboard[i];
                    entry.Deaths += 1;
                    victimName = entry.PlayerName;
                    Scoreboard[i] = entry;
                    break;
                }
            }

            // Update killer
            for (int i = 0; i < Scoreboard.Count; i++)
            {
                if (Scoreboard[i].ClientId == killerClientId)
                {
                    var entry = Scoreboard[i];
                    entry.Kills += 1;
                    entry.Score += 100;
                    killerKills = entry.Kills;
                    killerName = entry.PlayerName;
                    Scoreboard[i] = entry;
                    break;
                }
            }

            BroadcastKillClientRpc(killerName, victimName);

            // Check victory condition only if match ending is enabled
            if (enableMatchEnd && killsToWin > 0 && killerKills >= killsToWin)
            {
                MatchEnded.Value = true;
                WinnerName.Value = killerName;
                BroadcastMatchOverClientRpc(killerName);
            }
        }

        [ClientRpc]
        private void BroadcastKillClientRpc(FixedString32Bytes killer, FixedString32Bytes victim)
        {
            string k = killer.ToString();
            string v = victim.ToString();
            OnKillFeed?.Invoke(k, v);
            AudioService.Instance?.PlaySound("laser_fire", 0.5f);
        }

        [ClientRpc]
        private void BroadcastMatchOverClientRpc(FixedString32Bytes winner)
        {
            string w = winner.ToString();
            OnMatchOver?.Invoke(w);
            AudioService.Instance?.PlaySound("upgrade_acquired", 1.0f);
        }

        public void DisconnectAndReturnToMenu()
        {
            if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer))
            {
                NetworkManager.Singleton.Shutdown();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameMode(GameMode.PvE);
                GameManager.Instance.ChangeState(GameState.Menu);
            }
        }
    }
}

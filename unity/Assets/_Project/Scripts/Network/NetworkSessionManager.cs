using System;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace StarStrike.Network
{
    public class NetworkSessionManager : MonoBehaviour
    {
        private static NetworkSessionManager instance;
        public static NetworkSessionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<NetworkSessionManager>();
                    if (instance == null)
                    {
                        var go = new GameObject("NetworkSessionManager");
                        instance = go.AddComponent<NetworkSessionManager>();
                    }
                }
                return instance;
            }
        }

        public static event Action<NetworkSessionData> OnSessionStarted;
        public static event Action<string> OnSessionFailed;
        public static event Action OnSessionEnded;

        public NetworkSessionData CurrentSession { get; private set; }
        public bool IsSessionActive => CurrentSession != null && NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient);

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        public void EnsureInfrastructure()
        {
            if (NetworkManager.Singleton == null)
            {
                var netMgrObj = new GameObject("NetworkManager", typeof(NetworkManager), typeof(UnityTransport));
                var netMgr = netMgrObj.GetComponent<NetworkManager>();
                var transport = netMgrObj.GetComponent<UnityTransport>();
                netMgr.NetworkConfig = new NetworkConfig
                {
                    NetworkTransport = transport
                };
            }

            var singleton = NetworkManager.Singleton;
            if (singleton != null && singleton.NetworkConfig != null && singleton.NetworkConfig.PlayerPrefab == null)
            {
                var shipPrefab = Resources.Load<GameObject>("Prefabs/Network/NetworkPlayerShip");
                var laserPrefab = Resources.Load<GameObject>("Prefabs/Network/NetworkLaserProjectile");
                if (shipPrefab != null)
                {
                    singleton.NetworkConfig.PlayerPrefab = shipPrefab;
                    singleton.NetworkConfig.Prefabs.Add(new NetworkPrefab { Prefab = shipPrefab });
                }
                if (laserPrefab != null)
                {
                    singleton.NetworkConfig.Prefabs.Add(new NetworkPrefab { Prefab = laserPrefab });
                }
            }

            if (NetworkMatchManager.Instance == null)
            {
                var matchMgrObj = new GameObject("NetworkMatchManager", typeof(NetworkMatchManager), typeof(NetworkObject));
                var netObj = matchMgrObj.GetComponent<NetworkObject>();
                if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer && !netObj.IsSpawned)
                {
                    netObj.Spawn();
                }
            }
        }

        public bool CreateSession(string hostName, int maxPlayers = 8, ushort port = 7777)
        {
            EnsureInfrastructure();

            if (NetworkManager.Singleton == null)
            {
                OnSessionFailed?.Invoke("NetworkManager failed to initialize.");
                return false;
            }

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.SetConnectionData("127.0.0.1", port);
            }

            bool success = NetworkManager.Singleton.StartHost();
            if (success)
            {
                string joinCode = NetworkSessionData.GenerateJoinCode(port);
                CurrentSession = new NetworkSessionData
                {
                    SessionId = Guid.NewGuid().ToString("N"),
                    JoinCode = joinCode,
                    HostPlayerName = hostName,
                    MaxPlayers = maxPlayers,
                    CurrentPlayers = 1,
                    HostAddress = "127.0.0.1",
                    Port = port,
                    IsRelay = false,
                    CreatedAtUtc = DateTime.UtcNow
                };

                OnSessionStarted?.Invoke(CurrentSession);
                return true;
            }

            OnSessionFailed?.Invoke($"Failed to host session on port {port}.");
            return false;
        }

        public bool JoinSessionByCode(string joinCode, string pilotName)
        {
            EnsureInfrastructure();

            if (!NetworkSessionData.TryParseJoinCode(joinCode, out string host, out ushort port))
            {
                OnSessionFailed?.Invoke($"Invalid session code '{joinCode}'.");
                return false;
            }

            if (NetworkManager.Singleton == null)
            {
                OnSessionFailed?.Invoke("NetworkManager failed to initialize.");
                return false;
            }

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.SetConnectionData(host, port);
            }

            bool success = NetworkManager.Singleton.StartClient();
            if (success)
            {
                CurrentSession = new NetworkSessionData
                {
                    SessionId = Guid.NewGuid().ToString("N"),
                    JoinCode = joinCode.ToUpperInvariant(),
                    HostPlayerName = "Host",
                    MaxPlayers = 8,
                    CurrentPlayers = 1,
                    HostAddress = host,
                    Port = port,
                    IsRelay = false,
                    CreatedAtUtc = DateTime.UtcNow
                };

                OnSessionStarted?.Invoke(CurrentSession);
                return true;
            }

            OnSessionFailed?.Invoke($"Failed to connect to session {joinCode} at {host}:{port}.");
            return false;
        }

        public void LeaveSession()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
            }
            CurrentSession = null;
            OnSessionEnded?.Invoke();
        }
    }
}

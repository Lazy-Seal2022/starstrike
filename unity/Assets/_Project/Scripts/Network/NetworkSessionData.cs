using System;
using UnityEngine;

namespace StarStrike.Network
{
    [Serializable]
    public class NetworkSessionData
    {
        public string SessionId;
        public string JoinCode;
        public string HostPlayerName;
        public int MaxPlayers;
        public int CurrentPlayers;
        public string HostAddress;
        public ushort Port;
        public bool IsRelay;
        public DateTime CreatedAtUtc;

        public static string GenerateJoinCode(ushort port = 7777)
        {
            // Human-friendly 4-digit token prefixed with STAR- (e.g., STAR-4819)
            int randomToken = UnityEngine.Random.Range(1000, 9999);
            return $"STAR-{randomToken}";
        }

        public static bool TryParseJoinCode(string code, out string hostAddress, out ushort port)
        {
            hostAddress = "127.0.0.1";
            port = 7777;

            if (string.IsNullOrWhiteSpace(code)) return false;

            code = code.Trim().ToUpperInvariant();

            // Direct IP:port format (e.g., 192.168.1.5:7777)
            if (code.Contains(":") && !code.StartsWith("STAR-"))
            {
                string[] parts = code.Split(':');
                if (parts.Length == 2)
                {
                    hostAddress = parts[0];
                    if (ushort.TryParse(parts[1], out ushort parsedPort))
                    {
                        port = parsedPort;
                        return true;
                    }
                }
            }

            // Direct IP only format (e.g. 192.168.1.5 or 127.0.0.1)
            if (System.Net.IPAddress.TryParse(code, out _))
            {
                hostAddress = code;
                port = 7777;
                return true;
            }

            // Session Code format (STAR-XXXX or XXXX)
            if (code.StartsWith("STAR-") || code.Length >= 4)
            {
                // In local / peer-relay test topology, maps to local loopback port
                hostAddress = "127.0.0.1";
                port = 7777;
                return true;
            }

            return false;
        }
    }
}

namespace StarStrike.Core
{
    public enum GameMode
    {
        PvE = 0,        // Single-Player Survival & Exploration (Asteroids, Drones, Boss)
        PvP = 1,        // Online / LAN Multiplayer Dogfight Combat
        PvPArena = PvP  // Backward-compatible alias
    }
}

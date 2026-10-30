namespace Racing.Networking.Protocol
{
    /// <summary>
    /// Identifies the payload carried by a NetworkEnvelope.
    /// </summary>
    public enum MessageType
    {
        JoinRequest,
        JoinAccepted,
        PlayerSpawn,
        PlayerDespawn,
        InputCommand,
        WorldSnapshot,
        RaceEvent,
        Ping,
        Pong,
        Leave
    }
}

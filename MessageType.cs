namespace Margatroid.Alice
{
    public enum MessageType : ushort
    {
        // register pair
        Register,
        RegisterSuccess,
        // tunnel pair
        Tunnel,
        TunnelSuccess,
        // heartbeat -> { heartbeat, expired }
        HeartBeat,
        SessionExpired,
        // session broadcast pair
        SessionBroadcast,
        SessionBroadCastSuccess,
        // session query -> { exists, not found }
        SessionQuery,
        SessionExists,
        SessionNotFound
    }
}

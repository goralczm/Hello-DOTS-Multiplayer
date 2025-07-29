using System;

public static class NetworkEvents
{
    public class LocalClientConnectedEventArgs : EventArgs
    {
        public string IPv4;
        public ushort Port;
    }

    public static EventHandler<LocalClientConnectedEventArgs> s_OnLocalClientConnected;
    public static Action<int> s_OnClientConnected;
    public static Action<int> s_OnClientDisconnected;
}
using System;

public static class NetworkEvents
{
    /// <summary>
    /// Called on the client side when the connection to the server has been established.
    /// </summary>
    public static EventHandler<ClientConnectedLocalEventArgs> s_OnClientConnectedLocal;
    
    /// <summary>
    /// Called on the server when new client has connected.
    /// </summary>
    public static Action<int> s_OnClientConnected;
    
    /// <summary>
    /// Called on the server when the client has disconnected.
    /// </summary>
    public static Action<int> s_OnClientDisconnected;
}
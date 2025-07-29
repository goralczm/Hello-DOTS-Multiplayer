using System;

public class ClientConnectedLocalEventArgs : EventArgs
{
    public string IPv4;
    public ushort Port;
}
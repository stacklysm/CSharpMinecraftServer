using Core.Types;

namespace Core.Packets;

[BoundTo(PacketDestination.Server)]
public struct Handshake : INetworkType
{
    private Packet Packet;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0251:Make member 'readonly'", Justification = "Length can be modified internally by the class instance.")]
    private int DataLength => ProtocolVersion.Length + ServerAddress.Length + sizeof(ushort) + NextState.Length;

    public VarInt ProtocolVersion { get; set; }

    public VarString ServerAddress { get; set; }

    public ushort ServerPort { get; set; }

    public VarInt NextState { get; set; }

    public Handshake(VarInt protocolVersion, VarString serverAddress, ushort serverPort, VarInt nextState)
    {
        ProtocolVersion = protocolVersion;
        ServerAddress = serverAddress;
        ServerPort = serverPort;
        NextState = nextState;
        Packet = new(new VarInt(DataLength), new VarInt(0));
    }

    public byte[] GetBytes()
    {
        byte[] protocolVersion = ProtocolVersion.GetBytes();
        byte[] serverAddress = ServerAddress.GetBytes();
        byte[] serverPort = BitConverter.GetBytes(ServerPort).Reverse().ToArray();
        byte[] nextState = NextState.GetBytes();

        byte[] buffer = new byte[DataLength];

        Array.Copy(protocolVersion, 0, buffer, 0, protocolVersion.Length);
        Array.Copy(serverAddress, 0, buffer, protocolVersion.Length, serverAddress.Length);
        Array.Copy(serverPort, 0, buffer, protocolVersion.Length + serverAddress.Length, serverPort.Length);
        Array.Copy(nextState, 0, buffer, protocolVersion.Length + serverAddress.Length + serverPort.Length, nextState.Length);

        return buffer;
    }

    public void ReadFromStream(Stream stream)
    {
        Packet packet = Packet.FromStream(stream);

        Packet = packet;

        MemoryStream dataStream = new(packet.Data);

        ProtocolVersion = VarInt.FromStream(dataStream);
        ServerAddress = VarString.FromStream(dataStream);

        byte[] serverPortBuffer = new byte[sizeof(ushort)];
        dataStream.Read(serverPortBuffer, 0, sizeof(ushort));

        ServerPort = BitConverter.ToUInt16(serverPortBuffer);
        NextState = VarInt.FromStream(dataStream);
    }

    public void WriteToStream(Stream stream)
    {
        Packet.WriteToStream(stream);
        stream.Write(GetBytes());
    }
}

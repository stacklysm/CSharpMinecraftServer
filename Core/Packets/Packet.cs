using Core.Types;

namespace Core.Packets;

public readonly struct Packet : INetworkType
{
    public VarInt Length { get; }

    public VarInt Id { get; }

    public byte[] Data { get; }

    public Packet(VarInt length, VarInt id)
    {
        Length = length;
        Id = id;
        Data = new byte[length.AsInteger()];
    }

    public byte[] GetBytes()
    {
        byte[] lengthData = Length.GetBytes();
        byte[] idData = Id.GetBytes();
        byte[] buffer = new byte[lengthData.Length + idData.Length + Data.Length];

        Array.Copy(lengthData, 0, buffer, 0, lengthData.Length);
        Array.Copy(idData, 0, buffer, lengthData.Length, idData.Length);
        Array.Copy(Data, 0, buffer, lengthData.Length + idData.Length, Data.Length);

        return buffer;
    }
}

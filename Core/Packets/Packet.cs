using Core.Types;

namespace Core.Packets;

public class Packet : INetworkType
{
    public VarInt Length { get; private set; }

    public VarInt Id { get; private set; }

    public byte[] Data { get; private set; }

    public Packet(VarInt length, VarInt id)
    {
        Length = length;
        Id = id;
        Data = new byte[length.AsInteger() - id.Length];
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

    public void ReadFromStream(Stream stream)
    {
        Length = VarInt.FromStream(stream);
        Id = VarInt.FromStream(stream);

        int bufferLength = Length.AsInteger() - Id.Length;
        byte[] dataBuffer = new byte[bufferLength];

        int bytesRead = stream.Read(dataBuffer, 0, bufferLength);

        if (bytesRead != bufferLength)
        {
            throw new InvalidOperationException($"Failed to read packet data. Expected length: {bufferLength}, bytes read: {bytesRead}");
        }

        Data = dataBuffer;
    }

    public void WriteToStream(Stream stream)
    {
        Length.WriteToStream(stream);
        Id.WriteToStream(stream);
        stream.Write(Data, 0, Data.Length);
    }
}

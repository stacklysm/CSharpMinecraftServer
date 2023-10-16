using System.Text;

namespace Core.Types;

public struct VarString : INetworkType
{
    public string Value { get; set; }

    public int Length { get; private set; }

    public VarString(string value)
    {
        Value = value;
        Length = value.Length;
    }

    public static VarString FromStream(Stream stream)
    {
        int length = VarInt.FromStream(stream).AsInteger();

        if (length < 0)
        {
            throw new InvalidOperationException("Invalid data format, string length was negative.");
        }

        byte[] buffer = new byte[length];

        int bytesRead = stream.Read(buffer, 0, length);

        if (bytesRead != length)
        {
            throw new InvalidOperationException($"Failed to read string data. Expected length: {length}, bytes read: {bytesRead}");
        }

        return new(Encoding.UTF8.GetString(buffer));
    }

    public readonly byte[] GetBytes()
    {
        byte[] stringLengthData = new VarInt(Length).GetBytes();
        byte[] stringData = Encoding.UTF8.GetBytes(Value);
        byte[] buffer = new byte[stringLengthData.Length + stringData.Length];

        Array.Copy(stringLengthData, 0, buffer, 0, stringLengthData.Length);
        Array.Copy(stringData, 0, buffer, stringLengthData.Length, stringData.Length);

        return buffer;
    }

    public void ReadFromStream(Stream stream)
    {
        int length = VarInt.FromStream(stream).AsInteger();

        if (length < 0)
        {
            throw new InvalidOperationException("Invalid data format, string length was negative.");
        }

        byte[] buffer = new byte[length];

        int bytesRead = stream.Read(buffer, 0, length);

        if (bytesRead != length)
        {
            throw new InvalidOperationException($"Failed to read string data. Expected length: {length}, bytes read: {bytesRead}");
        }

        Length = length;
        Value = Encoding.UTF8.GetString(buffer);
    }

    public readonly void WriteToStream(Stream stream)
    {
        VarInt length = new(Length);
        byte[] data = Encoding.UTF8.GetBytes(Value);

        length.WriteToStream(stream);
        stream.Write(data, 0, data.Length);
    }
}

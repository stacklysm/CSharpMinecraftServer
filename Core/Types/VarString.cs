using System.Text;

namespace Core.Types;

public class VarString : INetworkType
{
    public string Value { get; set; }

    public VarString(string value)
    {
        Value = value;
    }

    public byte[] GetBytes()
    {
        byte[] stringLengthData = new VarInt(Value.Length).GetBytes();
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
            throw new InvalidOperationException($"Failed to read string. Expected length: {length}, bytes read: {bytesRead}");
        }

        Value = Encoding.UTF8.GetString(buffer);
    }

    public void WriteToStream(Stream stream)
    {
        byte[] data = Encoding.UTF8.GetBytes(Value);
        VarInt length = new(data.Length);

        length.WriteToStream(stream);
        stream.Write(data, 0, data.Length);
    }
}

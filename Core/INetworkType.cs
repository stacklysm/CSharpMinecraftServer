namespace Core;

public interface INetworkType
{
    byte[] GetBytes();

    void ReadFromStream(Stream stream);

    void WriteToStream(Stream stream);
}

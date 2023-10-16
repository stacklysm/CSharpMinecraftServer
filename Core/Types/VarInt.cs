namespace Core.Types;

public class VarInt : INetworkType
{
    private const int VARINT_MAX_ARRAY_ELEMENT_COUNT = 5;
    private const int LEAST_SIGNIFICANT_BIT_OFFSET = 7;
    private const int MAX_READER_POSITION_OFFSET = 32;
    private const int SEGMENT_BITS = 0x7f;
    private const int CONTINUE_BIT = 0x80;
    private const int END_OF_STREAM = -1;

    private readonly byte[] InternalBuffer;
    private int InternalValue;

    public int Length { get; private set; }

    public VarInt(int value = 0)
    {
        InternalBuffer = new byte[VARINT_MAX_ARRAY_ELEMENT_COUNT];
        InternalValue = value;
    }

    public static VarInt FromStream(Stream stream)
    {
        int value = 0;
        int position = 0;
        int read;
        int currentIndex = 0;

        while ((read = stream.ReadByte()) != END_OF_STREAM)
        {
            byte currentByte = (byte)read;

            value |= (currentByte & SEGMENT_BITS) << position;

            if ((currentByte & CONTINUE_BIT) == 0)
            {
                break;
            }

            position += LEAST_SIGNIFICANT_BIT_OFFSET;

            if (position >= MAX_READER_POSITION_OFFSET)
            {
                throw new InvalidOperationException("VarInt exceeded the 32-bit signed integer value limit.");
            }

            currentIndex++;
        }

        return new(value);
    }

    public byte[] GetBytes()
    {
        return InternalBuffer[0..Length];
    }

    public void ReadFromStream(Stream stream)
    {
        int value = 0;
        int position = 0;
        int read;
        int currentIndex = 0;

        while ((read = stream.ReadByte()) != END_OF_STREAM)
        {
            byte currentByte = (byte)read;
            InternalBuffer[currentIndex] = currentByte;

            value |= (currentByte & SEGMENT_BITS) << position;

            if ((currentByte & CONTINUE_BIT) == 0)
            {
                break;
            }

            position += LEAST_SIGNIFICANT_BIT_OFFSET;

            if (position >= MAX_READER_POSITION_OFFSET)
            {
                throw new InvalidOperationException("VarInt exceeded the 32-bit signed integer value limit.");
            }

            currentIndex++;
        }

        Length = currentIndex + 1;
        InternalValue = value;
    }

    public void WriteToStream(Stream stream)
    {
        uint castedValue = (uint)InternalValue;

        while (true)
        {
            if ((castedValue & ~SEGMENT_BITS) == 0)
            {
                stream.WriteByte((byte)castedValue);
                break;
            }

            stream.WriteByte((byte)((castedValue & SEGMENT_BITS) | CONTINUE_BIT));
            castedValue >>= LEAST_SIGNIFICANT_BIT_OFFSET;
        }
    }

    public int AsInteger()
    {
        return InternalValue;
    }
}

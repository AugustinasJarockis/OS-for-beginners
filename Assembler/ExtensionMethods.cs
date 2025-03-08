namespace Assembler;

public static class ExtensionMethods
{
    public static byte[] ToBytes(this int value)
    {
        var bytes = BitConverter.GetBytes(value);
        return BitConverter.IsLittleEndian ? bytes.Reverse().ToArray() : bytes;
    }
}
namespace OperatingSystem.Hardware.Constants;

public static class InterruptCodes
{
    public const byte DivByZero = 0;
    public const byte KeyboardInput = 1;
    public const byte InvalidOpCode = 2;
    public const byte PageFault = 3;
    public const byte TerminalOutput = 4;
    public const byte PeriodicInterrupt = 5;
    public const byte WriteToExternalStorage = 6;
    public const byte ReadFromExternalStorage = 7;
    public const byte GetFileHandle = 8;
    public const byte ReleaseFileHandle = 9;
    public const byte DeleteFile = 10;
    public const byte Halt = 13;
    public const byte ReadKeyboardInput = 14;
}
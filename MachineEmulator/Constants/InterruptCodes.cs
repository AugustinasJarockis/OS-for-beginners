namespace MachineEmulator.Constants;

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
}
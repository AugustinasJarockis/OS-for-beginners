namespace MachineEmulator.Constants;

public static class InterruptCodes
{
    public const byte InvalidOpCode = 2;
    public const byte PageFault = 3;
    public const byte TerminalOutput = 4;
    public const byte PeriodicInterrupt = 5;
}
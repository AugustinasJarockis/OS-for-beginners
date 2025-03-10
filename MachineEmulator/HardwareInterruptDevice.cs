namespace MachineEmulator;

public class HardwareInterruptDevice
{
    private readonly Stack<byte> _interrupts = new();

    public bool IsInterrupted()
    {
        return _interrupts.Count > 0;
    }

    public void Interrupt(byte interruptCode)
    {
        _interrupts.Push(interruptCode);
    }

    public byte GetInterruptCode()
    {
        return _interrupts.Pop();
    }
}
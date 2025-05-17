namespace OperatingSystem.Hardware;

public class HardwareInterruptDevice
{
    private readonly Stack<byte> _interrupts = new();
    private readonly Lock _lock = new();

    public void Interrupt(byte interruptCode)
    {
        lock (_lock)
        {
            if (!_interrupts.Contains(interruptCode))
            {
                _interrupts.Push(interruptCode);
            }
        }
    }

    public byte? TryGetInterruptCode()
    {
        lock (_lock)
        {
            return _interrupts.TryPop(out var code) ? code : null;
        }
    }
}
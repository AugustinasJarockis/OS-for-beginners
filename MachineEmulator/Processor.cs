using System.Diagnostics.CodeAnalysis;

namespace MachineEmulator;

public class Processor
{
    //pc, fr, sp, ptbr, r0, r1, r2, r3, r4, r5, r6, r7;
    public List<int> registers = new(8);

    private readonly RAM _ram;
    
    public Processor(RAM ram)
    {
        _ram = ram;
    }

    [DoesNotReturn]
    public void Run()
    {
        while (true)
        {
            var instruction = FetchInstruction();
            // TODO: Decode
            // TODO: Execute
        }
    }

    private int FetchInstruction()
    {
        throw new NotImplementedException();
    }
}
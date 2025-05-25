using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OperatingSystem.Hardware.Constants;

namespace OperatingSystem.Hardware;

public static class HardwareDevices
{
    public static void WatchTerminalOutput(RAM ram)
    {
        new Thread(() =>
        {
            while (true) 
            {
                var terminalValue = ram.GetByte(MemoryLocations.TerminalOutput);
                if (terminalValue != 0)
                {
                    Console.Write((char)terminalValue);
                    ram.SetByte(MemoryLocations.TerminalOutput, 0);
                }
            }
        }).Start();
    }

    public static void WatchKeyboardInput(RAM ram, Action onInput)
    {
        new Thread(() =>
        {
            while (true)
            {
                var key = Console.ReadKey();
                ram.SetByte(MemoryLocations.KeyboardInput, (byte)key.KeyChar);
                onInput();
            }
        }).Start();
    }

    public static void WatchWriteToExternalStorage(RAM ram, ExternalStorage externalStorage)
    {
        new Thread(() =>
        {
            while (true)
            {
                var indicatorByte = ram.GetByte(0x12003);
                if ((indicatorByte & 1) == 0)
                    continue;

                var blockNumber = ram.GetDWord(0x12000) & 0xFFFFFFFE;
                var data = new uint[ExternalStorage.BLOCK_SIZE];
                for (var i = 0; i < ExternalStorage.BLOCK_SIZE; i++)
                    data[i] = ram.GetDWord((uint)(0x12004 + i));

                externalStorage.WriteBlock(blockNumber, data);
                ram.SetByte(0x12003, (byte)(indicatorByte - 1));
            }
        }).Start();
    }

    public static void WatchReadFromExternalStorage(RAM ram, ExternalStorage externalStorage)
    {
        new Thread(() =>
        {
            while (true)
            {
                var indicatorByte = ram.GetByte(0x13007);
                if ((indicatorByte & 1) == 0)
                    continue;

                var blockNumber = ram.GetDWord(0x13004) & 0xFFFFFFFE;
                var data = externalStorage.ReadBlock(blockNumber);
                for (var i = 0; i < data.Length; i++)
                    ram.SetDWord((uint)(0x13008 + i), data[i]);

                ram.SetByte(0x13007, (byte)(indicatorByte - 1));
            }
        }).Start();
    }
    
    public static void RunPeriodicInterruptTimer(TimeSpan interval, Action func)
    {
        new Thread(() =>
        {
            while (true)
            {
                Thread.Sleep(interval);
                func();
            }
        }).Start();
    }
    
    public static void TrackTime(RAM ram)
    {
        new Thread(() =>
        {
            var stopwatch = Stopwatch.StartNew();
            while (true)
            {
                ram.SetDWord(MemoryLocations.Time, (uint)stopwatch.Elapsed.TotalMilliseconds);
            }
        }).Start();
    }
}
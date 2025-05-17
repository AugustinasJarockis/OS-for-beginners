using OperatingSystem.Hardware.Enums;

namespace OperatingSystem.Hardware.Operations
{
    class FlagUtils
    {
        public static void AdjustResultFlags(Processor proc, long result) {
            if (result > Int32.MaxValue)
                proc.Registers[(int)Register.FR] |= 0b1;
            else
                proc.Registers[(int)Register.FR] -= (proc.Registers[(int)Register.FR] & 0b1);

            AdjustZeroAndSignFlags(proc, result);
        }

        public static void AdjustZeroAndSignFlags(Processor proc, long result) {
            if (result == 0)
                proc.Registers[(int)Register.FR] |= 0b10;
            else
                proc.Registers[(int)Register.FR] -= (proc.Registers[(int)Register.FR] & 0b10);

            AdjustSignFlag(proc, result);
        }

        public static void AdjustSignFlag(Processor proc, long result) {
            if (result < 0)
                proc.Registers[(int)Register.FR] |= 0b1000;
            else
                proc.Registers[(int)Register.FR] -= (proc.Registers[(int)Register.FR] & 0b1000);
        }

        public static void SetModeFlag(Processor proc)
        {
            proc.Registers[(int)Register.FR] |= 0b0100;
        }
        
        public static void ClearModeFlag(Processor proc)
        {
            proc.Registers[(int)Register.FR] -= proc.Registers[(int)Register.FR] & 0b0100;
        }
    }
}

namespace OperatingSystem.ProcessManagement
{
    public abstract class ProcessProgram
    {
        protected int CurrentStep = 0;

        private bool IsInterrupted() {
            return false; //TODO: implement interrupt service
        }

        private bool IsBlocked() {
            return false; //TODO: implement a way to check if resource is blocked
        }

        public void Proceed() {
            while (!IsInterrupted() && !IsBlocked()) {
                CurrentStep = Next();
            }
        }

        protected abstract int Next();
    }
}

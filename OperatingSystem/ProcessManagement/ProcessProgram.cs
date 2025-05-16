namespace OperatingSystem.ProcessManagement
{
    public abstract class ProcessProgram
    {
        protected int currentStep = 0;

        private bool IsInterrupted() {
            throw new NotImplementedException(); //TODO: implement interrupt service
        }

        private bool IsBlocked() {
            throw new NotImplementedException(); //TODO: implement a way to check if resource is blocked
        }

        public void Proceed() {
            while (!IsInterrupted() && !IsBlocked()) {
                _proceed();
            }
        }

        protected abstract void _proceed();
    }
}

using OperatingSystem.ProcessManagement;

namespace OperatingSystem.Utilities
{
    internal class ProcessPriorityQueue {
        private readonly List<Process> queue;

        public ProcessPriorityQueue() {
            queue = new();
        }
        public bool Contains(Process process) => queue.Contains(process);

        public void Enqueue(Process process) {
            process.Priority = process.BasePriority;
            
            if (queue.Count == 0) {
                queue.Add(process);
                return;
            }

            for (int i = 0; i < queue.Count; i++) {
                if (queue[i].Priority >= process.Priority || i == queue.Count - 1) {
                    queue.Insert(i, process);
                    break;
                }
            }
        }

        public Process Dequeue() {
            Process removedProcess = queue.Last();
            queue.Remove(queue.Last());
            return removedProcess;
        }

        public void Remove(Process process) {
            queue.Remove(process);
        }

        public void RemoveAllNotReady() {
            for (int i = 0; i < queue.Count; i++) {
                if (queue[i].State != ProcessState.Ready) {
                    queue.Remove(queue[i]);
                    i--;
                }
            }
        }

        public void IncrementPriorities() {
            for (int i = 0; i < queue.Count; i++) {
                if (queue[i].Priority < 255) {
                    queue[i].Priority++;
                }
            }
        }
    }
}

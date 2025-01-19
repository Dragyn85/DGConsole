using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DragynGames.Console
{
    public class ConsoleEntryHistoryTracker
    {
        private Queue<ConsoleCommand> consoleCommands = new Queue<ConsoleCommand>();
        int size = 10;
        
        int currentSelectionIndex = -1;
        
        public void AddEntry(ConsoleCommand consoleCommand)
        {
            consoleCommands.Enqueue(consoleCommand);
            if (consoleCommands.Count > size)
            {
                consoleCommands.Dequeue();
            }
        }
        
        public bool GetPreviousEntry(out ConsoleCommand consoleCommand)
        {
            currentSelectionIndex++;
            if (currentSelectionIndex >= consoleCommands.Count)
            {
                currentSelectionIndex = -1;
                consoleCommand = new ConsoleCommand();
                return false;
            }
            consoleCommand = consoleCommands.ElementAt(currentSelectionIndex);
            return true;
        }

        public bool HasEntries()
        {
            return consoleCommands.Count > 0;
        }

        public bool GetNextEntry(out ConsoleCommand consoleCommand)
        { 
            currentSelectionIndex--;
            if (currentSelectionIndex < -1)
            {
                currentSelectionIndex = consoleCommands.Count - 1;
            }
            if (currentSelectionIndex == -1)
            {
                consoleCommand = new ConsoleCommand();
                return false;
            }
            consoleCommand = consoleCommands.ElementAt(currentSelectionIndex);
            return true;
        }
        
        public void ResetHistorySelection()
        {
            currentSelectionIndex = -1;
        }
    }
}
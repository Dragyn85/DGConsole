using System;
using System.Collections.Generic;
using UnityEngine;

namespace DragynGames.Commands
{
    public class ConsoleBuiltInActions
    {
        private readonly CommandManager commandManager;
        int maxMethodMatchingResults = 5;

        public ConsoleBuiltInActions(CommandManager commandManager)
        {
            this.commandManager = commandManager;
        }
        public void AddHelpCommands()
        {
            commandManager.AddCommand("Help", "Logs all available commands",this, new Func<string>(LogAllCommands));
            commandManager.AddCommand("HelpWith", 
                "Logs all available commands that are either equal to commandName or contain commandName as substring", 
                this,
                new Func<string, int,string>(LogAllCommandsWithName));
            
            commandManager.AddCommand("HelpWith", 
                "Logs all available commands that are either equal to commandName or contain commandName as substring", 
                this,
                new Func<string,string>(LogAllCommandsWithName));
            
            commandManager.AddCommand("SystemInfo", "Logs system information", this, new Func<string>(LogSystemInfo));
        }

        public string LogAllCommands()
        {
            List<CommandInfo> methods = commandManager.GetMethods();
            string allCommands = LogHelpMethods.LogAllCommands(methods);
            return allCommands;
        }

        // Logs the list of available commands that are either equal to commandName or contain commandName as substring
        internal string LogAllCommandsWithName(string commandName, int maxResults)
        {
            maxMethodMatchingResults = maxResults;
            List<CommandInfo> matchingMethods = new List<CommandInfo>(maxResults);
            string AllMatchingMethods =
                LogHelpMethods.LogAllCommandsWithName(commandName, CachedMethodFinder.FindCommands, commandManager.GetMethods(),commandManager._settings.caseInsensitiveComparer,maxResults);

            return AllMatchingMethods;
        }
        internal string LogAllCommandsWithName(string commandName)
        {
            string results = LogAllCommandsWithName(commandName, maxMethodMatchingResults);
            return results;
        }

        // Logs system information
        internal string LogSystemInfo()
        {
            return SystemInfoLogger.LogSystemInfo();
        }
    }
}
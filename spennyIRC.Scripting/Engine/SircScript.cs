using spennyIRC.Core.IRC;
using spennyIRC.Scripting.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace spennyIRC.Scripting.Engine
{
    public abstract class SircScript(IIrcCommands commands) : ICSharpScript
    {
        protected readonly IIrcCommands _commands = commands;
        private readonly Dictionary<string, Func<string, IIrcSession, Task>> _registeredCommands = [];
        private readonly Dictionary<string, Func<IIrcReceivedContext, Task>> _events = [];
        // TODO: add context menu support

        public abstract string Name { get; }
        public virtual string Version { get; } = "1.0";
        public virtual string Author { get; } = "Unspecified";
        public virtual string Description { get; } = string.Empty;

        public abstract void Execute();
        public abstract void Initialize();
        public virtual void Shutdown()
        {
            RemoveAllCommands();
        }

        protected void AddCommand(string name, string description, Func<string, IIrcSession, Task> command)
        {
            if (!_registeredCommands.ContainsKey(name) && _commands.AddCommand(name, description, command))
                _registeredCommands[name] = command;
        }

        protected void AddEvent(string eventName, Func<IIrcReceivedContext, Task> handler)
        {
            if (_events.ContainsKey(eventName))
            {
                _events[eventName] += handler;
                return;
            } 

            _events[eventName] = handler;
        }

        protected void RemoveAllCommands()
        {
            foreach (string cmd in _registeredCommands.Keys)
                _commands.RemoveCommand(cmd);

            _registeredCommands.Clear();
        }

        protected void RemoveAllEvents()
        {
            _events.Clear();
        }

        public async Task TriggerEvents(string eventName, IIrcReceivedContext context)
        {
            if (_events.TryGetValue(eventName, out Func<IIrcReceivedContext, Task>? handler))
                await handler(context);
        }
    }
}
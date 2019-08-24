using System;
using ManyConsole;

namespace aspx2razor {

    class Program {
        private static int Main(string[] args) {
            var commands = ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));

            var result = ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
            return result;
        }
    }
}

using BT.Logging;
using System;
using System.IO;

namespace BauphysikToolWPF.Services.Application
{
    public class StartupArguments
    {
        public bool SimulateFirstStart { get; set; } = false;
        public string ProjectPath { get; set; } = string.Empty;
    }

    public static class ArgumentParser
    {
        public static StartupArguments Parse(string[] args)
        {
            var result = new StartupArguments();

            foreach (var arg in args)
            {
                if (arg.Equals("--simulateFirstStart", StringComparison.OrdinalIgnoreCase))
                {
                    result.SimulateFirstStart = true;
                }
                else if (File.Exists(arg))
                {
                    result.ProjectPath = arg;
                }
                else
                {
                    Logger.LogWarning($"Unrecognized argument ignored: {arg}");
                }
            }

            return result;
        }
    }
}

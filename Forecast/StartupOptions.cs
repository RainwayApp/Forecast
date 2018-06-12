using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace Forecast
{
    public class StartupOptions
    {
        [Option('h', "host", Required = true, HelpText = "The host you wish to verify.")]
        public string Host { get; set; }

        [Option('p', "port", Required = true, HelpText = "The port the host is listening on.")]
        public ushort Port { get; set; }

        [Option('f', "family", Required = true, HelpText = "The address family you wish to verify.")]
        public string Family { get; set; }

        [Option('b', "beautiful", Required = false, Default = true, HelpText = "Indicates if the output should be pretty printed.")]
        public bool Beautiful { get; set; }
    }
}

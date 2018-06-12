using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json;

namespace Forecast
{
    class Program
    {

        private static StartupOptions _startOptions;
        /// <summary>
        /// Transform a string into the right address family.
        /// </summary>
        /// <param name="family"></param>
        /// <returns></returns>
        private static AddressFamily GetAddressFamily(string family)
        {
            if (family.Equals("ipv4"))
            {
                return AddressFamily.InterNetwork;
            }
            if (family.Equals("ipv6"))
            {
                return AddressFamily.InterNetworkV6;
            }
            throw new InvalidOperationException("Unknown address family");
        }

        public static async Task Main(string[] args)
        {
            Parser.Default.ParseArguments<StartupOptions>(args).WithParsed(options => { _startOptions = options; });
            if (_startOptions == null)
            {
                Environment.Exit(1);
                return;
            }
            var httpWire = new HttpTether();
            Console.WriteLine(JsonConvert.SerializeObject(await httpWire.Pull(_startOptions.Host, _startOptions.Port, GetAddressFamily(_startOptions.Family.ToLower())), _startOptions.Beautiful ? Formatting.Indented : Formatting.None));
        }
    }
}

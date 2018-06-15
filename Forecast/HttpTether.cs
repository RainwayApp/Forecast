using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;
using Forecast.Models;
using Newtonsoft.Json;

namespace Forecast
{

    /// <summary>
    /// 
    /// </summary>
    public class HttpTether
    {
        /// <summary>
        /// Pings a host and returns if a succesful connection was made or not.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="family"></param>
        /// <returns></returns>
        private bool PingHost(string host, ushort port, AddressFamily family)
        {
            try
            {
                using (var client = new TcpClient(family))
                {
                    client.LingerState = new LingerOption(true, 0);
                    var result = client.BeginConnect(host, port, null, null);
                    result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                    if (!client.Connected)
                    {
                        return false;
                    }
                    client.EndConnect(result);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates a host name
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        private async Task<bool> HostValid(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                return false;
            }
            if (Uri.CheckHostName(host) == UriHostNameType.Unknown)
            {
                return false;
            }
            try
            {
                await Dns.GetHostEntryAsync(host);
                return true;
            }
            catch
            {

                return false;
            }
        }

        private List<AddressInformation> GetRecords(IReadOnlyList<DnsResourceRecord> answers, ushort port, AddressFamily family)
        {
            var records = new List<AddressInformation>();
            if (family == AddressFamily.InterNetwork)
            {
                records.AddRange(answers.ARecords()
                    .Select(answer => new AddressInformation
                    {
                        IpAddress = answer.Address.ToString(),
                        TimeToLive = answer.TimeToLive,
                        CouldPing = PingHost(answer.Address.ToString(), port, family)
                    }));
            }
            else if (family == AddressFamily.InterNetworkV6)
            {
                records.AddRange(answers.AaaaRecords()
                    .Select(answer => new AddressInformation
                    {
                        IpAddress = answer.Address.ToString(),
                        TimeToLive = answer.TimeToLive,
                        CouldPing = PingHost(answer.Address.ToString(), port, family)
                    }));
            }
            return records;
        }


        /// <summary>
        /// Runs a set of basic test on a target HTTP or WebSocket to see if things are properly configured.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="family"></param>
        /// <returns></returns>
        private async Task<HostInformation> ParseHost(string host, ushort port, AddressFamily family = AddressFamily.InterNetwork)
        {
            //Check if the host is valid
            var hostInformation = new HostInformation { HostName = host, Port = port, AddressFamily = family, AddressInformation = new List<AddressInformation>() };
            if (!await HostValid(host))
            {
                hostInformation.HostValid = false;
                return hostInformation;
            }
            var lookup = new LookupClient();
            var result = await lookup.QueryAsync(host, family == AddressFamily.InterNetwork ? QueryType.A : QueryType.AAAA);
            if (result == null)
            {
                throw new DnsResponseException("DNS Failed to parse.");
            }

            hostInformation.AddressInformation = GetRecords(result.Answers, port, family);
            if (hostInformation.AddressInformation.Count == 0)
            {
                return hostInformation;
            }

            hostInformation.HasTargetAddressFamily = true;
            //Can we reach the host?
            hostInformation.CouldPingHostName = PingHost(host, port, family);
            if (!hostInformation.CouldPingHostName)
            {
                return hostInformation;
            }
            using (var client = new TcpClient(family))
            {
                client.LingerState = new LingerOption(true, 0);
                await client.ConnectAsync(host, port);
                using (var stream = client.GetStream())
                //Check for an SSL cert on the remote host and attempt to validate it.
                using (var sslStream = new SslStream(stream, false, (o, certificate, chain, errors) => UserCertificateValidationCallback(o, certificate, chain, errors, ref hostInformation)))
                {
                    try
                    {
                        sslStream.AuthenticateAsClient(host);
                        return hostInformation;
                    }
                    catch
                    {
                        return hostInformation;
                    }
                }
            }
        }

        /// <summary>
        /// Validates an SSL certficate 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslpolicyerrors"></param>
        /// <param name="hostInformation"></param>
        /// <returns></returns>
        private bool UserCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors, ref HostInformation hostInformation)
        {
            if (certificate == null)
            {
                return false;
            }
            var subject = certificate.Subject;
            var issuer = certificate.Issuer;
            if (string.IsNullOrWhiteSpace(subject))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(issuer))
            {
                return false;
            }
            chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
            chain.ChainPolicy.UrlRetrievalTimeout = TimeSpan.FromSeconds(5);
            chain.ChainPolicy.VerificationTime = DateTime.Now;
            var valid = chain.Build(new X509Certificate2(certificate));
            hostInformation.Cert = new CertInfo
            {
                Subject = CertInfo.DecodeCNID(subject),
                Issuer = CertInfo.DecodeCNID(issuer),
                CertValid = valid
            };
            return valid;

        }

        public async Task<HostInformation> Pull(string host, ushort port, AddressFamily family)
        {
            return await ParseHost(host, port, family);
        }
    }
}

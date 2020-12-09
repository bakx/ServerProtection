using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Serilog;
using SP.Models;
using SP.Models.Enums;
using SP.Plugins;

namespace Plugins
{
    public class Monitor
    {
        private Thread thread;

        private readonly ILogger log;
        private readonly ConfigurationItem configItem;
        private readonly IPluginBase.AccessAttempt accessAttemptsHandler;

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public Monitor(IPluginBase.AccessAttempt accessAttemptsHandler, ILogger log, ConfigurationItem configItem)
        {
            this.accessAttemptsHandler = accessAttemptsHandler;
            this.log = log;
            this.configItem = configItem;
        }

        public void Start()
        {          
            // setfacl -m user:centos:r /var/log/maillog*
            // setfacl -m user:centos:r /var/log/secure*

            thread = new Thread(async () =>
            {
                Process process = new Process();

                process.OutputDataReceived += AnalyzeEntry;
                process.ErrorDataReceived += AnalyzeEntry;

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                process.StartInfo.FileName = configItem.Command;
                process.StartInfo.Arguments = configItem.Arguments.Replace("{log}", configItem.Log);

                process.Start();
                process.BeginOutputReadLine();
                await process.WaitForExitAsync();

                process.Exited += async (sender, args) =>
                {
                    log.Warning("Process exited, restarting...");

                    process.Start();
                    process.BeginOutputReadLine();
                    await process.WaitForExitAsync();
                };
            })
            {
                IsBackground = true
            };

            thread.Start();
        }

        ///
        private void AnalyzeEntry(object sendingProcess, DataReceivedEventArgs data)
        {
            // Detect if there is data received.
            if (string.IsNullOrEmpty(data.Data))
            {
                return;
            }

            Regex temp = new Regex(configItem.Regex);
            
            Match match = temp.Match(data.Data);

            // RegEx failed, likely not an authentication failure event
            if (!match.Success)
            {
                return;
            }
            
            // Common properties
            string ip = "";
            string user = "";
            
            // Detect if an IP is extracted
            if (match.Groups.Keys.Contains("ip"))
            {
                ip = match.Groups["ip"].Value;
            }
            
            // Detect if an username is extracted
            if (match.Groups.Keys.Contains("user"))
            {
                user = match.Groups["user"].Value;
            }

            var description = configItem.Description
                .Replace("{ip}", ip)
                .Replace("{user}", user);

            // Trigger login attempt event
            AccessAttempts accessAttempt = new AccessAttempts
            {
                IpAddress = ip,
                EventDate = DateTime.Now,
                Details = description,
                AttackType = AttackType.BruteForce,
                Custom1 = user,
                Custom2 = 0,
                Custom3 = 0
            };

            // Log attempt
            log.Information($"{ip} failed SMTP authentication.");

            // Fire event
            accessAttemptsHandler?.Invoke(accessAttempt);
        }
    }
}
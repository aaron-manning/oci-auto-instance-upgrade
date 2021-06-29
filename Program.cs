using System;
using System.Threading.Tasks;
using Oci.CoreService;
using Oci.Common;
using Oci.Common.Auth;
using Microsoft.Extensions.Configuration;

namespace oci_auto_instance_upgrader
{
    public class Program
    {
        /// <summary>
        /// Parse appsettings.json in application folder and prompts user via console if the values are correct.
        /// If the user submits anything other than "yes" in the console prompt, an exception will throw and the application will terminate.
        /// </summary>
        public static IConfigurationSection GetConfig()
        {
            var appSettings = new ConfigurationBuilder().AddJsonFile("appsettings.json", false).Build();
            Console.WriteLine($"{appSettings.GetDebugView()}\ncontinue? (yes/no)");
            if (!Console.ReadLine().Equals("yes"))
            {
                throw new Exception("you must confirm your config is correct");
            }
            return appSettings.GetSection("OracleConfig");
        }

        public static async Task Main()
        {
            // acquire application config
            var config = GetConfig();

            // build update request from config
            var updateInstanceRequest = new Oci.CoreService.Requests.UpdateInstanceRequest()
            {
                InstanceId = config["InstanceId"],
                UpdateInstanceDetails = new Oci.CoreService.Models.UpdateInstanceDetails
                {
                    Shape = config["Shape"],
                    ShapeConfig = new Oci.CoreService.Models.UpdateInstanceShapeConfigDetails
                    {
                        Ocpus = float.Parse(config["Ocpus"]),
                        MemoryInGBs = float.Parse(config["MemoryInGBs"]),
                        BaselineOcpuUtilization = Oci.CoreService.Models.UpdateInstanceShapeConfigDetails.BaselineOcpuUtilizationEnum.Baseline11
                    },
                },
            };
            // load OCI client with default config located at ~/.oci/config
            var provider = new ConfigFileAuthenticationDetailsProvider("DEFAULT");
            using var client = new ComputeClient(provider, new ClientConfiguration());
            Console.WriteLine("successfully parsed [DEFAULT] client from ~/.oci/config");

            // async loop indefinitely until operation succeeds
            var count = 0;
            while (true)
            {
                try
                {
                    count++;
                    // send update request to the REST api and await response
                    var response = await client.UpdateInstance(updateInstanceRequest);
                    // update succeeded on any non-error response
                    Console.WriteLine($"success: {response}");
                    break;
                }
                catch (Exception e)
                {
                    // update failed. Log error, wait 1 second, loop and retry
                    Console.WriteLine($"fail attempt {count}: {e.Message}");
                    await Task.Delay(1000);
                }

            }
        }
    }
}

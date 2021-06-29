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

        /// <summary>
        /// will call api at most once per second
        /// </summary>
        public static async Task Main()
        {
            var config = GetConfig();
            var provider = new ConfigFileAuthenticationDetailsProvider("DEFAULT");
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
            using var client = new ComputeClient(provider, new ClientConfiguration());
            Console.WriteLine("parsed [DEFAULT] client from ~/.oci/config");

            var count = 0;
            while (true)
            {
                try
                {
                    count++;
                    var response = await client.UpdateInstance(updateInstanceRequest);
                    Console.WriteLine($"success: {response}");
                    break; // success on any non-error response
                }
                catch (Exception e)
                {
                    Console.WriteLine($"fail attempt {count}: {e.Message}");
                    await Task.Delay(1000);
                }

            }
        }
    }
}

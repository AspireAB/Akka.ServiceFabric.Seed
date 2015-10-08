using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Akka.Configuration;
using AkkaClusterTest;

namespace SfServiceDiscoveryDemo
{
    public class ServiceFabricConfiguration : IDisposable
    {
        private AkkaProperties _props;
        private string _seedName;

        public ServiceFabricConfiguration()
        {
            _props = new AkkaProperties("PropertyManager");
            _props.Initialize();
        }

        public Config GetWorkerNodeConfig(string system, int port = 0 /*0 = find free port */)
        {

            //HACK: should be the IPV4 address I guess, using hostname instead for now
            var ip = Dns.GetHostName();

            //TODO: READ these two values FROM SF Propery Manager
            string configStr = GetConfigString(system, port, ip);
            //.Replace("%seedhost%", seedhost)
            //.Replace("%seedport%", seedport.ToString());

            var appConfig = ConfigurationFactory.Load();
            var serviceDiscoveryConfig = ConfigurationFactory.ParseString(configStr);
            return serviceDiscoveryConfig.WithFallback(appConfig);
        }

        private string GetConfigString(string system, int port, string ip)
        {
            var seedIps = _props.getSeedIPs().Distinct();
            var hokonSeedIps = string.Join(", ", seedIps.Select(x => '"' + x + '"'));
            var configStr = @"
akka.remote.helios.tcp.hostname = ""%host%""
akka.remote.helios.tcp.port = %port%
akka.cluster.seed-nodes = [%seedNodes%] "
                .Replace("%host%", ip)
                .Replace("%port%", port.ToString())
                .Replace("%system%", system)
                .Replace("%seedNodes%", hokonSeedIps).ToString();
            return configStr;
        }

        public Config GetSeedNodeConfig(string system, int port)
        {
            //HACK: should be the IPV4 address I guess, using hostname instead for now
            var ip = Dns.GetHostName();

            //TODO: WRITE properties TO SF Propery Manager
            //props.SetSeedIP("Node1", ip).Wait();
            _seedName = _props.SetSeedIP("akka.tcp://" + system + "@" + ip + ":" + port).Result;

            string configStr = GetConfigString(system, port, ip);

            var appConfig = ConfigurationFactory.Load();
            var serviceDiscoveryConfig = ConfigurationFactory.ParseString(configStr);
            return serviceDiscoveryConfig.WithFallback(appConfig);
        }

        public void Dispose()
        {
            if (_seedName != null)
                _props.Remove(_seedName);
        }
    }
}


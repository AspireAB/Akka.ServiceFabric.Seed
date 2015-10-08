using System;
using Akka.Actor;
using SfServiceDiscoveryDemo;

namespace WorkerNodeDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var serviceFabricConfiguration = new ServiceFabricConfiguration())
            {
                using (var system = ActorSystem.Create("SF", serviceFabricConfiguration.GetWorkerNodeConfig("SF")))
                {
                    Console.ReadLine();
                }
            }
        }
    }
}
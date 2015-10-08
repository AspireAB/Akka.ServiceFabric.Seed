using System;
using Akka.Actor;
using SfServiceDiscoveryDemo;

namespace SeedModeDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var serviceFabricConfiguration = new ServiceFabricConfiguration())
            {
                using (var system = ActorSystem.Create("SF", serviceFabricConfiguration.GetSeedNodeConfig("SF", 8080)))
                {
                    Console.ReadLine();
                }
            }
        }
    }
}
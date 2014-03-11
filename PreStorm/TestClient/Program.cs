using System;
using PreStorm;

namespace TestClient
{
    class Program
    {
        static void Main()
        {
            var service = new Service("http://sampleserver6.arcgisonline.com/arcgis/rest/services/SF311/FeatureServer");

            foreach (var incident in service.Download<Incident>("Incidents"))
            {
                Console.WriteLine(incident.address);
            }

            Console.WriteLine("Press ENTER to exit.");
            Console.ReadLine();
        }
    }
}

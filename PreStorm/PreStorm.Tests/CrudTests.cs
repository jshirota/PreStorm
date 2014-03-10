using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PreStorm.Tests
{
    [TestClass]
    public class CrudTests
    {
        private void CrudTest<T>(string url, string layerName, Action<T> init, Action<T> change, Func<T, bool> test, string whereClause) where T : Feature, new()
        {
            var service = new Service(url);
            var f = new T();
            init(f);

            Assert.IsTrue(f.OID == -1);
            Assert.IsTrue((f = f.InsertInto(service, layerName)) != null);
            Assert.IsTrue(f.OID != -1);

            var changedProperties = new List<string>();
            f.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);
            change(f);

            Assert.IsTrue(changedProperties.Count == 2);
            Assert.IsTrue(test(f));
            Assert.IsTrue(f.IsDirty);
            Assert.IsTrue(f.Update());
            Assert.IsTrue(test(f));
            Assert.IsTrue(!f.IsDirty);

            f = service.Download<T>(layerName, whereClause).Single();

            Assert.IsTrue(test(f));
            Assert.IsTrue(!f.IsDirty);
            Assert.IsTrue(f.Delete());
            Assert.IsTrue(f.OID == -1);
            Assert.IsTrue(!service.Download<T>(layerName, whereClause).Any());
        }

        [TestMethod]
        public void CrudTestEmergencyFacility()
        {
            var key = Guid.NewGuid().ToString().Substring(0, 8);
            CrudTest<EmergencyFacility>(
                "http://sampleserver6.arcgisonline.com/arcgis/rest/services/EmergencyFacilities/FeatureServer",
                "Emergency Facilities",
                t => { t.Status = "Open"; t.Name = key; },
                t => t.Status = "Closed",
                t => t.Status == "Closed",
                "facname='" + key + "'");
        }

        [TestMethod]
        public void CrudTestIncident()
        {
            var key = Guid.NewGuid().ToString().Substring(0, 8);
            CrudTest<Incident>(
                "http://sampleserver6.arcgisonline.com/arcgis/rest/services/SF311/FeatureServer",
                "Incidents",
                t => { t.status = 123; t.req_type = key; },
                t => t.status = 456,
                t => t.status == 456,
                "req_type='" + key + "'");
        }

        [TestMethod]
        public void CrudTestIncidentArea()
        {
            var key = Guid.NewGuid().ToString().Substring(0, 8);
            CrudTest<IncidentArea>(
                "http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/HomelandSecurity/operations/FeatureServer",
                "Incident Areas",
                t => { t.lifecyclestatus = "Active/Confirmed"; t.description = key; },
                t => t.lifecyclestatus = "Closed/Completed",
                t => t.lifecyclestatus == "Closed/Completed",
                "description='" + key + "'");
        }

        [TestMethod]
        public void CrudTestWildfireResponseLine()
        {
            var key = Guid.NewGuid().ToString().Substring(0, 8);
            CrudTest<WildfireResponseLine>(
                "http://sampleserver5.arcgisonline.com/arcgis/rest/services/Wildfire/FeatureServer",
                "Wildfire Response Lines",
                t => { t.symbolid = 999; t.description = key; },
                t => t.symbolid = 777,
                t => t.symbolid == 777,
                "description='" + key + "'");
        }

        [TestMethod]
        public void CrudTestFacility()
        {
            var key = Guid.NewGuid().ToString().Substring(0, 8);
            CrudTest<Facility>(
                "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Recreation/FeatureServer",
                "Facilities",
                t => { t.facility = "Rest Area"; t.description = key; },
                t => t.facility = "Camping",
                t => t.facility == "Camping",
                "description='" + key + "'");
        }
    }
}

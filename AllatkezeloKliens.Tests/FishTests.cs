using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using allatkezelo_kliens.Services;
using System.Drawing;

namespace AllatkezeloKliens.Tests
{
    [TestClass]
    public class FishTests
    {
        private FishService _service;

        [TestInitialize]
        public void Setup()
        {
            _service = new FishService();
        }

        [TestMethod]
        public void ParseHtmlDescription_ShouldExtractFishDataCorrectly()
        {
            // Teszteljük a speciális, esetenként kódolt HTML-t is
            string html = @"<p style=""text-align: left;""><strong>Jellemzők</strong>: Szép hal</p>
                            <p style=""text-align: left;""><strong>Tart&aacute;s</strong>: Könnyű</p>";

            var details = _service.ParseHtmlDescription(html);

            Assert.AreEqual("Szép hal", details.Jellemzok);
            Assert.AreEqual("Könnyű", details.Tartas);
            Assert.AreEqual("", details.Vizparameterek); // Nincs megadva, üresnek kell lennie
        }

        [TestMethod]
        public void BuildLongDescription_ShouldContainAllFishFields()
        {
            var result = _service.BuildLongDescription("Gyors", "Közepes", "25C", "Mindent", "Ikrázó");

            Assert.IsTrue(result.Contains("Gyors"));
            Assert.IsTrue(result.Contains("<strong>Vízparaméterek</strong>: 25C"));
        }

        [TestMethod]
        public void ValidatePrices_InvalidNumbers_ShouldReturnFalse()
        {
            bool result = _service.ValidatePrices("1500", "nem_szam", out decimal listPrice, out decimal sitePrice);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CalculateAvailableStock_CorrectSubtraction()
        {
            int result = _service.CalculateAvailableStock(20, 5);
            Assert.AreEqual(15, result);
        }

        [TestMethod]
        public void GetStockStatusColor_PositiveStock_ShouldReturnDarkGreen()
        {
            var color = _service.GetStockStatusColor(1);
            Assert.AreEqual(Color.DarkGreen, color);
        }
    }
}

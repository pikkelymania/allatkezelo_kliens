using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using allatkezelo_kliens.Services;
using System.Drawing;

namespace AllatkezeloKliens.Tests
{
    [TestClass]
    public class ReptileTests
    {
        private ReptileService _service;

        [TestInitialize]
        public void Setup()
        {
            _service = new ReptileService();
        }

        [TestMethod]
        public void ValidatePrices_ValidNumbers_ShouldReturnTrueAndParseCorrectly()
        {
            // Teszteljük, hogy a helyes formátumokat jól alakítja-e számmá
            bool result = _service.ValidatePrices("15000", "12500,50", out decimal listPrice, out decimal sitePrice);

            Assert.IsTrue(result);
            Assert.AreEqual(15000m, listPrice);
            Assert.AreEqual(12500.50m, sitePrice);
        }

        [TestMethod]
        public void ValidatePrices_InvalidNumbers_ShouldReturnFalse()
        {
            // Teszteljük a felhasználói elgépeléseket
            bool result = _service.ValidatePrices("ingyen", "százezer", out decimal listPrice, out decimal sitePrice);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ParseHtmlDescription_ShouldExtractDataCorrectly()
        {
            // Teszteljük a Regex alapú kiszedést egy valós formátumú stringgel
            string html = @"<p style=""text-align: left;""><strong>Név</strong>: <br />Béla</p>
                            <p style=""text-align: left;""><strong>Született</strong>: <br />2023. 05. 10.</p>";

            var details = _service.ParseHtmlDescription(html);

            Assert.AreEqual("Béla", details.Nev);
            Assert.AreEqual(new DateTime(2023, 5, 10), details.Szuletett);
            Assert.AreEqual("", details.Genetika); // Ezt direkt hagytuk ki, üresnek kell lennie
        }

        [TestMethod]
        public void BuildLongDescription_ShouldContainAllFields()
        {
            // Teszteljük, hogy az összerakott HTML tartalmazza-e a paramétereket
            var result = _service.BuildLongDescription("Gizi", "2024. 01. 01.", "Nőstény", "Albínó", "Kedves");

            Assert.IsTrue(result.Contains("Gizi"));
            Assert.IsTrue(result.Contains("Albínó"));
            Assert.IsTrue(result.Contains("<strong>Nem</strong>"));
        }

        [TestMethod]
        public void CalculateAvailableStock_MoreReservedThanOnHand_ShouldReturnNegative()
        {
            // Teszteljük a határesetet: 5 van, de 10 lefoglalva
            int result = _service.CalculateAvailableStock(5, 10);

            Assert.AreEqual(-5, result);
        }

        [TestMethod]
        public void GetStockStatusColor_ZeroStock_ShouldReturnRed()
        {
            // Teszteljük, hogy 0 vagy negatív darabnál tényleg piros-e
            var color = _service.GetStockStatusColor(0);

            Assert.AreEqual(Color.Red, color);
        }

        [TestMethod]
        public void GetStockStatusColor_PositiveStock_ShouldReturnDarkGreen()
        {
            // Teszteljük, hogy van készleten, akkor sötétzöld-e
            var color = _service.GetStockStatusColor(10);

            Assert.AreEqual(Color.DarkGreen, color);
        }
    }
}
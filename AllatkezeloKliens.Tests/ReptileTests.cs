using Microsoft.VisualStudio.TestTools.UnitTesting;
using allatkezelo_kliens.Services;
using System;
using System.Drawing;

namespace AllatkezeloKliens.Tests
{
    [TestClass]
    public class ReptileTests
    {
        private ReptileService _service;

        [TestInitialize]
        public void Setup() => _service = new ReptileService(null);

        // 1. ÁTFOGÓ TESZT: Árak és bemeneti adatok ellenőrzése (Data-Driven)
        [DataTestMethod]
        [DataRow("15000", "12000", true, "Érvényes egész számok")]
        [DataRow("15000,5", "12000,2", true, "Érvényes tizedes számok")]
        [DataRow("ingyen", "1000", false, "Betű a listaárban")]
        [DataRow("1000", "drága", false, "Betű az eladási árban")]
        [DataRow("", "1000", false, "Üres mező")]
        [DataRow("-500", "1000", true, "Negatív ár (matematikailag érvényes)")]
        public void ValidatePrices_AllCases(string listP, string siteP, bool expected, string caseName)
        {
            bool actual = _service.ValidatePrices(listP, siteP, out _, out _);
            Assert.AreEqual(expected, actual, $"Hiba a következő esetnél: {caseName}");
        }

        // 2. ÁTFOGÓ TESZT: Raktárkészlet matek és színek (Data-Driven)
        [DataTestMethod]
        [DataRow(10, 2, 8, "DarkGreen")] // Van készlet
        [DataRow(5, 5, 0, "Red")]       // Pont elfogyott
        [DataRow(2, 10, -8, "Red")]     // Negatív készlet (túlfoglalás)
        [DataRow(0, 0, 0, "Red")]       // Alaphelyzet
        public void StockAndColor_AllCases(int onHand, int reserved, int expectedStock, string colorName)
        {
            int actualStock = _service.CalculateAvailableStock(onHand, reserved);
            Color actualColor = _service.GetStockStatusColor(actualStock);

            Assert.AreEqual(expectedStock, actualStock, "A készletszámítás hibás.");
            Assert.AreEqual(Color.FromName(colorName), actualColor, "A státuszszín hibás.");
        }

        // 3. ÁTFOGÓ TESZT: Új állat felvitele (Validáció + DTO építés)
        [DataTestMethod]
        [DataRow("Gizi", "SKU001", true, true, "Minden adat megvan")]
        [DataRow("", "SKU001", true, false, "Hiányzó név")]
        [DataRow("Gizi", "", true, false, "Hiányzó SKU")]
        [DataRow("Gizi", "SKU001", false, false, "Hiányzó kép")]
        public void NewReptile_Validation_AllCases(string name, string sku, bool hasImage, bool expected, string caseName)
        {
            byte[] img = hasImage ? new byte[] { 1, 2, 3 } : null;
            bool actual = _service.ValidateNewReptile(name, sku, img, out _);
            Assert.AreEqual(expected, actual, $"Hiba a validációnál: {caseName}");
        }
    }
}
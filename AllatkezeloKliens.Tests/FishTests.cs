using Microsoft.VisualStudio.TestTools.UnitTesting;
using allatkezelo_kliens.Services;
using System.Drawing;

namespace AllatkezeloKliens.Tests
{
    [TestClass]
    public class FishTests
    {
        private FishService _service;

        [TestInitialize]
        public void Setup() => _service = new FishService();

        // 1. ÁTFOGÓ TESZT: HTML visszafejtés és Regex (Minden eshetőségre)
        [DataTestMethod]
        [DataRow("<strong>Tartás</strong>: Könnyű</p>", "Könnyű", "Sima ékezet")]
        [DataRow("<strong>Tart&aacute;s</strong>: Nehéz</p>", "Nehéz", "HTML kódolt á betű")]
        [DataRow("<strong>T&aacute;pl&aacute;lkoz&aacute;s</strong>: Mindenevő</p>", "Mindenevő", "Többszörös HTML kódolás")]
        [DataRow("<strong>Jellemzők</strong>: <b>Vastag</b></p>", "Vastag", "Beágyazott HTML tagek")]
        [DataRow("", "", "Üres bemenet")]
        public void ParseHtml_Extraction_Comprehensive(string html, string expectedValue, string caseName)
        {
            var details = _service.ParseHtmlDescription(html);
            // Megnézzük, hogy az érték szerepel-e valamelyik mezőben
            bool found = details.Tartas.Contains(expectedValue) ||
                         details.Taplalkozas.Contains(expectedValue) ||
                         details.Jellemzok.Contains(expectedValue);

            if (expectedValue != "")
                Assert.IsTrue(found, $"Nem sikerült kinyerni az adatot: {caseName}");
        }

        // 2. ÁTFOGÓ TESZT: HTML generálás integritása
        [DataTestMethod]
        [DataRow("Gyors", "25C", "Mindent")]
        [DataRow("Páncélos", "22-28C", "Lapkát")]
        [DataRow("", "", "")]
        public void BuildDescription_FieldIntegrity(string j, string v, string t)
        {
            string html = _service.BuildLongDescription(j, "Közepes", v, t, "Nincs");

            Assert.IsTrue(html.Contains("<strong>Jellemzők</strong>"), "Hiányzó HTML tag.");
            Assert.IsTrue(html.Contains(j), "A jellemzők mező nem került a HTML-be.");
            Assert.IsTrue(html.Contains(v), "A vízparaméter mező nem került a HTML-be.");
        }

        // 3. ÁTFOGÓ TESZT: Új hal felvitele (Validáció)
        [DataTestMethod]
        [DataRow("Némó", "SKU002", true, true, "Minden adat megvan")]
        [DataRow("", "SKU002", true, false, "Hiányzó név")]
        [DataRow("Némó", "", true, false, "Hiányzó SKU")]
        [DataRow("Némó", "SKU002", false, false, "Hiányzó kép")]
        public void NewFish_Validation_AllCases(string name, string sku, bool hasImage, bool expected, string caseName)
        {
            byte[] img = hasImage ? new byte[] { 1, 2, 3 } : null;
            bool actual = _service.ValidateNewFish(name, sku, img, out _);
            Assert.AreEqual(expected, actual, $"Hiba a validációnál: {caseName}");
        }


    }
}
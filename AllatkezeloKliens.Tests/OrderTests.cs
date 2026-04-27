using Microsoft.VisualStudio.TestTools.UnitTesting;
using allatkezelo_kliens.Services;
using Hotcakes.CommerceDTO.v1.Contacts;
using Hotcakes.CommerceDTO.v1.Orders;
using System.Collections.Generic;

namespace AllatkezeloKliens.Tests
{
    [TestClass]
    public class OrderTests
    {
        private OrderService _service;

        [TestInitialize]
        public void Setup() => _service = new OrderService();

        // 1. ÁTFOGÓ TESZT: Készletfelszabadítás (Határesetek)
        [DataTestMethod]
        [DataRow(10, 2, 8, "Sima kivonás")]
        [DataRow(5, 5, 0, "Pont nullára fut")]
        [DataRow(2, 5, 0, "Negatív védelem (többet törölne, mint amennyi le van foglalva)")]
        [DataRow(0, 5, 0, "Alapból nulla foglalásból vonna le")]
        public void CalculateReleasedInventory_AllCases(int current, int canceled, int expected, string caseName)
        {
            int actual = _service.CalculateReleasedInventory(current, canceled);
            Assert.AreEqual(expected, actual, $"Hiba a készletszámításnál: {caseName}");
        }

        // 2. ÁTFOGÓ TESZT: Vevő nevének összerakása (Null referenciák)
        [DataTestMethod]
        [DataRow("Gipsz", "Jakab", "Gipsz Jakab")]
        [DataRow("Gipsz", "", "Gipsz")]
        [DataRow("", "Jakab", "Jakab")]
        [DataRow("", "", "Ismeretlen vevő")]
        [DataRow(null, null, "Ismeretlen vevő")]
        public void GetCustomerFullName_AllCases(string first, string last, string expected)
        {
            AddressDTO address = null;
            if (first != null || last != null)
            {
                address = new AddressDTO { FirstName = first, LastName = last };
            }

            string actual = _service.GetCustomerFullName(address);
            Assert.AreEqual(expected, actual);
        }

        // 3. NORMÁL TESZT: Üres rendelési lista kezelése
        [TestMethod]
        public void GetFirstItemSummary_EmptyList_ReturnsDefaultStrings()
        {
            _service.GetFirstItemSummary(new List<LineItemDTO>(), out string pName, out string sku, out string qty);

            Assert.AreEqual("Nincs termék a rendelésben", pName);
            Assert.AreEqual("-", sku);
            Assert.AreEqual("0", qty);
        }
    }
}

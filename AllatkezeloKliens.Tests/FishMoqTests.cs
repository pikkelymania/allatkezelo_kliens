using allatkezelo_kliens.Services;
using Hotcakes.CommerceDTO.v1;
using Hotcakes.CommerceDTO.v1.Catalog;
using Hotcakes.CommerceDTO.v1.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace AllatkezeloKliens.Tests
{
    [TestClass]
    public class FishMoqTests
    {
        [TestMethod]
        public void SaveFish_SikeresMentestSzimulal()
        {
            // 1. ARRANGE: Szimulátor létrehozása
            var mockApi = new Mock<IHotcakesApi>();

            // Beállítjuk a szimulátort: Bármilyen halat kap, adjon vissza sikeres választ (0 hiba)
            var successResponse = new ApiResponse<ProductDTO> { Errors = new List<ApiError>() };
            mockApi.Setup(api => api.ProductsCreate(It.IsAny<ProductDTO>(), It.IsAny<byte[]>()))
                   .Returns(successResponse);

            var service = new FishService(mockApi.Object);
            var tesztHal = new ProductDTO { ProductName = "Moq Teszt Hal", Sku = "MOQ-001" };

            // 2. ACT: Meghívjuk a mentést
            bool eredmeny = service.SaveFishToWebshop(tesztHal, null);

            // 3. ASSERT: Ellenőrizzük, hogy a Service elhitte-e a sikert
            Assert.IsTrue(eredmeny);
            // Plusz ellenőrzés: Tényleg meghívta a Service az API-t pontosan egyszer?
            mockApi.Verify(a => a.ProductsCreate(It.IsAny<ProductDTO>(), It.IsAny<byte[]>()), Times.Once);
        }

        [TestMethod]
        public void SaveFish_SikertelenMentes_AmikorAzApiHibatAdVissza()
        {
            // 1. ARRANGE: A szimulátor előkészítése a hibás válaszra
            var mockApi = new Mock<IHotcakesApi>();

            // Létrehozunk egy olyan API választ, amiben van legalább egy hiba
            var errorResponse = new ApiResponse<ProductDTO>
            {
                Errors = new List<ApiError> { new ApiError { Description = "Már létezik ilyen SKU a rendszerben!" } }
            };

            // Megmondjuk a szimulátornak, hogy BÁRMILYEN mentési kísérletre ezt a hibát dobja vissza
            mockApi.Setup(api => api.ProductsCreate(It.IsAny<ProductDTO>(), It.IsAny<byte[]>()))
                   .Returns(errorResponse);

            var service = new FishService(mockApi.Object);
            var tesztHal = new ProductDTO { ProductName = "Hibás Hal", Sku = "MOQ-BAD-001" };

            // 2. ACT: Végrehajtjuk a mentést a szervizen keresztül
            bool eredmeny = service.SaveFishToWebshop(tesztHal, null);

            // 3. ASSERT: Mivel az API hibát jelzett, a Service-nek ezt fel kell ismernie, 
            // és a SaveFishToWebshop metódusnak 'false' (hamis) értékkel kell visszatérnie.
            Assert.IsFalse(eredmeny);

            // Plusz ellenőrzés: Biztosítjuk, hogy az API hívás valóban megtörtént
            mockApi.Verify(a => a.ProductsCreate(It.IsAny<ProductDTO>(), It.IsAny<byte[]>()), Times.Once);
        }

        [TestMethod]
        public void SaveFish_KiveteltKezel_AmikorMegszakadAkapcsolat()
        {
            // 1. ARRANGE: A szimulátor összeomlást fog szimulálni
            var mockApi = new Mock<IHotcakesApi>();

            // Beállítjuk, hogy hálózat/szerver hiba (Exception) történjen a híváskor
            mockApi.Setup(api => api.ProductsCreate(It.IsAny<ProductDTO>(), It.IsAny<byte[]>()))
                   .Throws(new System.Exception("Megszakadt az internetkapcsolat!"));

            var service = new FishService(mockApi.Object);
            var tesztHal = new ProductDTO { ProductName = "Árva Hal", Sku = "MOQ-NET-001" };

            // 2. ACT: Végrehajtjuk a mentést a szervizen keresztül
            bool eredmeny = service.SaveFishToWebshop(tesztHal, null);

            // 3. ASSERT: A Service-nek a try-catch blokkban meg kell fognia a hibát, 
            // nem fagyhat le a teszt (és a program sem), hanem csendben false-t kell visszaadnia.
            Assert.IsFalse(eredmeny);
        }
    }
}

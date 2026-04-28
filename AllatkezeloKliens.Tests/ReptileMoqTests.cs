using allatkezelo_kliens.Services;
using Hotcakes.CommerceDTO.v1;
using Hotcakes.CommerceDTO.v1.Catalog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace AllatkezeloKliens.Tests
{
    [TestClass]
    public class ReptileMoqTests
    {
        [TestMethod]
        public void SaveReptile_SikeresMentestSzimulal()
        {
            var mockApi = new Mock<IHotcakesApi>();
            var successResponse = new ApiResponse<ProductDTO> { Errors = new List<ApiError>() };

            mockApi.Setup(api => api.ProductsCreate(It.IsAny<ProductDTO>(), It.IsAny<byte[]>()))
                   .Returns(successResponse);

            var service = new ReptileService(mockApi.Object);
            var tesztHullo = new ProductDTO { ProductName = "Moq Piton", Sku = "REPT-001" };

            bool eredmeny = service.SaveReptileToWebshop(tesztHullo, null);

            Assert.IsTrue(eredmeny);
            mockApi.Verify(a => a.ProductsCreate(It.IsAny<ProductDTO>(), It.IsAny<byte[]>()), Times.Once);
        }

        [TestMethod]
        public void SaveReptile_SikertelenMentes_AmikorAzApiHibatAdVissza()
        {
            // ARRANGE
            var mockApi = new Mock<IHotcakesApi>();
            var errorResponse = new ApiResponse<ProductDTO>
            {
                Errors = new List<ApiError> { new ApiError { Description = "Már létezik ilyen SKU a hüllőknél!" } }
            };

            mockApi.Setup(api => api.ProductsCreate(It.IsAny<ProductDTO>(), It.IsAny<byte[]>()))
                   .Returns(errorResponse);

            var service = new ReptileService(mockApi.Object);
            var tesztHullo = new ProductDTO { ProductName = "Hibás Hüllő", Sku = "REPT-BAD-001" };

            // ACT
            bool eredmeny = service.SaveReptileToWebshop(tesztHullo, null);

            // ASSERT
            Assert.IsFalse(eredmeny);
        }

        [TestMethod]
        public void SaveReptile_KiveteltKezel_AmikorMegszakadAkapcsolat()
        {
            // ARRANGE
            var mockApi = new Mock<IHotcakesApi>();

            mockApi.Setup(api => api.ProductsCreate(It.IsAny<ProductDTO>(), It.IsAny<byte[]>()))
                   .Throws(new System.Exception("Megszakadt az internetkapcsolat a hüllő mentése közben!"));

            var service = new ReptileService(mockApi.Object);
            var tesztHullo = new ProductDTO { ProductName = "Árva Hüllő", Sku = "REPT-NET-001" };

            // ACT
            bool eredmeny = service.SaveReptileToWebshop(tesztHullo, null);

            // ASSERT
            Assert.IsFalse(eredmeny);
        }


    }
}

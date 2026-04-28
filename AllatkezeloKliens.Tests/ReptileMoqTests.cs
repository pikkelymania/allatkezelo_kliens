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
    }
}

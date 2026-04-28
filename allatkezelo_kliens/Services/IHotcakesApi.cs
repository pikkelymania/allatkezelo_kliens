using Hotcakes.CommerceDTO.v1;
using Hotcakes.CommerceDTO.v1.Catalog;
using Hotcakes.CommerceDTO.v1.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace allatkezelo_kliens.Services
{
    // Ez az interfész mondja meg, mit kell tudnia az API-nak
    public interface IHotcakesApi
    {
        ApiResponse<ProductDTO> ProductsCreate(ProductDTO product, byte[] imageBytes);
    }

    // Ez az osztály az igazi API-t csomagolja be (ezt használjuk élesben)
    public class HotcakesApiWrapper : IHotcakesApi
    {
        private readonly Api _api;
        public HotcakesApiWrapper(Api api) => _api = api;

        public ApiResponse<ProductDTO> ProductsCreate(ProductDTO product, byte[] imageBytes)
        {
            return _api.ProductsCreate(product, imageBytes);
        }
    }
}

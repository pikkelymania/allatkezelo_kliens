using Hotcakes.CommerceDTO.v1.Catalog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace allatkezelo_kliens.Services
{
    // Adattároló a HTML-ből visszafejtett halas adatoknak
    public class FishDetails
    {
        public string Jellemzok { get; set; } = "";
        public string Tartas { get; set; } = "";
        public string Vizparameterek { get; set; } = "";
        public string Taplalkozas { get; set; } = "";
        public string Szaporitas { get; set; } = "";
    }

    public interface IFishService
    {
        string BuildLongDescription(string jellemzok, string tartas, string vizparameterek, string taplalkozas, string szaporitas);
        FishDetails ParseHtmlDescription(string htmlDescription);
        bool ValidatePrices(string listPriceText, string sitePriceText, out decimal listPrice, out decimal sitePrice);
        int CalculateAvailableStock(int onHand, int reserved);
        Color GetStockStatusColor(int availableStock);
        bool ValidateNewFish(string productName, string sku, byte[] imageBytes, out string errorMessage);
        Hotcakes.CommerceDTO.v1.Catalog.ProductDTO CreateNewFishDTO(string sku, string productName, decimal listPrice, decimal sitePrice, string htmlDescription, bool isAvailable);
    }

    public class FishService : IFishService
    {

        private readonly IHotcakesApi _apiWrapper;

        // Konstruktor: Ha kap API-t, azt használja, ha nem (null), akkor is lefut a többi teszt
        public FishService(IHotcakesApi apiWrapper)
        {
            _apiWrapper = apiWrapper;
        }

        // Az új metódus, amit Moq-val fogunk tesztelni
        public bool SaveFishToWebshop(Hotcakes.CommerceDTO.v1.Catalog.ProductDTO fish, byte[] img)
        {
            if (_apiWrapper == null) return false;

            try
            {
                // Megpróbáljuk elküldeni az adatokat a (szimulált vagy igazi) API-nak
                var response = _apiWrapper.ProductsCreate(fish, img);

                // Ha nincs hiba a válaszban, akkor a mentés sikeres volt
                return response.Errors == null || response.Errors.Count == 0;
            }
            catch
            {
                // Ha bármilyen váratlan hiba történik (pl. kábelszakadás, Exception), 
                // megfogjuk a hibát, és lefagyás helyett simán hamisat adunk vissza.
                return false;
            }
        }

        // 1. HTML Generálása (A halas formátum szerint)
        public string BuildLongDescription(string jellemzok, string tartas, string vizparameterek, string taplalkozas, string szaporitas)
        {
            return $@"<p style=""text-align: left;""><strong>Jellemzők</strong>: {jellemzok}</p>
<p style=""text-align: left;""><strong>Tartás</strong>: {tartas}</p>
<p style=""text-align: left;""><strong>Vízparaméterek</strong>: {vizparameterek}</p>
<p style=""text-align: left;""><strong>Táplálkozás</strong>: {taplalkozas}</p>
<p style=""text-align: left;""><strong>Szaporítás</strong>: {szaporitas}</p>";
        }

        // 2. HTML Visszafejtése (A sok Regex ide kerül)
        public FishDetails ParseHtmlDescription(string htmlDescription)
        {
            var details = new FishDetails();
            if (string.IsNullOrWhiteSpace(htmlDescription)) return details;

            string decodedHtml = WebUtility.HtmlDecode(htmlDescription);

            var matchJellemzok = Regex.Match(decodedHtml, @"<strong>Jellemzők</strong>:\s*(.*?)\s*</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (matchJellemzok.Success) details.Jellemzok = WebUtility.HtmlDecode(matchJellemzok.Groups[1].Value.Trim());

            var matchTartas = Regex.Match(decodedHtml, @"<strong>Tart(?:ás|&aacute;s)</strong>:\s*(.*?)\s*</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (matchTartas.Success) details.Tartas = WebUtility.HtmlDecode(matchTartas.Groups[1].Value.Trim());

            var matchViz = Regex.Match(decodedHtml, @"<strong>V(?:ízparaméterek|&iacute;zparam&eacute;terek)</strong>:\s*(.*?)\s*</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (matchViz.Success) details.Vizparameterek = WebUtility.HtmlDecode(matchViz.Groups[1].Value.Trim());

            var matchTaplalkozas = Regex.Match(decodedHtml, @"<strong>T(?:áplálkozás|&aacute;pl&aacute;lkoz&aacute;s)</strong>:\s*(.*?)\s*</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (matchTaplalkozas.Success) details.Taplalkozas = WebUtility.HtmlDecode(matchTaplalkozas.Groups[1].Value.Trim());

            var matchSzaporitas = Regex.Match(decodedHtml, @"<strong>Szapor(?:ítás|&iacute;t&aacute;s)</strong>:\s*(.*?)\s*</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (matchSzaporitas.Success) details.Szaporitas = WebUtility.HtmlDecode(matchSzaporitas.Groups[1].Value.Trim());

            return details;
        }

        // 3. Árak validálása
        public bool ValidatePrices(string listPriceText, string sitePriceText, out decimal listPrice, out decimal sitePrice)
        {
            bool isListValid = decimal.TryParse(listPriceText, System.Globalization.NumberStyles.Any, null, out listPrice);
            bool isSiteValid = decimal.TryParse(sitePriceText, System.Globalization.NumberStyles.Any, null, out sitePrice);
            return isListValid && isSiteValid;
        }

        // 4. Raktárkészlet számítása
        public int CalculateAvailableStock(int onHand, int reserved)
        {
            return onHand - reserved;
        }

        // 5. Státuszszín
        public Color GetStockStatusColor(int availableStock)
        {
            if (availableStock <= 0) return Color.Red;
            return Color.DarkGreen;
        }

        public bool ValidateNewFish(string productName, string sku, byte[] imageBytes, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(productName) || string.IsNullOrWhiteSpace(sku))
            {
                errorMessage = "A Terméknév és a Cikkszám megadása kötelező!";
                return false;
            }
            if (imageBytes == null || imageBytes.Length == 0)
            {
                errorMessage = "Kérlek, válassz ki egy képet a mentés előtt!";
                return false;
            }
            errorMessage = string.Empty;
            return true;
        }

        public Hotcakes.CommerceDTO.v1.Catalog.ProductDTO CreateNewFishDTO(string sku, string productName, decimal listPrice, decimal sitePrice, string htmlDescription, bool isAvailable)
        {
            return new Hotcakes.CommerceDTO.v1.Catalog.ProductDTO
            {
                Sku = sku,
                ProductName = productName,
                ListPrice = listPrice,
                SitePrice = sitePrice,
                LongDescription = htmlDescription,
                IsAvailableForSale = isAvailable,

                // Alapértelmezett értékek
                ProductTypeId = "",
                SitePriceOverrideText = "",
                SiteCost = 0m,
                TaxExempt = false,
                ShippingDetails = new Hotcakes.CommerceDTO.v1.Shipping.ShippableItemDTO { IsNonShipping = true },
                ShippingMode = Hotcakes.CommerceDTO.v1.Shipping.ShippingModeDTO.ShipFromSite,
                ShippingCharge = Hotcakes.CommerceDTO.v1.Shipping.ShippingChargeTypeDTO.ChargeShippingAndHandling,
                Status = Hotcakes.CommerceDTO.v1.Catalog.ProductStatusDTO.Active,
                CreationDateUtc = DateTime.UtcNow,
                ShortDescription = "",
                ManufacturerId = "",
                VendorId = "",
                GiftWrapAllowed = false,
                GiftWrapPrice = 0m,
                Keywords = "",
                PreContentColumnId = "",
                PostContentColumnId = "",
                UrlSlug = "",
                InventoryMode = Hotcakes.CommerceDTO.v1.Catalog.ProductInventoryModeDTO.WhenOutOfStockShow,
                Featured = false,
                AllowReviews = true,
                StoreId = 1,
                IsSearchable = true,
                AllowUpcharge = false,
                UpchargeAmount = 0m, // A halaknál a te kódodban ez 0 volt!
                UpchargeUnit = "1"
            };
        }

    }
}

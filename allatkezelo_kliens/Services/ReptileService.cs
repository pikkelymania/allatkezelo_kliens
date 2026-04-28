using System;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;

namespace allatkezelo_kliens.Services
{
    // Adattároló osztály a HTML-ből visszafejtett adatoknak
    public class ReptileDetails
    {
        public string Nev { get; set; } = "";
        public DateTime? Szuletett { get; set; }
        public string Nem { get; set; } = "";
        public string Genetika { get; set; } = "";
        public string Szemelyiseg { get; set; } = "";
    }

    public interface IReptileService
    {
        string BuildLongDescription(string nev, string szuletett, string nem, string genetika, string szemelyiseg);
        ReptileDetails ParseHtmlDescription(string htmlDescription);
        bool ValidatePrices(string listPriceText, string sitePriceText, out decimal listPrice, out decimal sitePrice);
        int CalculateAvailableStock(int onHand, int reserved);
        Color GetStockStatusColor(int availableStock);
        bool ValidateNewReptile(string productName, string sku, byte[] imageBytes, out string errorMessage);
        Hotcakes.CommerceDTO.v1.Catalog.ProductDTO CreateNewReptileDTO(string sku, string productName, decimal listPrice, decimal sitePrice, string htmlDescription, bool isAvailable);

        // ÚJ: A mentés metódus az interfészben is szerepel
        bool SaveReptileToWebshop(Hotcakes.CommerceDTO.v1.Catalog.ProductDTO reptile, byte[] img);
    }

    public class ReptileService : IReptileService
    {
        // --- 0. FÜGGŐSÉGINJEKTÁLÁS ÉS MENTÉS (Moq teszteléshez) ---
        private readonly IHotcakesApi _apiWrapper;

        // A konstruktor most már kéri a "hidat" az API-hoz
        public ReptileService(IHotcakesApi apiWrapper)
        {
            _apiWrapper = apiWrapper;
        }

        // ÚJ: Ezt a metódust fogjuk Moq-val tesztelni
        public bool SaveReptileToWebshop(Hotcakes.CommerceDTO.v1.Catalog.ProductDTO reptile, byte[] img)
        {
            if (_apiWrapper == null) return false;

            try
            {
                var response = _apiWrapper.ProductsCreate(reptile, img);
                return response.Errors == null || response.Errors.Count == 0;
            }
            catch
            {
                return false;
            }
        }
        // ----------------------------------------------------------

        // 1. HTML Generálása
        public string BuildLongDescription(string nev, string szuletett, string nem, string genetika, string szemelyiseg)
        {
            return $@"<p style=""text-align: left;""><strong>Név</strong>: <br />{nev}</p>
<p style=""text-align: left;""><strong>Született</strong>: <br />{szuletett}</p>
<p style=""text-align: left;""><strong>Nem</strong>: <br />{nem}</p>
<p style=""text-align: left;""><strong>Genetika</strong>: <br />{genetika}</p>
<p style=""text-align: left;""><strong>Személyiség</strong>: <br />{szemelyiseg}</p>";
        }

        // 2. HTML Visszafejtése 
        public ReptileDetails ParseHtmlDescription(string htmlDescription)
        {
            var details = new ReptileDetails();
            if (string.IsNullOrWhiteSpace(htmlDescription)) return details;

            string decodedHtml = WebUtility.HtmlDecode(htmlDescription);

            var matchNev = Regex.Match(decodedHtml, @"<strong>Név</strong>:\s*<br\s*/>\s*(.*?)\s*</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (matchNev.Success) details.Nev = matchNev.Groups[1].Value.Trim();

            var matchSzuletett = Regex.Match(decodedHtml, @"<strong>Született</strong>:\s*<br\s*/>\s*(.*?)\s*</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (matchSzuletett.Success && DateTime.TryParse(matchSzuletett.Groups[1].Value.Trim(), out DateTime parsedDate))
            {
                details.Szuletett = parsedDate;
            }

            var matchNem = Regex.Match(decodedHtml, @"<strong>Nem</strong>:\s*<br\s*/>\s*(.*?)\s*</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (matchNem.Success) details.Nem = matchNem.Groups[1].Value.Trim();

            var matchGenetika = Regex.Match(decodedHtml, @"<strong>Genetika</strong>:\s*<br\s*/>\s*(.*?)\s*</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (matchGenetika.Success) details.Genetika = matchGenetika.Groups[1].Value.Trim();

            var matchSzemelyiseg = Regex.Match(decodedHtml, @"<strong>Személyiség</strong>:\s*<br\s*/>\s*(.*?)\s*</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (matchSzemelyiseg.Success) details.Szemelyiseg = matchSzemelyiseg.Groups[1].Value.Trim();

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

        // 5. Raktárkészlet státuszszín meghatározása
        public Color GetStockStatusColor(int availableStock)
        {
            if (availableStock <= 0)
            {
                return Color.Red;
            }
            return Color.DarkGreen;
        }

        // 6. Új hüllő adatainak validálása
        public bool ValidateNewReptile(string productName, string sku, byte[] imageBytes, out string errorMessage)
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

        // 7. A gigantikus ProductDTO összerakása (hogy ne a Formot szemetelje tele)
        public Hotcakes.CommerceDTO.v1.Catalog.ProductDTO CreateNewReptileDTO(string sku, string productName, decimal listPrice, decimal sitePrice, string htmlDescription, bool isAvailable)
        {
            return new Hotcakes.CommerceDTO.v1.Catalog.ProductDTO
            {
                Sku = sku,
                ProductName = productName,
                ListPrice = listPrice,
                SitePrice = sitePrice,
                LongDescription = htmlDescription,
                IsAvailableForSale = isAvailable,

                // Alapértelmezett "behuzalozott" értékek az új állatokhoz
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
                UpchargeAmount = 3m,
                UpchargeUnit = "1"
            };
        }
    }
}
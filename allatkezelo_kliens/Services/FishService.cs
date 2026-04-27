using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;

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
    }

    public class FishService : IFishService
    {
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
    }
}

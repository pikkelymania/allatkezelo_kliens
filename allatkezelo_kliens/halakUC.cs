using Hotcakes.CommerceDTO.v1.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace allatkezelo_kliens
{
    public partial class halakUC : UserControl
    {
        private Hotcakes.CommerceDTO.v1.Client.Api _api;
        private List<Hotcakes.CommerceDTO.v1.Catalog.CategorySnapshotDTO> _mindenKategoria;
        //private Hotcakes.CommerceDTO.v1.Catalog.ProductDTO _kivalasztottTermek;

        private const string ApiKey = "1-45782d8b-85b9-4924-aafe-ea09050cbc9e";
        private const string StoreUrl = "http://www.pikkelymania.hu/";
        public halakUC()
        {
            InitializeComponent();
        }

        private void halakUC_Load(object sender, EventArgs e)
        {
            try
            {
                // Az API kliens inicializálása
                _api = new Api(StoreUrl, ApiKey);

                // Kategóriák letöltése a memóriába (hogy a gomboknak ne kelljen újra tölteniük)
                var catResponse = _api.CategoriesFindAll();

                if (catResponse.Errors == null || catResponse.Errors.Count == 0)
                {
                    _mindenKategoria = catResponse.Content;

                    // OPCIONÁLIS: Betöltéskor alapból mutassa az összes "Hüllők" kategóriás terméket
                    KategoriaSzures("Halak");
                }
                else
                {
                    MessageBox.Show($"Hiba a kategóriák lekérésekor: {catResponse.Errors[0].Description}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt: {ex.Message}");
            }
        }
        private void KategoriaSzures(string alkategoriaNev)
        {
            if (_mindenKategoria == null) return;

            // Megkeressük az alkategóriát a név alapján a letöltött listából
            var kategoria = _mindenKategoria.FirstOrDefault(c => c.Name.Equals(alkategoriaNev, StringComparison.OrdinalIgnoreCase));

            if (kategoria != null)
            {
                // Termékek lekérése a kategória azonosítója alapján
                var prodResponse = _api.ProductsFindForCategory(kategoria.Bvin, 1, 100);

                if (prodResponse.Errors == null || prodResponse.Errors.Count == 0)
                {
                    // A terméklista megjelenítése a táblázatban
                    dataGridView1.DataSource = prodResponse.Content.Products.OrderBy(p => p.Sku).ToList();

                    string[] lathatoOszlopok = { "Sku", "ProductName", "SitePrice", "ListPrice", "LongDescription", "IsAvailableForSale" };

                    foreach (DataGridViewColumn oszlop in dataGridView1.Columns)
                    {
                        // Ha az oszlop neve nincs benne a fenti listában, akkor elrejtjük
                        if (!lathatoOszlopok.Contains(oszlop.Name))
                        {
                            oszlop.Visible = false;
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show($"Nem található '{alkategoriaNev}' nevű kategória.");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            AktivGombKijeloles((Button)sender);
            KategoriaSzures("Halak");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AktivGombKijeloles((Button)sender);
            KategoriaSzures("Talajlakók");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AktivGombKijeloles((Button)sender);
            KategoriaSzures("Nano halak");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            AktivGombKijeloles((Button)sender);
            KategoriaSzures("Sziámi harcoshalak");
        }
        private void AktivGombKijeloles(Button klikkeltGomb)
        {
            // 1. Definiáljuk a színeket (ezeket írd át a saját dizájnodhoz!)
            Color alapHatter = SystemColors.ControlLight; // Világosszürke
            Color alapBetu = Color.Black;                     // Fekete betű

            Color aktivHatter = Color.FromArgb(120, 120, 120);
            Color aktivBetu = Color.White;                    // Fehér betű az aktívnak

            // 2. Végigmegyünk azon a panelen, amiben a gombok vannak
            foreach (Control vezerlo in klikkeltGomb.Parent.Controls)
            {
                // Ha a vezérlő egy gomb, akkor visszaállítjuk az alapszínére
                if (vezerlo is Button gomb)
                {
                    gomb.BackColor = alapHatter;
                    gomb.ForeColor = alapBetu;
                }
            }

            // 3. A ténylegesen megnyomott gombot átszínezzük az aktív színre
            klikkeltGomb.BackColor = aktivHatter;
            klikkeltGomb.ForeColor = aktivBetu;
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            // Ellenőrizzük, hogy van-e egyáltalán aktív sor és van-e mögötte adat
            if (dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.DataBoundItem != null)
            {
                var kivalasztottTermek = (Hotcakes.CommerceDTO.v1.Catalog.ProductDTO)dataGridView1.CurrentRow.DataBoundItem;

                if (kivalasztottTermek != null)
                {
                    // 1. Alap Hotcakes API adatok a dobozokba
                    txtSku.Text = kivalasztottTermek.Sku;
                    txtProductName.Text = kivalasztottTermek.ProductName;
                    txtListPrice.Text = kivalasztottTermek.ListPrice.ToString("C");
                    txtSitePrice.Text = kivalasztottTermek.SitePrice.ToString("C");
                    chkElerheto.Checked = kivalasztottTermek.IsAvailableForSale;

                    // 2. Kiegészítő adatok visszafejtése a LongDescription HTML kódjából

                    // Először is lenullázzuk a mezőket
                    textboxJellemzok.Text = "";
                    textboxTartas.Text = "";
                    textboxVizparameterek.Text = "";
                    textboxTaplalkozas.Text = "";
                    textboxSzaporitas.Text = "";

                    // Alaphelyzetbe állítjuk a raktárkészlet labelt betöltés közben
                    labelraktaron.Text = "Betöltés...";
                    labelraktaron.ForeColor = Color.Black;

                    // Kivesszük a nyers HTML-t
                    string nyersHtml = kivalasztottTermek.LongDescription ?? "";

                    // --- EZ A LÉNYEG! ---
                    // Visszaalakítjuk a csúnya HTML kódokat (pl. &uuml;) normál ékezetes betűkké!
                    string htmlLeiras = System.Net.WebUtility.HtmlDecode(nyersHtml);

                    // Ha van benne valami, elkezdjük kinyerni az adatokat
                    if (!string.IsNullOrWhiteSpace(htmlLeiras))
                    {
                        // Jellemzők kinyerése
                        var matchJellemzok = System.Text.RegularExpressions.Regex.Match(htmlLeiras, @"<strong>Jellemzők</strong>:\s*(.*?)\s*</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                        if (matchJellemzok.Success)
                            textboxJellemzok.Text = System.Net.WebUtility.HtmlDecode(matchJellemzok.Groups[1].Value.Trim());

                        // Tartás kinyerése (Figyelve a "Tart&aacute;s" és "Tartás" verziókra is)
                        var matchTartas = System.Text.RegularExpressions.Regex.Match(htmlLeiras, @"<strong>Tart(?:ás|&aacute;s)</strong>:\s*(.*?)\s*</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                        if (matchTartas.Success)
                            textboxTartas.Text = System.Net.WebUtility.HtmlDecode(matchTartas.Groups[1].Value.Trim());

                        // Vízparaméterek kinyerése
                        var matchViz = System.Text.RegularExpressions.Regex.Match(htmlLeiras, @"<strong>V(?:ízparaméterek|&iacute;zparam&eacute;terek)</strong>:\s*(.*?)\s*</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                        if (matchViz.Success)
                            textboxVizparameterek.Text = System.Net.WebUtility.HtmlDecode(matchViz.Groups[1].Value.Trim());

                        // Táplálkozás kinyerése
                        var matchTaplalkozas = System.Text.RegularExpressions.Regex.Match(htmlLeiras, @"<strong>T(?:áplálkozás|&aacute;pl&aacute;lkoz&aacute;s)</strong>:\s*(.*?)\s*</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                        if (matchTaplalkozas.Success)
                            textboxTaplalkozas.Text = System.Net.WebUtility.HtmlDecode(matchTaplalkozas.Groups[1].Value.Trim());

                        // Szaporítás kinyerése
                        var matchSzaporitas = System.Text.RegularExpressions.Regex.Match(htmlLeiras, @"<strong>Szapor(?:ítás|&iacute;t&aacute;s)</strong>:\s*(.*?)\s*</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                        if (matchSzaporitas.Success)
                            textboxSzaporitas.Text = System.Net.WebUtility.HtmlDecode(matchSzaporitas.Groups[1].Value.Trim());
                    }

                    // --- 3. RAKTÁRKÉSZLET (INVENTORY) LEKÉRDEZÉSE ÉS MEGJELENÍTÉSE ---
                    try
                    {
                        var keszletValasz = _api.ProductInventoryFindForProduct(kivalasztottTermek.Bvin);
                        int darabszam = 0; // Alapértelmezetten 0-nak vesszük

                        if (keszletValasz.Content != null && keszletValasz.Content.Count > 0)
                        {
                            darabszam = keszletValasz.Content[0].QuantityOnHand - keszletValasz.Content[0].QuantityReserved;
                        }

                        labelraktaron.Text = $"{darabszam} db";

                        // Szín beállítása a darabszám alapján
                        if (darabszam <= 0)
                        {
                            labelraktaron.ForeColor = Color.Red;
                        }
                        else
                        {
                            labelraktaron.ForeColor = Color.DarkGreen;
                        }
                    }
                    catch
                    {
                        labelraktaron.Text = "Hiba";
                        labelraktaron.ForeColor = Color.Red;
                    }
                }
            }
            else
            {
                // Ha valamiért nincs kiválasztva semmi (üres a lista), nullázzuk a labelt
                labelraktaron.Text = "-";
                labelraktaron.ForeColor = Color.Black;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // 1. Ellenőrizzük, hogy van-e kiválasztott termék a táblázatban
            if (dataGridView1.CurrentRow == null || dataGridView1.CurrentRow.DataBoundItem == null)
            {
                MessageBox.Show("Kérlek, először válassz ki egy terméket a listából!", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Kinyerjük a jelenleg kiválasztott terméket közvetlenül a táblázatból
            var szerkesztettTermek = (Hotcakes.CommerceDTO.v1.Catalog.ProductDTO)dataGridView1.CurrentRow.DataBoundItem;

            // 3. Szöveges és logikai adatok frissítése az űrlapról (Alapadatok)
            szerkesztettTermek.Sku = txtSku.Text;
            szerkesztettTermek.ProductName = txtProductName.Text;
            szerkesztettTermek.IsAvailableForSale = chkElerheto.Checked;

            // 4. Halak specifikus adatok kiolvasása a UI elemekből
            // (A Trim() eltávolítja a felesleges szóközöket az elejéről/végéről)
            string jellemzok = textboxJellemzok.Text.Trim();
            string tartas = textboxTartas.Text.Trim();
            string vizparameterek = textboxVizparameterek.Text.Trim();
            string taplalkozas = textboxTaplalkozas.Text.Trim();
            string szaporitas = textboxSzaporitas.Text.Trim();

            // 5. A HTML string összeállítása az ÚJ szerkezet alapján (NINCS <br /> tag!)
            // Figyelem: A dupla idézőjeleket a HTML attribútumokban (mint a style="") meg kell duplázni a C# kódban (""text-align..."")!
            string generaltHtmlLeiras = $@"<p style=""text-align: left;""><strong>Jellemzők</strong>: {jellemzok}</p>
<p style=""text-align: left;""><strong>Tartás</strong>: {tartas}</p>
<p style=""text-align: left;""><strong>Vízparaméterek</strong>: {vizparameterek}</p>
<p style=""text-align: left;""><strong>Táplálkozás</strong>: {taplalkozas}</p>
<p style=""text-align: left;""><strong>Szaporítás</strong>: {szaporitas}</p>";

            // 6. A generált HTML kód betöltése a termék LongDescription mezőjébe
            szerkesztettTermek.LongDescription = generaltHtmlLeiras;

            // 7. Árak visszaalakítása számmá
            // A NumberStyles.Any segít abban, hogy a pénznem jelek (pl. "Ft") és a szóközök ne okozzanak hibát konvertáláskor
            if (decimal.TryParse(txtListPrice.Text, System.Globalization.NumberStyles.Any, null, out decimal ujListPrice))
            {
                szerkesztettTermek.ListPrice = ujListPrice;
            }
            else
            {
                MessageBox.Show("A Listaár formátuma hibás! Kérlek, érvényes számot adj meg.", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Ha hiba van, megszakítjuk a mentést
            }

            if (decimal.TryParse(txtSitePrice.Text, System.Globalization.NumberStyles.Any, null, out decimal ujSitePrice))
            {
                szerkesztettTermek.SitePrice = ujSitePrice;
            }
            else
            {
                MessageBox.Show("Az Eladási ár formátuma hibás! Kérlek, érvényes számot adj meg.", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 8. Küldés az API-nak a webre
            try
            {
                // A ProductsUpdate metódus automatikusan frissíti a terméket a Bvin (rejtett azonosító) alapján
                var valasz = _api.ProductsUpdate(szerkesztettTermek);

                // Ellenőrizzük, hogy a szerver dobott-e vissza valamilyen hibát
                if (valasz.Errors == null || valasz.Errors.Count == 0)
                {
                    MessageBox.Show("A termék adatai sikeresen frissítve a webshopban!", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Frissítjük a táblázatot, hogy azonnal mutassa az új (elmentett) adatokat
                    dataGridView1.Refresh();
                }
                else
                {
                    MessageBox.Show($"Hiba a mentés során a szerveren: {valasz.Errors[0].Description}", "API Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Ha valami váratlan technikai hiba történik (pl. nincs internet)
                MessageBox.Show($"Váratlan hiba történt a kommunikáció során: {ex.Message}", "Hálózati Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // 1. Ellenőrizzük, hogy van-e kijelölt sor
            if (dataGridView1.CurrentRow == null || dataGridView1.CurrentRow.DataBoundItem == null)
            {
                MessageBox.Show("Kérlek, válassz ki egy terméket a törléshez!", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Kinyerjük a kijelölt terméket
            var kivalasztottTermek = (Hotcakes.CommerceDTO.v1.Catalog.ProductDTO)dataGridView1.CurrentRow.DataBoundItem;

            // 3. Megerősítés kérése a felhasználótól
            var megerosites = MessageBox.Show(
                $"Biztosan véglegesen törölni szeretnéd a következő terméket?\n\nNév: {kivalasztottTermek.ProductName}\nSKU: {kivalasztottTermek.Sku}",
                "Törlés megerősítése",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2); // Biztonság kedvéért a "Nem" az alapértelmezett

            if (megerosites == DialogResult.Yes)
            {
                try
                {
                    // 4. API hívás a törléshez (a Bvin alapján)
                    var valasz = _api.ProductsDelete(kivalasztottTermek.Bvin);

                    if (valasz.Errors == null || valasz.Errors.Count == 0)
                    {
                        MessageBox.Show("A termék sikeresen törölve a webshopból!", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // 5. A kezelőfelület frissítése
                        // A legegyszerűbb, ha "újra kattintunk" az éppen aktív kategória gombjára, 
                        // vagy csak meghívjuk a szűrést a halakra, hogy eltűnjön a törölt elem.
                        KategoriaSzures("Halak");

                        // Ha pontosabb akarsz lenni, tárolhatnád egy változóban, mi volt az utolsó szűrés, 
                        // és azt hívnád meg itt.
                    }
                    else
                    {
                        MessageBox.Show($"Hiba történt a törlés során: {valasz.Errors[0].Description}", "API Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Váratlan hiba történt a kommunikáció során: {ex.Message}", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            // 1. Példányosítjuk az új állat felvitelére szolgáló ablakot
            // Átadjuk neki a halakUC-ben már meglévő _api objektumot
            using (var ujAblak = new UjHalTermek(_api))
            {
                // 2. Megjelenítjük az ablakot felugró (Modal) módban
                var eredmeny = ujAblak.ShowDialog();

                // 3. Ha a felhasználó a Mentés gombra kattintott és az API válasza sikeres volt
                if (eredmeny == DialogResult.OK)
                {
                    // Itt frissítjük a főtáblázatot, hogy az új állat azonnal megjelenjen a listában.
                    // Ha van egy külön metódusod a betöltésre (pl. TermekekBetoltese()), hívd meg azt.
                    // Ha nincs, akkor a legegyszerűbb, ha újra lekérdezed a termékeket.

                    MessageBox.Show("Frissítsd a listát, hogy lásd az új terméket!", "Információ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Példa a frissítésre (használd azt a metódust, amivel az elején betöltötted az adatokat):
                    // var termekek = _api.ProductsFindAll();
                    // dataGridView1.DataSource = termekek.Content;
                }
            }
        }
    }
}

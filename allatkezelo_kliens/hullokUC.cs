using allatkezelo_kliens.Services;
using Hotcakes.CommerceDTO.v1.Catalog;
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
    public partial class hullokUC : UserControl
    {
        private Hotcakes.CommerceDTO.v1.Client.Api _api;
        private List<Hotcakes.CommerceDTO.v1.Catalog.CategorySnapshotDTO> _mindenKategoria;
    
        //private Hotcakes.CommerceDTO.v1.Catalog.ProductDTO _kivalasztottTermek;
        private readonly IReptileService _reptileService;

        private const string ApiKey = "1-45782d8b-85b9-4924-aafe-ea09050cbc9e";
        private const string StoreUrl = "http://www.pikkelymania.hu/";
        public hullokUC()
        {

            // 2. Itt építjük fel az igazi kapcsolatot
            var realApi = new Hotcakes.CommerceDTO.v1.Client.Api(StoreUrl, ApiKey);
            var wrapper = new HotcakesApiWrapper(realApi);

            // Injektáljuk a szervizbe
            _reptileService = new ReptileService(wrapper);
            _api = realApi;

            InitializeComponent();
        }

        private void hullokUC_Load(object sender, EventArgs e)
        {
            ThemeManager.ApplyTheme(this);
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
                    KategoriaSzures("Hüllők");
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

            var kategoria = _mindenKategoria.FirstOrDefault(c => c.Name.Equals(alkategoriaNev, StringComparison.OrdinalIgnoreCase));

            if (kategoria != null)
            {
                var prodResponse = _api.ProductsFindForCategory(kategoria.Bvin, 1, 100);

                if (prodResponse.Errors == null || prodResponse.Errors.Count == 0)
                {
                    var termekLista = prodResponse.Content.Products;
                    var megjelenitendoLista = new List<ProductViewModel>();

                    // Végigmegyünk a termékeken és lekérjük a készletüket
                    foreach (var p in termekLista)
                    {
                        int keszlet = 0;
                        int foglalt = 0; // <--- ÚJ VÁLTOZÓ
                        var invValasz = _api.ProductInventoryFindForProduct(p.Bvin);
                        if (invValasz.Content != null && invValasz.Content.Count > 0)
                        {
                            keszlet = _reptileService.CalculateAvailableStock(
                                invValasz.Content[0].QuantityOnHand,
                                invValasz.Content[0].QuantityReserved);

                            foglalt = invValasz.Content[0].QuantityReserved; // <--- KINYERJÜK AZ API-BÓL
                        }

                        megjelenitendoLista.Add(new ProductViewModel
                        {
                            Sku = p.Sku,
                            ProductName = p.ProductName,
                            SitePrice = p.SitePrice,
                            ListPrice = p.ListPrice,
                            LongDescription = p.LongDescription,
                            IsAvailableForSale = p.IsAvailableForSale,
                            Raktarkeszlet = keszlet,
                            Foglalva = foglalt, // <--- BEÁLLÍTJUK A VIEWMODEL-BE
                            EredetiTermek = p
                        });
                    }

                    dataGridView1.DataSource = megjelenitendoLista.OrderBy(x => x.Sku).ToList();

                    // Oszlopok nevei és láthatósága
                    string[] lathatoOszlopok = { "Sku", "ProductName", "ListPrice", "SitePrice", "Raktarkeszlet", "Foglalva", "IsAvailableForSale" };
                    
                    foreach (DataGridViewColumn oszlop in dataGridView1.Columns)
                    {
                        oszlop.Visible = lathatoOszlopok.Contains(oszlop.Name);
                    }

                    // Magyarítás
                    if (dataGridView1.Columns.Contains("Sku")) dataGridView1.Columns["Sku"].HeaderText = "Cikkszám";
                    if (dataGridView1.Columns.Contains("ProductName")) dataGridView1.Columns["ProductName"].HeaderText = "Megnevezés";
                    if (dataGridView1.Columns.Contains("SitePrice"))
                    {
                        dataGridView1.Columns["SitePrice"].HeaderText = "Eladási ár";
                        dataGridView1.Columns["SitePrice"].DefaultCellStyle.Format = "C0";
                    }

                    if (dataGridView1.Columns.Contains("ListPrice"))
                    {
                        dataGridView1.Columns["ListPrice"].HeaderText = "Listaár";
                        dataGridView1.Columns["ListPrice"].DefaultCellStyle.Format = "C0";
                    }
                    if (dataGridView1.Columns.Contains("Raktarkeszlet")) dataGridView1.Columns["Raktarkeszlet"].HeaderText = "Raktáron (db)";
                    if (dataGridView1.Columns.Contains("Foglalva")) dataGridView1.Columns["Foglalva"].HeaderText = "Foglalva";
                    if (dataGridView1.Columns.Contains("IsAvailableForSale")) dataGridView1.Columns["IsAvailableForSale"].HeaderText = "Elérhető";
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AktivGombKijeloles((Button)sender);
            KategoriaSzures("Királypitonok");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AktivGombKijeloles((Button)sender);
            KategoriaSzures("Szakállas agámák");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            AktivGombKijeloles((Button)sender);
            KategoriaSzures("Leopárdgekkók");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            AktivGombKijeloles((Button)sender);
            KategoriaSzures("Vitorlásgekkók");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            AktivGombKijeloles((Button)sender);
            KategoriaSzures("Gabonasiklók");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            AktivGombKijeloles((Button)sender);
            KategoriaSzures("Hüllők");
        }

        private void AktivGombKijeloles(Button klikkeltGomb)
        {
            // A többi gomb visszaállítása a látható alapállapotra
            foreach (Control vezerlo in klikkeltGomb.Parent.Controls)
            {
                if (vezerlo is Button gomb)
                {
                    gomb.BackColor = Color.White;                   // Fehér "doboz"
                    gomb.ForeColor = Color.FromArgb(30, 41, 59);    // Sötétszürke szöveg
                }
            }

            // A kiválasztott gomb elegáns ZÖLD kiemelése
            klikkeltGomb.BackColor = Color.FromArgb(220, 235, 230); // Halvány menta háttér
            klikkeltGomb.ForeColor = Color.FromArgb(55, 95, 82);    // Sötétzöld szöveg
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            // Ellenőrizzük, hogy van-e egyáltalán aktív sor és van-e mögötte adat
            if (dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.DataBoundItem != null)
            {
                // 1. ÁTÍRVA: Most már a ProductViewModel-t vesszük ki a táblázatból!
                var vm = (ProductViewModel)dataGridView1.CurrentRow.DataBoundItem;

                // Ebből nyerjük ki az eredeti termékadatokat
                var kivalasztottTermek = vm.EredetiTermek;

                if (kivalasztottTermek != null)
                {
                    // 2. Alap Hotcakes API adatok a dobozokba
                    txtSku.Text = kivalasztottTermek.Sku;
                    txtProductName.Text = kivalasztottTermek.ProductName;
                    txtListPrice.Text = kivalasztottTermek.ListPrice.ToString("C");
                    txtSitePrice.Text = kivalasztottTermek.SitePrice.ToString("C");
                    chkElerheto.Checked = kivalasztottTermek.IsAvailableForSale;

                    // 3. Kiegészítő adatok visszafejtése a LongDescription HTML kódjából
                    textboxNev.Text = "";
                    dateTimePicker1.Value = DateTime.Now;
                    comboBoxNem.Text = "";
                    textboxGenetika.Text = "";
                    textboxSzemelyiseg.Text = "";

                    string nyersHtml = kivalasztottTermek.LongDescription ?? "";
                    var reszletek = _reptileService.ParseHtmlDescription(nyersHtml);

                    textboxNev.Text = reszletek.Nev;
                    if (reszletek.Szuletett.HasValue) dateTimePicker1.Value = reszletek.Szuletett.Value;
                    comboBoxNem.Text = reszletek.Nem;
                    textboxGenetika.Text = reszletek.Genetika;
                    textboxSzemelyiseg.Text = reszletek.Szemelyiseg;

                    // --- 4. RAKTÁRKÉSZLET MEGJELENÍTÉSE (OPTIMALIZÁLVA) ---

                    // RAKTÁRKÉSZLET MEGJELENÍTÉSE
                    labelraktaron.Text = $"{vm.Raktarkeszlet} db";
                    labelraktaron.ForeColor = _reptileService.GetStockStatusColor(vm.Raktarkeszlet);

                    // --- FOGLALT MENNYISÉG MEGJELENÍTÉSE ---
                    labelfoglalt.Text = $"{vm.Foglalva} db";
                    labelfoglalt.ForeColor = Color.FromArgb(184, 134, 11);

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
            var vm = (ProductViewModel)dataGridView1.CurrentRow.DataBoundItem;
            var szerkesztettTermek = vm.EredetiTermek;

            // 3. Szöveges és logikai adatok frissítése az űrlapról
            szerkesztettTermek.Sku = txtSku.Text;
            szerkesztettTermek.ProductName = txtProductName.Text;
            szerkesztettTermek.IsAvailableForSale = chkElerheto.Checked;

            // 4. HTML leírás generálása a Service segítségével
            szerkesztettTermek.LongDescription = _reptileService.BuildLongDescription(
                textboxNev.Text.Trim(),
                dateTimePicker1.Value.ToString("yyyy. MM. dd."),
                comboBoxNem.Text.Trim(),
                textboxGenetika.Text.Trim(),
                textboxSzemelyiseg.Text.Trim()
            );

            // 5. Árak validálása a Service segítségével
            if (!_reptileService.ValidatePrices(txtListPrice.Text, txtSitePrice.Text, out decimal ujListPrice, out decimal ujSitePrice))
            {
                MessageBox.Show("Az árak formátuma hibás! Kérlek, érvényes számot adj meg.", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; 
            }

            szerkesztettTermek.ListPrice = ujListPrice;
            szerkesztettTermek.SitePrice = ujSitePrice;

            // 6. Küldés az API-nak a webre
            try
            {
                // A ProductsUpdate metódus automatikusan frissíti a terméket 
                var valasz = _api.ProductsUpdate(szerkesztettTermek);

                if (valasz.Errors == null || valasz.Errors.Count == 0)
                {
                    MessageBox.Show("A termék adatai sikeresen frissítve a webshopban!", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dataGridView1.Refresh();
                }
                else
                {
                    MessageBox.Show($"Hiba a mentés során a szerveren: {valasz.Errors[0].Description}", "API Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Váratlan hiba történt a kommunikáció során: {ex.Message}", "Hálózati Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            // 1. Példányosítjuk az új állat felvitelére szolgáló ablakot
            // Átadjuk neki a hullokUC-ben már meglévő _api objektumot
            using (var ujAblak = new UjHulloTermek(_api))
            {
                var eredmeny = ujAblak.ShowDialog();

                if (eredmeny == DialogResult.OK)
                {
                    MessageBox.Show("Frissítsd a listát, hogy lásd az új terméket!", "Információ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Ellenőrizzük, hogy van-e kijelölt sor
            if (dataGridView1.CurrentRow == null || dataGridView1.CurrentRow.DataBoundItem == null)
            {
                MessageBox.Show("Kérlek, válassz ki egy terméket a törléshez!", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kinyerjük a kijelölt terméket
            var vm = (ProductViewModel)dataGridView1.CurrentRow.DataBoundItem;
            var kivalasztottTermek = vm.EredetiTermek;

            // Megerősítés kérése a felhasználótól
            var megerosites = MessageBox.Show(
                $"Biztosan véglegesen törölni szeretnéd a következő terméket?\n\nNév: {kivalasztottTermek.ProductName}\nSKU: {kivalasztottTermek.Sku}",
                "Törlés megerősítése",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (megerosites == DialogResult.Yes)
            {
                try
                {
                    // API hívás a törléshez (a Bvin alapján)
                    var valasz = _api.ProductsDelete(kivalasztottTermek.Bvin);

                    if (valasz.Errors == null || valasz.Errors.Count == 0)
                    {
                        MessageBox.Show("A termék sikeresen törölve a webshopból!", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        KategoriaSzures("Hüllők");
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
    }
    // Ez az osztály fogja tárolni a terméket és a darabszámot együtt
    public class ProductViewModel
    {
        public string Sku { get; set; }
        public string ProductName { get; set; }
        public decimal SitePrice { get; set; }
        public decimal ListPrice { get; set; }
        public string LongDescription { get; set; }
        public bool IsAvailableForSale { get; set; }
        public int Raktarkeszlet { get; set; }
        public int Foglalva { get; set; }
        public Hotcakes.CommerceDTO.v1.Catalog.ProductDTO EredetiTermek { get; set; }
    }
}

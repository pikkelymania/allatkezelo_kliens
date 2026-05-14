using allatkezelo_kliens.Services;
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
        private readonly allatkezelo_kliens.Services.IFishService _fishService;

        private const string ApiKey = "1-45782d8b-85b9-4924-aafe-ea09050cbc9e";
        private const string StoreUrl = "http://www.pikkelymania.hu/";
        public halakUC()
        {
            InitializeComponent();

            var realApi = new Hotcakes.CommerceDTO.v1.Client.Api("URL", "KEY");
            var wrapper = new HotcakesApiWrapper(realApi);
            _fishService = new FishService(wrapper);
        }

        private void halakUC_Load(object sender, EventArgs e)
        {
            ThemeManager.ApplyTheme(this);
            try
            {
                _api = new Api(StoreUrl, ApiKey);
                var catResponse = _api.CategoriesFindAll();

                if (catResponse.Errors == null || catResponse.Errors.Count == 0)
                {
                    _mindenKategoria = catResponse.Content;
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

            var kategoria = _mindenKategoria.FirstOrDefault(c => c.Name.Equals(alkategoriaNev, StringComparison.OrdinalIgnoreCase));

            if (kategoria != null)
            {
                var prodResponse = _api.ProductsFindForCategory(kategoria.Bvin, 1, 100);

                if (prodResponse.Errors == null || prodResponse.Errors.Count == 0)
                {
                    var termekLista = prodResponse.Content.Products;

                    // --- 1. A KÖZÖS VIEWMODEL HASZNÁLATA ---
                    var megjelenitendoLista = new List<ProductViewModel>();

                    foreach (var p in termekLista)
                    {
                        int keszlet = 0;
                        var invValasz = _api.ProductInventoryFindForProduct(p.Bvin);
                        if (invValasz.Content != null && invValasz.Content.Count > 0)
                        {
                            keszlet = invValasz.Content[0].QuantityOnHand - invValasz.Content[0].QuantityReserved;
                        }

                        megjelenitendoLista.Add(new ProductViewModel
                        {
                            Sku = p.Sku,
                            ProductName = p.ProductName,
                            SitePrice = p.SitePrice,
                            ListPrice = p.ListPrice,
                            LongDescription = p.LongDescription,
                            IsAvailableForSale = p.IsAvailableForSale,
                            Raktarkeszlet = keszlet, // Készlet betöltése
                            EredetiTermek = p
                        });
                    }

                    dataGridView1.DataSource = megjelenitendoLista.OrderBy(x => x.Sku).ToList();

                    // --- 2. OSZLOPOK SZŰRÉSE ÉS FORMÁZÁSA ---
                    string[] lathatoOszlopok = { "Sku", "ProductName", "ListPrice", "SitePrice", "Raktarkeszlet", "IsAvailableForSale" };

                    foreach (DataGridViewColumn oszlop in dataGridView1.Columns)
                    {
                        oszlop.Visible = lathatoOszlopok.Contains(oszlop.Name);
                    }

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
                    if (dataGridView1.Columns.Contains("IsAvailableForSale")) dataGridView1.Columns["IsAvailableForSale"].HeaderText = "Elérhető";
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
            foreach (Control vezerlo in klikkeltGomb.Parent.Controls)
            {
                if (vezerlo is Button gomb)
                {
                    gomb.BackColor = Color.White;
                    gomb.ForeColor = Color.FromArgb(30, 41, 59);
                }
            }

            klikkeltGomb.BackColor = Color.FromArgb(220, 235, 230);
            klikkeltGomb.ForeColor = Color.FromArgb(55, 95, 82);
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.DataBoundItem != null)
            {
                // --- 3. BIZTONSÁGOS KINYERÉS A VIEWMODEL-BŐL ---
                var vm = (ProductViewModel)dataGridView1.CurrentRow.DataBoundItem;
                var kivalasztottTermek = vm.EredetiTermek;

                if (kivalasztottTermek != null)
                {
                    txtSku.Text = kivalasztottTermek.Sku;
                    txtProductName.Text = kivalasztottTermek.ProductName;
                    txtListPrice.Text = kivalasztottTermek.ListPrice.ToString("C");
                    txtSitePrice.Text = kivalasztottTermek.SitePrice.ToString("C");
                    chkElerheto.Checked = kivalasztottTermek.IsAvailableForSale;

                    textboxJellemzok.Text = "";
                    textboxTartas.Text = "";
                    textboxVizparameterek.Text = "";
                    textboxTaplalkozas.Text = "";
                    textboxSzaporitas.Text = "";

                    string nyersHtml = kivalasztottTermek.LongDescription ?? "";
                    string htmlLeiras = System.Net.WebUtility.HtmlDecode(nyersHtml);

                    if (!string.IsNullOrWhiteSpace(htmlLeiras))
                    {
                        var matchJellemzok = System.Text.RegularExpressions.Regex.Match(htmlLeiras, @"<strong>Jellemzők</strong>:\s*(.*?)\s*</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                        if (matchJellemzok.Success) textboxJellemzok.Text = System.Net.WebUtility.HtmlDecode(matchJellemzok.Groups[1].Value.Trim());

                        var matchTartas = System.Text.RegularExpressions.Regex.Match(htmlLeiras, @"<strong>Tart(?:ás|&aacute;s)</strong>:\s*(.*?)\s*</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                        if (matchTartas.Success) textboxTartas.Text = System.Net.WebUtility.HtmlDecode(matchTartas.Groups[1].Value.Trim());

                        var matchViz = System.Text.RegularExpressions.Regex.Match(htmlLeiras, @"<strong>V(?:ízparaméterek|&iacute;zparam&eacute;terek)</strong>:\s*(.*?)\s*</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                        if (matchViz.Success) textboxVizparameterek.Text = System.Net.WebUtility.HtmlDecode(matchViz.Groups[1].Value.Trim());

                        var matchTaplalkozas = System.Text.RegularExpressions.Regex.Match(htmlLeiras, @"<strong>T(?:áplálkozás|&aacute;pl&aacute;lkoz&aacute;s)</strong>:\s*(.*?)\s*</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                        if (matchTaplalkozas.Success) textboxTaplalkozas.Text = System.Net.WebUtility.HtmlDecode(matchTaplalkozas.Groups[1].Value.Trim());

                        var matchSzaporitas = System.Text.RegularExpressions.Regex.Match(htmlLeiras, @"<strong>Szapor(?:ítás|&iacute;t&aacute;s)</strong>:\s*(.*?)\s*</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                        if (matchSzaporitas.Success) textboxSzaporitas.Text = System.Net.WebUtility.HtmlDecode(matchSzaporitas.Groups[1].Value.Trim());
                    }

                    // --- 4. RAKTÁRKÉSZLET AZONNALI MEGJELENÍTÉSE ---
                    int darabszam = vm.Raktarkeszlet;
                    labelraktaron.Text = $"{darabszam} db";

                    if (darabszam <= 0)
                        labelraktaron.ForeColor = Color.Red;
                    else
                        labelraktaron.ForeColor = Color.DarkGreen;
                }
            }
            else
            {
                labelraktaron.Text = "-";
                labelraktaron.ForeColor = Color.Black;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null || dataGridView1.CurrentRow.DataBoundItem == null)
            {
                MessageBox.Show("Kérlek, először válassz ki egy terméket a listából!", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ViewModel-ből kiszedve a mentéshez
            var vm = (ProductViewModel)dataGridView1.CurrentRow.DataBoundItem;
            var szerkesztettTermek = vm.EredetiTermek;

            szerkesztettTermek.Sku = txtSku.Text;
            szerkesztettTermek.ProductName = txtProductName.Text;
            szerkesztettTermek.IsAvailableForSale = chkElerheto.Checked;

            string generaltHtmlLeiras = _fishService.BuildLongDescription(
                textboxJellemzok.Text.Trim(),
                textboxTartas.Text.Trim(),
                textboxVizparameterek.Text.Trim(),
                textboxTaplalkozas.Text.Trim(),
                textboxSzaporitas.Text.Trim()
            );

            szerkesztettTermek.LongDescription = generaltHtmlLeiras;

            if (!_fishService.ValidatePrices(txtListPrice.Text, txtSitePrice.Text, out decimal ujListPrice, out decimal ujSitePrice))
            {
                MessageBox.Show("Az árak formátuma hibás! Kérlek, érvényes számot adj meg.", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            szerkesztettTermek.ListPrice = ujListPrice;
            szerkesztettTermek.SitePrice = ujSitePrice;

            try
            {
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

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null || dataGridView1.CurrentRow.DataBoundItem == null)
            {
                MessageBox.Show("Kérlek, válassz ki egy terméket a törléshez!", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ViewModel-ből kiszedve a törléshez
            var vm = (ProductViewModel)dataGridView1.CurrentRow.DataBoundItem;
            var kivalasztottTermek = vm.EredetiTermek;

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
                    var valasz = _api.ProductsDelete(kivalasztottTermek.Bvin);

                    if (valasz.Errors == null || valasz.Errors.Count == 0)
                    {
                        MessageBox.Show("A termék sikeresen törölve a webshopból!", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        KategoriaSzures("Halak");
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
            using (var ujAblak = new UjHalTermek(_api))
            {
                var eredmeny = ujAblak.ShowDialog();

                if (eredmeny == DialogResult.OK)
                {
                    MessageBox.Show("Frissítsd a listát, hogy lásd az új terméket!", "Információ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}

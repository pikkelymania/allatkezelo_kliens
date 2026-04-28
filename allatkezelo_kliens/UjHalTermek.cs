using Hotcakes.CommerceDTO.v1.Catalog;
using Hotcakes.CommerceDTO.v1.Client;
using Hotcakes.CommerceDTO.v1.Shipping;
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

    public partial class UjHalTermek : Form
    {
        private Api _api;
        private byte[] _kivalasztottKepByteok = null;
        private string _halakFokategoriaBvin = "";
        private string _eloAllatokFokategoriaBvin = "";
        private readonly allatkezelo_kliens.Services.IFishService _fishService;
        public UjHalTermek(Api api)
        {
            InitializeComponent();
            _api = api;

            // 1. Csomagoljuk be a kapott api-t a Wrapperbe
            var wrapper = new allatkezelo_kliens.Services.HotcakesApiWrapper(_api);

            // 2. Adjuk át a Service-nek
            _fishService = new allatkezelo_kliens.Services.FishService(wrapper);

            // Kategóriák betöltése a Hotcakes-ből a comboBoxKategoria-ba
            KategoriakBetoltese();
        }
        private void KategoriakBetoltese()
        {
            try
            {
                var kategoriakValasz = _api.CategoriesFindAll();
                if (kategoriakValasz.Content != null)
                {
                    var osszesKategoria = kategoriakValasz.Content;

                    // 1. Megkeressük a "Halak" főkategóriát
                    var halakKategoria = osszesKategoria.FirstOrDefault(k => k.Name.Equals("Halak", StringComparison.OrdinalIgnoreCase));
                    if (halakKategoria != null)
                    {
                        _halakFokategoriaBvin = halakKategoria.Bvin;

                        // Kikeressük a "Halak" kategória alkategóriáit (pl. Elevenszülők, Lazacfélék, stb.)
                        var halakAlkategoriak = osszesKategoria.Where(k => k.ParentId == halakKategoria.Bvin).ToList();

                        // Bekötjük a listát a ComboBox-ba
                        comboBoxKategoria.DataSource = halakAlkategoriak;
                        comboBoxKategoria.DisplayMember = "Name";
                        comboBoxKategoria.ValueMember = "Bvin";
                    }

                    // 2. Megkeressük az "Élő állatok" legfelső kategóriát is
                    var eloAllatKateg = osszesKategoria.FirstOrDefault(k => k.Name.Equals("Élő Állatok", StringComparison.OrdinalIgnoreCase));
                    if (eloAllatKateg != null)
                    {
                        _eloAllatokFokategoriaBvin = eloAllatKateg.Bvin;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a kategóriák betöltésekor: {ex.Message}");
            }
        }

        private void btnMentes_Click(object sender, EventArgs e)
        {
            // 1. Validáció a Service-en keresztül
            if (!_fishService.ValidateNewFish(txtProductName.Text, txtSku.Text, _kivalasztottKepByteok, out string errorMsg))
            {
                MessageBox.Show(errorMsg, "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Árak ellenőrzése
            if (!_fishService.ValidatePrices(txtListPrice.Text, txtSitePrice.Text, out decimal listPrice, out decimal sitePrice))
            {
                MessageBox.Show("Az árak formátuma hibás! Kérlek, érvényes számot adj meg.", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 3. HTML leírás összeállítása
            string generaltHtmlLeiras = _fishService.BuildLongDescription(
                textBoxJellemzok.Text.Trim(),
                textBoxTartas.Text.Trim(),
                textBoxVizparam.Text.Trim(),
                textBoxTaplalkozas.Text.Trim(),
                textBoxSzaporitas.Text.Trim()
            );

            // 4. A DTO létrehozása
            var ujTermek = _fishService.CreateNewFishDTO(
                txtSku.Text.Trim(),
                txtProductName.Text.Trim(),
                listPrice,
                sitePrice,
                generaltHtmlLeiras,
                chkElerheto.Checked
            );

            // 5. API hívás (A meglévő kódod)
            try
            {
                var valasz = _api.ProductsCreate(ujTermek, _kivalasztottKepByteok);

                if (valasz.Errors != null && valasz.Errors.Count > 0)
                {
                    MessageBox.Show($"API Hiba a termék létrehozásakor: {valasz.Errors[0].Description}", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (valasz.Content != null)
                {
                    string ujTermekBvin = valasz.Content.Bvin;

                    // Kategóriák
                    if (comboBoxKategoria.SelectedValue != null)
                    {
                        _api.CategoryProductAssociationsCreate(new CategoryProductAssociationDTO { ProductId = ujTermekBvin, CategoryId = comboBoxKategoria.SelectedValue.ToString() });
                    }
                    if (!string.IsNullOrEmpty(_halakFokategoriaBvin))
                    {
                        _api.CategoryProductAssociationsCreate(new CategoryProductAssociationDTO { ProductId = ujTermekBvin, CategoryId = _halakFokategoriaBvin });
                    }
                    if (!string.IsNullOrEmpty(_eloAllatokFokategoriaBvin))
                    {
                        _api.CategoryProductAssociationsCreate(new CategoryProductAssociationDTO { ProductId = ujTermekBvin, CategoryId = _eloAllatokFokategoriaBvin });
                    }

                    // Készlet beállítása
                    try
                    {
                        int megadottKeszlet = Convert.ToInt32(numericUpDown1.Value);
                        var keszletValasz = _api.ProductInventoryFindForProduct(ujTermekBvin);

                        if (keszletValasz.Content != null && keszletValasz.Content.Count > 0)
                        {
                            var aktualisKeszlet = keszletValasz.Content[0];
                            aktualisKeszlet.QuantityOnHand = megadottKeszlet;
                            _api.ProductInventoryUpdate(aktualisKeszlet);
                        }
                        else
                        {
                            var ujKeszlet = new ProductInventoryDTO { ProductBvin = ujTermekBvin, QuantityOnHand = megadottKeszlet };
                            _api.ProductInventoryCreate(ujKeszlet);
                        }

                        valasz.Content.InventoryMode = ProductInventoryModeDTO.WhenOutOfStockShow;
                        _api.ProductsUpdate(valasz.Content);
                    }
                    catch (Exception invEx)
                    {
                        MessageBox.Show($"A termék létrejött, de a készlet/megjelenés módosítása sikertelen: {invEx.Message}", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                MessageBox.Show("A hal sikeresen hozzáadva a rendszerhez!", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hálózati vagy rendszerhiba történt: {ex.Message}", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnMegse_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnKepTallozas_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Válassz egy képet a halhoz";
                ofd.Filter = "Képfájlok (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _kivalasztottKepByteok = System.IO.File.ReadAllBytes(ofd.FileName);
                    MessageBox.Show("Kép sikeresen kiválasztva!", "Információ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}

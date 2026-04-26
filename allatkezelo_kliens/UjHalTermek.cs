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
        public UjHalTermek(Api api)
        {
            InitializeComponent();
            _api = api;

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
            // 1. Kötelező adatok gyors ellenőrzése
            if (string.IsNullOrWhiteSpace(txtProductName.Text) || string.IsNullOrWhiteSpace(txtSku.Text))
            {
                MessageBox.Show("A Terméknév és a Cikkszám megadása kötelező!", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_kivalasztottKepByteok == null)
            {
                MessageBox.Show("Kérlek, válassz ki egy képet a mentés előtt!", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. A generált HTML leírás összeállítása a Halak új szerkezete alapján
            string jellemzok = textBoxJellemzok.Text.Trim();
            string tartas = textBoxTartas.Text.Trim();
            string vizparameterek = textBoxVizparam.Text.Trim();
            string taplalkozas = textBoxTaplalkozas.Text.Trim();
            string szaporitas = textBoxSzaporitas.Text.Trim();

            // A $@ jelzés teszi lehetővé a többsoros stringet. NINCS <br />, egyből írjuk az adatot!
            string generaltHtmlLeiras = $@"<p style=""text-align: left;""><strong>Jellemzők</strong>: {jellemzok}</p>
<p style=""text-align: left;""><strong>Tartás</strong>: {tartas}</p>
<p style=""text-align: left;""><strong>Vízparaméterek</strong>: {vizparameterek}</p>
<p style=""text-align: left;""><strong>Táplálkozás</strong>: {taplalkozas}</p>
<p style=""text-align: left;""><strong>Szaporítás</strong>: {szaporitas}</p>";

            // 3. Új Hotcakes Termék objektum létrehozása és kitöltése
            var ujTermek = new ProductDTO();

            ujTermek.Sku = txtSku.Text.Trim();
            ujTermek.ProductName = txtProductName.Text.Trim();
            ujTermek.ProductTypeId = ""; // Empty

            // Árak
            decimal.TryParse(txtListPrice.Text, System.Globalization.NumberStyles.Any, null, out decimal listPrice);
            ujTermek.ListPrice = listPrice;

            decimal.TryParse(txtSitePrice.Text, System.Globalization.NumberStyles.Any, null, out decimal sitePrice);
            ujTermek.SitePrice = sitePrice;

            ujTermek.SitePriceOverrideText = ""; // Empty
            ujTermek.SiteCost = 0m;

            // SEO / Meta adatok
            ujTermek.MetaKeywords = txtMetaK.Text.Trim();
            ujTermek.MetaDescription = txtMetaD.Text.Trim();
            ujTermek.MetaTitle = txtMetaT.Text.Trim();

            // Adó és Szállítás
            ujTermek.TaxExempt = false;
            ujTermek.ShippingDetails = new ShippableItemDTO();
            ujTermek.ShippingDetails.IsNonShipping = true;
            ujTermek.ShippingMode = ShippingModeDTO.ShipFromSite;
            ujTermek.ShippingCharge = ShippingChargeTypeDTO.ChargeShippingAndHandling;

            // Státusz, Dátumok
            ujTermek.Status = ProductStatusDTO.Active;
            ujTermek.CreationDateUtc = DateTime.UtcNow;

            // Leírások
            ujTermek.ShortDescription = ""; // Empty
            ujTermek.LongDescription = generaltHtmlLeiras;

            // Azonosítók és beállítások
            ujTermek.ManufacturerId = "";
            ujTermek.VendorId = "";
            ujTermek.GiftWrapAllowed = false;
            ujTermek.GiftWrapPrice = 0m;
            ujTermek.Keywords = "";
            ujTermek.PreContentColumnId = "";
            ujTermek.PostContentColumnId = "";
            ujTermek.UrlSlug = "";

            // Készlet és Megjelenés
            ujTermek.InventoryMode = ProductInventoryModeDTO.WhenOutOfStockShow;
            ujTermek.IsAvailableForSale = chkElerheto.Checked;
            ujTermek.Featured = false;
            ujTermek.AllowReviews = true;
            ujTermek.StoreId = 1;
            ujTermek.IsSearchable = true;

            // Upcharge beállítások
            ujTermek.AllowUpcharge = false;
            ujTermek.UpchargeAmount = 0m;
            ujTermek.UpchargeUnit = "1";

            // 4. Mentés az API-val 
            try
            {
                var valasz = _api.ProductsCreate(ujTermek, _kivalasztottKepByteok);

                if (valasz.Errors != null && valasz.Errors.Count > 0)
                {
                    MessageBox.Show($"API Hiba a termék létrehozásakor: {valasz.Errors[0].Description}", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 5. Kategóriák hozzárendelése az új termékhez
                if (valasz.Content != null)
                {
                    string ujTermekBvin = valasz.Content.Bvin;

                    // A) Konkrét hal alkategória (ComboBox alapján)
                    if (comboBoxKategoria.SelectedValue != null)
                    {
                        _api.CategoryProductAssociationsCreate(new CategoryProductAssociationDTO
                        {
                            ProductId = ujTermekBvin,
                            CategoryId = comboBoxKategoria.SelectedValue.ToString()
                        });
                    }

                    // B) "Halak" főkategória
                    if (!string.IsNullOrEmpty(_halakFokategoriaBvin))
                    {
                        _api.CategoryProductAssociationsCreate(new CategoryProductAssociationDTO
                        {
                            ProductId = ujTermekBvin,
                            CategoryId = _halakFokategoriaBvin
                        });
                    }

                    // C) "Élő állatok" főkategória
                    if (!string.IsNullOrEmpty(_eloAllatokFokategoriaBvin))
                    {
                        _api.CategoryProductAssociationsCreate(new CategoryProductAssociationDTO
                        {
                            ProductId = ujTermekBvin,
                            CategoryId = _eloAllatokFokategoriaBvin
                        });
                    }

                    // --- 6. KÉSZLET (INVENTORY) BEÁLLÍTÁSA A NUMERICUPDOWN ALAPJÁN ---
                    try
                    {
                        int megadottKeszlet = Convert.ToInt32(numericUpDown1.Value);
                        var keszletValasz = _api.ProductInventoryFindForProduct(ujTermekBvin);

                        if (keszletValasz.Content != null && keszletValasz.Content.Count > 0)
                        {
                            var aktualisKeszlet = keszletValasz.Content[0];
                            aktualisKeszlet.QuantityOnHand = megadottKeszlet; // Itt használjuk a NumericUpDown értékét!
                            _api.ProductInventoryUpdate(aktualisKeszlet);
                        }
                        else
                        {
                            var ujKeszlet = new ProductInventoryDTO
                            {
                                ProductBvin = ujTermekBvin,
                                QuantityOnHand = megadottKeszlet // Itt is!
                            };
                            _api.ProductInventoryCreate(ujKeszlet);
                        }

                        // --- 7. INVENTORY MODE KIKÉNYSZERÍTÉSE (A MEGOLDÁS) ---
                        // Ezzel kiküszöböljük a Hotcakes API bugját: a már létrehozott terméket külön frissítjük!
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

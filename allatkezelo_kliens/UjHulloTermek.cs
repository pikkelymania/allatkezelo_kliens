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
    public partial class UjHulloTermek : Form
    {
        private Api _api;
        private byte[] _kivalasztottKepByteok = null;
        private string _hullokFokategoriaBvin = "";
        private string _eloAllatokFokategoriaBvin = "";
        public UjHulloTermek(Api api)
        {
            InitializeComponent();
            _api = api;

            // Kategóriák (Fajok) betöltése a Hotcakes-ből a comboBoxFaj-ba
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

                    // 1. Megkeressük a "Hüllők" kategóriát (ezt már megcsináltuk)
                    var hullokKategoria = osszesKategoria.FirstOrDefault(k => k.Name.Equals("Hüllők", StringComparison.OrdinalIgnoreCase));
                    if (hullokKategoria != null)
                    {
                        _hullokFokategoriaBvin = hullokKategoria.Bvin;
                        var hullokAlkategoriak = osszesKategoria.Where(k => k.ParentId == hullokKategoria.Bvin).ToList();
                        comboBoxFaj.DataSource = hullokAlkategoriak;
                        comboBoxFaj.DisplayMember = "Name";
                        comboBoxFaj.ValueMember = "Bvin";
                    }

                    // 2. ÚJ: Megkeressük az "Élő állatok" kategóriát is
                    var eloAllatKateg = osszesKategoria.FirstOrDefault(k => k.Name.Equals("Élő Állatok", StringComparison.OrdinalIgnoreCase));
                    if (eloAllatKateg != null)
                    {
                        _eloAllatokFokategoriaBvin = eloAllatKateg.Bvin;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
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

            // 2. A generált HTML leírás összeállítása
            string nev = textboxNev.Text.Trim();
            string szuletett = dateTimePicker1.Value.ToString("yyyy. MM. dd.");
            string nem = comboBoxNem.Text.Trim();
            string genetika = textboxGenetika.Text.Trim();
            string szemelyiseg = textboxSzemelyiseg.Text.Trim();

            string generaltHtmlLeiras = $@"<p style=""text-align: left;""><strong>Név</strong>: <br />{nev}</p>
<p style=""text-align: left;""><strong>Született</strong>: <br />{szuletett}</p>
<p style=""text-align: left;""><strong>Nem</strong>: <br />{nem}</p>
<p style=""text-align: left;""><strong>Genetika</strong>: <br />{genetika}</p>
<p style=""text-align: left;""><strong>Személyiség</strong>: <br />{szemelyiseg}</p>";

            // 3. Új Hotcakes Termék objektum létrehozása és kitöltése a listád alapján
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
            ujTermek.ShippingMode = ShippingModeDTO.ShipFromSite;
            ujTermek.ShippingCharge = ShippingChargeTypeDTO.ChargeShippingAndHandling;

            // Státusz, Dátumok
            ujTermek.Status = ProductStatusDTO.Active;
            ujTermek.CreationDateUtc = DateTime.UtcNow;

            // Leírások
            ujTermek.ShortDescription = ""; // Empty
            ujTermek.LongDescription = generaltHtmlLeiras;

            // Azonosítók és beállítások
            ujTermek.ManufacturerId = ""; // Empty
            ujTermek.VendorId = ""; // Empty
            ujTermek.GiftWrapAllowed = false;
            ujTermek.GiftWrapPrice = 0m;
            ujTermek.Keywords = "";
            ujTermek.PreContentColumnId = ""; // Empty
            ujTermek.PostContentColumnId = ""; // Empty
            ujTermek.UrlSlug = "";

            // Készlet és Megjelenés
            ujTermek.InventoryMode = ProductInventoryModeDTO.WhenOutOfStockHide;
            ujTermek.IsAvailableForSale = chkElerheto.Checked;
            ujTermek.Featured = false;
            ujTermek.AllowReviews = true;
            ujTermek.StoreId = 1;
            ujTermek.IsSearchable = true;

            // Upcharge beállítások
            ujTermek.AllowUpcharge = false;
            ujTermek.UpchargeAmount = 3m;
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

                // 5. Kategória (Faj) hozzárendelése az új termékhez
                if (valasz.Content != null)
                {
                    string ujTermekBvin = valasz.Content.Bvin;

                    // A) Konkrét fajta (ComboBox alapján)
                    if (comboBoxFaj.SelectedValue != null)
                    {
                        _api.CategoryProductAssociationsCreate(new CategoryProductAssociationDTO
                        {
                            ProductId = ujTermekBvin,
                            CategoryId = comboBoxFaj.SelectedValue.ToString()
                        });
                    }

                    // B) "Hüllők" főkategória
                    if (!string.IsNullOrEmpty(_hullokFokategoriaBvin))
                    {
                        _api.CategoryProductAssociationsCreate(new CategoryProductAssociationDTO
                        {
                            ProductId = ujTermekBvin,
                            CategoryId = _hullokFokategoriaBvin
                        });
                    }

                    // C) ÚJ: "Élő állatok" főkategória
                    if (!string.IsNullOrEmpty(_eloAllatokFokategoriaBvin))
                    {
                        _api.CategoryProductAssociationsCreate(new CategoryProductAssociationDTO
                        {
                            ProductId = ujTermekBvin,
                            CategoryId = _eloAllatokFokategoriaBvin
                        });
                    }

                    // --- 6. KÉSZLET (INVENTORY) BEÁLLÍTÁSA 1-RE ---
                    try
                    {
                        // JAVÍTVA: Egyes számú API hívások (ProductInventory...)
                        var keszletValasz = _api.ProductInventoryFindForProduct(ujTermekBvin);

                        if (keszletValasz.Content != null && keszletValasz.Content.Count > 0)
                        {
                            var aktualisKeszlet = keszletValasz.Content[0];
                            aktualisKeszlet.QuantityOnHand = 1;
                            _api.ProductInventoryUpdate(aktualisKeszlet);
                        }
                        else
                        {
                            var ujKeszlet = new ProductInventoryDTO
                            {
                                ProductBvin = ujTermekBvin, // JAVÍTVA: ProductId helyett ProductBvin
                                QuantityOnHand = 1
                            };
                            _api.ProductInventoryCreate(ujKeszlet);
                        }
                    }
                    catch (Exception invEx)
                    {
                        MessageBox.Show($"A termék létrejött, de a készlet módosítása sikertelen: {invEx.Message}", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                } // JAVÍTVA: Itt zárjuk le a valasz.Content != null blokkot!

                MessageBox.Show("Az állat sikeresen hozzáadva a rendszerhez!", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
                ofd.Title = "Válassz egy képet az állathoz";
                // Csak a leggyakoribb képformátumokat engedjük
                ofd.Filter = "Képfájlok (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    // Beolvassuk a fájlt a számítógépről a memóriába
                    _kivalasztottKepByteok = System.IO.File.ReadAllBytes(ofd.FileName);

                    // Megjelenítjük a fájl nevét a felületen (ha csináltál hozzá egy lblKepNev labelt)
                    // lblKepNev.Text = System.IO.Path.GetFileName(ofd.FileName);

                    MessageBox.Show("Kép sikeresen kiválasztva!", "Információ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}

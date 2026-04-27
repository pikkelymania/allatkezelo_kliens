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
        private readonly allatkezelo_kliens.Services.IReptileService _reptileService = new allatkezelo_kliens.Services.ReptileService();
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
            // 1. Kötelező adatok ellenőrzése a Service-en keresztül
            if (!_reptileService.ValidateNewReptile(txtProductName.Text, txtSku.Text, _kivalasztottKepByteok, out string errorMsg))
            {
                MessageBox.Show(errorMsg, "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Árak ellenőrzése a meglévő Service metódussal
            if (!_reptileService.ValidatePrices(txtListPrice.Text, txtSitePrice.Text, out decimal listPrice, out decimal sitePrice))
            {
                MessageBox.Show("Az árak formátuma hibás! Kérlek, érvényes számot adj meg.", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 3. A generált HTML leírás összeállítása a Service-szel
            string generaltHtmlLeiras = _reptileService.BuildLongDescription(
                textboxNev.Text.Trim(),
                dateTimePicker1.Value.ToString("yyyy. MM. dd."),
                comboBoxNem.Text.Trim(),
                textboxGenetika.Text.Trim(),
                textboxSzemelyiseg.Text.Trim()
            );

            // 4. Új Hotcakes Termék objektum kérése a Service-től
            var ujTermek = _reptileService.CreateNewReptileDTO(
                txtSku.Text.Trim(),
                txtProductName.Text.Trim(),
                listPrice,
                sitePrice,
                generaltHtmlLeiras,
                chkElerheto.Checked
            );

            // 5. Mentés az API-val (INNENTŐL A RÉGI KÓD MARAD)
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

                    // Kategória (Faj) hozzárendelése
                    if (comboBoxFaj.SelectedValue != null)
                    {
                        _api.CategoryProductAssociationsCreate(new CategoryProductAssociationDTO
                        {
                            ProductId = ujTermekBvin,
                            CategoryId = comboBoxFaj.SelectedValue.ToString()
                        });
                    }

                    if (!string.IsNullOrEmpty(_hullokFokategoriaBvin))
                    {
                        _api.CategoryProductAssociationsCreate(new CategoryProductAssociationDTO
                        {
                            ProductId = ujTermekBvin,
                            CategoryId = _hullokFokategoriaBvin
                        });
                    }

                    if (!string.IsNullOrEmpty(_eloAllatokFokategoriaBvin))
                    {
                        _api.CategoryProductAssociationsCreate(new CategoryProductAssociationDTO
                        {
                            ProductId = ujTermekBvin,
                            CategoryId = _eloAllatokFokategoriaBvin
                        });
                    }

                    // KÉSZLET BEÁLLÍTÁSA FIX 1-RE
                    try
                    {
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
                                ProductBvin = ujTermekBvin,
                                QuantityOnHand = 1
                            };
                            _api.ProductInventoryCreate(ujKeszlet);
                        }

                        // INVENTORY MODE KIKÉNYSZERÍTÉSE 
                        valasz.Content.InventoryMode = ProductInventoryModeDTO.WhenOutOfStockShow;
                        _api.ProductsUpdate(valasz.Content);
                    }
                    catch (Exception invEx)
                    {
                        MessageBox.Show($"A termék létrejött, de a készlet/megjelenés módosítása sikertelen: {invEx.Message}", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

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

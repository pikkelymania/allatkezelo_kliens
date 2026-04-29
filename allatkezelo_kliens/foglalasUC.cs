using Hotcakes.CommerceDTO.v1.Catalog;
using Hotcakes.CommerceDTO.v1.Client;
using Hotcakes.CommerceDTO.v1.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using allatkezelo_kliens.Services; // Az új Service névtér

namespace allatkezelo_kliens
{
    public partial class foglalasUC : UserControl
    {
        private Api _api;
        private List<CategorySnapshotDTO> _mindenKategoria;
        private readonly IOrderService _orderService = new OrderService();

        private const string ApiKey = "1-45782d8b-85b9-4924-aafe-ea09050cbc9e";
        private const string StoreUrl = "http://www.pikkelymania.hu/";

        public foglalasUC()
        {
            InitializeComponent();
        }

        private void foglalasUC_Load(object sender, EventArgs e)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            try
            {
                _api = new Api(StoreUrl, ApiKey);
                var catResponse = _api.CategoriesFindAll();

                if (catResponse.Errors == null || catResponse.Errors.Count == 0)
                {
                    _mindenKategoria = catResponse.Content;
                    KategoriaSzures("Élő állatok");
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

        private async void KategoriaSzures(string kategoriaNev)
        {
            if (_mindenKategoria == null || _api == null) return;

            try
            {
                dataGridView1.DataSource = null;
                Cursor.Current = Cursors.WaitCursor;

                var megjelenitendoAdatok = await Task.Run(() =>
                {
                    var eredmenyLista = new List<RendelesViewModel>();

                    var kategoria = _mindenKategoria.FirstOrDefault(c => c.Name.Equals(kategoriaNev, StringComparison.OrdinalIgnoreCase));
                    if (kategoria == null) return null;

                    var prodResponse = _api.ProductsFindForCategory(kategoria.Bvin, 1, 1000);
                    if (prodResponse.Errors != null && prodResponse.Errors.Count > 0)
                    {
                        throw new Exception(prodResponse.Errors[0].Description);
                    }

                    var termekLista = prodResponse.Content.Products;
                    if (termekLista == null || termekLista.Count == 0) return eredmenyLista;

                    var kategoriaTermekBvinLista = termekLista.Select(p => p.Bvin).ToList();
                    var rendelesekValasz = _api.OrdersFindAll();
                    var szurtRendelesek = new List<OrderDTO>();

                    if (rendelesekValasz.Content != null)
                    {
                        Parallel.ForEach(rendelesekValasz.Content, new ParallelOptions { MaxDegreeOfParallelism = 15 }, rendelesSnapshot =>
                        {
                            try
                            {
                                var rendelesKereses = _api.OrdersFind(rendelesSnapshot.bvin);
                                if (rendelesKereses.Content != null)
                                {
                                    var teljesRendeles = rendelesKereses.Content;
                                    if (teljesRendeles.Items != null && teljesRendeles.Items.Any(t => kategoriaTermekBvinLista.Contains(t.ProductId)))
                                    {
                                        lock (szurtRendelesek) { szurtRendelesek.Add(teljesRendeles); }
                                    }
                                }
                            }
                            catch { }
                        });
                    }

                    return szurtRendelesek
                        .OrderByDescending(r => r.TimeOfOrderUtc)
                        .Select(r => _orderService.MapToViewModel(r))
                        .ToList();
                });

                if (megjelenitendoAdatok == null)
                {
                    MessageBox.Show($"Nem található '{kategoriaNev}' nevű kategória.");
                    return;
                }

                dataGridView1.DataSource = megjelenitendoAdatok;

                if (megjelenitendoAdatok.Count == 0)
                {
                    MessageBox.Show($"Jelenleg nincsenek foglalások a(z) '{kategoriaNev}' kategóriában.", "Információ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a lekérdezés során: {ex.Message}", "Hálózati Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.DataBoundItem != null)
            {
                var kivalasztottSor = (RendelesViewModel)dataGridView1.CurrentRow.DataBoundItem;
                var rendeles = kivalasztottSor.EredetiRendeles;

                if (rendeles != null)
                {
                    label1.Text = _orderService.GetCustomerFullName(rendeles.BillingAddress);
                    label3.Text = rendeles.UserEmail;

                    _orderService.GetFirstItemSummary(rendeles.Items, out string pName, out string pSku, out string pQty);
                    label4.Text = pName;
                    label5.Text = pSku;
                    label6.Text = pQty;
                    label8.Text = rendeles.StatusName;
                }
            }
            else
            {
                ResetLabels();
            }
        }

        private void ValtoztassRendelesStatust(string ujKod, string ujNev)
        {
            if (dataGridView1.CurrentRow == null || dataGridView1.CurrentRow.DataBoundItem == null)
            {
                MessageBox.Show("Kérlek, válassz ki egy rendelést a listából!", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var kivalasztottSor = (RendelesViewModel)dataGridView1.CurrentRow.DataBoundItem;
            var rendeles = kivalasztottSor.EredetiRendeles;

            if (rendeles != null)
            {
                rendeles.StatusCode = ujKod;
                rendeles.StatusName = ujNev;

                try
                {
                    var valasz = _api.OrdersUpdate(rendeles);

                    if (valasz.Errors == null || valasz.Errors.Count == 0)
                    {
                        if (ujNev == "Cancelled" && rendeles.Items != null)
                        {
                            foreach (var tetel in rendeles.Items)
                            {
                                try
                                {
                                    var keszletValasz = _api.ProductInventoryFindForProduct(tetel.ProductId);
                                    if (keszletValasz.Content != null && keszletValasz.Content.Count > 0)
                                    {
                                        var aktualisKeszlet = keszletValasz.Content[0];
                                        aktualisKeszlet.QuantityReserved = _orderService.CalculateReleasedInventory(aktualisKeszlet.QuantityReserved, (int)tetel.Quantity);
                                        _api.ProductInventoryUpdate(aktualisKeszlet);
                                    }
                                }
                                catch (Exception invEx)
                                {
                                    MessageBox.Show($"Hiba a(z) {tetel.ProductName} készletének visszaállításakor: {invEx.Message}", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }
                        // --- INNENTŐL KEZDŐDIK AZ ÚJ RÉSZ A "COMPLETE" STÁTUSZHOZ ---
                        else if (ujNev == "Complete" && rendeles.Items != null)
                        {
                            foreach (var tetel in rendeles.Items)
                            {
                                try
                                {
                                    var keszletValasz = _api.ProductInventoryFindForProduct(tetel.ProductId);
                                    if (keszletValasz.Content != null && keszletValasz.Content.Count > 0)
                                    {
                                        var aktualisKeszlet = keszletValasz.Content[0];
                                        int rendeltDarab = (int)tetel.Quantity;

                                        // Levonjuk az OnHand-ből és a Reserved-ből is a rendelt darabszámot
                                        aktualisKeszlet.QuantityOnHand -= rendeltDarab;
                                        if (aktualisKeszlet.QuantityOnHand < 0) aktualisKeszlet.QuantityOnHand = 0;

                                        aktualisKeszlet.QuantityReserved -= rendeltDarab;
                                        if (aktualisKeszlet.QuantityReserved < 0) aktualisKeszlet.QuantityReserved = 0;

                                        _api.ProductInventoryUpdate(aktualisKeszlet);
                                    }
                                }
                                catch (Exception invEx)
                                {
                                    MessageBox.Show($"Hiba a(z) {tetel.ProductName} készletének frissítésekor: {invEx.Message}", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }
                        // --- ITT VÉGE AZ ÚJ RÉSZNEK ---

                        MessageBox.Show($"A rendelés állapota sikeresen frissítve erre: {ujNev}", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        kivalasztottSor.Státusz = ujNev;
                        dataGridView1.Refresh();
                        label8.Text = ujNev;
                    }
                    else
                    {
                        MessageBox.Show($"Hiba a frissítés során: {valasz.Errors[0].Description}", "API Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Váratlan hiba történt: {ex.Message}", "Hálózati Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void buttonTorles_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null || dataGridView1.CurrentRow.DataBoundItem == null)
            {
                MessageBox.Show("Kérlek, válassz ki egy rendelést!", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var kivalasztottSor = (RendelesViewModel)dataGridView1.CurrentRow.DataBoundItem;
            var rendeles = kivalasztottSor.EredetiRendeles;

            if (rendeles != null)
            {
                var megerosites = MessageBox.Show($"Biztosan törölni szeretnéd a(z) {rendeles.OrderNumber} számú rendelést?", "Törlés megerősítése", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (megerosites == DialogResult.Yes)
                {
                    try
                    {
                        var valasz = _api.OrdersDelete(rendeles.Bvin);
                        if (valasz.Errors == null || valasz.Errors.Count == 0)
                        {
                            MessageBox.Show("A rendelés törölve!", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            var jelenlegiAdatok = (List<RendelesViewModel>)dataGridView1.DataSource;
                            if (jelenlegiAdatok != null)
                            {
                                jelenlegiAdatok.Remove(kivalasztottSor);
                                dataGridView1.DataSource = null;
                                dataGridView1.DataSource = jelenlegiAdatok;
                            }
                            ResetLabels();
                        }
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
            }
        }

        private void ResetLabels()
        {
            label1.Text = "-"; label3.Text = "-"; label4.Text = "-"; label5.Text = "-"; label6.Text = "-"; label8.Text = "-";
        }

        private void AktivGombKijeloles(Button klikkeltGomb)
        {
            foreach (Control vezerlo in klikkeltGomb.Parent.Controls)
            {
                if (vezerlo is Button gomb)
                {
                    gomb.BackColor = SystemColors.ControlLight;
                    gomb.ForeColor = Color.Black;
                }
            }
            klikkeltGomb.BackColor = Color.FromArgb(120, 120, 120);
            klikkeltGomb.ForeColor = Color.White;
        }

        private void btnHullok_Click(object sender, EventArgs e) { KategoriaSzures("Hüllők"); AktivGombKijeloles((Button)sender); }
        private void btnHalak_Click(object sender, EventArgs e) { KategoriaSzures("Halak"); AktivGombKijeloles((Button)sender); }
        private void btnAllOrders_Click(object sender, EventArgs e) { KategoriaSzures("Élő állatok"); AktivGombKijeloles((Button)sender); }
        private void buttonTeljesitve_Click(object sender, EventArgs e) { ValtoztassRendelesStatust("09D7305D-BD95-48d2-A025-16ADC827582A", "Complete"); }
        private void buttonLemondas_Click(object sender, EventArgs e) { ValtoztassRendelesStatust("A7FFDB90-C566-4cf2-93F4-D42367F359D5", "Cancelled"); }
    }
}
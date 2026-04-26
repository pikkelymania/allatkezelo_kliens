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

namespace allatkezelo_kliens
{
    public partial class foglalasUC : UserControl
    {
        private Api _api;
        private List<CategorySnapshotDTO> _mindenKategoria;

        private const string ApiKey = "1-45782d8b-85b9-4924-aafe-ea09050cbc9e";
        private const string StoreUrl = "http://www.pikkelymania.hu/";
        public foglalasUC()
        {
            InitializeComponent();
        }
        public class RendelesViewModel
        {
            public string Rendelésszám { get; set; }
            public string Dátum { get; set; }
            public string Vevő_Neve { get; set; }
            public string Végösszeg { get; set; }
            public string Státuszkód { get; set; }
            public string Státusz { get; set; }

            // Ez az attribútum elrejti ezt a property-t, így a DataGridView NEM csinál belőle oszlopot!
            [Browsable(false)]
            public OrderDTO EredetiRendeles { get; set; }
        }

        private void foglalasUC_Load(object sender, EventArgs e)
        {
            // Tervező-védelem: Ha a VS rajzolja a felületet, ne próbáljon csatlakozni
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            try
            {
                // Az API kliens inicializálása
                _api = new Api(StoreUrl, ApiKey);

                // Kategóriák letöltése a memóriába
                var catResponse = _api.CategoriesFindAll();

                if (catResponse.Errors == null || catResponse.Errors.Count == 0)
                {
                    _mindenKategoria = catResponse.Content;

                    // Betöltéskor alapból mutassa az összes "Élő állatok" kategóriás rendelést
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

        private void btnHullok_Click(object sender, EventArgs e)
        {
            KategoriaSzures("Hüllők");
            AktivGombKijeloles((Button)sender);
        }

        private void btnHalak_Click(object sender, EventArgs e)
        {
            KategoriaSzures("Halak");
            AktivGombKijeloles((Button)sender);
        }
        private async void KategoriaSzures(string kategoriaNev)
        {
            if (_mindenKategoria == null || _api == null) return;

            try
            {
                // UI felkészítése: táblázat ürítése és homokóra (töltés) kurzor beállítása
                dataGridView1.DataSource = null;
                Cursor.Current = Cursors.WaitCursor;

                // --- HÁTTÉRSZÁL INDÍTÁSA ---
                var megjelenitendoAdatok = await Task.Run(() =>
                {
                    // Itt már a mi új ViewModelünket használjuk a dynamic helyett
                    var eredmenyLista = new List<RendelesViewModel>();

                    // 1. Megkeressük a kategóriát
                    var kategoria = _mindenKategoria.FirstOrDefault(c => c.Name.Equals(kategoriaNev, StringComparison.OrdinalIgnoreCase));
                    if (kategoria == null) return null; // A null jelzi majd a főszálnak, hogy nincs ilyen kategória

                    // 2. Termékek lekérése a kategória azonosítója alapján
                    var prodResponse = _api.ProductsFindForCategory(kategoria.Bvin, 1, 1000);

                    if (prodResponse.Errors != null && prodResponse.Errors.Count > 0)
                    {
                        throw new Exception(prodResponse.Errors[0].Description);
                    }

                    var termekLista = prodResponse.Content.Products;
                    if (termekLista == null || termekLista.Count == 0)
                    {
                        return eredmenyLista;
                    }

                    var kategoriaTermekBvinLista = termekLista.Select(p => p.Bvin).ToList();

                    // 3. Összes rendelés lekérése
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
                                        lock (szurtRendelesek)
                                        {
                                            szurtRendelesek.Add(teljesRendeles);
                                        }
                                    }
                                }
                            }
                            catch { }
                        });
                    }

                    // 4. Formázás és rendezés (Itt töltjük fel a ViewModel-t!)
                    eredmenyLista = szurtRendelesek
                        .OrderByDescending(r => r.TimeOfOrderUtc)
                        .Select(r => new RendelesViewModel
                        {
                            Rendelésszám = r.OrderNumber,
                            Dátum = r.TimeOfOrderUtc.ToLocalTime().ToString("yyyy. MM. dd. HH:mm"),
                            Vevő_Neve = r.BillingAddress.FirstName + " " + r.BillingAddress.LastName,
                            Végösszeg = r.TotalGrand.ToString("C"),
                            Státuszkód = r.StatusCode,
                            Státusz = r.StatusName,
                            EredetiRendeles = r // Elmentjük a nyers, teljes API adatot a háttérben
                        }).ToList();

                    return eredmenyLista;
                });

                // --- VISSZATÉRTÜNK A FŐSZÁLRA ---
                if (megjelenitendoAdatok == null)
                {
                    MessageBox.Show($"Nem található '{kategoriaNev}' nevű kategória a rendszerben.");
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

        private void btnAllOrders_Click(object sender, EventArgs e)
        {
            KategoriaSzures("Élő állatok");
            AktivGombKijeloles((Button)sender);
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            // Ellenőrizzük, hogy van-e aktív sor
            if (dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.DataBoundItem != null)
            {
                // Most már az általunk készített RendelesViewModel-re castolunk!
                var kivalasztottSor = (RendelesViewModel)dataGridView1.CurrentRow.DataBoundItem;

                // És elővesszük belőle az elrejtett eredeti rendelést
                var rendeles = kivalasztottSor.EredetiRendeles;

                if (rendeles != null)
                {
                    // Vevő neve (Számlázási címből)
                    if (rendeles.BillingAddress != null)
                        label1.Text = $"{rendeles.BillingAddress.FirstName} {rendeles.BillingAddress.LastName}";
                    else
                        label1.Text = "Ismeretlen vevő";

                    // Vevő E-mail címe
                    label3.Text = rendeles.UserEmail;

                    // Termék adatok (Az első lefoglalt termék lekérése)
                    if (rendeles.Items != null && rendeles.Items.Count > 0)
                    {
                        var elsoTetel = rendeles.Items[0];
                        label4.Text = elsoTetel.ProductName;
                        label5.Text = elsoTetel.ProductSku;
                        label6.Text = elsoTetel.Quantity.ToString();
                    }
                    else
                    {
                        label4.Text = "Nincs termék a rendelésben";
                        label5.Text = "-";
                        label6.Text = "0";
                    }

                    // Rendelés állapota
                    label8.Text = rendeles.StatusName; // Vagy .StatusCode, amelyik neked szimpatikusabb
                }
            }
            else
            {
                // Ha valamiért nincs kiválasztva semmi, lenullázzuk a labeleket
                label1.Text = "-";
                label3.Text = "-";
                label4.Text = "-";
                label5.Text = "-";
                label6.Text = "-";
                label8.Text = "-";
            }
        }
        private void ValtoztassRendelesStatust(string ujKod, string ujNev)
        {
            // 1. Ellenőrizzük, van-e kiválasztott rendelés
            if (dataGridView1.CurrentRow == null || dataGridView1.CurrentRow.DataBoundItem == null)
            {
                MessageBox.Show("Kérlek, válassz ki egy rendelést a listából!", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Kinyerjük az adatokat
            var kivalasztottSor = (RendelesViewModel)dataGridView1.CurrentRow.DataBoundItem;
            var rendeles = kivalasztottSor.EredetiRendeles;

            if (rendeles != null)
            {
                // 3. Beállítjuk az új státuszt
                rendeles.StatusCode = ujKod;
                rendeles.StatusName = ujNev;

                try
                {
                    // 4. API hívás: elküldjük a frissítést a webshopnak
                    var valasz = _api.OrdersUpdate(rendeles);

                    if (valasz.Errors == null || valasz.Errors.Count == 0)
                    {
                        MessageBox.Show($"A rendelés állapota sikeresen frissítve erre: {ujNev}", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // 5. Frissítjük a UI-t (Táblázat és a Label)
                        kivalasztottSor.Státusz = ujNev;
                        dataGridView1.Refresh();

                        // Ha van egy label8, ami a státuszt mutatja, azt is átírjuk, hogy azonnal látszódjon
                        label8.Text = ujNev;

                        // TIPP: Itt esetleg meghívhatsz egy készlet-visszaíró kódot, 
                        // ha a státusz "Cancelled"-re változott (amit az előző kérdésedben tárgyaltunk).
                    }
                    else
                    {
                        MessageBox.Show($"Hiba a frissítés során: {valasz.Errors[0].Description}", "API Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Váratlan hiba történt a kommunikáció során: {ex.Message}", "Hálózati Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void buttonTeljesitve_Click(object sender, EventArgs e)
        {
            ValtoztassRendelesStatust("09D7305D-BD95-48d2-A025-16ADC827582A", "Complete");
        }

        private void buttonLemondas_Click(object sender, EventArgs e)
        {
            ValtoztassRendelesStatust("A7FFDB90-C566-4cf2-93F4-D42367F359D5", "Cancelled");
        }

        private void buttonTorles_Click(object sender, EventArgs e)
        {
            // 1. Ellenőrzés
    if (dataGridView1.CurrentRow == null || dataGridView1.CurrentRow.DataBoundItem == null)
    {
        MessageBox.Show("Kérlek, válassz ki egy rendelést a listából a törléshez!", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    var kivalasztottSor = (RendelesViewModel)dataGridView1.CurrentRow.DataBoundItem;
    var rendeles = kivalasztottSor.EredetiRendeles;

    if (rendeles != null)
    {
        // 2. Megerősítés kérése (biztonsági lépés)
        var megerosites = MessageBox.Show(
            $"Biztosan véglegesen törölni szeretnéd a(z) {rendeles.OrderNumber} számú rendelést?\nEz a művelet nem vonható vissza!", 
            "Törlés megerősítése", 
            MessageBoxButtons.YesNo, 
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2); // A "Nem" az alapértelmezett gomb a biztonság kedvéért

        if (megerosites == DialogResult.Yes)
        {
            try
            {
                // 3. API hívás: rendelés törlése a Bvin (belső azonosító) alapján
                var valasz = _api.OrdersDelete(rendeles.Bvin);

                if (valasz.Errors == null || valasz.Errors.Count == 0)
                {
                    MessageBox.Show("A rendelés sikeresen törölve a rendszerből!", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // 4. Eltávolítjuk a sort a helyi listából is, hogy eltűnjön a táblázatból
                    var jelenlegiAdatok = (List<RendelesViewModel>)dataGridView1.DataSource;
                    if (jelenlegiAdatok != null)
                    {
                        jelenlegiAdatok.Remove(kivalasztottSor);
                        
                        // Kis WinForms trükk a DataGridView frissítésére lista esetén:
                        dataGridView1.DataSource = null;
                        dataGridView1.DataSource = jelenlegiAdatok;
                    }

                    // Kijelölés megszűnt, szóval lenullázzuk a labeleket
                    label1.Text = "-";
                    label3.Text = "-";
                    label4.Text = "-";
                    label5.Text = "-";
                    label6.Text = "-";
                    label8.Text = "-";
                }
                else
                {
                    MessageBox.Show($"Hiba a törlés során: {valasz.Errors[0].Description}", "API Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Váratlan hiba történt: {ex.Message}", "Hálózati Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
        }
    }
}

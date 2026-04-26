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

        private void btnHullok_Click(object sender, EventArgs e)
        {
            KategoriaSzures("Hüllők");
        }

        private void btnHalak_Click(object sender, EventArgs e)
        {
            KategoriaSzures("Halak");
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
                // Az await miatt a felület itt NEM fagy le, hanem türelmesen megvárja, amíg a Task.Run végez a lekérdezésekkel.
                var megjelenitendoAdatok = await Task.Run(() =>
                {
                    var eredmenyLista = new List<dynamic>();

                    // 1. Megkeressük a kategóriát
                    var kategoria = _mindenKategoria.FirstOrDefault(c => c.Name.Equals(kategoriaNev, StringComparison.OrdinalIgnoreCase));
                    if (kategoria == null) return null; // A null jelzi majd a főszálnak, hogy nincs ilyen kategória

                    // 2. Termékek lekérése a kategória azonosítója alapján
                    var prodResponse = _api.ProductsFindForCategory(kategoria.Bvin, 1, 1000);

                    if (prodResponse.Errors != null && prodResponse.Errors.Count > 0)
                    {
                        throw new Exception(prodResponse.Errors[0].Description); // Ha hiba van, eldobjuk, a catch elkapja
                    }

                    var termekLista = prodResponse.Content.Products;
                    if (termekLista == null || termekLista.Count == 0)
                    {
                        return eredmenyLista; // Üres lista visszaadása, ha nincsenek termékek
                    }

                    // Kimentjük az összes releváns termék azonosítóját (Bvin)
                    var kategoriaTermekBvinLista = termekLista.Select(p => p.Bvin).ToList();

                    // 3. Összes rendelés lekérése
                    var rendelesekValasz = _api.OrdersFindAll();
                    var szurtRendelesek = new List<OrderDTO>();

                    if (rendelesekValasz.Content != null)
                    {
                        // --- PÁRHUZAMOS (GYORS) LETÖLTÉS ---
                        // A lassú foreach helyett 15 szálon bombázzuk a szervert, így a letöltési idő a töredékére csökken!
                        Parallel.ForEach(rendelesekValasz.Content, new ParallelOptions { MaxDegreeOfParallelism = 15 }, rendelesSnapshot =>
                        {
                            try
                            {
                                // JAVÍTÁS: Nagy B betű a Bvin-ben!
                                var rendelesKereses = _api.OrdersFind(rendelesSnapshot.bvin);

                                if (rendelesKereses.Content != null)
                                {
                                    var teljesRendeles = rendelesKereses.Content;

                                    if (teljesRendeles.Items != null && teljesRendeles.Items.Any(t => kategoriaTermekBvinLista.Contains(t.ProductId)))
                                    {
                                        // Szálbiztos (lock) hozzáadás, nehogy a 15 szál összeakadjon írás közben
                                        lock (szurtRendelesek)
                                        {
                                            szurtRendelesek.Add(teljesRendeles);
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                // Ha egy rendelés lekérdezése közben pillanatnyi hálózati hiba van, átugorja és megy tovább
                            }
                        });
                    }

                    // 4. Formázás és rendezés
                    // Az OrderByDescending azért kell, mert a párhuzamos letöltés összekeverheti a sorrendet. Ezzel a legújabb rendelés lesz legfelül.
                    eredmenyLista = szurtRendelesek
                        .OrderByDescending(r => r.TimeOfOrderUtc)
                        .Select(r => new
                        {
                            Rendelésszám = r.OrderNumber,
                            Dátum = r.TimeOfOrderUtc.ToLocalTime().ToString("yyyy. MM. dd. HH:mm"),
                            Vevő_Neve = r.BillingAddress.FirstName + " " + r.BillingAddress.LastName,
                            Vevő_Email = r.UserEmail,
                            Végösszeg = r.TotalGrand.ToString("C"),
                            Státusz = r.StatusCode
                        }).ToList<dynamic>();

                    return eredmenyLista;
                });

                // --- VISSZATÉRTÜNK A FŐSZÁLRA ---
                if (megjelenitendoAdatok == null)
                {
                    MessageBox.Show($"Nem található '{kategoriaNev}' nevű kategória a rendszerben.");
                    return;
                }

                // Betöltjük a kész adatokat a táblázatba
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
                // Bármi történik, az egeret visszaállítjuk normál állapotba a töltés (homokóra) után
                Cursor.Current = Cursors.Default;
            }
        }
    }
}

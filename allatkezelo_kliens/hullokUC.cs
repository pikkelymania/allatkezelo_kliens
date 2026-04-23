using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hotcakes.CommerceDTO.v1.Client;

namespace allatkezelo_kliens
{
    public partial class hullokUC : UserControl
    {
        private Hotcakes.CommerceDTO.v1.Client.Api _api;
        private List<Hotcakes.CommerceDTO.v1.Catalog.CategorySnapshotDTO> _mindenKategoria;

        private const string ApiKey = "1-45782d8b-85b9-4924-aafe-ea09050cbc9e";
        private const string StoreUrl = "http://www.pikkelymania.hu/";
        public hullokUC()
        {
            InitializeComponent();
        }

        private void hullokUC_Load(object sender, EventArgs e)
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

            // Megkeressük az alkategóriát a név alapján a letöltött listából
            var kategoria = _mindenKategoria.FirstOrDefault(c => c.Name.Equals(alkategoriaNev, StringComparison.OrdinalIgnoreCase));

            if (kategoria != null)
            {
                // Termékek lekérése a kategória azonosítója alapján
                var prodResponse = _api.ProductsFindForCategory(kategoria.Bvin, 1, 100);

                if (prodResponse.Errors == null || prodResponse.Errors.Count == 0)
                {
                    // A terméklista megjelenítése a táblázatban
                    dataGridView1.DataSource = prodResponse.Content.Products;
                }
            }
            else
            {
                MessageBox.Show($"Nem található '{alkategoriaNev}' nevű kategória.");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            KategoriaSzures("Királypitonok");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            KategoriaSzures("Szakállas agámák");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            KategoriaSzures("Leopárdgekkók");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            KategoriaSzures("Vitorlásgekkók");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            KategoriaSzures("Gabonasiklók");
        }
    }
}

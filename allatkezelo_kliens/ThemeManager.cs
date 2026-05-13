using System;
using System.Drawing;
using System.Windows.Forms;

namespace allatkezelo_kliens
{
    public static class ThemeManager
    {
        // --- MODERN PREMIUM LIGHT THEME (PikkelyMánia Zöld kiemeléssel) ---
        public static Color FeherHatter = Color.White;
        public static Color HalvanyHatter = Color.FromArgb(246, 249, 248);     // Picit mentás/zöldes törtfehér
        public static Color SzovegSzin = Color.FromArgb(30, 41, 59);           // Sötétszürke a kontraszt miatt
        public static Color HalvanySzoveg = Color.FromArgb(100, 116, 139);

        // A prezentációd alapján kikevert elegáns sötétzöld szín
        public static Color KiemeloZold = Color.FromArgb(55, 95, 82);
        public static Color VonalSzin = Color.FromArgb(226, 232, 240);

        public static void ApplyTheme(Control szuloVezerlo)
        {
            szuloVezerlo.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular);
            szuloVezerlo.BackColor = FeherHatter;
            szuloVezerlo.ForeColor = SzovegSzin;

            VegigiteralControls(szuloVezerlo);
        }

        private static void VegigiteralControls(Control szulo)
        {
            foreach (Control vezerlo in szulo.Controls)
            {
                StilusBeallitasa(vezerlo);
                if (vezerlo.HasChildren) VegigiteralControls(vezerlo);
            }
        }
        private static void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            TabPage tabPage = tabControl.TabPages[e.Index];
            Rectangle tabBounds = tabControl.GetTabRect(e.Index);

            // Megnézzük, hogy épp ez a fül van-e kiválasztva
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            // 1. Háttér kifestése
            // Ha ki van választva, akkor PikkelyMánia Zöld, ha nincs, akkor halvány szürke/fehér
            using (SolidBrush bgBrush = new SolidBrush(isSelected ? KiemeloZold : HalvanyHatter))
            {
                e.Graphics.FillRectangle(bgBrush, tabBounds);
            }

            // 2. Szöveg kifestése
            // Ha ki van választva, akkor fehér betűk, ha nincs, akkor a sötétszürke alapbetű
            using (SolidBrush textBrush = new SolidBrush(isSelected ? Color.White : SzovegSzin))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                // A betűtípust egy picit vastagítjuk a kiválasztott fülnél
                Font betutipus = isSelected ? new Font(tabControl.Font, FontStyle.Bold) : tabControl.Font;

                e.Graphics.DrawString(tabPage.Text, betutipus, textBrush, tabBounds, sf);
            }
        }

        private static void StilusBeallitasa(Control vezerlo)
        {
            if (vezerlo is Panel pnl)
            {
                if (pnl.Dock == DockStyle.Left) pnl.BackColor = HalvanyHatter;
            }
            else if (vezerlo is Label lbl)
            {
                lbl.ForeColor = SzovegSzin;
                lbl.BackColor = Color.Transparent;
            }
            else if (vezerlo is TextBox txt)
            {
                txt.BackColor = FeherHatter;
                txt.ForeColor = SzovegSzin;
                txt.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (vezerlo is Button gomb)
            {
                gomb.UseVisualStyleBackColor = false;
                gomb.FlatStyle = FlatStyle.Flat;
                gomb.FlatAppearance.BorderSize = 0;
                gomb.BackColor = HalvanyHatter;
                gomb.ForeColor = SzovegSzin;
                gomb.Cursor = Cursors.Hand;
                gomb.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            }
            else if (vezerlo is DataGridView dgv)
            {
                // --- 1. SZERKESZTÉS LETILTÁSA ---
                dgv.ReadOnly = true;
                dgv.AllowUserToAddRows = false;
                dgv.AllowUserToDeleteRows = false;
                dgv.AllowUserToResizeRows = false;

                dgv.BackgroundColor = FeherHatter;
                dgv.BorderStyle = BorderStyle.None;
                dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                dgv.GridColor = VonalSzin;

                // --- 2. KISEBB SOROK ÉS ZÖLD KIJELÖLÉS ---
                dgv.DefaultCellStyle.BackColor = FeherHatter;
                dgv.DefaultCellStyle.ForeColor = SzovegSzin;
                dgv.DefaultCellStyle.SelectionBackColor = KiemeloZold;
                dgv.DefaultCellStyle.SelectionForeColor = Color.White;

                // Kisebb margók és alacsonyabb sorok (50 helyett 35)
                dgv.DefaultCellStyle.Padding = new Padding(10, 3, 10, 3);
                dgv.RowTemplate.Height = 35;

                // --- 3. ZEBRA-CSÍKOZÁS (Váltakozó sorszínek) ---
                // Minden második sor egy nagyon halvány zöldes-szürke árnyalatot kap
                dgv.AlternatingRowsDefaultCellStyle.BackColor = HalvanyHatter;

                // --- 4. VILÁGOS, JÓL OLVASHATÓ PASZTELL FEJLÉC ---
                dgv.EnableHeadersVisualStyles = false;
                dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

                // Halvány mentazöld/jégzöld háttér, határozott sötétszürke betűkkel
                dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(220, 235, 230);
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = SzovegSzin;

                // Hogy kattintáskor is maradjon ilyen (ne villanjon kékre)
                dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 230);
                dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = SzovegSzin;

                // Vastagított betűtípus
                dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                dgv.ColumnHeadersHeight = 40;

                dgv.RowHeadersVisible = false;
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            }
            else if (vezerlo is TabControl tabControl)
            {
                // Megmondjuk, hogy mi rajzoljuk a füleket
                tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
                tabControl.Padding = new Point(20, 8); // Tágasabb, nagyobb fülek

                // Lecsatoljuk a régit (hogy ne fusson le kétszer), majd rákötjük a saját rajzoló metódusunkat
                tabControl.DrawItem -= TabControl_DrawItem;
                tabControl.DrawItem += TabControl_DrawItem;
            }
        }
    }
}
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
            TabControl tab = (TabControl)sender;
            Graphics g = e.Graphics;
            Rectangle bounds = tab.GetTabRect(e.Index);
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            // 1. KÉPMINŐSÉG JAVÍTÁSA (Éles, gyönyörű szövegek recésedés nélkül!)
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            // 2. HÁTTÉR TISZTÍTÁSA (Eltüntetjük a 3D kereteket egy picit nagyobb kitöltéssel)
            Rectangle fillRect = new Rectangle(bounds.X - 2, bounds.Y - 2, bounds.Width + 4, bounds.Height + 4);

            // A kiválasztott sötétzöld, a többi beolvad a fehér háttérbe
            using (SolidBrush bgBrush = new SolidBrush(isSelected ? KiemeloZold : FeherHatter))
            {
                g.FillRectangle(bgBrush, fillRect);
            }

            // 3. VIZUÁLIS ELVÁLASZTÓ (Az inaktív fülek kapnak egy finom alsó vonalat)
            if (!isSelected)
            {
                using (Pen borderPen = new Pen(VonalSzin, 1))
                {
                    g.DrawLine(borderPen, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
                }
            }

            // 4. SZÖVEG KIRAJZOLÁSA
            Color textColor = isSelected ? Color.White : HalvanySzoveg;
            Font textFont = isSelected ? new Font(tab.Font, FontStyle.Bold) : tab.Font;

            // A TextRenderer a modern WinForms szövegmegjelenítő, sokkal szebb a régi DrawString-nél
            TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;
            TextRenderer.DrawText(g, tab.TabPages[e.Index].Text, textFont, bounds, textColor, flags);
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

                // --- LÁTHATÓ GOMBOK ---
                gomb.FlatAppearance.BorderSize = 1;           // Visszaadjuk a keretet (0 helyett 1)
                gomb.FlatAppearance.BorderColor = VonalSzin;  // Finom szürke keretszín
                gomb.BackColor = FeherHatter;             // Hófehér háttér, hogy elüssön a panel színétől

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
                tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;

                // --- ÚJ RÉSZ: Fix, egyenletes méretű, modern fülek ---
                tabControl.SizeMode = TabSizeMode.Fixed;
                tabControl.ItemSize = new Size(160, 45); // Széles és magas fülek a modern kinézetért

                tabControl.DrawItem -= TabControl_DrawItem;
                tabControl.DrawItem += TabControl_DrawItem;
            }
            else if (vezerlo is NumericUpDown num)
            {
                num.BackColor = FeherHatter;
                num.ForeColor = SzovegSzin;
                num.BorderStyle = BorderStyle.FixedSingle;
            }
        }
    }
}
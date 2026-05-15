using System;
using System.Drawing;
using System.Windows.Forms;

namespace allatkezelo_kliens
{
    public static class ThemeManager
    {
        // --- MODERN PREMIUM LIGHT THEME (PikkelyMánia Zöld kiemeléssel) ---
        public static Color FeherHatter = Color.White;
        public static Color HalvanyHatter = Color.FromArgb(246, 249, 248);      // Picit mentás/zöldes törtfehér
        public static Color SzovegSzin = Color.FromArgb(30, 41, 59);            // Sötétszürke a kontraszt miatt
        public static Color HalvanySzoveg = Color.FromArgb(100, 116, 139);

        // A prezentációd alapján kikevert elegáns sötétzöld szín
        public static Color KiemeloZold = Color.FromArgb(55, 95, 82);
        public static Color VonalSzin = Color.FromArgb(226, 232, 240);

        // --- ÁLLAPOT ÉS KÉSZLET SZÍNEK (Halvány pasztell árnyalatok) ---
        public static Color HalvanyPiros = Color.FromArgb(255, 235, 235);
        public static Color PirosKijelolt = Color.FromArgb(255, 200, 200);

        public static Color HalvanySarga = Color.FromArgb(255, 250, 205);
        public static Color SargaKijelolt = Color.FromArgb(255, 240, 150);

        public static Color HalvanyZold = Color.FromArgb(220, 245, 220);
        public static Color ZoldKijelolt = Color.FromArgb(170, 230, 170);

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

            // 1. KÉPMINŐSÉG JAVÍTÁSA
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            // 2. HÁTTÉR TISZTÍTÁSA
            Rectangle fillRect = new Rectangle(bounds.X - 2, bounds.Y - 2, bounds.Width + 4, bounds.Height + 4);

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

            TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;
            TextRenderer.DrawText(g, tab.TabPages[e.Index].Text, textFont, bounds, textColor, flags);
        }
        private static void StilusBeallitasa(Control vezerlo)
        {
            if (vezerlo is GroupBox grp)
            {
                grp.ForeColor = KiemeloZold; 
                grp.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            }
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
                gomb.FlatAppearance.BorderSize = 1;            
                gomb.FlatAppearance.BorderColor = VonalSzin; 
                gomb.BackColor = FeherHatter;             

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

                dgv.CellFormatting -= Dgv_CellFormatting; 
                dgv.CellFormatting += Dgv_CellFormatting; 
            }
            else if (vezerlo is TabControl tabControl)
            {
                tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;

                tabControl.SizeMode = TabSizeMode.Fixed;
                tabControl.ItemSize = new Size(160, 45);

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
        private static void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var dgv = (DataGridView)sender;
            var sor = dgv.Rows[e.RowIndex];

            // --------------------------------------------------------------------------
            // 1. MEGOLDÁS: FOGLALÁSOK (RENDELÉSEK) SZÍNEZÉSE (Ha van "Státusz" oszlop)
            // --------------------------------------------------------------------------
            bool isRendelesTabla = false;
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (col.Name == "Státusz" || col.HeaderText == "Státusz")
                {
                    isRendelesTabla = true;
                    var statuszErtek = sor.Cells[col.Index].Value?.ToString();

                    if (statuszErtek == "Received")
                    {
                        e.CellStyle.BackColor = HalvanySarga;
                        e.CellStyle.SelectionBackColor = SargaKijelolt;
                        e.CellStyle.SelectionForeColor = Color.DarkGoldenrod;
                    }
                    else if (statuszErtek == "Cancelled")
                    {
                        e.CellStyle.BackColor = HalvanyPiros;
                        e.CellStyle.SelectionBackColor = PirosKijelolt;
                        e.CellStyle.SelectionForeColor = Color.Maroon;
                    }
                    else if (statuszErtek == "Complete")
                    {
                        e.CellStyle.BackColor = HalvanyZold;
                        e.CellStyle.SelectionBackColor = ZoldKijelolt;
                        e.CellStyle.SelectionForeColor = Color.DarkGreen;
                    }
                    break; 
                }
            }

            if (isRendelesTabla) return;


            // --------------------------------------------------------------------------
            // 2. MEGOLDÁS: KÉSZLET SZÍNEZÉSE (Hüllők / Halak esetén)
            // --------------------------------------------------------------------------
            int? elerhetoKeszlet = null;
            int? foglaltKeszlet = null;

            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (col.HeaderText.Contains("Készlet") || col.HeaderText.Contains("Raktáron") || col.HeaderText.Contains("db"))
                {
                    var cellaErtek = sor.Cells[col.Index].Value;
                    if (cellaErtek != null && int.TryParse(cellaErtek.ToString(), out int db))
                    {
                        elerhetoKeszlet = db;
                    }
                }

                if (col.HeaderText.Contains("Foglalva"))
                {
                    var cellaErtek = sor.Cells[col.Index].Value;
                    if (cellaErtek != null && int.TryParse(cellaErtek.ToString(), out int foglalt))
                    {
                        foglaltKeszlet = foglalt;
                    }
                }
            }

            // Színek beállítása a készlet értékek alapján
            if (elerhetoKeszlet.HasValue && elerhetoKeszlet.Value == 0)
            {
                if (foglaltKeszlet.HasValue && foglaltKeszlet.Value > 0)
                {
                    e.CellStyle.BackColor = HalvanySarga;
                    e.CellStyle.SelectionBackColor = SargaKijelolt;
                    e.CellStyle.SelectionForeColor = Color.DarkGoldenrod;
                }
                else
                {
                    e.CellStyle.BackColor = HalvanyPiros;
                    e.CellStyle.SelectionBackColor = PirosKijelolt;
                    e.CellStyle.SelectionForeColor = Color.Maroon;
                }
            }
        }
        // ==============================================================================================
        // --- ÚJ BLOKK: KIFEJEZETTEN A FELUGRÓ ABLAKOK (UjHalTermek, UjHulloTermek) FORMÁZÁSÁRA ---
        // ==============================================================================================

        public static void ApplyPopupTheme(Form popupAblak)
        {
            // Az űrlap alap beállításai
            popupAblak.BackColor = FeherHatter;
            popupAblak.ForeColor = SzovegSzin;
            popupAblak.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            // A Form összes elemének formázása rekurzívan
            PopupStilusBeallitasa(popupAblak);
        }

        private static void PopupStilusBeallitasa(Control szulo)
        {
            foreach (Control vezerlo in szulo.Controls)
            {
                if (vezerlo is GroupBox grp)
                {
                    grp.ForeColor = KiemeloZold; // Elegáns zöld keret és cím
                    grp.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                }
                else if (vezerlo is TextBox txt)
                {
                    txt.BackColor = HalvanyHatter; // Finom eltérés a fehér Form háttértől
                    txt.ForeColor = SzovegSzin;
                    txt.BorderStyle = BorderStyle.FixedSingle;
                    txt.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
                }
                else if (vezerlo is ComboBox cmb)
                {
                    cmb.BackColor = HalvanyHatter;
                    cmb.ForeColor = SzovegSzin;
                    cmb.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
                }
                else if (vezerlo is NumericUpDown num)
                {
                    num.BackColor = HalvanyHatter;
                    num.ForeColor = SzovegSzin;
                    num.BorderStyle = BorderStyle.FixedSingle;
                    num.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
                }
                else if (vezerlo is DateTimePicker dtp)
                {
                    dtp.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
                }
                else if (vezerlo is CheckBox chk)
                {
                    chk.ForeColor = SzovegSzin;
                    chk.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
                }
                else if (vezerlo is Label lbl)
                {
                    lbl.ForeColor = SzovegSzin;
                    lbl.BackColor = Color.Transparent;
                    lbl.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
                }
                else if (vezerlo is Button gomb)
                {
                    gomb.FlatStyle = FlatStyle.Flat;
                    gomb.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                    gomb.Cursor = Cursors.Hand;

                    // Szemantikus gombok egyedi színezése a feliratuk alapján!
                    if (gomb.Text.Contains("Mentés"))
                    {
                        gomb.BackColor = KiemeloZold;
                        gomb.ForeColor = Color.White;
                        gomb.FlatAppearance.BorderSize = 0; // Sima kitöltés (prémium kinézet)
                    }
                    else if (gomb.Text.Contains("Mégse"))
                    {
                        gomb.BackColor = HalvanyPiros;
                        gomb.ForeColor = Color.Maroon;
                        gomb.FlatAppearance.BorderSize = 0; // Sima kitöltés
                    }
                    else // pl. "Kép tallózása" gomb
                    {
                        gomb.BackColor = FeherHatter;
                        gomb.ForeColor = SzovegSzin;
                        gomb.FlatAppearance.BorderColor = VonalSzin;
                        gomb.FlatAppearance.BorderSize = 1;
                    }
                }

                // Rekurzió: Ha a vezérlőnek (pl. GroupBoxnak) vannak beágyazott elemei, menjen beljebb
                if (vezerlo.HasChildren)
                {
                    PopupStilusBeallitasa(vezerlo);
                }
            }
        }

        // ==============================================================================================
        // ==============================================================================================
    }
}
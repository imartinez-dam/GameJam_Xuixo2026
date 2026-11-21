# Genera el kit d'interficie (UI) pixel-art del Xuixo 2026.
# Us: powershell -ExecutionPolicy Bypass -File tools/genera_art_ui.ps1
# Sortida: Assets/Art/UI/*.png + preview_ui.png

Add-Type -AssemblyName System.Drawing

$codi = @'
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.IO;

public static class ArtUI
{
    static readonly Color Contorn   = Color.FromArgb(26, 16, 10);
    static readonly Color Fosc      = Color.FromArgb(43, 27, 18);
    static readonly Color Fusta     = Color.FromArgb(139, 90, 52);
    static readonly Color FustaClara= Color.FromArgb(192, 138, 82);
    static readonly Color FustaFosca= Color.FromArgb(94, 58, 34);
    static readonly Color Crema     = Color.FromArgb(242, 227, 198);
    static readonly Color CremaFosca= Color.FromArgb(214, 191, 150);
    static readonly Color Or        = Color.FromArgb(232, 177, 76);
    static readonly Color OrClar    = Color.FromArgb(247, 217, 138);
    static readonly Color Vermell   = Color.FromArgb(181, 84, 60);
    static readonly Color Blau      = Color.FromArgb(79, 124, 140);
    static readonly Color Metall    = Color.FromArgb(154, 163, 168);
    static readonly Color Transparent = Color.FromArgb(0, 0, 0, 0);

    static string _carpeta;

    public static void Genera(string carpeta)
    {
        _carpeta = carpeta;
        Directory.CreateDirectory(carpeta);

        Logo();
        Panell();
        Buttons();
        Barra();
        Icons();
        Dots();
        Contacte();
    }

    // ------------------------------------------------------------- utilitats

    static Bitmap Nou(int w, int h)
    {
        var b = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(b))
        {
            g.SmoothingMode = SmoothingMode.None;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.Clear(Transparent);
        }
        return b;
    }

    static void Desa(Bitmap b, string nom)
    {
        b.Save(Path.Combine(_carpeta, nom + ".png"), ImageFormat.Png);
        b.Dispose();
    }

    static GraphicsPath RR(float x, float y, float w, float h, float r)
    {
        var p = new GraphicsPath();
        r = Math.Min(r, Math.Min(w, h) / 2f);
        p.AddArc(x, y, r * 2, r * 2, 180, 90);
        p.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
        p.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
        p.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
        p.CloseFigure();
        return p;
    }

    static void Omple(Graphics g, Color c, float x, float y, float w, float h, float r)
    {
        using (var p = RR(x, y, w, h, r))
        using (var b = new SolidBrush(c))
            g.FillPath(b, p);
    }

    static void Contorna(Graphics g, Color c, float x, float y, float w, float h, float r, float gruix)
    {
        using (var p = RR(x, y, w, h, r))
        using (var pen = new Pen(c, gruix))
        {
            pen.Alignment = PenAlignment.Inset;
            g.DrawPath(pen, p);
        }
    }

    static void PanellBase(Graphics g, int w, int h, Color farciment, Color vora, float r)
    {
        Omple(g, Contorn, 0, 0, w, h, r);
        Omple(g, vora, 2, 2, w - 4, h - 4, r - 1);
        Omple(g, farciment, 4, 4, w - 8, h - 8, r - 2);
        Contorna(g, Contorn, 4.5f, 4.5f, w - 9, h - 9, r - 2, 1.5f);
    }

    // ------------------------------------------------------------------ logo

    static readonly Dictionary<char, string[]> Lletres = new Dictionary<char, string[]>
    {
        { 'X', new[] { "10001", "10001", "01010", "00100", "01010", "10001", "10001" } },
        { 'U', new[] { "10001", "10001", "10001", "10001", "10001", "10001", "01110" } },
        { 'I', new[] { "11111", "00100", "00100", "00100", "00100", "00100", "11111" } },
        { 'O', new[] { "01110", "10001", "10001", "10001", "10001", "10001", "01110" } },
        { '2', new[] { "01110", "10001", "00001", "00010", "00100", "01000", "11111" } },
        { '0', new[] { "01110", "10001", "10001", "10001", "10001", "10001", "01110" } },
        { '6', new[] { "00110", "01000", "10000", "11110", "10001", "10001", "01110" } },
        { 'A', new[] { "01110", "10001", "10001", "11111", "10001", "10001", "10001" } },
        { 'D', new[] { "11110", "10001", "10001", "10001", "10001", "10001", "11110" } }
    };

    static void Escriu(Graphics g, string text, float x, float y, int escala, Color color, int separacio)
    {
        using (var b = new SolidBrush(color))
        {
            float cx = x;
            foreach (char ch in text)
            {
                if (ch == ' ') { cx += 3 * escala; continue; }
                string[] gl = Lletres[ch];
                for (int fy = 0; fy < 7; fy++)
                    for (int fx = 0; fx < 5; fx++)
                        if (gl[fy][fx] == '1')
                            g.FillRectangle(b, cx + fx * escala, y + fy * escala, escala, escala);
                cx += 5 * escala + separacio;
            }
        }
    }

    static float AmpladaText(string text, int escala, int separacio)
    {
        float w = 0;
        foreach (char ch in text) w += (ch == ' ' ? 3 * escala : 5 * escala) + separacio;
        return w - separacio;
    }

    static void Logo()
    {
        const int escala = 20, sep = 10, ombra = 10;
        const string titol = "XUIXO";
        const string sub = "2026";

        float wt = AmpladaText(titol, escala, sep);
        int escalaSub = 9;
        float ws = AmpladaText(sub, escalaSub, 14);
        int w = (int)Math.Ceiling(wt + 120);
        int h = 420;

        var bmp = Nou(w, h);
        using (var g = Graphics.FromImage(bmp))
        {
            float x = (w - wt) / 2f, y = 40;

            // ombra
            Escriu(g, titol, x + ombra, y + ombra, escala, Color.FromArgb(120, 20, 10, 6), sep);
            // contorn
            for (int dx = -3; dx <= 3; dx++)
                for (int dy = -3; dy <= 3; dy++)
                    if (dx != 0 || dy != 0)
                        Escriu(g, titol, x + dx, y + dy, escala, Contorn, sep);
            // farciment
            Escriu(g, titol, x, y, escala, Crema, sep);

            // filet daurat
            using (var pen = new Pen(Contorn, 8))
                g.DrawLine(pen, x, y + 7 * escala + 40, x + wt, y + 7 * escala + 40);
            using (var pen = new Pen(Or, 4))
                g.DrawLine(pen, x, y + 7 * escala + 40, x + wt, y + 7 * escala + 40);

            // subtitol
            float xs = (w - ws) / 2f;
            for (int dx = -2; dx <= 2; dx++)
                for (int dy = -2; dy <= 2; dy++)
                    if (dx != 0 || dy != 0)
                        Escriu(g, sub, xs + dx, y + 7 * escala + 70 + dy, escalaSub, Contorn, 14);
            Escriu(g, sub, xs, y + 7 * escala + 70, escalaSub, Or, 14);
        }
        Desa(bmp, "logo_xuixo");
    }

    // --------------------------------------------------------------- panells

    static void Panell()
    {
        var bmp = Nou(96, 96);
        using (var g = Graphics.FromImage(bmp))
        {
            PanellBase(g, 96, 96, Color.FromArgb(238, 220, 186), Fusta, 14);
            // cantonades
            using (var b = new SolidBrush(FustaFosca))
            {
                g.FillRectangle(b, 10, 10, 6, 6);
                g.FillRectangle(b, 80, 10, 6, 6);
                g.FillRectangle(b, 10, 80, 6, 6);
                g.FillRectangle(b, 80, 80, 6, 6);
            }
            using (var b = new SolidBrush(Color.FromArgb(70, 255, 255, 255)))
                g.FillRectangle(b, 12, 6, 72, 3);
        }
        Desa(bmp, "panell_9slice");
    }

    static void Boto(string nom, Color farciment, Color lluentor, bool enfonsat)
    {
        var bmp = Nou(320, 96);
        using (var g = Graphics.FromImage(bmp))
        {
            PanellBase(g, 320, 96, farciment, Fusta, 18);
            using (var b = new SolidBrush(lluentor))
            {
                if (enfonsat)
                    g.FillRectangle(b, 14, 72, 292, 8);
                else
                    g.FillRectangle(b, 14, 14, 292, 10);
            }
            using (var b = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
            {
                if (!enfonsat)
                    g.FillRectangle(b, 14, 80, 292, 6);
            }
        }
        Desa(bmp, nom);
    }

    static void Buttons()
    {
        Boto("boto_normal", FustaClara, Color.FromArgb(120, 255, 240, 210), false);
        Boto("boto_hover", Color.FromArgb(214, 160, 96), Color.FromArgb(140, 255, 245, 215), false);
        Boto("boto_premut", Color.FromArgb(120, 78, 46), Color.FromArgb(60, 0, 0, 0), true);
    }

    // ---------------------------------------------------------------- barres

    static void Barra()
    {
        var marc = Nou(560, 48);
        using (var g = Graphics.FromImage(marc))
        {
            PanellBase(g, 560, 48, Color.FromArgb(58, 38, 26), FustaFosca, 12);
            Omple(g, Color.FromArgb(34, 22, 15), 12, 12, 536, 24, 8);
            Contorna(g, Color.FromArgb(120, 232, 177, 76), 12.5f, 12.5f, 535, 23, 8, 1.5f);
        }
        Desa(marc, "barra_marc");

        var ple = Nou(536, 24);
        using (var g = Graphics.FromImage(ple))
        {
            Omple(g, Or, 0, 0, 536, 24, 8);
            using (var b = new SolidBrush(OrClar))
                g.FillRectangle(b, 6, 3, 524, 6);
            using (var b = new SolidBrush(Color.FromArgb(80, 120, 60, 20)))
                g.FillRectangle(b, 6, 17, 524, 4);
        }
        Desa(ple, "barra_farciment");
    }

    // --------------------------------------------------------------- icones

    static Bitmap Tecla()
    {
        var bmp = Nou(64, 64);
        using (var g = Graphics.FromImage(bmp))
        {
            PanellBase(g, 64, 64, Crema, Fusta, 12);
            using (var b = new SolidBrush(Color.FromArgb(90, 255, 255, 255)))
                g.FillRectangle(b, 10, 8, 44, 4);
        }
        return bmp;
    }

    static void Fletxa(string nom, float angle)
    {
        var bmp = Tecla();
        using (var g = Graphics.FromImage(bmp))
        {
            var estat = g.Save();
            g.TranslateTransform(32, 32);
            g.RotateTransform(angle);
            g.TranslateTransform(-32, -32);
            using (var b = new SolidBrush(Fosc))
            {
                // punta
                g.FillPolygon(b, new[]
                {
                    new PointF(32, 14), new PointF(48, 32), new PointF(40, 32),
                    new PointF(40, 48), new PointF(24, 48), new PointF(24, 32), new PointF(16, 32)
                });
            }
            g.Restore(estat);
        }
        Desa(bmp, nom);
    }

    static void Icons()
    {
        Fletxa("icona_fletxa_amunt", 0f);
        Fletxa("icona_fletxa_dreta", 90f);
        Fletxa("icona_fletxa_avall", 180f);
        Fletxa("icona_fletxa_esquerra", 270f);

        foreach (var t in new[] { "A", "D" })
        {
            var bmp = Tecla();
            using (var g = Graphics.FromImage(bmp))
            {
                float escala = 6f;
                float w = AmpladaText(t, (int)escala, 4);
                Escriu(g, t, 32 - w / 2f, 32 - 7 * escala / 2f, (int)escala, Fosc, 4);
            }
            Desa(bmp, "icona_tecla_" + t.ToLower());
        }

        var stick = Tecla();
        using (var g = Graphics.FromImage(stick))
        {
            using (var b = new SolidBrush(Fosc))
            {
                g.FillEllipse(b, 14, 14, 36, 36);
                g.FillEllipse(new SolidBrush(Crema), 24, 24, 16, 16);
            }
        }
        Desa(stick, "icona_stick");
    }

    static void Dots()
    {
        foreach (var actiu in new[] { true, false })
        {
            var bmp = Nou(20, 20);
            using (var g = Graphics.FromImage(bmp))
            {
                if (actiu)
                {
                    using (var b = new SolidBrush(Contorn)) g.FillEllipse(b, 1, 1, 18, 18);
                    using (var b = new SolidBrush(Or)) g.FillEllipse(b, 3, 3, 14, 14);
                    using (var b = new SolidBrush(OrClar)) g.FillEllipse(b, 6, 5, 6, 5);
                }
                else
                {
                    using (var b = new SolidBrush(Contorn)) g.FillEllipse(b, 1, 1, 18, 18);
                    using (var b = new SolidBrush(Color.FromArgb(94, 58, 34))) g.FillEllipse(b, 3, 3, 14, 14);
                }
            }
            Desa(bmp, actiu ? "indicador_actiu" : "indicador_inactiu");
        }
    }

    // -------------------------------------------------------- full de contacte

    static void Contacte()
    {
        var bmp = new Bitmap(1280, 780, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.FromArgb(38, 29, 23));
            using (var b = new SolidBrush(Color.FromArgb(14, 255, 255, 255)))
                for (int y = 0; y < 780; y += 40)
                for (int x = 0; x < 1280; x += 40)
                    if (((x / 40) + (y / 40)) % 2 == 0) g.FillRectangle(b, x, y, 40, 40);

            using (var f = new Font("Consolas", 11))
            using (var fb = new SolidBrush(Crema))
            {
                Image logo = Image.FromFile(Path.Combine(_carpeta, "logo_xuixo.png"));
                g.DrawImage(logo, 40, 20, 400, 151);
                g.DrawString("logo_xuixo", f, fb, 40, 175);
                logo.Dispose();

                Image p = Image.FromFile(Path.Combine(_carpeta, "panell_9slice.png"));
                g.DrawImage(p, 500, 30, 144, 144);
                g.DrawString("panell_9slice (96x96)", f, fb, 470, 180);
                p.Dispose();

                string[] icones = { "icona_fletxa_esquerra", "icona_fletxa_dreta", "icona_fletxa_amunt", "icona_fletxa_avall", "icona_tecla_a", "icona_tecla_d", "icona_stick" };
                for (int i = 0; i < icones.Length; i++)
                {
                    Image ic = Image.FromFile(Path.Combine(_carpeta, icones[i] + ".png"));
                    g.DrawImage(ic, 700 + i * 78, 60, 64, 64);
                    g.DrawString(icones[i].Replace("icona_", "").Replace("fletxa_", ""), f, fb, 706 + i * 78, 130);
                    ic.Dispose();
                }

                Image act = Image.FromFile(Path.Combine(_carpeta, "indicador_actiu.png"));
                Image ina = Image.FromFile(Path.Combine(_carpeta, "indicador_inactiu.png"));
                g.DrawImage(act, 700, 160, 20, 20);
                g.DrawImage(ina, 730, 160, 20, 20);
                g.DrawString("indicadors", f, fb, 762, 160);
                act.Dispose(); ina.Dispose();

                int y2 = 240;
                string[] botons = { "boto_normal", "boto_hover", "boto_premut" };
                for (int i = 0; i < botons.Length; i++)
                {
                    Image bt = Image.FromFile(Path.Combine(_carpeta, botons[i] + ".png"));
                    g.DrawImage(bt, 40 + i * 360, y2, 320, 96);
                    g.DrawString(botons[i], f, fb, 44 + i * 360, y2 + 100);
                    bt.Dispose();
                }

                Image marc = Image.FromFile(Path.Combine(_carpeta, "barra_marc.png"));
                Image farc = Image.FromFile(Path.Combine(_carpeta, "barra_farciment.png"));
                g.DrawImage(marc, 40, 400, 560, 48);
                g.DrawImage(farc, 52, 412, 400, 24);
                g.DrawString("barra_marc + barra_farciment", f, fb, 44, 452);
                marc.Dispose(); farc.Dispose();
            }
        }
        bmp.Save(Path.Combine(_carpeta, "..", "..", "..", "docs", "preview_ui.png"), ImageFormat.Png);
        bmp.Dispose();
    }
}
'@

Add-Type -TypeDefinition $codi -ReferencedAssemblies System.Drawing

$desti = Join-Path $PSScriptRoot "..\Assets\Art\UI"
$desti = [System.IO.Path]::GetFullPath($desti)
[ArtUI]::Genera($desti)
"Generat a: $desti"
Get-ChildItem $desti -Filter *.png | Select-Object Name,Length | Format-Table -AutoSize

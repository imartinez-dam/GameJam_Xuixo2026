# Genera objectes i efectes de mon a la mateixa densitat que els fons:
# 160 px per unitat de joc (= 4 px per pixel de disseny, disseny a 40 px/unitat).
# Us: powershell -ExecutionPolicy Bypass -File tools/genera_art_mon.ps1

Add-Type -AssemblyName System.Drawing

$codi = @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.IO;

public class Lienzo
{
    public int W, H;
    public Color[] P;

    public Lienzo(int w, int h)
    {
        W = w; H = h;
        P = new Color[w * h];
        for (int i = 0; i < P.Length; i++) P[i] = Color.Transparent;
    }

    public void Punt(int x, int y, Color c)
    {
        if (x < 0 || y < 0 || x >= W || y >= H) return;
        if (c.A == 0) return;
        P[y * W + x] = c;
    }

    public void Rect(int x, int y, int w, int h, Color c)
    {
        for (int j = y; j < y + h; j++)
            for (int i = x; i < x + w; i++) Punt(i, j, c);
    }

    public void Disc(int cx, int cy, int r, Color c)
    {
        for (int y = -r; y <= r; y++)
            for (int x = -r; x <= r; x++)
                if (x * x + y * y <= r * r) Punt(cx + x, cy + y, c);
    }

    public void RectRodo(int x0, int y0, int w, int h, int r, Color c)
    {
        r = Math.Max(1, Math.Min(r, Math.Min(w, h) / 2));
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int cx = x < r ? r : (x > w - 1 - r ? w - 1 - r : x);
                int cy = y < r ? r : (y > h - 1 - r ? h - 1 - r : y);
                int dx = x - cx, dy = y - cy;
                if (dx * dx + dy * dy <= r * r) Punt(x0 + x, y0 + y, c);
            }
        }
    }

    public void Linia(int x0, int y0, int x1, int y1, Color c)
    {
        int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;
        while (true)
        {
            Punt(x0, y0, c);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    public void Elipse(int cx, int cy, int rx, int ry, Color c)
    {
        for (int y = -ry; y <= ry; y++)
            for (int x = -rx; x <= rx; x++)
                if ((x * x) / (double)(rx * rx) + (y * y) / (double)(ry * ry) <= 1.0)
                    Punt(cx + x, cy + y, c);
    }

    public Bitmap Escala(int factor)
    {
        var b = new Bitmap(W * factor, H * factor, PixelFormat.Format32bppArgb);
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                Color c = P[y * W + x];
                if (c.A == 0) continue;
                for (int j = 0; j < factor; j++)
                    for (int i = 0; i < factor; i++)
                        b.SetPixel(x * factor + i, y * factor + j, c);
            }
        return b;
    }
}

public static class ArtMon
{
    const int ESCALA = 4;   // 40 px/unitat de disseny x4 = 160 px/unitat

    static readonly Color Contorn   = Color.FromArgb(26, 16, 10);
    static readonly Color Fusta     = Color.FromArgb(139, 90, 52);
    static readonly Color FustaClara= Color.FromArgb(192, 138, 82);
    static readonly Color FustaFosca= Color.FromArgb(94, 58, 34);
    static readonly Color Crema     = Color.FromArgb(242, 227, 198);
    static readonly Color CremaFosca= Color.FromArgb(206, 184, 146);
    static readonly Color Or        = Color.FromArgb(232, 177, 76);
    static readonly Color OrClar    = Color.FromArgb(247, 217, 138);
    static readonly Color Vermell   = Color.FromArgb(181, 84, 60);
    static readonly Color Blau      = Color.FromArgb(79, 124, 140);
    static readonly Color Verd      = Color.FromArgb(110, 127, 69);
    static readonly Color Metall    = Color.FromArgb(154, 163, 168);
    static readonly Color MetallFosc= Color.FromArgb(96, 104, 108);
    static readonly Color Blanc     = Color.FromArgb(250, 246, 236);

    static string _bowl, _shared, _archive, _preview;

    public static void Genera(string bowl, string shared, string archive, string preview)
    {
        _bowl = bowl; _shared = shared; _archive = archive; _preview = preview;
        Directory.CreateDirectory(Path.GetDirectoryName(preview));

        Desa(Corro(), "corro");
        Desa(BolDarrere(), "bol_darrere");
        Desa(BolVora(), "bol_vora");
        Desa(SacFarina(), "sac_farina");
        Desa(Pot(), "pot");
        Desa(Cullera(), "cullera");
        Desa(Cistella(), "cistella");
        Desa(Tapa(), "tapa");
        Desa(Safata(), "safata");
        Desa(Pinces(), "pinces");
        Desa(Sortidor(), "sortidor");
        Desa(Plat(), "plat");

        Desa(TaulaGran(), "taula_gran");
        Desa(BolGranDarrere(), "bol_gran_darrere");
        Desa(BolGranVora(), "bol_gran_vora");
        Desa(CorroGran(), "corro_gran");

        Full("farina", Farina());
        Full("sucre", Sucre());
        Full("oli", Oli());
        Full("esquitx", Esquitx());
        Full("crema", CremaRajoli());
        Full("estrelles", Estrelles());
        Full("vapor", Vapor());

        Contacte();
    }

    static string PropPath(string nom)
    {
        string root = nom == "corro_gran" ? _shared
            : nom == "taula_gran" || nom == "bol_gran_darrere" || nom == "bol_gran_vora" ? _bowl : _archive;
        return Path.Combine(root, "Props", nom + ".png");
    }

    static string EffectPath(string nom)
    {
        return Path.Combine(nom == "farina" || nom == "estrelles" ? _bowl : _archive, "Effects", nom + ".png");
    }

    static Color Mescla(Color a, Color b, float t)
    {
        return Color.FromArgb(
            Math.Max(0, Math.Min(255, (int)(a.R + (b.R - a.R) * t))),
            Math.Max(0, Math.Min(255, (int)(a.G + (b.G - a.G) * t))),
            Math.Max(0, Math.Min(255, (int)(a.B + (b.B - a.B) * t))));
    }

    static void Desa(Lienzo l, string nom)
    {
        string path = PropPath(nom);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using (var b = l.Escala(ESCALA))
            b.Save(path, ImageFormat.Png);
    }

    static void Full(string nom, List<Lienzo> frames)
    {
        int w = frames[0].W, h = frames[0].H;
        var l = new Lienzo(w * frames.Count, h);
        for (int f = 0; f < frames.Count; f++)
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    Color c = frames[f].P[y * w + x];
                    if (c.A > 0) l.Punt(f * w + x, y, c);
                }
        string path = EffectPath(nom);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using (var b = l.Escala(ESCALA))
            b.Save(path, ImageFormat.Png);
    }

    // ------------------------------------------------------------- objectes

    static Lienzo Corro()
    {
        // 0,90 x 0,30 unitats  ->  36 x 12 px de disseny
        var l = new Lienzo(36, 12);
        l.RectRodo(5, 2, 26, 8, 4, Contorn);
        l.RectRodo(6, 3, 24, 6, 3, Fusta);
        l.Rect(9, 3, 18, 2, FustaClara);
        l.Rect(8, 3, 1, 6, FustaFosca);
        l.Rect(27, 3, 1, 6, FustaFosca);
        l.Rect(2, 4, 4, 4, Contorn);
        l.Rect(1, 5, 3, 2, FustaClara);
        l.Rect(30, 4, 4, 4, Contorn);
        l.Rect(32, 5, 3, 2, FustaClara);
        return l;
    }

    static Lienzo BolDarrere()
    {
        // 0,75 x 0,40 unitats -> 30 x 16 px
        var l = new Lienzo(30, 16);
        l.Elipse(15, 2, 14, 13, Contorn);
        l.Elipse(15, 2, 12, 11, Crema);
        l.Elipse(15, 6, 9, 7, CremaFosca);
        l.Rect(2, 3, 26, 3, Crema);
        return l;
    }

    static Lienzo BolVora()
    {
        // 0,75 x 0,12 -> 30 x 5 px
        var l = new Lienzo(30, 5);
        l.RectRodo(0, 0, 30, 5, 2, Contorn);
        l.RectRodo(1, 1, 28, 3, 1, Crema);
        l.Rect(2, 1, 26, 1, Blanc);
        return l;
    }

    static Lienzo SacFarina()
    {
        // 0,60 x 0,85 -> 24 x 34
        var l = new Lienzo(24, 34);
        l.RectRodo(3, 6, 18, 26, 5, Contorn);
        l.RectRodo(4, 7, 16, 24, 4, Crema);
        l.Rect(6, 9, 12, 3, CremaFosca);
        l.Rect(7, 16, 10, 7, FustaClara);
        l.Rect(8, 18, 8, 1, FustaFosca);
        l.Rect(8, 20, 5, 1, FustaFosca);
        // coll lligat
        l.RectRodo(8, 1, 8, 6, 2, Contorn);
        l.RectRodo(9, 2, 6, 4, 1, Crema);
        l.Rect(7, 6, 10, 2, FustaFosca);
        return l;
    }

    static Lienzo Pot()
    {
        // 0,35 x 0,55 -> 14 x 22
        var l = new Lienzo(14, 22);
        l.RectRodo(2, 4, 10, 17, 2, Contorn);
        l.RectRodo(3, 5, 8, 15, 1, Color.FromArgb(220, 228, 226));
        l.RectRodo(4, 9, 6, 10, 1, Or);
        l.Rect(4, 9, 6, 2, OrClar);
        l.RectRodo(2, 1, 10, 4, 1, Contorn);
        l.RectRodo(3, 2, 8, 2, 1, Vermell);
        return l;
    }

    static Lienzo Cullera()
    {
        // 1,20 x 0,20 -> 48 x 8
        var l = new Lienzo(48, 8);
        l.Elipse(8, 4, 6, 3, Contorn);
        l.Elipse(8, 4, 5, 2, FustaClara);
        l.Rect(12, 3, 32, 2, Contorn);
        l.Rect(12, 3, 32, 1, FustaClara);
        return l;
    }

    static Lienzo Cistella()
    {
        // 0,90 x 0,40 -> 36 x 16
        var l = new Lienzo(36, 16);
        l.RectRodo(3, 4, 30, 11, 3, Contorn);
        l.RectRodo(4, 5, 28, 9, 2, Fusta);
        l.Rect(5, 6, 26, 1, FustaClara);
        l.Rect(6, 9, 24, 1, FustaFosca);
        l.Rect(8, 11, 20, 1, FustaFosca);
        // nanses
        l.Linia(8, 1, 15, 4, Contorn);
        l.Linia(27, 1, 20, 4, Contorn);
        return l;
    }

    static Lienzo Tapa()
    {
        // 0,60 x 0,15 -> 24 x 6
        var l = new Lienzo(24, 6);
        l.Elipse(12, 3, 11, 2, Contorn);
        l.Elipse(12, 3, 10, 1, Metall);
        l.Rect(10, 0, 4, 2, Contorn);
        l.Rect(11, 0, 2, 1, Metall);
        return l;
    }

    static Lienzo Safata()
    {
        // 1,20 x 0,25 -> 48 x 10
        var l = new Lienzo(48, 10);
        l.RectRodo(1, 2, 46, 7, 2, Contorn);
        l.RectRodo(2, 3, 44, 5, 1, MetallFosc);
        l.Rect(3, 3, 42, 1, Metall);
        l.Rect(3, 6, 42, 1, Color.FromArgb(60, 0, 0, 0));
        return l;
    }

    static Lienzo Pinces()
    {
        // 0,50 x 0,20 -> 20 x 8
        var l = new Lienzo(20, 8);
        l.Linia(1, 1, 17, 6, Contorn);
        l.Linia(2, 1, 18, 6, Metall);
        l.Linia(1, 6, 17, 1, Contorn);
        l.Linia(2, 6, 18, 1, MetallFosc);
        l.Disc(9, 4, 2, Contorn);
        l.Disc(9, 4, 1, Or);
        return l;
    }

    static Lienzo Sortidor()
    {
        // 0,55 x 0,75 -> 22 x 30
        var l = new Lienzo(22, 30);
        // tova
        l.RectRodo(4, 2, 14, 14, 3, Contorn);
        l.RectRodo(5, 3, 12, 12, 2, Metall);
        l.Rect(6, 3, 10, 2, Color.FromArgb(210, 220, 225));
        // embut
        for (int y = 0; y < 8; y++)
        {
            int ample = 12 - y;
            l.Rect(11 - ample / 2, 16 + y, ample, 1, Contorn);
            if (ample > 2) l.Rect(12 - ample / 2, 16 + y, ample - 2, 1, MetallFosc);
        }
        // broquet
        l.Rect(9, 24, 4, 5, Contorn);
        l.Rect(10, 24, 2, 5, Or);
        // nansa
        l.Rect(17, 6, 4, 2, Contorn);
        l.Rect(18, 6, 3, 1, Or);
        return l;
    }

    static Lienzo Plat()
    {
        // 0,80 x 0,15 -> 32 x 6
        var l = new Lienzo(32, 6);
        l.Elipse(16, 3, 15, 2, Contorn);
        l.Elipse(16, 3, 14, 1, Blanc);
        l.Rect(4, 1, 24, 1, Crema);
        return l;
    }

    static Lienzo TaulaGran()
    {
        var l = new Lienzo(800, 60);
        for (int y = 0; y < 60; y++)
        {
            for (int x = 0; x < 800; x++)
            {
                Color c;
                if (y < 9) c = Mescla(FustaClara, Color.FromArgb(224, 170, 104), y / 9f);
                else c = Mescla(Fusta, FustaFosca, (y - 9) / 51f);
                float veta = (float)(Math.Sin(x * Math.PI * 2 / 400 * 3) * 0.5 +
                           Math.Sin(x * Math.PI * 2 / 400 * 9 + 1.3) * 0.3 +
                           Math.Sin(x * Math.PI * 2 / 400 * 21 + 4.2) * 0.2);
                c = Color.FromArgb((int)Math.Min(255, c.R * (1 + veta * 0.06)), (int)Math.Min(255, c.G * (1 + veta * 0.06)), (int)Math.Min(255, c.B * (1 + veta * 0.06)));
                if (x % 200 < 2) c = Color.FromArgb((int)(c.R * 0.86), (int)(c.G * 0.86), (int)(c.B * 0.86));
                if (y < 2) c = Color.FromArgb(232, 190, 132);
                l.Punt(x, y, c);
            }
        }
        return l;
    }

    static Lienzo BolGranDarrere()
    {
        var l = new Lienzo(64, 44);
        for (int y = 20; y < 44; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float nx = (x - 32) / 26f;
                float ny = (y - 20) / 19f;
                float d = nx * nx + ny * ny;
                if (d > 1f) continue;
                float prof = (y - 20) / 19f;
                Color c = Mescla(Crema, CremaFosca, prof * 0.4f);
                if (d > 0.78f) c = Mescla(c, FustaFosca, 0.22f);
                l.Punt(x, y, c);
            }
        }
        return l;
    }

    static Lienzo BolGranVora()
    {
        var l = new Lienzo(64, 44);
        for (int y = 20; y < 44; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float nx = (x - 32) / 26f;
                float ny = (y - 20) / 19f;
                float din = nx * nx + ny * ny;
                float nxo = (x - 32) / 30f;
                float nyo = (y - 20) / 23f;
                float dout = nxo * nxo + nyo * nyo;

                bool capEsq = (x - 6) * (x - 6) + (y - 20) * (y - 20) <= 9;
                bool capDre = (x - 58) * (x - 58) + (y - 20) * (y - 20) <= 9;
                bool paret = dout <= 1f && din >= 1f;
                if (!paret && !capEsq && !capDre) continue;
                float llum = Math.Max(0f, Math.Min(1f, (44 - y) / 24f));
                Color c = capEsq || capDre ? FustaClara : Mescla(Fusta, FustaFosca, llum * 0.7f);
                l.Punt(x, y, c);
            }
        }
        return l;
    }

    static Lienzo CorroGran()
    {
        var l = new Lienzo(56, 18);
        l.RectRodo(1, 6, 7, 5, 2, Contorn);
        l.RectRodo(2, 7, 5, 3, 1, FustaFosca);
        l.RectRodo(48, 6, 7, 5, 2, Contorn);
        l.RectRodo(49, 7, 5, 3, 1, FustaFosca);
        l.RectRodo(7, 2, 42, 13, 5, Contorn);
        l.RectRodo(8, 3, 40, 11, 4, Fusta);
        l.Rect(10, 3, 36, 3, FustaClara);
        l.Rect(10, 3, 36, 1, CremaFosca);
        l.Rect(12, 3, 1, 11, FustaFosca);
        l.Rect(43, 3, 1, 11, FustaFosca);
        return l;
    }

    // ------------------------------------------------------------------ vfx

    static Color Amb(Color c, float a)
    {
        return Color.FromArgb((int)(255 * a), c.R, c.G, c.B);
    }

    static List<Lienzo> Farina()
    {
        var llista = new List<Lienzo>();
        for (int f = 0; f < 8; f++)
        {
            var l = new Lienzo(20, 20);
            float t = f / 7f;
            for (int i = 0; i < 7; i++)
            {
                double ang = i * Math.PI * 2 / 7 + i;
                int dist = (int)(2 + t * 8);
                int x = 10 + (int)(Math.Cos(ang) * dist);
                int y = 10 + (int)(Math.Sin(ang) * dist * 0.7);
                l.Disc(x, y, 1, Amb(Crema, 1f - t * 0.8f));
            }
            llista.Add(l);
        }
        return llista;
    }

    static List<Lienzo> Sucre()
    {
        var llista = new List<Lienzo>();
        for (int f = 0; f < 6; f++)
        {
            var l = new Lienzo(18, 18);
            float t = f / 5f;
            for (int i = 0; i < 6; i++)
            {
                int x = 2 + (i * 7 + f * 3) % 16;
                int y = 16 - (int)(t * (4 + i * 2));
                l.Punt(x, y, Amb(OrClar, 1f - t * 0.6f));
                l.Punt(x + 1, y, Amb(Or, 1f - t * 0.6f));
            }
            llista.Add(l);
        }
        return llista;
    }

    static List<Lienzo> Oli()
    {
        var llista = new List<Lienzo>();
        for (int f = 0; f < 8; f++)
        {
            var l = new Lienzo(24, 24);
            float t = f / 7f;
            for (int i = 0; i < 5; i++)
            {
                int x = 4 + i * 4;
                int y = 20 - (int)(t * 14) - i * 2;
                int r = 1 + (i % 2);
                l.Disc(x, y, r, Amb(OrClar, 0.85f - t * 0.5f));
                l.Punt(x - 1, y - 1, Amb(Blanc, 0.7f - t * 0.4f));
            }
            llista.Add(l);
        }
        return llista;
    }

    static List<Lienzo> Esquitx()
    {
        var llista = new List<Lienzo>();
        for (int f = 0; f < 4; f++)
        {
            var l = new Lienzo(18, 18);
            float t = f / 3f;
            for (int i = 0; i < 8; i++)
            {
                double ang = i * Math.PI / 4;
                int dist = (int)(2 + t * 7);
                int x = 9 + (int)(Math.Cos(ang) * dist);
                int y = 9 + (int)(Math.Sin(ang) * dist * 0.5);
                l.Disc(x, y, 1, Amb(Or, 1f - t * 0.7f));
            }
            llista.Add(l);
        }
        return llista;
    }

    static List<Lienzo> CremaRajoli()
    {
        var llista = new List<Lienzo>();
        for (int f = 0; f < 4; f++)
        {
            var l = new Lienzo(10, 26);
            float t = f / 3f;
            int llarg = (int)(6 + t * 18);
            for (int y = 0; y < llarg; y++)
            {
                l.Rect(4, y, 2, 1, Or);
                if (y % 3 == 0) l.Punt(3, y, OrClar);
            }
            l.Disc(5, llarg, 2, OrClar);
            llista.Add(l);
        }
        return llista;
    }

    static List<Lienzo> Estrelles()
    {
        var llista = new List<Lienzo>();
        for (int f = 0; f < 6; f++)
        {
            var l = new Lienzo(22, 22);
            float t = f / 5f;
            int radi = (int)(2 + t * 7);
            for (int i = 0; i < 4; i++)
            {
                double ang = i * Math.PI / 2 + Math.PI / 4;
                int x = 11 + (int)(Math.Cos(ang) * radi);
                int y = 11 + (int)(Math.Sin(ang) * radi);
                l.Disc(x, y, 1, Amb(OrClar, 1f - t * 0.8f));
            }
            l.Punt(11, 11, Amb(Blanc, 1f - t * 0.8f));
            llista.Add(l);
        }
        return llista;
    }

    static List<Lienzo> Vapor()
    {
        var llista = new List<Lienzo>();
        for (int f = 0; f < 8; f++)
        {
            var l = new Lienzo(24, 24);
            float t = f / 7f;
            for (int i = 0; i < 3; i++)
            {
                int x = 8 + i * 4;
                int y = 20 - (int)(t * 12) - i * 2;
                int r = 2 + (int)(t * 2);
                for (int dy = -r; dy <= r; dy++)
                    for (int dx = -r; dx <= r; dx++)
                        if (dx * dx + dy * dy <= r * r && ((dx + dy + i) & 1) == 0)
                            l.Punt(x + dx, y + dy, Color.FromArgb((int)(70 * (1 - t)), 240, 240, 235));
            }
            llista.Add(l);
        }
        return llista;
    }

    // ------------------------------------------------- normalitzar la massa

    /// <summary>
    /// Reescala els sprites de la massa a la densitat del fons (160 px/unitat,
    /// blocs de 4 px) amb l'amplada de mon indicada. Desa els originals.
    /// </summary>
    public static void Normalitza(string origen, string desti, string[] noms, float[] unitats)
    {
        Directory.CreateDirectory(desti);
        for (int i = 0; i < noms.Length; i++)
        {
            string ruta = Path.Combine(origen, noms[i] + ".png");
            if (!File.Exists(ruta)) continue;

            using (var src = new Bitmap(ruta))
            {
                int minX = src.Width, minY = src.Height, maxX = -1, maxY = -1;
                for (int y = 0; y < src.Height; y += 2)
                    for (int x = 0; x < src.Width; x += 2)
                        if (src.GetPixel(x, y).A > 20)
                        {
                            if (x < minX) minX = x;
                            if (x > maxX) maxX = x;
                            if (y < minY) minY = y;
                            if (y > maxY) maxY = y;
                        }
                if (maxX < 0) continue;

                int cw = maxX - minX + 1, ch = maxY - minY + 1;
                int tw = Math.Max(4, (int)Math.Round(unitats[i] * 40f));
                int th = Math.Max(4, (int)Math.Round(tw * ch / (float)cw));

                var small = new Color[tw * th];
                for (int ty = 0; ty < th; ty++)
                    for (int tx = 0; tx < tw; tx++)
                    {
                        int x0 = minX + tx * cw / tw, x1 = Math.Max(x0 + 1, minX + (tx + 1) * cw / tw);
                        int y0 = minY + ty * ch / th, y1 = Math.Max(y0 + 1, minY + (ty + 1) * ch / th);
                        double r = 0, g = 0, b = 0, a = 0;
                        int n = 0;
                        for (int y = y0; y < y1; y++)
                            for (int x = x0; x < x1; x++)
                            {
                                Color c = src.GetPixel(x, y);
                                double al = c.A / 255.0;
                                r += c.R * al; g += c.G * al; b += c.B * al; a += al;
                                n++;
                            }
                        if (n == 0 || a <= 0.001) { small[ty * tw + tx] = Color.Transparent; continue; }
                        small[ty * tw + tx] = Color.FromArgb(
                            (int)Math.Min(255, 255 * a / n),
                            (int)Math.Min(255, r / a),
                            (int)Math.Min(255, g / a),
                            (int)Math.Min(255, b / a));
                    }

                var fin = new Bitmap(tw * 4, th * 4, PixelFormat.Format32bppArgb);
                for (int y = 0; y < th; y++)
                    for (int x = 0; x < tw; x++)
                    {
                        Color c = small[y * tw + x];
                        if (c.A == 0) continue;
                        for (int j = 0; j < 4; j++)
                            for (int k = 0; k < 4; k++)
                                fin.SetPixel(x * 4 + k, y * 4 + j, c);
                    }
                fin.Save(Path.Combine(desti, noms[i] + ".png"), ImageFormat.Png);
                fin.Dispose();
            }
        }
    }
    // ------------------------------------------------------- full de contacte

    static void Contacte()
    {
        string[] nomsEines = { "corro", "bol_darrere", "bol_vora", "sac_farina", "pot", "cullera", "cistella", "tapa", "safata", "pinces", "sortidor", "plat" };
        string[] nomsVfx = { "farina", "sucre", "oli", "esquitx", "crema", "estrelles", "vapor" };
        string[] unitats = { "0,90 x 0,30", "0,75 x 0,40", "0,75 x 0,12", "0,60 x 0,85", "0,35 x 0,55", "1,20 x 0,20", "0,90 x 0,40", "0,60 x 0,15", "1,20 x 0,25", "0,50 x 0,20", "0,55 x 0,75", "0,80 x 0,15" };

        var bmp = new Bitmap(1280, 900, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            g.Clear(Color.FromArgb(38, 29, 23));
            using (var b = new SolidBrush(Color.FromArgb(14, 255, 255, 255)))
                for (int y = 0; y < 900; y += 40)
                    for (int x = 0; x < 1280; x += 40)
                        if (((x / 40) + (y / 40)) % 2 == 0) g.FillRectangle(b, x, y, 40, 40);

            using (var f = new Font("Consolas", 10))
            using (var fb = new SolidBrush(Crema))
            {
                for (int i = 0; i < nomsEines.Length; i++)
                {
                     using (var im = Image.FromFile(PropPath(nomsEines[i])))
                    {
                        int x = 40 + (i % 4) * 300;
                        int y = 40 + (i / 4) * 190;
                        float esc = Math.Min(2f, Math.Min(240f / im.Width, 140f / im.Height));
                        int w = (int)(im.Width * esc), h = (int)(im.Height * esc);
                        g.DrawImage(im, x, y, w, h);
                        g.DrawString(nomsEines[i], f, fb, x, y + 145);
                        g.DrawString(unitats[i] + " u", f, fb, x + 110, y + 145);
                    }
                }

                for (int i = 0; i < nomsVfx.Length; i++)
                {
                     using (var im = Image.FromFile(EffectPath(nomsVfx[i])))
                    {
                        int x = 40 + (i % 4) * 300;
                        int y = 640 + (i / 4) * 120;
                        float esc = Math.Min(2f, Math.Min(260f / im.Width, 80f / im.Height));
                        g.DrawImage(im, x, y, (int)(im.Width * esc), (int)(im.Height * esc));
                        g.DrawString(nomsVfx[i], f, fb, x, y + 86);
                    }
                }
            }
        }
        bmp.Save(_preview, ImageFormat.Png);
        bmp.Dispose();
    }
}
'@

Add-Type -TypeDefinition $codi -ReferencedAssemblies System.Drawing

$arrel = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$content = Join-Path $arrel "Assets\Content"
$bowl = Join-Path $content "01_BowlEscape\Art\World"
$shared = Join-Path $content "Shared\Art\World"
$archive = Join-Path $content "Archive\Art"
$archiveWorld = Join-Path $archive "World"
$frying = Join-Path $content "05_FryPastry\Art\Sprites"
$preview = Join-Path $arrel "docs\preview_mon.png"

[ArtMon]::Genera($bowl, $shared, $archiveWorld, $preview)
$source = Join-Path $archive "Sprites"
$normalized = Join-Path $archiveWorld "Normalized"
[ArtMon]::Normalitza($source, $bowl, @("dough_pixel"), @(0.50))
[ArtMon]::Normalitza($source, $normalized,
    @("kneaded_dough_pixel","stretched_dough_pixel","stretched_dough_with_custard_pixel"), @(0.40, 1.00, 1.00))
[ArtMon]::Normalitza($frying, $normalized,
    @("rolled_dough_with_custard_pixel","fried_rolled_dough_pixel","heavily_sugared_fried_rolled_dough_pixel","finished_croissant_pixel"),
    @(1.20, 1.20, 1.20, 1.00))
"Generated world art and preview: $preview"

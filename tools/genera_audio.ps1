# Genera la musica i els efectes del Xuixo 2026 (WAV mono 22 kHz).
# Us: powershell -ExecutionPolicy Bypass -File tools/genera_audio.ps1

Add-Type -AssemblyName System.Drawing

$codi = @'
using System;
using System.IO;
using System.Collections.Generic;

public static class AudioGen
{
    const int SR = 22050;

    public static void Genera(string bowl, string shared, string archive)
    {
        Directory.CreateDirectory(bowl);
        Directory.CreateDirectory(shared);
        Directory.CreateDirectory(archive);
        Desa(bowl, "massa_remenar", Remenar());
        Desa(bowl, "massa_sortida", Sortida());
        Desa(bowl, "massa_rodar", Rodar());
        Desa(bowl, "massa_aterrar", Aterrar());
        Desa(bowl, "corro", Corro());
        Desa(archive, "farina", Farina());
        Desa(shared, "victoria", Victoria());
        Desa(bowl, "ui_clic", Clic());
        Desa(bowl, "musica_joc", Musica());
    }

    // ------------------------------------------------------------- utilitats

    static void Desa(string carpeta, string nom, float[] mostres)
    {
        string ruta = Path.Combine(carpeta, nom + ".wav");
        using (var fs = new FileStream(ruta, FileMode.Create))
        using (var w = new BinaryWriter(fs))
        {
            int n = mostres.Length;
            w.Write(new[] { 'R', 'I', 'F', 'F' });
            w.Write(36 + n * 2);
            w.Write(new[] { 'W', 'A', 'V', 'E' });
            w.Write(new[] { 'f', 'm', 't', ' ' });
            w.Write(16);
            w.Write((short)1);
            w.Write((short)1);
            w.Write(SR);
            w.Write(SR * 2);
            w.Write((short)2);
            w.Write((short)16);
            w.Write(new[] { 'd', 'a', 't', 'a' });
            w.Write(n * 2);
            for (int i = 0; i < n; i++)
            {
                short s = (short)(Math.Max(-1f, Math.Min(1f, mostres[i])) * 32000);
                w.Write(s);
            }
        }
    }

    static float[] Buit(double segons)
    {
        return new float[(int)(SR * segons)];
    }

    static double Freq(int semitons)
    {
        return 440.0 * Math.Pow(2.0, semitons / 12.0);
    }

    static void Nota(float[] buf, double inici, double duracio, double freq, double volum, int tipus, Random rnd)
    {
        int i0 = (int)(inici * SR);
        int n = (int)(duracio * SR);
        double fase = 0;
        for (int i = 0; i < n; i++)
        {
            int idx = i0 + i;
            if (idx < 0 || idx >= buf.Length) continue;
            double t = i / (double)SR;
            double env = Math.Min(1.0, t / 0.008) * Math.Exp(-t * 3.2 / Math.Max(0.05, duracio));
            double v;
            switch (tipus)
            {
                case 0: // quadrada
                    fase += freq / SR;
                    v = (fase % 1.0) < 0.5 ? 1.0 : -1.0;
                    break;
                case 1: // triangle
                    fase += freq / SR;
                    v = 2.0 * Math.Abs(2.0 * (fase % 1.0) - 1.0) - 1.0;
                    break;
                default: // soroll
                    v = rnd.NextDouble() * 2.0 - 1.0;
                    break;
            }
            buf[idx] += (float)(v * env * volum);
        }
    }

    static void Soroll(float[] buf, double inici, double duracio, double volum, double toGreu, Random rnd)
    {
        int i0 = (int)(inici * SR);
        int n = (int)(duracio * SR);
        double anterior = 0;
        for (int i = 0; i < n; i++)
        {
            int idx = i0 + i;
            if (idx < 0 || idx >= buf.Length) continue;
            double t = i / (double)SR;
            double env = Math.Exp(-t / Math.Max(0.01, duracio * 0.45));
            double soroll = rnd.NextDouble() * 2.0 - 1.0;
            anterior = anterior * toGreu + soroll * (1.0 - toGreu);
            buf[idx] += (float)(anterior * env * volum);
        }
    }

    static float[] Normalitza(float[] buf, float pic)
    {
        float max = 0.001f;
        for (int i = 0; i < buf.Length; i++) max = Math.Max(max, Math.Abs(buf[i]));
        float k = pic / max;
        for (int i = 0; i < buf.Length; i++) buf[i] *= k;
        return buf;
    }

    // ------------------------------------------------------------------ sfx

    static float[] Remenar()
    {
        var b = Buit(0.60);
        var rnd = new Random(1);
        for (int i = 0; i < b.Length; i++)
        {
            double t = i / (double)SR;
            double env = Math.Sin(t / 0.6 * Math.PI);
            double v = Math.Sin(2 * Math.PI * 95 * t) * 0.5 + Math.Sin(2 * Math.PI * 140 * t) * 0.25;
            v += (rnd.NextDouble() * 2 - 1) * 0.06;
            b[i] = (float)(v * env * 0.35);
        }
        return Normalitza(b, 0.6f);
    }

    static float[] Sortida()
    {
        var b = Buit(0.40);
        var rnd = new Random(2);
        for (int i = 0; i < b.Length; i++)
        {
            double t = i / (double)SR;
            double f = 280 + t / 0.4 * 900;
            double env = Math.Exp(-t * 6);
            double v = Math.Sin(2 * Math.PI * f * t) * 0.5 + (rnd.NextDouble() * 2 - 1) * 0.15;
            b[i] = (float)(v * env * 0.6);
        }
        return Normalitza(b, 0.8f);
    }

    static float[] Rodar()
    {
        var b = Buit(0.50);
        var rnd = new Random(3);
        for (int i = 0; i < b.Length; i++)
        {
            double t = i / (double)SR;
            double env = 0.6 + 0.4 * Math.Sin(t / 0.5 * Math.PI * 2);
            double v = (rnd.NextDouble() * 2 - 1) * 0.5 + Math.Sin(2 * Math.PI * 70 * t) * 0.4;
            b[i] = (float)(v * 0.22 * env);
        }
        return Normalitza(b, 0.45f);
    }

    static float[] Aterrar()
    {
        var b = Buit(0.22);
        var rnd = new Random(4);
        for (int i = 0; i < b.Length; i++)
        {
            double t = i / (double)SR;
            double env = Math.Exp(-t * 22);
            double v = Math.Sin(2 * Math.PI * 110 * t) * 0.7 + (rnd.NextDouble() * 2 - 1) * 0.25;
            b[i] = (float)(v * env * 0.7);
        }
        return Normalitza(b, 0.75f);
    }

    static float[] Corro()
    {
        var b = Buit(0.45);
        var rnd = new Random(5);
        for (int i = 0; i < b.Length; i++)
        {
            double t = i / (double)SR;
            double f = 420 - t / 0.45 * 300;
            double env = Math.Exp(-t * 7);
            double v = Math.Sin(2 * Math.PI * f * t) * 0.5 + (rnd.NextDouble() * 2 - 1) * 0.18;
            b[i] = (float)(v * env * 0.55);
        }
        return Normalitza(b, 0.7f);
    }

    static float[] Farina()
    {
        var b = Buit(0.35);
        Soroll(b, 0, 0.35, 0.5, 0.55, new Random(6));
        return Normalitza(b, 0.55f);
    }

    static float[] Victoria()
    {
        var b = Buit(1.30);
        var rnd = new Random(7);
        int[] notes = { 0, 4, 7, 12, 16 };
        double t = 0;
        foreach (int n in notes)
        {
            Nota(b, t, 0.22, Freq(n), 0.35, 0, rnd);
            Nota(b, t, 0.22, Freq(n - 12), 0.2, 1, rnd);
            t += 0.17;
        }
        Nota(b, t, 0.55, Freq(19), 0.35, 0, rnd);
        Nota(b, t, 0.55, Freq(7), 0.22, 1, rnd);
        return Normalitza(b, 0.8f);
    }

    static float[] Clic()
    {
        var b = Buit(0.06);
        var rnd = new Random(8);
        Nota(b, 0, 0.06, 900, 0.3, 0, rnd);
        return Normalitza(b, 0.5f);
    }

    // ---------------------------------------------------------------- musica

    static float[] Musica()
    {
        // 8 compassos a 100 bpm (19,2 s), progressio Am - F - C - G
        double bpm = 100, beat = 60.0 / bpm, bar = beat * 4;
        var b = Buit(bar * 8);
        var rnd = new Random(11);

        int[] arrels = { -12, -16, -21, -14 };          // A, F, C, G (greus)
        int[][] acords = {
            new[] { 0, 3, 7 }, new[] { 5, 9, 12 }, new[] { 7, 11, 14 }, new[] { 2, 6, 9 }
        };

        for (int compas = 0; compas < 8; compas++)
        {
            int c = compas % 4;
            double inici = compas * bar;

            // baix
            Nota(b, inici, beat * 1.6, Freq(arrels[c]), 0.26, 1, rnd);
            Nota(b, inici + beat * 2, beat * 1.6, Freq(arrels[c] + 12), 0.20, 1, rnd);

            // percussio
            for (int p = 0; p < 8; p++)
                Soroll(b, inici + p * beat * 0.5, 0.05, p % 2 == 0 ? 0.10 : 0.06, 0.2, rnd);

            // melodia (corxeres)
            for (int p = 0; p < 8; p++)
            {
                if (p == 3 && compas % 2 == 1) continue;
                int[] acord = acords[c];
                int grau = (p + compas) % 3;
                int octava = (p == 5 || p == 7) ? 12 : 0;
                int nota = acord[grau] + 12 + octava;
                Nota(b, inici + p * beat * 0.5, beat * 0.45, Freq(nota), 0.20, 0, rnd);
            }
        }

        // fosa d'entrada i sortida perque el bucle no peti
        int fade = (int)(0.02 * SR);
        for (int i = 0; i < fade; i++)
        {
            b[i] *= i / (float)fade;
            b[b.Length - 1 - i] *= i / (float)fade;
        }
        return Normalitza(b, 0.7f);
    }
}
'@

Add-Type -TypeDefinition $codi

$arrel = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$bowl = Join-Path $arrel "Assets\Content\01_BowlEscape\Audio"
$shared = Join-Path $arrel "Assets\Content\Shared\Audio"
$archive = Join-Path $arrel "Assets\Content\Archive\Audio"
[AudioGen]::Genera($bowl, $shared, $archive)
Get-ChildItem $bowl,$shared,$archive -Filter *.wav | Select-Object Name,Length | Format-Table -AutoSize

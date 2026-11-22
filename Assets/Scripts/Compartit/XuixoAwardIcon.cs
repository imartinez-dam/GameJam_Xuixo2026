using UnityEngine;
using UnityEngine.UI;

namespace Xuixo
{
    // Original pixel-art rosette for the game's fictional ceremony, not the contest logo.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class XuixoAwardIcon : MaskableGraphic
    {
        private static readonly string[] Pixels =
        {
            ".......######.......",
            ".....##yyyyyy##.....",
            "....#yyyYYYYyyy#....",
            "...#yyYYooooYYyy#...",
            "..#yyYooooooooYyy#..",
            "..#yYooooYYooooYy#..",
            ".#yYooooYYYYooooYy#.",
            ".#yYooYYYYYYYYooYy#.",
            ".#yYoooYYYYYYoooYy#.",
            ".#yYooooYYYYooooYy#.",
            ".#yYoooYYooYYoooYy#.",
            "..#yYooooooooooYy#..",
            "..#yyYooooooooYyy#..",
            "...#yyYYooooYYyy#...",
            "....#yyyYYYYyyy#....",
            "...##rr######rr##...",
            "...#Rrr#....#rrR#...",
            "..#RRrr#....#rrRR#..",
            "..#Rrr#......#rrR#..",
            ".#RRrr#......#rrRR#.",
            ".#R#rr#......#rr#R#.",
            ".##.##........##.##."
        };

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect rect = GetPixelAdjustedRect();
            int columns = Pixels[0].Length;
            float size = Mathf.Min(rect.width / columns, rect.height / Pixels.Length);
            Vector2 origin = rect.center - new Vector2(columns, Pixels.Length) * (size * 0.5f);

            for (int y = 0; y < Pixels.Length; y++)
            for (int x = 0; x < columns; x++)
            {
                char pixel = Pixels[y][x];
                if (pixel == '.') continue;
                Color tint = color * (Color)PixelColor(pixel);
                Vector2 p = origin + new Vector2(x, Pixels.Length - 1 - y) * size;
                int index = mesh.currentVertCount;
                mesh.AddVert(p, tint, Vector2.zero);
                mesh.AddVert(p + Vector2.up * size, tint, Vector2.zero);
                mesh.AddVert(p + Vector2.one * size, tint, Vector2.zero);
                mesh.AddVert(p + Vector2.right * size, tint, Vector2.zero);
                mesh.AddTriangle(index, index + 1, index + 2);
                mesh.AddTriangle(index, index + 2, index + 3);
            }
        }

        private static Color32 PixelColor(char pixel) => pixel switch
        {
            'Y' => new Color32(255, 230, 150, 255),
            'y' => new Color32(248, 181, 69, 255),
            'o' => new Color32(188, 109, 36, 255),
            'R' => new Color32(219, 77, 53, 255),
            'r' => new Color32(151, 46, 40, 255),
            _ => new Color32(66, 33, 25, 255)
        };
    }
}

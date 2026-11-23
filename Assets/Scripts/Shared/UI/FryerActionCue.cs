using UnityEngine;
using UnityEngine.UI;

namespace Xuixo.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class FryerActionCue : MaskableGraphic
    {
        public enum ActionIcon { Drop, Flip, Lift, Sugar, Rock }

        private static readonly string[][] Icons =
        {
            new[] { "###........", "...##......", ".....#.....", "......#....", ".......#...", ".....#####.", "......###..", ".......#...", "...........", ".##.##.##..", "..##.##.##." },
            new[] { "...####....", "..##..##...", ".##....##..", ".#......#..", "......#####", ".......###.", "........#..", "...........", "..#######..", ".#########.", "..#######.." },
            new[] { ".......#...", "......###..", ".....#####.", ".......#...", "..#.#..#...", "..#.#......", "..###......", ".##.##.....", "###.#####..", ".#########.", "..#######.." },
            new[] { "...####....", "..#.#.##...", "..######...", "..######...", "...#####...", "....###....", "...........", "..#..#..#..", "....#..#...", "..#######..", ".#########." },
            new[] { "..#.....#..", ".###...###.", "###.....###", "..#######..", "...........", "....###....", "...#####...", "#..#####..#", ".#########.", "..#######..", "...#####..." }
        };

        // Pixel glyphs spell “Espai” without a font dependency.
        private static readonly string[] Espai =
        {
            "111 000 000 000 010",
            "100 000 000 000 000",
            "100 011 110 110 010",
            "110 100 101 001 010",
            "100 010 110 111 010",
            "100 001 100 101 010",
            "111 110 100 111 010"
        };

        private static readonly Color32 Ink = new Color32(48, 25, 17, 255);
        private static readonly Color32 Gold = new Color32(255, 222, 148, 255);
        private static readonly Color32 Brass = new Color32(169, 104, 56, 255);
        private ActionIcon icon;
        private bool showKey = true;
        private float keyShownAt = -1f;
        private int rockDirection;

        public void Show(ActionIcon action, bool pressNow)
        {
            if (pressNow && (!showKey || action != icon || !enabled)) keyShownAt = Time.unscaledTime;
            icon = action;
            showKey = pressNow;
            enabled = true;
            SetVerticesDirty();
        }

        public void ShowRocking(int direction)
        {
            if (enabled && icon == ActionIcon.Rock && rockDirection == direction) return;
            icon = ActionIcon.Rock;
            rockDirection = direction;
            showKey = false;
            keyShownAt = -1f;
            enabled = true;
            SetVerticesDirty();
        }

        private void Update()
        {
            if (!showKey || keyShownAt < 0f) return;
            if (Time.unscaledTime - keyShownAt >= 0.14f) keyShownAt = -1f;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect bounds = GetPixelAdjustedRect();
            float unit = Mathf.Min(bounds.width / 44f, bounds.height / 16f);
            Vector2 center = bounds.center;
            Square(mesh, center, new Rect(-22, -8, 44, 16), unit, Ink);
            Square(mesh, center, new Rect(-21, -7, 42, 14), unit, Brass);
            Square(mesh, center, new Rect(-21, -6, 42, 12), unit, Ink);

            string[] pixels = Icons[(int)icon];
            for (int y = 0; y < pixels.Length; y++)
                for (int x = 0; x < pixels[y].Length; x++)
                    if (pixels[y][x] == '#') Square(mesh, center, new Rect(x - 19, 5 - y, 1, 1), unit, Gold);

            if (icon == ActionIcon.Rock)
            {
                DrawArrowKey(mesh, center, unit, -1, -1f);
                DrawArrowKey(mesh, center, unit, 1, 13f);
                return;
            }

            if (!showKey) return;

            float age = keyShownAt < 0f ? 1f : Mathf.Clamp01((Time.unscaledTime - keyShownAt) / 0.14f);
            float pop = 1f + 0.12f * Mathf.Sin(age * Mathf.PI);
            Vector2 keyCenter = center + Vector2.right * (7f * unit);
            float keyUnit = unit * pop;
            Square(mesh, keyCenter, new Rect(-14, -6, 28, 12), keyUnit, Ink);
            Square(mesh, keyCenter, new Rect(-13, -5, 26, 10), keyUnit, Brass);
            Square(mesh, keyCenter, new Rect(-13, -3, 26, 8), keyUnit, Gold);
            for (int y = 0; y < Espai.Length; y++)
                for (int x = 0; x < Espai[y].Length; x++)
                    if (Espai[y][x] == '1') Square(mesh, keyCenter, new Rect(x - 9.5f, 3.5f - y, 1, 1), keyUnit, Ink);
        }

        private void DrawArrowKey(VertexHelper mesh, Vector2 center, float unit, int direction, float offset)
        {
            bool pressed = rockDirection == direction;
            Vector2 keyCenter = center + new Vector2(offset, pressed ? -1f : 0f) * unit;
            Square(mesh, keyCenter, new Rect(-6, -6, 12, 12), unit, Ink);
            Square(mesh, keyCenter, new Rect(-5, -5, 10, 10), unit, Brass);
            Square(mesh, keyCenter, new Rect(-5, -3, 10, 8), unit,
                pressed ? new Color32(255, 246, 204, 255) : Gold);
            Square(mesh, keyCenter, new Rect(-3, 0, 6, 2), unit, Ink);
            for (int step = 0; step < 3; step++)
            {
                int x = direction < 0 ? -4 + step : 3 - step;
                Square(mesh, keyCenter, new Rect(x, -step, 1, 2 + step * 2), unit, Ink);
            }
        }

        private void Square(VertexHelper mesh, Vector2 center, Rect r, float unit, Color32 pixelColor)
        {
            Vector2 p = center + r.min * unit;
            Vector2 size = r.size * unit;
            Color tint = color * (Color)pixelColor;
            int index = mesh.currentVertCount;
            mesh.AddVert(p, tint, Vector2.zero);
            mesh.AddVert(p + Vector2.up * size.y, tint, Vector2.zero);
            mesh.AddVert(p + size, tint, Vector2.zero);
            mesh.AddVert(p + Vector2.right * size.x, tint, Vector2.zero);
            mesh.AddTriangle(index, index + 1, index + 2);
            mesh.AddTriangle(index, index + 2, index + 3);
        }
    }
}

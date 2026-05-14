using UnityEngine;

namespace Blockblast.Model
{
    public static class PieceShapes
    {
        private static Vector2Int V(int x, int y) => new Vector2Int(x, y);

        private static Color C(float r, float g, float b) => new Color(r, g, b, 1f);

        public static readonly Piece[] All = new[]
        {
            // 1x1
            new Piece(new[] { V(0, 0) }, C(1.00f, 0.27f, 0.27f)),

            // 2x1 horizontal
            new Piece(new[] { V(0, 0), V(1, 0) }, C(1.00f, 0.55f, 0.20f)),

            // 3x1 horizontal
            new Piece(new[] { V(0, 0), V(1, 0), V(2, 0) }, C(1.00f, 0.85f, 0.20f)),

            // 2x1 vertical
            new Piece(new[] { V(0, 0), V(0, 1) }, C(0.65f, 0.85f, 0.25f)),

            // 3x1 vertical
            new Piece(new[] { V(0, 0), V(0, 1), V(0, 2) }, C(0.30f, 0.78f, 0.45f)),

            // 2x2 square
            new Piece(new[] { V(0, 0), V(1, 0), V(0, 1), V(1, 1) }, C(0.20f, 0.72f, 0.70f)),

            // 3x3 square
            new Piece(new[]
            {
                V(0, 0), V(1, 0), V(2, 0),
                V(0, 1), V(1, 1), V(2, 1),
                V(0, 2), V(1, 2), V(2, 2),
            }, C(0.30f, 0.80f, 1.00f)),

            // L
            new Piece(new[] { V(0, 0), V(0, 1), V(0, 2), V(1, 0) }, C(0.30f, 0.55f, 1.00f)),

            // J
            new Piece(new[] { V(0, 0), V(1, 0), V(1, 1), V(1, 2) }, C(0.45f, 0.40f, 0.85f)),

            // T
            new Piece(new[] { V(0, 1), V(1, 0), V(1, 1), V(2, 1) }, C(0.65f, 0.40f, 0.85f)),

            // S
            new Piece(new[] { V(0, 0), V(1, 0), V(1, 1), V(2, 1) }, C(0.95f, 0.45f, 0.75f)),

            // Z
            new Piece(new[] { V(0, 1), V(1, 1), V(1, 0), V(2, 0) }, C(0.95f, 0.30f, 0.55f)),

            // I (4 in a row)
            new Piece(new[] { V(0, 0), V(1, 0), V(2, 0), V(3, 0) }, C(0.65f, 0.50f, 0.35f)),
        };

        public static Piece Random()
        {
            int index = UnityEngine.Random.Range(0, All.Length);
            return All[index];
        }
    }
}

using UnityEngine;

namespace Blockblast.Model
{
    public static class PieceShapes
    {
        // Below this level we bias the random pick toward easier shapes and
        // filter out pieces that don't fit anywhere on the current grid.
        // At and above it, the pick is uniform over All with no filtering.
        public const int WeightedLevelCutoff = 3;

        // "Easyness" score per shape, indexed parallel to All. Higher = more
        // likely to appear early. The values are lerp'd toward a uniform 1.0
        // as the player progresses through levels 0..2, so the bias fades.
        // Lines (I-shapes, including the 4×1) and the 2×2 square are easy;
        // S/Z staircases, 3×3 square, and asymmetric L/J/T are progressively
        // harder.
        private static readonly float[] EasyScore =
        {
            10f, // 1×1
            10f, // 2×1 H
            10f, // 3×1 H
            10f, // 2×1 V
            10f, // 3×1 V
            10f, // 2×2 square
             2f, // 3×3 square
             4f, // L
             4f, // J
             3f, // T
             2f, // S
             2f, // Z
            10f, // I (4 in a row)
        };

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

        static PieceShapes()
        {
            Debug.Assert(EasyScore.Length == All.Length,
                $"EasyScore ({EasyScore.Length}) and All ({All.Length}) are out of sync.");
        }

        public static Piece PickForContext(int level, Grid grid)
        {
            if (level >= WeightedLevelCutoff)
            {
                return All[UnityEngine.Random.Range(0, All.Length)];
            }

            // Levels 0..2: weighted random biased toward easy pieces, with
            // pieces that can't fit anywhere dropped. The bias fades linearly
            // from "full" at level 0 to "nearly uniform" at level 2.
            float biasT = (float)level / WeightedLevelCutoff;

            float totalWeight = 0f;
            for (int i = 0; i < All.Length; i++)
            {
                if (!grid.CanFitAnywhere(All[i])) continue;
                totalWeight += Mathf.Lerp(EasyScore[i], 1f, biasT);
            }

            // Nothing fits anywhere — game over is imminent, hand back any
            // piece so the existing CheckGameOver path triggers normally.
            if (totalWeight <= 0f)
            {
                return All[UnityEngine.Random.Range(0, All.Length)];
            }

            float roll = UnityEngine.Random.value * totalWeight;
            float cumulative = 0f;
            int lastEligible = -1;
            for (int i = 0; i < All.Length; i++)
            {
                if (!grid.CanFitAnywhere(All[i])) continue;
                cumulative += Mathf.Lerp(EasyScore[i], 1f, biasT);
                lastEligible = i;
                if (roll < cumulative) return All[i];
            }
            // Float precision can leave roll just at or past totalWeight; fall
            // back to the last eligible piece rather than off the end.
            return All[lastEligible];
        }
    }
}

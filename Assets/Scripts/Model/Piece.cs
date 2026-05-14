using UnityEngine;

namespace Blockblast.Model
{
    public readonly struct Piece
    {
        public readonly Vector2Int[] Cells;
        public readonly Color Color;

        public Piece(Vector2Int[] cells, Color color)
        {
            Cells = cells;
            Color = color;
        }
    }
}

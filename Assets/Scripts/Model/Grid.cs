using System.Collections.Generic;
using UnityEngine;

namespace Blockblast.Model
{
    public class Grid
    {
        public const int Size = 8;

        private readonly Color?[,] cells = new Color?[Size, Size];

        public Color? GetCell(int x, int y) => cells[x, y];

        public bool IsEmpty(int x, int y) => cells[x, y] == null;

        public bool CanPlace(Piece piece, Vector2Int origin)
        {
            foreach (Vector2Int offset in piece.Cells)
            {
                int x = origin.x + offset.x;
                int y = origin.y + offset.y;
                if (x < 0 || x >= Size || y < 0 || y >= Size) return false;
                if (cells[x, y] != null) return false;
            }
            return true;
        }

        public bool CanFitAnywhere(Piece piece)
        {
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    if (CanPlace(piece, new Vector2Int(x, y))) return true;
                }
            }
            return false;
        }

        public void Reset()
        {
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    cells[x, y] = null;
                }
            }
        }

        public void SetCell(int x, int y, Color? color)
        {
            cells[x, y] = color;
        }

        public void Place(Piece piece, Vector2Int origin)
        {
            foreach (Vector2Int offset in piece.Cells)
            {
                int x = origin.x + offset.x;
                int y = origin.y + offset.y;
                cells[x, y] = piece.Color;
            }
        }

        public List<int> GetFullRows()
        {
            var rows = new List<int>();
            for (int y = 0; y < Size; y++)
            {
                bool full = true;
                for (int x = 0; x < Size; x++)
                {
                    if (cells[x, y] == null) { full = false; break; }
                }
                if (full) rows.Add(y);
            }
            return rows;
        }

        public List<int> GetFullColumns()
        {
            var cols = new List<int>();
            for (int x = 0; x < Size; x++)
            {
                bool full = true;
                for (int y = 0; y < Size; y++)
                {
                    if (cells[x, y] == null) { full = false; break; }
                }
                if (full) cols.Add(x);
            }
            return cols;
        }

        public void ClearLines(IEnumerable<int> fullRows, IEnumerable<int> fullColumns)
        {
            foreach (int y in fullRows)
            {
                for (int x = 0; x < Size; x++) cells[x, y] = null;
            }
            foreach (int x in fullColumns)
            {
                for (int y = 0; y < Size; y++) cells[x, y] = null;
            }
        }
    }
}

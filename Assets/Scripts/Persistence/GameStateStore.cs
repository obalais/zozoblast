using System;
using System.Collections.Generic;
using Blockblast.Model;
using Blockblast.View;
using UnityEngine;
using Grid = Blockblast.Model.Grid;

namespace Blockblast.Persistence
{
    public static class GameStateStore
    {
        private const string PrefKey = "GameStateV1";

        // SavedCell/SavedPiece flatten Color? and Vector2Int[] into primitives because
        // JsonUtility doesn't serialize nullable structs or Vector2Int arrays. When
        // adding fields here, bump PrefKey to a new version so stale payloads are
        // rejected by TryLoad rather than silently mis-deserialized.
        [Serializable]
        public class SavedState
        {
            public List<SavedCell> cells = new List<SavedCell>();
            public List<SavedPiece> trayPieces = new List<SavedPiece>();
            public int score;
            public int level;
        }

        [Serializable]
        public class SavedCell
        {
            public bool filled;
            public float r;
            public float g;
            public float b;
        }

        [Serializable]
        public class SavedPiece
        {
            public bool present;
            public List<int> offsetsX = new List<int>();
            public List<int> offsetsY = new List<int>();
            public float r;
            public float g;
            public float b;
        }

        public static void Save(Grid grid, IReadOnlyList<Piece?> trayPieces, int score, int level)
        {
            var state = new SavedState { score = score, level = level };

            for (int x = 0; x < Grid.Size; x++)
            {
                for (int y = 0; y < Grid.Size; y++)
                {
                    Color? cellColor = grid.GetCell(x, y);
                    SavedCell saved = new SavedCell();
                    if (cellColor.HasValue)
                    {
                        saved.filled = true;
                        saved.r = cellColor.Value.r;
                        saved.g = cellColor.Value.g;
                        saved.b = cellColor.Value.b;
                    }
                    state.cells.Add(saved);
                }
            }

            foreach (Piece? maybePiece in trayPieces)
            {
                SavedPiece savedPiece = new SavedPiece();
                if (maybePiece.HasValue)
                {
                    savedPiece.present = true;
                    Piece piece = maybePiece.Value;
                    savedPiece.r = piece.Color.r;
                    savedPiece.g = piece.Color.g;
                    savedPiece.b = piece.Color.b;
                    foreach (Vector2Int offset in piece.Cells)
                    {
                        savedPiece.offsetsX.Add(offset.x);
                        savedPiece.offsetsY.Add(offset.y);
                    }
                }
                state.trayPieces.Add(savedPiece);
            }

            string json = JsonUtility.ToJson(state);
            PlayerPrefs.SetString(PrefKey, json);
            PlayerPrefs.Save();
        }

        public static bool TryLoad(out SavedState state)
        {
            state = null;
            string json = PlayerPrefs.GetString(PrefKey, null);
            if (string.IsNullOrEmpty(json)) return false;

            try
            {
                state = JsonUtility.FromJson<SavedState>(json);
            }
            catch
            {
                state = null;
                return false;
            }

            if (state == null || state.cells == null || state.cells.Count != Grid.Size * Grid.Size)
            {
                state = null;
                return false;
            }
            return true;
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(PrefKey);
            PlayerPrefs.Save();
        }

        public static Piece ToPiece(SavedPiece saved)
        {
            Vector2Int[] cells = new Vector2Int[saved.offsetsX.Count];
            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = new Vector2Int(saved.offsetsX[i], saved.offsetsY[i]);
            }
            return new Piece(cells, new Color(saved.r, saved.g, saved.b, 1f));
        }
    }
}

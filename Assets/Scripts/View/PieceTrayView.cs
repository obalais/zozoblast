using System;
using System.Collections;
using System.Collections.Generic;
using Blockblast.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Blockblast.View
{
    public class PieceTrayView : MonoBehaviour
    {
        private const int SlotCount = 3;
        private const float TrayHeight = 360f;
        private const float SlotSpacing = 24f;
        private const float RelayoutAnimDuration = 0.18f;

        private RectTransform trayRect;
        private Image trayBackground;
        private RectTransform[] slotRects;
        private PieceView[] currentPieces;
        private Canvas rootCanvas;
        private float gridCellSize;
        private Theme currentTheme;
        private Func<Piece> pieceFactory;
        private Coroutine relayoutRoutine;
        private readonly int[] activeSlotIndicesBuffer = new int[SlotCount];
        private readonly float[] activeSlotTargetsBuffer = new float[SlotCount];
        private readonly float[] animationStartsBuffer = new float[SlotCount];

        public event Action<PieceView> PieceSpawned;

        public void Build(Transform parent, Canvas canvas, float cellSize, Func<Piece> pieceFactory)
        {
            rootCanvas = canvas;
            gridCellSize = cellSize;
            this.pieceFactory = pieceFactory;

            var go = new GameObject("PieceTrayView", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            trayRect = (RectTransform)go.transform;

            trayRect.anchorMin = new Vector2(0f, 0f);
            trayRect.anchorMax = new Vector2(1f, 0f);
            trayRect.pivot = new Vector2(0.5f, 0f);
            trayRect.sizeDelta = new Vector2(0f, TrayHeight);
            trayRect.anchoredPosition = new Vector2(0f, 40f);

            trayBackground = go.GetComponent<Image>();
            trayBackground.color = new Color(0.05f, 0.06f, 0.10f, 0.7f);

            slotRects = new RectTransform[SlotCount];
            currentPieces = new PieceView[SlotCount];
            for (int slotIndex = 0; slotIndex < SlotCount; slotIndex++)
            {
                var slotGo = new GameObject($"Slot_{slotIndex}", typeof(RectTransform));
                slotGo.transform.SetParent(trayRect, false);
                var slotRect = (RectTransform)slotGo.transform;
                slotRect.anchorMin = new Vector2((slotIndex + 0.5f) / SlotCount, 0.5f);
                slotRect.anchorMax = new Vector2((slotIndex + 0.5f) / SlotCount, 0.5f);
                slotRect.pivot = new Vector2(0.5f, 0.5f);
                slotRect.sizeDelta = new Vector2(TrayHeight - SlotSpacing, TrayHeight - SlotSpacing);
                slotRect.anchoredPosition = Vector2.zero;
                slotRects[slotIndex] = slotRect;
            }
        }

        public void SpawnAll()
        {
            // Anchors may still reflect 2-piece spacing from the previous
            // round; reset before spawning so pieces appear at correct
            // positions immediately.
            for (int slotIndex = 0; slotIndex < SlotCount; slotIndex++)
            {
                SetSlotAnchorX(slotRects[slotIndex], (slotIndex + 0.5f) / SlotCount);
            }
            for (int slotIndex = 0; slotIndex < SlotCount; slotIndex++)
            {
                SpawnInSlot(slotIndex, pieceFactory());
            }
        }

        public void SpawnSpecific(int slotIndex, Piece piece)
        {
            SpawnInSlot(slotIndex, piece);
            RelayoutSlots(animate: false);
        }

        public IReadOnlyList<Piece?> SnapshotCurrentPieces()
        {
            var snapshot = new Piece?[SlotCount];
            for (int i = 0; i < SlotCount; i++)
            {
                snapshot[i] = currentPieces[i] != null ? currentPieces[i].Piece : (Piece?)null;
            }
            return snapshot;
        }

        public void NotifyPiecePlaced(PieceView placedPiece)
        {
            for (int slotIndex = 0; slotIndex < SlotCount; slotIndex++)
            {
                if (currentPieces[slotIndex] == placedPiece)
                {
                    currentPieces[slotIndex] = null;
                    break;
                }
            }
            Destroy(placedPiece.gameObject);

            if (AllSlotsEmpty())
            {
                SpawnAll();
            }
            else
            {
                RelayoutSlots(animate: true);
            }
        }

        public IEnumerable<PieceView> ActivePieces()
        {
            foreach (PieceView piece in currentPieces)
            {
                if (piece != null) yield return piece;
            }
        }

        public bool HasAnyPiece()
        {
            foreach (PieceView piece in currentPieces)
            {
                if (piece != null) return true;
            }
            return false;
        }

        public void ClearAll()
        {
            for (int slotIndex = 0; slotIndex < currentPieces.Length; slotIndex++)
            {
                if (currentPieces[slotIndex] != null)
                {
                    Destroy(currentPieces[slotIndex].gameObject);
                    currentPieces[slotIndex] = null;
                }
            }
        }

        public void ApplyTheme(Theme nextTheme)
        {
            currentTheme = nextTheme;
            if (trayBackground != null)
            {
                Color tinted = nextTheme.GridBackgroundColor;
                trayBackground.color = new Color(tinted.r, tinted.g, tinted.b, 0.7f);
            }
            foreach (PieceView piece in currentPieces)
            {
                if (piece != null) piece.ApplyTheme(nextTheme);
            }
        }

        private void SpawnInSlot(int slotIndex, Piece piece)
        {
            var pieceGo = new GameObject("Piece", typeof(RectTransform));
            pieceGo.transform.SetParent(slotRects[slotIndex], false);

            var pieceView = pieceGo.AddComponent<PieceView>();
            pieceView.Initialize(piece, rootCanvas, gridCellSize);
            if (currentTheme != null) pieceView.ApplyTheme(currentTheme);

            CenterPieceInSlot((RectTransform)pieceGo.transform, piece);

            currentPieces[slotIndex] = pieceView;
            PieceSpawned?.Invoke(pieceView);
        }

        private void CenterPieceInSlot(RectTransform pieceRect, Piece piece)
        {
            int maxOffsetX = 0;
            int maxOffsetY = 0;
            foreach (Vector2Int offset in piece.Cells)
            {
                if (offset.x > maxOffsetX) maxOffsetX = offset.x;
                if (offset.y > maxOffsetY) maxOffsetY = offset.y;
            }
            int boundingWidth = maxOffsetX + 1;
            int boundingHeight = maxOffsetY + 1;

            float visualWidth = boundingWidth * gridCellSize * pieceRect.localScale.x;
            float visualHeight = boundingHeight * gridCellSize * pieceRect.localScale.y;

            pieceRect.anchorMin = new Vector2(0.5f, 0.5f);
            pieceRect.anchorMax = new Vector2(0.5f, 0.5f);
            pieceRect.anchoredPosition = new Vector2(-visualWidth * 0.5f, -visualHeight * 0.5f);
        }

        private bool AllSlotsEmpty()
        {
            foreach (PieceView piece in currentPieces)
            {
                if (piece != null) return false;
            }
            return true;
        }

        private void RelayoutSlots(bool animate)
        {
            if (slotRects == null) return;
            if (relayoutRoutine != null)
            {
                StopCoroutine(relayoutRoutine);
                relayoutRoutine = null;
            }
            if (!animate || !gameObject.activeInHierarchy)
            {
                SnapSlotsToTargets();
                return;
            }
            relayoutRoutine = StartCoroutine(AnimateRelayoutRoutine());
        }

        private void SnapSlotsToTargets()
        {
            int count = GatherActiveSlotTargets();
            for (int i = 0; i < count; i++)
            {
                SetSlotAnchorX(slotRects[activeSlotIndicesBuffer[i]], activeSlotTargetsBuffer[i]);
            }
        }

        private IEnumerator AnimateRelayoutRoutine()
        {
            int count = GatherActiveSlotTargets();
            if (count == 0) { relayoutRoutine = null; yield break; }

            for (int i = 0; i < count; i++)
            {
                animationStartsBuffer[i] = slotRects[activeSlotIndicesBuffer[i]].anchorMin.x;
            }

            yield return TweenUtils.LerpEased(RelayoutAnimDuration, eased =>
            {
                for (int i = 0; i < count; i++)
                {
                    float x = Mathf.Lerp(animationStartsBuffer[i], activeSlotTargetsBuffer[i], eased);
                    SetSlotAnchorX(slotRects[activeSlotIndicesBuffer[i]], x);
                }
            });
            relayoutRoutine = null;
        }

        private int GatherActiveSlotTargets()
        {
            int count = 0;
            for (int i = 0; i < SlotCount; i++)
            {
                if (currentPieces[i] != null)
                {
                    activeSlotIndicesBuffer[count] = i;
                    count++;
                }
            }
            for (int i = 0; i < count; i++)
            {
                activeSlotTargetsBuffer[i] = (i + 0.5f) / count;
            }
            return count;
        }

        private static void SetSlotAnchorX(RectTransform rect, float x)
        {
            rect.anchorMin = new Vector2(x, 0.5f);
            rect.anchorMax = new Vector2(x, 0.5f);
        }
    }
}

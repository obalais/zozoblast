using System;
using Blockblast.Model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Blockblast.View
{
    public class PieceView : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Piece Piece { get; private set; }
        public event Action<PieceView, Vector2> PlacementRequested;
        public event Action<PieceView, Vector2> DragMoved;
        public event Action<PieceView> DragStarted;

        private const float TrayScale = 0.7f;
        private const float DragLiftCanvasUnits = 150f;
        private const float HitAreaPaddingUnscaledPixels = 60f;

        private RectTransform pieceRect;
        private RectTransform canvasRect;
        private Canvas rootCanvas;
        private Transform originalParent;
        private Vector2 originalAnchoredPosition;
        private Vector2 originalAnchorMin;
        private Vector2 originalAnchorMax;
        private float gridCellSize;
        private Theme currentTheme;
        private System.Collections.Generic.List<Image> cellImages = new System.Collections.Generic.List<Image>();

        public void Initialize(Piece piece, Canvas canvas, float cellSize)
        {
            Piece = piece;
            rootCanvas = canvas;
            canvasRect = (RectTransform)canvas.transform;
            gridCellSize = cellSize;
            pieceRect = (RectTransform)transform;

            int maxOffsetX = 0;
            int maxOffsetY = 0;
            foreach (Vector2Int offset in piece.Cells)
            {
                if (offset.x > maxOffsetX) maxOffsetX = offset.x;
                if (offset.y > maxOffsetY) maxOffsetY = offset.y;
            }
            int boundingWidth = maxOffsetX + 1;
            int boundingHeight = maxOffsetY + 1;

            pieceRect.anchorMin = new Vector2(0.5f, 0.5f);
            pieceRect.anchorMax = new Vector2(0.5f, 0.5f);
            pieceRect.pivot = new Vector2(0f, 0f);
            pieceRect.sizeDelta = new Vector2(boundingWidth * cellSize, boundingHeight * cellSize);

            Sprite roundedSquare = CellSpriteFactory.GetRoundedSquare();
            foreach (Vector2Int offset in piece.Cells)
            {
                var cellGo = new GameObject($"Cell_{offset.x}_{offset.y}", typeof(RectTransform), typeof(Image));
                cellGo.transform.SetParent(transform, false);

                var cellRect = (RectTransform)cellGo.transform;
                cellRect.anchorMin = new Vector2(0f, 0f);
                cellRect.anchorMax = new Vector2(0f, 0f);
                cellRect.pivot = new Vector2(0f, 0f);
                cellRect.sizeDelta = new Vector2(cellSize, cellSize);
                cellRect.anchoredPosition = new Vector2(offset.x * cellSize, offset.y * cellSize);

                var cellImage = cellGo.GetComponent<Image>();
                cellImage.sprite = roundedSquare;
                cellImage.type = Image.Type.Sliced;
                cellImage.color = piece.Color;
                cellImage.raycastTarget = false;
                cellImages.Add(cellImage);
            }

            var hitGraphicGo = new GameObject("HitArea", typeof(RectTransform), typeof(Image));
            hitGraphicGo.transform.SetParent(transform, false);
            hitGraphicGo.transform.SetAsFirstSibling();
            var hitRect = (RectTransform)hitGraphicGo.transform;
            hitRect.anchorMin = new Vector2(0f, 0f);
            hitRect.anchorMax = new Vector2(0f, 0f);
            hitRect.pivot = new Vector2(0f, 0f);
            hitRect.sizeDelta = new Vector2(
                boundingWidth * cellSize + 2f * HitAreaPaddingUnscaledPixels,
                boundingHeight * cellSize + 2f * HitAreaPaddingUnscaledPixels
            );
            hitRect.anchoredPosition = new Vector2(-HitAreaPaddingUnscaledPixels, -HitAreaPaddingUnscaledPixels);

            var hitGraphic = hitGraphicGo.GetComponent<Image>();
            hitGraphic.color = new Color(0f, 0f, 0f, 0f);
            hitGraphic.raycastTarget = true;

            transform.localScale = Vector3.one * TrayScale;
        }

        public void SnapBackToTray()
        {
            if (originalParent == null) return;
            transform.SetParent(originalParent, false);
            pieceRect.anchorMin = originalAnchorMin;
            pieceRect.anchorMax = originalAnchorMax;
            pieceRect.anchoredPosition = originalAnchoredPosition;
            transform.localScale = Vector3.one * TrayScale;
        }

        public void ApplyTheme(Theme nextTheme)
        {
            currentTheme = nextTheme;
            Color tinted = nextTheme != null ? nextTheme.ColorFilter(Piece.Color) : Piece.Color;
            foreach (Image cellImage in cellImages)
            {
                if (cellImage == null) continue;
                cellImage.color = tinted;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            originalParent = transform.parent;
            originalAnchorMin = pieceRect.anchorMin;
            originalAnchorMax = pieceRect.anchorMax;
            originalAnchoredPosition = pieceRect.anchoredPosition;

            transform.SetParent(rootCanvas.transform, true);
            transform.SetAsLastSibling();
            transform.localScale = Vector3.one;

            UpdateAnchorFromPointer(eventData);
            DragStarted?.Invoke(this);
            DragMoved?.Invoke(this, CurrentVirtualAnchorScreenPoint());
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateAnchorFromPointer(eventData);
            DragMoved?.Invoke(this, CurrentVirtualAnchorScreenPoint());
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            PlacementRequested?.Invoke(this, CurrentVirtualAnchorScreenPoint());
        }

        private Vector2 CurrentVirtualAnchorScreenPoint()
        {
            Vector3 virtualAnchorLocal = new Vector3(gridCellSize * 0.5f, gridCellSize * 0.5f, 0f);
            Vector3 virtualAnchorWorld = pieceRect.TransformPoint(virtualAnchorLocal);
            return RectTransformUtility.WorldToScreenPoint(rootCanvas.worldCamera, virtualAnchorWorld);
        }

        private void UpdateAnchorFromPointer(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, eventData.position, eventData.pressEventCamera, out Vector2 canvasLocal))
            {
                return;
            }

            int maxOffsetX = 0;
            int maxOffsetY = 0;
            foreach (Vector2Int offset in Piece.Cells)
            {
                if (offset.x > maxOffsetX) maxOffsetX = offset.x;
                if (offset.y > maxOffsetY) maxOffsetY = offset.y;
            }
            float halfBoundingWidth = (maxOffsetX + 1) * 0.5f * gridCellSize;

            pieceRect.anchoredPosition = new Vector2(
                canvasLocal.x - halfBoundingWidth,
                canvasLocal.y + DragLiftCanvasUnits
            );
        }
    }
}

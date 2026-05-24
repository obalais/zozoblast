using System.Collections;
using System.Collections.Generic;
using Blockblast.Model;
using UnityEngine;
using UnityEngine.UI;
using Grid = Blockblast.Model.Grid;

namespace Blockblast.View
{
    public class GridView : MonoBehaviour
    {
        public const float CellSize = 100f;
        private const float CellSpacing = 4f;
        private const float ThemeTransitionDuration = 0.6f;

        private RectTransform gridRect;
        private GridLayoutGroup gridLayout;
        private Image gridBackground;
        private Image[,] cellImages;
        private Theme currentTheme;
        private Grid lastRenderedGrid;
        private Coroutine themeTransitionRoutine;
        private readonly List<Vector2Int> previewedCells = new List<Vector2Int>();

        public RectTransform RectTransform => gridRect;
        public float StepSize => CellSize + CellSpacing;

        public void Build(Transform parent)
        {
            var go = new GameObject("GridView", typeof(RectTransform), typeof(Image), typeof(GridLayoutGroup));
            go.transform.SetParent(parent, false);

            gridRect = (RectTransform)go.transform;
            gridRect.pivot = new Vector2(0.5f, 0.5f);
            gridRect.anchorMin = new Vector2(0.5f, 0.5f);
            gridRect.anchorMax = new Vector2(0.5f, 0.5f);
            float gridDimension = Grid.Size * CellSize + (Grid.Size - 1) * CellSpacing;
            gridRect.sizeDelta = new Vector2(gridDimension, gridDimension);

            gridBackground = go.GetComponent<Image>();

            gridLayout = go.GetComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(CellSize, CellSize);
            gridLayout.spacing = new Vector2(CellSpacing, CellSpacing);
            gridLayout.startCorner = GridLayoutGroup.Corner.LowerLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = Grid.Size;

            cellImages = new Image[Grid.Size, Grid.Size];
            Sprite roundedSquare = CellSpriteFactory.GetRoundedSquare();
            for (int y = 0; y < Grid.Size; y++)
            {
                for (int x = 0; x < Grid.Size; x++)
                {
                    var cell = new GameObject($"Cell_{x}_{y}", typeof(RectTransform), typeof(Image));
                    cell.transform.SetParent(go.transform, false);
                    var image = cell.GetComponent<Image>();
                    image.sprite = roundedSquare;
                    image.type = Image.Type.Sliced;
                    image.raycastTarget = false;
                    cellImages[x, y] = image;
                }
            }

            ApplyThemeInstant(ThemeManagerDefault());
        }

        public void Render(Grid model)
        {
            lastRenderedGrid = model;
            for (int y = 0; y < Grid.Size; y++)
            {
                for (int x = 0; x < Grid.Size; x++)
                {
                    cellImages[x, y].color = ResolveCellColor(model.GetCell(x, y));
                }
            }
        }

        public void ApplyTheme(Theme nextTheme, bool animated = true)
        {
            if (themeTransitionRoutine != null) StopCoroutine(themeTransitionRoutine);

            if (!animated)
            {
                ApplyThemeInstant(nextTheme);
                return;
            }

            themeTransitionRoutine = StartCoroutine(TransitionToTheme(nextTheme));
        }

        public IEnumerator AnimateLineClear(List<int> rows, List<int> columns, int comboCount)
        {
            float comboIntensity = Mathf.Clamp01((comboCount - 1) / 3f);
            float baseDuration = Mathf.Lerp(0.28f, 0.62f, comboIntensity);
            float finalCellScale = Mathf.Lerp(0.55f, 0.10f, comboIntensity);
            float peakCellScale = 1f + 0.22f * comboIntensity;
            float flashPhaseRatio = Mathf.Lerp(0.30f, 0.50f, comboIntensity);
            float punchPhaseRatio = comboCount >= 2 ? 0.22f : 0f;
            float maxWaveDelay = 0.10f * comboIntensity;
            Color accentColor = currentTheme != null ? currentTheme.CelebrationAccentColor : Color.white;
            Color flashColor = Color.Lerp(Color.white, accentColor, comboIntensity * 0.45f);

            var animatedCells = new HashSet<Vector2Int>();
            foreach (int rowIndex in rows)
            {
                for (int columnIndex = 0; columnIndex < Grid.Size; columnIndex++)
                {
                    animatedCells.Add(new Vector2Int(columnIndex, rowIndex));
                }
            }
            foreach (int columnIndex in columns)
            {
                for (int rowIndex = 0; rowIndex < Grid.Size; rowIndex++)
                {
                    animatedCells.Add(new Vector2Int(columnIndex, rowIndex));
                }
            }

            var targetImages = new List<Image>(animatedCells.Count);
            var originalColors = new List<Color>(animatedCells.Count);
            var cellWaveDelays = new List<float>(animatedCells.Count);
            float waveDenominator = Mathf.Max(1f, (Grid.Size - 1) * 2f);
            foreach (Vector2Int coord in animatedCells)
            {
                Image cellImage = cellImages[coord.x, coord.y];
                targetImages.Add(cellImage);
                originalColors.Add(cellImage.color);
                cellWaveDelays.Add(((coord.x + coord.y) / waveDenominator) * maxWaveDelay);
            }

            float totalDuration = baseDuration + maxWaveDelay;
            float elapsedTime = 0f;
            while (elapsedTime < totalDuration)
            {
                for (int i = 0; i < targetImages.Count; i++)
                {
                    float cellElapsed = elapsedTime - cellWaveDelays[i];
                    if (cellElapsed < 0f) continue;
                    float cellProgress = Mathf.Clamp01(cellElapsed / baseDuration);
                    float alpha = 1f - cellProgress;

                    float cellScale;
                    if (punchPhaseRatio > 0f && cellProgress < punchPhaseRatio)
                    {
                        cellScale = Mathf.Lerp(1f, peakCellScale, cellProgress / punchPhaseRatio);
                    }
                    else
                    {
                        float postPunchProgress = punchPhaseRatio > 0f
                            ? (cellProgress - punchPhaseRatio) / (1f - punchPhaseRatio)
                            : cellProgress;
                        float startScale = punchPhaseRatio > 0f ? peakCellScale : 1f;
                        cellScale = Mathf.Lerp(startScale, finalCellScale, postPunchProgress);
                    }

                    Color baseColor = originalColors[i];
                    Color displayColor;
                    if (cellProgress < flashPhaseRatio)
                    {
                        float flashAmount = 1f - (cellProgress / flashPhaseRatio);
                        displayColor = Color.Lerp(baseColor, flashColor, flashAmount);
                    }
                    else
                    {
                        displayColor = baseColor;
                    }

                    targetImages[i].color = new Color(displayColor.r, displayColor.g, displayColor.b, alpha);
                    targetImages[i].transform.localScale = Vector3.one * cellScale;
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            for (int i = 0; i < targetImages.Count; i++)
            {
                targetImages[i].transform.localScale = Vector3.one;
            }
        }

        public void ShowPlacementPreview(Piece piece, Vector2Int origin)
        {
            ClearPlacementPreview();
            if (lastRenderedGrid == null) return;

            Color baseColor = currentTheme != null ? currentTheme.ColorFilter(piece.Color) : piece.Color;
            Color previewColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.42f);

            foreach (Vector2Int offset in piece.Cells)
            {
                int x = origin.x + offset.x;
                int y = origin.y + offset.y;
                if (x < 0 || x >= Grid.Size || y < 0 || y >= Grid.Size) continue;
                if (lastRenderedGrid.GetCell(x, y) != null) continue;

                cellImages[x, y].color = previewColor;
                previewedCells.Add(new Vector2Int(x, y));
            }
        }

        public void ClearPlacementPreview()
        {
            if (previewedCells.Count == 0) return;
            foreach (Vector2Int coord in previewedCells)
            {
                Color? state = lastRenderedGrid != null ? lastRenderedGrid.GetCell(coord.x, coord.y) : null;
                cellImages[coord.x, coord.y].color = ResolveCellColor(state);
            }
            previewedCells.Clear();
        }

        public bool TryGetCellAtScreenPoint(Vector2 screenPoint, Camera camera, out Vector2Int gridCoord)
        {
            gridCoord = default;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(gridRect, screenPoint, camera, out Vector2 localPoint))
            {
                return false;
            }

            float halfSize = gridRect.rect.width / 2f;
            float normalizedX = (localPoint.x + halfSize) / StepSize;
            float normalizedY = (localPoint.y + halfSize) / StepSize;
            int candidateX = Mathf.FloorToInt(normalizedX);
            int candidateY = Mathf.FloorToInt(normalizedY);

            if (candidateX < 0 || candidateX >= Grid.Size || candidateY < 0 || candidateY >= Grid.Size)
            {
                return false;
            }

            gridCoord = new Vector2Int(candidateX, candidateY);
            return true;
        }

        public Image GetCellImage(int x, int y) => cellImages[x, y];

        private void ApplyThemeInstant(Theme nextTheme)
        {
            currentTheme = nextTheme;
            if (gridBackground != null) gridBackground.color = nextTheme.GridBackgroundColor;
            if (lastRenderedGrid != null) Render(lastRenderedGrid);
            else RepaintAsEmpty();
        }

        private IEnumerator TransitionToTheme(Theme nextTheme)
        {
            Theme previousTheme = currentTheme;
            currentTheme = nextTheme;

            Color startGridBg = gridBackground != null ? gridBackground.color : nextTheme.GridBackgroundColor;
            Color targetGridBg = nextTheme.GridBackgroundColor;

            var startColors = new Color[Grid.Size, Grid.Size];
            var targetColors = new Color[Grid.Size, Grid.Size];
            for (int y = 0; y < Grid.Size; y++)
            {
                for (int x = 0; x < Grid.Size; x++)
                {
                    startColors[x, y] = cellImages[x, y].color;
                    targetColors[x, y] = ResolveCellColor(lastRenderedGrid != null ? lastRenderedGrid.GetCell(x, y) : null);
                }
            }

            yield return TweenUtils.LerpEased(ThemeTransitionDuration, eased =>
            {
                if (gridBackground != null) gridBackground.color = Color.Lerp(startGridBg, targetGridBg, eased);
                for (int y = 0; y < Grid.Size; y++)
                {
                    for (int x = 0; x < Grid.Size; x++)
                    {
                        cellImages[x, y].color = Color.Lerp(startColors[x, y], targetColors[x, y], eased);
                    }
                }
            });
            themeTransitionRoutine = null;
        }

        private Color ResolveCellColor(Color? cellState)
        {
            if (currentTheme == null) return cellState ?? new Color(0.18f, 0.20f, 0.28f, 1f);
            return cellState.HasValue ? currentTheme.ColorFilter(cellState.Value) : currentTheme.EmptyCellColor;
        }

        private void RepaintAsEmpty()
        {
            Color emptyColor = currentTheme != null ? currentTheme.EmptyCellColor : new Color(0.18f, 0.20f, 0.28f, 1f);
            for (int y = 0; y < Grid.Size; y++)
            {
                for (int x = 0; x < Grid.Size; x++)
                {
                    cellImages[x, y].color = emptyColor;
                }
            }
        }

        private static Theme ThemeManagerDefault() => Blockblast.Controller.ThemeManager.GetForLevel(0);
    }
}

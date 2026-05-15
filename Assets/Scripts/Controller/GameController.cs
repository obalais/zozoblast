using System.Collections;
using System.Collections.Generic;
using Blockblast.Audio;
using Blockblast.Model;
using Blockblast.Persistence;
using Blockblast.View;
using UnityEngine;
using UnityEngine.UI;
using Grid = Blockblast.Model.Grid;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Blockblast.Controller
{
    public class GameController : MonoBehaviour
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void SyncPersistentDataPathToIndexedDB();
#endif

        private const int LineClearScore = 10;
        private const string BestScorePrefKey = "BestScore";

        private Grid grid;
        private GridView gridView;
        private PieceTrayView trayView;
        private GameOverPanel gameOverPanel;
        private AudioManager audioManager;
        private ScoreboardEffects scoreboardEffects;
        private Canvas rootCanvas;
        private Image backgroundImage;
        private Text scoreText;
        private Text bestScoreText;
        private int currentScore;
        private int bestScore;
        private int recordAtGameStart;
        private int currentLevel;
        private bool isGameOver;

        private void Awake()
        {
            audioManager = gameObject.AddComponent<AudioManager>();
            BuildHierarchy();
            grid = new Grid();
            bestScore = PlayerPrefs.GetInt(BestScorePrefKey, 0);
            trayView.PieceSpawned += AttachPieceHandlers;
            gameOverPanel.RestartRequested += StartNewGame;

            if (GameStateStore.TryLoad(out GameStateStore.SavedState saved))
            {
                RestoreSavedState(saved);
            }
            else
            {
                StartNewGame();
            }
        }

        private void BuildHierarchy()
        {
            UIBuilder.BuildEventSystem();
            rootCanvas = UIBuilder.BuildCanvas(transform);
            backgroundImage = UIBuilder.BuildBackground(rootCanvas.transform);
            RectTransform scoreRect;
            (scoreText, scoreRect) = UIBuilder.BuildScoreText(rootCanvas.transform);
            bestScoreText = UIBuilder.BuildBestScoreText(rootCanvas.transform);
            gridView = UIBuilder.BuildGridView(rootCanvas.transform);
            trayView = UIBuilder.BuildTrayView(rootCanvas.transform, rootCanvas, GridView.CellSize, GenerateNextPiece);
            RectTransform celebrationOverlayRect = UIBuilder.BuildCelebrationOverlay(rootCanvas.transform);
            UIBuilder.BuildNewGameButton(rootCanvas.transform, StartNewGame);
            gameOverPanel = UIBuilder.BuildGameOverPanel(rootCanvas.transform);

            scoreboardEffects = new ScoreboardEffects(this, scoreText, scoreRect, backgroundImage, celebrationOverlayRect);
        }

        private void RestoreSavedState(GameStateStore.SavedState saved)
        {
            currentScore = saved.score;
            currentLevel = saved.level;
            recordAtGameStart = bestScore;
            ApplyThemeForCurrentLevel(animated: false);
            for (int x = 0; x < Grid.Size; x++)
            {
                for (int y = 0; y < Grid.Size; y++)
                {
                    var savedCell = saved.cells[x * Grid.Size + y];
                    grid.SetCell(x, y, savedCell.filled ? (Color?)new Color(savedCell.r, savedCell.g, savedCell.b, 1f) : null);
                }
            }
            gridView.Render(grid);
            UpdateScoreDisplay();

            for (int slotIndex = 0; slotIndex < saved.trayPieces.Count && slotIndex < 3; slotIndex++)
            {
                var savedPiece = saved.trayPieces[slotIndex];
                if (savedPiece.present)
                {
                    trayView.SpawnSpecific(slotIndex, GameStateStore.ToPiece(savedPiece));
                }
            }
            if (!trayView.HasAnyPiece()) trayView.SpawnAll();
        }

        private void StartNewGame()
        {
            isGameOver = false;
            grid.Reset();
            currentScore = 0;
            currentLevel = 0;
            recordAtGameStart = bestScore;
            ApplyThemeForCurrentLevel(animated: true);
            gridView.Render(grid);
            UpdateScoreDisplay();
            gameOverPanel.Hide();
            trayView.ClearAll();
            trayView.SpawnAll();
            PersistGameState();
        }

        private Piece GenerateNextPiece()
        {
            return PieceShapes.PickForContext(currentLevel, grid);
        }

        private void AttachPieceHandlers(PieceView pieceView)
        {
            pieceView.PlacementRequested += HandlePlacementRequested;
            pieceView.DragMoved += HandleDragMoved;
            pieceView.DragStarted += HandleDragStarted;
        }

        private void HandleDragStarted(PieceView pieceView)
        {
            gridView.ClearPlacementPreview();
        }

        private void HandleDragMoved(PieceView pieceView, Vector2 anchorScreenPoint)
        {
            if (isGameOver) return;
            Camera eventCamera = ResolveEventCamera();
            if (gridView.TryGetCellAtScreenPoint(anchorScreenPoint, eventCamera, out Vector2Int gridOrigin)
                && grid.CanPlace(pieceView.Piece, gridOrigin))
            {
                gridView.ShowPlacementPreview(pieceView.Piece, gridOrigin);
            }
            else
            {
                gridView.ClearPlacementPreview();
            }
        }

        private void HandlePlacementRequested(PieceView pieceView, Vector2 anchorScreenPoint)
        {
            gridView.ClearPlacementPreview();

            if (isGameOver)
            {
                pieceView.SnapBackToTray();
                return;
            }

            Camera eventCamera = ResolveEventCamera();

            bool placed = false;
            if (gridView.TryGetCellAtScreenPoint(anchorScreenPoint, eventCamera, out Vector2Int gridOrigin))
            {
                if (grid.CanPlace(pieceView.Piece, gridOrigin))
                {
                    PlacePieceOnGrid(pieceView, gridOrigin);
                    placed = true;
                }
            }

            if (!placed)
            {
                pieceView.SnapBackToTray();
                audioManager.PlayInvalid();
            }
        }

        private Camera ResolveEventCamera()
        {
            return rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
        }

        private void PlacePieceOnGrid(PieceView pieceView, Vector2Int gridOrigin)
        {
            grid.Place(pieceView.Piece, gridOrigin);
            int placementPoints = pieceView.Piece.Cells.Length;
            currentScore += placementPoints;
            gridView.Render(grid);
            audioManager.PlayPlace();
            UpdateScoreDisplay();

            List<int> fullRows = grid.GetFullRows();
            List<int> fullColumns = grid.GetFullColumns();
            bool hasLinesToClear = fullRows.Count > 0 || fullColumns.Count > 0;
            bool isDynamicClear = (fullRows.Count + fullColumns.Count) >= 2;
            int clearBonus = hasLinesToClear ? (fullRows.Count + fullColumns.Count) * LineClearScore : 0;

            trayView.NotifyPiecePlaced(pieceView);

            if (hasLinesToClear)
            {
                scoreboardEffects.BumpScore(big: false, accentColor: scoreText.color);
                audioManager.PlayClear();
                if (isDynamicClear)
                {
                    scoreboardEffects.PulseBackground();
                    scoreboardEffects.ShowFloatingBonus(clearBonus, ThemeManager.GetForLevel(currentLevel).CelebrationAccentColor);
                }
                StartCoroutine(AnimateThenClearAndApplyBonus(fullRows, fullColumns, clearBonus, isDynamicClear));
            }
            else
            {
                ApplyScoreLevelChanges();
                PersistGameState();
                CheckGameOver();
            }
        }

        private IEnumerator AnimateThenClearAndApplyBonus(List<int> fullRows, List<int> fullColumns, int clearBonus, bool dynamicClear)
        {
            yield return StartCoroutine(gridView.AnimateLineClear(fullRows, fullColumns, dynamicClear));
            grid.ClearLines(fullRows, fullColumns);
            gridView.Render(grid);
            currentScore += clearBonus;
            UpdateScoreDisplay();
            ApplyScoreLevelChanges();
            PersistGameState();
            CheckGameOver();
        }

        private void ApplyScoreLevelChanges()
        {
            int newLevel = ThemeManager.LevelForScore(currentScore);
            Theme currentTheme = ThemeManager.GetForLevel(currentLevel);
            if (newLevel > currentLevel)
            {
                currentLevel = newLevel;
                Theme newTheme = ThemeManager.GetForLevel(currentLevel);
                ApplyThemeForCurrentLevel(animated: true);
                scoreboardEffects.ShowLevelCelebration(newTheme.Name, newTheme.CelebrationAccentColor);
                scoreboardEffects.BumpScore(big: true, accentColor: newTheme.CelebrationAccentColor);
            }
            else
            {
                scoreboardEffects.BumpScore(big: false, accentColor: currentTheme.CelebrationAccentColor);
            }
        }

        private void CheckGameOver()
        {
            foreach (PieceView remainingPiece in trayView.ActivePieces())
            {
                if (grid.CanFitAnywhere(remainingPiece.Piece)) return;
            }

            isGameOver = true;
            bool isNewRecord = currentScore > recordAtGameStart;
            GameStateStore.Clear();
            audioManager.PlayGameOver();
            gameOverPanel.Show(currentScore, bestScore, isNewRecord);
        }

        private void UpdateScoreDisplay()
        {
            scoreText.text = $"Score: {currentScore}";
            if (currentScore > bestScore)
            {
                bestScore = currentScore;
                PlayerPrefs.SetInt(BestScorePrefKey, bestScore);
                PlayerPrefs.Save();
#if UNITY_WEBGL && !UNITY_EDITOR
                SyncPersistentDataPathToIndexedDB();
#endif
            }
            if (bestScoreText != null) bestScoreText.text = $"Meilleur : {bestScore}";
        }

        private void PersistGameState()
        {
            if (isGameOver) return;
            GameStateStore.Save(grid, trayView.SnapshotCurrentPieces(), currentScore, currentLevel);
#if UNITY_WEBGL && !UNITY_EDITOR
            SyncPersistentDataPathToIndexedDB();
#endif
        }

        private void ApplyThemeForCurrentLevel(bool animated)
        {
            Theme theme = ThemeManager.GetForLevel(currentLevel);
            if (backgroundImage != null) backgroundImage.color = theme.BackgroundColor;
            if (scoreText != null) scoreText.color = theme.ScoreTextColor;
            if (bestScoreText != null)
            {
                Color baseScoreColor = theme.ScoreTextColor;
                bestScoreText.color = new Color(baseScoreColor.r, baseScoreColor.g, baseScoreColor.b, 0.65f);
            }
            gridView.ApplyTheme(theme, animated);
            trayView.ApplyTheme(theme);
        }
    }
}

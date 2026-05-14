using System;
using UnityEngine;

namespace Blockblast.View
{
    public class Theme
    {
        public readonly string Name;
        public readonly Func<Color, Color> ColorFilter;
        public readonly Color BackgroundColor;
        public readonly Color GridBackgroundColor;
        public readonly Color EmptyCellColor;
        public readonly Color ScoreTextColor;
        public readonly Color CelebrationAccentColor;

        public Theme(
            string name,
            Func<Color, Color> colorFilter,
            Color backgroundColor,
            Color gridBackgroundColor,
            Color emptyCellColor,
            Color scoreTextColor,
            Color celebrationAccentColor)
        {
            Name = name;
            ColorFilter = colorFilter;
            BackgroundColor = backgroundColor;
            GridBackgroundColor = gridBackgroundColor;
            EmptyCellColor = emptyCellColor;
            ScoreTextColor = scoreTextColor;
            CelebrationAccentColor = celebrationAccentColor;
        }
    }
}

using System;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    public static class UITools
    {
        public static void DrawBoxGridView(out Rect viewRect, out Rect outRect, ref Rect inRect, ref Vector2 scrollPosition, int perRow, int gap, Action<int, Rect> iterateAction, int iteratorSize, Action<float, float> endAction, int boxHeightDivisor = 1)
        {
            var boxWidth = (inRect.width - gap * perRow) / perRow;
            var boxHeight = boxWidth;
            
            if (boxHeightDivisor != 1)
            {
                boxHeight = boxWidth / boxHeightDivisor;
            }
            
            viewRect = new Rect(0, 0, inRect.width - gap, (Mathf.Ceil((float) (iteratorSize) / perRow)) * boxWidth + (iteratorSize / perRow) * gap);
            outRect = new Rect(inRect.AtZero());
            
            GUI.BeginGroup(inRect);
            
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            for (var i = 0; i < iteratorSize; i++)
            {
                var y = boxHeight * Mathf.Floor((float) i / perRow) + (i / perRow) * gap;
                
                var boxRect = new Rect((boxWidth * (i % perRow)) + (i % perRow) * gap, y, boxWidth, boxHeight);

                iterateAction?.Invoke(i, boxRect);
            }

            endAction?.Invoke(boxWidth, boxHeight);

            Widgets.EndScrollView();
            
            GUI.EndGroup();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Harmony;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    public static class ScrollableListUI
    {
        public static void DrawList(ref Rect inRect, ref Vector2 scrollPosition, ref List<ScrollableListItem> items)
        {
            var vector2_1 = new Vector2(inRect.width - 16f, 36f);
            var vector2_2 = new Vector2(100f, vector2_1.y - 2f);

            inRect.height -= 45f;

            var height = (float) items.Count * (vector2_1.y + 3f);
            
            var viewRect = new Rect(0.0f, 0.0f, inRect.width - 16f, height);
            var outRect = new Rect(inRect.AtZero());
            
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect, true);

            var y = 0.0f;
            var num = 0;

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                
                if ((double) y + (double) vector2_1.y >= (double) scrollPosition.y &&
                    (double) y <= (double) scrollPosition.y + (double) outRect.height)
                {
                    var rect1 = new Rect(0.0f, y, vector2_1.x, vector2_1.y);
                    
                    if(num % 2 == 0)
                        Widgets.DrawAltRect(rect1);

                    var position = rect1.ContractedBy(1f);
                    
                    GUI.BeginGroup(position);

                    var text = item.Text;
                    GUI.color = new Color(1f, 1f, 0.6f); // Color of item name..
                    
                    var rect2 = new Rect(15f, 0.0f, (float) byte.MaxValue, position.height);

                    Text.Anchor = TextAnchor.MiddleLeft;
                    Text.Font = GameFont.Small;
                    
                    Widgets.Label(rect2, text);

                    GUI.color = Color.white;

                    if (item.Info != null && item.Info.Count > 0)
                    {                        
                        //var rect3 = new Rect(270f, 0.0f, 200f, position.height);
                        var rect3 = new Rect(inRect.width * 0.5f, 0.0f, 200f, position.height);
                        
                        DrawItemInfo(item, rect3);
                    }

                    GUI.color = Color.white;

                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.Font = GameFont.Small;

                    var x = vector2_1.x - 2f - vector2_2.x - vector2_2.y;

                    /*
                     * Color changing..
                     */
                    if (item.canChangeColor)
                    {
                        var colorBoxX = x - (vector2_2.y + 5);
                        var colorBoxRect = new Rect(colorBoxX, 0.0f, vector2_2.y, vector2_2.y);

                        if (item.texture == null)
                        {
                            var texture = new Texture2D((int) colorBoxRect.width, (int) colorBoxRect.height,
                                TextureFormat.RGBA32, false);

                            item.texture = texture;
                        }
                        
                        GUI.color = item.color;
                        var press = Widgets.ButtonImage(colorBoxRect, item.texture);
                        GUI.color = Color.white;
                        
                        if (press)
                        {
                            Find.WindowStack.Add(new Dialog_ColorPicker(item));
                        }
                    }
                    /*
                     * End
                     */
                    
                    if(!string.IsNullOrEmpty(item.ActionButtonText))
                        if (Widgets.ButtonText(new Rect(x, 0.0f, vector2_2.x, vector2_2.y), item.ActionButtonText, true, false,
                            true))
                            item.ActionButtonAction();
                    
                    var rect4 = new Rect((float) ((double) x + (double) vector2_2.x + 5.0), 0.0f, vector2_2.y, vector2_2.y);
                    
                    var deleteXTexture = ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true);

                    if (item.DeleteButtonAction != null)
                    {
                        if (Widgets.ButtonImage(rect4, deleteXTexture, Color.white, GenUI.SubtleMouseoverColor))
                        {
                            item.DeleteButtonAction();
                        }
                    }

                    if (!string.IsNullOrEmpty(item.DeleteButtonTooltip))
                    {
                        TooltipHandler.TipRegion(rect4, (TipSignal) item.DeleteButtonTooltip);
                    }
                    
                    GUI.EndGroup();
                }

                y += vector2_1.y + 3f;
                num++;
            }
            
            Widgets.EndScrollView();
        }

        private static void DrawItemInfo(ScrollableListItem item, Rect rect)
        {
            GUI.BeginGroup(rect);

            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperLeft;

            var prevRect = new Rect(0.0f, 2f, 0, 0);
            
            // TODO: Add support for more than two elements by using a List<ScrollableListItemInfo> with a Color, Height/Width proportion value and text.
            foreach (var info in item.Info)
            {
                var rectInfo = new Rect(0.0f, prevRect.yMax, rect.width, rect.height / item.Info.Count);

                GUI.color = info.color;
                
                Widgets.Label(rectInfo, info.Text);

                if (!string.IsNullOrEmpty(info.tooltip))
                {
                    TooltipHandler.TipRegion(rectInfo, info.tooltip);
                }

                prevRect = rectInfo;
            }
            
            GUI.EndGroup();
        }
    }
}
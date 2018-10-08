using System;
using System.Collections.Generic;
using UnityEngine;

namespace PersistentWorlds.UI
{
    public sealed class ScrollableListItem
    {
        public string Text;
        
        public string ActionButtonText;
        public Action ActionButtonAction;
        
        public Action DeleteButtonAction;
        public string DeleteButtonTooltip;

        public bool canChangeColor;
        public Color color = Color.white;
        public Texture2D texture;
        
        public List<ScrollableListItemInfo> Info = new List<ScrollableListItemInfo>();
    }
}
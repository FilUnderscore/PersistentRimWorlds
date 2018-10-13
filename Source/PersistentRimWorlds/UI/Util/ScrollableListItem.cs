using System;
using System.Collections.Generic;
using UnityEngine;

namespace PersistentWorlds.UI
{
    public class ScrollableListItem
    {
        #region Fields
        public string Text;
        
        public string ActionButtonText;
        public Action ActionButtonAction;
        
        public Action DeleteButtonAction;
        public string DeleteButtonTooltip;

        public Texture2D texture;
        
        public readonly List<ScrollableListItemInfo> Info = new List<ScrollableListItemInfo>();
        #endregion
    }
}
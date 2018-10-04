using System;
using System.Collections.Generic;

namespace PersistentWorlds.UI
{
    public sealed class ScrollableListItem
    {
        public string Text;
        
        public string ActionButtonText;
        public Action ActionButtonAction;
        
        public Action DeleteButtonAction;
        public string DeleteButtonTooltip;
        
        public List<ScrollableListItemInfo> Info = new List<ScrollableListItemInfo>();
    }
}
using UnityEngine;

namespace PersistentWorlds.UI
{
    public sealed class ScrollableListItemColored : ScrollableListItem
    {
        #region Fields
        public bool canChangeColor;
        public Color color = Color.white;
        #endregion
    }
}
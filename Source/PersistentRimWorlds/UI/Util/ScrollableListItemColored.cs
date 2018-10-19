using UnityEngine;

namespace PersistentWorlds.UI
{
    public sealed class ScrollableListItemColored : ScrollableListItem
    {
        #region Fields
        public bool canSeeColor;
        public bool canChangeColor;

        private bool colorRefSet;
        private Color color;
        #endregion
        
        #region Properties
        public Color Color
        {
            get => color;
            set
            {
                if (!colorRefSet)
                {
                    colorRefSet = true;
                    color = value;

                    return;
                }
                
                color.r = value.r;
                color.g = value.g;
                color.b = value.b;
                color.a = value.a;
            }
        }
        #endregion
    }
}
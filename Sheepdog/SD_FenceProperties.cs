using System;
using System.Drawing;

namespace Grasshopper.Kernel.Special
{
    /*public enum NameVerticalStates
    {
        Top,
        Bottom
    }
    public enum NameHorizontalStates
    {
        Left,
        Centre,
        Right
    }

    public enum NamePlacementStates
    {
        Inside,
        Outside
    }*/

    /// <exclude />
    public struct SD_FenceProperties
    {
        public void SetDefault()
        {
            GH_SettingsServer settings = new GH_SettingsServer("Sheepdog");
            this.Width = settings.GetValue("Width", 3);
            this.Colour = settings.GetValue("Colour", Color.Black);
            this.Pattern = settings.GetValue("Pattern", "Continuous");
            this.NameSize = settings.GetValue("NameSize", 12);
            this.NameVertical = settings.GetValue("NameVertical", "Top"); //NameVerticalStates.Top;
            this.NameHorizontal = settings.GetValue("NameHorizontal", "Left"); //NameHorizontalStates.Left;
            this.NamePlacement = settings.GetValue("NamePlacement", "Inside"); //NamePlacementStates.Outside;
        }

        public float Width { get; set; }

        public Color Colour { get; set; }

        public string Pattern { get; set; }

        public Single NameSize { get; set; } // using single because that is the format of the font size in the graphics.DrawString method

        public string NameVertical { get; set; } //NameVerticalStates NameVertical { get; set; }

        public string NameHorizontal { get; set; }

        public string NamePlacement { get; set; }
    }
}

using Grasshopper.GUI.Canvas;
//using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel.Special;
using GH_IO.Serialization;

namespace Sheepdog
{
    public class SD_FenceAttributes : SD_ResizableAttributes<SD_Fence>
    {
        //private Grasshopper.Kernel.Special.GH_Markup mark = new Grasshopper.Kernel.Special.GH_Markup();
        private GH_Document ghDocument;
        public Size DefaultSize { get; private set; }
        //public DisplayExpiredEventHandler DisplayExpiredHandler;
        public SD_FenceAttributes(SD_Fence owner) : base(owner)
        {
            // Set value to ghDocument
            ghDocument = this.Owner.OnPingDocument();

            // Set the DefaultSize
            this.DefaultSize = new Size(100, 50);

            // Adjust the Bounds property to reflect the DefaultSize. Assuming the Pivot is the starting point for drawing
            this.Bounds = new RectangleF(this.Pivot.X, this.Pivot.Y, DefaultSize.Width, DefaultSize.Height);

            // Initialize the properties and set default values
            SD_FenceProperties fenceProperties = new SD_FenceProperties();
            fenceProperties.SetDefault();
            this.Properties = fenceProperties;

            GH_SettingsServer settings = new GH_SettingsServer("Sheepdog");
            settings.SetValue("Width", this.Owner.WidthMedium); // Reference to WidthMedium property
            settings.SetValue("Colour", Color.Black);
            settings.SetValue("Pattern", "Continuous");
            settings.SetValue("NameSize", 12);
            settings.SetValue("NameVertical", "Top"); //NameVerticalStates.Top;
            settings.SetValue("NameHorizontal", "Left"); //NameHorizontalStates.Left;
            settings.SetValue("NamePlacement", "Inside"); //NamePlacementStates.Outside;
            settings.WritePersistentSettings();
        }

        // Rest of the code...
    }
}
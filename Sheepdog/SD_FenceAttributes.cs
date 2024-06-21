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
        //public DisplayExpiredEventHandler DisplayExpiredHandler;
        public SD_FenceAttributes(SD_Fence owner) : base(owner)
        {
            // set value to ghDocument
            ghDocument = this.Owner.OnPingDocument();

            if (!this.Properties.IsAnyPropertySet())
            {
                SD_FenceProperties fenceProperties = new SD_FenceProperties();
                fenceProperties.SetDefault();
                this.Properties = fenceProperties;
            }
            
            GH_SettingsServer settings = new GH_SettingsServer("Sheepdog");
            settings.SetValue("Width", 3f);
            settings.SetValue("Colour", Color.Black);
            settings.SetValue("Pattern", "Continuous");
            settings.SetValue("NameSize", 12f);
            settings.SetValue("NameVertical", "Top"); //NameVerticalStates.Top;
            settings.SetValue("NameHorizontal", "Left"); //NameHorizontalStates.Left;
            settings.SetValue("NamePlacement", "Inside"); //NamePlacementStates.Outside;
            settings.WritePersistentSettings(); 
        }

        public SD_FenceProperties Properties { get; set; }


        protected override System.Drawing.Size MinimumSize => new System.Drawing.Size(50, 20);
        protected override System.Windows.Forms.Padding SizingBorders => new System.Windows.Forms.Padding(6);

        protected override void Render(GH_Canvas canvas, System.Drawing.Graphics graphics, GH_CanvasChannel channel)
        {
            GH_Document ghDocument = this.Owner.OnPingDocument();
            if (ghDocument != null)
            {
                // Set pen properties
                System.Drawing.Color color = this.Properties.Colour;
                System.Drawing.Brush brush = new System.Drawing.SolidBrush(color);
                System.Drawing.Pen pen = new System.Drawing.Pen(brush, this.Properties.Width);

                // Set linetype
                if (this.Properties.Pattern == "Continuous")
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                }
                else if (this.Properties.Pattern == "Dashed")
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                }
                else if (this.Properties.Pattern == "Dotted")
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                }
                else if (this.Properties.Pattern.Contains("Custom"))
                {
                    // Extract the numbers after "Custom: "
                    var patternData = this.Properties.Pattern.Replace("Custom: ", "");
                    var parts = patternData.Split(',').Select(p => float.Parse(p.Trim())).ToArray();

                    // Use the extracted numbers for the DashPattern
                    pen.DashPattern = parts;
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                }
                else
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                }

                // Set other properties
                pen.DashCap = System.Drawing.Drawing2D.DashCap.Round;
                pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

                // Draw rectangle in yellow when selected
                if (this.Selected)
                {
                    // Draw a selection rectangle around the object
                    Pen penSelected = (Pen) pen.Clone();
                    penSelected.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid; 
                    penSelected.Color = System.Drawing.Color.Yellow;
                    penSelected.Width = penSelected.Width + 3;
                    graphics.DrawRectangle(penSelected, this.Pivot.X, this.Pivot.Y, this.Bounds.Width, this.Bounds.Height);
                    penSelected.Dispose(); // disposing of this pen causes an error "System.ArgumentException: 'Parameter is not valid.'", no idea why
                }

                // Draw rectangle
                graphics.DrawRectangle(pen, this.Pivot.X, this.Pivot.Y, this.Bounds.Width, this.Bounds.Height);
                pen.Dispose();

                // Call the method to find where to place the text and how to align it
                var (textLocation, format) = CalculateTextLocationAndFormat();

                // Display name)
                System.Drawing.Font font = new System.Drawing.Font("Microsoft Sans Serif Regular",this.Properties.NameSize); // Adjust font and size
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias; // Enable anti-aliasing for smoother text rendering
                graphics.DrawString(this.Owner.NickName, font, brush, textLocation, format);
            }

            //base.Render(canvas, graphics, channel); //this controls whether the base component is shown or not. 
        }

        public override bool IsPickRegion(System.Drawing.PointF point) // this function defines the pick region when clicking
        {
            // Create a copy of the bounds
            System.Drawing.RectangleF outerBounds = this.Bounds;

            float width = this.Properties.Width / 2; // define half the lineweight to inflate by this amount

            // Inflate the bounds by 5 units in all directions
            outerBounds.Inflate(width, width);

            // Create a smaller rectangle that represents the inner bounds
            System.Drawing.RectangleF innerBounds = this.Bounds;

            // Inflate the bounds by 5 units in all directions
            innerBounds.Inflate(-width, -width);

            // Check if the point is within the expanded bounds but not within the original bounds
            if (outerBounds.Contains(point) && !innerBounds.Contains(point))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override bool IsPickRegion(System.Drawing.RectangleF box, GH_PickBox method) // this function defines the pick region for a selection box
        {
            // Create a copy of the bounds
            System.Drawing.RectangleF outerBounds = this.Bounds;

            // Check if the point is within the expanded bounds but not within the original bounds
            if (outerBounds.IntersectsWith(box) && !outerBounds.Contains(box))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private (PointF location, StringFormat format) CalculateTextLocationAndFormat()
        {
            // Retrieve the bounds of the rectangle
            var rect = this.Bounds;

            // Initialize the location variables
            float locationX = 0;
            float locationY = 0;

            // Initialize the StringFormat object
            StringFormat format = new StringFormat();

            // Adjust horizontal alignment
            switch (this.Properties.NameHorizontal)
            {
                case "Left":
                    format.Alignment = StringAlignment.Near;
                    if (this.Properties.NamePlacement == "Inside")
                    {
                        locationX = rect.Left + (this.Properties.Width / 2);
                    }
                    else // Outside
                    {
                        locationX = rect.Left - (this.Properties.Width / 2);
                    }
                    break;
                case "Centre":
                    format.Alignment = StringAlignment.Center;
                    locationX = rect.Left + rect.Width / 2;
                    break;
                case "Right":
                    format.Alignment = StringAlignment.Far;
                    if (this.Properties.NamePlacement == "Inside")
                    {
                        locationX = rect.Right - (this.Properties.Width / 2);
                    }
                    else // Outside
                    {
                        locationX = rect.Right + (this.Properties.Width / 2);
                    }
                    break;
            }

            // Adjust vertical alignment and placement
            switch (this.Properties.NameVertical)
            {
                case "Top":
                    if (this.Properties.NamePlacement == "Inside")
                    {
                        format.LineAlignment = StringAlignment.Near;
                        locationY = rect.Top + (this.Properties.Width / 2);
                    }
                    else // Outside
                    {
                        format.LineAlignment = StringAlignment.Far;
                        locationY = rect.Top - (this.Properties.Width / 2);
                    }
                    break;
                case "Bottom":
                    if (this.Properties.NamePlacement == "Inside")
                    {
                        format.LineAlignment = StringAlignment.Far;
                        locationY = rect.Bottom - (this.Properties.Width / 2);
                    }
                    else // Outside
                    {
                        format.LineAlignment = StringAlignment.Near;
                        locationY = rect.Bottom + (this.Properties.Width / 2);
                    }
                    break;
            }

            return (new PointF(locationX, locationY), format);
        }
    }
}
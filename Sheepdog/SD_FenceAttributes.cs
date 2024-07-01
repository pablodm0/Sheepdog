using Grasshopper.GUI.Canvas;
//using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel.Special;
//using GH_IO.Serialization;
using System;
using System.Runtime.CompilerServices;

namespace Sheepdog
{
    public class SD_FenceAttributes : SD_ResizableAttributes<SD_Fence>
    {
        //private Grasshopper.Kernel.Special.GH_Markup mark = new Grasshopper.Kernel.Special.GH_Markup();
        private GH_Document ghDocument;
        //public DisplayExpiredEventHandler DisplayExpiredHandler;
        public SD_FenceAttributes(SD_Fence owner) : base(owner)
        {
            // Set value to ghDocument
            ghDocument = this.Owner.OnPingDocument();

            // Adjust the Bounds property to reflect the DefaultSize. Assuming the Pivot is the starting point for drawing
            this.Bounds = new RectangleF(this.Pivot.X, this.Pivot.Y, this.DefaultSize.Width, this.DefaultSize.Height);

            // Initialize the properties and set default values
            SD_FenceProperties fenceProperties = new SD_FenceProperties();
            fenceProperties.SetDefault();
            this.Properties = fenceProperties;

            GH_SettingsServer settings = new GH_SettingsServer("Sheepdog");
            settings.SetValue("Width", this.Owner.WidthMedium);
            settings.SetValue("Colour", Color.Black);
            settings.SetValue("Linetype", "Continuous");
            settings.SetValue("Pattern", "[0,0,0,0]");
            settings.SetValue("NameSize", 12);
            settings.SetValue("NameVertical", "Top"); //NameVerticalStates.Top;
            settings.SetValue("NameHorizontal", "Left"); //NameHorizontalStates.Left;
            settings.SetValue("NamePlacement", "Inside"); //NamePlacementStates.Outside;
            settings.WritePersistentSettings();
        }

        public SD_FenceProperties Properties { get; set; }

        protected System.Drawing.Size DefaultSize => new System.Drawing.Size(300, 300);

        protected override System.Drawing.Size MinimumSize => new System.Drawing.Size(150, 150);
        protected override System.Windows.Forms.Padding SizingBorders => new System.Windows.Forms.Padding(6);

        //protected RectangleF NameBBox { get; set; }

        protected override void Render(GH_Canvas canvas, System.Drawing.Graphics graphics, GH_CanvasChannel channel)
        {
            GH_Document ghDocument = this.Owner.OnPingDocument();
            float zoomLevel = canvas.Viewport.Zoom;
            if (ghDocument != null)
            {
                // PREPARE DRAWING TOOLS

                // Set pen properties
                System.Drawing.Color color = this.Properties.Colour;
                System.Drawing.Brush brush = new System.Drawing.SolidBrush(color);
                System.Drawing.Pen pen = new System.Drawing.Pen(brush, this.Properties.Width);

                // Set other properties
                pen.DashCap = System.Drawing.Drawing2D.DashCap.Round;
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

                // DRAW NAME

                // Call the method to find where to place the text and how to align it
                var (textLocation, format) = CalculateTextLocationAndFormat();

                // Display name
                System.Drawing.Font font = new System.Drawing.Font("Microsoft Sans Serif Regular", this.Properties.NameSize); // Adjust font and size
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias; // Enable anti-aliasing for smoother text rendering
                graphics.DrawString(this.Owner.NickName, font, brush, textLocation, format);
                //System.Drawing.Size size = new System.Drawing.Size(5000, 5000);
                //SizeF textSize = graphics.MeasureString(this.Owner.NickName, font, size, format, out int charactersFitted, out int linesFilled);
                //NameBBox = new RectangleF(textLocation, textSize);

                // DRAW SELECTED 

                if (this.Selected)
                {
                    // Draw a selection rectangle around the object
                    Pen penSelected = (Pen)pen.Clone();
                    penSelected.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                    penSelected.Color = System.Drawing.Color.Yellow;
                    penSelected.Width = penSelected.Width + 3;
                    graphics.DrawRectangle(penSelected, this.Pivot.X, this.Pivot.Y, this.Bounds.Width, this.Bounds.Height);
                    penSelected.Dispose();
                    //graphics.FillRectangle(System.Drawing.Brushes.Yellow, NameBBox);
                }

                // DRAW RECTANGLE

                DrawNiceRectangle(graphics, pen, this.Pivot.X, this.Pivot.Y, this.Bounds.Width, this.Bounds.Height);
                pen.Dispose();

                // DRAW HANDLES

                // Draw resize handles when selected
                if (this.Selected)
                {
                    // Draw circles in the corners
                    float radius = 15 / zoomLevel;
                    float centerX = this.Pivot.X - radius;
                    float centerY = this.Pivot.Y - radius;
                    graphics.FillEllipse(System.Drawing.Brushes.White, centerX, centerY, radius * 2, radius * 2); // Top-left corner
                    graphics.FillEllipse(System.Drawing.Brushes.White, centerX, centerY + this.Bounds.Height, radius * 2, radius * 2); // Bottom-left corner
                    graphics.FillEllipse(System.Drawing.Brushes.White, centerX + this.Bounds.Width, centerY, radius * 2, radius * 2); // Top-right corner
                    graphics.FillEllipse(System.Drawing.Brushes.White, centerX + this.Bounds.Width, centerY + this.Bounds.Height, radius * 2, radius * 2); // Bottom-right corner

                    // Draw circle in the middle of the edges
                    float middleX = this.Pivot.X + this.Bounds.Width / 2;
                    float middleY = this.Pivot.Y + this.Bounds.Height / 2;
                    graphics.FillEllipse(System.Drawing.Brushes.White, middleX - radius, this.Pivot.Y - radius, radius * 2, radius * 2); // Top edge
                    graphics.FillEllipse(System.Drawing.Brushes.White, middleX - radius, this.Pivot.Y + this.Bounds.Height - radius, radius * 2, radius * 2); // Bottom edge
                    graphics.FillEllipse(System.Drawing.Brushes.White, this.Pivot.X - radius, middleY - radius, radius * 2, radius * 2); // Left edge
                    graphics.FillEllipse(System.Drawing.Brushes.White, this.Pivot.X + this.Bounds.Width - radius, middleY - radius, radius * 2, radius * 2); // Right edge
                }
            }
            //base.Render(canvas, graphics, channel); // this controls whether the base component is shown or not. 
        }
        private void DrawNiceRectangle(Graphics graphics, Pen pen, float x, float y, float width, float height)
        {
            if (this.Properties.Linetype == "Continuous")
            {
                graphics.DrawRectangle(pen, x, y, width, height);
                return;
            }

            else
            {
                // Extract the numbers after "Custom: "
                var patternInput = this.Properties.Pattern.Replace("[", "").Replace("]", ""); // remove the []
                var patternData = patternInput.Split(',').Select(p => float.Parse(p.Trim())).ToArray();

                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;

                // Use the adjusted pattern for X for the DashPattern
                var adjustedPatternX = AdjustPattern(patternData, width + pen.Width, pen.Width);
                pen.DashPattern = adjustedPatternX;

                // Draw the four lines of the rectangle
                //   Horizontal lines
                graphics.DrawLine(pen, x, y, x + width + pen.Width, y); // Top line
                graphics.DrawLine(pen, x, y + height, x + width + pen.Width, y + height); // Bottom line

                // Use the adjusted pattern for Y for the DashPattern
                var adjustedPatternY = AdjustPattern(patternData, height + pen.Width, pen.Width);
                pen.DashPattern = adjustedPatternY;

                //   Vertical lines
                graphics.DrawLine(pen, x, y, x, y + height + pen.Width); // Left line
                graphics.DrawLine(pen, x + width, y, x + width, y + height + pen.Width); // Right line
            }
        }

        public static float[] AdjustPattern(float[] dashPattern, float lineLength, float penWidth)
        {
            float numCycles = (lineLength - dashPattern[0]) / (dashPattern.Sum()); 
            int numCyclesInt = (int)Math.Floor(numCycles);
            /*/ to adjust both dash and space length
            float ratio = (lineLength) / (dashPattern.Sum() * numCyclesInt + dashPattern[0]);
            float[] adjustedPattern = dashPattern.Select(x => x * ratio / penWidth).ToArray();*/

            // to adjust space length only, not dash length
            float ratio = (lineLength - dashPattern[0] * (1 + numCyclesInt) - numCyclesInt * dashPattern[2]) / (numCyclesInt * (dashPattern[1] + dashPattern[3]));
            float[] adjustedPattern = dashPattern.Select(x => x / penWidth).ToArray();
            adjustedPattern[1] = adjustedPattern[1] * ratio;
            adjustedPattern[3] = adjustedPattern[3] * ratio;

            return adjustedPattern;
        }

        public override bool IsPickRegion(PointF point) // this function defines the pick region when clicking
        {

            float width = (this.Properties.Width / 2) + 20; // define half the lineweight to inflate by this amount

            // Create a copy of the bounds
            RectangleF outerBounds = this.Bounds;

            // Inflate the bounds by 5 units in all directions
            outerBounds.Inflate(width, width);

            // Create a smaller rectangle that represents the inner bounds
            RectangleF innerBounds = this.Bounds;

            // Shrink the bounds in all directions
            innerBounds.Inflate(-width, -width);

            // Check if the point is within the expanded bounds but not within the original bounds
            if (outerBounds.Contains(point) && !innerBounds.Contains(point)) //|| NameBBox.Contains(point))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override bool IsPickRegion(RectangleF box, GH_PickBox method) // this function defines the pick region for a selection box
        {
            // Create a copy of the bounds
            RectangleF outerBounds = this.Bounds;

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
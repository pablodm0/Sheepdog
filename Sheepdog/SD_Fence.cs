using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Base;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;


namespace Sheepdog
{
    public class SD_Fence : GH_Component
    {
        public SD_Fence() : base("Fence", "", "A fence to help you organise your canvas", "Params", "Sheepdog")
        {
            GH_Document ghDocument = this.OnPingDocument();
        }
        public override Guid ComponentGuid
        {
            // Don't copy this GUID, make a new one
            get { return new Guid("9A389611-1984-42AA-ADB9-4D08C1080ED5"); }
        }
        public override void CreateAttributes()
        {
            m_attributes = new SD_FenceAttributes(this);
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            //pManager.AddColourParameter("Colour", "C", "Border Colour", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            //throw new NotImplementedException();
        }

        public override TimeSpan ProcessorTime // To stop Profiler from showing in the object
        {
            get
            {
                return TimeSpan.Zero;
            }
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //throw new NotImplementedException();
        }

        public override bool AppendMenuItems(ToolStripDropDown menu)
        {
            this.Menu_AppendObjectName(menu);
            GH_DocumentObject.Menu_AppendSeparator((ToolStrip)menu);
            GH_DocumentObject.Menu_AppendColourPicker(GH_DocumentObject.Menu_AppendItem((ToolStrip)menu, "Colour").DropDown, ((SD_FenceAttributes)this.Attributes).Properties.Colour, new GH_DocumentObject.ColourEventHandler(this.ColourPicker_ColourChanged));
            ToolStripMenuItem toolStripMenuItemThickness = GH_DocumentObject.Menu_AppendItem((ToolStrip)menu, "Thickness");
            GH_DocumentObject.Menu_AppendItem((ToolStrip)toolStripMenuItemThickness.DropDown, "4", new EventHandler(this.Menu_T4Clicked));
            GH_DocumentObject.Menu_AppendItem((ToolStrip)toolStripMenuItemThickness.DropDown, "8", new EventHandler(this.Menu_T8Clicked));
            GH_DocumentObject.Menu_AppendItem((ToolStrip)toolStripMenuItemThickness.DropDown, "12", new EventHandler(this.Menu_T12Clicked));

            // For line type setting
            ToolStripMenuItem toolStripMenuItemLinetype = GH_DocumentObject.Menu_AppendItem((ToolStrip)menu, "Linetype");
            GH_DocumentObject.Menu_AppendItem((ToolStrip)toolStripMenuItemLinetype.DropDown, "Continuous", new EventHandler(this.Menu_LineContinuousClicked));
            GH_DocumentObject.Menu_AppendItem((ToolStrip)toolStripMenuItemLinetype.DropDown, "Dashed", new EventHandler(this.Menu_LineDashedClicked));
            GH_DocumentObject.Menu_AppendItem((ToolStrip)toolStripMenuItemLinetype.DropDown, "Dotted", new EventHandler(this.Menu_LineDottedClicked));
            // Add a new menu item for "Custom" option under "Linetype"
            ToolStripMenuItem toolStripMenuItemCustom = GH_DocumentObject.Menu_AppendItem((ToolStrip)toolStripMenuItemLinetype.DropDown, "Custom");
            // Create variable to hold current custom pattern to show it in the custom linetype text box 
            string currentCustom = "";
            if (((SD_FenceAttributes)this.Attributes).Properties.Pattern.Contains("Custom"))
            {
                currentCustom = ((SD_FenceAttributes)this.Attributes).Properties.Pattern.Replace("Custom: ", "");
            }
            // Add a text input item to the "Custom" option's dropdown for custom linetype input
            GH_DocumentObject.Menu_AppendTextItem(toolStripMenuItemCustom.DropDown, currentCustom, new GH_MenuTextBox.KeyDownEventHandler(this.Menu_LineCustomKeyDown), null, true, 200, true);
            

            GH_DocumentObject.Menu_AppendSeparator((ToolStrip)menu);

            ToolStripMenuItem toolStripMenuItemNameSize = GH_DocumentObject.Menu_AppendItem((ToolStrip)menu, "Name Size");
            GH_DocumentObject.Menu_AppendTextItem(toolStripMenuItemNameSize.DropDown, (((SD_FenceAttributes)this.Attributes).Properties.NameSize).ToString(), new GH_MenuTextBox.KeyDownEventHandler(this.Menu_NameSizeKeyDown), null, true, 200, true);

            // For name position setting
            ToolStripMenuItem toolStripMenuNameVertical = GH_DocumentObject.Menu_AppendItem((ToolStrip)menu, "Name Vertical Position");
            GH_DocumentObject.Menu_AppendItem((ToolStrip)toolStripMenuNameVertical.DropDown, "Top", new EventHandler(this.Menu_NameVerticalTopClicked));
            GH_DocumentObject.Menu_AppendItem((ToolStrip)toolStripMenuNameVertical.DropDown, "Bottom", new EventHandler(this.Menu_NameVerticalBottomClicked));
            ToolStripMenuItem toolStripMenuNameHorizontal = GH_DocumentObject.Menu_AppendItem((ToolStrip)menu, "Name Horizontal Position");
            GH_DocumentObject.Menu_AppendItem((ToolStrip)toolStripMenuNameHorizontal.DropDown, "Left", new EventHandler(this.Menu_NameHorizontalLeftClicked));
            GH_DocumentObject.Menu_AppendItem((ToolStrip)toolStripMenuNameHorizontal.DropDown, "Centre", new EventHandler(this.Menu_NameHorizontalCentreClicked));
            GH_DocumentObject.Menu_AppendItem((ToolStrip)toolStripMenuNameHorizontal.DropDown, "Right", new EventHandler(this.Menu_NameHorizontalRightClicked));
            ToolStripMenuItem toolStripMenuNamePlacement = GH_DocumentObject.Menu_AppendItem((ToolStrip)menu, "Name Placement");
            GH_DocumentObject.Menu_AppendItem((ToolStrip)toolStripMenuNamePlacement.DropDown, "Inside", new EventHandler(this.Menu_NamePlacementInsideClicked));
            GH_DocumentObject.Menu_AppendItem((ToolStrip)toolStripMenuNamePlacement.DropDown, "Outside", new EventHandler(this.Menu_NamePlacementOutsideClicked));

            // For default settings
            GH_DocumentObject.Menu_AppendSeparator((ToolStrip)menu);
            ToolStripMenuItem toolStripMenuItemMakeDefault = GH_DocumentObject.Menu_AppendItem((ToolStrip)menu, "Set as Default Settings", new EventHandler(this.Menu_MakeDefaultClicked));
            ToolStripMenuItem toolStripMenuItemApplyDefault = GH_DocumentObject.Menu_AppendItem((ToolStrip)menu, "Apply Default Settings", new EventHandler(this.Menu_ApplyDefaultClicked));

            return true;
        }

        private void ColourPicker_ColourChanged(GH_ColourPicker sender, GH_ColourPickerEventArgs e)
        {
            var tempProperties = ((SD_FenceAttributes)this.Attributes).Properties;
            tempProperties.Colour = e.Colour;
            ((SD_FenceAttributes)this.Attributes).Properties = tempProperties;

            Instances.RedrawCanvas();
        }
        private void Menu_T4Clicked(object sender, EventArgs e)
        {
            var tempProperties = ((SD_FenceAttributes)this.Attributes).Properties;
            tempProperties.Width = 4;
            ((SD_FenceAttributes)this.Attributes).Properties = tempProperties;

            Instances.RedrawCanvas();
        }
        private void Menu_T8Clicked(object sender, EventArgs e)
        {
            var tempProperties = ((SD_FenceAttributes)this.Attributes).Properties;
            tempProperties.Width = 8;
            ((SD_FenceAttributes)this.Attributes).Properties = tempProperties;

            Instances.RedrawCanvas();
        }
        private void Menu_T12Clicked(object sender, EventArgs e)
        {
            var tempProperties = ((SD_FenceAttributes)this.Attributes).Properties;
            tempProperties.Width = 12;
            ((SD_FenceAttributes)this.Attributes).Properties = tempProperties;

            Instances.RedrawCanvas();
        }
        private void Menu_LineContinuousClicked(object sender, EventArgs e)
        {
            var tempProperties = ((SD_FenceAttributes)this.Attributes).Properties;
            tempProperties.Pattern = "Continuous";
            ((SD_FenceAttributes)this.Attributes).Properties = tempProperties;

            Instances.RedrawCanvas();
        }
        private void Menu_LineDashedClicked(object sender, EventArgs e)
        {
            var tempProperties = ((SD_FenceAttributes)this.Attributes).Properties;
            tempProperties.Pattern = "Dashed";
            ((SD_FenceAttributes)this.Attributes).Properties = tempProperties;

            Instances.RedrawCanvas();
        }
        private void Menu_LineDottedClicked(object sender, EventArgs e)
        {
            var tempProperties = ((SD_FenceAttributes)this.Attributes).Properties;
            tempProperties.Pattern = "Dotted";
            ((SD_FenceAttributes)this.Attributes).Properties = tempProperties;

            Instances.RedrawCanvas();
        }
        private void Menu_LineCustomKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                GH_MenuTextBox textBox = sender as GH_MenuTextBox;
                if (textBox != null)
                {
                    string input = textBox.Text.Replace(" ", ""); // Remove all spaces

                    // Regular expression to match 1, 2, or 4 numbers (integers or decimals) separated by commas
                    string regexPattern = @"^(\d+(\.\d+)?|\d+(\.\d+)?(,\d+(\.\d+)?){1}|\d+(\.\d+)?(,\d+(\.\d+)?){3})$";
                    System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(regexPattern);

                    if (regex.IsMatch(input))
                    {
                        var parts = input.Split(',').Select(part => decimal.Parse(part)).ToArray();
                        // Check if any of the numbers are 0
                        if (parts.Any(part => part == 0))
                        {
                            MessageBox.Show("Invalid input. None of the numbers can be 0.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return; // Exit the method early
                        }
                        var tempProperties = ((SD_FenceAttributes)this.Attributes).Properties;

                        string pattern;
                        if (parts.Length == 1)
                        {
                            // Format as "Custom: A,A,A,A" if input is "A"
                            pattern = $"Custom: {parts[0]},{parts[0]},{parts[0]},{parts[0]}";
                        }
                        else if (parts.Length == 2)
                        {
                            // Format as "Custom: A,B,A,B" if input is "A,B"
                            pattern = $"Custom: {parts[0]},{parts[1]},{parts[0]},{parts[1]}";
                        }
                        else
                        {
                            // Format as "Custom: A,B,C,D" if input is "A,B,C,D"
                            pattern = $"Custom: {parts[0]},{parts[1]},{parts[2]},{parts[3]}";
                        }
                        tempProperties.Pattern = pattern;
                        ((SD_FenceAttributes)this.Attributes).Properties = tempProperties;

                        Instances.RedrawCanvas();
                    }
                    else
                    {
                        MessageBox.Show("Invalid input. Please enter in one of these formats: \n\n1\n1,1\n1,1,1,1", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void Menu_NameSizeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                GH_MenuTextBox textBox = sender as GH_MenuTextBox;
                if (textBox != null)
                {
                    string input = textBox.Text.Replace(" ", ""); // Remove all spaces

                    // Regular expression to match 1, 2, or 4 numbers (integers or decimals) separated by commas
                    string regexPattern = @"^\d+(\.\d+)?$";
                    System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(regexPattern);

                    if (regex.IsMatch(input))
                    {
                        Single size = Convert.ToSingle(input);
                        if (size > 0 && size <= 300)
                        {
                            var tempProperties = ((SD_FenceAttributes)this.Attributes).Properties;

                            tempProperties.NameSize = size;
                            ((SD_FenceAttributes)this.Attributes).Properties = tempProperties;

                            Instances.RedrawCanvas();
                        }
                        else
                        {
                            MessageBox.Show("Invalid input. Please enter a number between 0 and 300", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid input. Please enter a number between 0 and 300", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void Menu_NameVerticalTopClicked(object sender, EventArgs e)
        {
            var tempProperties = ((SD_FenceAttributes)this.Attributes).Properties;
            tempProperties.NameVertical = "Top";
            ((SD_FenceAttributes)this.Attributes).Properties = tempProperties;

            Instances.RedrawCanvas();
        }
        private void Menu_NameVerticalBottomClicked(object sender, EventArgs e)
        {
            var tempProperties = ((SD_FenceAttributes)this.Attributes).Properties;
            tempProperties.NameVertical = "Bottom";
            ((SD_FenceAttributes)this.Attributes).Properties = tempProperties;

            Instances.RedrawCanvas();
        }
        private void Menu_NameHorizontalLeftClicked(object sender, EventArgs e)
        {
            var tempProperties = ((SD_FenceAttributes)this.Attributes).Properties;
            tempProperties.NameHorizontal = "Left";
            ((SD_FenceAttributes)this.Attributes).Properties = tempProperties;

            Instances.RedrawCanvas();
        }
        private void Menu_NameHorizontalCentreClicked(object sender, EventArgs e)
        {
            var tempProperties = ((SD_FenceAttributes)this.Attributes).Properties;
            tempProperties.NameHorizontal = "Centre";
            ((SD_FenceAttributes)this.Attributes).Properties = tempProperties;

            Instances.RedrawCanvas();
        }
        private void Menu_NameHorizontalRightClicked(object sender, EventArgs e)
        {
            var tempProperties = ((SD_FenceAttributes)this.Attributes).Properties;
            tempProperties.NameHorizontal = "Right";
            ((SD_FenceAttributes)this.Attributes).Properties = tempProperties;

            Instances.RedrawCanvas();
        }
        private void Menu_NamePlacementInsideClicked(object sender, EventArgs e)
        {
            var tempProperties = ((SD_FenceAttributes)this.Attributes).Properties;
            tempProperties.NamePlacement = "Inside";
            ((SD_FenceAttributes)this.Attributes).Properties = tempProperties;

            Instances.RedrawCanvas();
        }
        private void Menu_NamePlacementOutsideClicked(object sender, EventArgs e)
        {
            var tempProperties = ((SD_FenceAttributes)this.Attributes).Properties;
            tempProperties.NamePlacement = "Outside";
            ((SD_FenceAttributes)this.Attributes).Properties = tempProperties;

            Instances.RedrawCanvas();
        }
        private void Menu_MakeDefaultClicked(object sender, EventArgs e)
        {
            GH_SettingsServer settings = new GH_SettingsServer("Sheepdog");
            settings.SetValue("Width", ((SD_FenceAttributes)this.Attributes).Properties.Width);
            settings.SetValue("Colour", ((SD_FenceAttributes)this.Attributes).Properties.Colour);
            settings.SetValue("Pattern", ((SD_FenceAttributes)this.Attributes).Properties.Pattern);
            settings.SetValue("NameSize", ((SD_FenceAttributes)this.Attributes).Properties.NameSize);
            settings.SetValue("NameVertical", ((SD_FenceAttributes)this.Attributes).Properties.NameVertical);
            settings.SetValue("NameHorizontal", ((SD_FenceAttributes)this.Attributes).Properties.NameHorizontal);
            settings.SetValue("NamePlacement", ((SD_FenceAttributes)this.Attributes).Properties.NamePlacement);
            settings.WritePersistentSettings();
        }
        private void Menu_ApplyDefaultClicked(object sender, EventArgs e)
        {
            GH_SettingsServer settings = new GH_SettingsServer("Sheepdog");
            var tempProperties = ((SD_FenceAttributes)this.Attributes).Properties;
            tempProperties.Width = settings.GetValue("Width", 3);
            tempProperties.Colour = settings.GetValue("Colour", Color.Black);
            tempProperties.Pattern = settings.GetValue("Pattern", "Continuous");
            tempProperties.NameSize = settings.GetValue("NameSize", 12);
            tempProperties.NameVertical = settings.GetValue("NameVertical", "Top"); 
            tempProperties.NameHorizontal = settings.GetValue("NameHorizontal", "Left"); 
            tempProperties.NamePlacement = settings.GetValue("NamePlacement", "Inside"); 

            ((SD_FenceAttributes)this.Attributes).Properties = tempProperties;
            Instances.RedrawCanvas();
        }
    }
}

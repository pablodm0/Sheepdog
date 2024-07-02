using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Sheepdog;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Padding = System.Windows.Forms.Padding;
using PointF = System.Drawing.PointF;
using RectangleF = System.Drawing.RectangleF;
using Size = System.Drawing.Size;
using SizeF = System.Drawing.SizeF;
using System.Drawing;


namespace Grasshopper.Kernel.Attributes
{
  /// <summary>These Attributes provide basic resizing logic.</summary>
  public abstract class SD_ResizableAttributes<T> : GH_Attributes<T> where T : SD_Fence//IGH_DocumentObject
  {
    private GH_ResizeBorder m_resize_data;
    private bool m_resize_undo;

    protected SD_ResizableAttributes(T owner)
      : base(owner)
    {
      this.m_resize_undo = false;
    }

    protected abstract Size MinimumSize { get; }

    protected virtual Size MaximumSize => new Size(int.MaxValue, int.MaxValue);

    protected abstract Padding SizingBorders { get; }


    public float resizeHandleRadius { get; set; }

    public float zoomLevel { get; set; }

    public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
      GH_ObjectResponse mouseDown;
      if (e.Button == MouseButtons.Left)
      {
        List<GH_Border> borders1 = GH_Border.CreateBorders(this.Bounds, this.SizingBorders);
        try
        {
          foreach (GH_Border borders2 in borders1)
          {

            // Define a corner region as a circle with radius X units centered on the corner
            float radius = this.resizeHandleRadius;
            

            // Create areas for corners
            bool isTopLeftCorner = Distance(e.CanvasLocation, new PointF(borders2.Region.Left, borders2.Region.Top)) <= radius;
            bool isTopRightCorner = Distance(e.CanvasLocation, new PointF(borders2.Region.Right, borders2.Region.Top)) <= radius;
            bool isBottomLeftCorner = Distance(e.CanvasLocation, new PointF(borders2.Region.Left, borders2.Region.Bottom)) <= radius;
            bool isBottomRightCorner = Distance(e.CanvasLocation, new PointF(borders2.Region.Right, borders2.Region.Bottom)) <= radius;
            // Create areas for edge centres
            bool isTopMiddle = Distance(e.CanvasLocation, new PointF(borders2.Region.Left + borders2.Region.Width / 2, borders2.Region.Top)) <= radius;
            bool isRightMiddle = Distance(e.CanvasLocation, new PointF(borders2.Region.Right, borders2.Region.Top + borders2.Region.Height / 2)) <= radius;
            bool isBottomMiddle = Distance(e.CanvasLocation, new PointF(borders2.Region.Left + borders2.Region.Width / 2, borders2.Region.Bottom)) <= radius;
            bool isLeftMiddle = Distance(e.CanvasLocation, new PointF(borders2.Region.Left, borders2.Region.Top + borders2.Region.Height / 2)) <= radius;


            if (isTopLeftCorner || isTopRightCorner || isBottomLeftCorner || isBottomRightCorner || isTopMiddle || isRightMiddle || isBottomMiddle || isLeftMiddle)
            {
                this.m_resize_data = new GH_ResizeBorder(borders2);
                this.m_resize_data.Setup((IGH_Attributes) this, e.CanvasLocation, (SizeF) this.MinimumSize, (SizeF) this.MaximumSize);
                mouseDown = GH_ObjectResponse.Capture;
                goto label_8;
            }
          }
        }
        finally
        {
          //List<GH_Border>.Enumerator enumerator;
          //enumerator.Dispose();
        }
      }
      mouseDown = base.RespondToMouseDown(sender, e);
label_8:
      return mouseDown;
    }

    public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
      GH_ObjectResponse mouseMove;
      if (e.Button == MouseButtons.None)
      {
        List<GH_Border> borders = GH_Border.CreateBorders(this.Bounds, this.SizingBorders);

        try
        {
            foreach (GH_Border ghBorder in borders)
            {
                // Define a corner region as a circle with radius X units centered on the corner
                float radius = this.resizeHandleRadius;

                // Create areas for corners
                bool isTopLeftCorner = Distance(e.CanvasLocation, new PointF(ghBorder.Region.Left, ghBorder.Region.Top)) <= radius;
                bool isTopRightCorner = Distance(e.CanvasLocation, new PointF(ghBorder.Region.Right, ghBorder.Region.Top)) <= radius;
                bool isBottomLeftCorner = Distance(e.CanvasLocation, new PointF(ghBorder.Region.Left, ghBorder.Region.Bottom)) <= radius;
                bool isBottomRightCorner = Distance(e.CanvasLocation, new PointF(ghBorder.Region.Right, ghBorder.Region.Bottom)) <= radius;
                // Create areas for edge centres
                bool isTopMiddle = Distance(e.CanvasLocation, new PointF(ghBorder.Region.Left + ghBorder.Region.Width / 2, ghBorder.Region.Top)) <= radius;
                bool isRightMiddle = Distance(e.CanvasLocation, new PointF(ghBorder.Region.Right, ghBorder.Region.Top + ghBorder.Region.Height / 2)) <= radius;
                bool isBottomMiddle = Distance(e.CanvasLocation, new PointF(ghBorder.Region.Left + ghBorder.Region.Width / 2, ghBorder.Region.Bottom)) <= radius;
                bool isLeftMiddle = Distance(e.CanvasLocation, new PointF(ghBorder.Region.Left, ghBorder.Region.Top + ghBorder.Region.Height / 2)) <= radius;


                if (isTopLeftCorner || isTopRightCorner || isBottomLeftCorner || isBottomRightCorner || isTopMiddle || isRightMiddle || isBottomMiddle || isLeftMiddle)
                {
                    sender.Cursor = ghBorder.Size_Cursor;
                    mouseMove = ghBorder.Topology != GH_BorderTopology.None ? GH_ObjectResponse.Handled : base.RespondToMouseMove(sender, e);
                    goto label_13;
                }
            }
        }

        finally
        {
          //List<GH_Border>.Enumerator enumerator;
          //enumerator.Dispose();
        }
      }
      if (e.Button == MouseButtons.Left && this.m_resize_data != null)
      {
        if (!this.m_resize_undo)
        {
          this.m_resize_undo = true;
          this.Owner.OnPingDocument()?.UndoUtil.RecordLayoutEvent("Resize " + this.Owner.Name, (IGH_DocumentObject) this.Owner);
        }
        RectangleF new_shape;
        PointF new_pivot;
        this.m_resize_data.Solve(e.CanvasLocation, out new_shape, out new_pivot);
        this.Pivot = new_pivot;
        this.Bounds = new_shape;
        this.ExpireLayout();
        sender.Refresh();
        mouseMove = GH_ObjectResponse.Handled;
      }
      else
        mouseMove = base.RespondToMouseMove(sender, e);
label_13:
      return mouseMove;
    }

    public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
      this.m_resize_undo = false;
      GH_Document ghDocument = this.Owner.OnPingDocument();
      GH_ObjectResponse mouseUp;
      if (this.m_resize_data != null)
      {
        this.m_resize_data = (GH_ResizeBorder)null;
        mouseUp = GH_ObjectResponse.Release;
        //this.Owner.OnRaiseResizedEvent();
      }
      else
      {
        mouseUp = base.RespondToMouseUp(sender, e);
      }
      ghDocument.NewSolution(false); //if set to true, this will recompute the whole canvas
      return mouseUp;
    }
    // Helper method to calculate distance between two points
    protected float Distance(PointF point1, PointF point2)
    {
        float dx = point1.X - point2.X;
        float dy = point1.Y - point2.Y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }
  }
}
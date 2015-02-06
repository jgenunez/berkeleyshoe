using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace MarketplaceManager
{
    //public class DataGridViewProgressColumn : DataGridViewImageColumn
    //{
    //    public DataGridViewProgressColumn()
    //    {
    //        CellTemplate = new DataGridViewProgressCell();
    //    }
    //}

    //public class DataGridViewProgressCell : DataGridViewImageCell
    //{
    //    private ProgressBar _progressBar;
    //    private Timer _animationStepTimer;
    //    private Timer _animationStopTimer;
    //    // Used to make custom cell consistent with a DataGridViewImageCell
    //    private static Image _emptyImage;
        
    //    static DataGridViewProgressCell()
    //    {
    //        _emptyImage = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
    //    }

    //    public DataGridViewProgressCell()
    //    {
    //        _progressBar = new ProgressBar() { Style = ProgressBarStyle.Marquee };

    //        //_text = String.Format("{0} of {1}", MessageSpecialValue.CurrentValue, MessageSpecialValue.Maximum);

    //        // repaint every 25 milliseconds while progress is active
    //        _animationStepTimer = new Timer { Interval = 25, Enabled = true };

    //        // stop repainting 1 second after progress becomes inactive
    //        _animationStopTimer = new Timer { Interval = 1000, Enabled = false };

    //        _animationStepTimer.Tick += (x, y) => { stopAnimation(); refresh(); };
    //        _animationStopTimer.Tick += (x, y) => { _animationStepTimer.Stop(); _animationStopTimer.Stop(); };
    //    }

    //    protected override void Paint(
    //        Graphics graphics, 
    //        Rectangle clipBounds, 
    //        Rectangle cellBounds, 
    //        int rowIndex, 
    //        DataGridViewElementStates elementState, 
    //        object value, object formattedValue, string errorText, 
    //        DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
    //    {
    //        //base.Paint(graphics, clipBounds, cellBounds,
    //        // rowIndex, elementState, value, formattedValue, errorText,
    //        // cellStyle, advancedBorderStyle, (paintParts & ~DataGridViewPaintParts.ContentForeground));

    //        Bitmap bitmap = new Bitmap(cellBounds.Width, cellBounds.Height);
    //        Rectangle bmpBounds = new Rectangle(0, 0, cellBounds.Width, cellBounds.Height);

    //        _progressBar.Size = cellBounds.Size;
    //        _progressBar.DrawToBitmap(bitmap, bmpBounds);

    //        graphics.DrawImage(bitmap, cellBounds);
    //    }

    //    // Method required to make the Progress Cell consistent with the default Image Cell. 
    //    // The default Image Cell assumes an Image as a value, although the value of the Progress Cell is an int.
    //    protected override object GetFormattedValue(object value,
    //                        int rowIndex, ref DataGridViewCellStyle cellStyle,
    //                        TypeConverter valueTypeConverter,
    //                        TypeConverter formattedValueTypeConverter,
    //                        DataGridViewDataErrorContexts context)
    //    {
    //        return _emptyImage;
    //    }

    //    private void refresh()
    //    {
    //        if (DataGridView != null) DataGridView.InvalidateCell(this);
    //    }

    //    private void startAnimation()
    //    {
    //        if (_progressBar.Style == ProgressBarStyle.Marquee ||
    //            (_progressBar.Value > _progressBar.Minimum && _progressBar.Value < _progressBar.Maximum))
    //            _animationStepTimer.Start();
    //    }

    //    private void stopAnimation()
    //    {
    //        if (_progressBar.Style != ProgressBarStyle.Marquee &&
    //            (_progressBar.Value == _progressBar.Minimum || _progressBar.Value == _progressBar.Maximum))
    //            _animationStopTimer.Start();
    //    }
    //}
}

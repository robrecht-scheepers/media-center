using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace MediaCenter.WPF.Controls
{
    public class SeekBar : ProgressBar
    {
        private Cursor _previousCursor = Cursors.Arrow;
        private bool _isDragging = false;

        public event EventHandler DragStarted;
        public event EventHandler DragCompleted;

        public SeekBar()
        {
            this.MouseDown += OnMouseDown;
            this.MouseEnter += OnMouseEnter;
            this.MouseMove += OnMouseMove;
            this.MouseLeave += OnMouseLeave;
            this.MouseUp += this_MouseUp;

            this.Minimum = 0;
            this.Maximum = 1;
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            _previousCursor = this.Cursor;
            this.Cursor = Cursors.Hand;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if(_isDragging)
                OnDragCompleted();
            this.Cursor = _previousCursor;
        }
        private void SetProgressBarValue(double mousePosition)
        {
            this.Value = Math.Round((Maximum - Minimum) * (mousePosition / this.ActualWidth),3);
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            double mousePosition = e.GetPosition(this).X;
            SetProgressBarValue(mousePosition);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if(!_isDragging)
                    OnDragStarted();

                double mousePosition = e.GetPosition(this).X;
                SetProgressBarValue(mousePosition);
            }
        }

        private void this_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
                OnDragCompleted();
        }

        protected virtual void OnDragStarted()
        {
            _isDragging = true;
            DragStarted?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDragCompleted()
        {
            _isDragging = false;
            DragCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace MediaCenter.Styles
{
    public class OpenCloseWindowBehavior : Behavior<UserControl>
    {
        private Window _windowInstance;

        public Type WindowType { get { return (Type)GetValue(WindowTypeProperty); } set { SetValue(WindowTypeProperty, value); } }
        public static readonly DependencyProperty WindowTypeProperty = DependencyProperty.Register("WindowType", typeof(Type), typeof(OpenCloseWindowBehavior), new PropertyMetadata(null));

        public object WindowDataContext { get { return (object)GetValue(WindowDataContextProperty); } set { SetValue(WindowDataContextProperty, value); } }
        public static readonly DependencyProperty WindowDataContextProperty = DependencyProperty.Register("WindowDataContext", typeof(object), typeof(OpenCloseWindowBehavior), new PropertyMetadata(null));

        public bool Trigger { get { return (bool)GetValue(TriggerProperty); } set { SetValue(TriggerProperty, value); } }
        public static readonly DependencyProperty TriggerProperty = DependencyProperty.Register("Trigger", typeof(bool), typeof(OpenCloseWindowBehavior), new PropertyMetadata(false, OnTriggerChanged));

        /// <summary>
        /// Opens or closes a window of type 'WindowType' and assigns the datacontext.
        /// </summary>
        private static void OnTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = (OpenCloseWindowBehavior)d;
            if ((bool)e.NewValue)
            {
                object instance = Activator.CreateInstance(me.WindowType);
                var window = instance as Window;
                if (window != null)
                {
                    window.Closing += (s, ev) =>
                    {
                        if (me.Trigger) // window closed directly by user
                        {
                            me._windowInstance = null; // prevents repeated Close call
                            me.Trigger = false; // set to false, so next time Open is set to true, OnOpenChanged is triggered again
                        }
                    };
                    window.DataContext = me.WindowDataContext;
                    window.Show();
                    me._windowInstance = window;
                }
                else
                {
                    throw new ArgumentException($"Type '{me.WindowType}' does not derive from System.Windows.Window.");
                }
            }
            else
            {
                me._windowInstance?.Close(); // closed by viewmodel
            }
        }

    }
}

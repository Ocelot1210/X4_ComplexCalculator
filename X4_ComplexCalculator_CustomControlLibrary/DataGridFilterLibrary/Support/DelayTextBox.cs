using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Support
{
    /// <summary>
    /// WPF port of windows forms version: http://www.codeproject.com/KB/miscctrl/CustomTextBox.aspx
    /// </summary>
    public class DelayTextBox : TextBox
    {
        #region private globals

        private readonly Timer DelayTimer;              // used for the delay
        private bool TimerElapsed = false;              // if true OnTextChanged is fired.
        private bool KeysPressed = false;               // makes event fire immediately if it wasn't a keypress
        private readonly int DELAY_TIME = 250;          //for now best empiric value

        public static readonly DependencyProperty DelayTimeProperty =
            DependencyProperty.Register("DelayTime", typeof(int), typeof(DelayTextBox));
        #endregion


        #region ctor
        public DelayTextBox() : base()
        {
            // Initialize Timer
            DelayTimer = new Timer(DELAY_TIME);
            DelayTimer.Elapsed += new ElapsedEventHandler(DelayTimer_Elapsed);

            previousTextChangedEventArgs = null;

            AddHandler(TextBox.PreviewKeyDownEvent, new KeyEventHandler(DelayTextBox_PreviewKeyDown));

            PreviousTextValue = String.Empty;
        }

        void DelayTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!DelayTimer.Enabled)
            {
                DelayTimer.Enabled = true;
            }
            else
            {
                DelayTimer.Enabled = false;
                DelayTimer.Enabled = true;
            }

            KeysPressed = true;
        }
        #endregion


        #region event handlers

        void DelayTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // stop timer.
            DelayTimer.Enabled = false;

            // set timer elapsed to true, so the OnTextChange knows to fire
            TimerElapsed = true;

            // use invoke to get back on the UI thread.
            Dispatcher.Invoke(new DelayOverHandler(DelayOver), null);
        }
        #endregion


        #region overrides

        private TextChangedEventArgs? previousTextChangedEventArgs;

        public string PreviousTextValue { get; private set; }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            // if the timer elapsed or text was changed by something besides a keystroke
            // fire base.OnTextChanged
            if (TimerElapsed || !KeysPressed)
            {
                TimerElapsed = false;
                KeysPressed = false;
                base.OnTextChanged(e);

                var be = GetBindingExpression(TextBox.TextProperty);
                if (be is not null && be.Status == BindingStatus.Active)
                {
                    be.UpdateSource();
                }

                PreviousTextValue = Text;
            }

            previousTextChangedEventArgs = e;
        }

        #endregion


        #region delegates
        public delegate void DelayOverHandler();
        #endregion


        #region private helpers
        private void DelayOver()
        {
            if (previousTextChangedEventArgs != null)
            {
                OnTextChanged(previousTextChangedEventArgs);
            }
        }
        #endregion
    }
}

//------------------------------------------------------------------------------
// <copyright file="KinectHoverButton.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Threading;

    using Microsoft.Kinect.Toolkit.Controls;

    /// <summary>
    /// A button that continually triggers a click when the mouse or hand pointer hovers over it
    /// </summary>
    internal class KinectHoverButton : KinectButtonBase
    {
        /// <summary>
        /// IsHandPointerOver dependency property for use in the control template triggers
        /// </summary>
        public static readonly DependencyProperty IsHandPointerOverProperty = DependencyProperty.Register(
            "IsHandPointerOver", typeof(bool), typeof(KinectHoverButton), new PropertyMetadata(false));

        // Trigger a click 60 times per second
        private const int ButtonRepeatIntervalMilliseconds = 1000 / 60;

        /// <summary>
        /// Boolean value to tell us if the control is being displayed in the Visual Studio designer
        /// </summary>
        private static readonly bool IsInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());

        /// <summary>
        /// Timer to handle triggering the click events
        /// </summary>
        private readonly DispatcherTimer repeatTimer;

        private HandPointer activeHandpointer;

        int[] positivePins = { 10, 9, 6, 5, 3 };
        int[] negativePins = { 15, 14, 12, 8, 2 };

        string[] valuesMax = { "255", "255", "255", "255", "255" };
        string[] valuesMedium = { "120", "120", "120", "120", "120" };
        string[] valuesMinium = { "0", "0", "0", "0", "0" };
        string[] valuesOFF = { "LOW", "LOW", "LOW", "LOW", "LOW" };

        public KinectHoverButton()
        {
            if (!IsInDesignMode)
            {
                this.InitializeKinectHoverButton();
                this.repeatTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ButtonRepeatIntervalMilliseconds) };
                this.repeatTimer.Tick += this.RepeatTimerTick;
            }
        }

        /// <summary>
        /// Boolean value that returns true if a mouse or hand pointer is over this button
        /// </summary>
        public bool IsHandPointerOver
        {
            get
            {
                return (bool)this.GetValue(IsHandPointerOverProperty);
            }

            set
            {
                this.SetValue(IsHandPointerOverProperty, value);
            }
        }

        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            this.IsHandPointerOver = true;
            this.repeatTimer.Start();
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            this.IsHandPointerOver = false;
            this.repeatTimer.Stop();
        }
        
        private void InitializeKinectHoverButton()
        {
            KinectRegion.AddHandPointerEnterHandler(this, this.OnHandPointerEnter);
            KinectRegion.AddHandPointerLeaveHandler(this, this.OnHandPointerLeave);
        }

        private void RepeatTimerTick(object sender, EventArgs e)
        {
            this.OnClick();
        }

        private void OnHandPointerEnter(object sender, HandPointerEventArgs e)
        {
            if (!e.HandPointer.IsPrimaryHandOfUser || !e.HandPointer.IsPrimaryUser)
            {
                return;
            }

            this.activeHandpointer = e.HandPointer;
            this.IsHandPointerOver = true;
            this.repeatTimer.Start();
            ((MainWindow)System.Windows.Application.Current.MainWindow).glove.ActivateMotor(positivePins, valuesMax);

        }

        private void OnHandPointerLeave(object sender, HandPointerEventArgs e)
        {
            if (this.activeHandpointer != e.HandPointer)
            {
                return;
            }

            this.activeHandpointer = null;
            this.IsHandPointerOver = false;
            this.repeatTimer.Stop();

            ((MainWindow)System.Windows.Application.Current.MainWindow).glove.ActivateMotor(positivePins, valuesMinium);
        }
    }
}

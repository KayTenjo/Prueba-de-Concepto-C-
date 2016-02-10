//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    using Microsoft.Kinect.Toolkit.Controls;
    using HapticsGlove;
    using System.Timers;
    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow
    {
        public static readonly DependencyProperty PageLeftEnabledProperty = DependencyProperty.Register(
            "PageLeftEnabled", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public static readonly DependencyProperty PageRightEnabledProperty = DependencyProperty.Register(
            "PageRightEnabled", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        private const double ScrollErrorMargin = 0.001;

        private const int PixelScrollByAmount = 20;

        private readonly KinectSensorChooser sensorChooser;

        public HapticsGlove glove = new HapticsGlove();
        int[] positivePins = { 10, 9, 6, 5, 3 };
        int[] negativePins = {15, 14, 12, 8, 2 };
 
        string[] valuesMax = { "255", "255", "255", "255", "255" };
        string[] valuesMedium = {"120", "120", "120", "120","120"};
        string[] valuesMinium = { "0","0", "0", "0", "0" };
        string[] valuesOFF = { "LOW", "LOW","LOW", "LOW", "LOW" };


        internal System.Timers.Timer Timer;






        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class. 
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();

            // initialize the sensor chooser and UI
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            // Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);

            // Clear out placeholder content
            this.wrapPanel.Children.Clear();

            // Add in display content
            for (var index = 0; index < 300; ++index)
            {
                var button = new KinectTileButton { Label = (index + 1).ToString(CultureInfo.CurrentCulture) };
                KinectRegion.AddHandPointerGripHandler(button, OnHandPointerGrip);
                KinectRegion.AddHandPointerGripReleaseHandler(button, OnHandPointerGripRelease);
                this.wrapPanel.Children.Add(button);
            }

            // Bind listner to scrollviwer scroll position change, and check scroll viewer position
            this.UpdatePagingButtonState();
            scrollViewer.ScrollChanged += (o, e) => this.UpdatePagingButtonState();
            //KinectRegion.AddHandPointerGripHandler(scrollViewer, OnHandPointerGrip);
            //KinectRegion.AddHandPointerGripReleaseHandler(scrollViewer, OnHandPointerGripRelease);

            //Initialize Glove 

            glove.OpenPort("COM3", 38400);

            glove.InitializeMotor(positivePins);
            glove.InitializeMotor(negativePins);
            glove.ActivateMotor(negativePins, valuesOFF);
            glove.ActivateMotor(negativePins, valuesOFF);




            /*Timer timer = new Timer();
            timer.Interval = 1000;
            mytimer.Tick += new System.EventHandler(timer1_Tick);
            mytimer.Enabled = true;


            mytimer.Tick -= new System.EventHandler(timer1_Tick); */

        }

        private void OnHandPointerGripRelease(object sender, HandPointerEventArgs e)
        {
          
            glove.ActivateMotor(positivePins, valuesMinium);
        }

        private void OnHandPointerGrip(object sender, HandPointerEventArgs e)
        {
            glove.ActivateMotor(positivePins, valuesMedium);
        }

        /// <summary>
        /// CLR Property Wrappers for PageLeftEnabledProperty
        /// </summary>
        public bool PageLeftEnabled
        {
            get
            {
                return (bool)GetValue(PageLeftEnabledProperty);
            }

            set
            {
                this.SetValue(PageLeftEnabledProperty, value);
            }
        }

        /// <summary>
        /// CLR Property Wrappers for PageRightEnabledProperty
        /// </summary>
        public bool PageRightEnabled
        {
            get
            {
                return (bool)GetValue(PageRightEnabledProperty);
            }

            set
            {
                this.SetValue(PageRightEnabledProperty, value);
            }
        }

        /// <summary>
        /// Called when the KinectSensorChooser gets a new sensor
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="args">event arguments</param>
        private static void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();

                    try
                    {
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.sensorChooser.Stop();
        }

        /// <summary>
        /// Handle a button click from the wrap panel.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void KinectTileButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (KinectTileButton)e.OriginalSource;
            var selectionDisplay = new SelectionDisplay(button.Label as string);
            this.kinectRegionGrid.Children.Add(selectionDisplay);
            e.Handled = true;
            glove.ActivateMotor(positivePins, valuesMax);
            Timer = new System.Timers.Timer() { Interval = 200 };
            Timer.Elapsed += DeactivateVibration;
            Timer.Enabled = true;

        }

        private void DeactivateVibration(object sender, System.Timers.ElapsedEventArgs e)
        {
            glove.ActivateMotor(positivePins, valuesMinium);
            Timer.Stop();
            Timer.Enabled = false;
            Timer.Elapsed -= DeactivateVibration;
            Timer = null;
        }


        /// <summary>
        /// Handle paging right (next button).
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void PageRightButtonClick(object sender, RoutedEventArgs e)
        {
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + PixelScrollByAmount);
        }

        /// <summary>
        /// Handle paging left (previous button).
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void PageLeftButtonClick(object sender, RoutedEventArgs e)
        {
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - PixelScrollByAmount);
        }

        /// <summary>
        /// Change button state depending on scroll viewer position
        /// </summary>
        private void UpdatePagingButtonState()
        {
            this.PageLeftEnabled = scrollViewer.HorizontalOffset > ScrollErrorMargin;
            this.PageRightEnabled = scrollViewer.HorizontalOffset < scrollViewer.ScrollableWidth - ScrollErrorMargin;
        }

       
    }
}

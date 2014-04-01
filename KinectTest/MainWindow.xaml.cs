using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace KinectTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() {
            InitializeComponent();
        }

        KinectSensor _sensor;
        WriteableBitmap colorBitmap;
        Skeleton[] skeletonData;

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            if (KinectSensor.KinectSensors.Count > 0) {
                //Could be more than one just set to the first
                _sensor = KinectSensor.KinectSensors[0];

                //Check the State of the sensor
                if (_sensor.Status == KinectStatus.Connected) {

                    colorBitmap = new WriteableBitmap(_sensor.ColorStream.FrameWidth, _sensor.ColorStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);

                    //Enable the feature
                    _sensor.ColorStream.Enable();
                    _sensor.DepthStream.Enable();
                    _sensor.SkeletonStream.Enable();
                    _sensor.AllFramesReady += _sensor_AllFramesReady; //Double Tab
                    // Start the sensor!
                    try {
                        _sensor.Start();
                    }
                    catch (IOException) {
                        _sensor = null;
                    }
                }
            }
        }

        void _sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e) {
            //using - Automatically dispose of the open when complete
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame()) {
                if (colorFrame == null) {
                    return;
                }
                byte[] pixels = new byte[colorFrame.PixelDataLength];
                colorFrame.CopyPixelDataTo(pixels);

                int stride = colorFrame.Width * 4;
                image1.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
            }
            
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame == null) {return;}
                skeletonData = new Skeleton[_sensor.SkeletonStream.FrameSkeletonArrayLength];
                skeletonFrame.CopySkeletonDataTo(skeletonData);
                var skeleton = skeletonData.Where(s => s.TrackingState == SkeletonTrackingState.Tracked).FirstOrDefault();

               if (skeleton != null)
               {
                   JointCollection jointCollection = skeleton.Joints;

                   if(jointCollection[JointType.Head] != null){
                        btnFound.Background = Brushes.Green;
                        btnFound.Content = "Skeleton";
                   }

                   skeletonCanvas.Children.Clear();
                   

                   foreach(Joint joint in jointCollection){
                       
                       Ellipse ellipse = new Ellipse();
                       ellipse.Width = 20;
                       ellipse.Height = 20;
                       ellipse.StrokeThickness = 5;
                       ellipse.Stroke = Brushes.Blue;

                       

                       Canvas.SetTop(ellipse, (joint.Position.Y * -400) + 300);
                       //Console.WriteLine(joint.Position.X);
                       Canvas.SetLeft(ellipse, (joint.Position.X * 500) + 400);
                       //Console.WriteLine(joint.Position.Y);
                       
                       skeletonCanvas.Children.Add(ellipse);

                   }

               } else {
                    btnFound.Background = Brushes.Red;
                    btnFound.Content = "Empty";
               }
               
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {

        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            new Thread(() => 
            {
                _sensor.ElevationAngle = Convert.ToInt32(txtTilt.Text);
            }).Start();
        }
    }
}

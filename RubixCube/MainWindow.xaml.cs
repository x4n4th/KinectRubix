using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RubixCube
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int SIZE_OF_CUBE = 3;
        private const int BOARD_SIZE = 600;
        private const int BLOCK_WIDTH = BOARD_SIZE / SIZE_OF_CUBE;
        private const int BLOCK_HEIGHT = BOARD_SIZE / SIZE_OF_CUBE;

        private const int LOWER_BOUND = -(int)(SIZE_OF_CUBE / 2);
        private const int UPPER_BOUND = (int)(SIZE_OF_CUBE / 2);

        private Vector2i leftHand = null;
        private Vector2i rightHand = null;

        Rectangle[,] rectangles;

        private List<Block> blocks;

        KinectSensor _sensor;
        WriteableBitmap colorBitmap;
        Skeleton[] skeletonData;

        DateTime lastCommandTime;

        private const int SENSITIVITY = 55; //Distance Required to trigger a command
        private const int TIME_BETWEEN_COMMANDS = 1000; //milliseconds

        public MainWindow() {
            InitializeComponent();
            rectangles = new Rectangle[SIZE_OF_CUBE, SIZE_OF_CUBE];
            lastCommandTime = DateTime.Now;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            blocks = new List<Block>();

            for (int x = LOWER_BOUND; x < UPPER_BOUND + 1; x++) {
                for (int y = LOWER_BOUND; y < UPPER_BOUND + 1; y++) {
                    for (int z = LOWER_BOUND; z < UPPER_BOUND + 1; z++) {
                        blocks.Add(new Block(new Point3D(x, y, z), Colors.Green, Colors.Red, Colors.Orange, Colors.White, Colors.Blue, Colors.Yellow));
                    }
                }
            }

            drawRectangles();

            drawColors();

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

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame()) {
                if (skeletonFrame == null) { return; }
                skeletonData = new Skeleton[_sensor.SkeletonStream.FrameSkeletonArrayLength];
                skeletonFrame.CopySkeletonDataTo(skeletonData);
                var skeleton = skeletonData.Where(s => s.TrackingState == SkeletonTrackingState.Tracked).FirstOrDefault();

                if (skeleton != null) {
                    JointCollection jointCollection = skeleton.Joints;

                    Vector2i currentRight = null;
                    Vector2i currentLeft = null;

                    if (jointCollection[JointType.WristRight] != null) {
                        Ellipse ellipse = new Ellipse();
                        ellipse.Width = 20;
                        ellipse.Height = 20;
                        ellipse.StrokeThickness = 5;
                        ellipse.Stroke = Brushes.AliceBlue;
                        float y = jointCollection[JointType.WristRight].Position.Y * -700 + 300;
                        float x = jointCollection[JointType.WristRight].Position.X * 650 + 400;

                        currentRight = new Vector2i(x, y);

                        //Console.WriteLine(currentRight.ToString());

                        Canvas.SetTop(ellipse, y);
                        //Console.WriteLine(joint.Position.X);
                        Canvas.SetLeft(ellipse, x);
                        //Console.WriteLine(joint.Position.Y);
                        cursorArray.Children.Clear();
                        cursorArray.Children.Add(ellipse);
                    }
                    if(jointCollection[JointType.WristLeft] != null){
                        float y = jointCollection[JointType.WristLeft].Position.Y * -600;
                        float x = jointCollection[JointType.WristLeft].Position.X * 550;

                        currentLeft = new Vector2i(x, y);
                    }
                    if (leftHand != null && currentLeft != null) {
                        float distance = leftHand.DistanceTo(currentLeft);
                        //Console.WriteLine("Distance Left: " + leftHand.DistanceTo(currentLeft));
                        if (distance > 30 || distance < -30) {
                            //Console.WriteLine("Fast Movement: Do rotate");
       
                        }
                    }

                    if (rightHand != null && currentRight != null && DateTime.Now.Subtract(lastCommandTime).TotalMilliseconds >= TIME_BETWEEN_COMMANDS) {
                        float distance = rightHand.DistanceTo(currentRight);
                        if (leftHand != null && currentLeft != null) {
                            float distanceLeft = leftHand.DistanceTo(currentLeft);
                            //Console.WriteLine("Distance Left: " + leftHand.DistanceTo(currentLeft));
                            if ((distance > SENSITIVITY || distance < -SENSITIVITY) && (distanceLeft < -SENSITIVITY || distanceLeft > SENSITIVITY)) {
                                if (distance > SENSITIVITY) {
                                    TransformCube(Vector3i.left);
                                }
                                else if (distance < -SENSITIVITY) {
                                    TransformCube(Vector3i.right);
                                }
                                else if (distanceLeft < -SENSITIVITY) {
                                    TransformCube(Vector3i.up);
                                }
                                else if (distanceLeft > SENSITIVITY) {
                                    TransformCube(Vector3i.down);
                                }
                                //Console.WriteLine("Fast Movement: Do rotate");
                                lastCommandTime = DateTime.Now;
                            }
                        }
                        //Console.WriteLine("Distance Right: " + rightHand.DistanceTo(currentRight));
                        if (distance > SENSITIVITY || distance < -SENSITIVITY) {
                            //Console.WriteLine("Fast Movement: Rotate Row");
                            float xDistance = rightHand.DistanceToX(currentRight);
                            float yDistance = rightHand.DistanceToY(currentRight);

                            float xLocation = currentRight.x;
                            float yLocation = currentRight.y;
                            float increment = (BOARD_SIZE + 30) / SIZE_OF_CUBE;
                            float localIncrement = 0;

                            int matrixXLocation = 0;
                            int matrixYLocation = 0;

                            for (int i = LOWER_BOUND - 1; i <= UPPER_BOUND; i++) {
                                //Console.WriteLine("xI: " + i);
                                if (xLocation > localIncrement) {
                                    localIncrement += increment;
                                }
                                else {
                                    matrixXLocation = i;
                                    break;
                                }
                            }

                            localIncrement = 0;

                            for (int i = LOWER_BOUND - 1; i <= UPPER_BOUND; i++) {
                                //Console.WriteLine("yI: " + i);
                                if (yLocation > localIncrement) {
                                    localIncrement += increment;
                                }
                                else {
                                    matrixYLocation = i;
                                    break;
                                }
                            }

                            if (xDistance > 60) {
                                Console.WriteLine(rightHand.ToString());
                                yRotate(matrixYLocation, Vector3i.left);
                            }
                            else if (xDistance < -60) {
                                Console.WriteLine(rightHand.ToString());
                                yRotate(matrixYLocation, Vector3i.right);
                            }
                            else if (yDistance > 60) {
                                Console.WriteLine(rightHand.ToString());
                                zRotate(matrixXLocation, Vector3i.up);
                            }
                            else if (yDistance < -60) {
                                Console.WriteLine(rightHand.ToString());
                                zRotate(matrixXLocation, Vector3i.down);
                            }
                            lastCommandTime = DateTime.Now;
                        }
                    }

                    rightHand = currentRight;
                    leftHand = currentLeft;
                }
            }
        }
        private void TransformCube(Vector3i vector) {
            if (vector == Vector3i.up) {
                RotateCube(-90, new Vector3D(0, 0, 0), vector);
            }
            else if (vector == Vector3i.down) {
                RotateCube(90, new Vector3D(0, 0, 0), vector);
            }
            else if (vector == Vector3i.left) {
                RotateCube(-90, new Vector3D(0, 0, 0), vector);
            }
            else if (vector == Vector3i.right) {
                RotateCube(90, new Vector3D(0, 0, 0), vector);
            }

            //blocks = temp;
            drawColors();
        }

        public void RotateCube(int angle, Vector3D axis, Vector3i direction) {
            RotateTransform3D xrotation = new RotateTransform3D(new AxisAngleRotation3D(
                                  axis, angle));

            foreach (Block block in blocks) {
                Point3D rotatedPoint = xrotation.Transform(block.vector3d);

                block.vector3d = rotatedPoint;
                block.transform(direction);
            }
        }

        private void drawRectangles() {
            ArrayOfBlocks.Children.Clear();
            for (int x = 0; x < SIZE_OF_CUBE; x++) {
                for (int y = 0; y < SIZE_OF_CUBE; y++) {
                    Rectangle rect = rectangles[x, y];

                    rect = new Rectangle {
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        Height = BLOCK_HEIGHT,
                        Width = BLOCK_WIDTH
                    };

                    rectangles[x, y] = rect;

                    Canvas.SetLeft(rect, x * BLOCK_HEIGHT);
                    Canvas.SetTop(rect, y * BLOCK_WIDTH);

                    ArrayOfBlocks.Children.Add(rect);
                }
            }
        }


        private void drawColors() {
            // We will draw the faces of all the blocks in the xy axis at z= 0

            foreach (Block block in blocks) {
                if (block.vector3d.X == LOWER_BOUND) {
                    block.DrawFront(rectangles[(int)block.vector3d.Z + UPPER_BOUND, (int)block.vector3d.Y + UPPER_BOUND]);
                }
            }
        }

        private void direction_Click(object sender, RoutedEventArgs e) {
            //Sender is a button
            Button btn = (Button)sender;

            string btnName = btn.Name;

            if (btnName == "left") {
                TransformCube(Vector3i.left);
            }
            else if (btnName == "right") {
                TransformCube(Vector3i.right);
            }
            else if (btnName == "down") {
                TransformCube(Vector3i.down);
            }
            else if (btnName == "up") {
                TransformCube(Vector3i.up);
            }
        }

        /*   *
         *   *
         * Y *
         *   *
         *   * * * * * *
         *        Z
         */
        private void yRotate_Click(object sender, RoutedEventArgs e) {
            RotateTransform3D xrotation = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 0), 90));

            foreach (Block block in blocks) {
                if (block.vector3d.Z == 1) {

                    Point3D rotatedPoint = xrotation.Transform(block.vector3d);

                    block.vector3d = rotatedPoint;
                    block.transform(Vector3i.right);
                }
            }
            drawColors();
        }

        private void zRotate_Click(object sender, RoutedEventArgs e) {
            RotateTransform3D xrotation = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 0), 90));

            foreach (Block block in blocks) {
                if (block.vector3d.Y == 1) {

                    Point3D rotatedPoint = xrotation.Transform(block.vector3d);

                    block.vector3d = rotatedPoint;
                    block.transform(Vector3i.down);
                }
            }
            drawColors();
        }

        private void yRotate(int y, Vector3i direction) {
            RotateTransform3D xrotation = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 0), 90));

            foreach (Block block in blocks) {
                if (block.vector3d.Y == y) {

                    Point3D rotatedPoint = xrotation.Transform(block.vector3d);

                    block.vector3d = rotatedPoint;
                    block.transform(direction);
                }
            }
            drawColors();
        }

        private void zRotate(int z, Vector3i direction) {
            RotateTransform3D xrotation = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 0), 90));

            foreach (Block block in blocks) {
                if (block.vector3d.Z == z) {

                    Point3D rotatedPoint = xrotation.Transform(block.vector3d);

                    block.vector3d = rotatedPoint;
                    block.transform(direction);
                }
            }
            drawColors();
        }
        /*private void zRotate2_Click(object sender, RoutedEventArgs e) {
            RotateTransform3D xrotation = new RotateTransform3D(new AxisAngleRotation3D(
          new Vector3D(0, 0, 0), 90));

            foreach (Block block in blocks) {
                if (block.vector3d.Y == 0) {

                    Point3D rotatedPoint = xrotation.Transform(block.vector3d);

                    block.vector3d = rotatedPoint;
                    block.transform(Vector3i.right);
                }
            }
            drawColors();
        }

        private void zRotate3_Click(object sender, RoutedEventArgs e) {
            RotateTransform3D xrotation = new RotateTransform3D(new AxisAngleRotation3D(
          new Vector3D(0, 0, 0), 90));

            foreach (Block block in blocks) {
                if (block.vector3d.Y == -1) {

                    Point3D rotatedPoint = xrotation.Transform(block.vector3d);

                    block.vector3d = rotatedPoint;
                    block.transform(Vector3i.right);
                }
            }
            drawColors();
        }

        private void yRotate1_Click(object sender, RoutedEventArgs e) {
            RotateTransform3D xrotation = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 0), 90));

            foreach (Block block in blocks) {
                if (block.vector3d.Z == 0) {

                    Point3D rotatedPoint = xrotation.Transform(block.vector3d);

                    block.vector3d = rotatedPoint;
                    block.transform(Vector3i.down);
                }
            }
            drawColors();
        }

        private void yRotate2_Click(object sender, RoutedEventArgs e) {
            RotateTransform3D xrotation = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 0), 90));

            foreach (Block block in blocks) {
                if (block.vector3d.Z == -1) {

                    Point3D rotatedPoint = xrotation.Transform(block.vector3d);

                    block.vector3d = rotatedPoint;
                    block.transform(Vector3i.down);
                }
            }
            drawColors();
        }

        private void zRotateOther2_Click(object sender, RoutedEventArgs e) {
            RotateTransform3D xrotation = new RotateTransform3D(new AxisAngleRotation3D(
          new Vector3D(0, 0, 0), -90));

            foreach (Block block in blocks) {
                if (block.vector3d.Y == 1) {

                    Point3D rotatedPoint = xrotation.Transform(block.vector3d);

                    block.vector3d = rotatedPoint;
                    block.transform(Vector3i.left);
                }
            }
            drawColors();
        }

        private void zRotateOther1_Click(object sender, RoutedEventArgs e) {
            RotateTransform3D xrotation = new RotateTransform3D(new AxisAngleRotation3D(
          new Vector3D(0, 0, 0), -90));

            foreach (Block block in blocks) {
                if (block.vector3d.Y == 0) {

                    Point3D rotatedPoint = xrotation.Transform(block.vector3d);

                    block.vector3d = rotatedPoint;
                    block.transform(Vector3i.left);
                }
            }
            drawColors();
        }

        private void zRotateOther_Click(object sender, RoutedEventArgs e) {
            RotateTransform3D xrotation = new RotateTransform3D(new AxisAngleRotation3D(
          new Vector3D(0, 0, 0), -90));

            foreach (Block block in blocks) {
                if (block.vector3d.Y == -1) {

                    Point3D rotatedPoint = xrotation.Transform(block.vector3d);

                    block.vector3d = rotatedPoint;
                    block.transform(Vector3i.left);
                }
            }
            drawColors();
        }

        private void yRotateOther_Click(object sender, RoutedEventArgs e) {
            RotateTransform3D xrotation = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 0), -90));

            foreach (Block block in blocks) {
                if (block.vector3d.Z == -1) {

                    Point3D rotatedPoint = xrotation.Transform(block.vector3d);

                    block.vector3d = rotatedPoint;
                    block.transform(Vector3i.right);
                }
            }
            drawColors();
        }

        private void yRotate_Other1_Click(object sender, RoutedEventArgs e) {
            RotateTransform3D xrotation = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 0), -90));

            foreach (Block block in blocks) {
                if (block.vector3d.Z == 0) {

                    Point3D rotatedPoint = xrotation.Transform(block.vector3d);

                    block.vector3d = rotatedPoint;
                    block.transform(Vector3i.right);
                }
            }
            drawColors();
        }

        private void yRotate_Other2_Click(object sender, RoutedEventArgs e) {
            RotateTransform3D xrotation = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 0), 90));

            foreach (Block block in blocks) {
                if (block.vector3d.Z == 1) {

                    Point3D rotatedPoint = xrotation.Transform(block.vector3d);

                    block.vector3d = rotatedPoint;
                    block.transform(Vector3i.right);
                }
            }
            drawColors();
        }
    }*/
    }
}

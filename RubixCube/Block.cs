using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace RubixCube
{
    class Block
    {
        public Point3D vector3d { get; set; }

        
        /* this block consists of 6 sides 
         */
        private Color front;
        private Color right;
        private Color left;
        private Color bottom;
        private Color back;
        private Color top;

        //Set the colors of these sides
        public Block(Point3D vector3d, Color front, Color right, Color left, Color bottom, Color back, Color top) {
            this.vector3d = vector3d;
            this.front = front;
            this.right = right;
            this.left = left;
            this.bottom = bottom;
            this.back = back;
            this.top = top;
        }

        private void setFields(Point3D vector3d, Color front, Color right, Color left, Color bottom, Color back, Color top){
            this.vector3d = vector3d;
            this.front = front;
            this.right = right;
            this.left = left;
            this.bottom = bottom;
            this.back = back;
            this.top = top;
        }

        /*
         * Return a new transform block rotated by a vecotr
         */
        public void transform(Vector3i vector) {
            if (vector == Vector3i.up) {
                setFields(vector3d, bottom, right, left, back, top, front);
            }
            else if (vector == Vector3i.down) {
                setFields(vector3d, top, right, left, front, bottom, back);
            }
            else if (vector == Vector3i.left) {
                setFields(vector3d, left, front, back, bottom, right, top);
            }
            else if (vector == Vector3i.right) {
                setFields(vector3d, right, back, front, bottom, left, top);
            }
        }

        public void DrawFront(Rectangle rect) {
            rect.Fill = new SolidColorBrush(this.front);
            double x = Canvas.GetLeft(rect);
            double y = Canvas.GetTop(rect);

            //Console.WriteLine("X: " + x + " Y: " + y + " Vector: " + vector3d.X + " " + vector3d.Y + " " + vector3d.Z);
        }

        public void ButtonColor(Button btn, string direction) {
            if (direction == "right") {
                btn.Background = new SolidColorBrush(right);
            }
            else if (direction == "left") {
                btn.Background = new SolidColorBrush(left);
            }
            else if (direction == "top") {
                btn.Background = new SolidColorBrush(top);
            }
            else if (direction == "bottom") {
                btn.Background = new SolidColorBrush(bottom);
            }
            else {
                btn.Background = new SolidColorBrush(back);
            }
        }
        public Color getFrontColor() {
            return front;
        }
    }
}

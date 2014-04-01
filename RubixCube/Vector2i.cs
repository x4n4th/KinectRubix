using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubixCube
{
    class Vector2i
    {
        public float x { get; private set; }
        public float y { get; private set; }

        public Vector2i(float x, float y) {
            this.x = x;
            this.y = y;
        }

        public float DistanceTo(Vector2i to) {
            float dx = to.x - x;
            float dy = to.y - y;

            double distance = Math.Sqrt(dx * dx + dy * dy);

            return (float)distance;
        }
        public float DistanceToY(Vector2i to) {
            return to.y - y;
        }

        public float DistanceToX(Vector2i to) {
            return to.x - x;
        }

        public override string ToString() {
            return "X: " + x + " Y: " + y;
        }
    }
}

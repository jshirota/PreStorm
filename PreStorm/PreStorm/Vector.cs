namespace PreStorm
{
    internal class Vector
    {
        public double X { get; private set; }
        public double Y { get; private set; }

        public Vector(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator Vector(Point point)
        {
            return new Vector(point.x, point.y);
        }

        public static implicit operator Point(Vector vector)
        {
            return new Point { x = vector.X, y = vector.Y };
        }

        public static Vector operator +(Vector v1, Vector v2)
        {
            return new Vector(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            return new Vector(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector operator *(Vector v1, double n)
        {
            return new Vector(v1.X * n, v1.Y * n);
        }

        public static Vector operator *(double n, Vector v1)
        {
            return v1 * n;
        }

        public static Vector operator /(Vector v1, double n)
        {
            return new Vector(v1.X / n, v1.Y / n);
        }

        public static double DotProduct(Vector v1, Vector v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }
    }
}

using System;
using System.Windows;

namespace HandyScreenshot.Common
{
    public struct ReadOnlyRect
    {
        public static readonly ReadOnlyRect Zero = (0D, 0D, 0D, 0D);
        public static readonly ReadOnlyRect Empty = (double.PositiveInfinity, double.PositiveInfinity, double.NegativeInfinity, double.NegativeInfinity);

        public readonly double X;
        public readonly double Y;
        public readonly double Width;
        public readonly double Height;
        public readonly bool IsEmpty;

        public ReadOnlyRect(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            IsEmpty = width < 0;
        }

        public bool Contains(double x, double y)
        {
            return X <= x && Y <= y && x <= X + Width && y <= Y + Height;
        }

        public bool IntersectsWith(ReadOnlyRect rect)
        {
            return !IsEmpty && !rect.IsEmpty &&
                   rect.X <= X + Width &&
                   rect.X + rect.Width >= X &&
                   rect.Y <= Y + Height &&
                   rect.Y + rect.Height >= Y;
        }

        public ReadOnlyRect Intersect(ReadOnlyRect rect)
        {
            if (!IntersectsWith(rect))
            {
                return Empty;
            }

            double x = Math.Max(X, rect.X);
            double y = Math.Max(Y, rect.Y);
            double width = Math.Max(Math.Min(X + Width, rect.X + rect.Width) - x, 0.0);
            double height = Math.Max(Math.Min(Y + Height, rect.Y + rect.Height) - y, 0.0);
            return (x, y, width, height);


        }

        public ReadOnlyRect Union(ReadOnlyRect rect)
        {
            if (IsEmpty)
            {
                return Empty;
            }

            if (rect.IsEmpty) return Empty;
            double x = Math.Min(X, rect.X);
            double y = Math.Min(Y, rect.Y);
            // ReSharper disable CompareOfFloatsByEqualityOperator
            double width = rect.Width == double.PositiveInfinity || Width == double.PositiveInfinity
                ? double.PositiveInfinity
                : Math.Max(Math.Max(X + Width, rect.X + rect.Width) - x, 0.0);
            double height = rect.Height == double.PositiveInfinity || Height == double.PositiveInfinity
                ? double.PositiveInfinity
                : Math.Max(Math.Max(Y + Height, rect.Y + Height) - y, 0.0);
            // ReSharper restore CompareOfFloatsByEqualityOperator
            return (x, y, width, height);
        }

        public ReadOnlyRect Offset(double x, double y)
        {
            return (X - x, Y - y, Width, Height);
        }

        public ReadOnlyRect Scale(double scaleX, double scaleY)
        {
            return IsEmpty ? Empty : (X * scaleX, Y * scaleY, Width * scaleX, Height * scaleY);
        }

        public override string ToString() => $"({X}, {Y}) [{Width}, {Height}]";

        internal void Deconstruct(out double x, out double y, out double width, out double height)
        {
            x = X;
            y = Y;
            width = Width;
            height = Height;
        }

        public static implicit operator ReadOnlyRect((double, double, double, double) rect)
        {
            var (x, y, w, h) = rect;
            return new ReadOnlyRect(x, y, w, h);
        }

        public static implicit operator ReadOnlyRect(Rect rect)
        {
            return new ReadOnlyRect(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public bool Equals(ReadOnlyRect other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);
        }

        public override bool Equals(object obj)
        {
            return obj is ReadOnlyRect other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Width.GetHashCode();
                hashCode = (hashCode * 397) ^ Height.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ReadOnlyRect left, ReadOnlyRect right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ReadOnlyRect left, ReadOnlyRect right)
        {
            return !(left == right);
        }
    }
}

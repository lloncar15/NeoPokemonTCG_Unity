using System;

namespace GimGim.Utility {
    public static class MathUtilities {
        public static bool ApproximatelyEquals(this float a, float b, float epsilon = float.Epsilon) {
            return Math.Abs(a - b) < epsilon;
        }
        
        public static bool ApproximatelyEquals(this double a, double b, double epsilon = double.Epsilon)
        {
            return Math.Abs(a - b) < epsilon;
        }
    }
}
using UnityEngine;

namespace UnityExtensions {
    public class MathfExtensions : MonoBehaviour {
        
        /// <summary>
        /// Returns the minimum of two double values.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Min(double a, double b) {
            return (a < b) ? a : b;
        }

        /// <summary>
        /// Returns a minimum of the given double values.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double Min(params double[] values) {
            int num = values.Length;
            if (num == 0) {
                return 0f;
            }

            double num2 = values[0];
            for (int i = 1; i < num; i++) {
                if (values[i] < num2) {
                    num2 = values[i];
                }
            }

            return num2;
        }
        
        /// <summary>
        /// Returns the maximum of two double values.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Max(double a, double b) {
            return (a > b) ? a : b;
        }

        /// <summary>
        /// Returns a maximum of the given double values.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double Max(params double[] values) {
            int num = values.Length;
            if (num == 0) {
                return 0f;
            }

            double num2 = values[0];
            for (int i = 1; i < num; i++) {
                if (values[i] > num2) {
                    num2 = values[i];
                }
            }

            return num2;
        }
    }
}


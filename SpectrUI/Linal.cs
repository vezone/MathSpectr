using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrUI
{
    class Linal
    {
        public delegate double DefaultFunction(double value);

        public static double[] MakeArray(int length)
        {
            double[] array = new double[length];
            
            for (int i = 0; i < length; i++)
            {
                array[i] = 0;
            }

            return array;
        }

        public static double[][] MakeArray(int firstLength, int secondLength)
        {
            double[][] array = new double[firstLength][];

            for (int i = 0; i < firstLength; i++)
            {
                array[i] = new double[secondLength];
            }

            return array;
        }

        public static double[] MakeArray(
            double a, double b, int number)
        {
            double[] array = new double[number];

            int index = 0;
            double h = (b - a) / number;
            for (double i = a; i < b; i += h, index++)
            {
                array[index] = i;
            }

            return array;
        }

        public static double Max(double[] array)
        {
            double max = array[0];
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i] > max)
                    max = array[i];
            }
            return max;
        }

        public static double Min(double[] array)
        {
            double min = array[0];
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i] < min)
                    min = array[i];
            }
            return min;
        }

        public static void UseFunctionOnArray(ref double[] array, DefaultFunction function)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = function(array[i]);
            }
            System.Math.Abs(1.2);
        }

        public static double[] UseFunctionOnArrayR(double[] array, DefaultFunction function)
        {
            double[] newArray = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = function(array[i]);
            }
            return newArray;
        }

        public static double[] GaussSolve(double[][] matrix, double[] solution)
        {
            //прямой ход
            double v;
            int k, j, im;
            int n = matrix.Length;

            for (k = 0; k < (n - 1); k++)
            {
                im = k;
                for (int i = k+1; i < n; i++)
                {
                    if (Math.Abs(matrix[im][k]) < Math.Abs(matrix[i][k]))
                    {
                        im = i;
                    }//matrix[i][j] -= matrix[i][j] * matrix[i - 1][j];
                }
                if (im != k)
                {
                    for (j = 0; j < n; j++)
                    {
                        v = matrix[im][j];
                        matrix[im][j] = matrix[k][j];
                        matrix[k][j] = v;
                    }
                    v = solution[im];
                    solution[im] = solution[k];
                    solution[k] = v;
                }
                for (int i = k+1; i < n; i++)
                {
                    v = 1.0 * matrix[i][k] / matrix[k][k];
                    matrix[i][k] = 0;
                    solution[i] = solution[i] - v * solution[k];
                    if (v != 0)
                    {
                        for (j = k+1; j < n; j++)
                        {
                            matrix[i][j] = matrix[i][j] - v * matrix[k][j];
                        }
                    }
                }
            }

            //обратный ход
            double s;
            double[] result = new double[n];
            result[n - 1] = 1.0 * solution[n - 1] / matrix[n-1][n-1];
            for (int i = n-2; i >= 0; i--)
            {
                s = 0;
                for (j = i+1; j < n; j++)
                {
                    s = s + matrix[i][j] * result[j];
                }
                result[i] = 1.0 * (solution[i] - s) / matrix[i][i];
            }

            return result;
        }

        public static double[] SplitArray(double[] oldArray, int length)
        {
            double[] array = new double[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = oldArray[i];
            }
            return array;
        }

        public static double[] SearchFor(double value, double[] array1)
        {
            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] == value)
                {
                    return new double[] { i, array1[i] };
                }
            }
            return new double[] { 0, 0 };
        }

    }
}

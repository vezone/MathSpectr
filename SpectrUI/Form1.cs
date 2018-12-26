//#define vx

using System;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SpectrUI
{
    public partial class Form1 : Form
    {

        public class Result
        {
            public double[] spectrArray;
            public double[] aArray;
        }
        int x_num = 100;
#if vx
        static double a0 = -3.2, b0 = 3.2;
#else
        static double a0 = 0.0, b0 = 3.0;
#endif
        static double h;
        
        double[] spectrXEven;
        double[] spectrYEven;
        double[] spectrXNotEven;
        double[] spectrYNotEven;

        double defChartValueMinX = -0.5;//Linal.Min(aresult);
        double defChartValueMaxX = 4;//Linal.Max(aresult);
        double defChartValueIntervalX = 1;

        double defChartValueMinY = -0.5;
        double defChartValueMaxY = 3;
        double defChartValueIntervalY = 1;

        public delegate double FunctionPrototype(double value);

        public static double V(double x)
        {
            double quad = x * x;
            x = (System.Math.Abs(x) < 3)
                 //&& (System.Math.Abs(x) > a) 
                 ?
                0.1 * (quad + 1.0) * (quad - 9.0) * Math.Pow(Math.E, (x / 3.0))
                : 0.0;
            return x;
        }

        public static double dV(double x)
        {
            double part1 = 0.1 * x * x + 0.1,
                   eps = System.Math.Pow(System.Math.E, x / 3),
                   quad = x * x - 9;

            x = //System.Math.Abs(x) <= 3. ?
                2 * x * part1 * eps +
                0.2 * x * quad * eps +
                (part1 * quad * eps) / 3;
            //: 0.0;
            return x;
        }

        public double[] PsiPositive(
            double A, double B,
            double x, double lamda)
        {
            double u =
                A * Math.Sin(Math.Sqrt(lamda) * x) +
                B * Math.Cos(Math.Sqrt(lamda) * x);
            double du =
                A * Math.Sqrt(lamda) *
                Math.Cos(Math.Sqrt(lamda) * x) -
                B * Math.Sqrt(lamda) *
                Math.Sin(Math.Sqrt(lamda) * x);
            return new double[] { u, du };
        }

        public double[] PsiNegative(
            double A, double B,
            double x, double lamda)
        {
            double u =
                A * Math.Sinh(Math.Sqrt(-lamda) * x) +
                B * Math.Cosh(Math.Sqrt(-lamda) * x);
            double du =
                A * Math.Sqrt(-lamda) * Math.Cosh(Math.Sqrt(-lamda) * x) -
                B * Math.Sqrt(-lamda) * Math.Sinh(Math.Sqrt(-lamda) * x);
            return new double[] { u, du};
        }

        public double ddV()
        {
            return 0.0;
        }

        public static double Hole(double x)
        {
            return ((x <= 3.0 && x >= -3.0) ? -5.0 : 0.0);
        }

        //TODO:
        public double[] FindSpectr(
            double[] gridX,
            string mode,
            double spectr = 0,
            int NumberOfPsi = 10)
        {
            double[][] psi = Linal.MakeArray(NumberOfPsi, 2);
            double lamda =
                -V((gridX[0] + gridX[1]) / 2) - spectr;
            
            if (mode.Equals("even"))
            {
                if (lamda > 0)
                {
                    //значение psi на х0
                    psi[0][0] = Math.Cos(Math.Sqrt(lamda) * gridX[0]);
                    psi[0][1] = -Math.Sqrt(lamda) * Math.Sin(Math.Sqrt(lamda) * gridX[0]);

                    //значение psi на х1
                    psi[1][0] = Math.Cos(Math.Sqrt(lamda) * gridX[1]);
                    psi[1][1] = -Math.Sqrt(lamda) * Math.Sin(Math.Sqrt(lamda) * gridX[1]);
                }
                else
                {
                    //значение psi на х0
                    psi[0][0] = Math.Cosh(Math.Sqrt(-lamda) * gridX[0]);
                    psi[0][1] = -Math.Sqrt(-lamda) * Math.Sinh(Math.Sqrt(-lamda) * gridX[0]);

                    //значение psi на х1
                    psi[1][0] = Math.Cosh(Math.Sqrt(-lamda) * gridX[1]);
                    psi[1][1] = -Math.Sqrt(-lamda) * Math.Sinh(Math.Sqrt(-lamda) * gridX[1]);
                }
            }
            else
            {
                if (lamda > 0)
                {
                    //значение psi на х0
                    psi[0][0] = Math.Sin(Math.Sqrt(lamda) * gridX[0]);
                    psi[0][1] = Math.Sqrt(lamda) * Math.Cos(Math.Sqrt(lamda) * gridX[0]);

                    //значение psi на х1
                    psi[1][0] = Math.Sin(Math.Sqrt(lamda) * gridX[1]);
                    psi[1][1] = Math.Sqrt(lamda) * Math.Cos(Math.Sqrt(lamda) * gridX[1]);
                }
                else
                {
                    //значение psi на х0
                    psi[0][0] = Math.Sinh(Math.Sqrt(lamda) * gridX[0]);
                    psi[0][1] = Math.Sqrt(lamda) * Math.Cosh(Math.Sqrt(lamda) * gridX[0]);

                    //значение psi на х1
                    psi[1][0] = Math.Sinh(Math.Sqrt(lamda) * gridX[1]);
                    psi[1][1] = Math.Sqrt(lamda) * Math.Cosh(Math.Sqrt(lamda) * gridX[1]);
                }
            }

            double[][] left;
            //находим значение функции при spectr'е
            for (int j = 1; j < (NumberOfPsi-1); j++)
            {
                lamda =
                    //#находим лямбду
                    -V((gridX[j] + gridX[j + 1]) / 2)
                    - spectr;
                
                if (lamda > 0)
                {
                    left = new double[2][]
                    {
                        new double[] {
                            Math.Sin(Math.Sqrt(lamda) * gridX[j]),
                            Math.Cos(Math.Sqrt(lamda) * gridX[j]) },
                        new double[] {
                             Math.Sqrt(lamda) * Math.Cos(Math.Sqrt(lamda) * gridX[j]),
                            -Math.Sqrt(lamda) * Math.Sin(Math.Sqrt(lamda) * gridX[j]) }
                    };

                    //решаем систему и находим А и В
                    double[] AB = Linal.GaussSolve(left, psi[j]);
                    //записываем значение функции и производной в точке x[j+1]
                    psi[j + 1] = PsiPositive(AB[0], AB[1], gridX[j + 1], lamda);
                }
                else
                {
                    //NOTE: lamda -
                    left = new double[2][]
                    {
                        new double[] {
                            Math.Sinh(Math.Sqrt(lamda) * gridX[j]),
                            Math.Cosh(Math.Sqrt(lamda) * gridX[j]) },
                        new double[] {
                            Math.Sqrt(lamda) * Math.Cosh(Math.Sqrt(lamda) * gridX[j]),
                            Math.Sqrt(lamda) * Math.Sinh(Math.Sqrt(lamda) * gridX[j]) }
                    };
                    //решаем систему и находим А и В
                    double[] AB = Linal.GaussSolve(left, psi[j]);
                    //записываем значение функции и производной в точке x[j+1]
                    psi[j + 1] = PsiNegative(AB[0], AB[1], gridX[j + 1], lamda);
                }
            }

            //находим решение на конце отрезка xm = b0
            double sqrtSpec = Math.Sqrt(spectr);
            double lastGridX = gridX[NumberOfPsi - 1];

            left = new double[2][]
            {
                new double[] {
                    Math.Exp( sqrtSpec * lastGridX),
                    Math.Exp(-sqrtSpec * lastGridX) },
                new double[] {
                     sqrtSpec * Math.Exp(sqrtSpec * lastGridX),
                    -sqrtSpec * Math.Sinh(sqrtSpec * lastGridX) }
            };
            double[] a1b1 = Linal.GaussSolve(left, psi[NumberOfPsi - 1]);

            return new double[] { a1b1[0], a1b1[1] };
        }

        public Result Resh(string mode="even")
        {
            double[] x_ = Linal.MakeArray(a0, b0 + 1, 1000);
            double[] v = Linal.UseFunctionOnArrayR(x_, V);
            double U0 = Linal.Max(Linal.UseFunctionOnArrayR(v, Math.Abs));
            int numberOfNodes = 50;
            double[] gridX = Linal.MakeArray(a0, b0, numberOfNodes);
            int numberOfSpectrs = 500; //500
            double hSpectr = U0 / numberOfSpectrs;
            double[] spectrX = Linal.MakeArray(hSpectr, U0, numberOfSpectrs);
            //spectrX = Linal.SplitArray(spectrX, spectrX.Length - 1);
            double[] spectrY = Linal.MakeArray(spectrX.Length);

            for (int i = 0; i < (numberOfSpectrs-1); i++)
            {
                spectrY[i] = FindSpectr(gridX, mode, spectrX[i], numberOfNodes)[0];
                //searching for nulls
                if (i != (0) && ((spectrY[i] * spectrY[i-1]) < 0))
                {
                    double start = spectrX[i-1];
                    double end   = spectrX[i];
                    
                    double h = (end)/2;
                    double position = start;
                    while (position <= end)
                    {
                        double spectrYValue = FindSpectr(gridX, mode, position, numberOfNodes)[0];
                        if (Math.Abs(spectrYValue) <= 0.005)
                        { 
                            label3.Text = $"spectrX = {Math.Sqrt(position)}";
                            label7.Text = $"spectrY = {spectrYValue}";
                            Console.WriteLine($"spectrX = {position} spectrY={spectrYValue}");
                        }
                        position += h;
                    }
                    
                }
            }

            Result gresult = new Result();
            gresult.spectrArray = spectrX;
            gresult.aArray = spectrY;
            return gresult;
        }

        public void AlgoInit()
        {
            Result gresult = Resh();
            spectrXEven = Linal.SplitArray(gresult.spectrArray, 500);
            spectrYEven = Linal.SplitArray(gresult.aArray, 500);

            gresult = Resh("noteven");
            spectrXNotEven = Linal.SplitArray(gresult.spectrArray, 500);
            spectrYNotEven = Linal.SplitArray(gresult.aArray,      500);
        }

        public void ChartInit()
        {
            defChartValueMinX = 0;
            defChartValueMaxX = 0.75;
            defChartValueIntervalX = 0.25;

            defChartValueMinY = -1;
            defChartValueMaxY = 1;
            defChartValueIntervalY = 0.25;

            chart1.Visible = true;

            chart1.ChartAreas[0].Name = "ChartArea0";
            chart1.ChartAreas[0].Visible = true;
            chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;


            chart1.ChartAreas[0].AxisX.Minimum =
#if vx
                -4;
#else
                defChartValueMinX;
#endif
            chart1.ChartAreas[0].AxisX.Maximum =
#if vx
                4;
#else
                defChartValueMaxX;
#endif
            chart1.ChartAreas[0].AxisX.Interval =
#if vx
                1;
#else
                defChartValueIntervalX;
#endif

            chart1.ChartAreas[0].AxisY.Minimum =
#if vx
                -6;
#else
                defChartValueMinY;
#endif
            chart1.ChartAreas[0].AxisY.Maximum =
#if vx
                0;
#else
                defChartValueMaxY;
#endif
            chart1.ChartAreas[0].AxisY.Interval =
#if vx
                1;
#else
                defChartValueIntervalY;
#endif

            chart1.Series.RemoveAt(0);
            chart1.Series.Add(new Series("x"));
            chart1.Series.Add(new Series("y"));
            chart1.Series.Add(new Series("V(x)"));
            chart1.Series.Add(new Series("Четная_функция"));
            chart1.Series.Add(new Series("Нечетная_функция"));

            chart1.Series["x"].ChartType = SeriesChartType.Line;
            chart1.Series["y"].ChartType = SeriesChartType.Line;
            chart1.Series["V(x)"].ChartType = SeriesChartType.Line;
            chart1.Series["Четная_функция"].ChartType = SeriesChartType.Line;
            chart1.Series["Нечетная_функция"].ChartType = SeriesChartType.Line;
            chart1.Series["x"].Color = System.Drawing.Color.Black;
            chart1.Series["y"].Color = System.Drawing.Color.Black;
            chart1.Series["V(x)"].Color = System.Drawing.Color.Blue;
            chart1.Series["Четная_функция"].Color = System.Drawing.Color.Indigo;
            chart1.Series["Нечетная_функция"].Color = System.Drawing.Color.Orange;
            chart1.Series["x"].ChartArea = "ChartArea0";
            chart1.Series["y"].ChartArea = "ChartArea0";
            chart1.Series["V(x)"].ChartArea = "ChartArea0";
            chart1.Series["Четная_функция"].ChartArea = "ChartArea0";
            chart1.Series["Нечетная_функция"].ChartArea = "ChartArea0";

            //MAXIMUM

            double[][] matrix = new double[2][] {
                    new double[]{1, 2},
                    new double[]{3, 4}
            };
            double[] solution = new double[2] { 4, 5 };
            double[] result = Linal.GaussSolve(matrix, solution);

            //label3.Text = $"({gresult.kArray})";
            //label7.Text = $"({gresult.a})";

            //grif x
            double[] d1 = new double[x_num];
            //V(a)
            double[] d2 = new double[x_num];
            //dV(a)
            double[] d3 = new double[x_num];
            //0 array
            double[] d4 = new double[x_num];
            //default grid stuff (X, Y)
            double[] d5 = new double[x_num];
            double[] d6 = new double[x_num];
            h = (b0 - a0) / x_num;

            double add = 35.0, add2 = -35.0;
            label1.Text = h.ToString();

            for (int i = 0;
                 i < x_num;
                 i++, a0 += h, add -= 3.0, add2 += 3.0)
            {
                d1[i] = a0;
                d2[i] = V(a0);
                d3[i] = dV(a0);
                d4[i] = 0;
                d5[i] = add;
                d6[i] = add2;
            }

            //V(x)
#if vx
            chart1.Series["V(x)"].Points.DataBindXY(d1, d2);
#else
            //result
            chart1.Series["Четная_функция"].Points.DataBindXY(spectrXEven, spectrYEven);
            chart1.Series["Нечетная_функция"].Points.DataBindXY(spectrXNotEven, spectrYNotEven);
#endif
            //dV(x)
            //chart1.Series["Series4"].Points.DataBindXY(d1, d3);
            //x
            chart1.Series["x"].Points.DataBindXY(d6, d4);
            //y
            chart1.Series["y"].Points.DataBindXY(d4, d5);
        }

        public Form1()
        {
            InitializeComponent();

            AlgoInit();
            ChartInit();
            
        }


        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            chart1.ChartAreas[0].AxisX.Minimum  /= 2;
            chart1.ChartAreas[0].AxisX.Maximum  /= 2;
                                                  
            chart1.ChartAreas[0].AxisY.Minimum  /= 2;
            chart1.ChartAreas[0].AxisY.Maximum  /= 2;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            chart1.ChartAreas[0].AxisX.Minimum  *= 2;
            chart1.ChartAreas[0].AxisX.Maximum  *= 2;
            //chart1.ChartAreas[0].AxisX.Interval *= 2;

            chart1.ChartAreas[0].AxisY.Minimum  *= 2;
            chart1.ChartAreas[0].AxisY.Maximum  *= 2;
            //chart1.ChartAreas[0].AxisY.Interval *= 2;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (chart1.ChartAreas[0].AxisX.Minimum > -15)
            {
                double xShift = (chart1.ChartAreas[0].AxisX.Maximum - chart1.ChartAreas[0].AxisX.Minimum) / 4;
                chart1.ChartAreas[0].AxisX.Maximum -= xShift;
                chart1.ChartAreas[0].AxisX.Minimum -= xShift;
                label2.Text =
                    chart1.ChartAreas[0].AxisX.Minimum
                    .ToString();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (chart1.ChartAreas[0].AxisX.Maximum < 15)
            {
                double xShift = (chart1.ChartAreas[0].AxisX.Maximum - chart1.ChartAreas[0].AxisX.Minimum) / 4;
                chart1.ChartAreas[0].AxisX.Maximum += xShift;
                chart1.ChartAreas[0].AxisX.Minimum += xShift;
                label2.Text =
                    chart1.ChartAreas[0].AxisX.Maximum
                    .ToString();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!chart1.ChartAreas[0].AxisX.MajorGrid.Enabled)
            {
                chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
                chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = true;
            }
            else
            {
                chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
                chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            chart1.ChartAreas[0].AxisX.Minimum = defChartValueMinX;
            chart1.ChartAreas[0].AxisX.Maximum = defChartValueMaxX;
            chart1.ChartAreas[0].AxisX.Interval = defChartValueIntervalX;

            chart1.ChartAreas[0].AxisY.Minimum = defChartValueMinY;
            chart1.ChartAreas[0].AxisY.Maximum = defChartValueMaxX;
            chart1.ChartAreas[0].AxisY.Interval = defChartValueIntervalY;

        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (chart1.ChartAreas[0].AxisY.Maximum < 15)
            {
                double yShift = (chart1.ChartAreas[0].AxisY.Maximum - chart1.ChartAreas[0].AxisY.Minimum) / 4;
                chart1.ChartAreas[0].AxisY.Maximum += yShift;
                chart1.ChartAreas[0].AxisY.Minimum += yShift;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (chart1.ChartAreas[0].AxisY.Minimum > -15)
            {
                double yShift = (chart1.ChartAreas[0].AxisY.Maximum - chart1.ChartAreas[0].AxisY.Minimum) / 4;
                chart1.ChartAreas[0].AxisY.Minimum -= yShift;
                chart1.ChartAreas[0].AxisY.Maximum -= yShift;
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {
            //
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}

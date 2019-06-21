//#define vx

using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SpectrUI
{
    public partial class Form1 : Form
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double Cos(double x)
        {
            return (Math.Cos(x));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double Sin(double x)
        {
            return (Math.Sin(x));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double Sqrt(double x)
        {
            return (Math.Sqrt(x));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double Exp(double x)
        {
            return (Math.Exp(x));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double Sinh(double x)
        {
            return Math.Sinh(x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double Cosh(double x)
        {
            return Math.Cosh(x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double Abs(double x)
        {
            return Math.Abs(x);
        }

        //public class Result
        //{
        //    public double[] spectrArray;
        //    public double[] aArray;
        //}
        static double m_Left = -4, m_Right = 4;
        static double m_A0 = -3.2, m_B0 = 3.2;

        static double m_H;
        
        double[] m_SpectrX;
        double[] m_SpectrY;

        double defChartValueMinX;
        double defChartValueMaxX;
        double defChartValueIntervalX;
        double defChartValueMinY;
        double defChartValueMaxY;
        double defChartValueIntervalY;

        bool m_HiddenClicked;

        public delegate double FunctionPrototype(double value);

        public static double V(double x)
        {
            double quad = x * x;
            x = ((Abs(x) <= m_B0)
                 &&
                 (Abs(x) >= m_A0)) 
                 ?
                0.1*(quad + 1.0) * (quad - 9.0) * Exp((x / 3.0))
                : 0.0;
            return x;
        }
        
        public double[] PsiPositive(
            double A, double B,
            double x, double lamda)
        {
            double u =
                A * Sin(Sqrt(lamda) * x) +
                B * Cos(Sqrt(lamda) * x);
            double du =
                A * Math.Sqrt(lamda) * Math.Cos(Math.Sqrt(lamda) * x) -
                B * Math.Sqrt(lamda) * Math.Sin(Math.Sqrt(lamda) * x);
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

        public static double Hole(double x)
        {
            return ((x <= 3.0 && x >= -3.0) ? -5.0 : 0.0);
        }

        //TODO:
        public double[] FindSpectr(
            double[] gridX,
            double spectrX,
            int NumberOfPsi)
        {
            double[][] psi = Linal.MakeArray(NumberOfPsi, 2);
            double lamda =
                -V((gridX[0] + gridX[1]) / 2) - spectrX;
            
            if (lamda > 0)
            {
                //значение psi на х0, x1
                for (int i = 0; i < 2; i++)
                {
                    psi[i][0] =  Math.Cos(Math.Sqrt(lamda) * gridX[i]);
                    psi[i][1] = -Math.Sqrt(lamda) * Math.Sin(Math.Sqrt(lamda) * gridX[i]);
                }
            }
            else
            {
                //значение psi на х0, x1
                for (int i = 0; i < 2; i++)
                {
                    psi[i][0] =  Math.Cosh(Math.Sqrt(-lamda) * gridX[i]);
                    psi[i][1] = -Math.Sqrt(-lamda) * Math.Sinh(Math.Sqrt(-lamda) * gridX[i]);
                }
            }
            

            double[][] left;
            //находим значение функции при spectr'е
            for (int j = 1; j < (NumberOfPsi-1); j++)
            {
                lamda =
                    //находим лямбду
                    -V((gridX[j] + gridX[j + 1]) / 2)
                    - spectrX;
                
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
            double sqrtSpec = Math.Sqrt(spectrX);
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
            //a1b1[0] - спектрY
            return new double[] {a1b1[0]};
        }

        // (left; a)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double[] Spectr1(
            double[] spectrX,
            double[] gridX,
            int numberOfNodes)
        {
            double[] psi = new double[numberOfNodes];
            for (int i = 0; i < (numberOfNodes-1); i++)
            {
                psi[i] = Exp(spectrX[i] * gridX[i]);
            }
            return psi;
        }

        // [a; b]
        public double[] Spectr2(
            double[] gridX,
            double[] spectrX,
            int numberOfSpectrs,
            int numberOfNodes)
        {
            double gridXV   = 0.0;
            double gridXVP  = 0.0;
            double spectrXV = 0.0;
            double lamda    = 0.0;
            double[] AB = new double[2];
            double[] psi = new double[numberOfSpectrs];

            for (int n = numberOfSpectrs; n < (2 * numberOfSpectrs-1); n++)
            {
                gridXV = gridX[n - numberOfSpectrs];
                gridXVP = gridX[n - (numberOfSpectrs-1)];
                spectrXV = spectrX[n- numberOfSpectrs];
                lamda =
                   V((gridXV + gridXVP) / 2) - (spectrXV * spectrXV);
                
                if (lamda > 0)
                {
                    //10
                    double[][] left = new double[2][]
                    {
                        new double[] {
                            Cos(Sqrt(lamda)*gridXV),
                            Sin(Sqrt(lamda)*gridXV)
                        },
                        new double[]{
                             Sqrt(lamda)*Sin(Sqrt(lamda)*gridXV),
                            -Sqrt(lamda)*Cos(Sqrt(lamda)*gridXV),
                        }
                    };

                    //9
                    double[] right = new double[2]
                    {
                        Exp(spectrXV*gridXV),
                        spectrXV*Exp(spectrXV*gridXV)
                    };

                    AB = Linal.GaussSolve(left, right);

                    psi[n- numberOfSpectrs] = AB[0] * Cos(Sqrt(lamda) * gridXV) + AB[1] * Sin(Sqrt(lamda));
                    //psi[n- numberOfSpectrs] *= ;

                }
                else
                {
                    //10
                    double[][] left = new double[2][]
                    {
                        new double[] {
                            Sinh(Sqrt(-lamda)*gridXV),
                            Cosh(Sqrt(-lamda)*gridXV)
                        },
                        new double[]{
                            Sqrt(-lamda)*Cosh(Sqrt(-lamda)*gridXV),
                            Sqrt(-lamda)*Sinh(Sqrt(-lamda)*gridXV),
                        }
                    };

                    //9
                    double[] right = new double[2]
                    {
                        Exp(spectrXV*gridXV),
                        spectrXV*Exp(spectrXV*gridXV)
                    };

                    AB = Linal.GaussSolve(left, right);

                    psi[n - numberOfSpectrs] = AB[0] * Cosh(Sqrt(-lamda) * gridXV) + AB[1] * Sinh(Sqrt(-lamda));
                    //psi[n - numberOfSpectrs] /= 10;
                }
            }

            return psi;
        }

        public double[] Spectr3(
           double[] gridX,
           double[] spectrX,
           int numberOfNodes)
        {
            double temp = 0.0;
            double[] psi = new double[numberOfNodes];
            int from = 2 * numberOfNodes;
            int to = 
                //(2 * numberOfNodes + 99);
                3 * numberOfNodes - 1;

            for (int i = from; i < to; i++)
            {
                temp = spectrX[i] * gridX[i - from];
                psi[i - from] = -m_A0 * Exp(temp) + m_B0 * Exp(-temp);
            }
            return psi;
        }

        public double[] Spectr(
            double[] gridX,
            double[] spectrX)
        {
            double[] spectrY = new double[spectrX.Length];

            int numberOfSpectrs = spectrX.Length;
            int numberOfNodes   = gridX.Length;
            //spectr
            for (int s = 0; s < numberOfSpectrs; s++)
            {
                double gridXV = 0.0;
                double gridXVP = 0.0;
                double spectrXV = 0.0;
                double lamda = 0.0;
                double[] AB = new double[2];
                double[] psi = new double[numberOfSpectrs];
                double[] tempSpectrY = new double[spectrX.Length];
                double[][] left;
                tempSpectrY[0] = Exp(spectrX[0] * gridX[0]);
                //nodes
                for (int n = 1; n < (numberOfNodes - 1); n++)
                {
                    gridXV = gridX[n];
                    gridXVP = gridX[n];
                    spectrXV = spectrX[n];
                    lamda =
                       V((gridXV + gridXVP) / 2) - (spectrXV * spectrXV);

                    if (lamda > 0)
                    {
                        //10
                        left = new double[2][]
                        {
                        new double[] {
                            Sin(Sqrt(lamda)*gridXV),
                            Cos(Sqrt(lamda)*gridXV)
                        },
                        new double[]{
                             Sqrt(lamda)*Cos(Sqrt(lamda)*gridXV),
                            -Sqrt(lamda)*Sin(Sqrt(lamda)*gridXV),
                        }
                        };

                        //9
                        double[] right = new double[2]
                        {
                            Exp(spectrXV*gridXV),
                            spectrXV*Exp(spectrXV*gridXV)
                        };

                        AB = Linal.GaussSolve(left, right);

                        psi[n] = AB[0] * Cos(Sqrt(lamda) * gridXV) + AB[1] * Sin(Sqrt(lamda));

                    }
                    else
                    {
                        //10
                        left = new double[2][]
                        {
                        new double[] {
                            Sinh(Sqrt(-lamda)*gridXV),
                            Cosh(Sqrt(-lamda)*gridXV)
                        },
                        new double[]{
                            Sqrt(-lamda)*Cosh(Sqrt(-lamda)*gridXV),
                            Sqrt(-lamda)*Sinh(Sqrt(-lamda)*gridXV),
                        }
                        };

                        //9
                        double[] right = new double[2]
                        {
                            Exp(spectrXV*gridXV),
                            spectrXV*Exp(spectrXV*gridXV)
                        };

                        AB = Linal.GaussSolve(left, right);

                        psi[n] = Cosh(Sqrt(-lamda) * gridXV) + AB[1] * Sinh(Sqrt(-lamda));
                    }
                }
                
                left = new double[2][]
                {
                    new double[] { Exp( tempSpectrY[spectrX.Length - 1] * gridX[gridX.Length-1]) },
                    new double[] { Exp(-tempSpectrY[spectrX.Length - 1] * gridX[gridX.Length-1]) }
                };

                spectrY[s] = //Linal.GaussSolve(left, new double[] { tempSpectrY[spectrX.Length - 1] })[0];
                    tempSpectrY[spectrX.Length - 1] / Exp(tempSpectrY[spectrX.Length - 1] * gridX[gridX.Length - 1]);
            }

            return spectrY;
        }

        public void Resh()
        {
            double U0Max;
            {
                double[] v =
                    Linal.UseFunctionOnArrayR(
                        Linal.MakeArray(m_A0, m_B0 + 1, 1000),
                        V);
                U0Max = Linal.Max(  //U0Max = 5.0592738405662523
                    Linal.UseFunctionOnArrayR(v, Math.Abs));
            }

            //в зависимости от numberOfNodes
            int numberOfNodes   = 500; //50
            int numberOfSpectrs = 500; //500
            m_H = (m_B0 - m_A0) / numberOfNodes;

            m_SpectrX =
               Linal.MakeArray(
                   (U0Max / numberOfSpectrs),
                   //(U0Max / (numberOfSpectrs*3)),
                   //hSpectr, 
                   U0Max,
                   3*numberOfSpectrs);

            double[] gridX;
            double[] SpectrYTemp;

            //Spectr1
            gridX = Linal.MakeArray(m_Left, m_A0, numberOfNodes);
            SpectrYTemp = Spectr1(
                gridX, 
                m_SpectrX, 
                numberOfNodes);
            m_SpectrY = new double[SpectrYTemp.Length];
            System.Array.Copy(SpectrYTemp, m_SpectrY, SpectrYTemp.Length); 

            //Spectr2
            gridX = Linal.MakeArray(m_A0, m_B0, numberOfNodes);
            SpectrYTemp = Spectr2(
                gridX, 
                m_SpectrX, 
                numberOfSpectrs,
                numberOfNodes);
            m_SpectrY = Linal.MakeArray(m_SpectrY, SpectrYTemp);

            //Spectr3
            gridX = Linal.MakeArray(m_B0, m_Right, numberOfNodes);
            SpectrYTemp = Spectr3(gridX, m_SpectrX, numberOfNodes);
            m_SpectrY = Linal.MakeArray(m_SpectrY, SpectrYTemp);
        }

        public void ChartInit()
        {
            defChartValueMinX = -0.5;
            defChartValueMaxX = 1.5;
            defChartValueIntervalX = 0.25;

            defChartValueMinY = -0.5;
            defChartValueMaxY = 1.5;
            defChartValueIntervalY = 0.25;

            m_HiddenClicked = false;

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
            chart1.ChartAreas.Add("HiddenArea");
            chart1.Series.RemoveAt(0);
            chart1.Series.Add(new Series("x"));
            chart1.Series.Add(new Series("y"));
            chart1.Series.Add(new Series("left"));
            chart1.Series.Add(new Series("right"));
            chart1.Series.Add(new Series("V(x)"));
            chart1.Series.Add(new Series("Спектр"));
            chart1.Series.Add(new Series("1"));
            chart1.Series.Add(new Series("2"));
            chart1.Series.Add(new Series("3"));

            chart1.Series["x"].ChartType      = SeriesChartType.Line;
            chart1.Series["y"].ChartType      = SeriesChartType.Line;
            chart1.Series["left"].ChartType   = SeriesChartType.Line;
            chart1.Series["right"].ChartType  = SeriesChartType.Line;
            chart1.Series["V(x)"].ChartType   = SeriesChartType.Line;
            chart1.Series["Спектр"].ChartType = SeriesChartType.Line;
            chart1.Series["1"].ChartType = SeriesChartType.Line;
            chart1.Series["2"].ChartType = SeriesChartType.Line;
            chart1.Series["3"].ChartType = SeriesChartType.Line;

            chart1.Series["x"].Color      = System.Drawing.Color.Black;
            chart1.Series["y"].Color      = System.Drawing.Color.Black;
            chart1.Series["V(x)"].Color   = System.Drawing.Color.Blue;
            chart1.Series["Спектр"].Color = System.Drawing.Color.Indigo;
            chart1.Series["1"].Color = System.Drawing.Color.Red;
            chart1.Series["2"].Color = System.Drawing.Color.Red;
            chart1.Series["3"].Color = System.Drawing.Color.Red;

            chart1.Series["x"].ChartArea      = "ChartArea0";
            chart1.Series["y"].ChartArea      = "ChartArea0";
            chart1.Series["V(x)"].ChartArea   = "ChartArea0";
            chart1.Series["Спектр"].ChartArea = "ChartArea0";
            chart1.Series["1"].ChartArea = "ChartArea0";
            chart1.Series["2"].ChartArea = "ChartArea0";
            chart1.Series["3"].ChartArea = "ChartArea0";

            chart1.ChartAreas[1].Visible = false;
            //MAXIMUM
            
            //label3.Text = $"({gresult.kArray})";
            //label7.Text = $"({gresult.a})";

            int x_num = 100;
            
            //x, V(x)
            double[] x = new double[x_num];
            double[] Vx = new double[x_num];
            //0 array
            double[] nullArray = new double[x_num];
            //x & y
            double[] xLine = new double[x_num];
            double[] yLine = new double[x_num];
            //Spectr1
            double[] spectr1Line = new double[x_num];
            double[] spectr2Line = new double[x_num];
            double[] spectr3Line = new double[x_num];

            m_H = (m_B0 - m_A0) / x_num;

            x = Linal.MakeArray(m_Left, m_Right, 100);
            Vx = Linal.UseFunctionOnArrayR(x, V);
            nullArray = Linal.MakeArray(100);
            //x&y
            xLine = Linal.MakeArray(-100, 100, 100);
            yLine = Linal.MakeArray(-100, 100, 100);
            //Spectr1
            spectr1Line = Linal.MakeArray(100, m_SpectrX[498]);
            spectr2Line = Linal.MakeArray(100, m_SpectrX[998]);
            spectr3Line = Linal.MakeArray(100, m_SpectrX[1098]);
#if vx
            //V(x)
            chart1.Series["V(x)"].Points.DataBindXY(x, Vx);
#else
            //result
            chart1.Series["Спектр"].Points.DataBindXY(m_SpectrX, m_SpectrY);
#endif
            //x&y
            chart1.Series["x"].Points.DataBindXY(yLine, nullArray);
            chart1.Series["y"].Points.DataBindXY(nullArray, xLine);
            //spectr1
            chart1.Series["1"].Points.DataBindXY(spectr1Line, xLine);
            chart1.Series["2"].Points.DataBindXY(spectr2Line, xLine);
            chart1.Series["3"].Points.DataBindXY(spectr3Line, xLine);
        }

        public Form1()
        {
            InitializeComponent();
            
            Resh();
            ChartInit();
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
            if (chart1.ChartAreas[0].AxisX.Minimum > -30)
            {
                double xShift = (chart1.ChartAreas[0].AxisX.Maximum - chart1.ChartAreas[0].AxisX.Minimum) / 4;
                chart1.ChartAreas[0].AxisX.Maximum -= xShift;
                chart1.ChartAreas[0].AxisX.Minimum -= xShift;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (chart1.ChartAreas[0].AxisX.Maximum < 30)
            {
                double xShift = (chart1.ChartAreas[0].AxisX.Maximum - chart1.ChartAreas[0].AxisX.Minimum) / 4;
                chart1.ChartAreas[0].AxisX.Maximum += xShift;
                chart1.ChartAreas[0].AxisX.Minimum += xShift;
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

        private void button9_Click(object sender, EventArgs e)
        {
            if (!m_HiddenClicked)
            {
                chart1.Series["left"].ChartArea = "HiddenArea";
                chart1.Series["right"].ChartArea = "HiddenArea";
                chart1.Series["1"].ChartArea = "HiddenArea";
                chart1.Series["2"].ChartArea = "HiddenArea";
                chart1.Series["3"].ChartArea = "HiddenArea";
                button9.Text = "Show";
                m_HiddenClicked = true;
            }
            else
            {
                chart1.Series["left"].ChartArea =  "ChartArea0";
                chart1.Series["right"].ChartArea = "ChartArea0";
                chart1.Series["1"].ChartArea =     "ChartArea0";
                chart1.Series["2"].ChartArea =     "ChartArea0";
                chart1.Series["3"].ChartArea =     "ChartArea0";
                button9.Text = "Hide";
                m_HiddenClicked = false;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (chart1.ChartAreas[0].AxisY.Maximum < 2000)
            {
                double yShift = (chart1.ChartAreas[0].AxisY.Maximum - chart1.ChartAreas[0].AxisY.Minimum) / 4;
                chart1.ChartAreas[0].AxisY.Maximum += yShift;
                chart1.ChartAreas[0].AxisY.Minimum += yShift;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (chart1.ChartAreas[0].AxisY.Minimum > -200)
            {
                double yShift = (chart1.ChartAreas[0].AxisY.Maximum - chart1.ChartAreas[0].AxisY.Minimum) / 4;
                chart1.ChartAreas[0].AxisY.Minimum -= yShift;
                chart1.ChartAreas[0].AxisY.Maximum -= yShift;
            }
        }
        
    }
}

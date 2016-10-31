using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

using System;
using System.Windows;
using System.Collections.Generic;
using Windows.Foundation;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace newRTGraph
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            //drawExample();
            //drawLine();
            DrawChart();
        }
        private void drawExample()
        {
            // Create a StackPanel to contain the shape.
            StackPanel myStackPanel = new StackPanel();

            // Create a red Ellipse.
            Ellipse myEllipse = new Ellipse();
            //Line myLine = new Line();

            // Create a SolidColorBrush with a red color to fill the 
            // Ellipse with.
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();

            // Describes the brush's color using RGB values. 
            // Each value has a range of 0-255.
            mySolidColorBrush.Color = Color.FromArgb(255, 255, 255, 0);
            myEllipse.Fill = mySolidColorBrush;
            myEllipse.StrokeThickness = 2;
            //myEllipse.Stroke = Brushes.Black;

            // Set the width and height of the Ellipse.
            myEllipse.Width = 200;
            myEllipse.Height = 100;

            // Add the Ellipse to the StackPanel.
            myStackPanel.Children.Add(myEllipse);

            this.Content = myStackPanel;
        }
        private void drawLine()
        {
            // Create a StackPanel to contain the shape.
            // StackPanel myStackPanel = new StackPanel();
            //Canvas canvas = new Canvas();    // 새 캔버스 생성
            // Create a Line.
            Line myLine = new Line();

            // myLine.Stroke = LightSteelBlue;
            myLine.X1 = 1;
            myLine.X2 = 50;
            myLine.Y1 = 1;
            myLine.Y2 = 50;
            //          myLine.HorizontalAlignment = HorizontalAlignment.Left;
            //          myLine.VerticalAlignment = VerticalAlignment.Center;
            myLine.StrokeThickness = 2;
            //          myGrid.Children.Add(myLine);

            // Add the Ellipse to the StackPanel.
            //  myStackPanel.Children.Add(myLine);

            //   this.Content = myStackPanel;
            canGraph.Children.Add(myLine);    // 사각형을 캔버스에 추가
        }

        private void DrawChart()
        {

            canGraph.Children.Clear();
            double[] RectWall;
            RectWall = new double[4];
            RectWall[0] = 0;                // left 
            RectWall[1] = 0;                // top
            //RectWall[2] = canGraph.ActualWidth;   // right
            //RectWall[3] = canGraph.ActualHeight;  // bottom
            RectWall[2] = canGraph.Width;   // right
            RectWall[3] = canGraph.Height;  // bottom


            double[] RectGap;
            RectGap = new double[4];
            RectGap[0] = 30;    // left 
            RectGap[1] = 20;    // top
            RectGap[2] = 20;    // right
            RectGap[3] = 20;    // bottom

            double[] RectChart;
            RectChart = new double[4];
            RectChart[0] = RectWall[0] + RectGap[0]; // left
            RectChart[1] = RectWall[1] + RectGap[1]; // top
            RectChart[2] = RectWall[2] - RectGap[2]; // right
            RectChart[3] = RectWall[3] - RectGap[3]; // bottom

            double fChartWidth = RectChart[2] - RectChart[0];
            double fChartHeight = RectChart[3] - RectChart[1];

            int xGridNum = 5;
            int yGridNum = 10;

            double fGridGapX = fChartWidth / xGridNum;
            double fGridGapY = fChartHeight / yGridNum;

            // 차트 그리기.
            Rectangle pChartRect = new Rectangle();
            pChartRect.StrokeThickness = 1;
            pChartRect.Fill = new SolidColorBrush(Colors.Black);
            pChartRect.Stroke = new SolidColorBrush(Colors.Yellow);
            pChartRect.Width = fChartWidth;
            pChartRect.Height = fChartHeight;
            Canvas.SetLeft(pChartRect, RectChart[0]);
            Canvas.SetTop(pChartRect, RectChart[1]);

            canGraph.Children.Add(pChartRect);
            // x축 그리기.
            int iCount = 0;

            GeometryGroup xaxis_geom = new GeometryGroup();
            for (double x = 0; x <= fChartWidth; x += fGridGapX)
            {

                LineGeometry xtick = new LineGeometry();
                xtick.StartPoint = new Point(x + RectChart[0], RectChart[3] + 5);
                xtick.EndPoint = new Point(x + RectChart[0], RectChart[1]);
                xaxis_geom.Children.Add(xtick);



                TextBlock xlabel = new TextBlock();

                // int ivalue = (int)axisx_min + (int)(axisx_max - axisx_min) / xGridNum * iCount;
                int ivalue = 30 + (int)(300 - 30) / xGridNum * iCount;
                xlabel.Text = ivalue.ToString();
                xlabel.FontSize = 10;
                Canvas.SetLeft(xlabel, x + RectChart[0]);
                Canvas.SetTop(xlabel, RectChart[3] + 5);
                canGraph.Children.Add(xlabel);
                iCount = iCount + 1;

            }



            Path xaxis_path = new Path();
            xaxis_path.StrokeThickness = 1;
            xaxis_path.Stroke = new SolidColorBrush(Colors.Green);
            xaxis_path.Data = xaxis_geom;
            canGraph.Children.Add(xaxis_path);
            // y축 그리기.

            iCount = 0;

            GeometryGroup yxaxis_geom = new GeometryGroup();
            for (double y = 0; y <= fChartHeight; y += fGridGapY)
            {

                LineGeometry ytick = new LineGeometry();
                ytick.StartPoint = new Point(RectChart[0] - 5, RectChart[3] - y);
                ytick.EndPoint = new Point(RectChart[2], RectChart[3] - y);
                yxaxis_geom.Children.Add(ytick);

                TextBlock ylabel = new TextBlock();
                int ivalue = (int)10 + (int)(1000 - 10) / yGridNum * iCount;
                ylabel.Text = ivalue.ToString();
                ylabel.FontSize = 10;

                Canvas.SetLeft(ylabel, RectChart[0] - 20);
                Canvas.SetTop(ylabel, RectChart[3] - y - ylabel.FontSize);
                canGraph.Children.Add(ylabel);
                iCount = iCount + 1;

            }



            Path yaxis_path = new Path();
            yaxis_path.StrokeThickness = 1;
            yaxis_path.Stroke = new SolidColorBrush(Colors.Green);
            yaxis_path.Data = yxaxis_geom;
            canGraph.Children.Add(yaxis_path);
        }
        /*
        private void DrawChart(Canvas canGraph, List<NameValueItem>[] pList, double axisx_min, double axisx_max, double data_min, double data_max)

        {

            canGraph.Children.Clear();



            double[] RectWall;

            RectWall = new double[4];

            RectWall[0] = 0;                // left 

            RectWall[1] = 0;                // top

            RectWall[2] = canGraph.ActualWidth;   // right

            RectWall[3] = canGraph.ActualHeight;  // bottom



            double[] RectGap;

            RectGap = new double[4];

            RectGap[0] = 30;    // left 

            RectGap[1] = 20;    // top

            RectGap[2] = 20;    // right

            RectGap[3] = 20;    // bottom



            double[] RectChart;

            RectChart = new double[4];

            RectChart[0] = RectWall[0] + RectGap[0]; // left 

            RectChart[1] = RectWall[1] + RectGap[1]; // top

            RectChart[2] = RectWall[2] - RectGap[2]; // right

            RectChart[3] = RectWall[3] - RectGap[3]; // bottom



            double fChartWidth = RectChart[2] - RectChart[0];

            double fChartHeight = RectChart[3] - RectChart[1];



            int xGridNum = 5;

            int yGridNum = 10;

            double fGridGapX = fChartWidth / xGridNum;

            double fGridGapY = fChartHeight / yGridNum;



            // 차트 그리기.

            Rectangle pChartRect = new Rectangle();

            pChartRect.StrokeThickness = 1;

            pChartRect.Fill = new SolidColorBrush(Colors.Black);

            pChartRect.Stroke = new SolidColorBrush(Colors.Yellow);

            pChartRect.Width = fChartWidth;

            pChartRect.Height = fChartHeight;

            Canvas.SetLeft(pChartRect, RectChart[0]);

            Canvas.SetTop(pChartRect, RectChart[1]);

            canGraph.Children.Add(pChartRect);



            // x축 그리기.

            int iCount = 0;

            GeometryGroup xaxis_geom = new GeometryGroup();

            for (double x = 0; x <= fChartWidth; x += fGridGapX)

            {

                LineGeometry xtick = new LineGeometry();

                xtick.StartPoint = new Point(x + RectChart[0], RectChart[3] + 5);

                xtick.EndPoint = new Point(x + RectChart[0], RectChart[1]);

                xaxis_geom.Children.Add(xtick);



                TextBlock xlabel = new TextBlock();

                int ivalue = (int)axisx_min + (int)(axisx_max - axisx_min) / xGridNum * iCount;

                xlabel.Text = ivalue.ToString();

                xlabel.FontSize = 10;

                Canvas.SetLeft(xlabel, x + RectChart[0]);

                Canvas.SetTop(xlabel, RectChart[3] + 5);

                canGraph.Children.Add(xlabel);

                iCount = iCount + 1;

            }



            Path xaxis_path = new Path();

            xaxis_path.StrokeThickness = 1;

            xaxis_path.Stroke = new SolidColorBrush(Colors.Green);

            xaxis_path.Data = xaxis_geom;

            canGraph.Children.Add(xaxis_path);



            // y축 그리기.

            iCount = 0;

            GeometryGroup yxaxis_geom = new GeometryGroup();

            for (double y = 0; y <= fChartHeight; y += fGridGapY)

            {

                LineGeometry ytick = new LineGeometry();

                ytick.StartPoint = new Point(RectChart[0] - 5, RectChart[3] - y);

                ytick.EndPoint = new Point(RectChart[2], RectChart[3] - y);

                yxaxis_geom.Children.Add(ytick);



                TextBlock ylabel = new TextBlock();

                int ivalue = (int)data_min + (int)(data_max - data_min) / yGridNum * iCount;

                ylabel.Text = ivalue.ToString();

                ylabel.FontSize = 10;

                Canvas.SetLeft(ylabel, RectChart[0] - 20);

                Canvas.SetTop(ylabel, RectChart[3] - y - ylabel.FontSize);

                canGraph.Children.Add(ylabel);

                iCount = iCount + 1;

            }



            Path yaxis_path = new Path();

            yaxis_path.StrokeThickness = 1;

            yaxis_path.Stroke = new SolidColorBrush(Colors.Green);

            yaxis_path.Data = yxaxis_geom;

            canGraph.Children.Add(yaxis_path);



            // data 그리기.

            double x1 = 0;

            double y1 = 0;

            double x2 = 0;

            double y2 = 0;



            int idim = 0;

            idim = pList.Length;

            if (idim > 0)

            {

                for (int i = 0; i < idim; i++)

                {

                    GeometryGroup data_geom = new GeometryGroup();



                    List<NameValueItem> newitems = pList[i];

                    double xstep = fChartWidth / newitems.Count;



                    for (int j = 0; j < newitems.Count - 1; j++)

                    {

                        LineGeometry vline = new LineGeometry();



                        x1 = RectChart[0] + xstep * j;

                        y1 = RectChart[3] - fChartHeight * ((newitems[j].Value - data_min) / (data_max - data_min));

                        if (y1 < RectChart[1])

                            y1 = RectChart[1];

                        if (y1 > RectChart[3])

                            y1 = RectChart[3];

                        x2 = RectChart[0] + xstep * (j + 1);

                        y2 = RectChart[3] - fChartHeight * ((newitems[j + 1].Value - data_min) / (data_max - data_min));

                        if (y2 < RectChart[1])

                            y2 = RectChart[1];

                        if (y2 > RectChart[3])

                            y2 = RectChart[3];

                        vline.StartPoint = new Point(x1, y1);

                        vline.EndPoint = new Point(x2, y2);

                        data_geom.Children.Add(vline);

                    }



                    Path value_path = new Path();

                    value_path.StrokeThickness = 1;

                    if (i == 0)

                        value_path.Stroke = new SolidColorBrush(Colors.Red);

                    else if (i == 1)

                        value_path.Stroke = new SolidColorBrush(Colors.Green);

                    else if (i == 2)

                        value_path.Stroke = new SolidColorBrush(Colors.Blue);

                    else

                        value_path.Stroke = new SolidColorBrush(Colors.Yellow);

                    value_path.Data = data_geom;

                    canGraph.Children.Add(value_path);

                }

            }
            
        }
    */
    }
}

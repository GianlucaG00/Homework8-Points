using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using static System.Windows.Forms.LinkLabel;

namespace Homework_5
{
    public partial class Form1 : Form
    {
        Random r = new Random(); 
        Bitmap b, b1, b2;
        Graphics g, g1, g2;
        Rectangle virtualWindow, virtualWindow1, virtualWindow2;

        SortedDictionary<double, long> distX = new SortedDictionary<double, long>();
        SortedDictionary<double, long> distY = new SortedDictionary<double, long>();

        int num = 3000; 

        List<Rectangle> points = new List<Rectangle>();

        int x_mouse, y_mouse;
        int x_down, y_down;

        int r_width, r_height;

        bool drag = false;
        bool resizing = false;

        bool pictureBox1_MouseWheel_SR;

        Pen penRectangle = new Pen(Color.Black, 0.2f);

        int interval = 3;

        public Form1()
        {
            InitializeComponent();
            b = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(b);
            pictureBox1.Image = b;


            b1 = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            g1 = Graphics.FromImage(b1);
            pictureBox2.Image = b1;

            b2 = new Bitmap(pictureBox3.Width, pictureBox3.Height); 
            g2 = Graphics.FromImage(b2);
            pictureBox3.Image = b2;
            
            virtualWindow = new Rectangle(20, 20, b.Width - 20, b.Height - 20);
            virtualWindow1 = new Rectangle(20, 20, b1.Width - 20, b1.Height - 20);
            virtualWindow2 = new Rectangle(20, 20, b2.Width - 20, b2.Height - 20);

            //timer1.Enabled = true;
            //timer1.Interval = 16;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!virtualWindow.Contains(e.X, e.Y)) return;

            x_mouse = e.X;
            y_mouse = e.Y;

            x_down = virtualWindow.X;
            y_down = virtualWindow.Y;

            r_width = virtualWindow.Width;
            r_height = virtualWindow.Height;

            if (e.Button == MouseButtons.Left)
            {
                drag = true;
            }
            else if (e.Button == MouseButtons.Right)
            {
                resizing = true;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            drag = false;
            resizing = false;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (b == null) return;

            int delta_x = e.X - x_mouse;
            int delta_y = e.Y - y_mouse;



            if (drag)
            {
                virtualWindow.X = x_down + delta_x;
                virtualWindow.Y = y_down + delta_y;
                if (virtualWindow.X < 0) virtualWindow.X = 0;
                if (virtualWindow.Y < 0) virtualWindow.Y = 0;
                if (virtualWindow.X > pictureBox1.Width - virtualWindow.Width) virtualWindow.X = pictureBox1.Width - virtualWindow.Width;
                if (virtualWindow.Y > pictureBox1.Height - virtualWindow.Height) virtualWindow.Y = pictureBox1.Height - virtualWindow.Height;
            }
            else if (resizing)
            {

                virtualWindow.Width = r_width + delta_x;
                virtualWindow.Height = r_height + delta_y;
                if (virtualWindow.Width < 100) virtualWindow.Width = 100;
                if (virtualWindow.Height < 100) virtualWindow.Height = 100;
                if (virtualWindow.Width > pictureBox1.Width - 20) virtualWindow.Width = pictureBox1.Width - 20;
                if (virtualWindow.Height > pictureBox1.Height - 20) virtualWindow.Height = pictureBox1.Height - 20;
            }

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            num = trackBar1.Value;
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!(ModifierKeys == Keys.Control)) return;
            if (pictureBox1_MouseWheel_SR) return;

            pictureBox1_MouseWheel_SR = true;

            float stepZoom;
            if (ModifierKeys == (Keys.Shift | Keys.Control))
            {
                stepZoom = 0.01F;
            }
            else
            {
                stepZoom = 0.1F;
            }

            virtualWindow.Inflate((int)(e.Delta * stepZoom), (int)(e.Delta * stepZoom));

            if (virtualWindow.Width < 100) virtualWindow.Width = 100;
            if (virtualWindow.Height < 100) virtualWindow.Height = 100;
            if (virtualWindow.Width > pictureBox1.Width - 20) virtualWindow.Width = pictureBox1.Width - 20;
            if (virtualWindow.Height > pictureBox1.Height - 20) virtualWindow.Height = pictureBox1.Height - 20;
            pictureBox1_MouseWheel_SR = false;

        }

        private void generateHistogram(Rectangle r, SortedDictionary<double, long> d, Graphics g, int interval, bool vertical = false)
        {

            if (d == null || d.Count == 0) return;
            int n = d.Count;


            double maxKey = d.Keys.Max();
            double maxValue = d.Values.Max();
            for (int i = 0; i < n; i++)
            {
                Rectangle rr;
                int left, top, right, bottom;
                if (vertical)
                {
                    // vertical histogram
                    left = fromXRealToXVirtual(0, 0, maxValue, r.Left, r.Width);
                    top = fromYRealToYVirtual(i + 1, 0, n, r.Top, r.Height);
                    right = fromXRealToXVirtual(d.ElementAt(i).Value, 0, maxValue, r.Left, r.Width);
                    bottom = fromYRealToYVirtual(i, 0, n, r.Top, r.Height);
                }
                else
                {
                    // horizontal histogram
                    left = fromXRealToXVirtual(i, 0, n, r.Left, r.Width);
                    top = fromYRealToYVirtual(d.ElementAt(i).Value, 0, maxValue, r.Top, r.Height);
                    right = fromXRealToXVirtual(i + 1, 0, n, r.Left, r.Width);
                    bottom = fromYRealToYVirtual(0, 0, maxValue, r.Top, r.Height);
                }
                rr = Rectangle.FromLTRB(left, top, right, bottom);

                g.DrawRectangle(penRectangle, rr);
                g.FillRectangle(new SolidBrush(Color.FromArgb(180, 200, 89, 0)), rr);

                // g.DrawString(vertical ? Math.Round(d.ElementAt(i).Key*interval, 1).ToString() : Math.Round((double)d.ElementAt(i).Value, 1).ToString(), DefaultFont, Brushes.Black, r.Right, vertical ? top : top);
                // g.DrawString(vertical ? Math.Round((double)d.ElementAt(i).Value, 1).ToString() : Math.Round(d.ElementAt(i).Key*interval, 1).ToString(), DefaultFont, Brushes.Black, vertical ? right: left, r.Bottom);
            }
        }

        private double[] fromPolarToCartesian(double module, double angle)
        {
            double[] res = new double[2];

            res[0] = module * Math.Cos(angle); // x
            res[1] = module * Math.Sin(angle); // y

            return res; 

        }
        private void drawPoints()
        {
            int i; 

            foreach(Rectangle rec in points)
            {
                g.DrawEllipse(new Pen(Color.Green), rec);
                g.FillEllipse(new SolidBrush(Color.Green), rec);
            }
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            redraw();
        }



        // It generates the gaussian distribution 
        private void button2_Click(object sender, EventArgs e)
        {
            distX.Clear();
            distY.Clear();
            points.Clear();

            textBox1.Text = num.ToString(); 
            for(int i=0; i<num; i++)
            {
                double module = r.NextDouble();
                double angle = 2 * Math.PI * r.NextDouble();
                double[] res = fromPolarToCartesian(module, angle);

                int x = fromXRealToXVirtual(res[0], -1, 1, 0, b.Width);
                int y = fromYRealToYVirtual(res[1], 1, -1, 0, b.Height);

                points.Add(new Rectangle(x, y, 8, 8));

                double xx = Math.Round(res[0], 2);
                double yy = Math.Round(res[1], 2);

                if (distX.ContainsKey(xx))
                {
                    distX[xx]++;
                }
                else
                {
                    distX.Add(xx, 1);
                }

                if (distY.ContainsKey(yy))
                {
                    distY[yy]++;
                }
                else
                {
                    distY.Add(yy, 1);
                }
            }

            g.Clear(BackColor);
            g1.Clear(BackColor);
            g2.Clear(BackColor);

            generateHistogram(virtualWindow1, distX, g1, interval, true);
            generateHistogram(virtualWindow2, distY, g2, interval, false);
            drawPoints();

            pictureBox1.Image = b; 
            pictureBox2.Image = b1;
            pictureBox3.Image = b2;


        }


        private int fromXRealToXVirtual(double x, double minX, double maxX, int left, int w)
        {
            return left + (int)(w * (x - minX) / (maxX - minX));
        }

        private int fromYRealToYVirtual(double y, double minY, double maxY, int top, int h)
        {
            return top + (int)(h - h * (y - minY) / (maxY - minY));
        }

        private void redraw()
        {

            g.Clear(BackColor);
            generateHistogram(virtualWindow1, distX, g1, interval, true);
            generateHistogram(virtualWindow2, distY, g2, interval, false); 
            drawPoints();

            pictureBox1.Image = b;

        }
    }
}
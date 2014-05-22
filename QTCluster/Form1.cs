using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QTCluster
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// The maximum X value of a point
        /// </summary>
        private const int MaxPointX = 600;

        /// <summary>
        /// The maximum Y value of a point
        /// </summary>
        private const int MaxPointY = 400;

        /// <summary>
        /// The number of points to generate
        /// </summary>
        private const int PointsToGenerate = 200;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            // Clear the points collection and canvas
            btnClear_Click(sender, e);

            // Generate a random collection of points
            Random rand = new Random();
            for (var i = 0; i < PointsToGenerate; i++)
            {
                var p = new Point(rand.Next(MaxPointX), rand.Next(MaxPointY));
                lstPoints.Items.Add(p);
            }

            btnCluster.Enabled = true;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            // Clear points collection and clear canvas
            lstPoints.Items.Clear();

            using (var g = CreateGraphics())
            {
                g.Clear(Color.FromKnownColor(KnownColor.Control));
            }

            btnCluster.Enabled = false;
        }

        private void btnCluster_Click(object sender, EventArgs e)
        {
            List<Point> generatedPoints = lstPoints.Items.Cast<Point>().ToList();

            // Get the clusters, this is where all the magic happens
            var qtc = Clustering.QualityThresholdCluster.GetClusters(generatedPoints, (double)numericUpDown1.Value);

            // Draw the clustered points and the average point within that cluster
            using (var g = CreateGraphics())
            {
                g.Clear(Color.FromKnownColor(KnownColor.Control));

                Random rand = new Random(100);

                foreach (var clusterPoint in qtc)
                {
                    // Generate a random color for this cluster
                    Color c = Color.FromArgb(rand.Next(70, 200), rand.Next(100, 225), rand.Next(100, 230));

                    // Go through each point in the cluster and draw the point
                    foreach (var points in clusterPoint)
                    {
                        points.ForEach(
                            point =>
                            {
                                Rectangle originalPointRect = new Rectangle(point.X, point.Y, 10, 10);
                                g.FillEllipse(new SolidBrush(c), originalPointRect);
                            });
                    }

                    // Draw the average point in this cluster
                    Rectangle clusteredPointRect = new Rectangle(clusterPoint.Key.X, clusterPoint.Key.Y, 10, 10);
                    g.FillRectangle(new SolidBrush(Color.Red), clusteredPointRect);
                }
            }
        }
    }
}

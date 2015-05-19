using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Otsu_Thresholding
{
    public partial class MainWindow : Window
    {
        private string filename;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            Nullable<bool> result = ofd.ShowDialog();
            if (result == true)
            {
                filename = ofd.FileName;
                Bitmap BitmapOriginal = new Bitmap(ofd.FileName);
                ImageOriginal.Source = convertBitmapToBitmapImage(convertToGrayscale(BitmapOriginal));
            }
        }

        private Bitmap convertToGrayscale(Bitmap BitmapOriginal)
        {
            for (int i = 0; i < BitmapOriginal.Width; i++)
            {
                for (int j = 0; j < BitmapOriginal.Height; j++)
                {
                    Color color = BitmapOriginal.GetPixel(i, j);
                    int gray = (int)((color.R * 0.3) + (color.G * 0.59) + (color.B * 0.11));
                    Color newColor = Color.FromArgb(gray, gray, gray);
                    BitmapOriginal.SetPixel(i, j, newColor);
                }
            }
            return BitmapOriginal;
        }

        private BitmapImage convertBitmapToBitmapImage(Bitmap Bitmap)
        {
            MemoryStream ms = new MemoryStream();
            Bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }

        private void ButtonThreshold_Click(object sender, RoutedEventArgs e)
        {
            if (ImageOriginal.Source == null)
            {
                MessageBox.Show("Please browse an image first");
                return;
            }
            else
            {
                Bitmap BitmapOriginal = new Bitmap(filename);
                Bitmap BitmapEdited = new Bitmap(BitmapOriginal.Width, BitmapOriginal.Height);
                BitmapOriginal = convertToGrayscale(BitmapOriginal);
                BitmapEdited = applyOtsuThreshold(BitmapOriginal);
                ImageEdited.Source = convertBitmapToBitmapImage(BitmapEdited);
            }
        }

        private Bitmap applyOtsuThreshold(Bitmap BitmapOriginal)
        {
            double[] histogram = new double[256];
            double[] normalizedhistogram = new double[256];

            for (int i = 0; i < histogram.Length; i++)
            {
                histogram[i] = 0;
                normalizedhistogram[i] = 0;
            }

            for (int i = 1; i < BitmapOriginal.Width; i++)
            {
                for (int j = 1; j < BitmapOriginal.Height; j++)
                {
                    Color tempColor = BitmapOriginal.GetPixel(i, j);
                    int temp = tempColor.R;
                    histogram[temp]++;
                }
            }
            double cumulative = histogram.Sum();
            for (int i = 1; i < histogram.Length; i++)
            {
                normalizedhistogram[i] = histogram[i] / cumulative;
            }
            double mG = 0;
            for (int i = 0; i < normalizedhistogram.Length; i++)
            {
                mG = mG + (i * normalizedhistogram[i]);
            }
            List<double> matrixBCV = new List<double>();
            List<double> listnormalizedhistogram = normalizedhistogram.ToList();
            for (int k = 1; k < normalizedhistogram.Length; k++)
            {
                double P1 = normalizedhistogram.Take(k).Sum();
                double P2 = normalizedhistogram.Skip(k + 1).Take(normalizedhistogram.Length - (k + 1)).Sum();
                double smallP1 = 0;
                double smallP2 = 0;
                for (int i = 1; i < k; i++)
                {
                    smallP1 = smallP1 + (i * normalizedhistogram[i]);
                }
                double m1 = smallP1 / P1;
                for (int i = k + 1; i < normalizedhistogram.Length; i++)
                {
                    smallP2 = smallP2 + (i * normalizedhistogram[i]);
                }
                double m2 = smallP2 / P2;
                double BCV = P1 * Math.Pow((m1 - mG), 2) + P2 * Math.Pow((m2 - mG), 2);
                matrixBCV.Add(BCV);
            }
            int optimal = matrixBCV.IndexOf(matrixBCV.Max());
            Bitmap BitmapEdited = new Bitmap(BitmapOriginal.Width, BitmapOriginal.Height);
            for (int i = 0; i < BitmapEdited.Width; i++)
            {
                for (int j = 0; j < BitmapEdited.Height; j++)
                {
                    if (BitmapOriginal.GetPixel(i, j).G > optimal)
                    {
                        BitmapEdited.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                    }
                    else
                    {
                        BitmapEdited.SetPixel(i, j, Color.FromArgb(255, 0, 0, 0));
                    }
                }
            }
            return BitmapEdited;
        }
    }
}

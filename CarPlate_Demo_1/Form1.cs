using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.UI;
using Emgu.CV.Structure;

namespace CarPlate_Demo_1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                Image<Bgr, byte> My_Image = new Image<Bgr, byte>(Openfile.FileName);
                Image<Gray, byte> gray_image = My_Image.Convert<Gray, byte>();
                Image<Gray, byte> eh_gray_image = My_Image.Convert<Gray, byte>();
                Image<Gray, byte> smooth_gray_image = My_Image.Convert<Gray, byte>();
                Image<Gray, byte> ed_gray_image = new Image<Gray, byte>(gray_image.Size);
                Image<Bgr, byte> final_image = new Image<Bgr, byte>(Openfile.FileName);
                MemStorage stor = new MemStorage();
                List<MCvBox2D> detectedLicensePlateRegionList = new List<MCvBox2D>();


                

                CvInvoke.cvEqualizeHist(gray_image, eh_gray_image);
                CvInvoke.cvSmooth(eh_gray_image, smooth_gray_image, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_GAUSSIAN, 3, 3, 0, 0);
                //CvInvoke.cvAdaptiveThreshold(smooth_gray_image, bi_gray_image, 255, Emgu.CV.CvEnum.ADAPTIVE_THRESHOLD_TYPE.CV_ADAPTIVE_THRESH_GAUSSIAN_C, Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY, 71, 1);
                CvInvoke.cvCanny(smooth_gray_image, ed_gray_image, 100, 50, 3);
                Contour<Point> contours = ed_gray_image.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_TREE, stor);
                DetectPlate(contours, detectedLicensePlateRegionList);

                for (int i = 0; i < detectedLicensePlateRegionList.Count; i++)
                {
                    final_image.Draw(detectedLicensePlateRegionList[i], new Bgr(Color.Red), 2);
                }
                imageBox1.Image = My_Image;
                imageBox2.Image = gray_image;
                imageBox3.Image = eh_gray_image;
                imageBox4.Image = smooth_gray_image;
                imageBox5.Image = ed_gray_image;
                imageBox6.Image = final_image;
            }
        }




        private static int GetNumberOfChildren(Contour<Point> contours)
        {
            Contour<Point> child = contours.VNext;
            if (child == null) return 0;
            int count = 0;
            while (child != null)
            {
                count++;
                child = child.HNext;
            }
            return count;
        }


        private void DetectPlate(Contour<Point> contours, List<MCvBox2D> detectedLicensePlateRegionList)
        {
            for (; contours != null; contours = contours.HNext)
            {
                int numberOfChildren = GetNumberOfChildren(contours);
                if (numberOfChildren == 0) continue;

                if (contours.Area > 400)
                {
                    if (numberOfChildren < 3)
                    {
                        DetectPlate(contours.VNext, detectedLicensePlateRegionList);
                        continue;
                    }

                    MCvBox2D box = contours.GetMinAreaRect();
                    if (box.angle < -45.0)
                    {
                        float tmp = box.size.Width;
                        box.size.Width = box.size.Height;
                        box.size.Height = tmp;
                        box.angle += 90.0f;
                    }
                    else if (box.angle > 45.0)
                    {
                        float tmp = box.size.Width;
                        box.size.Width = box.size.Height;
                        box.size.Height = tmp;
                        box.angle -= 90.0f;
                    }

                    double whRatio = (double)box.size.Width / box.size.Height;
                    if (!(3.0 < whRatio && whRatio < 10.0))
                    {
                        Contour<Point> child = contours.VNext;
                        if (child != null)
                            DetectPlate(child, detectedLicensePlateRegionList);
                        continue;
                    }
                    detectedLicensePlateRegionList.Add(box);
                }
            }
        }


    }
}

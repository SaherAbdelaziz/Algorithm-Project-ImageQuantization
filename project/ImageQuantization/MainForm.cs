using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ImageQuantization
{
    public partial class MainForm : Form
    {
        List<RGBPixel> DistinctColors = new List<RGBPixel>();
        List<GraphOperations.Edgee> mst = new List<GraphOperations.Edgee>();
        string globalpathforimage;
        public MainForm()
        {
            InitializeComponent();
        }

        public static RGBPixel[,] ImageMatrix;
        public static RGBPixel[,] Result;
        private void btnOpen_Click(object sender, EventArgs e)
        {
            textBox1.Text = "0";
            textBox2.Text = "0";
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    //Open the browsed image and display it
                    string OpenedFilePath = openFileDialog1.FileName;
                    globalpathforimage = OpenedFilePath;
                    ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                    ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
                    pictureBox1.Visible = true;
                }
                txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
                txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
                
            }

            catch
            {
                MessageBox.Show("Please Select a photo");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            try
            {
                GraphOperations.GlobalData.setData();
                List<RGBPixel> test = new List<RGBPixel>();
                DistinctColors = GraphOperations.getD(test);
                List<GraphOperations.Edgee> testt = new List<GraphOperations.Edgee>();
                mst = GraphOperations.GetMST(DistinctColors, testt);
                
                long sz = GraphOperations.GlobalData.DC;
                double sz2 = GraphOperations.GlobalData.mstcost;
                textBox1.Text = sz.ToString();
                textBox2.Text = sz2.ToString();
            }
            catch
            {
                MessageBox.Show("Please Select a Photo");
            }


        } /// mst


        // clusters
        private void btnGaussSmooth_Click(object sender, EventArgs e) 
        {

            try
            {
                if (txtKC.Text != "0")
                {
                    int Kclusters = int.Parse(txtKC.Text);
                    Result = ImageOperations.OpenImage(globalpathforimage);
                    GraphOperations.Kcluster(Kclusters, DistinctColors, mst);

                    

                    ImageOperations.DisplayImage(Result, pictureBox2);
                    pictureBox2.Visible = true;
                }
                else
                {
                    MessageBox.Show("Clusers Can't be Zero");
                }

            }
            catch
            {
                MessageBox.Show("Please Select a Photo");   
            }
           
        }

        private void button2_Click(object sender, EventArgs e)
        {

            SaveFileDialog openFileDialog1 = new SaveFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string OpenedFilePath = openFileDialog1.FileName;
                OpenedFilePath = OpenedFilePath + ".bmp";
                pictureBox2.Image.Save(OpenedFilePath);

            }
        } //save

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        // exit

        private void frm_menu_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void txtKC_TextChanged(object sender, EventArgs e)
        {
            if(int.Parse(txtKC.Text)>DistinctColors.Count)
            {
                txtKC.Text = DistinctColors.Count.ToString();
            }
        }
        //Close form


      }
}
using System;
using System.Drawing;
using System.Windows.Forms;
using Tesseract;

namespace _20260210ocr
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                var img = (Bitmap)Clipboard.GetImage();

                using (var tesseract = new TesseractEngine(@"./", "jpn"))
                {
                    Pix pix = PixConverter.ToPix(img);
                    Page page = tesseract.Process(pix);
                    textBox1.Text = page.GetText().Replace("\n", "\r\n");
                }
            }

        }
    }
}

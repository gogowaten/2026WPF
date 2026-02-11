using Windows.Graphics.Imaging;
using Windows.Media.Ocr;

// Windowsに標準で搭載されているOCR機能を使ってC#で簡単なOCRアプリを作ってみる - パソコン関連もろもろ
// https://touch-sp.hatenablog.com/entry/2026/02/09/182536
namespace _20260211_ocr
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Clipboard.ContainsImage())
                {
                    MessageBox.Show("画像がない"); return;
                }

                using Bitmap? bmp = (Bitmap?)Clipboard.GetImage();
                if (bmp == null) return;

                var engine = OcrEngine.TryCreateFromUserProfileLanguages();
                if (engine == null)
                {
                    MessageBox.Show("OCRエンジンを起動できなかった");
                    return;
                }

                using var stream = new MemoryStream();
                bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;

                var decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
                using var softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                pictureBox1.Image = bmp;

                var result = await engine.RecognizeAsync(softwareBitmap);

                if (result != null) { textBox1.Text = result.Text.Trim(); }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"エラー発生：{ex.Message}");
                throw;
            }
        }

    }
}

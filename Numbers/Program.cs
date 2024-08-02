using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;

class Program
{
    static void Main()
    {
        int width = 1000;
        int height = 100;
        int digitWidth = width / 10;

        using (Bitmap bitmap = new Bitmap(width, height))
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);

                using (Font font = new Font("Arial", 72, FontStyle.Bold, GraphicsUnit.Pixel))
                {
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                    for (int i = 0; i < 10; i++)
                    {
                        string digit = i.ToString();
                        SizeF size = g.MeasureString(digit, font);

                        float x = i * digitWidth + (digitWidth - size.Width) / 2;
                        float y = (height - size.Height) / 2;

                        using (Brush brush = new SolidBrush(Color.White))
                        {
                            g.DrawString(digit, font, brush, x, y);
                        }
                    }
                }
            }

            bitmap.Save("numbers.png", ImageFormat.Png);
        }

        Console.WriteLine("numbers.png has been created successfully.");
    }
}
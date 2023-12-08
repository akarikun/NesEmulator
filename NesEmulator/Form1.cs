using Microsoft.Win32;
using NesEmulator.Nes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NesEmulator
{
    public partial class Form1 : Form
    {
        ByteCode code = new ByteCode();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //ByteCode code = new ByteCode();
            //this.Text = code.IsValid(0x69).ToString();
            //this.Text = code.IsValid("AND",0x29).ToString();

            pictureBox1.Paint += PictureBox1_Paint;
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            // 在内存中创建一个和PictureBox一样大的bitmap
            using (var buffer = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height))
            {
                using (var g = Graphics.FromImage(buffer))
                {
                    // 在这里进行你的绘制逻辑
                    g.Clear(Color.Black);
                    Paint(g);
                    // 将内存中的内容绘制到PictureBox上
                    e.Graphics.DrawImageUnscaled(buffer, 0, 0);
                }
            }
        }

        new void Paint(Graphics g)
        {
            var colorFn = new Func<byte, Color>((byte b) =>
            {
                /*
                内存$200到$5ff映射到屏幕的像素,其中的值带班不同的颜色:

                $0: Black(黑)
                $1: White(白)
                $2: Red(红)
                $3: Cyan(蓝绿)
                $4: Purple(紫)
                $5: Green(绿)
                $6: Blue(蓝)
                $7: Yellow(黄)
                $8: Orange(橙)
                $9: Brown(棕色)
                $a: Light red(淡红)
                $b: Dark grey(深灰)
                $c: Grey(灰色)
                $d: Light green(淡绿)
                $e: Light blue(淡蓝)
                $f: Light grey(淡灰)
                */
                switch (b)
                {
                    case 0x00: return Color.Black;
                    case 0x01: return Color.White;
                    case 0x02: return Color.Red;
                    case 0x03: return Color.Cyan;
                    case 0x04: return Color.Purple;
                    case 0x05: return Color.Green;
                    case 0x06: return Color.Blue;
                    case 0x07: return Color.Yellow;
                    case 0x08: return Color.Orange;
                    case 0x09: return Color.Brown;
                    case 0x0a: return Color.LightPink;
                    case 0x0b: return Color.DarkGray;
                    case 0x0c: return Color.Gray;
                    case 0x0d: return Color.LightGreen;
                    case 0x0e: return Color.LightBlue;
                    case 0x0f: return Color.LightGray;
                }
                return Color.Black;
            });
            for (var i = 0x200; i <= 0x05ff; i++)
            {
                var wh = 10;
                var col = (i - 0x200) / 32;
                var row = (i - 0x200) % 32;
                g.FillRectangle(new SolidBrush(colorFn(code.addr[i])), new RectangleF(row * wh, col * wh, wh, wh));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
        byte[] GetInputByte()
        {
            return richTextBox1.Text.Trim().Split(' ')
                        .Select(hex => Convert.ToByte(hex, 16))
                        .ToArray();
        }

        void AddressLog(int index, int length)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                sb.Append(code.addr[index + i].ToString("X2") + " ");
            }
            Console.WriteLine("Address-log");
            Console.WriteLine(sb);
        }

        private void Test1()
        {
            /*
            0600: a9 01 8d 00 02 a9 05 8d 01 02 a9 08 8d 02 02 
            Address  Hexdump   Dissassembly
            -------------------------------
            $0600    a9 01     LDA #$01
            $0602    8d 00 02  STA $0200
            $0605    a9 05     LDA #$05
            $0607    8d 01 02  STA $0201
            $060a    a9 08     LDA #$08
            $060c    8d 02 02  STA $0202
                        */
            var buffer = new byte[] { 0xa9, 0x01, 0x8d, 0x00, 0x02, 0xa9, 0x05, 0x8d, 0x01, 0x02, 0xa9, 0x08, 0x8d, 0x02, 0x02 };
            //var buffer = GetInputByte();

            code.Execute(buffer, (addr, cd, parm) =>
            {
                Console.WriteLine("{0},params:({1})", cd.ToString("X2"), parm);
                if (cd == 0x8d)
                {
                    this.pictureBox1.BeginInvoke(new Action(() =>
                    {
                        this.pictureBox1.Invalidate();
                    }));
                }
            });

            richTextBox2.Clear();
            richTextBox2.AppendText(@"
Stopped

Program end at PC=$" + code.register.PC.ToString("X4"));
        }
        private void Test2()
        {
            /*
Address  Hexdump   Dissassembly
-------------------------------
$0600    a2 08     LDX #$08
$0602    ca        DEX 
$0603    8e 00 02  STX $0200
$0606    e0 03     CPX #$03
$0608    d0 f8     BNE $0602
$060a    8e 01 02  STX $0201
$060d    00        BRK 
             */
            var buffer = new byte[] { 0xa2, 0x08, 0xca, 0x8e, 0x00, 0x02, 0xe0, 0x03, 0xd0, 0xf8, 0x8e, 0x01, 0x02, 0x00 };
            //var buffer = GetInputByte();

            code.Execute(buffer, (addr, cd, parm) =>
            {
                Console.WriteLine("{0},params:({1})", cd.ToString("X2"), parm);
                if (cd == 0x8d)
                {
                    this.pictureBox1.BeginInvoke(new Action(() =>
                    {
                        this.pictureBox1.Invalidate();
                    }));
                }
                else if(cd == 0xe0)
                {
                    Console.WriteLine(code.register.P);
                }
            });

            richTextBox2.Clear();
            richTextBox2.AppendText(@"
Stopped

Program end at PC=$" + code.register.PC.ToString("X4") + @"
");
            AddressLog(0x200, 10);
        }
        private void button2_Click(object sender, EventArgs e)
        {
        	Test1();
        }
    }
}

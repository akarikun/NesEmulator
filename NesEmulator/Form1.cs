using NesEmulator.Nes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NesEmulator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //ByteCode code = new ByteCode();
            //this.Text = code.IsValid(0x69).ToString();
            //this.Text = code.IsValid("AND",0x29).ToString();
            Test();
        }
        public void Test()
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
            ByteCode code = new ByteCode();
            code.Execute(buffer);
        }
    }
}

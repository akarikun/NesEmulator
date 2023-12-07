using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesEmulator.Nes
{
    internal class Register
    {
        public byte A { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }
        public ushort PC { get; set; }
        public byte S { get; set; }
        /// <summary>
        /// FLAG寄存器
        /// </summary>
        public byte P { get; set; }
        public int P_N { get { return (P & 0b10000000); } }
        public int P_V { get { return (P & 0b01000000); } }
        public int P_D { get { return (P & 0b00010000); } }
        public int P_B { get { return (P & 0b00001000); } }
        public int P_I { get { return (P & 0b00000100); } }
        public int P_Z { get { return (P & 0b00000010); } }
        public int P_C { get { return (P & 0b00000001); } }

    }
}

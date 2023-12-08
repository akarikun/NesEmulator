using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NesEmulator.Nes
{
    internal enum ERegisterFlag
    {
        C, Z, I, B, D, V = 6, N
    }
    internal class Register
    {
        public byte A { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }
        public ushort PC { get; set; }
        public byte S { get; set; }
        public byte P { get; set; }
        public void SetFlag(ERegisterFlag flag, bool b)
        {
            P |= (byte)((b ? 1 : 0) << (int)flag);
        }
        public int GetFlag(ERegisterFlag flag)
        {
            return P & 1 << (int)flag;
        }
    }
}

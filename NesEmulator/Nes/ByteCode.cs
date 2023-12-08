using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NesEmulator.Nes
{
	[AttributeUsage(AttributeTargets.Property)]
	internal class ByteCodeLengthAttribute : Attribute
	{
		public int Length { get; set; }
		public ByteCodeLengthAttribute(int length)
		{
			Length = length;
		}

		public ByteCodeLengthAttribute()
		{

		}
	}
	internal class ByteCode
	{
		#region
		public byte[] ADC { get { return new byte[] { 0x69, 0x65, 0x75, 0x6D, 0x7D, 0x79, 0x61, 0x71 }; } }
		public byte[] AND { get { return new byte[] { 0x29, 0x25, 0x35, 0x2D, 0x3D, 0x39, 0x21, 0x31 }; } }
		public byte[] ASL { get { return new byte[] { 0x0A, 0x06, 0x16, 0x0E, 0x1E }; } }
		public byte[] BCC { get { return new byte[] { 0x90 }; } }
		public byte[] BCS { get { return new byte[] { 0xB0 }; } }
		public byte[] BEQ { get { return new byte[] { 0xF0 }; } }
		public byte[] BIT { get { return new byte[] { 0x24 }; } }
		public byte[] BMI { get { return new byte[] { 0x30 }; } }
		public byte[] BNE { get { return new byte[] { 0xD0 }; } }
		public byte[] BPL { get { return new byte[] { 0x10 }; } }
		public byte[] BRK { get { return new byte[] { 0x00 }; } }
		public byte[] BVC { get { return new byte[] { 0x50 }; } }
		public byte[] BVS { get { return new byte[] { 0x70 }; } }

		public byte[] CLC { get { return new byte[] { 0x18 }; } }
		public byte[] CLD { get { return new byte[] { 0xD8 }; } }
		public byte[] CLI { get { return new byte[] { 0x58 }; } }
		public byte[] CLV { get { return new byte[] { 0xB8 }; } }
		public byte[] CMP { get { return new byte[] { 0xC9, 0xC5, 0xD5, 0xCD, 0xDD, 0xD9, 0xC1, 0xD1 }; } }
		public byte[] CPX { get { return new byte[] { 0xE0 }; } }
		public byte[] CPY { get { return new byte[] { 0xC0 }; } }
		public byte[] DEC { get { return new byte[] { 0xC6, 0xD6, 0xCE, 0xDE }; } }
		public byte[] DEX { get { return new byte[] { 0xCA }; } }
		public byte[] DEY { get { return new byte[] { 0x88 }; } }
		public byte[] EOR { get { return new byte[] { 0x49, 0x45, 0x55, 0x4D, 0x5D, 0x59, 0x41, 0x51 }; } }
		public byte[] INC { get { return new byte[] { 0xE6, 0xF6, 0xEE, 0xFE }; } }
		public byte[] INX { get { return new byte[] { 0xE8 }; } }
		public byte[] INY { get { return new byte[] { 0xC8 }; } }
		public byte[] JMP { get { return new byte[] { 0x4C, 0x6C }; } }
		public byte[] JSR { get { return new byte[] { 0x20 }; } }
		[ByteCodeLength(2)]
		public byte[] LDA { get { return new byte[] { 0xA9, 0xA5, 0xB5, 0xAD, 0xBD, 0xB9, 0xA1, 0xB1 }; } }
		public byte[] LDX { get { return new byte[] { 0xA2, 0xA6, 0xB6, 0xAE, 0xBE }; } }
		public byte[] LDY { get { return new byte[] { 0xA0, 0xA4, 0xB4, 0xAC, 0xBC }; } }
		public byte[] LSR { get { return new byte[] { 0x4A, 0x46, 0x56, 0x4E, 0x5E }; } }
		public byte[] NOP { get { return new byte[] { 0xEA }; } }
		public byte[] ORA { get { return new byte[] { 0x09, 0x05, 0x15, 0x0D, 0x1D, 0x19, 0x01, 0x11 }; } }
		public byte[] PHA { get { return new byte[] { 0x48 }; } }
		public byte[] PHP { get { return new byte[] { 0x08 }; } }
		public byte[] PLA { get { return new byte[] { 0x68 }; } }
		public byte[] PLP { get { return new byte[] { 0x28 }; } }
		public byte[] ROL { get { return new byte[] { 0x2A, 0x26, 0x36, 0x2E, 0x3E }; } }
		public byte[] ROR { get { return new byte[] { 0x6A, 0x66, 0x76, 0x6E, 0x7E }; } }
		public byte[] RTI { get { return new byte[] { 0x40 }; } }
		public byte[] RTS { get { return new byte[] { 0x60 }; } }
		public byte[] SBC { get { return new byte[] { 0xE9, 0xE5, 0xF5, 0xED, 0xFD, 0xF9, 0xE1, 0xF1 }; } }
		public byte[] SEC { get { return new byte[] { 0x38 }; } }
		public byte[] SED { get { return new byte[] { 0xF8 }; } }
		public byte[] SEI { get { return new byte[] { 0x78 }; } }
		public byte[] STA { get { return new byte[] { 0x85, 0x95, 0x8D, 0x9D, 0x99, 0x81, 0x91 }; } }
		public byte[] STX { get { return new byte[] { 0x86, 0x96, 0x8E }; } }
		public byte[] STY { get { return new byte[] { 0x84, 0x94, 0x8C }; } }
		#endregion
		//public byte[] this[string name]
		//{
		//    get
		//    {
		//        name = name.ToUpper();
		//        return this.GetType().GetProperties().Where(x => x.Name == name).FirstOrDefault().GetValue(this) as byte[];
		//    }
		//}
		//public ByteCodeLengthAttribute this[byte val]
		//{
		//    get
		//    {
		//        var ps = this.GetType().GetProperties();
		//        foreach (var p in ps)
		//        {
		//            if ((p.GetValue(this) as byte[]).Contains(val))
		//            {
		//                return p.GetCustomAttribute<ByteCodeLengthAttribute>();
		//            }
		//        }
		//        return default;
		//    }
		//}

		//public bool IsValid(byte code)
		//{
		//    return this.GetType().GetProperties().Any(x => ((byte[])x.GetValue(this)).Contains(code));
		//}
		//public bool IsValid(string name, byte code)
		//{
		//    name = name.ToUpper();
		//    return this.GetType().GetProperties().Where(x => x.Name == name).Any(x => ((byte[])x.GetValue(this)).Contains(code));
		//}
		public Register register = new Register();
		public byte[] addr = new byte[0xffff];
		/// <summary>
		/// 初始化内存数据
		/// </summary>
		public void InitAddr(byte[] code)
		{
			/*
说明:
    内存$fe在每条指令中包含一个随机类型。
    内存$ff则包含最后的按键ascii编码。
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
			addr[0xfe] = (byte)new Random().Next(0, 0x100);
			addr[0xff] = 0x30;
			register.PC = 0x0600;//程序是从0x0600开始运行，每次执行后要处理PC寄存器及FLAG位
			for (var i = 0; i < code.Length; i++)
			{
				addr[0x0600 + i] = code[i];
			}
		}
		public void Execute(byte[] code, Action<byte[], byte, object> action = null)
		{
			if(action == null) action = (byte[] a, byte b, object c)=>{};
			InitAddr(code);
			byte index = 0;

			var SetStep = new Action<byte>(
				(step) => {
					index += step;
					register.PC += step;
				});
			var STA_STX_STY = new Action<byte>(
				(b) => //Affects Flags: none
				{
					var offset = (code[index + 2] << 8) + code[index + 1];
					addr[offset] = b;
					action.Invoke(addr, code[index], offset);
					SetStep(3);
				});

			while (index < code.Length)
			{
				//参考: http://www.6502.org/tutorials/6502opcodes.html
				//特别注意执行完后FLAG标志位的影响
				switch (code[index])
				{
					case 0xa0://LDY a0 08 Affects Flags: N Z
						register.Y = code[index + 1];
						register.SetFlag(ERegisterFlag.Z,register.A == 0);
						register.SetFlag(ERegisterFlag.N,(register.A & 0x80) != 0);
						action.Invoke(addr, code[index], null);
						SetStep(2);
						break;
					case 0xa2://a2 08 Affects Flags: N Z
						register.X = code[index + 1];
						register.SetFlag(ERegisterFlag.Z,register.A == 0);
						register.SetFlag(ERegisterFlag.N,(register.A & 0x80) != 0);
						action.Invoke(addr, code[index], null);
						SetStep(2);
						break;
					case 0xa9://a9 01 Affects Flags: N Z
						register.A = code[index + 1];
						register.SetFlag(ERegisterFlag.Z,register.A == 0);
						register.SetFlag(ERegisterFlag.N,(register.A & 0x80) != 0);
						action.Invoke(addr, code[index], null);
						SetStep(2);
						break;
					case 0x8D://8d 00 02  STA $0200    addr[0x0200]=A
					case 0x9D:
					case 0x99:
					case 0x81:
					case 0x91:
						STA_STX_STY(register.A);
						break;
					case 0x86://STX  0x86, 0x96, 0x8E
					case 0x96:
					case 0x8E:
						STA_STX_STY(register.X);
						break;
					case 0x84://0x84, 0x94, 0x8C
						//case 0x94:
						//case 0x8C:
						STA_STX_STY(register.Y);
						break;
					case 0xca://ca        DEX       x--;
						register.X--;
						register.SetFlag(ERegisterFlag.Z,register.X == 0);
						register.SetFlag(ERegisterFlag.N,(register.X & 0x80) != 0);
						SetStep(1);
						break;
					case 0x88://88        DEY       y--;
						register.Y--;
						register.SetFlag(ERegisterFlag.Z,register.Y == 0);
						register.SetFlag(ERegisterFlag.N,(register.Y & 0x80) != 0);
						SetStep(1);
						break;
					case 0xe8://INX       x++;
						register.X++;
						register.SetFlag(ERegisterFlag.Z,register.X == 0);
						register.SetFlag(ERegisterFlag.N,(register.X & 0x80) != 0);
						SetStep(1);
						break;
					case 0xc8://INY       y++;
						register.X++;
						register.SetFlag(ERegisterFlag.Z,register.Y == 0);
						register.SetFlag(ERegisterFlag.N,(register.Y & 0x80) != 0);
						SetStep(1);
						break;
					case 0xe0://e0 03     CPX #$03      Compare(X, nn);  Affects Flags: N Z C
						register.SetFlag(ERegisterFlag.Z, register.X == code[index + 1]);
						register.SetFlag(ERegisterFlag.C, register.X >= code[index + 1]);
						var result =  register.X - code[index + 1];
						register.SetFlag(ERegisterFlag.C, (result & 0x80) != 0);
						SetStep(2);
						break;
					default:
						throw new Exception(string.Format("指令未处理：({0})",code[index].ToString("X2")));//($"指令未处理：({ code[index].ToString("X2") })");
				}
			}
		}
	}
}

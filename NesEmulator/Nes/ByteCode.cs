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
	internal class ByteCode
	{
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
						register.SetFlag(ERegisterFlag.Z,register.Y == 0);
						register.SetFlag(ERegisterFlag.N,(register.Y & 0x80) != 0);
						action.Invoke(addr, code[index], null);
						SetStep(2);
						break;
					case 0xa2://a2 08 Affects Flags: N Z
						register.X = code[index + 1];
						register.SetFlag(ERegisterFlag.Z,register.X == 0);
						register.SetFlag(ERegisterFlag.N,(register.X & 0x80) != 0);
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
						register.Y++;
						register.SetFlag(ERegisterFlag.Z,register.Y == 0);
						register.SetFlag(ERegisterFlag.N,(register.Y & 0x80) != 0);
						SetStep(1);
						break;
					case 0xe0://e0 03     CPX #$03      Compare(X, nn);  Affects Flags: N Z C
						var val = code[index + 1];
						register.SetFlag(ERegisterFlag.Z, register.X == val);
						register.SetFlag(ERegisterFlag.C, register.X >= val);
						register.SetFlag(ERegisterFlag.N, (register.X - val & 0x80) != 0);
						SetStep(2);
						break;
					default:
						throw new Exception(string.Format("指令未处理：({0})",code[index].ToString("X2")));//($"指令未处理：({ code[index].ToString("X2") })");
				}
			}
		}
	}
}

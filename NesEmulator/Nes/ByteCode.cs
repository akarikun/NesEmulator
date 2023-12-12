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
	internal partial class ByteCode
	{
		internal enum ERegisterFlag
		{
			C, Z, I, B, D, V = 6, N
		}
		
		public byte A { get; set; }
		public byte X { get; set; }
		public byte Y { get; set; }
		public ushort PC { get; set; }
		public byte S { get; set; }
		public byte P { get; set; }

		const byte CarryFlag = 1 << 0; // 假设进位标志位于状态寄存器的第一位
		const byte ZeroFlag = 1 << 1;  // 零标志
		const byte InterruptDisableFlag = 1 << 2;  // 中断禁用标志
		const byte DecimalModeFlag = 1 << 3;  // 十进制模式标志
		const byte BreakCommandFlag = 1 << 4;  // 中断命令标志
		const byte OverflowFlag = 1 << 6;  // 溢出标志
		const byte NegativeFlag = 1 << 7;  // 负标志

		// 设置或清除状态寄存器中的特定标志位
		void SetFlag(byte flag, bool state)
		{
			if (state)
				P |= flag;
			else
				P &= (byte)~flag;
		}

		// 检查状态寄存器中的特定标志位是否设置
		bool CheckFlag(byte flag)
		{
			return (P & flag) != 0;
		}
		
		// Push方法将一个字节压入堆栈
		void Push(byte value)
		{
			memory[0x0100 + S] = value;
			S--;
		}

		// Pull方法从堆栈中取出一个字节
		byte Pull()
		{
			S++;
			return memory[0x0100 + S];
		}
	}
	internal partial class ByteCode
	{
		public byte[] memory = new byte[0xffff];
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
			memory[0xfe] = (byte)new Random().Next(0, 0x100);
			memory[0xff] = 0x30;
			PC = 0x0600;//程序是从0x0600开始运行，每次执行后要处理PC寄存器及FLAG位
			for (var i = 0; i < code.Length; i++)
			{
				memory[0x0600 + i] = code[i];
			}
		}
		
		// ADC (Add with Carry)
		// Affects Flags: N Z C V
		int ADC(byte[] code, int index)
		{
			byte value = 0;
			byte additionalCycles = 0;
			int step = 0;
			
			

			switch(code[index])
			{
				case 0x69: // Immediate
					value = code[index + 1];
					step= 2;
					break;
				case 0x65: // Zero Page
					value = memory[code[index + 1]];
					step= 2;
					break;
				case 0x75: // Zero Page,X
					value = memory[(byte)(code[index + 1] + X)];
					step= 2;
					break;
				case 0x6D: // Absolute
					value = memory[(ushort)(code[index + 1] | (code[index + 2] << 8))];
					step= 3;
					break;
				case 0x7D: // Absolute,X
					value = memory[(ushort)((code[index + 1] | (code[index + 2] << 8)) + X)];
					additionalCycles = 1; // 可能需要额外的周期用于页面边界
					step= 3 + additionalCycles;
					break;
				case 0x79: // Absolute,Y
					value = memory[(ushort)((code[index + 1] | (code[index + 2] << 8)) + Y)];
					additionalCycles = 1; // 可能需要额外的周期用于页面边界
					step= 3 + additionalCycles;
					break;
				case 0x61: // Indexed Indirect (X)
					var addr = (ushort)(memory[(byte)(code[index + 1] + X)] | memory[(byte)(code[index + 1] + X + 1)] << 8);
					value = memory[addr];
					step= 2;
					break;
				case 0x71: // Indirect Indexed (Y)
					var baseAddr = (ushort)(memory[code[index + 1]] | memory[code[index + 1] + 1] << 8);
					value = memory[(ushort)(baseAddr + Y)];
					additionalCycles = 1; // 可能需要额外的周期用于页面边界
					step= 2 + additionalCycles;
					break;
				default:
					return 0; // 如果是未知的指令，返回0
			}
			
			int result = A + value + (CheckFlag(CarryFlag) ? 1 : 0);
			
			// 设置或清除零标志 (Z)
			SetFlag(ZeroFlag, result == 0);

			// 设置或清除负标志 (N)
			SetFlag(NegativeFlag, (result & 0x80) != 0);

			// 设置或清除进位标志 (C)
			SetFlag(CarryFlag, result > 0xFF);

			// 设置或清除溢出标志 (V)
			bool overflow = (~(A ^ value) & (A ^ result) & 0x80) != 0;
			SetFlag(OverflowFlag, overflow);

			// 更新累加器 (A)
			A = (byte)result;
			return step;
		}

		// AND (Logical AND with Accumulator)
		// Affects Flags: N Z
		int AND(byte[] code, int index)
		{
			byte value = 0;
			int step = 0;

			switch (code[index])
			{
				case 0x29: // Immediate
					value = code[index + 1];
					step = 2;
					break;
				case 0x25: // Zero Page
					value = memory[code[index + 1]];
					step = 2;
					break;
				case 0x35: // Zero Page,X
					value = memory[(byte)(code[index + 1] + X)];
					step = 2;
					break;
				case 0x2D: // Absolute
					value = memory[(ushort)(code[index + 1] | (code[index + 2] << 8))];
					step = 3;
					break;
				case 0x3D: // Absolute,X
					value = memory[(ushort)((code[index + 1] | (code[index + 2] << 8)) + X)];
					step = 3;
					break;
				case 0x39: // Absolute,Y
					value = memory[(ushort)((code[index + 1] | (code[index + 2] << 8)) + Y)];
					step = 3;
					break;
				case 0x21: // Indexed Indirect (X)
					var addr = (ushort)(memory[(byte)(code[index + 1] + X)] | memory[(byte)(code[index + 1] + X + 1)] << 8);
					value = memory[addr];
					step = 2;
					break;
				case 0x31: // Indirect Indexed (Y)
					var baseAddr = (ushort)(memory[code[index + 1]] | memory[code[index + 1] + 1] << 8);
					value = memory[(ushort)(baseAddr + Y)];
					step = 2;
					break;
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 与操作，将结果存储在累加器中
			A &= value;

			// 设置或清除零标志 (Z)
			SetFlag(ZeroFlag, A == 0);

			// 设置或清除负标志 (N)
			SetFlag(NegativeFlag, (A & 0x80) != 0);

			return step;
		}

		// Affects Flags: N, Z, C
		int ASL(byte[] code, int index)
		{
			int step = 0;
			byte value = 0;

			switch (code[index])
			{
				case 0x0A: // Accumulator
					// 累加器A左移一位，最低位补0，最高位存储到进位标志 (C)
					var carry = (A & 0x80) != 0;
					A <<= 1;
					// 设置或清除零标志 (Z)
					SetFlag(ZeroFlag, A == 0);
					// 设置或清除负标志 (N)
					SetFlag(NegativeFlag, (A & 0x80) != 0);
					// 设置进位标志 (C)
					SetFlag(CarryFlag, carry);
					step = 1;
					break;
				case 0x06: // Zero Page
					value = memory[code[index + 1]];
					// 内存中的值左移一位，最低位补0，最高位存储到进位标志 (C)
					carry = (value & 0x80) != 0;
					value <<= 1;
					memory[code[index + 1]] = value;
					// 设置或清除零标志 (Z)
					SetFlag(ZeroFlag, value == 0);
					// 设置或清除负标志 (N)
					SetFlag(NegativeFlag, (value & 0x80) != 0);
					// 设置进位标志 (C)
					SetFlag(CarryFlag, carry);
					step = 2;
					break;
				case 0x16: // Zero Page,X
					value = memory[(byte)(code[index + 1] + X)];
					// 内存中的值左移一位，最低位补0，最高位存储到进位标志 (C)
					carry = (value & 0x80) != 0;
					value <<= 1;
					memory[(byte)(code[index + 1] + X)] = value;
					// 设置或清除零标志 (Z)
					SetFlag(ZeroFlag, value == 0);
					// 设置或清除负标志 (N)
					SetFlag(NegativeFlag, (value & 0x80) != 0);
					// 设置进位标志 (C)
					SetFlag(CarryFlag, carry);
					step = 2;
					break;
				case 0x0E: // Absolute
					value = memory[(ushort)(code[index + 1] | (code[index + 2] << 8))];
					// 内存中的值左移一位，最低位补0，最高位存储到进位标志 (C)
					carry = (value & 0x80) != 0;
					value <<= 1;
					memory[(ushort)(code[index + 1] | (code[index + 2] << 8))] = value;
					// 设置或清除零标志 (Z)
					SetFlag(ZeroFlag, value == 0);
					// 设置或清除负标志 (N)
					SetFlag(NegativeFlag, (value & 0x80) != 0);
					// 设置进位标志 (C)
					SetFlag(CarryFlag, carry);
					step = 3;
					break;
				case 0x1E: // Absolute,X
					value = memory[(ushort)((code[index + 1] | (code[index + 2] << 8)) + X)];
					// 内存中的值左移一位，最低位补0，最高位存储到进位标志 (C)
					carry = (value & 0x80) != 0;
					value <<= 1;
					memory[(ushort)((code[index + 1] | (code[index + 2] << 8)) + X)] = value;
					// 设置或清除零标志 (Z)
					SetFlag(ZeroFlag, value == 0);
					// 设置或清除负标志 (N)
					SetFlag(NegativeFlag, (value & 0x80) != 0);
					// 设置进位标志 (C)
					SetFlag(CarryFlag, carry);
					step = 3;
					break;
				default:
					return 0; // 如果是未知的指令，返回0
			}

			return step;
		}

		// BCC指令 (Branch if Carry Clear)
		// Affects Flags: None
		int BCC(byte[] code, int index)
		{
			int step = 0;

			if (!CheckFlag(CarryFlag))
			{
				sbyte offset = (sbyte)code[index + 1]; // 转换为有符号偏移
				step = 2;

				// 计算跳转的目标地址
				int targetAddress = index + 2 + offset;

				// 检查是否需要跨页
				if (((targetAddress ^ index) & 0xFF00) != 0)
				{
					step++; // 跨页需要额外的周期
				}

				// 执行跳转
				PC = (ushort)targetAddress;
			}

			return step;
		}

		// BCS指令 (Branch if Carry Set)
		// Affects Flags: None
		int BCS(byte[] code, int index)
		{
			int step = 0;

			if (CheckFlag(CarryFlag))
			{
				sbyte offset = (sbyte)code[index + 1]; // 转换为有符号偏移
				step = 2;

				// 计算跳转的目标地址
				int targetAddress = index + 2 + offset;

				// 检查是否需要跨页
				if (((targetAddress ^ index) & 0xFF00) != 0)
				{
					step++; // 跨页需要额外的周期
				}

				// 执行跳转
				PC = (ushort)targetAddress;
			}

			return step;
		}
		// BEQ指令 (Branch if Equal)
		// Affects Flags: None
		int BEQ(byte[] code, int index)
		{
			int step = 0;

			if (CheckFlag(ZeroFlag))
			{
				sbyte offset = (sbyte)code[index + 1]; // 转换为有符号偏移
				step = 2;

				// 计算跳转的目标地址
				int targetAddress = index + 2 + offset;

				// 检查是否需要跨页
				if (((targetAddress ^ index) & 0xFF00) != 0)
				{
					step++; // 跨页需要额外的周期
				}

				// 执行跳转
				PC = (ushort)targetAddress;
			}

			return step;
		}

		// BIT指令
		// Affects Flags: Z, N, V
		int BIT(byte[] code, int index)
		{
			int step = 0;

			switch (code[index])
			{
				case 0x24: // Zero Page
					var zeroPageAddr = code[index + 1];
					var zeroPageValue = memory[zeroPageAddr];
					step = 2;

					// 测试A与内存中的值的位，并更新标志位
					SetFlag(ZeroFlag, (A & zeroPageValue) == 0);
					SetFlag(NegativeFlag, (zeroPageValue & 0x80) != 0);
					SetFlag(OverflowFlag, (zeroPageValue & 0x40) != 0);
					break;
				case 0x2C: // Absolute
					var absoluteAddr = (ushort)(code[index + 1] | (code[index + 2] << 8));
					var absoluteValue = memory[absoluteAddr];
					step = 3;

					// 测试A与内存中的值的位，并更新标志位
					SetFlag(ZeroFlag, (A & absoluteValue) == 0);
					SetFlag(NegativeFlag, (absoluteValue & 0x80) != 0);
					SetFlag(OverflowFlag, (absoluteValue & 0x40) != 0);
					break;
				default:
					return 0; // 如果是未知的指令，返回0
			}

			return step;
		}

		// BMI指令 (Branch if Minus)
		// Affects Flags: None
		int BMI(byte[] code, int index)
		{
			byte offset = 0;
			int step = 0;

			switch(code[index])
			{
				case 0x30: // Relative
					offset = code[index + 1];
					step = 2;
					break;
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 检查负标志 (N)
			if (CheckFlag(NegativeFlag))
			{
				// 如果负标志被设置，将程序计数器跳转到特定的内存地址
				PC = (ushort)(PC + offset);
			}

			return step;
		}


		// BNE (Branch if Not Equal) 指令
		int BNE(byte[] code, int index)
		{
			byte offset = 0;
			int step = 0;

			switch(code[index])
			{
				case 0xD0: // Relative
					offset = code[index + 1];
					step = 2;
					break;
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 检查零标志 (Z)
			if (!CheckFlag(ZeroFlag))
			{
				// 如果零标志未被设置，将程序计数器跳转到特定的内存地址
				PC = (ushort)(PC + offset);
			}

			return step;
		}

		// BPL (Branch if Plus) 指令
		int BPL(byte[] code, int index)
		{
			byte offset = 0;
			int step = 0;

			switch(code[index])
			{
				case 0x10: // Relative
					offset = code[index + 1];
					step = 2;
					break;
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 检查负标志 (N)
			if (!CheckFlag(NegativeFlag))
			{
				// 如果负标志未被设置，将程序计数器跳转到特定的内存地址
				PC = (ushort)(PC + offset);
			}

			return step;
		}

		// BRK (Break) 指令
		int BRK(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0x00: // Implied
					// 设置中断标志 (B)
					SetFlag(BreakCommandFlag, true);
					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}

		// BVC (Branch if Overflow Clear) 指令
		int BVC(byte[] code, int index)
		{
			byte offset = 0;
			int step = 0;

			switch(code[index])
			{
				case 0x50: // Relative
					offset = code[index + 1];
					step = 2;
					break;
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 检查溢出标志 (V)
			if (!CheckFlag(OverflowFlag))
			{
				// 如果溢出标志未被设置，将程序计数器跳转到特定的内存地址
				PC = (ushort)(PC + offset);
			}

			return step;
		}

		// BVS (Branch if Overflow Set) 指令
		int BVS(byte[] code, int index)
		{
			byte offset = 0;
			int step = 0;

			switch(code[index])
			{
				case 0x70: // Relative
					offset = code[index + 1];
					step = 2;
					break;
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 检查溢出标志 (V)
			if (CheckFlag(OverflowFlag))
			{
				// 如果溢出标志被设置，将程序计数器跳转到特定的内存地址
				PC = (ushort)(PC + offset);
			}

			return step;
		}

		// CLC (Clear Carry Flag) 指令
		int CLC(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0x18: // Implied
					// 清除进位标志 (C)
					SetFlag(CarryFlag, false);
					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}


		// CLD (Clear Decimal Mode) 指令
		int CLD(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0xD8: // Implied
					// 清除十进制模式标志 (D)
					SetFlag(DecimalModeFlag, false);
					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}

		// CLI (Clear Interrupt Disable) 指令
		int CLI(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0x58: // Implied
					// 清除中断禁用标志 (I)
					SetFlag(InterruptDisableFlag, false);
					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}

		// CLV (Clear Overflow Flag) 指令
		int CLV(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0xB8: // Implied
					// 清除溢出标志 (V)
					SetFlag(OverflowFlag, false);
					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}

		// CMP (Compare) 指令
		int CMP(byte[] code, int index)
		{
			byte value = 0;
			int step = 0;

			switch(code[index])
			{
				case 0xC9: // Immediate
					value = code[index + 1];
					step = 2;
					break;
					// 其他情况省略...
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 执行比较操作
			int result = A - value;

			// 设置或清除进位标志 (C)
			SetFlag(CarryFlag, A >= value);

			// 设置或清除零标志 (Z)
			SetFlag(ZeroFlag, (byte)result == 0);

			// 设置或清除负标志 (N)
			SetFlag(NegativeFlag, (result & 0x80) != 0);

			return step;
		}
		// CPX (Compare X Register) 指令
		int CPX(byte[] code, int index)
		{
			byte value = 0;
			int step = 0;

			switch(code[index])
			{
				case 0xE0: // Immediate
					value = code[index + 1];
					step = 2;
					break;
					// 其他情况省略...
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 执行比较操作
			int result = X - value;

			// 设置或清除进位标志 (C)
			SetFlag(CarryFlag, X >= value);

			// 设置或清除零标志 (Z)
			SetFlag(ZeroFlag, (byte)result == 0);

			// 设置或清除负标志 (N)
			SetFlag(NegativeFlag, (result & 0x80) != 0);

			return step;
		}

		// CPY (Compare Y Register) 指令
		int CPY(byte[] code, int index)
		{
			byte value = 0;
			int step = 0;

			switch(code[index])
			{
				case 0xC0: // Immediate
					value = code[index + 1];
					step = 2;
					break;
					// 其他情况省略...
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 执行比较操作
			int result = Y - value;

			// 设置或清除进位标志 (C)
			SetFlag(CarryFlag, Y >= value);

			// 设置或清除零标志 (Z)
			SetFlag(ZeroFlag, (byte)result == 0);

			// 设置或清除负标志 (N)
			SetFlag(NegativeFlag, (result & 0x80) != 0);

			return step;
		}

		// DEC (Decrement Memory) 指令
		int DEC(byte[] code, int index)
		{
			ushort addr = 0;
			int step = 0;

			switch(code[index])
			{
				case 0xC6: // Zero Page
					addr = code[index + 1];
					step = 2;
					break;
					// 其他情况省略...
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 执行递减操作
			memory[addr]--;

			// 设置或清除零标志 (Z)
			SetFlag(ZeroFlag, memory[addr] == 0);

			// 设置或清除负标志 (N)
			SetFlag(NegativeFlag, (memory[addr] & 0x80) != 0);

			return step;
		}

		// DEX (Decrement X Register) 指令
		int DEX(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0xCA: // Implied
					// 执行递减操作
					X--;

					// 设置或清除零标志 (Z)
					SetFlag(ZeroFlag, X == 0);

					// 设置或清除负标志 (N)
					SetFlag(NegativeFlag, (X & 0x80) != 0);

					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}

		// DEY (Decrement Y Register) 指令
		int DEY(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0x88: // Implied
					// 执行递减操作
					Y--;

					// 设置或清除零标志 (Z)
					SetFlag(ZeroFlag, Y == 0);

					// 设置或清除负标志 (N)
					SetFlag(NegativeFlag, (Y & 0x80) != 0);

					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}

		// EOR (Exclusive OR) 指令
		int EOR(byte[] code, int index)
		{
			byte value = 0;
			int step = 0;

			switch(code[index])
			{
				case 0x49: // Immediate
					value = code[index + 1];
					step = 2;
					break;
					// 其他情况省略...
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 执行异或操作
			A = (byte)(A ^ value);

			// 设置或清除零标志 (Z)
			SetFlag(ZeroFlag, A == 0);

			// 设置或清除负标志 (N)
			SetFlag(NegativeFlag, (A & 0x80) != 0);

			return step;
		}

		// INC (Increment Memory) 指令
		int INC(byte[] code, int index)
		{
			ushort addr = 0;
			int step = 0;

			switch(code[index])
			{
				case 0xE6: // Zero Page
					addr = code[index + 1];
					step = 2;
					break;
					// 其他情况省略...
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 执行递增操作
			memory[addr]++;

			// 设置或清除零标志 (Z)
			SetFlag(ZeroFlag, memory[addr] == 0);

			// 设置或清除负标志 (N)
			SetFlag(NegativeFlag, (memory[addr] & 0x80) != 0);

			return step;
		}

		// INX (Increment X Register) 指令
		int INX(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0xE8: // Implied
					// 执行递增操作
					X++;

					// 设置或清除零标志 (Z)
					SetFlag(ZeroFlag, X == 0);

					// 设置或清除负标志 (N)
					SetFlag(NegativeFlag, (X & 0x80) != 0);

					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}
		
		// INY (Increment Y Register) 指令
		int INY(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0xC8: // Implied
					// 执行递增操作
					Y++;

					// 设置或清除零标志 (Z)
					SetFlag(ZeroFlag, Y == 0);

					// 设置或清除负标志 (N)
					SetFlag(NegativeFlag, (Y & 0x80) != 0);

					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}

		// JMP (Jump) 指令
		int JMP(byte[] code, int index)
		{
			ushort addr = 0;

			switch(code[index])
			{
				case 0x4C: // Absolute
					addr = (ushort)(code[index + 1] | (code[index + 2] << 8));
					break;
					// 其他情况省略...
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 执行跳转操作
			PC = addr;

			return 0; // JMP指令不会增加PC，因此返回0
		}

		// JSR (Jump to Subroutine) 指令
		int JSR(byte[] code, int index)
		{
			ushort addr = 0;

			switch(code[index])
			{
				case 0x20: // Absolute
					addr = (ushort)(code[index + 1] | (code[index + 2] << 8));
					break;
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 将返回地址压入堆栈
			Push((byte)((PC - 1) >> 8));
			Push((byte)(PC - 1));

			// 执行跳转操作
			PC = addr;

			return 0; // JSR指令不会增加PC，因此返回0
		}

		// LDA (Load Accumulator) 指令
		int LDA(byte[] code, int index)
		{
			byte value = 0;
			int step = 0;

			switch(code[index])
			{
				case 0xA9: // Immediate
					value = code[index + 1];
					step = 2;
					break;
					// 其他情况省略...
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 执行加载操作
			A = value;

			// 设置或清除零标志 (Z)
			SetFlag(ZeroFlag, A == 0);

			// 设置或清除负标志 (N)
			SetFlag(NegativeFlag, (A & 0x80) != 0);

			return step;
		}
		// LDX (Load X Register) 指令
		int LDX(byte[] code, int index)
		{
			byte value = 0;
			int step = 0;

			switch(code[index])
			{
				case 0xA2: // Immediate
					value = code[index + 1];
					step = 2;
					break;
					// 其他情况省略...
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 执行加载操作
			X = value;

			// 设置或清除零标志 (Z)
			SetFlag(ZeroFlag, X == 0);

			// 设置或清除负标志 (N)
			SetFlag(NegativeFlag, (X & 0x80) != 0);

			return step;
		}

		// LDY (Load Y Register) 指令
		int LDY(byte[] code, int index)
		{
			byte value = 0;
			int step = 0;

			switch(code[index])
			{
				case 0xA0: // Immediate
					value = code[index + 1];
					step = 2;
					break;
					// 其他情况省略...
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 执行加载操作
			Y = value;

			// 设置或清除零标志 (Z)
			SetFlag(ZeroFlag, Y == 0);

			// 设置或清除负标志 (N)
			SetFlag(NegativeFlag, (Y & 0x80) != 0);

			return step;
		}

		// LSR (Logical Shift Right) 指令
		int LSR(byte[] code, int index)
		{
			ushort addr = 0;
			int step = 0;

			switch(code[index])
			{
				case 0x46: // Zero Page
					addr = code[index + 1];
					step = 2;
					break;
					// 其他情况省略...
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 执行逻辑右移操作
			byte value = memory[addr];
			SetFlag(CarryFlag, (value & 0x01) != 0);
			value >>= 1;
			memory[addr] = value;

			// 设置或清除零标志 (Z)
			SetFlag(ZeroFlag, value == 0);

			// 设置或清除负标志 (N)
			SetFlag(NegativeFlag, (value & 0x80) != 0);

			return step;
		}

		// NOP (No Operation) 指令
		int NOP(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0xEA: // Implied
					// 无操作
					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}

		// ORA (Logical Inclusive OR) 指令
		int ORA(byte[] code, int index)
		{
			byte value = 0;
			int step = 0;

			switch(code[index])
			{
				case 0x09: // Immediate
					value = code[index + 1];
					step = 2;
					break;
					// 其他情况省略...
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 执行逻辑或操作
			A = (byte)(A | value);

			// 设置或清除零标志 (Z)
			SetFlag(ZeroFlag, A == 0);

			// 设置或清除负标志 (N)
			SetFlag(NegativeFlag, (A & 0x80) != 0);

			return step;
		}

		// PHA (Push Accumulator) 指令
		int PHA(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0x48: // Implied
					// 将累加器压入堆栈
					Push(A);
					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}

		// PHP (Push Processor Status) 指令
		int PHP(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0x08: // Implied
					// 将处理器状态压入堆栈
					Push(P);
					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}

		// PLA (Pull Accumulator) 指令
		int PLA(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0x68: // Implied
					// 从堆栈中取出累加器
					A = Pull();

					// 设置或清除零标志 (Z)
					SetFlag(ZeroFlag, A == 0);

					// 设置或清除负标志 (N)
					SetFlag(NegativeFlag, (A & 0x80) != 0);

					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}
		
		// PLP (Pull Processor Status) 指令
		int PLP(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0x28: // Implied
					// 从堆栈中取出处理器状态
					P = Pull();
					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}

		// ROL (Rotate Left) 指令
		int ROL(byte[] code, int index)
		{
			ushort addr = 0;
			int step = 0;

			switch(code[index])
			{
				case 0x26: // Zero Page
					addr = code[index + 1];
					step = 2;
					break;
					// 其他情况省略...
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 执行左旋操作
			byte value = memory[addr];
			byte carry = (byte)(value >> 7);
			value = (byte)((value << 1) | (CheckFlag(CarryFlag) ? 1 : 0));
			memory[addr] = value;

			// 设置或清除进位标志 (C)
			SetFlag(CarryFlag, carry != 0);

			// 设置或清除零标志 (Z)
			SetFlag(ZeroFlag, value == 0);

			// 设置或清除负标志 (N)
			SetFlag(NegativeFlag, (value & 0x80) != 0);

			return step;
		}

		// ROR (Rotate Right) 指令
		int ROR(byte[] code, int index)
		{
			ushort addr = 0;
			int step = 0;

			switch(code[index])
			{
				case 0x66: // Zero Page
					addr = code[index + 1];
					step = 2;
					break;
					// 其他情况省略...
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 执行右旋操作
			byte value = memory[addr];
			byte carry = (byte)(value & 0x01);
			value = (byte)((value >> 1) | (CheckFlag(CarryFlag) ? 0x80 : 0));
			memory[addr] = value;

			// 设置或清除进位标志 (C)
			SetFlag(CarryFlag, carry != 0);

			// 设置或清除零标志 (Z)
			SetFlag(ZeroFlag, value == 0);

			// 设置或清除负标志 (N)
			SetFlag(NegativeFlag, (value & 0x80) != 0);

			return step;
		}

		// RTI (Return from Interrupt) 指令
		int RTI(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0x40: // Implied
					// 从堆栈中取出处理器状态
					P = Pull();
					// 从堆栈中取出程序计数器
					PC = (ushort)(Pull() | (Pull() << 8));
					return 0; // RTI指令不会增加PC，因此返回0
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}
		
		// RTS (Return from Subroutine) 指令
		int RTS(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0x60: // Implied
					// 从堆栈中取出返回地址
					PC = (ushort)(Pull() | (Pull() << 8));
					// RTS指令会将PC增加1，因此返回1
					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}

		// SBC (Subtract with Carry) 指令
		int SBC(byte[] code, int index)
		{
			byte value = 0;
			int step = 0;

			switch(code[index])
			{
				case 0xE9: // Immediate
					value = code[index + 1];
					step = 2;
					break;
					// 其他情况省略...
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 执行减法操作
			int result = A - value - (CheckFlag(CarryFlag) ? 0 : 1);

			// 设置或清除进位标志 (C)
			SetFlag(CarryFlag, result >= 0);

			// 设置或清除零标志 (Z)
			SetFlag(ZeroFlag, (byte)result == 0);

			// 设置或清除负标志 (N)
			SetFlag(NegativeFlag, (result & 0x80) != 0);

			// 设置或清除溢出标志 (V)
			bool overflow = ((A ^ value) & (A ^ (byte)result) & 0x80) != 0;
			SetFlag(OverflowFlag, overflow);

			// 更新累加器 (A)
			A = (byte)result;

			return step;
		}

		// SEC (Set Carry Flag) 指令
		int SEC(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0x38: // Implied
					// 设置进位标志 (C)
					SetFlag(CarryFlag, true);
					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}

		// SED (Set Decimal Flag) 指令
		int SED(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0xF8: // Implied
					// 设置十进制模式标志 (D)
					SetFlag(DecimalModeFlag, true);
					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}
		// SEI (Set Interrupt Disable) 指令
		int SEI(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0x78: // Implied
					// 设置中断禁用标志 (I)
					SetFlag(InterruptDisableFlag, true);
					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}

		// STA (Store Accumulator) 指令
		int STA(byte[] code, int index)
		{
			ushort addr = 0;
			int step = 0;

			switch(code[index])
			{
				case 0x85: // Zero Page
					addr = code[index + 1];
					step = 2;
					break;
				case 0x8D:
					var offset = (code[index + 2] << 8) + code[index + 1];
					memory[offset] = A;
					step = 3;
					break;
					// 其他情况省略...
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 执行存储操作
			memory[addr] = A;

			return step;
		}

		// STX (Store X Register) 指令
		int STX(byte[] code, int index)
		{
			ushort addr = 0;
			int step = 0;

			switch(code[index])
			{
				case 0x86: // Zero Page
					addr = code[index + 1];
					step = 2;
					break;
					// 其他情况省略...
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 执行存储操作
			memory[addr] = X;

			return step;
		}

		// STY (Store Y Register) 指令
		int STY(byte[] code, int index)
		{
			ushort addr = 0;
			int step = 0;

			switch(code[index])
			{
				case 0x84: // Zero Page
					addr = code[index + 1];
					step = 2;
					break;
					// 其他情况省略...
				default:
					return 0; // 如果是未知的指令，返回0
			}

			// 执行存储操作
			memory[addr] = Y;

			return step;
		}
		// TAX (Transfer Accumulator to X) 指令
		int TAX(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0xAA: // Implied
					// 将累加器的值传送到X寄存器
					X = A;

					// 设置或清除零标志 (Z)
					SetFlag(ZeroFlag, X == 0);

					// 设置或清除负标志 (N)
					SetFlag(NegativeFlag, (X & 0x80) != 0);

					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}

		// TAY (Transfer Accumulator to Y) 指令
		int TAY(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0xA8: // Implied
					// 将累加器的值传送到Y寄存器
					Y = A;

					// 设置或清除零标志 (Z)
					SetFlag(ZeroFlag, Y == 0);

					// 设置或清除负标志 (N)
					SetFlag(NegativeFlag, (Y & 0x80) != 0);

					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}

		// TSX (Transfer Stack Pointer to X) 指令
		int TSX(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0xBA: // Implied
					// 将堆栈指针的值传送到X寄存器
					X = S;

					// 设置或清除零标志 (Z)
					SetFlag(ZeroFlag, X == 0);

					// 设置或清除负标志 (N)
					SetFlag(NegativeFlag, (X & 0x80) != 0);

					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}

		// TXA (Transfer X to Accumulator) 指令
		int TXA(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0x8A: // Implied
					// 将X寄存器的值传送到累加器
					A = X;

					// 设置或清除零标志 (Z)
					SetFlag(ZeroFlag, A == 0);

					// 设置或清除负标志 (N)
					SetFlag(NegativeFlag, (A & 0x80) != 0);

					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}
		// TXS (Transfer X to Stack Pointer) 指令
		int TXS(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0x9A: // Implied
					// 将X寄存器的值传送到堆栈指针
					S = X;
					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
			}
		}

		// TYA (Transfer Y to Accumulator) 指令
		int TYA(byte[] code, int index)
		{
			switch(code[index])
			{
				case 0x98: // Implied
					// 将Y寄存器的值传送到累加器
					A = Y;

					// 设置或清除零标志 (Z)
					SetFlag(ZeroFlag, A == 0);

					// 设置或清除负标志 (N)
					SetFlag(NegativeFlag, (A & 0x80) != 0);

					return 1;
				default:
					return 0; // 如果是未知的指令，返回0
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
					PC += step;
				});

			var arr = new List<Func<byte[],int,int>>(){
//				ADC,AND,ASL,BCC,BCS,BEQ,BIT,BMI,BNE,BPL,BRK,BVC,BVS,CLC,
//				CLD,CLI,CLV,CMP,CPX,CPY,DEC,DEX,DEY,EOR,INC,INX,INY,JMP,
//				JSR,LDA,LDX,LDY,LSR,NOP,ORA,PHA,PHP,PLA,PLP,ROL,ROR,RTI,
//				RTS,SBC,SEC,SED,SEI,STA,STX,STY,TAX,TAY,TSX,TXA,TXS,TYA
				LDA,STA
			};
			while (index < code.Length)
			{
				byte step = 0;
				for(var i = 0;i<arr.Count();i++){
					if(i == 29){
						Console.WriteLine(arr[index]);
					}
					step = (byte)arr[i](code,index);
					if(step>0){
						action.Invoke(code, code[index], null);
						SetStep(step);
						continue;
					}
				}
				if(step == 0){
					throw new Exception(string.Format("指令未处理：({0})",code[index].ToString("X2")));
				}
			}
		}
	}
}

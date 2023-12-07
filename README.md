# NesEmulator
 Nes模拟器

6502寄存器参考：  https://happysoul.github.io/nes/6502/page/BASIC/register.htm

6502指令集参考：  http://www.6502.org/tutorials/6502opcodes.html

| 6502指令 | 机器码 | 伪代码 (C#) | 说明 |
|---------|--------|-------------|------|
| ADC #nn | 69 nn | A += nn + C; | 加上立即数和进位位 |
| AND #nn | 29 nn | A &= nn; | 与立即数进行AND操作 |
| ASL A | 0A | A <<= 1; | 累加器A向左移位 |
| BCC rel | 90 nn | if (!C) { PC += nn; } | 进位清除时分支 |
| BCS rel | B0 nn | if (C) { PC += nn; } | 进位设置时分支 |
| BEQ rel | F0 nn | if (Z) { PC += nn; } | 零标志设置时分支 |
| BIT zp | 24 nn | Z = !(A & ZeroPage[nn]); | 测试零页地址的位 |
| BMI rel | 30 nn | if (N) { PC += nn; } | 负标志设置时分支 |
| BNE rel | D0 nn | if (!Z) { PC += nn; } | 零标志清除时分支 |
| BPL rel | 10 nn | if (!N) { PC += nn; } | 负标志清除时分支 |
| BRK | 00 | /* 中断 */ | 强制中断 |
| BVC rel | 50 nn | if (!V) { PC += nn; } | 溢出清除时分支 |
| BVS rel | 70 nn | if (V) { PC += nn; } | 溢出设置时分支 |
| CLC | 18 | C = false; | 清除进位标志 |
| CLD | D8 | D = false; | 清除十进制模式 |
| CLI | 58 | I = false; | 清除中断禁止标志 |
| CLV | B8 | V = false; | 清除溢出标志 |
| CMP #nn | C9 nn | Compare(A, nn); | 比较累加器和立即数 |
| CPX #nn | E0 nn | Compare(X, nn); | 比较X寄存器和立即数 |
| CPY #nn | C0 nn | Compare(Y, nn); | 比较Y寄存器和立即数 |
| DEC zp | C6 nn | ZeroPage[nn]--; | 零页地址的值减1 |
| DEX | CA | X--; | X寄存器减1 |
| DEY | 88 | Y--; | Y寄存器减1 |
| EOR #nn | 49 nn | A ^= nn; | 对累加器进行异或操作 |
| INC zp | E6 nn | ZeroPage[nn]++; | 零页地址的值加1 |
| INX | E8 | X++; | X寄存器加1 |
| INY | C8 | Y++; | Y寄存器加1 |
| JMP abs | 4C nn nn | PC = nnnn; | 跳转到指定地址 |
| JSR abs | 20 nn nn | Call(nnnn); | 跳到子程序 |
| LDA #nn | A9 nn | A = nn; | 加载立即数到累加器 |
| LDX #nn | A2 nn | X = nn; | 加载立即数到X寄存器 |
| LDY #nn | A0 nn | Y = nn; | 加载立即数到Y寄存器 |
| LSR A | 4A | A >>= 1; | 累加器A向右移位 |
| NOP | EA | /* 无操作 */ | 无操作 |
| ORA #nn | 09 nn | A |= nn; | 对累加器进行或操作 |
| PHA | 48 | Push(A); | 将累加器压入栈 |
| PHP | 08 | Push(P); | 将状态寄存器压入栈 |
| PLA | 68 | A = Pop(); | 从栈弹出到累加器 |
| PLP | 28 | P = Pop(); | 从栈弹出到状态寄存器 |
| ROL A | 2A | RotateLeft(A); | 累加器A向左旋转 |
| ROR A | 6A | RotateRight(A); | 累加器A向右旋转 |
| RTI | 40 | ReturnFromInterrupt(); | 从中断返回 |
| RTS | 60 | Return(); | 从子程序返回 |
| SBC #nn | E9 nn | A -= nn + !C; | 从累加器减去立即数和非进位位 |
| SEC | 38 | C = true; | 设置进位标志 |
| SED | F8 | D = true; | 设置十进制模式 |
| SEI | 78 | I = true; | 设置中断禁止标志 |
| STA zp | 85 nn | ZeroPage[nn] = A; | 将累加器存储到零页地址 |
| STX zp | 86 nn | ZeroPage[nn] = X; | 将X寄存器存储到零页地址 |
| STY zp | 84 nn | ZeroPage[nn] = Y; | 将Y寄存器存储到零页地址 |
| TAX | AA | X = A; | 将累加器的值传送到X寄存器 |
| TAY | A8 | Y = A; | 将累加器的值传送到Y寄存器 |
| TSX | BA | X = S; | 将栈指针的值传送到X寄存器 |
| TXA | 8A | A = X; | 将X寄存器的值传送到累加器 |
| TXS | 9A | S = X; | 将X寄存器的值传送到栈指针 |
| TYA | 98 | A = Y; | 将Y寄存器的值传送到累加器 |


标志寄存器
| 位 | 标志 | 描述 |
|----|------|------|
| 7  | N - 负标志 (Negative Flag) | 当算术或逻辑运算结果的最高位（第7位）为1时设置。通常表示结果为负数。 |
| 6  | V - 溢出标志 (Overflow Flag) | 当算术运算结果超出有符号数的合法范围（-128至+127）时设置。 |
| 5  | - | 未使用，通常为1。 |
| 4  | B - 中断标志 (Break Command) | 表示BRK指令是否已执行。在PHP和BRK指令执行后设置，由PLP和RTI指令清除。 |
| 3  | D - 十进制模式标志 (Decimal Mode) | 当设置时，处理器在执行算术指令时使用二进制编码的十进制（BCD）模式。 |
| 2  | I - 中断禁止标志 (Interrupt Disable) | 当设置时，处理器将忽略所有非强制性的中断请求。 |
| 1  | Z - 零标志 (Zero Flag) | 当算术或逻辑运算结果为零时设置。 |
| 0  | C - 携带标志 (Carry Flag) | 在算术运算中表示进位或借位。加法中超出范围时设置，减法中未发生借位时设置。 |



第一个程序
```
Address  Hexdump   Dissassembly
-------------------------------
$0600    a9 01     LDA #$01
$0602    8d 00 02  STA $0200
$0605    a9 05     LDA #$05
$0607    8d 01 02  STA $0201
$060a    a9 08     LDA #$08
$060c    8d 02 02  STA $0202

//伪代码大概是这样的
A=0x01
Addr[0x0200]=A
A=0x05
Addr[0x0201]=A
A=0x08
Addr[0x0202]=A
```

```
public static Register register = new Register();

//对应的方法实现
public void Execute(byte[] code)
{
    var addr = new byte[0xffff];
    //for (var i = 0; i < code.Length; i++)
    //{
    //    addr[0x0600 + i] = code[i];
    //}
    register.PC = 0x0600;//程序是从0x0600开始运行，每次执行后要处理PC寄存器及FLAG位
    byte index = 0;
    while (index < code.Length)
    {
        switch (code[index])
        {
            case 0xa9://a9 01     LDA #$01    A = code[1]
                {
                    register.A = code[1];
                    index += 2;
                    register.PC += index;
                }
                break;
            case 0x8d://8d 00 02  STA $0200    addr[0x0200]=A
                {
                    var offset = (code[index + 2] << 8) + code[index + 1];
                    addr[offset] = register.A;
                    index += 3;
                    register.PC += index;
                }
                break;
        }
        //这里处理FLAG位
    }
}
```
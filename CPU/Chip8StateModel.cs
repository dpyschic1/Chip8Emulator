namespace Chip8;

public class Chip8Processor
{
    private readonly byte[] _memory = new byte[0x1000];
    private readonly byte[] _v = new byte[16];
    private readonly ushort _i = 0;
    private readonly ushort[] _stack = new ushort[16];
    private const int SCREEN_WIDTH = 64;
    private const int SCREEN_HEIGHT = 32;
    private readonly bool[,] _screen =  new bool[SCREEN_WIDTH, SCREEN_HEIGHT];
    private ushort _pc = 0;
    private ushort _sp = 0;
    private readonly Dictionary<byte, Action<OpCode>> _instructions = [];

    public Chip8Processor()
    {
        _instructions[0x0] = this.JumptoAddress;
        _instructions[0x1]  = this.CallSubroutine;
        _instructions[0x2] = this.SkipVxEqNN;
        _instructions[0x3] = this.SkipVxNeqNN;
        _instructions[0x4] = this.SkipVxEqVy;
        _instructions[0x5] = this.SetVxtoNN;
        _instructions[0x6] = this.AddNNtoVx;
    }

    private void JumptoAddress(OpCode opCode)
    {
        _pc = opCode.NNN;
    }
    
    private void CallSubroutine(OpCode opCode)
    {
        Push(_pc);
        _pc = opCode.NNN;
    }
    
    private void SkipVxEqNN(OpCode opCode)
    {
        if(_v[opCode.X] == opCode.NN)
        {
            _pc += 2;
        }
    }

    private void SkipVxNeqNN(OpCode opCode)
    {
        if(_v[opCode.X] != opCode.NN)
        {
            _pc += 2;
        }
    }
    private void SkipVxEqVy(OpCode opCode)
    {
        if(_v[opCode.X] == _v[opCode.Y])
        {
            _pc += 2;
        }
    }

    private void SetVxtoNN(OpCode opCode)
    {
        _v[opCode.X] = opCode.NN;
    }

    private void AddNNtoVx(OpCode opCode)
    {
        var res =_v[opCode.X] + opCode.NN;
        bool carry = res > 255;
        if(carry)
        {
            res -= 255;
        }

        _v[opCode.X] = (byte)(res & 0x00FF);
    }

    private void XYOps(OpCode opCode)
    {
        switch(opCode.N)
        {
            case 0x0:
                _v[opCode.X] = _v[opCode.Y];
                break;
            
            case 0x1:
                _v[opCode.X] |= _v[opCode.Y];
                break;
            
            case 0x2:
                _v[opCode.X] &= _v[opCode.Y];
                break;
            
            case 0x3:
                _v[opCode.X] ^= _v[opCode.Y];
                break;
            
            case 0x4:
                var res = _v[opCode.X] + _v[opCode.Y];
                var carry = res > 255;
                _v[opCode.X] = (byte)res;
                _v[0xF] = (byte)(carry ? 1 : 0);
                break;
            
            case 0x5:
                _v[0xF] = (byte)(_v[opCode.X] >= _v[opCode.Y] ? 1 : 0);
                _v[opCode.X] -= _v[opCode.Y];
                break;

            case 0x6:
                _v[0xF] = (byte)((_v[opCode.X] & 0x1) == 1 ? 1 : 0);
                _v[opCode.X] >>= 1;
                break;
            
            case 0x7:
                _v[opCode.X] = (byte)(_v[opCode.Y] - _v[opCode.X]);
                _v[0xF] = (byte)(_v[opCode.Y] >= _v[opCode.X] ? 1 : 0);
                break;

            default:
                throw new InstructionNoValidException()
                break;


        }
    }


    private void Push(ushort ProgramCounter)
    {
        _stack[_sp++] = ProgramCounter;

    }

    private ushort Pop()
    {
        return _stack[--_sp];
    }
}

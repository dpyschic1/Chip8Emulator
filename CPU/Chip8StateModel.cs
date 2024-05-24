using Chip8.CPU;
using Chip8.Exceptions;

namespace Chip8;

public class Chip8Processor
{
    private readonly byte[] _memory = new byte[0x1000];
    private readonly byte[] _v = new byte[16];
    private readonly ushort[] _stack = new ushort[16];
    private const int SCREEN_WIDTH = 64;
    private const int SCREEN_HEIGHT = 32;
    private const int ROM_START_LOCATION = 0x200;
    private readonly bool[,] _screen =  new bool[SCREEN_WIDTH, SCREEN_HEIGHT];
    private ushort _pc = 0;
    private ushort _sp = 0;
    private ushort _i = 0;
    private byte _delay = 0;
    private readonly Dictionary<byte, Action<OpCode>> _instructions = [];
    private readonly Dictionary<byte, Action<OpCode>> _miscInstructions = [];
    private readonly Random _rand = new ();

    public Chip8Processor()
    {
        _instructions[0x0] = this.ZeroOps;
        _instructions[0x1] = this.JumptoAddress;
        _instructions[0x2]  = this.CallSubroutine;
        _instructions[0x3] = this.SkipVxEqNN;
        _instructions[0x4] = this.SkipVxNeqNN;
        _instructions[0x5] = this.SkipVxEqVy;
        _instructions[0x6] = this.SetVxtoNN;
        _instructions[0x7] = this.AddNNtoVx;
        _instructions[0x8] = this.XYOps;

        _instructions[0xA] = this.SetI;
        _instructions[0xB] = this.JumpWithV0;
        _instructions[0xC] = this.Rand;
        _instructions[0xD] = this.Draw;
    }

    public async Task LoadRom(Stream rom)
    {
        Reset();
        using var memory = new MemoryStream(_memory, ROM_START_LOCATION, (int)rom.Length, true);
        await rom.CopyToAsync(memory);
    }

    public void Tick()
    {
        var data = (ushort) (_memory[++_pc] << 8 | _memory[_pc++]);
        var opCode = new OpCode(data);

        if(!_instructions.TryGetValue(opCode.Set, out var instruction))
        {
            throw new InstructionNotValidException($"Instruction is not part of arch or is not implemented");
        }

        instruction(opCode);
        
        if(_delay > 0)
        {
            _delay--;
        }
    }

    public void Reset()
    {
        Array.Clear(_memory, 0, _memory.Length);
        for (var i = 0; i != Fonts.Characters.Length; ++i)
            _memory[i] = Fonts.Characters[i];

        Array.Clear(_v, 0, _v.Length);
        Array.Clear(_stack, 0, _stack.Length);
        Array.Clear(_screen, 0, _screen.Length);
        _pc = ROM_START_LOCATION;
        _i = 0;
        _sp = 0;
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

            case 0xE:
                _v[0xF] = (byte)((_v[opCode.X] & 0xA0) == 0xA0 ? 1 : 0);
                _v[opCode.X] <<= 1;
                break;

            default:
                throw new InstructionNotValidException($"The instruction {opCode.N} is not part of the CPU");

        }
    }

    private void SetI(OpCode opCode)
    {
        _i = opCode.NNN;
    }

    private void JumpWithV0(OpCode opCode)
    {
        _pc = (ushort)(_v[0] + opCode.NNN);
    }

    private void Rand(OpCode opCode)
    {
        _v[opCode.X] = (byte)(_rand.Next(0,255) & opCode.NNN);
    }

    private void Draw(OpCode opCode)
    {
        throw new NotImplementedException();
    }

    private void ZeroOps(OpCode opCode)
    {
        switch(opCode.NN)
        {
            case 0xE0:
                break;
            
            case 0xEE:
                _pc = Pop();
                break;

            default:
                throw new InstructionNotValidException();
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

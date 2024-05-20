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


    private void Push(ushort ProgramCounter)
    {
        _stack[_sp++] = ProgramCounter;

    }

    private ushort Pop()
    {
        return _stack[--_sp];
    }
}

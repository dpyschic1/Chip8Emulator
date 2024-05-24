using Chip8;

public class Program 
{
    public static void Main()
    {
        using var romLoader = new BinaryReader(new FileStream("demo.ch8", FileMode.Open));
        var cpu = new Chip8Processor();
        cpu.
    }
}
using Chip8;
using Chip8.Peripherals;

public class Program 
{
    public static async Task Main()
    {
        try{
        using var romLoader = new FileStream("demo.ch8", FileMode.Open);
        var soundPlayer = new SoundPlayer();
        var cpu = new Chip8Processor(soundPlayer);
        await cpu.LoadRom(romLoader);
        while(true)
        {
            cpu.Tick();
        }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
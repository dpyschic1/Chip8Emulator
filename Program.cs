using Chip8;

public class Program 
{
    public static async Task Main()
    {
        try{
        using var romLoader = new FileStream("demo.ch8", FileMode.Open);
        var cpu = new Chip8Processor();
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
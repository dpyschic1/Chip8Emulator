namespace Chip8.Peripherals
{
    public class LambdaRenderer
    {
        private readonly Action _draw;
        public LambdaRenderer(Action draw)
        {
            _draw = draw;
        }
        public void Draw()
         => _draw?.Invoke();
    }
}
using System;

namespace FluidSimulation1
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (FluidSimulation1 game = new FluidSimulation1())
            {
                game.Run();
            }
        }
    }
#endif
}


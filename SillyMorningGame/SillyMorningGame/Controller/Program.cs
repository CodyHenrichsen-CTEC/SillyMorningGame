using System;

namespace SillyMorningGame.Controller
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (SillyShooterGame game = new SillyShooterGame())
            {
                game.Run();
            }
        }
    }
#endif
}


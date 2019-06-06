namespace EnsoulSharp.Katarina
{
    using System;
    using EnsoulSharp.SDK;

    internal class Program
    {
        private static void Main(string[] args)
        {
            GameEvent.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad()
        {
            if (ObjectManager.Player.CharacterName != "Katarina")
                return;
            Katarina.OnLoad();
            Chat.PrintChat("<font color = '#FFFFFF' > [Nicky -> Katarina]");
        }
    }
}

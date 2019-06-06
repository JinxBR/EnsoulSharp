using EnsoulSharp.SDK;


namespace EnsoulSharp.Jinx
{
    class Program
    {
        static void Main(string[] args)
        {
            GameEvent.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad()
        {
            if (ObjectManager.Player.CharacterName != "Jinx")
                return;

            JinxLoading.OnLoad();
            Chat.Print("<font color = \"#6B9FE3\">EnsoulSharp.Jinx</font><font color = \"#E3AF6B\"> by Nicky</font>");
           
        }
    }
}

using System.Windows.Forms;
using EnsoulSharp.SDK.MenuUI.Values;

namespace EnsoulSharp.Jinx
{
    class IsMenu
    {
        public class Combat
        {
            public static readonly MenuBool Q = new MenuBool("w", "Use Q");
            public static readonly MenuBool W = new MenuBool("w", "Use W");
            public static readonly MenuBool E = new MenuBool("w", "Use R");
        }

        public class Harass
        {
            public static readonly MenuBool Q = new MenuBool("q", "Use Q");
            public static readonly MenuBool W = new MenuBool("w", "Use W");
            public static readonly MenuSlider Mana = new MenuSlider("minmana", "^ Min ManaPercent <= x%", 50);
        }


        public class KillAble
        {
            public static readonly MenuBool W = new MenuBool("w", "Use W");
            public static readonly MenuBool R = new MenuBool("w", "Use R");
        }

        public class Misc
        {
            public static readonly MenuBool RAntiGapcloser = new MenuBool("rantigapcloser", "Use E AntiGapcloser");
            public static readonly MenuBool RInterrupt = new MenuBool("rinterrupt", "Use E Interrupt Spell");
        }

        public class Draw
        {
            public static readonly MenuBool W = new MenuBool("w", "Draw W Range");
        }

        public class SemiR
        {
            public static readonly MenuKeyBind Key = new MenuKeyBind("semir", "Semi R Key", Keys.T, KeyBindType.Press);
        }
    }
}

namespace EnsoulSharp.Katarina
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.MenuUI;
    using EnsoulSharp.SDK.MenuUI.Values;
    using EnsoulSharp.SDK.Prediction;
    using EnsoulSharp.SDK.Utility;

    using Color = System.Drawing.Color;

    internal class Katarina
    {
        private static readonly Menu MenuKat;
        public static Spell Q, W, E, R;
        private static AIBaseClient target;

        public class ComboKat
        {
            public static readonly MenuBool Q = new MenuBool("useq", "Use Q");
            public static readonly MenuBool W = new MenuBool("usew", "Use W");
            public static readonly MenuBool E = new MenuBool("usee", "Use E");
            public static readonly MenuBool R = new MenuBool("usee2", "^- Save E if no Daggers");
        }

        public class RSet
        {
            public static readonly MenuBool UseR = new MenuBool("useRR", "Use R");
            public static readonly MenuSlider Ds = new MenuSlider("daggers", "R Daggers for Damage Check", 8, 1, 16);
            public static readonly MenuSlider hitR = new MenuSlider("hiftr", "R If Hits", 1, 1, 5);
            public static readonly MenuBool cancelR = new MenuBool("cancelr", "Cancel R if no Enemies");
            public static readonly MenuBool caKR = new MenuBool("cancelRk", "^- Cancel R for Killsteal");
            public static readonly MenuSlider waster = new MenuSlider("waster", "Dont waste R if Enemy HP lower than", 100, 0, 500);
        }

        public class Draws
        {
            public static readonly MenuBool DQ = new MenuBool("DQ", "Draw Q Range");
            public static readonly MenuBool DE = new MenuBool("DE", "Draw E Range");
            public static readonly MenuBool DR = new MenuBool("DR", "Draw R Range");
            public static readonly MenuBool DaggerD = new MenuBool("DD", "Draw Dagger Range");
        }

        public class HaraSs
        {
            public static readonly MenuBool hQ = new MenuBool("useq", "Use Q");
            public static readonly MenuBool hW = new MenuBool("usew", "Use W");
            public static readonly MenuBool hE = new MenuBool("usee", "Use E");
        }

        public class Furmed
        {
            public static readonly MenuBool useq = new MenuBool("useq", "Use Q to Farm");
            public static readonly MenuBool lastq = new MenuBool("usew", "^- Only for Last Hit");
            public static readonly MenuBool lastaa = new MenuBool("usee", "Don't Last Hit in AA Range");
            public static readonly MenuBool usew = new MenuBool("useq", "Use W to Farm");
            public static readonly MenuSlider hhitwW = new MenuSlider("hitw", "^- if Hits", 3, 1, 6);
            public static readonly MenuBool usee = new MenuBool("usee", "Use E to Farm");
            public static readonly MenuSlider hite = new MenuSlider("useq", "^- if Dagger Hits", 3, 1, 6);
            public static readonly MenuBool turret = new MenuBool("usew", "Don't E Under the Turret");
        }

        public class LastHit
        {
            public static readonly MenuBool lastq = new MenuBool("DQ", "Use Q to Last Hit");
            public static readonly MenuBool lastaa = new MenuBool("DE", "Don't Last Hit in AA Range");
        }

        public class KillStealKat
        {
            public static readonly MenuBool KQ = new MenuBool("DQ", "Killsteal with Q");
            public static readonly MenuBool KE = new MenuBool("DE", "Killsteal with E");
            public static readonly MenuBool Ked = new MenuBool("DR", "^- Killsteal with E Dagger");
            public static readonly MenuBool KDD = new MenuBool("DD", "Gap with E for Q Killsteal");
        }

        public class Flee
        {

        }

        public static void OnLoad()
        {
            Q = new Spell(SpellSlot.Q, 625);
            W = new Spell(SpellSlot.W, 350);
            E = new Spell(SpellSlot.E, 725);
            R = new Spell(SpellSlot.R, 550);


            var MenuKat = new Menu("ensoulsharp.katarina", "EnsoulSharp.Katarina", true);
            //Combo
            var comboKat = new Menu("combo", "Combo");
            comboKat.Add(ComboKat.Q);
            comboKat.Add(ComboKat.W);
            comboKat.Add(ComboKat.E);
            comboKat.Add(ComboKat.R);
            //AddMenuCombo
            var rset = new Menu("rset", "R Settings");
            rset.Add(RSet.UseR);
            rset.Add(RSet.Ds);
            rset.Add(RSet.hitR);
            rset.Add(RSet.cancelR);
            rset.Add(RSet.caKR);
            rset.Add(RSet.waster);
            //Definity
            MenuKat.Add(comboKat);
            comboKat.Add(rset);
            //Harras
            var HarassMenu = new Menu("harass", "Harass");
            HarassMenu.Add(HaraSs.hQ);
            HarassMenu.Add(HaraSs.hW);
            HarassMenu.Add(HaraSs.hE);
            MenuKat.Add(HarassMenu);

            var ClearMenu = new Menu("farming1", "Farming");
            ClearMenu.Add(Furmed.useq);
            MenuKat.Add(ClearMenu);
            //Last
            var LastMenu = new Menu("lasthit", "Last Hit");
            LastMenu.Add(LastHit.lastq);
            LastMenu.Add(LastHit.lastaa);
            
            MenuKat.Add(LastMenu);
            //Ks
            var KSMenu = new Menu("killsteal", "Killsteal");
            KSMenu.Add(KillStealKat.KQ);
            KSMenu.Add(KillStealKat.KE);
            KSMenu.Add(KillStealKat.Ked);
            KSMenu.Add(KillStealKat.KDD);
            MenuKat.Add(KSMenu);
            //Draw
            var DrawMenu = new Menu("drawings", "Drawings");
            DrawMenu.Add(Draws.DQ);
            DrawMenu.Add(Draws.DE);
            DrawMenu.Add(Draws.DR);
            DrawMenu.Add(Draws.DaggerD);
            MenuKat.Add(DrawMenu);
            //Flee
            var FleeMenu = new Menu("flee", "Flee");
            FleeMenu.Add(new MenuBool("fleew", "Use W to Flee"));
            FleeMenu.Add(new MenuBool("fleee", "Use E to Flee"));
            FleeMenu.Add(new MenuBool("dagger", "^- Use E on Daggers"));
            //Loading
            MenuKat.Add(FleeMenu);
            MenuKat.Attach();

            Game.OnUpdate += OnTick;
            Drawing.OnDraw += OnDraw;
        }

        static double GetR(AIHeroClient target)
        {
            double meow = 0;
            if (GameObjects.Player.Spellbook.GetSpell(SpellSlot.R).Level == 1)
            {
                meow = 25;
            }
            if (GameObjects.Player.Spellbook.GetSpell(SpellSlot.R).Level == 2)
            {
                meow = 37.5;
            }
            if (GameObjects.Player.Spellbook.GetSpell(SpellSlot.R).Level == 3)
            {
                meow = 50;
            }
            double ap = GameObjects.Player.TotalAttackDamage * 0.19;
            double ad = (GameObjects.Player.TotalAttackDamage - GameObjects.Player.BaseAttackDamage) * 0.22;
            double full = ap + ad + meow;
            double damage = GameObjects.Player.CalculateDamage(target, DamageType.Magical, full);
            return damage;

        }
        private static void Killsteal()
        {
            foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q.Range) && !x.IsInvulnerable && x.Health < Q.GetDamage(x)))
            {
                if (Q.IsReady() && KillStealKat.KQ.Enabled)
                {
                    if (GameObjects.Player.GetSpellDamage(target,  SpellSlot.Q) >= target.Health && target.IsValidTarget(Q.Range))
                    {
                        if (Player.HasBuff("katarinarsound"))
                        {
                            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPosRaw);
                        }
                        Q.CastOnUnit(target);
                    }
                }
            }
            foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(E.Range) && !x.IsInvulnerable && x.Health < E.GetDamage(x)))
            {
                if (E.IsReady() && KillStealKat.KE.Enabled)
                {
                    if (target != null && GameObjects.Player.GetSpellDamage(target, SpellSlot.E) >= target.Health && target.IsValidTarget(E.Range))
                    {
                        if (Player.HasBuff("katarinarsound"))
                        {
                            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPosRaw);
                        }
                        E.CastOnUnit(target);
                    }
                }
            }

            if (Q.IsReady() && KillStealKat.KDD.Enabled)
            {
                foreach (var bestTarget in GameObjects.EnemyHeroes.Where(x => x.IsValid && !x.IsDead && !x.IsInvulnerable && x.Health < Q.GetDamage(x)))
                {
                    if (bestTarget != null &&
                        bestTarget.Distance(GameObjects.Player) > Q.Range &&
                        bestTarget.Health <= GameObjects.Player.GetSpellDamage(target, SpellSlot.Q))
                    {
                        foreach (var en in ObjectManager.Get<AIBaseClient>())
                        {
                            if (!en.IsDead &&
                                en.Distance(bestTarget) < Q.Range && en.Distance(GameObjects.Player) < E.Range)
                            {
                                if (Player.HasBuff("katarinarsound"))
                                {
                                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPosRaw);
                                }
                                E.Cast(en.Position);
                            }
                        }
                    }
                }
            }
        }

        private static void Harass()
        {
            bool useQ = HaraSs.hQ.Enabled;
            bool useW = HaraSs.hW.Enabled;
            bool useE = HaraSs.hE.Enabled;
            var target = TargetSelector.GetTarget(E.Range);
            if (target != null && target.IsValidTarget(E.Range))
            {
                if (!target.IsValidTarget())
                {
                    return;
                }

            if (Q.IsReady() && useQ && target.IsValidTarget(Q.Range))
                {
                    if (target != null)
                    {
                        Q.CastOnUnit(target);
                    }
                }

                if (E.IsReady() && useE && target.IsValidTarget(E.Range) && !Q.IsReady())
                {
                    if (target != null)
                    {
                        var dagger = ObjectManager.Get<AIBaseClient>().Where(a => a.Name == "HiddenMinion" && a.IsValid && !a.IsDead);
                        foreach (var daggers in GameObjects.AllGameObjects)

                        {
                            if (daggers.Name == "HiddenMinion" && !daggers.IsDead && daggers.IsValid)
                            {
                                if (target.Distance(daggers) < 450 &&
                                    target.IsValidTarget(E.Range))
                                {

                                    E.Cast(daggers.Position.Extend(target.Position, 200));


                                }
                                if (daggers.Distance(GameObjects.Player) > E.Range)
                                {
                                    E.Cast(target.Position.Extend(GameObjects.Player.Position, -50));
                                }
                                if (daggers.Distance(target) > 450)
                                {

                                    E.Cast(target.Position.Extend(GameObjects.Player.Position, -50));
                                }
                            }
                            if (dagger.Count() == 0)
                            {

                                E.Cast(target.Position.Extend(GameObjects.Player.Position, -50));
                            }

                        }

                    }
                }
                if (W.IsReady() && useW && target.IsValidTarget(W.Range))
                {
                    if (target != null)
                    {
                        W.Cast();
                    }
                }
            }
        }

        private static void OnTick(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.IsRecalling() || MenuGUI.IsChatOpen || ObjectManager.Player.IsWindingUp)
            {
                return;
            }

            if (Player.HasBuff("katarinarsound"))
            {
                Orbwalker.MovementState = false;
                Orbwalker.AttackState = false;

            }
            else
            {

                Orbwalker.MovementState = true;
                Orbwalker.AttackState = true;
            }


            Killsteal();

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    if (!GameObjects.Player.HasBuff("katarinarsound"))
                    {
                        Combat();
                    }
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LastHit:
                    Lasthit();
                    break;
                case OrbwalkerMode.LaneClear:
                    Clearing();
                    Jungle();
                    break;
            }
        }

        public static List<AIMinionClient> GetGenericJungleMinionsTargets()
        {
            return GetGenericJungleMinionsTargetsInRange(float.MaxValue);
        }

        public static List<AIMinionClient> GetGenericJungleMinionsTargetsInRange(float range)
        {
            return GameObjects.Jungle.Where(m => !GameObjects.JungleSmall.Contains(m) && m.IsValidTarget(range)).ToList();
        }

        public static List<AIMinionClient> GetEnemyLaneMinionsTargets()
        {
            return GetEnemyLaneMinionsTargetsInRange(float.MaxValue);
        }

        public static List<AIMinionClient> GetEnemyLaneMinionsTargetsInRange(float range)
        {
            return GameObjects.EnemyMinions.Where(m => m.IsValidTarget(range)).ToList();
        }

        private static void Jungle()
        {
            foreach (var jungleTarget in GetGenericJungleMinionsTargetsInRange(Q.Range))
            {

                if (jungleTarget.IsValidTarget(Q.Range))
                {
                    Q.CastOnUnit(jungleTarget);
                }
                if (jungleTarget.IsValidTarget(250f))
                {
                    W.Cast();
                }
            }
        }
        private static void Clearing()
        {
            //bool useQ = Furmed.useq.Enabled;
            //bool useW = Furmed.usew.Enabled;
            //bool useE = Farm.usee.Enabled;
            //float hitW = Furmed.hhitwW.Value;
            //float hitE = Farm.hite.Value;
            if (Q.IsReady())
            {
                foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
                {
                    if (minion.IsValidTarget(Q.Range) && minion != null && !Furmed.lastq.Enabled)
                    {
                        Q.CastOnUnit(minion);
                    }
                }
            }
            if (W.IsReady())
            {

                foreach (var minion in GetEnemyLaneMinionsTargetsInRange(300))
                {

                    if (minion.IsValidTarget(W.Range) && minion != null &&
                        GetEnemyLaneMinionsTargetsInRange(300).Count >= 3)
                    {

                        W.Cast();

                    }
                }
            }

        }

        private static void Lasthit()
        {
      

                foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
                {

                    if (minion.Health <= GameObjects.Player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        if (LastHit.lastaa.Enabled)
                        {
                            if (minion.Distance(GameObjects.Player) > 250)
                            {
                                Q.CastOnUnit(minion);
                            }
                        }
                        if (!LastHit.lastaa.Enabled)
                        {
                            Q.CastOnUnit(minion);
                        }

                    }
                
            };
        }

        private static void Combat()
        {
            float meow = RSet.waster.Value;
            float hitr = RSet.hitR.Value;
            var target = TargetSelector.GetTarget(925);
            if (target != null && target.IsValidTarget(925))
            {
                if (GameObjects.Player.HasBuff("katarinarsound"))
                {
                    if (GameObjects.Player.CountEnemyHeroesInRange(550 + 10) == 0)
                    {
                        GameObjects.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPosRaw);
                    }
                }
                if (GameObjects.Player.HasBuff("katarinarsound"))
                {
                    if (target.Distance(GameObjects.Player) >= R.Range - 100 && target != null && E.IsReady())
                    {
                        GameObjects.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPosRaw);
                        var dagger = ObjectManager.Get<AIBaseClient>().Where(a => a.Name == "HiddenMinion" && a.IsValid && !a.IsDead);
                        foreach (var daggers in GameObjects.AllGameObjects)

                        {
                            if (daggers.Name == "HiddenMinion" && !daggers.IsDead && daggers.IsValid && E.IsReady())
                            {
                                if (target.Distance(daggers) < 450 &&
                                    target.IsValidTarget(E.Range) && E.IsReady())

                                {

                                    E.Cast(daggers.Position.Extend(target.Position, 200));


                                }
                                if (daggers.Distance(GameObjects.Player) > E.Range)
                                {
                                    E.Cast(target.Position.Extend(GameObjects.Player.Position, -50));
                                }
                                if (daggers.Distance(target) > 450)
                                {

                                    E.Cast(target.Position.Extend(GameObjects.Player.Position, -50));
                                }
                            }
                            if (dagger.Count() == 0)
                            {

                                E.Cast(target.Position.Extend(GameObjects.Player.Position, -50));
                            }
                        }
                    }
                    if (GameObjects.Player.GetSpellDamage(target, SpellSlot.Q) + GameObjects.Player.GetSpellDamage(target, SpellSlot.E) >=
                        target.Health)
                    {

                        var dagger = ObjectManager.Get<AIBaseClient>().Where(a => a.Name == "HiddenMinion" && a.IsValid && !a.IsDead);
                        foreach (var daggers in GameObjects.AllGameObjects)

                        {
                            if (daggers.Name == "HiddenMinion" && !daggers.IsDead && daggers.IsValid && E.IsReady())
                            {
                                GameObjects.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPosRaw);
                                if (target.Distance(daggers) < 450 &&
                                    target.IsValidTarget(E.Range) && E.IsReady())

                                {

                                    E.Cast(daggers.Position.Extend(target.Position, 200));


                                }
                                if (daggers.Distance(GameObjects.Player) > E.Range)
                                {
                                    E.Cast(target.Position.Extend(GameObjects.Player.Position, -50));
                                }
                                if (daggers.Distance(target) > 450)
                                {

                                    E.Cast(target.Position.Extend(GameObjects.Player.Position, -50));
                                }
                            }
                            if (dagger.Count() == 0 && E.IsReady())
                            {
                                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPosRaw);
                                E.Cast(target.Position.Extend(GameObjects.Player.Position, -50));
                            }

                            if (target.IsValidTarget(Q.Range) && Q.IsReady())
                            {
                                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPosRaw);
                                Q.CastOnUnit(target);
                            }

                        }

                    }
                }
            }
            if (!target.IsValidTarget())
            {
                return;
            }
            if (E.IsReady() && target.IsValidTarget(E.Range))
            {
                if (target != null)
                {
                    var dagger = ObjectManager.Get<AIBaseClient>().Where(a => a.Name == "HiddenMinion" && a.IsValid && !a.IsDead);
                    foreach (var daggers in GameObjects.AllGameObjects)
                    {
                        if (daggers.Name == "HiddenMinion" && !daggers.IsDead && daggers.IsValid)
                        {
                            if (target.Distance(daggers) < 450 &&
                                target.IsValidTarget(E.Range))
                            {

                                E.Cast(daggers.Position.Extend(target.Position, 200));


                            }
                            if (daggers.Distance(GameObjects.Player) > E.Range)
                            {
                                E.Cast(target.Position.Extend(GameObjects.Player.Position, 50));
                            }
                            if (daggers.Distance(target) > 450)
                            {

                                E.Cast(target.Position.Extend(GameObjects.Player.Position, 50));
                            }
                        }
                        if (dagger.Count() == 0)
                        {
                            E.Cast(target.Position.Extend(GameObjects.Player.Position, 50));
                        }

                    }
                }
            }
            if (W.IsReady() && target.IsValidTarget(W.Range))
            {
                if (target != null)
                {
                    W.Cast();
                }
            }

            if (Q.IsReady() && target.IsValidTarget(Q.Range))
            {
                if (target != null)
                {
                    Q.CastOnUnit(target);
                }
            }
            if (R.IsReady() && target.IsValidTarget(R.Range - 50))
            {
                if (target != null && GameObjects.Player.CountEnemyHeroesInRange(R.Range - 150) >= hitr)
                {
                    if (target.Health > meow && !Q.IsReady())
                    {
                        R.Cast();
                    }
                }
            }
            if (R.IsReady() && target.IsValidTarget(R.Range - 150))
            {
                if (target != null && target.Health <=
                    GameObjects.Player.GetSpellDamage(target, SpellSlot.Q) +
                    GameObjects.Player.GetSpellDamage(target, SpellSlot.E) + GetR(target) * 8)
                {
                    if (target.Health > meow && !Q.IsReady())
                    {
                        R.Cast();
                    }
                }
            }
        }


        private static void OnDraw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || MenuGUI.IsChatOpen)
            {
                return;
            }
            if (Draws.DQ.Enabled && Q.IsReady())
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, Q.Range, Color.Red);
            }

            if (Draws.DE.Enabled &&  E.IsReady())
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, E.Range, Color.Crimson);
            }
            if (Draws.DR.Enabled &&  R.IsReady())
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, R.Range, Color.Crimson);
            }

            if (Draws.DaggerD.Enabled)
            {
                foreach (var daggers in GameObjects.AllGameObjects)
                {
                    if (daggers.Name == "HiddenMinion" && !daggers.IsDead && daggers.IsValid)
                    {
                        if (daggers.CountEnemyHeroesInRange(450) != 0)
                        {
                            Render.Circle.DrawCircle(daggers.Position, 355, Color.LawnGreen);
                            Render.Circle.DrawCircle(daggers.Position, 150, Color.LawnGreen);
                        }
                        if (daggers.CountEnemyHeroesInRange(450) == 0)
                        {
                            Render.Circle.DrawCircle(daggers.Position, 355, Color.Red);
                            Render.Circle.DrawCircle(daggers.Position, 150, Color.Red);
                        }
                    }
                }
            }
        }
    }
}

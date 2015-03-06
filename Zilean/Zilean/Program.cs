using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Zilean
{
    class Program
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        private static Orbwalking.Orbwalker Orbwalker;

        private static Spell Q, W, E, R;

        private static Menu Menu;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Zilean")
                return;

            Q = new Spell(SpellSlot.Q, 900);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 550);
            R = new Spell(SpellSlot.R, 900);
            //Q.SetSkillshot(300, 50, 2000, false, SkillshotType.SkillshotLine);


            Menu = new Menu(Player.ChampionName, Player.ChampionName, true);
            Menu orbwalkerMenu = new Menu("Orbwalker", "Orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu); 
            Menu.AddSubMenu(orbwalkerMenu);
            Menu ts = Menu.AddSubMenu(new Menu("Target Selector", "Target Selector")); ;
            TargetSelector.AddToMenu(ts);

            Menu spellMenu = Menu.AddSubMenu(new Menu("Spells", "Spells"));
            //spellMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            //spellMenu.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            spellMenu.AddItem(new MenuItem("Use E", "Use E")).SetValue(false);
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly))
            {
                spellMenu.AddItem(new MenuItem("use R" + hero.SkinName, "use R" + hero.SkinName)).SetValue(false);
            }
            //spellMenu.AddItem(new MenuItem("useR", "Use R to Farm").SetValue(true));
            //spellMenu.AddItem(new MenuItem("LaughButton", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
            //spellMenu.AddItem(new MenuItem("ConsumeHealth", "Consume below HP").SetValue(new Slider(40, 1, 100)));

            Menu.AddToMainMenu();

            //Drawing.OnDraw += Drawing_OnDraw;

            Game.OnUpdate += Game_OnGameUpdate;


            Game.PrintChat("Welcome to ZileanWorld");
        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                Harass();
            }
            Savior();
           

        }

        public static void Combo()
        {
            if (Q.IsReady())
            {
                var target = TargetSelector.GetTarget(920, TargetSelector.DamageType.Magical);
                float x = target.MoveSpeed;
                float y = x*500/1000;
                if (target == null)
                {
                    return;
                }
                else
                {
                    var t = Prediction.GetPrediction(target, 300).CastPosition;
                    
                    var pos = target.Position;
                    if (target.Distance(t) <= y)
                    {
                        pos = t;
                    }
                    if (target.Distance(t) > y)
                    {
                        pos = target.Position.Extend(t, y);
                    }
                    if (Player.Distance(pos) <= 899)
                    {
                        Q.Cast(pos);
                    }

                }
                

            }
            if (W.IsReady())
            {
                if (!Q.IsReady(3000) ||! Q.IsReady())
                W.Cast();
            }
            if (E.IsReady() && Menu.Item("Use E").GetValue<bool>())
            {
                var target = TargetSelector.GetTarget(550, TargetSelector.DamageType.Magical);
                if (target != null)
                    E.Cast(target);
            }
        }
        public static void Harass()
        {
            if (Q.IsReady())
            {
                var target = TargetSelector.GetTarget(920, TargetSelector.DamageType.Magical);
                float x = target.MoveSpeed;
                float y = x * 500 / 1000;
                if (target == null)
                {
                    return;
                }
                else
                {
                    var t = Prediction.GetPrediction(target, 300).CastPosition;

                    var pos = target.Position;
                    if (target.Distance(t) <= y)
                    {
                        pos = t;
                    }
                    if (target.Distance(t) > y)
                    {
                        pos = target.Position.Extend(t, y);
                    }
                    Render.Circle.DrawCircle(pos, 50, Color.Green);
                    if (Player.Distance(pos) <= 899)
                    {
                        Q.Cast(pos);
                    }

                }
                

            }
        }
        public static void Savior()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly))
            {
                if (hero.IsDead || hero.IsZombie)
                    return;
                if (!Menu.Item("use R" + hero.SkinName).GetValue<bool>())
                    return;
                if (hero.Health / hero.MaxHealth * 100 >= 15)
                    return;
                if (Player.Distance(hero.Position) > 900)
                    return;
                if (!R.IsReady())
                    return;
                R.Cast(hero);
            }
        }
    }
}

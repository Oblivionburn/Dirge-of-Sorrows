﻿using System;
using System.Timers;
using System.Diagnostics;
using System.Windows.Forms;
using System.Reflection;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Scenes;
using OP_Engine.Sounds;
using OP_Engine.Weathers;
using OP_Engine.Inventories;
using OP_Engine.Spells;
using OP_Engine.Characters;
using OP_Engine.Rendering;
using OP_Engine.Utility;
using OP_Engine.Time;

using DoS1.Scenes;
using DoS1.Menus;
using DoS1.Util;

namespace DoS1
{
    public class Main : Game
    {
        #region Variables

        public static OP_Game Game;
        public static string Version;
        public static bool LostFocus;

        public static int TimeSpeed = 1;
        public static int CombatSpeed = 2;
        public static double GameTime;
        public static System.Timers.Timer Timer = new System.Timers.Timer(1);

        #endregion

        #region Constructors

        public Main()
        {
            try
            {
                Game = new OP_Game
                {
                    Form = (Form)Control.FromHandle(Window.Handle),
                    Zoom = 2
                };
                Game.Init(this, Window);

                Timer.Elapsed += Timer_Elapsed;
            }
            catch (Exception e)
            {
                Game.CrashHandler(e);
            }
        }

        #endregion

        #region Methods

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                Version = fvi.FileVersion;

                LoadComponents();

                Game.SpriteBatch = new SpriteBatch(Game.GraphicsManager.GraphicsDevice);

                RenderingManager.InitDefaults(Game.GraphicsManager, Game.Resolution);
                RenderingManager.LightingRenderer = new LightingRenderer
                {
                    Name = "Lighting",
                    SetRenderTarget_BeforeDraw = true,
                    ClearGraphics_BeforeDraw = true,
                    ClearRenderTarget_AfterDraw = true,
                    BlendState = BlendState.Additive
                };
                RenderingManager.LightingRenderer.Init(Game.GraphicsManager, Game.Resolution);
                RenderingManager.AddLightingRenderer.RenderTarget = RenderingManager.LightingRenderer.RenderTarget;

                Handler.Init(this);
                Game.Zoom = 1.5f;

                if (!Game.GraphicsManager.IsFullScreen)
                {
                    Game.Form.WindowState = FormWindowState.Maximized;
                }

                LoadScenes();
                LoadMenus();

                SceneManager.ChangeScene("Logo");
                AssetManager.PlaySound("Logo");

                InputManager.MouseEnabled = true;
                InputManager.KeyboardEnabled = true;

                IsMouseVisible = true;

                Timer.Start();
            }
            catch (Exception e)
            {
                Game.CrashHandler(e);
            }
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            try
            {
                if (Window != null)
                {
                    if (Window.ClientBounds.Width > 0 &&
                        Window.ClientBounds.Height > 0)
                    {
                        if (!Game.Form.Focused)
                        {
                            if (Game.GameStarted &&
                                !LostFocus)
                            {
                                if (!TimeManager.Paused ||
                                    Handler.Combat)
                                {
                                    LostFocus = true;

                                    if (Handler.Combat)
                                    {
                                        GameUtil.Toggle_Pause_Combat(false);
                                    }
                                    else if (!TimeManager.Paused)
                                    {
                                        SoundManager.AmbientPaused = true;

                                        OP_Engine.Menus.Menu alerts = MenuManager.GetMenu("Alerts");

                                        OP_Engine.Menus.Menu currentMenu = MenuManager.GetCurrentMenu();
                                        if (currentMenu.Name != "Army" &&
                                            currentMenu.Name != "Squad" &&
                                            currentMenu.Name != "Character" &&
                                            currentMenu.Name != "Item" &&
                                            !alerts.Visible)
                                        {
                                            TimeManager.Paused = true;

                                            OP_Engine.Menus.Menu ui = MenuManager.GetMenu("UI");
                                            ui.Active = false;
                                            ui.Visible = false;

                                            MenuManager.ChangeMenu("Main");
                                        }
                                    }
                                }
                            }

                            SoundManager.Paused = true;
                        }
                        else if (Game.Form.Focused)
                        {
                            if (LostFocus)
                            {
                                LostFocus = false;

                                if (Handler.Combat)
                                {
                                    GameUtil.Toggle_Pause_Combat(false);
                                }
                            }

                            SoundManager.Paused = false;

                            InputManager.Update();
                            MenuManager.Update(Game.Game, Content);
                            SceneManager.Update(Game.Game, Content);
                            RenderingManager.Update();

                            if (!SoundManager.AmbientPaused)
                            {
                                WeatherManager.Update(Game.Resolution);
                            }
                        }
                    }
                    else if (!LostFocus)
                    {
                        LostFocus = true;

                        TimeManager.Paused = true;
                        SoundManager.Paused = true;

                        if (Handler.Combat)
                        {
                            GameUtil.Toggle_Pause_Combat(false);
                            Handler.CombatTimer.Stop();
                        }
                    }

                    SoundManager.Update();
                }

                if (Game.Quit)
                {
                    SoundManager.StopAll();
                    Game.Game.Exit();
                }
            }
            catch (Exception e)
            {
                Game.CrashHandler(e);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            if (Game.Window != null)
            {
                //Don't bother drawing if the window is minimized
                if (Game.Window.ClientBounds.Width > 0 &&
                    Game.Window.ClientBounds.Height > 0)
                {
                    if (Game.SpriteBatch != null)
                    {
                        if (RenderingManager.UsingDefaults &&
                            Game.GraphicsManager != null)
                        {
                            //Set ambient light in case the color changed
                            RenderingManager.LightingRenderer.GraphicsClearColor = RenderingManager.Lighting.DrawColor;

                            //Render lighting
                            RenderingManager.LightingRenderer.Draw(Game.SpriteBatch, Game.Resolution);

                            //Render world
                            Game.GraphicsManager.GraphicsDevice.Clear(Color.Black);
                            Game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
                            SceneManager.Draw_WorldsOnly(Game.SpriteBatch, Game.Resolution, Color.White);
                            Game.SpriteBatch.End();

                            //Add lighting to world
                            if (!Handler.Combat)
                            {
                                RenderingManager.AddLightingRenderer.Draw(Game.SpriteBatch, Game.Resolution);
                            }

                            Game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

                            //Alt method with no lighting applied
                            SceneManager.Draw_WorldsOnly(Game.SpriteBatch, Game.Resolution);

                            //Render weather underneath title menu
                            if (SceneManager.GetScene("Title").Visible &&
                                !Handler.Combat &&
                                !SceneManager.GetScene("GameOver").Visible)
                            {
                                WeatherManager.Draw(Game.SpriteBatch);
                            }

                            //Render scene specific menus
                            SceneManager.Draw_MenusOnly(Game.SpriteBatch);

                            //Render weather ontop of scene but underneath menus
                            if (Handler.LocalMap &&
                                !Handler.Combat &&
                                !SceneManager.GetScene("GameOver").Visible)
                            {
                                WeatherManager.Draw(Game.SpriteBatch);
                            }

                            if (WeatherManager.Flash)
                            {
                                Game.SpriteBatch.Draw(AssetManager.Textures["White"], new Rectangle(0, 0, Game.ScreenWidth, Game.ScreenHeight), Color.White * 0.75f);
                                AssetManager.PlaySound_Random("Thunder");
                                WeatherManager.Flash = false;
                            }

                            //Render standalone menus
                            MenuManager.Draw(Game.SpriteBatch);

                            Game.SpriteBatch.End();
                        }

                        int count = RenderingManager.Renderers.Count;
                        for (int i = 0; i < count; i++)
                        {
                            RenderingManager.Renderers[i].Draw(Game.SpriteBatch, Game.Resolution);
                        }
                    }
                }
            }
        }

        private void LoadComponents()
        {
            Components.Add(new InputManager(this));
            Components.Add(new SoundManager(this));
            Components.Add(new MenuManager(this));
            Components.Add(new SceneManager(this));
            Components.Add(new WeatherManager(this));
            Components.Add(new InventoryManager(this));
            Components.Add(new SpellbookManager(this));
            Components.Add(new CharacterManager(this));
            Components.Add(new Handler(this));
        }

        private void LoadScenes()
        {
            SceneManager.Scenes.Add(new Logo());
            SceneManager.Scenes.Add(new Title(Content));
            SceneManager.Scenes.Add(new Worldmap(Content));
            SceneManager.Scenes.Add(new Localmap(Content));
            SceneManager.Scenes.Add(new Combat(Content));
            SceneManager.Scenes.Add(new GameOver(Content));
        }

        private void LoadMenus()
        {
            MenuManager.Menus.Add(new Menu_UI(Content));
            MenuManager.Menus.Add(new Menu_Main(Content));
            MenuManager.Menus.Add(new Menu_Save_Load(Content));
            MenuManager.Menus.Add(new Menu_CharGen(Content));
            MenuManager.Menus.Add(new Menu_Options(Content));
            MenuManager.Menus.Add(new Menu_Army(Content));
            MenuManager.Menus.Add(new Menu_Squad(Content));
            MenuManager.Menus.Add(new Menu_Character(Content));
            MenuManager.Menus.Add(new Menu_Inventory(Content));
            MenuManager.Menus.Add(new Menu_Item(Content));
            MenuManager.Menus.Add(new Menu_Shop(Content));
            MenuManager.Menus.Add(new Menu_Academy(Content));
            MenuManager.Menus.Add(new Menu_Alerts(Content));
            MenuManager.Menus.Add(new Menu_CharEdit(Content));
        }

        public static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!TimeManager.Paused &&
                !Handler.LocalPause)
            {
                GameTime++;

                if (RenderingManager.Lighting.FadingIn)
                {
                    if (RenderingManager.Lighting.LerpAmount < 1)
                    {
                        RenderingManager.Lighting.LerpAmount += 0.025f;
                        RenderingManager.Lighting.FadeIn();
                    }
                    else
                    {
                        RenderingManager.Lighting.FadingIn = false;
                        RenderingManager.Lighting.LerpAmount = 0;
                    }
                }
                else if (RenderingManager.Lighting.FadingOut)
                {
                    if (RenderingManager.Lighting.LerpAmount < 1)
                    {
                        RenderingManager.Lighting.LerpAmount += 0.025f;
                        RenderingManager.Lighting.FadeOut();
                    }
                    else
                    {
                        RenderingManager.Lighting.FadingOut = false;
                        RenderingManager.Lighting.LerpAmount = 0;
                    }
                }

                TimeManager.Now.AddTime(TimeRate.Second, TimeSpeed);
            }
        }

        #endregion
    }
}
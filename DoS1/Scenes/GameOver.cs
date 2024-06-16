﻿using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Scenes;
using OP_Engine.Sounds;
using OP_Engine.Utility;

using DoS1.Util;

namespace DoS1.Scenes
{
    public class GameOver : Scene
    {
        #region Variables

        public float value;
        public bool Increasing;
        public int delay = 200;

        #endregion

        #region Constructor

        public GameOver(ContentManager content)
        {
            ID = Handler.GetID();
            Menu.ID = Handler.GetID();

            Name = "GameOver";
            Menu.Name = "GameOver";

            Load();
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible)
            {
                if (SoundManager.NeedMusic)
                {
                    SoundManager.MusicLooping = true;
                    AssetManager.PlayMusic_Random("GameOver", true);
                }

                if (InputManager.Mouse_LB_Pressed)
                {
                    InputManager.Mouse.Flush();
                    GameUtil.ReturnToTitle();
                }

                if (Increasing)
                {
                    delay--;
                    if (delay == 0)
                    {
                        value += 1;

                        if (value >= 255)
                        {
                            Increasing = false;
                        }

                        delay = 5;
                    }
                }
            }
        }

        public override void DrawMenu(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                foreach (Picture picture in Menu.Pictures)
                {
                    picture.DrawColor.R = (byte)value;
                    picture.DrawColor.G = (byte)value;
                    picture.DrawColor.B = (byte)value;

                    picture.Draw(spriteBatch);
                }
            }
        }

        public override void Load()
        {
            Increasing = true;
            value = 0;

            Menu.AddPicture(0, "GameOver", AssetManager.Textures["GameOver"], new Region(0, 0, 0, 0), new Color(0, 0, 0, 255), true);
            Menu.Visible = true;

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            Menu.GetPicture("GameOver").Region = new Region(0, 0, Main.Game.ScreenWidth, Main.Game.ScreenHeight);
        }

        #endregion
    }
}

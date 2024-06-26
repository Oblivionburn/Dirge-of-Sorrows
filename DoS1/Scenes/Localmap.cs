﻿using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Scenes;
using OP_Engine.Tiles;
using OP_Engine.Characters;
using OP_Engine.Sounds;
using OP_Engine.Utility;

using DoS1.Util;

namespace DoS1.Scenes
{
    public class Localmap : Scene
    {
        #region Variables



        #endregion

        #region Constructors

        public Localmap(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Localmap";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            base.Update(gameRef, content);

            if (Visible ||
                Active)
            {
                if (SoundManager.NeedMusic)
                {
                    SoundManager.MusicLooping = false;

                    if (World.Maps.Any())
                    {
                        Map map = World.Maps[Handler.Level];

                        if (map.Type.Contains("Snow") ||
                            map.Type.Contains("Ice"))
                        {
                            AssetManager.PlayMusic_Random("Snowy", false);
                        }
                        else if (map.Type.Contains("Desert"))
                        {
                            AssetManager.PlayMusic_Random("Desert", false);
                        }
                        else
                        {
                            AssetManager.PlayMusic_Random("Plains", false);
                        }
                    }
                }
            }
        }

        public override void DrawWorld(SpriteBatch spriteBatch, Point resolution, Color color)
        {
            if (Visible)
            {
                foreach (Map map in World.Maps)
                {
                    if (map.Visible)
                    {
                        foreach (Layer layer in map.Layers)
                        {
                            if (layer.Name == "Pathing")
                            {
                                layer.Draw(spriteBatch, resolution, Color.White);
                            }
                            else
                            {
                                layer.Draw(spriteBatch, resolution, color);
                            }
                        }
                    }
                }

                foreach (Army army in CharacterManager.Armies)
                {
                    foreach (Squad squad in army.Squads)
                    {
                        if (squad.Characters.Any() &&
                            squad.Visible)
                        {
                            squad.Draw(spriteBatch, resolution, color);
                        }
                    }
                }
            }
        }

        public override void Load(ContentManager content)
        {
            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            if (World.Maps.Any())
            {
                WorldUtil.Resize_OnZoom(World.Maps[Handler.Level]);
            }
        }

        #endregion
    }
}

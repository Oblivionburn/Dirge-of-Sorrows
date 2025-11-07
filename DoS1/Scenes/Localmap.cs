using System.Linq;

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

                if (Handler.StoryStep <= 5 ||
                    (Handler.StoryStep >= 7 && Handler.StoryStep <= 9) ||
                    Handler.StoryStep == 14 ||
                    Handler.StoryStep == 19 ||
                    (Handler.StoryStep >= 22 && Handler.StoryStep <= 28) ||
                    Handler.StoryStep == 36 ||
                    Handler.StoryStep == 37 ||
                    (Handler.StoryStep >= 45 && Handler.StoryStep <= 48) ||
                    Handler.StoryStep == 55)
                {
                    GameUtil.Alert_Story();
                }
            }
        }

        public override void DrawWorld(SpriteBatch spriteBatch, Point resolution, Color color)
        {
            if (Visible)
            {
                for (int i = 0; i < World.Maps.Count; i++)
                {
                    Map map = World.Maps[i];
                    if (map.Visible)
                    {
                        foreach (Layer layer in map.Layers)
                        {
                            if (layer.Name != "Pathing")
                            {
                                layer.Draw(spriteBatch, resolution, color);
                            }
                        }
                    }
                }
            }
        }

        public override void DrawWorld(SpriteBatch spriteBatch, Point resolution)
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
                                break;
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
                            squad.Draw(spriteBatch, resolution, Color.White);
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
                WorldUtil.Resize_OnZoom(World.Maps[Handler.Level], false);
            }
        }

        #endregion
    }
}

﻿using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Rendering;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Scenes;
using OP_Engine.Characters;

namespace DoS1
{
    public class LightingRenderer : Renderer
    {
        #region Variables



        #endregion

        #region Constructors

        public LightingRenderer() : base()
        {

        }

        #endregion

        #region Methods

        public override void Update()
        {
            Handler.light_maps.Clear();

            Map map = null;
            string sceneType = "Worldmap";

            Scene scene = SceneManager.GetScene(sceneType);
            if (Handler.LocalMap)
            {
                sceneType = "Localmap";
                scene = SceneManager.GetScene(sceneType);
                map = scene.World.Maps[Handler.Level];
            }
            else if (scene.World.Maps.Count > 0)
            {
                map = scene.World.Maps[0];
            }

            if (map != null)
            {
                List<Vector2> light_sources = new List<Vector2>();

                if (sceneType == "Localmap")
                {
                    foreach (Army army in CharacterManager.Armies)
                    {
                        foreach (Squad squad in army.Squads)
                        {
                            if (squad.Characters.Any() &&
                                squad.Visible)
                            {
                                light_sources.Add(new Vector2(squad.Region.X + (squad.Region.Width / 2), squad.Region.Y + (squad.Region.Height / 2)));
                            }
                        }
                    }
                }

                Layer locations = map.GetLayer("Locations");
                foreach (Tile tile in locations.Tiles)
                {
                    if (tile.Visible)
                    {
                        light_sources.Add(new Vector2(tile.Region.X + (tile.Region.Width / 2), tile.Region.Y + (tile.Region.Height / 2)));
                    }
                }

                foreach (Vector2 light_source in light_sources)
                {
                    int width = Main.Game.TileSize.X * (Handler.light_tile_distance * 2) + 1;
                    int half_width = Main.Game.TileSize.X * Handler.light_tile_distance;
                    int x = (int)light_source.X - half_width;
                    int y = (int)light_source.Y - half_width;

                    Handler.light_maps.Add(new Something
                    {
                        Region = new Region(x, y, width, width),
                        DrawColor = new Color(255, 240, 160, 255)
                    });
                }
            }
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D light = AssetManager.Textures["point_light"];

            for (int i = 0; i < Handler.light_maps.Count; i++)
            {
                Something map = Handler.light_maps[i];

                Rectangle region = new Rectangle((int)map.Region.X, (int)map.Region.Y, (int)map.Region.Width, (int)map.Region.Height);
                Rectangle image = new Rectangle(0, 0, light.Width, light.Height);

                spriteBatch.Draw(light, region, image, map.DrawColor);
            }
        }

        #endregion
    }
}

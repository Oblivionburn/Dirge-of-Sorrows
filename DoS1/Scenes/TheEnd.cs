using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Scenes;
using OP_Engine.Menus;
using OP_Engine.Controls;
using OP_Engine.Utility;
using OP_Engine.Sounds;
using DoS1.Util;

namespace DoS1.Scenes
{
    public class TheEnd : Scene
    {
        #region Variables

        

        #endregion

        #region Constructor

        public TheEnd(ContentManager content)
        {
            ID = Handler.GetID();
            Menu.ID = Handler.GetID();

            Name = "TheEnd";
            Menu.Name = "TheEnd";

            Load(content);
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
                    AssetManager.PlayMusic_Random("Victory", true);
                }

                if (Handler.StoryStep >= 92 &&
                    Handler.StoryStep <= 98)
                {
                    StoryUtil.Alert_Story(Menu);
                }
            }
        }

        public override void DrawMenu(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                foreach (Picture picture in Menu.Pictures)
                {
                    picture.Draw(spriteBatch);
                }
            }
        }

        public override void Load(ContentManager content)
        {
            Menu.AddPicture(0, "Background", AssetManager.Textures["Black"], new Region(0, 0, 0, 0), Color.White, true);
            Menu.AddPicture(0, "Victory", AssetManager.Textures["Victory"], new Region(0, 0, 0, 0), Color.White, true);
            Menu.Visible = true;

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            Menu.GetPicture("Background").Region = new Region(0, 0, Main.Game.ScreenWidth, Main.Game.ScreenHeight);
            Menu.GetPicture("Victory").Region = new Region(0, 0, Main.Game.ScreenWidth, Main.Game.ScreenHeight);
        }

        #endregion
    }
}

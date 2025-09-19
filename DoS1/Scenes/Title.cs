using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Scenes;
using OP_Engine.Menus;
using OP_Engine.Sounds;
using OP_Engine.Controls;
using OP_Engine.Utility;

namespace DoS1.Scenes
{
    public class Title : Scene
    {
        #region Variables



        #endregion

        #region Constructor

        public Title(ContentManager content)
        {
            ID = Handler.GetID();
            Menu.ID = Handler.GetID();

            Name = "Title";
            Menu.Name = "Title";

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
                    AssetManager.PlayMusic_Random("Title", true);
                }
            }
        }

        public override void DrawMenu(SpriteBatch spriteBatch)
        {
            if (Visible &&
                Menu.Visible)
            {
                foreach (Picture existing in Menu.Pictures)
                {
                    existing.Draw(spriteBatch);
                }

                foreach (Label label in Menu.Labels)
                {
                    label.Draw(spriteBatch);
                }
            }
        }

        public override void Load(ContentManager content)
        {
            Menu.Clear();

            Menu.AddPicture(Handler.GetID(), "Title", AssetManager.Textures["Title2"], new Region(0, 0, 0, 0), Color.White, true);

            Menu.Visible = true;

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            Menu.GetPicture("Title").Region = new Region(0, 0, Main.Game.ScreenWidth, Main.Game.ScreenHeight);
        }

        #endregion
    }
}

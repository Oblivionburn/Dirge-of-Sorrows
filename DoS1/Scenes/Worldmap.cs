using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using OP_Engine.Scenes;
using OP_Engine.Tiles;
using OP_Engine.Sounds;
using OP_Engine.Utility;

using DoS1.Util;

namespace DoS1.Scenes
{
    public class Worldmap : Scene
    {
        #region Variables

        

        #endregion

        #region Constructors

        public Worldmap(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Worldmap";
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
                    AssetManager.PlayMusic_Random("Worldmap", true);
                }

                if (Handler.StoryStep == -1)
                {
                    GameUtil.Alert_Tutorial();
                }
                else if (Handler.StoryStep == 0)
                {
                    GameUtil.Alert_Story();
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
                WorldUtil.Resize_OnZoom(World.Maps[0], true);
            }
        }

        #endregion
    }
}

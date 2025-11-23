using System.Threading;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using OP_Engine.Controls;
using OP_Engine.Menus;
using OP_Engine.Utility;

using DoS1.Util;

namespace DoS1.Menus
{
    public class Menu_Loading : Menu
    {
        #region Variables



        #endregion

        #region Constructor

        public Menu_Loading(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Loading";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible)
            {
                foreach (Label label in Labels)
                {
                    label.Update();
                }

                ProgressBar bar = GetProgressBar("Loading1");
                bar.Max_Value = 1;

                UpdateMessagebar();

                if (Handler.Loading_Step == 0)
                {
                    if (Handler.Loading_Percent == 0 &&
                        Handler.Loading_Task == null)
                    {
                        Handler.Loading_TokenSource = new CancellationTokenSource();
                        Handler.Loading_Task = Task.Factory.StartNew(() => InventoryUtil.GenAssets(), Handler.Loading_TokenSource.Token);
                    }

                    if (Handler.Loading_Task != null)
                    {
                        if (Handler.Loading_Task.Status == TaskStatus.RanToCompletion)
                        {
                            Handler.Loading_Task = null;
                            Handler.Loading_TokenSource.Dispose();
                            Handler.Loading_Percent = 0;
                            Handler.Loading_Step++;
                        }
                    }
                }
                else if (Handler.Loading_Step == 1)
                {
                    Handler.Loaded = true;
                    Handler.Loading_Step++;

                    Finish();
                }
            }
        }

        private void UpdateMessagebar()
        {
            ProgressBar bar = GetProgressBar("Loading1");
            Label label = GetLabel("Loading1");

            bar.Value = Handler.Loading_Percent;
            label.Text = Handler.Loading_Message + " (" + Handler.Loading_Percent.ToString() + "%)";

            float CurrentVal = ((float)bar.Bar_Texture.Width / 100) * bar.Value;
            bar.Bar_Image = new Rectangle(bar.Bar_Image.X, bar.Bar_Image.Y, (int)CurrentVal, bar.Bar_Image.Height);

            CurrentVal = ((float)bar.Base_Region.Width / 100) * bar.Value;
            bar.Bar_Region = new Region(bar.Base_Region.X, bar.Base_Region.Y, (int)CurrentVal, bar.Base_Region.Height);
        }

        private void Finish()
        {
            if (Handler.Saves.Count > 0)
            {
                MenuManager.ChangeMenu("Save_Load");
            }
            else
            {
                MenuManager.ChangeMenu("CharGen");
            }
        }

        public override void Load(ContentManager content)
        {
            Clear();

            AddProgressBar(Handler.GetID(), "Loading1", 100, 0, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), new Color(150, 0, 0, 255), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Loading1", "Initializing (0%)", Color.White, new Region(0, 0, 0, 0), true);

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            int Width = Main.Game.MenuSize_X;
            int Height = Main.Game.MenuSize_Y;

            int X = Main.Game.ScreenWidth / 2;
            int Y = (Main.Game.ScreenHeight / 20) * 8;

            ProgressBar bar = GetProgressBar("Loading1");
            bar.Base_Region = new Region(X - (Width * 8), Y, Width * 16, Height);
            GetLabel("Loading1").Region = new Region(X - (Width * 6), bar.Base_Region.Y, Width * 12, Height);
        }

        #endregion
    }
}

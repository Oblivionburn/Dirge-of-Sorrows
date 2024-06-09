using System;
using System.IO;
using System.Xml;

using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Sounds;

namespace DoS1.Util
{
    public static class Load
    {
        #region Variables



        #endregion

        #region Constructors



        #endregion

        #region Methods

        #region Parse INI for Options

        public static void ParseINI(string file)
        {
            using (XmlTextReader reader = new XmlTextReader(File.OpenRead(file)))
            {
                try
                {
                    while (reader.Read())
                    {
                        switch (reader.Name)
                        {
                            case "Game":
                                VisitGame(reader);
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Main.Game.CrashHandler(e);
                }
            }
        }

        private static void VisitGame(XmlTextReader reader)
        {
            while (reader.Read())
            {
                if (reader.Name == "Game" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Options":
                        VisitOptions(reader);
                        break;
                }
            }
        }

        private static void VisitOptions(XmlTextReader reader)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Fullscreen":
                        if (reader.Value == "True")
                        {
                            Main.Game.GraphicsManager.IsFullScreen = true;
                        }
                        else
                        {
                            Main.Game.GraphicsManager.IsFullScreen = false;
                        }

                        Main.Game.GraphicsManager.ApplyChanges();
                        break;

                    case "Resolution":
                        var parts = reader.Value.Split(',');
                        int X = int.Parse(parts[0]);
                        int Y = int.Parse(parts[1]);

                        if (Main.Game.GraphicsManager.IsFullScreen)
                        {
                            Main.Game.GraphicsManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                            Main.Game.GraphicsManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                        }
                        else
                        {
                            Main.Game.Form.Width = X;
                            Main.Game.Form.Height = Y;
                            Main.Game.GraphicsManager.PreferredBackBufferWidth = Main.Game.Form.Width;
                            Main.Game.GraphicsManager.PreferredBackBufferHeight = Main.Game.Form.Height;
                        }

                        Main.Game.ScreenWidth = X;
                        Main.Game.ScreenHeight = Y;

                        Main.Game.ResolutionChange();

                        Main.Game.GraphicsManager.ApplyChanges();
                        break;

                    case "MusicEnabled":
                        if (reader.Value == "True")
                        {
                            SoundManager.MusicEnabled = true;
                        }
                        else
                        {
                            SoundManager.MusicEnabled = false;
                        }
                        break;

                    case "MusicVolume":
                        SoundManager.MusicVolume = float.Parse(reader.Value) / 10;
                        break;

                    case "AmbientEnabled":
                        if (reader.Value == "True")
                        {
                            SoundManager.AmbientEnabled = true;
                        }
                        else
                        {
                            SoundManager.AmbientEnabled = false;
                        }
                        break;

                    case "AmbientVolume":
                        SoundManager.AmbientVolume = float.Parse(reader.Value) / 10;
                        break;

                    case "SoundEnabled":
                        if (reader.Value == "True")
                        {
                            SoundManager.SoundEnabled = true;
                        }
                        else
                        {
                            SoundManager.SoundEnabled = false;
                        }
                        break;

                    case "SoundVolume":
                        SoundManager.SoundVolume = float.Parse(reader.Value) / 10;
                        break;
                }
            }
        }

        #endregion

        #endregion
    }
}

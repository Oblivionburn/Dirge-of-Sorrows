using System;
using System.IO;
using System.Xml;

using OP_Engine.Sounds;
using OP_Engine.Utility;

namespace DoS1.Util
{
    public static class Save
    {
        #region Variables

        private static Stream SaveStream;
        private static XmlWriter Writer;

        #endregion

        #region Constructors



        #endregion

        #region Methods

        #region XML Methods

        private static void WriteStream(string path)
        {
            SaveStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.IndentChars = "\t";
            Writer = XmlWriter.Create(SaveStream, xmlWriterSettings);
            Writer.WriteStartDocument();
        }

        private static void EnterNode(string elementName)
        {
            Writer.WriteStartElement(elementName);
        }

        private static void ExitNode()
        {
            Writer.WriteEndElement();
        }

        private static void FinalizeWriting()
        {
            Writer.WriteEndDocument();
            Writer.Close();
            SaveStream.Close();
        }

        #endregion

        #region Export INI

        public static void ExportINI()
        {
            try
            {
                string file = AssetManager.Files["Config"];
                WriteStream(file);
                SaveINI();
            }
            catch (Exception e)
            {
                Main.Game.CrashHandler(e);
            }
            finally
            {
                FinalizeWriting();
            }
            GC.Collect();
        }

        private static void SaveINI()
        {
            EnterNode("Game");

            #region Options

            EnterNode("Options");
            Writer.WriteAttributeString("Fullscreen", Main.Game.GraphicsManager.IsFullScreen.ToString());
            Writer.WriteAttributeString("Resolution", Main.Game.Resolution.X.ToString() + "," + Main.Game.Resolution.Y.ToString());
            Writer.WriteAttributeString("MusicEnabled", SoundManager.MusicEnabled.ToString());
            Writer.WriteAttributeString("MusicVolume", (SoundManager.MusicVolume * 10).ToString());
            Writer.WriteAttributeString("AmbientEnabled", SoundManager.AmbientEnabled.ToString());
            Writer.WriteAttributeString("AmbientVolume", (SoundManager.AmbientVolume * 10).ToString());
            Writer.WriteAttributeString("SoundEnabled", SoundManager.SoundEnabled.ToString());
            Writer.WriteAttributeString("SoundVolume", (SoundManager.SoundVolume * 10).ToString());
            ExitNode();

            #endregion

            #region Speed

            EnterNode("Speed");
            Writer.WriteAttributeString("TimeSpeed", Main.TimeSpeed.ToString());
            Writer.WriteAttributeString("CombatSpeed", Main.CombatSpeed.ToString());
            ExitNode();

            #endregion

            ExitNode();
        }

        #endregion

        #endregion
    }
}

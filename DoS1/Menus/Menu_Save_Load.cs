using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Scenes;
using OP_Engine.Utility;
using OP_Engine.Enums;
using DoS1.Util;

namespace DoS1.Menus
{
    public class Menu_Save_Load : Menu
    {
        #region Variables

        int Top = 1;

        #endregion

        #region Constructor

        public Menu_Save_Load(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Save_Load";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible ||
                Active)
            {
                UpdateControls();
                base.Update(gameRef, content);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                foreach (Picture existing in Pictures)
                {
                    if (existing.Name == "Background")
                    {
                        existing.Draw(spriteBatch);
                        break;
                    }
                }

                foreach (Picture existing in Pictures)
                {
                    if (existing.Name == "SaveList")
                    {
                        existing.Draw(spriteBatch);
                        break;
                    }
                }

                foreach (Button existing in Buttons)
                {
                    existing.Draw(spriteBatch);
                }

                foreach (Label existing in Labels)
                {
                    if (existing.Name != "Examine")
                    {
                        existing.Draw(spriteBatch);
                    }
                }

                foreach (Picture existing in Pictures)
                {
                    if (existing.Name != "Background" &&
                        existing.Name != "SaveList" &&
                        existing.Name != "Loading")
                    {
                        existing.Draw(spriteBatch);
                    }
                }

                foreach (Label existing in Labels)
                {
                    if (existing.Name == "Examine")
                    {
                        existing.Draw(spriteBatch);
                        break;
                    }
                }

                foreach (Picture existing in Pictures)
                {
                    if (existing.Name == "Loading" &&
                        existing.Visible)
                    {
                        existing.Draw(spriteBatch);
                        break;
                    }
                }
            }
        }

        private void UpdateControls()
        {
            bool hovering_back = Hovering_Back();
            bool hovering_new = Hovering_New();
            bool hovering_delete = Hovering_Delete();

            bool hovering_save = false;
            if (!hovering_delete)
            {
                hovering_save = Hovering_Save();
            }

            if (!hovering_back &&
                !hovering_delete &&
                !hovering_new)
            {
                GetLabel("Examine").Visible = false;
            }

            Picture saveList = GetPicture("SaveList");
            if (InputManager.MouseWithin(saveList.Region.ToRectangle))
            {
                if (InputManager.Mouse_ScrolledDown)
                {
                    Top++;

                    if (Top > Handler.Saves.Count - 4)
                    {
                        Top = Handler.Saves.Count - 4;
                    }

                    if (Top <= 1)
                    {
                        Top = 1;
                    }

                    Load();
                }
                else if (InputManager.Mouse_ScrolledUp)
                {
                    Top--;
                    if (Top <= 1)
                    {
                        Top = 1;
                    }

                    Load();
                }
            }

            if (Top > 1)
            {
                GetPicture("Arrow_Up").DrawColor = Color.White;
            }
            else
            {
                GetPicture("Arrow_Up").DrawColor = Color.White * 0f;
            }

            if (Handler.Saves.Count > 5 &&
                Top + 4 < Handler.Saves.Count)
            {
                GetPicture("Arrow_Down").DrawColor = Color.White;
            }
            else
            {
                GetPicture("Arrow_Down").DrawColor = Color.White * 0f;
            }

            if (InputManager.KeyPressed("Esc"))
            {
                Close();
            }
        }

        private bool Hovering_Back()
        {
            bool found = false;

            Button back = GetButton("Back");
            if (InputManager.MouseWithin(back.Region.ToRectangle))
            {
                found = true;
                if (back.HoverText != null)
                {
                    GameUtil.Examine(this, back.HoverText);
                }

                back.Opacity = 1;
                back.Selected = true;

                if (InputManager.Mouse_LB_Pressed)
                {
                    found = false;
                    CheckClick(back);

                    back.Opacity = 0.8f;
                    back.Selected = false;
                }
            }
            else if (InputManager.Mouse.Moved)
            {
                back.Opacity = 0.8f;
                back.Selected = false;
            }

            return found;
        }

        private bool Hovering_New()
        {
            bool found = false;

            Button back = GetButton("New");
            if (InputManager.MouseWithin(back.Region.ToRectangle))
            {
                found = true;
                if (back.HoverText != null)
                {
                    GameUtil.Examine(this, back.HoverText);
                }

                back.Opacity = 1;
                back.Selected = true;

                if (InputManager.Mouse_LB_Pressed)
                {
                    found = false;
                    CheckClick(back);

                    back.Opacity = 0.8f;
                    back.Selected = false;
                }
            }
            else if (InputManager.Mouse.Moved)
            {
                back.Opacity = 0.8f;
                back.Selected = false;
            }

            return found;
        }

        private bool Hovering_Save()
        {
            bool found = false;

            foreach (Button button in Buttons)
            {
                if (button.Name != "Back" &&
                    !button.Name.Contains("Delete"))
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        found = true;
                        button.Opacity = 1;
                        button.Selected = true;

                        SaveHighlight(button.Name);

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            found = false;
                            CheckClick(button);

                            button.Opacity = 0.8f;
                            button.Selected = false;

                            SaveDim(button.Name);

                            break;
                        }
                    }
                    else
                    {
                        button.Opacity = 0.8f;
                        button.Selected = false;

                        SaveDim(button.Name);
                    }
                }
            }

            return found;
        }

        private bool Hovering_Delete()
        {
            bool found = false;

            foreach (Button button in Buttons)
            {
                if (button.Name.Contains("Delete"))
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        found = true;
                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(this, button.HoverText);
                        }

                        button.Opacity = 1;
                        button.Selected = true;

                        foreach (Button existing in Buttons)
                        {
                            if (existing.Name != "Back" &&
                                !existing.Name.Contains("Delete"))
                            {
                                existing.Opacity = 0.8f;
                                existing.Selected = false;

                                SaveDim(existing.Name);
                            }
                        }

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            found = false;
                            CheckClick(button);

                            button.Opacity = 0.8f;
                            button.Selected = false;

                            break;
                        }
                    }
                    else
                    {
                        button.Opacity = 0.8f;
                        button.Selected = false;
                    }
                }
            }

            return found;
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse.Flush();

            button.Opacity = 0.8f;
            button.Selected = false;

            if (button.Name == "Back")
            {
                Close();
            }
            else if (button.Name == "New")
            {
                MenuManager.ChangeMenu("CharGen");
            }
            else if (button.Name.Contains("Delete"))
            {
                string name_part = button.Name.Split('_')[0];
                string name_num = name_part.Substring(4);
                int num = int.Parse(name_num);

                string save = Handler.Saves[num - 1];
                string savePath = Path.Combine(AssetManager.Directories["Saves"], save);

                Directory.Delete(savePath, true);
                ClearSaves();
                Handler.Saves.Remove(save);

                if (Top + 4 > Handler.Saves.Count)
                {
                    Top--;
                    if (Top < 1)
                    {
                        Top = 1;
                    }
                }

                Load();

                InputManager.Mouse.Flush();
            }
            else
            {
                GetPicture("Loading").Visible = true;

                Active = false;

                int num = int.Parse(button.Name);
                string save = Handler.Saves[num - 1];
                Handler.Selected_Save = save;

                Task.Factory.StartNew(() => GameUtil.LoadGame());
            }
        }

        private void SaveHighlight(string save)
        {
            if (save != "Back" &&
                !save.Contains("Delete"))
            {
                Label saveNum = GetLabel("Save" + save + "_Num");
                if (saveNum != null)
                {
                    saveNum.TextColor = Color.White;
                }

                Picture savePortrait = GetPicture("Save" + save + "_Portrait");
                if (savePortrait != null)
                {
                    savePortrait.DrawColor = Color.White;
                }

                Label saveName = GetLabel("Save" + save + "_Name");
                if (saveName != null)
                {
                    saveName.TextColor = Color.White;
                }

                Label saveTime = GetLabel("Save" + save + "_Time");
                if (saveTime != null)
                {
                    saveTime.TextColor = Color.White;
                }

                Label saveDate = GetLabel("Save" + save + "_Date");
                if (saveDate != null)
                {
                    saveDate.TextColor = Color.White;
                }
            }
        }

        private void SaveDim(string save)
        {
            if (save != "Back" &&
                !save.Contains("Delete"))
            {
                Label saveNum = GetLabel("Save" + save + "_Num");
                if (saveNum != null)
                {
                    saveNum.TextColor = Color.White * 0.8f;
                }

                Picture savePortrait = GetPicture("Save" + save + "_Portrait");
                if (savePortrait != null)
                {
                    savePortrait.DrawColor = Color.White * 0.8f;
                }

                Label saveName = GetLabel("Save" + save + "_Name");
                if (saveName != null)
                {
                    saveName.TextColor = Color.White * 0.8f;
                }

                Label saveTime = GetLabel("Save" + save + "_Time");
                if (saveTime != null)
                {
                    saveTime.TextColor = Color.White * 0.8f;
                }

                Label saveDate = GetLabel("Save" + save + "_Date");
                if (saveDate != null)
                {
                    saveDate.TextColor = Color.White * 0.8f;
                }
            }
        }

        private string GetTime(int day, int hour, int minute)
        {
            string time_day = "Day " + day;

            string time = "";
            string minutes = "";
            string hours = "";
            bool pm = false;

            if (hour > 12)
            {
                hour = hour - 12;
                pm = true;
            }
            else if (hour == 0)
            {
                hour = 12;
            }
            else if (hour == 12)
            {
                pm = true;
            }

            if (hour < 10)
            {
                hours = "0" + hour.ToString();
            }
            else
            {
                hours = hour.ToString();
            }

            if (minute < 10)
            {
                minutes = "0" + minute.ToString();
            }
            else
            {
                minutes = minute.ToString();
            }

            if (pm == false)
            {
                time = hours + ":" + minutes + " AM";
            }
            else
            {
                time = hours + ":" + minutes + " PM";
            }

            return time_day + ", " + time;
        }

        public override void Close()
        {
            InputManager.Mouse.Flush();
            InputManager.Keyboard.Flush();

            if (Main.Game.GameStarted)
            {
                InputManager.Mouse.Flush();
                MenuManager.ChangeMenu("Main");
                MenuManager.PreviousMenus.Clear();
            }
            else
            {
                SceneManager.GetScene("Title").Menu.Visible = true;
                MenuManager.ChangeMenu("Main");
            }
        }

        public void ClearSaves()
        {
            for (int i = 1; i <= Handler.Saves.Count; i++)
            {
                Button saveButton = GetButton(i.ToString());
                if (saveButton != null)
                {
                    Buttons.Remove(saveButton);
                }

                Label saveNum = GetLabel("Save" + i.ToString() + "_Num");
                if (saveNum != null)
                {
                    Labels.Remove(saveNum);
                }

                Picture savePortrait = GetPicture("Save" + i.ToString() + "_Portrait");
                if (savePortrait != null)
                {
                    Pictures.Remove(savePortrait);
                }

                Label saveName = GetLabel("Save" + i.ToString() + "_Name");
                if (saveName != null)
                {
                    Labels.Remove(saveName);
                }

                Label saveTime = GetLabel("Save" + i.ToString() + "_Time");
                if (saveTime != null)
                {
                    Labels.Remove(saveTime);
                }

                Label saveDate = GetLabel("Save" + i.ToString() + "_Date");
                if (saveDate != null)
                {
                    Labels.Remove(saveDate);
                }

                Button saveDelete = GetButton("Save" + i.ToString() + "_Delete");
                if (saveDelete != null)
                {
                    Buttons.Remove(saveDelete);
                }
            }
        }

        public override void Load()
        {
            ClearSaves();

            for (int i = Top; i <= Handler.Saves.Count; i++)
            {
                if (i <= Top + 4)
                {
                    string save = Handler.Saves[i - 1];

                    DirectoryInfo saveDir = null;
                    Texture2D portrait = null;

                    //Get save folder path and data
                    DirectoryInfo saveDirs = new DirectoryInfo(AssetManager.Directories["Saves"]);
                    foreach (DirectoryInfo existing in saveDirs.GetDirectories())
                    {
                        if (existing.Name == save)
                        {
                            saveDir = existing;

                            //Get portrait image
                            foreach (var file in saveDir.GetFiles("*.png"))
                            {
                                var name = Path.GetFileNameWithoutExtension(file.Name);
                                if (name == "portrait")
                                {
                                    using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open))
                                    {
                                        portrait = Texture2D.FromStream(Main.Game.GraphicsManager.GraphicsDevice, fileStream);
                                    }
                                    break;
                                }
                            }

                            break;
                        }
                    }

                    if (saveDir != null)
                    {
                        int day = LoadUtil.Get_Day(save);
                        int hour = LoadUtil.Get_Hour(save);
                        int minute = LoadUtil.Get_Minute(save);
                        string time = GetTime(day, hour, minute);

                        AddButton(new ButtonOptions
                        {
                            id = Handler.GetID(),
                            name = i.ToString(),
                            texture = AssetManager.Textures["Frame_Save"],
                            texture_highlight = AssetManager.Textures["Frame_Save_Hover"],
                            region = new Region(0, 0, 0, 0),
                            draw_color = Color.White * 0.8f,
                            enabled = true,
                            visible = true
                        });

                        AddLabel(new LabelOptions
                        {
                            id = Handler.GetID(),
                            font = AssetManager.Fonts["ControlFont"],
                            name = "Save" + i.ToString() + "_Num",
                            text = i.ToString() + ".",
                            text_color = Color.White * 0.8f,
                            region = new Region(0, 0, 0, 0),
                            visible = true
                        });

                        if (portrait != null)
                        {
                            AddPicture(new PictureOptions
                            {
                                id = Handler.GetID(),
                                name = "Save" + i.ToString() + "_Portrait",
                                texture = portrait,
                                region = new Region(0, 0, 0, 0),
                                color = Color.White * 0.8f,
                                visible = true
                            });
                        }

                        AddLabel(new LabelOptions
                        {
                            id = Handler.GetID(),
                            font = AssetManager.Fonts["ControlFont"],
                            name = "Save" + i.ToString() + "_Name",
                            text = "Save Name:   " + save,
                            text_color = Color.White * 0.8f,
                            region = new Region(0, 0, 0, 0),
                            visible = true
                        });

                        AddLabel(new LabelOptions
                        {
                            id = Handler.GetID(),
                            font = AssetManager.Fonts["ControlFont"],
                            name = "Save" + i.ToString() + "_Time",
                            text = "Game Time:   " + time,
                            text_color = Color.White * 0.8f,
                            region = new Region(0, 0, 0, 0),
                            visible = true
                        });

                        AddLabel(new LabelOptions
                        {
                            id = Handler.GetID(),
                            font = AssetManager.Fonts["ControlFont"],
                            name = "Save" + i.ToString() + "_Date",
                            text = "Save Date:   " + saveDir.LastWriteTime.ToString("MM-dd-yyyy hh:mm:ss tt"),
                            text_color = Color.White * 0.8f,
                            region = new Region(0, 0, 0, 0),
                            visible = true
                        });

                        AddButton(new ButtonOptions
                        {
                            id = Handler.GetID(),
                            name = "Save" + i.ToString() + "_Delete",
                            hover_text = "Delete " + save,
                            texture = AssetManager.Textures["Button_Trash"],
                            texture_highlight = AssetManager.Textures["Button_Trash_Hover"],
                            region = new Region(0, 0, 0, 0),
                            draw_color = Color.White * 0.8f,
                            enabled = true,
                            visible = true
                        });
                    }
                }
            }

            Resize(Main.Game.Resolution);
        }

        public override void Load(ContentManager content)
        {
            Clear();

            AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Black"], new Region(0, 0, 0, 0), Color.White * 0.6f, true);
            AddPicture(Handler.GetID(), "Loading", AssetManager.Textures["Loading"], new Region(0, 0, 0, 0), Color.White, false);

            AddPicture(Handler.GetID(), "SaveList", AssetManager.Textures["Frame_Full"], new Region(0, 0, 0, 0), Color.White * 0.8f, true);
            AddPicture(Handler.GetID(), "Arrow_Up", AssetManager.Textures["ArrowIcon_Up"], new Region(0, 0, 0, 0), Color.White * 0f, true);
            AddPicture(Handler.GetID(), "Arrow_Down", AssetManager.Textures["ArrowIcon_Down"], new Region(0, 0, 0, 0), Color.White * 0f, true);

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Back",
                hover_text = "Back",
                texture = AssetManager.Textures["Button_Back"],
                texture_highlight = AssetManager.Textures["Button_Back_Hover"],
                texture_disabled = AssetManager.Textures["Button_Back_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "New",
                hover_text = "New Game",
                texture = AssetManager.Textures["Button_Add"],
                texture_highlight = AssetManager.Textures["Button_Add_Hover"],
                texture_disabled = AssetManager.Textures["Button_Add_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = true,
                visible = true
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["ButtonFrame_Large"],
                new Region(0, 0, 0, 0), false);
        }

        public override void Resize(Point point)
        {
            GetPicture("Background").Region = new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y);
            GetPicture("Loading").Region = new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y);

            int X = (Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize.X * 8);
            int Y = Main.Game.MenuSize.Y * 2;
            int saveWidth = Main.Game.MenuSize.X * 16;
            int saveHeight = Main.Game.MenuSize.Y * 2;
            int listHeight = saveHeight * 5;

            GetPicture("SaveList").Region = new Region(X, Y, saveWidth, listHeight);
            GetPicture("Arrow_Up").Region = new Region(X + saveWidth, Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);
            GetPicture("Arrow_Down").Region = new Region(X + saveWidth, Y + listHeight - Main.Game.MenuSize.Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);

            for (int i = 1; i <= Handler.Saves.Count; i++)
            {
                Button saveButton = GetButton(i.ToString());
                if (saveButton != null)
                {
                    saveButton.Region = new Region(X, Y, saveWidth, saveHeight);

                    Label saveNum = GetLabel("Save" + i.ToString() + "_Num");
                    saveNum.Region = new Region(saveButton.Region.X, saveButton.Region.Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);

                    float margin = saveButton.Region.Height / 10;

                    Picture savePortrait = GetPicture("Save" + i.ToString() + "_Portrait");
                    if (savePortrait != null)
                    {
                        savePortrait.Region = new Region(saveNum.Region.X + saveNum.Region.Width + margin, saveButton.Region.Y + margin, (Main.Game.MenuSize.X * 2) - (margin * 2), saveHeight - (margin * 2));

                        float saveName_X = saveNum.Region.X + saveNum.Region.Width + savePortrait.Region.Width + (margin * 2);
                        float dataWidth = saveWidth - saveNum.Region.Width - savePortrait.Region.Width - Main.Game.MenuSize.X;

                        Label saveName = GetLabel("Save" + i.ToString() + "_Name");
                        saveName.Alignment_Horizontal = Alignment.Left;
                        saveName.Region = new Region(saveName_X, saveButton.Region.Y, dataWidth, saveButton.Region.Height / 3);

                        Label saveTime = GetLabel("Save" + i.ToString() + "_Time");
                        saveTime.Alignment_Horizontal = Alignment.Left;
                        saveTime.Region = new Region(saveName_X, saveButton.Region.Y + (saveButton.Region.Height / 3), dataWidth, saveButton.Region.Height / 3);

                        Label saveDate = GetLabel("Save" + i.ToString() + "_Date");
                        saveDate.Alignment_Horizontal = Alignment.Left;
                        saveDate.Region = new Region(saveName_X, saveButton.Region.Y + ((saveButton.Region.Height / 3) * 2), dataWidth, saveButton.Region.Height / 3);

                        Button saveDelete = GetButton("Save" + i.ToString() + "_Delete");
                        saveDelete.Region = new Region(X + saveWidth - Main.Game.MenuSize.X, Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);
                    }

                    Y += saveHeight;
                }
            }

            Button back = GetButton("Back");
            back.Region = new Region(X, Main.Game.MenuSize.Y * 2 + listHeight, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);

            Button newGame = GetButton("New");
            newGame.Region = new Region(X + saveWidth - Main.Game.MenuSize.X, Main.Game.MenuSize.Y * 2 + listHeight, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);

            Label examine = GetLabel("Examine");
            examine.Region = new Region(0, 0, 0, 0);
        }

        #endregion
    }
}

using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Scenes;
using OP_Engine.Characters;
using OP_Engine.Utility;

using DoS1.Util;

namespace DoS1.Menus
{
    public class Menu_CharGen : Menu
    {
        #region Variables

        private CryptoRandom random = new CryptoRandom();
        private Character leader;
        private string LeaderName;
        private int warning = 0;

        private int Head;
        private int Skin;
        private int HairStyle;
        private string[] HairColors = new string[] { "Brown", "Red", "Blonde", "Black", "Gray", "White", "Purple", "Blue", "Cyan", "Green", "Pink" };
        private int HairColor;
        private string[] EyeColors = new string[] { "Green", "Brown", "Blue", "Cyan", "Purple", "Gold", "Red", "Black", "Gray" };
        private int EyeColor;

        #endregion

        #region Constructors

        public Menu_CharGen(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "CharGen";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible)
            {
                UpdateControls();

                if (warning > 0)
                {
                    GetLabel("Warning").Visible = true;
                    InputBox input = GetInput("Name");

                    if (warning % 2 == 0)
                    {
                        if (input.DrawColor == Color.White)
                        {
                            input.Opacity = 1;
                            input.DrawColor = Color.Red;
                        }
                        else
                        {
                            input.DrawColor = Color.White;
                        }
                    }

                    warning--;

                    if (warning == 0)
                    {
                        GetLabel("Warning").Visible = false;
                        input.Opacity = 0.8f;
                        input.DrawColor = Color.White;
                    }
                }

                base.Update(gameRef, content);
            }
        }

        private void UpdateControls()
        {
            bool found = false;
            bool examine = false;

            foreach (Button button in Buttons)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        found = true;
                        examine = true;

                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(this, button.HoverText);
                        }

                        button.Opacity = 1;
                        button.Selected = true;

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            found = false;
                            examine = false;

                            CheckClick(button);
                            break;
                        }
                    }
                    else if (InputManager.Mouse.Moved)
                    {
                        button.Opacity = 0.8f;
                        button.Selected = false;
                    }
                }
            }

            foreach (InputBox input in Inputs)
            {
                if (input.Active)
                {
                    input.Text = LeaderName;
                }
                else if (InputManager.MouseWithin(input.Region.ToRectangle))
                {
                    found = true;
                    input.Opacity = 1;

                    if (InputManager.Mouse_LB_Pressed)
                    {
                        found = false;
                        input.Active = true;

                        InputManager.Keyboard.OnKeyStateChange += Keyboard_OnKeyStateChange;
                        InputManager.Mouse.Flush();

                        break;
                    }
                }
                else if (InputManager.Mouse.Moved &&
                         !input.Active)
                {
                    input.Opacity = 0.8f;
                }
            }

            if (!examine)
            {
                GetLabel("Examine").Visible = false;
            }

            if (!found)
            {
                if (InputManager.Mouse_LB_Pressed)
                {
                    InputManager.Keyboard.OnKeyStateChange -= Keyboard_OnKeyStateChange;

                    InputBox nameBox = GetInput("Name");
                    nameBox.Active = false;
                    nameBox.Opacity = 0.8f;

                    if (string.IsNullOrEmpty(LeaderName))
                    {
                        nameBox.Text = "Type your name here";
                    }
                }
            }
        }

        private void Keyboard_OnKeyStateChange(object sender, KeyEventArgs e)
        {
            bool shift = false;
            foreach (var key in e.KeysDown)
            {
                if (key == Keys.LeftShift ||
                    key == Keys.RightShift)
                {
                    shift = true;
                }
            }

            foreach (var key in e.KeysPressed)
            {
                if (key == Keys.Space)
                {
                    LeaderName += " ";
                }
                else if (key == Keys.Back)
                {
                    LeaderName = LeaderName.Remove(LeaderName.Length - 1, 1);
                }
                else if (key != Keys.LeftShift &&
                         key != Keys.RightShift &&
                         key != Keys.Escape &&
                         key.ToString().Length == 1)
                {
                    if (shift)
                    {
                        LeaderName += key.ToString().ToUpper();
                    }
                    else
                    {
                        LeaderName += key.ToString().ToLower();
                    }
                }
            }
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            button.Opacity = 0.8f;
            button.Selected = false;

            if (button.Name == "Back")
            {
                Back();
            }
            else if (button.Name == "Next")
            {
                if (!string.IsNullOrEmpty(LeaderName))
                {
                    Finish();
                }
                else
                {
                    warning = 60;
                }
            }
            else if (button.Name == "HairStyle_Minus")
            {
                GetButton("HairStyle_Plus").Enabled = true;

                if (HairStyle > 0)
                {
                    HairStyle--;
                }

                if (HairStyle == 0)
                {
                    button.Enabled = false;
                }

                GetPicture("Hair").Visible = true;
                GetPicture("Hair").Texture = AssetManager.Textures["Left_" + Handler.HairStyles[HairStyle]];
            }
            else if (button.Name == "HairStyle_Plus")
            {
                GetButton("HairStyle_Minus").Enabled = true;

                if (HairStyle < Handler.HairStyles.Length - 1)
                {
                    HairStyle++;
                }

                if (HairStyle == Handler.HairStyles.Length - 1)
                {
                    button.Enabled = false;
                    GetPicture("Hair").Visible = false;
                }
                else
                {
                    GetPicture("Hair").Visible = true;
                    GetPicture("Hair").Texture = AssetManager.Textures["Left_" + Handler.HairStyles[HairStyle]];
                }
            }
            else if (button.Name == "HairColor_Minus")
            {
                GetButton("HairColor_Plus").Enabled = true;

                if (HairColor > 0)
                {
                    HairColor--;
                }

                if (HairColor == 0)
                {
                    button.Enabled = false;
                }

                GetPicture("Hair").DrawColor = Handler.HairColors[HairColors[HairColor]];
            }
            else if (button.Name == "HairColor_Plus")
            {
                GetButton("HairColor_Minus").Enabled = true;

                if (HairColor < HairColors.Length - 1)
                {
                    HairColor++;
                }

                if (HairColor == HairColors.Length - 1)
                {
                    button.Enabled = false;
                }

                GetPicture("Hair").DrawColor = Handler.HairColors[HairColors[HairColor]];
            }
            else if (button.Name == "Head_Minus")
            {
                GetButton("Head_Plus").Enabled = true;

                if (Head > 0)
                {
                    Head--;
                }

                if (Head == 0)
                {
                    button.Enabled = false;
                }

                GetPicture("Head").Texture = AssetManager.Textures["Left_" + Handler.SkinTones[Skin] + "_" + Handler.HeadStyles[Head]];
            }
            else if (button.Name == "Head_Plus")
            {
                GetButton("Head_Minus").Enabled = true;

                if (Head < Handler.HeadStyles.Length - 1)
                {
                    Head++;
                }

                if (Head == Handler.HeadStyles.Length - 1)
                {
                    button.Enabled = false;
                }

                GetPicture("Head").Texture = AssetManager.Textures["Left_" + Handler.SkinTones[Skin] + "_" + Handler.HeadStyles[Head]];
            }
            else if (button.Name == "EyeColor_Minus")
            {
                GetButton("EyeColor_Plus").Enabled = true;

                if (EyeColor > 0)
                {
                    EyeColor--;
                }

                if (EyeColor == 0)
                {
                    button.Enabled = false;
                }

                GetPicture("Eyes").DrawColor = Handler.EyeColors[EyeColors[EyeColor]];
            }
            else if (button.Name == "EyeColor_Plus")
            {
                GetButton("EyeColor_Minus").Enabled = true;

                if (EyeColor < EyeColors.Length - 1)
                {
                    EyeColor++;
                }

                if (EyeColor == EyeColors.Length - 1)
                {
                    button.Enabled = false;
                }

                GetPicture("Eyes").DrawColor = Handler.EyeColors[EyeColors[EyeColor]];
            }
            else if (button.Name == "SkinColor_Minus")
            {
                GetButton("SkinColor_Plus").Enabled = true;

                if (Skin > 0)
                {
                    Skin--;
                }

                if (Skin == 0)
                {
                    button.Enabled = false;
                }

                GetPicture("Head").Texture = AssetManager.Textures["Left_" + Handler.SkinTones[Skin] + "_" + Handler.HeadStyles[Head]];
            }
            else if (button.Name == "SkinColor_Plus")
            {
                GetButton("SkinColor_Minus").Enabled = true;

                if (Skin < Handler.SkinTones.Length - 1)
                {
                    Skin++;
                }

                if (Skin == Handler.SkinTones.Length - 1)
                {
                    button.Enabled = false;
                }

                GetPicture("Head").Texture = AssetManager.Textures["Left_" + Handler.SkinTones[Skin] + "_" + Handler.HeadStyles[Head]];
            }
        }

        private void Back()
        {
            Active = false;
            Visible = false;

            SceneManager.GetScene("Title").Menu.Visible = true;
            MenuManager.ChangeMenu("Main");
        }

        private void Finish()
        {
            Active = false;
            Visible = false;

            InventoryUtil.GenAssets();
            ArmyUtil.Gen_StartingArmy();

            Army army = CharacterManager.GetArmy("Ally");
            Squad squad = army.Squads[0];
            squad.Name = LeaderName;

            leader = ArmyUtil.NewCharacter(LeaderName, new Vector2(1, 1), Handler.HairStyles[HairStyle], HairColors[HairColor], Handler.HeadStyles[Head], EyeColors[EyeColor], Handler.SkinTones[Skin]);
            squad.Characters.Add(leader);
            squad.Leader_ID = leader.ID;

            InventoryUtil.AddItem(leader.Inventory, "Cloth", "Cloth", "Armor");
            InventoryUtil.EquipItem(leader, leader.Inventory.Items[leader.Inventory.Items.Count - 1]);

            InventoryUtil.BeginningInventory();

            Task.Factory.StartNew(() => WorldGen.GenWorldmap());

            GameUtil.Start();
        }

        public override void Load(ContentManager content)
        {
            Clear();

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Name", "Name:", Color.White, new Region(0, 0, 0, 0), true);
            AddInput(AssetManager.Fonts["ControlFont"], Handler.GetID(), 100, "Name", "Type your name here", Color.DarkGray, AssetManager.Textures["TextFrame"],
                new Region(0, 0, 0, 0), true, true);
            GetInput("Name").CarrotDelay = 10;
            GetInput("Name").Alignment_Horizontal = Alignment.Left;
            GetInput("Name").Alignment_Verticle = Alignment.Center;
            GetInput("Name").TextColor_Selected = Color.White;

            AddPicture(Handler.GetID(), "Body", AssetManager.Textures["Left_Armor_Cloth_Cloth_Idle"], new Region(0, 0, 0, 0),
                new Color(255, 255, 255, 255), true);
            AddPicture(Handler.GetID(), "Head", AssetManager.Textures["Left_Light_Head1"], new Region(0, 0, 0, 0),
                new Color(255, 255, 255, 255), true);
            AddPicture(Handler.GetID(), "Eyes", AssetManager.Textures["Left_Eye"], new Region(0, 0, 0, 0),
                Handler.EyeColors["Green"], true);
            AddPicture(Handler.GetID(), "Hair", AssetManager.Textures["Left_Style1"], new Region(0, 0, 0, 0),
                Handler.HairColors["Brown"], true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "HairStyle", "Hair Style:", Color.White, new Region(0, 0, 0, 0), true);
            GetLabel("HairStyle").Alignment_Horizontal = Alignment.Right;
            GetLabel("HairStyle").Alignment_Verticle = Alignment.Center;

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "HairStyle_Minus",
                texture = AssetManager.Textures["Button_Remove"],
                texture_highlight = AssetManager.Textures["Button_Remove_Hover"],
                texture_disabled = AssetManager.Textures["Button_Remove_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = false,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "HairStyle_Plus",
                texture = AssetManager.Textures["Button_Add"],
                texture_highlight = AssetManager.Textures["Button_Add_Hover"],
                texture_disabled = AssetManager.Textures["Button_Add_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "HairColor", "Hair Color:", Color.White, new Region(0, 0, 0, 0), true);
            GetLabel("HairColor").Alignment_Horizontal = Alignment.Right;
            GetLabel("HairColor").Alignment_Verticle = Alignment.Center;

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "HairColor_Minus",
                texture = AssetManager.Textures["Button_Remove"],
                texture_highlight = AssetManager.Textures["Button_Remove_Hover"],
                texture_disabled = AssetManager.Textures["Button_Remove_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = false,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "HairColor_Plus",
                texture = AssetManager.Textures["Button_Add"],
                texture_highlight = AssetManager.Textures["Button_Add_Hover"],
                texture_disabled = AssetManager.Textures["Button_Add_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Head", "Head:", Color.White, new Region(0, 0, 0, 0), true);
            GetLabel("Head").Alignment_Horizontal = Alignment.Right;
            GetLabel("Head").Alignment_Verticle = Alignment.Center;

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Head_Minus",
                texture = AssetManager.Textures["Button_Remove"],
                texture_highlight = AssetManager.Textures["Button_Remove_Hover"],
                texture_disabled = AssetManager.Textures["Button_Remove_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = false,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Head_Plus",
                texture = AssetManager.Textures["Button_Add"],
                texture_highlight = AssetManager.Textures["Button_Add_Hover"],
                texture_disabled = AssetManager.Textures["Button_Add_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "EyeColor", "Eye Color:", Color.White, new Region(0, 0, 0, 0), true);
            GetLabel("EyeColor").Alignment_Horizontal = Alignment.Right;
            GetLabel("EyeColor").Alignment_Verticle = Alignment.Center;

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "EyeColor_Minus",
                texture = AssetManager.Textures["Button_Remove"],
                texture_highlight = AssetManager.Textures["Button_Remove_Hover"],
                texture_disabled = AssetManager.Textures["Button_Remove_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = false,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "EyeColor_Plus",
                texture = AssetManager.Textures["Button_Add"],
                texture_highlight = AssetManager.Textures["Button_Add_Hover"],
                texture_disabled = AssetManager.Textures["Button_Add_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "SkinColor", "Skin Color:", Color.White, new Region(0, 0, 0, 0), true);
            GetLabel("SkinColor").Alignment_Horizontal = Alignment.Right;
            GetLabel("SkinColor").Alignment_Verticle = Alignment.Center;

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "SkinColor_Minus",
                texture = AssetManager.Textures["Button_Remove"],
                texture_highlight = AssetManager.Textures["Button_Remove_Hover"],
                texture_disabled = AssetManager.Textures["Button_Remove_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = false,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "SkinColor_Plus",
                texture = AssetManager.Textures["Button_Add"],
                texture_highlight = AssetManager.Textures["Button_Add_Hover"],
                texture_disabled = AssetManager.Textures["Button_Add_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Back",
                hover_text = "Cancel",
                texture = AssetManager.Textures["Button_Back"],
                texture_highlight = AssetManager.Textures["Button_Back_Hover"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Next",
                hover_text = "Finish",
                texture = AssetManager.Textures["Button_Next"],
                texture_highlight = AssetManager.Textures["Button_Next_Hover"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Warning", "Name required!", Color.Red, new Region(0, 0, 0, 0), false);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"],
                new Region(0, 0, 0, 0), false);

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            int width = Main.Game.MenuSize.X;
            int height = Main.Game.MenuSize.Y;
            float center_x = Main.Game.ScreenWidth / 2;

            int Y = (Main.Game.ScreenHeight / 2) - (height * 6);
            GetLabel("Name").Region = new Region(center_x - (width * 6), Y, width * 2, height);
            GetInput("Name").Region = new Region(center_x - (width * 4), Y, width * 8, height);
            GetLabel("Warning").Region = new Region(center_x + (width * 4), Y, width * 4, height);

            Y += (height * 2);
            Picture body = GetPicture("Body");
            body.Region = new Region(center_x - (width * 3), Y, width * 6, height * 8);
            body.Image = new Rectangle(0, 0, body.Texture.Width / 4, body.Texture.Height);

            GetPicture("Head").Region = body.Region;
            GetPicture("Head").Image = body.Image;

            GetPicture("Eyes").Region = body.Region;
            GetPicture("Eyes").Image = body.Image;

            GetPicture("Hair").Region = body.Region;
            GetPicture("Hair").Image = body.Image;

            GetLabel("HairStyle").Region = new Region(body.Region.X - (width * 5), Y, width * 4, height);
            GetButton("HairStyle_Minus").Region = new Region(body.Region.X - width, Y, width, height);
            GetButton("HairStyle_Plus").Region = new Region(body.Region.X + body.Region.Width, Y, width, height);

            Y += height;
            GetLabel("HairColor").Region = new Region(body.Region.X - (width * 5), Y, width * 4, height);
            GetButton("HairColor_Minus").Region = new Region(body.Region.X - width, Y, width, height);
            GetButton("HairColor_Plus").Region = new Region(body.Region.X + body.Region.Width, Y, width, height);

            Y += height;
            GetLabel("Head").Region = new Region(body.Region.X - (width * 4), Y, width * 3, height);
            GetButton("Head_Minus").Region = new Region(body.Region.X - width, Y, width, height);
            GetButton("Head_Plus").Region = new Region(body.Region.X + body.Region.Width, Y, width, height);

            Y += height;
            GetLabel("EyeColor").Region = new Region(body.Region.X - (width * 4), Y, width * 3, height);
            GetButton("EyeColor_Minus").Region = new Region(body.Region.X - width, Y, width, height);
            GetButton("EyeColor_Plus").Region = new Region(body.Region.X + body.Region.Width, Y, width, height);

            Y += height;
            GetLabel("SkinColor").Region = new Region(body.Region.X - (width * 4), Y, width * 3, height);
            GetButton("SkinColor_Minus").Region = new Region(body.Region.X - width, Y, width, height);
            GetButton("SkinColor_Plus").Region = new Region(body.Region.X + body.Region.Width, Y, width, height);

            Y = (int)body.Region.Y + (int)body.Region.Height;
            GetButton("Back").Region = new Region(body.Region.X - width, Y, width, height);
            GetButton("Next").Region = new Region(body.Region.X + body.Region.Width, Y, width, height);

            GetLabel("Examine").Region = new Region(0, 0, 0, 0);
        }

        #endregion
    }
}

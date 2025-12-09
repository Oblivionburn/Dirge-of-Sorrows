using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Scenes;
using OP_Engine.Characters;
using OP_Engine.Utility;
using OP_Engine.Enums;
using DoS1.Util;

namespace DoS1.Menus
{
    public class Menu_CharGen : Menu
    {
        #region Variables

        bool ControlsLoaded;

        Character leader;
        string LeaderName;
        int warning = 0;
        bool Shift = false;

        int Head;
        int Skin;
        int HairStyle;
        string[] HairColors = new string[] { "Brown", "Red", "Blonde", "Black", "Gray", "White" };
        int HairColor;
        string[] EyeColors = new string[] { "Green", "Brown", "Blue", "Cyan", "Purple", "Gold", "Gray" };
        int EyeColor;
        int Gender;

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

                    if (warning % 4 == 0)
                    {
                        if (input.DrawColor == Color.White)
                        {
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

                foreach (InputBox input in Inputs)
                {
                    if (input.Active)
                    {
                        if (!string.IsNullOrEmpty(input.Text) &&
                            !string.IsNullOrEmpty(input.Caret))
                        {
                            input.Caret = "";
                        }
                    }
                }
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

                        InputManager.Mouse.Flush();

                        InputManager.Keyboard.OnKeyStateChange += Keyboard_OnKeyStateChange;
                        Main.Game.Window.TextInput += Window_TextInput;
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

            if ((!found && InputManager.Mouse_LB_Pressed) ||
                (GetInput("Name").Active && InputManager.KeyPressed("Enter")))
            {
                Main.Game.Window.TextInput -= Window_TextInput;
                InputManager.Keyboard.OnKeyStateChange -= Keyboard_OnKeyStateChange;

                InputBox nameBox = GetInput("Name");
                nameBox.Active = false;
                nameBox.Opacity = 0.8f;

                if (string.IsNullOrEmpty(LeaderName))
                {
                    nameBox.Text = "Type name here";
                }
            }
        }

        private void Window_TextInput(object sender, TextInputEventArgs e)
        {
            if (e.Key == Keys.Back)
            {
                if (!string.IsNullOrEmpty(LeaderName))
                {
                    LeaderName = LeaderName.Remove(LeaderName.Length - 1, 1);
                }
            }
            else if (InputManager.Keyboard.KeysMapped.ContainsValue(e.Key) &&
                     e.Key != Keys.LeftShift &&
                     e.Key != Keys.RightShift &&
                     e.Key != Keys.Escape &&
                     e.Key.ToString().Length == 1)
            {
                if (Shift)
                {
                    LeaderName += e.Key.ToString().ToUpper();
                }
                else
                {
                    LeaderName += e.Key.ToString().ToLower();
                }
            }
        }

        private void Keyboard_OnKeyStateChange(object sender, KeyEventArgs e)
        {
            foreach (var key in e.KeysPressed)
            {
                if (key == Keys.LeftShift ||
                    key == Keys.RightShift)
                {
                    Shift = false;
                }
            }

            foreach (var key in e.KeysDown)
            {
                if (key == Keys.LeftShift ||
                    key == Keys.RightShift)
                {
                    Shift = true;
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
            else if (button.Name == "Gender_Minus")
            {
                GetButton("Gender_Plus").Enabled = true;
                button.Enabled = false;

                Gender--;

                GetLabel("Gender").Text = "Male";

                HairStyle = 0;
                GetButton("HairStyle_Plus").Enabled = true;
                GetButton("HairStyle_Minus").Enabled = false;

                if (Gender == 0)
                {
                    GetPicture("Head").Texture = Handler.GetTexture("Left_" + Handler.SkinTones[Skin] + "_" + GetLabel("Gender").Text + "_" + Handler.HeadStyles_Male[Head]);
                    GetPicture("Hair").Texture = Handler.GetTexture("Left_" + GetLabel("Gender").Text + "_" + Handler.HairStyles_Male[HairStyle]);
                }
                else
                {
                    GetPicture("Head").Texture = Handler.GetTexture("Left_" + Handler.SkinTones[Skin] + "_" + GetLabel("Gender").Text + "_" + Handler.HeadStyles_Female[Head]);
                    GetPicture("Hair").Texture = Handler.GetTexture("Left_" + GetLabel("Gender").Text + "_" + Handler.HairStyles_Female[HairStyle]);
                }
            }
            else if (button.Name == "Gender_Plus")
            {
                GetButton("Gender_Minus").Enabled = true;
                button.Enabled = false;

                Gender++;

                GetLabel("Gender").Text = "Female";

                HairStyle = 0;
                GetButton("HairStyle_Plus").Enabled = true;
                GetButton("HairStyle_Minus").Enabled = false;

                if (Gender == 0)
                {
                    GetPicture("Head").Texture = Handler.GetTexture("Left_" + Handler.SkinTones[Skin] + "_" + GetLabel("Gender").Text + "_" + Handler.HeadStyles_Male[Head]);
                    GetPicture("Hair").Texture = Handler.GetTexture("Left_" + GetLabel("Gender").Text + "_" + Handler.HairStyles_Male[HairStyle]);
                }
                else
                {
                    GetPicture("Head").Texture = Handler.GetTexture("Left_" + Handler.SkinTones[Skin] + "_" + GetLabel("Gender").Text + "_" + Handler.HeadStyles_Female[Head]);
                    GetPicture("Hair").Texture = Handler.GetTexture("Left_" + GetLabel("Gender").Text + "_" + Handler.HairStyles_Female[HairStyle]);
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

                if (Gender == 0)
                {
                    GetPicture("Hair").Texture = Handler.GetTexture("Left_" + GetLabel("Gender").Text + "_" + Handler.HairStyles_Male[HairStyle]);
                }
                else
                {
                    GetPicture("Hair").Texture = Handler.GetTexture("Left_" + GetLabel("Gender").Text + "_" + Handler.HairStyles_Female[HairStyle]);
                }
            }
            else if (button.Name == "HairStyle_Plus")
            {
                GetButton("HairStyle_Minus").Enabled = true;

                if (Gender == 0)
                {
                    if (HairStyle < Handler.HairStyles_Male.Length - 1)
                    {
                        HairStyle++;
                    }

                    if (HairStyle == Handler.HairStyles_Male.Length - 1)
                    {
                        button.Enabled = false;
                        GetPicture("Hair").Visible = false;
                    }
                    else
                    {
                        GetPicture("Hair").Visible = true;
                        GetPicture("Hair").Texture = Handler.GetTexture("Left_" + GetLabel("Gender").Text + "_" + Handler.HairStyles_Male[HairStyle]);
                    }
                }
                else
                {
                    if (HairStyle < Handler.HairStyles_Female.Length - 1)
                    {
                        HairStyle++;
                    }

                    if (HairStyle == Handler.HairStyles_Female.Length - 1)
                    {
                        button.Enabled = false;
                    }

                    GetPicture("Hair").Visible = true;
                    GetPicture("Hair").Texture = Handler.GetTexture("Left_" + GetLabel("Gender").Text + "_" + Handler.HairStyles_Female[HairStyle]);
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

                if (Gender == 0)
                {
                    GetPicture("Head").Texture = Handler.GetTexture("Left_" + Handler.SkinTones[Skin] + "_" + GetLabel("Gender").Text + "_" + Handler.HeadStyles_Male[Head]);
                }
                else
                {
                    GetPicture("Head").Texture = Handler.GetTexture("Left_" + Handler.SkinTones[Skin] + "_" + GetLabel("Gender").Text + "_" + Handler.HeadStyles_Female[Head]);
                }
            }
            else if (button.Name == "Head_Plus")
            {
                GetButton("Head_Minus").Enabled = true;

                if (Gender == 0)
                {
                    if (Head < Handler.HeadStyles_Male.Length - 1)
                    {
                        Head++;
                    }

                    if (Head == Handler.HeadStyles_Male.Length - 1)
                    {
                        button.Enabled = false;
                    }

                    GetPicture("Head").Texture = Handler.GetTexture("Left_" + Handler.SkinTones[Skin] + "_" + GetLabel("Gender").Text + "_" + Handler.HeadStyles_Male[Head]);
                }
                else
                {
                    if (Head < Handler.HeadStyles_Female.Length - 1)
                    {
                        Head++;
                    }

                    if (Head == Handler.HeadStyles_Female.Length - 1)
                    {
                        button.Enabled = false;
                    }

                    GetPicture("Head").Texture = Handler.GetTexture("Left_" + Handler.SkinTones[Skin] + "_" + GetLabel("Gender").Text + "_" + Handler.HeadStyles_Female[Head]);
                }
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

                if (Gender == 0)
                {
                    GetPicture("Head").Texture = Handler.GetTexture("Left_" + Handler.SkinTones[Skin] + "_" + GetLabel("Gender").Text + "_" + Handler.HeadStyles_Male[Head]);
                }
                else
                {
                    GetPicture("Head").Texture = Handler.GetTexture("Left_" + Handler.SkinTones[Skin] + "_" + GetLabel("Gender").Text + "_" + Handler.HeadStyles_Female[Head]);
                }
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

                if (Gender == 0)
                {
                    GetPicture("Head").Texture = Handler.GetTexture("Left_" + Handler.SkinTones[Skin] + "_" + GetLabel("Gender").Text + "_" + Handler.HeadStyles_Male[Head]);
                }
                else
                {
                    GetPicture("Head").Texture = Handler.GetTexture("Left_" + Handler.SkinTones[Skin] + "_" + GetLabel("Gender").Text + "_" + Handler.HeadStyles_Female[Head]);
                }
            }
        }

        private void Back()
        {
            Active = false;
            Visible = false;

            if (Handler.Saves.Count > 0)
            {
                MenuManager.ChangeMenu("Save_Load");
            }
            else
            {
                SceneManager.GetScene("Title").Menu.Visible = true;
                MenuManager.ChangeMenu("Main");
            }
        }

        private void Finish()
        {
            Active = false;
            Visible = false;

            ArmyUtil.InitArmies();

            //Add friend
            Character friend = CharacterUtil.NewCharacter_Random(new Vector2(0, 0), false, Gender);

            InventoryUtil.AddItem(friend.Inventory, "Cloth", "Cloth", "Armor");
            InventoryUtil.EquipItem(friend, friend.Inventory.Items[friend.Inventory.Items.Count - 1]);

            Squad reserves = CharacterManager.GetArmy("Reserves").Squads[0];
            reserves.AddCharacter(friend);

            //Add spouse
            Character spouse;
            if (Gender == 0)
            {
                spouse = CharacterUtil.NewCharacter_Random(new Vector2(0, 0), false, 1);
            }
            else
            {
                spouse = CharacterUtil.NewCharacter_Random(new Vector2(0, 0), false, 0);
            }

            InventoryUtil.AddItem(spouse.Inventory, "Cloth", "Cloth", "Armor");
            InventoryUtil.EquipItem(spouse, spouse.Inventory.Items[spouse.Inventory.Items.Count - 1]);

            Squad special = CharacterManager.GetArmy("Special").Squads[0];
            special.AddCharacter(spouse);

            //Add hero
            Army army = CharacterManager.GetArmy("Ally");
            Squad squad = army.Squads[0];
            squad.Name = LeaderName;

            leader = CharacterUtil.NewCharacter(LeaderName, new Vector2(1, 1), "Ally", Direction.Left, Handler.HairStyles_Male[HairStyle], HairColors[HairColor],
                Handler.HeadStyles_Male[Head], EyeColors[EyeColor], Handler.SkinTones[Skin], Gender == 0 ? "Male" : "Female");

            squad.AddCharacter(leader);
            squad.Leader_ID = leader.ID;

            Handler.MainCharacter_ID = leader.ID;
            Handler.Selected_Save = LeaderName;

            leader.Inventory.Name = LeaderName;
            InventoryUtil.AddItem(leader.Inventory, "Cloth", "Cloth", "Armor");
            InventoryUtil.EquipItem(leader, leader.Inventory.Items[leader.Inventory.Items.Count - 1]);

            GameUtil.NewGame();
        }

        public override void Load()
        {
            Clear();

            AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Black"], new Region(0, 0, 0, 0), Color.White * 0.6f, true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Name", "Name:", Color.White, new Region(0, 0, 0, 0), true);
            AddInput(AssetManager.Fonts["ControlFont"], Handler.GetID(), 100, "Name", string.IsNullOrEmpty(LeaderName) ? "Type name here" : LeaderName, Color.DarkGray, AssetManager.Textures["ButtonFrame_Large"],
                new Region(0, 0, 0, 0), false, true);
            GetInput("Name").Alignment_Horizontal = Alignment.Left;
            GetInput("Name").Alignment_Verticle = Alignment.Center;
            GetInput("Name").TextColor_Selected = Color.White;

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Gender", "Male", Color.White, new Region(0, 0, 0, 0), true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "GenderLabel", "Gender:", Color.White, new Region(0, 0, 0, 0), true);
            GetLabel("GenderLabel").Alignment_Horizontal = Alignment.Right;
            GetLabel("GenderLabel").Alignment_Verticle = Alignment.Center;

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Gender_Minus",
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
                name = "Gender_Plus",
                texture = AssetManager.Textures["Button_Add"],
                texture_highlight = AssetManager.Textures["Button_Add_Hover"],
                texture_disabled = AssetManager.Textures["Button_Add_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

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

            AddPicture(Handler.GetID(), "Body", Handler.GetTexture("Left_Armor_Cloth_Cloth_Idle"), new Region(0, 0, 0, 0),
                new Color(255, 255, 255, 255), true);
            AddPicture(Handler.GetID(), "Head", Handler.GetTexture("Left_Light_Male_Head1"), new Region(0, 0, 0, 0),
                new Color(255, 255, 255, 255), true);
            AddPicture(Handler.GetID(), "Eyes", Handler.GetTexture("Left_Eye"), new Region(0, 0, 0, 0),
                Handler.EyeColors["Green"], true);
            AddPicture(Handler.GetID(), "Hair", Handler.GetTexture("Left_Male_Style1"), new Region(0, 0, 0, 0),
                Handler.HairColors["Brown"], true);

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Back",
                hover_text = "Back",
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
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["ButtonFrame_Large"],
                new Region(0, 0, 0, 0), false);

            ControlsLoaded = true;
            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            int width = Main.Game.MenuSize.X;
            int height = Main.Game.MenuSize.Y;
            float center_x = Main.Game.ScreenWidth / 2;

            int Y = (Main.Game.ScreenHeight / 2) - (height * 6);

            if (ControlsLoaded)
            {
                GetPicture("Background").Region = new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y);

                GetLabel("Name").Region = new Region(center_x - (width * 6), Y, width * 2, height);
                GetInput("Name").Region = new Region(center_x - (width * 4), Y, width * 8, height);
                GetLabel("Warning").Region = new Region(center_x + (width * 4), Y, width * 4, height);

                Y += (height * 2);
                Label gender_label = GetLabel("GenderLabel");
                gender_label.Region = new Region(center_x - (width * 8), Y, width * 4, height);

                Button gender_minus = GetButton("Gender_Minus");
                gender_minus.Region = new Region(gender_label.Region.X + gender_label.Region.Width, Y, width, height);

                Label gender = GetLabel("Gender");
                gender.Region = new Region(gender_minus.Region.X + gender_minus.Region.Width, Y, width * 6, height);

                GetButton("Gender_Plus").Region = new Region(gender.Region.X + gender.Region.Width, Y, width, height);

                Y += height;
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
        }

        #endregion
    }
}

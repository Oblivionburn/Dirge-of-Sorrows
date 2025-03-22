﻿using System.Linq;
using System.Timers;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Characters;
using OP_Engine.Scenes;
using OP_Engine.Sounds;
using OP_Engine.Tiles;
using OP_Engine.Utility;
using OP_Engine.Rendering;
using OP_Engine.Time;
using OP_Engine.Controls;
using OP_Engine.Inventories;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Weathers;

using DoS1.Util;

namespace DoS1.Scenes
{
    public class Combat : Scene
    {
        #region Variables

        private int mouseClickDelay = 0;

        private Squad ally_squad;
        private Squad enemy_squad;

        private List<Character> targets = new List<Character>();
        private Character current_character = null;
        private string attack_type = "";
        private bool hero_killed = false;
        private int ep_cost = 0;

        private int character_frame = 0;
        private float base_move_speed = 0;
        private float move_speed = 0;
        private int effect_frame = 0;
        private int animation_speed = 16;

        private bool paused = false;
        private string combat_state = "GetTargets";

        private int ally_total_damage;
        private int enemy_total_damage;

        private bool won_battle;
        private int gold;
        private int xp;

        #endregion

        #region Constructors

        public Combat(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Combat";
            Load(content);

            Handler.CombatTimer.Elapsed += Timer_Elapsed;
            Handler.CombatTimer_Tiles.Elapsed += TileTimer_Elapsed;
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible ||
                Active)
            {
                if (SoundManager.NeedMusic)
                {
                    SoundManager.MusicLooping = true;
                    AssetManager.PlayMusic_Random("Combat", true);
                }

                foreach (Button button in Menu.Buttons)
                {
                    button.Update();
                }

                for (int i = 0; i < Menu.Labels.Count; i++)
                {
                    Label label = Menu.Labels[i];
                    label.Update();
                }

                if (!TimeManager.Paused)
                {
                    UpdateControls();
                    AnimateMouseClick();
                    UpdateGrids();
                }
            }
        }

        public override void DrawWorld(SpriteBatch spriteBatch, Point resolution, Color color)
        {
            if (Visible)
            {
                Menu.GetPicture("Background").Draw(spriteBatch);

                World.Draw(spriteBatch, Main.Game.Resolution, color);

                if (ally_squad != null)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            Character character = ally_squad.GetCharacter(new Vector2(x, y));
                            if (character != null)
                            {
                                bool received_damage = false;

                                Something damage = character.GetStatusEffect("Damage");
                                if (damage != null)
                                {
                                    Label damage_label = Menu.GetLabel(damage.ID);
                                    if (damage_label != null)
                                    {
                                        received_damage = true;
                                        CharacterUtil.DrawCharacter(spriteBatch, character, damage.DrawColor);
                                        damage.DrawColor = Color.Lerp(damage.DrawColor, color, 0.025f);
                                    }
                                }

                                if (!received_damage)
                                {
                                    CharacterUtil.DrawCharacter(spriteBatch, character, color);
                                }
                            }
                        }
                    }
                }

                if (enemy_squad != null)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            Character character = enemy_squad.GetCharacter(new Vector2(x, y));
                            if (character != null)
                            {
                                bool received_damage = false;

                                Something damage = character.GetStatusEffect("Damage");
                                if (damage != null)
                                {
                                    Label damage_label = Menu.GetLabel(damage.ID);
                                    if (damage_label != null)
                                    {
                                        received_damage = true;
                                        CharacterUtil.DrawCharacter(spriteBatch, character, damage.DrawColor);
                                        damage.DrawColor = Color.Lerp(damage.DrawColor, color, 0.05f);
                                    }
                                }

                                if (!received_damage)
                                {
                                    CharacterUtil.DrawCharacter(spriteBatch, character, color);
                                }
                            }
                        }
                    }
                }

                WeatherManager.Draw(spriteBatch);
            }
        }

        public override void DrawMenu(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                for (int i = 0; i < Menu.Pictures.Count; i++)
                {
                    Picture picture = Menu.Pictures[i];
                    if (picture != null &&
                        picture.Name != "Damage" &&
                        picture.Name != "Cast" &&
                        picture.Name != "Result" &&
                        picture.Name != "MouseClick" &&
                        picture.Name != "Background")
                    {
                        picture.Draw(spriteBatch);
                    }
                }

                for (int i = 0; i < Menu.Pictures.Count; i++)
                {
                    Picture picture = Menu.Pictures[i];
                    if (picture.Name == "Damage" ||
                        picture.Name == "Cast")
                    {
                        picture.Draw(spriteBatch);
                    }
                }

                for (int i = 0; i < Menu.Labels.Count; i++)
                {
                    Label label = Menu.Labels[i];
                    if (label.Name != "Examine")
                    {
                        label.Draw(spriteBatch);
                    }
                }

                Menu.GetPicture("Result").Draw(spriteBatch);
                Menu.GetPicture("MouseClick").Draw(spriteBatch);

                foreach (Button button in Menu.Buttons)
                {
                    if (button.Visible)
                    {
                        button.Draw(spriteBatch);
                    }
                }

                for (int i = 0; i < Menu.Labels.Count; i++)
                {
                    Label label = Menu.Labels[i];
                    if (label.Name == "Examine")
                    {
                        label.Draw(spriteBatch);
                        break;
                    }
                }
            }
        }

        private void UpdateControls()
        {
            bool found = false;

            foreach (Button button in Menu.Buttons)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        found = true;

                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(Menu, button.HoverText);
                        }

                        button.Opacity = 1;
                        button.Selected = true;

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            CheckClick(button);

                            if (button.Name != "Result")
                            {
                                button.Opacity = 0.9f;
                            }
                            
                            button.Selected = false;

                            break;
                        }
                    }
                    else if (InputManager.Mouse.Moved)
                    {
                        if (button.Name != "Result")
                        {
                            button.Opacity = 0.9f;
                        }

                        button.Selected = false;
                    }
                }
            }

            if (!found)
            {
                Menu.GetLabel("Examine").Visible = false;
            }

            if (InputManager.KeyPressed("Space"))
            {
                Toggle_Pause();
            }
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse.Flush();

            if (button.Name == "PlayPause")
            {
                Toggle_Pause();
            }
            else if (button.Name == "Speed")
            {
                SpeedToggle();
            }
            else if (button.Name == "Retreat")
            {
                Retreat();
            }
            else if (button.Name == "Result")
            {
                Army ally_army = CharacterManager.GetArmy("Ally");

                if (hero_killed ||
                    !ally_army.Squads.Any())
                {
                    Handler.Combat = false;

                    SoundManager.StopMusic();
                    SoundManager.NeedMusic = true;

                    SceneManager.ChangeScene("GameOver");
                }
                else
                {
                    Leave();
                }
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!TimeManager.Paused &&
                !paused)
            {
                if (current_character != null)
                {
                    Tile origin_tile = CombatUtil.OriginTile(World, current_character);
                    Tile target_tile = CombatUtil.TargetTile(World, current_character);

                    if (origin_tile != null &&
                        target_tile != null)
                    {
                        switch (combat_state)
                        {
                            case "GetTargets":
                                bool can_attack = true;

                                ep_cost = InventoryUtil.Get_EP_Cost(current_character);
                                if (ep_cost > 0)
                                {
                                    if (current_character.ManaBar.Value < ep_cost)
                                    {
                                        can_attack = false;
                                    }
                                }

                                targets = CombatUtil.GetTargets(current_character, ally_squad, enemy_squad);
                                if (!targets.Any())
                                {
                                    can_attack = false;
                                    FinishCombat();
                                }

                                if (can_attack)
                                {
                                    attack_type = CharacterUtil.AttackType(current_character);
                                    CombatUtil.SwitchAnimation(current_character, attack_type);

                                    switch (attack_type)
                                    {
                                        case "Attack":
                                            combat_state = "MoveForward";
                                            break;

                                        case "Ranaged":
                                            combat_state = "Attack";
                                            break;

                                        case "Cast":
                                            StartCast();
                                            combat_state = "Attack";
                                            break;

                                        default:
                                            combat_state = "Attack";
                                            break;
                                    }
                                }
                                break;

                            case "MoveForward":
                                if (CombatUtil.AtTile(current_character, target_tile, move_speed))
                                {
                                    combat_state = "Attack";
                                }
                                else
                                {
                                    CombatUtil.MoveForward(current_character, move_speed);
                                }
                                break;

                            case "Attack":
                                if (character_frame % animation_speed == 0)
                                {
                                    CharacterUtil.Animate(current_character);

                                    if (attack_type == "Cast")
                                    {
                                        AnimateCastEffect();
                                    }

                                    if (character_frame == animation_speed * 2)
                                    {
                                        Attack();
                                    }
                                    else if (character_frame >= animation_speed * 3)
                                    {
                                        combat_state = "AnimateEffects";
                                    }

                                    character_frame += Main.CombatSpeed;
                                }
                                else
                                {
                                    character_frame += Main.CombatSpeed;
                                }
                                
                                break;

                            case "AnimateEffects":
                                if (effect_frame >= animation_speed * 4)
                                {
                                    bool continue_attacking = false;
                                    Item weapon = InventoryUtil.Get_EquippedItem(current_character, "Weapon");
                                    if (InventoryUtil.Item_HasElement(weapon, "Time"))
                                    {
                                        int chance = InventoryUtil.Get_Item_Element_Level(weapon, "Time");
                                        if (Utility.RandomPercent(chance))
                                        {
                                            continue_attacking = true;
                                        }
                                    }

                                    if (continue_attacking)
                                    {
                                        RemoveEffects();
                                        ResetCombat();
                                    }
                                    else
                                    {
                                        combat_state = "MoveBackward";
                                    }
                                }
                                else if (effect_frame % animation_speed == 0)
                                {
                                    AnimateDamageEffects();
                                    AnimateDamageLabels();
                                    effect_frame += Main.CombatSpeed;
                                }
                                else
                                {
                                    AnimateDamageLabels();
                                    effect_frame += Main.CombatSpeed;
                                }
                                break;

                            case "MoveBackward":
                                if (CombatUtil.AtTile(current_character, origin_tile, move_speed))
                                {
                                    combat_state = "Finish";
                                }
                                else
                                {
                                    CombatUtil.MoveBack(current_character, move_speed);
                                }
                                break;

                            case "Finish":
                            default:
                                FinishAttack();
                                break;
                        }
                    }
                }
                else
                {
                    GetCurrentCharacter();

                    if (current_character == null)
                    {
                        FinishRound();
                    }
                }
            }
        }

        private void TileTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!paused &&
                (Visible || Active))
            {
                WorldUtil.AnimateTiles();
            }
        }

        private void GetCurrentCharacter()
        {
            if (current_character == null)
            {
                if (ally_squad != null)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            Character character = ally_squad.GetCharacter(new Vector2(x, y));
                            if (character != null &&
                                character.CombatStep == 0 &&
                                character.ManaBar.Value > 0 &&
                                !character.Dead)
                            {
                                current_character = character;
                                break;
                            }
                        }

                        if (current_character != null)
                        {
                            break;
                        }
                    }
                }
            }

            if (current_character == null)
            {
                if (enemy_squad != null)
                {
                    for (int x = 2; x >= 0; x--)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            Character character = enemy_squad.GetCharacter(new Vector2(x, y));
                            if (character != null &&
                                character.CombatStep == 0 &&
                                character.ManaBar.Value > 0 &&
                                !character.Dead)
                            {
                                current_character = character;
                                break;
                            }
                        }

                        if (current_character != null)
                        {
                            break;
                        }
                    }
                }
            }
        }

        private void StartCast()
        {
            AssetManager.PlaySound_Random("Cast");

            if (current_character.Type == "Ally")
            {
                Menu.AddPicture(Handler.GetID(), "Cast", AssetManager.Textures["Cast"],
                    new Region(current_character.Region.X, current_character.Region.Y, current_character.Region.Width, current_character.Region.Height),
                        RenderingManager.Lighting.DrawColor * 0.9f, true);
            }
            else if (current_character.Type == "Enemy")
            {
                Menu.AddPicture(Handler.GetID(), "Cast", AssetManager.Textures["EvilCast"],
                    new Region(current_character.Region.X, current_character.Region.Y, current_character.Region.Width, current_character.Region.Height),
                        RenderingManager.Lighting.DrawColor * 0.9f, true);
            }

            Picture cast = Menu.GetPicture("Cast");
            cast.Image = new Rectangle(0, 0, cast.Texture.Width / 4, cast.Texture.Height);
        }

        private void Attack()
        {
            if (current_character != null)
            {
                current_character.ManaBar.Value -= ep_cost;
                if (current_character.ManaBar.Value < 0)
                {
                    current_character.ManaBar.Value = 0;
                }
                current_character.ManaBar.Update();

                Item weapon = InventoryUtil.Get_EquippedItem(current_character, "Weapon");

                Defense(weapon);
                Offense(weapon);
            }
        }

        private void Defense(Item weapon)
        {
            if (InventoryUtil.Item_IsAoE(weapon, "Health"))
            {
                //Full party heal HP
                int hp = InventoryUtil.Get_Item_AoE_Level(weapon, "Health");
                if (current_character.Type == "Enemy")
                {
                    foreach (Character character in enemy_squad.Characters)
                    {
                        CombatUtil.DoHeal_HP(Menu, character, weapon, hp);
                    }
                }
                else
                {
                    foreach (Character character in ally_squad.Characters)
                    {
                        CombatUtil.DoHeal_HP(Menu, character, weapon, hp);
                    }
                }
            }
            else if (InventoryUtil.Item_HasElement(weapon, "Health"))
            {
                //Single target heal HP
                Character target;

                if (current_character.Type == "Enemy")
                {
                    target = CombatUtil.GetTarget_LeastHP(enemy_squad);
                }
                else
                {
                    target = CombatUtil.GetTarget_LeastHP(ally_squad);
                }

                if (target != null)
                {
                    int hp = InventoryUtil.Get_Item_Element_Level(weapon, "Health") * Handler.Element_Multiplier;
                    CombatUtil.DoHeal_HP(Menu, target, weapon, hp);
                }
            }
            else if (InventoryUtil.Item_IsAoE(weapon, "Energy"))
            {
                //Full party heal EP
                int ep = InventoryUtil.Get_Item_AoE_Level(weapon, "Energy");
                if (current_character.Type == "Enemy")
                {
                    foreach (Character character in enemy_squad.Characters)
                    {
                        CombatUtil.DoHeal_EP(Menu, character, weapon, ep);
                    }
                }
                else
                {
                    foreach (Character character in ally_squad.Characters)
                    {
                        CombatUtil.DoHeal_EP(Menu, character, weapon, ep);
                    }
                }
            }
            else if (InventoryUtil.Item_HasElement(weapon, "Energy"))
            {
                //Single target heal EP
                Character target;

                if (current_character.Type == "Enemy")
                {
                    target = CombatUtil.GetTarget_LeastEP(enemy_squad);
                }
                else
                {
                    target = CombatUtil.GetTarget_LeastEP(ally_squad);
                }

                if (target != null)
                {
                    int ep = InventoryUtil.Get_Item_Element_Level(weapon, "Energy") * Handler.Element_Multiplier;
                    CombatUtil.DoHeal_EP(Menu, target, weapon, ep);
                }
            }
        }

        private void Offense(Item weapon)
        {
            foreach (Character target in targets)
            {
                Squad defender_squad = ArmyUtil.Get_Squad(target.ID);

                bool dodge = false;

                int dodge_chance = CombatUtil.GetArmor_Resistance(target, "Time");
                dodge_chance += ArmyUtil.Get_AoE_Defense(defender_squad, target, "Time");

                if (Utility.RandomPercent(dodge_chance))
                {
                    dodge = true;
                }

                if (dodge)
                {
                    CombatUtil.DoDodge(Menu, target);
                }
                else
                {
                    if (InventoryUtil.Item_HasElement(weapon, "Death"))
                    {
                        //Instant kill
                        int chance = InventoryUtil.Get_Item_Element_Level(weapon, "Death");
                        if (Utility.RandomPercent(chance))
                        {
                            int resist_chance = CombatUtil.GetArmor_Resistance(target, "Death");
                            resist_chance += ArmyUtil.Get_AoE_Defense(defender_squad, target, "Death");

                            if (!Utility.RandomPercent(resist_chance))
                            {
                                CombatUtil.AddEffect(Menu, target, weapon, "Death");
                                target.HealthBar.Value = 0;
                                target.Dead = true;
                            }
                        }
                    }

                    if (!target.Dead)
                    {
                        CombatUtil.DoDamage(Menu, current_character, target, weapon, ref ally_total_damage, ref enemy_total_damage);

                        if (target.Dead)
                        {
                            if (target.Type == "Enemy")
                            {
                                gold += 100;
                                xp += 1;
                            }
                            else if (target.ID == Handler.MainCharacter_ID)
                            {
                                MainCharacterKilled();
                                break;
                            }
                            else
                            {
                                foreach (Character character in enemy_squad.Characters)
                                {
                                    CombatUtil.GainExp(character, 1);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AnimateDamageLabels()
        {
            for (int i = 0; i < Menu.Labels.Count; i++)
            {
                Label label = Menu.Labels[i];
                if (label.Name == "Damage" &&
                    label.Region != null)
                {
                    label.Region.Y -= 1;
                    label.Opacity -= 0.05f;

                    if (label.Opacity <= 0)
                    {
                        Menu.Labels.Remove(label);
                        i--;
                    }
                }
            }
        }

        private void AnimateDamageEffects()
        {
            for (int i = 0; i < Menu.Pictures.Count; i++)
            {
                Picture picture = Menu.Pictures[i];
                if (picture.Name == "Damage")
                {
                    int X = picture.Image.X + picture.Image.Height;
                    if (X >= picture.Texture.Width)
                    {
                        Menu.Pictures.Remove(picture);
                        i--;
                    }
                    else
                    {
                        picture.Image = new Rectangle(X, picture.Image.Y, picture.Image.Width, picture.Image.Height);
                    }
                }
            }
        }

        private void AnimateCastEffect()
        {
            for (int i = 0; i < Menu.Pictures.Count; i++)
            {
                Picture picture = Menu.Pictures[i];
                if (picture.Name == "Cast")
                {
                    int X = picture.Image.X + picture.Image.Height;
                    if (X >= picture.Texture.Width)
                    {
                        Menu.Pictures.Remove(picture);
                        i--;
                    }
                    else
                    {
                        picture.Image = new Rectangle(X, picture.Image.Y, picture.Image.Width, picture.Image.Height);
                        picture.Opacity -= 0.2f;
                    }
                }
            }
        }

        private void AnimateMouseClick()
        {
            Picture mouseClick = Menu.GetPicture("MouseClick");
            if (mouseClick.Visible)
            {
                if (mouseClickDelay >= 10)
                {
                    mouseClickDelay = 0;

                    int X = mouseClick.Image.X + mouseClick.Image.Height;
                    if (X >= mouseClick.Texture.Width)
                    {
                        X = 0;
                    }

                    mouseClick.Image = new Rectangle(X, mouseClick.Image.Y, mouseClick.Image.Width, mouseClick.Image.Height);
                }
                else
                {
                    mouseClickDelay++;
                }
            }
        }

        private void RemoveEffects()
        {
            for (int i = 0; i < Menu.Labels.Count; i++)
            {
                Label label = Menu.Labels[i];
                if (label.Name == "Damage")
                {
                    Menu.Labels.Remove(label);
                    i--;
                }
            }

            foreach (Character target in targets)
            {
                Something damage = target.GetStatusEffect("Damage");
                if (damage != null)
                {
                    target.StatusEffects.Remove(damage);
                }
            }

            for (int i = 0; i < Menu.Pictures.Count; i++)
            {
                Picture picture = Menu.Pictures[i];
                if (picture.Name == "Damage" ||
                    picture.Name == "Cast")
                {
                    Menu.Pictures.Remove(picture);
                    i--;
                }
            }
        }

        private void ResetCombat()
        {
            combat_state = "GetTargets";
            character_frame = 0;
            effect_frame = 0;
            attack_type = "";
            ep_cost = 0;
            targets.Clear();
        }

        private void FinishAttack()
        {
            if (current_character != null)
            {
                RemoveEffects();
                CombatUtil.SwitchAnimation(current_character, "Idle");

                if (ally_squad != null)
                {
                    for (int i = 0; i < ally_squad.Characters.Count; i++)
                    {
                        Character character = ally_squad.Characters[i];
                        if (character.Dead)
                        {
                            ally_squad.Characters.Remove(character);
                            i--;
                        }
                    }

                    foreach (Character character in ally_squad.Characters)
                    {
                        if (character.ID == current_character.ID)
                        {
                            character.CombatStep = 1;
                            break;
                        }
                    }
                }

                if (enemy_squad != null)
                {
                    for (int i = 0; i < enemy_squad.Characters.Count; i++)
                    {
                        Character character = enemy_squad.Characters[i];
                        if (character.Dead)
                        {
                            enemy_squad.Characters.Remove(character);
                            i--;
                        }
                    }

                    foreach (Character character in enemy_squad.Characters)
                    {
                        if (character.ID == current_character.ID)
                        {
                            character.CombatStep = 1;
                            break;
                        }
                    }
                }

                current_character = null;
            }

            ResetCombat();
        }

        private void FinishRound()
        {
            if (ally_squad != null)
            {
                foreach (Character character in ally_squad.Characters)
                {
                    character.CombatStep = 0;
                }
            }

            if (enemy_squad != null)
            {
                foreach (Character character in enemy_squad.Characters)
                {
                    character.CombatStep = 0;
                }
            }

            GetCurrentCharacter();
        }

        private void FinishCombat()
        {
            SoundManager.AmbientPaused = true;
            Handler.CombatTimer.Stop();
            Handler.CombatTimer_Tiles.Stop();

            if (!ally_squad.Characters.Any())
            {
                CharacterManager.GetArmy("Ally").Squads.Remove(ally_squad);
            }

            if (!enemy_squad.Characters.Any())
            {
                CharacterManager.GetArmy("Enemy").Squads.Remove(enemy_squad);
            }

            Button button = Menu.GetButton("Result");

            if (!enemy_squad.Characters.Any())
            {
                won_battle = true;
                Menu.GetPicture("Result").Texture = AssetManager.Textures["Victory"];
                button.Text = ally_squad.Name + " was victorious!";
            }
            else if (!ally_squad.Characters.Any())
            {
                won_battle = false;
                Menu.GetPicture("Result").Texture = AssetManager.Textures["Defeat"];
                button.Text = ally_squad.Name + " was defeated!";
            }
            else if (enemy_total_damage > ally_total_damage)
            {
                won_battle = false;
                Menu.GetPicture("Result").Texture = AssetManager.Textures["Defeat"];
                button.Text = ally_squad.Name + " was defeated!\n\n" +
                    "Allies Total Damage: " + ally_total_damage + "\n" +
                    "Enemies Total Damage: " + enemy_total_damage;
            }
            else if (ally_total_damage > enemy_total_damage)
            {
                won_battle = true;
                Menu.GetPicture("Result").Texture = AssetManager.Textures["Victory"];
                button.Text = ally_squad.Name + " was victorious!\n\n" +
                    "Allies Total Damage: " + ally_total_damage + "\n" +
                    "Enemies Total Damage: " + enemy_total_damage;
            }
            else
            {
                won_battle = false;
                Menu.GetPicture("Result").Texture = AssetManager.Textures["Draw"];
                button.Text = "Both parties are retreating.";
            }

            Menu.GetButton("Retreat").Visible = false;
            Menu.GetButton("PlayPause").Visible = false;
            Menu.GetButton("Speed").Visible = false;

            Menu.GetPicture("Result").Visible = true;
            Menu.GetPicture("MouseClick").Visible = true;
            button.Visible = true;

            if (won_battle)
            {
                if (gold > 0 ||
                xp > 0)
                {
                    Handler.Gold += gold;
                    button.Text += "\n\n" + gold + " Gold was looted!";

                    foreach (Character character in ally_squad.Characters)
                    {
                        CombatUtil.GainExp(character, xp);
                    }
                    button.Text += "\n" + xp + " XP was gained!";
                }

                Character leader = ally_squad.GetLeader();
                if (leader == null)
                {
                    //If leader was killed, assign new leader
                    ally_squad.Leader_ID = ally_squad.Characters[0].ID;
                }
                else
                {
                    //If leader was not killed, remove enemy as target
                    leader.Target_ID = 0;
                }
            }

            gold = 0;
            xp = 0;
        }

        private void Retreat()
        {
            Handler.CombatTimer.Stop();
            Handler.CombatTimer_Tiles.Stop();

            won_battle = false;

            Picture battleResult = Menu.GetPicture("Result");
            battleResult.Texture = AssetManager.Textures["Defeat"];
            battleResult.Visible = true;

            Button button = Menu.GetButton("Result");
            button.Text = ally_squad.Name + " is retreating.";

            Menu.GetButton("Retreat").Visible = false;
            Menu.GetPicture("MouseClick").Visible = true;
            button.Visible = true;

            if (gold > 0 ||
                xp > 0)
            {
                Handler.Gold += gold;
                button.Text += "\n\n" + gold + " Gold was looted!";

                foreach (Character character in ally_squad.Characters)
                {
                    CombatUtil.GainExp(character, xp);
                }
                button.Text += "\n" + xp + " XP was gained!";
            }

            gold = 0;
            xp = 0;
        }

        private void Leave()
        {
            Scene localmap = SceneManager.GetScene("Localmap");
            Map map = localmap.World.Maps[Handler.Level];
            Layer ground = map.GetLayer("Ground");

            if (!won_battle &&
                ally_squad.Characters.Any())
            {
                //Bump squad away from enemy
                if (ally_squad.Direction == Direction.North)
                {
                    ally_squad.Location = new Location(ally_squad.Location.X, ally_squad.Location.Y + 1, 0);
                }
                else if (ally_squad.Direction == Direction.East)
                {
                    ally_squad.Location = new Location(ally_squad.Location.X - 1, ally_squad.Location.Y, 0);
                }
                else if (ally_squad.Direction == Direction.South)
                {
                    ally_squad.Location = new Location(ally_squad.Location.X, ally_squad.Location.Y - 1, 0);
                }
                else if (ally_squad.Direction == Direction.West)
                {
                    ally_squad.Location = new Location(ally_squad.Location.X + 1, ally_squad.Location.Y, 0);
                }

                Tile tile = ground.GetTile(new Vector2(ally_squad.Location.X, ally_squad.Location.Y));
                ally_squad.Region = new Region(tile.Region.X, tile.Region.Y, tile.Region.Width, tile.Region.Height);
            }

            Handler.CombatTimer.Stop();
            Handler.CombatTimer_Tiles.Stop();
            Handler.Combat = false;

            Menu ui = MenuManager.GetMenu("UI");
            ui.Active = true;
            ui.Visible = true;

            SoundManager.StopMusic();
            SoundManager.NeedMusic = true;
            SoundManager.AmbientPaused = false;

            SceneManager.ChangeScene("Localmap");

            Main.Timer.Start();
            GameUtil.Toggle_Pause(false);
        }

        private void MainCharacterKilled()
        {
            SoundManager.AmbientPaused = true;
            Handler.CombatTimer.Stop();
            Handler.CombatTimer_Tiles.Stop();
            hero_killed = true;

            Menu.GetPicture("Result").Texture = AssetManager.Textures["Defeat"];

            Button button = Menu.GetButton("Result");
            button.Text = GameUtil.WrapText(ally_squad.Name + " has been slain!\n\nThe story cannot continue without its hero...");

            Menu.GetButton("Retreat").Visible = false;
            Menu.GetButton("PlayPause").Visible = false;
            Menu.GetButton("Speed").Visible = false;

            Menu.GetPicture("Result").Visible = true;
            Menu.GetPicture("MouseClick").Visible = true;
            button.Visible = true;
        }

        public void UpdateGrids()
        {
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    Character character = enemy_squad.GetCharacter(new Vector2(x, y));
                    if (character != null)
                    {
                        Label hp_label = Menu.GetLabel(character.ID + "_HP,x:" + x.ToString() + ",y:" + y.ToString());
                        if (hp_label != null)
                        {
                            hp_label.Text = character.HealthBar.Value + "/" + character.HealthBar.Max_Value + " HP";
                        }

                        Label ep_label = Menu.GetLabel(character.ID + "_EP,x:" + x.ToString() + ",y:" + y.ToString());
                        if (ep_label != null)
                        {
                            ep_label.Text = character.ManaBar.Value + "/" + character.ManaBar.Max_Value + " EP";
                        }
                    }
                }
            }

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    Character character = ally_squad.GetCharacter(new Vector2(x, y));
                    if (character != null)
                    {
                        Label hp_label = Menu.GetLabel(character.ID + "_HP,x:" + x.ToString() + ",y:" + y.ToString());
                        if (hp_label != null)
                        {
                            hp_label.Text = character.HealthBar.Value + "/" + character.HealthBar.Max_Value + " HP";
                        }

                        Label ep_label = Menu.GetLabel(character.ID + "_EP,x:" + x.ToString() + ",y:" + y.ToString());
                        if (ep_label != null)
                        {
                            ep_label.Text = character.ManaBar.Value + "/" + character.ManaBar.Max_Value + " EP";
                        }
                    }
                }
            }
        }

        public void LoadGrids()
        {
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    Menu.AddPicture(Handler.GetID(), "Enemy,x:" + x.ToString() + ",y:" + y.ToString(), AssetManager.Textures["Grid"],
                        new Region(0, 0, 0, 0), Color.White * 0.8f, true);

                    Character character = enemy_squad.GetCharacter(new Vector2(x, y));
                    if (character != null)
                    {
                        Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), character.ID + "_Name,x:" + x.ToString() + ",y:" + y.ToString(), character.Name, Color.White,
                            new Region(0, 0, 0, 0), true);

                        Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), character.ID + "_HP,x:" + x.ToString() + ",y:" + y.ToString(), character.HealthBar.Value + "/" + character.HealthBar.Max_Value + " HP", Color.Red,
                            new Region(0, 0, 0, 0), true);

                        Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), character.ID + "_EP,x:" + x.ToString() + ",y:" + y.ToString(), character.ManaBar.Value + "/" + character.ManaBar.Max_Value + " EP", Color.Blue,
                            new Region(0, 0, 0, 0), true);
                    }
                }
            }

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    Menu.AddPicture(Handler.GetID(), "Ally,x:" + x.ToString() + ",y:" + y.ToString(), AssetManager.Textures["Grid"],
                        new Region(0, 0, 0, 0), Color.White * 0.8f, true);

                    Character character = ally_squad.GetCharacter(new Vector2(x, y));
                    if (character != null)
                    {
                        Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), character.ID + "_Name,x:" + x.ToString() + ",y:" + y.ToString(), character.Name, Color.White,
                            new Region(0, 0, 0, 0), true);

                        Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), character.ID + "_HP,x:" + x.ToString() + ",y:" + y.ToString(), character.HealthBar.Value + "/" + character.HealthBar.Max_Value + " HP", Color.Red,
                            new Region(0, 0, 0, 0), true);

                        Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), character.ID + "_EP,x:" + x.ToString() + ",y:" + y.ToString(), character.ManaBar.Value + "/" + character.ManaBar.Max_Value + " EP", Color.Blue,
                            new Region(0, 0, 0, 0), true);
                    }
                }
            }
        }

        public void ResizeGrids()
        {
            int width = Main.Game.MenuSize.X * 2;
            int height = Main.Game.MenuSize.Y * 2;
            int starting_x = 0;
            int starting_y = 0;

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    Menu.GetPicture("Enemy,x:" + x.ToString() + ",y:" + y.ToString()).Region = new Region(starting_x + (width * x), starting_y + (height * y), width, height);

                    Character character = enemy_squad.GetCharacter(new Vector2(x, y));
                    if (character != null)
                    {
                        Label name_label = Menu.GetLabel(character.ID + "_Name,x:" + x.ToString() + ",y:" + y.ToString());
                        if (name_label != null)
                        {
                            name_label.Region = new Region(starting_x + (width * x), starting_y + (height * y), width, height / 4);
                        }

                        Label hp_label = Menu.GetLabel(character.ID + "_HP,x:" + x.ToString() + ",y:" + y.ToString());
                        if (hp_label != null)
                        {
                            hp_label.Region = new Region(starting_x + (width * x), starting_y + (height * y) + (height / 4), width, height / 4);
                        }

                        Label ep_label = Menu.GetLabel(character.ID + "_EP,x:" + x.ToString() + ",y:" + y.ToString());
                        if (ep_label != null)
                        {
                            ep_label.Region = new Region(starting_x + (width * x), starting_y + (height * y) + (height / 2), width, height / 4);
                        }
                    }
                }
            }

            starting_x = Main.Game.Resolution.X - (width * 3);
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    Menu.GetPicture("Ally,x:" + x.ToString() + ",y:" + y.ToString()).Region = new Region(starting_x + (width * x), starting_y + (height * y), width, height);

                    Character character = ally_squad.GetCharacter(new Vector2(x, y));
                    if (character != null)
                    {
                        Label name_label = Menu.GetLabel(character.ID + "_Name,x:" + x.ToString() + ",y:" + y.ToString());
                        if (name_label != null)
                        {
                            name_label.Region = new Region(starting_x + (width * x), starting_y + (height * y), width, height / 4);
                        }

                        Label hp_label = Menu.GetLabel(character.ID + "_HP,x:" + x.ToString() + ",y:" + y.ToString());
                        if (hp_label != null)
                        {
                            hp_label.Region = new Region(starting_x + (width * x), starting_y + (height * y) + (height / 4), width, height / 4);
                        }

                        Label ep_label = Menu.GetLabel(character.ID + "_EP,x:" + x.ToString() + ",y:" + y.ToString());
                        if (ep_label != null)
                        {
                            ep_label.Region = new Region(starting_x + (width * x), starting_y + (height * y) + (height / 2), width, height / 4);
                        }
                    }
                }
            }
        }

        private void Toggle_Pause()
        {
            Button button = Menu.GetButton("PlayPause");

            if (paused)
            {
                paused = false;
                SoundManager.AmbientPaused = false;

                button.HoverText = "Pause";
                button.Texture = AssetManager.Textures["Button_Pause"];
                button.Texture_Highlight = AssetManager.Textures["Button_Pause_Hover"];
                button.Texture_Disabled = AssetManager.Textures["Button_Pause_Disabled"];
            }
            else
            {
                paused = true;
                SoundManager.AmbientPaused = true;

                button.HoverText = "Play";
                button.Texture = AssetManager.Textures["Button_Play"];
                button.Texture_Highlight = AssetManager.Textures["Button_Play_Hover"];
                button.Texture_Disabled = AssetManager.Textures["Button_Play_Disabled"];
            }
        }

        private void SpeedToggle()
        {
            Main.CombatSpeed *= 2;
            if (Main.CombatSpeed > 16)
            {
                Main.CombatSpeed = 2;
            }

            Button button = Menu.GetButton("Speed");

            if (Main.CombatSpeed == 2)
            {
                button.HoverText = "Speed x1";
                button.Texture = AssetManager.Textures["Button_Speed1"];
                button.Texture_Highlight = AssetManager.Textures["Button_Speed1_Hover"];
                button.Texture_Disabled = AssetManager.Textures["Button_Speed1_Disabled"];
            }
            else if (Main.CombatSpeed == 4)
            {
                button.HoverText = "Speed x2";
                button.Texture = AssetManager.Textures["Button_Speed2"];
                button.Texture_Highlight = AssetManager.Textures["Button_Speed2_Hover"];
                button.Texture_Disabled = AssetManager.Textures["Button_Speed2_Disabled"];
            }
            else if (Main.CombatSpeed == 8)
            {
                button.HoverText = "Speed x3";
                button.Texture = AssetManager.Textures["Button_Speed3"];
                button.Texture_Highlight = AssetManager.Textures["Button_Speed3_Hover"];
                button.Texture_Disabled = AssetManager.Textures["Button_Speed3_Disabled"];
            }
            else if (Main.CombatSpeed == 16)
            {
                button.HoverText = "Speed x4";
                button.Texture = AssetManager.Textures["Button_Speed4"];
                button.Texture_Highlight = AssetManager.Textures["Button_Speed4_Hover"];
                button.Texture_Disabled = AssetManager.Textures["Button_Speed4_Disabled"];
            }

            move_speed = base_move_speed * (Main.CombatSpeed / 2);

            Save.ExportINI();
        }

        public override void Load()
        {
            Menu.Clear();

            if (!string.IsNullOrEmpty(Handler.Combat_Terrain))
            {
                WorldGen.GenCombatMap();

                ally_squad = CharacterManager.GetArmy("Ally").GetSquad(Handler.Combat_Ally_Squad);
                enemy_squad = CharacterManager.GetArmy("Enemy").GetSquad(Handler.Combat_Enemy_Squad);

                if (Handler.Combat_Terrain == "Grass")
                {
                    Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Grass"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                }
                else if (Handler.Combat_Terrain == "Water")
                {
                    if (TimeManager.Now.Hours >= 22 ||
                        TimeManager.Now.Hours <= 5)
                    {
                        Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Sky_Night2"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                    }
                    else
                    {
                        Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Sky_Day"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                    }
                }
                else if (Handler.Combat_Terrain == "Desert")
                {
                    Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Desert"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                }
                else if (Handler.Combat_Terrain == "Snow" ||
                         Handler.Combat_Terrain == "Ice")
                {
                    Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Snow"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                }
                else if (Handler.Combat_Terrain.Contains("Forest"))
                {
                    if (Handler.Combat_Terrain.Contains("Snow"))
                    {
                        Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Forest_Snow"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                    }
                    else
                    {
                        Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Forest"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                    }
                }
                else if (Handler.Combat_Terrain.Contains("Mountains"))
                {
                    if (Handler.Combat_Terrain.Contains("Snow"))
                    {
                        Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Mountains_Snow"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                    }
                    else if (Handler.Combat_Terrain.Contains("Desert"))
                    {
                        Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Mountains_Desert"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                    }
                    else
                    {
                        Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Mountains"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                    }
                }

                Menu.AddButton(new ButtonOptions
                {
                    id = Handler.GetID(),
                    name = "PlayPause",
                    hover_text = "Pause",
                    texture = AssetManager.Textures["Button_Pause"],
                    texture_highlight = AssetManager.Textures["Button_Pause_Hover"],
                    texture_disabled = AssetManager.Textures["Button_Pause_Disabled"],
                    region = new Region(0, 0, 0, 0),
                    draw_color = Color.White * 0.8f,
                    enabled = true,
                    visible = true
                });

                Menu.AddButton(new ButtonOptions
                {
                    id = Handler.GetID(),
                    name = "Speed",
                    hover_text = "Speed x1",
                    texture = AssetManager.Textures["Button_Speed1"],
                    texture_highlight = AssetManager.Textures["Button_Speed1_Hover"],
                    texture_disabled = AssetManager.Textures["Button_Speed1_Disabled"],
                    region = new Region(0, 0, 0, 0),
                    draw_color = Color.White * 0.8f,
                    enabled = true,
                    visible = true
                });

                Button speed_button = Menu.GetButton("Speed");

                if (Main.CombatSpeed == 2)
                {
                    speed_button.HoverText = "Speed x1";
                    speed_button.Texture = AssetManager.Textures["Button_Speed1"];
                    speed_button.Texture_Highlight = AssetManager.Textures["Button_Speed1_Hover"];
                    speed_button.Texture_Disabled = AssetManager.Textures["Button_Speed1_Disabled"];
                }
                else if (Main.CombatSpeed == 4)
                {
                    speed_button.HoverText = "Speed x2";
                    speed_button.Texture = AssetManager.Textures["Button_Speed2"];
                    speed_button.Texture_Highlight = AssetManager.Textures["Button_Speed2_Hover"];
                    speed_button.Texture_Disabled = AssetManager.Textures["Button_Speed2_Disabled"];
                }
                else if (Main.CombatSpeed == 8)
                {
                    speed_button.HoverText = "Speed x3";
                    speed_button.Texture = AssetManager.Textures["Button_Speed3"];
                    speed_button.Texture_Highlight = AssetManager.Textures["Button_Speed3_Hover"];
                    speed_button.Texture_Disabled = AssetManager.Textures["Button_Speed3_Disabled"];
                }
                else if (Main.CombatSpeed == 16)
                {
                    speed_button.HoverText = "Speed x4";
                    speed_button.Texture = AssetManager.Textures["Button_Speed4"];
                    speed_button.Texture_Highlight = AssetManager.Textures["Button_Speed4_Hover"];
                    speed_button.Texture_Disabled = AssetManager.Textures["Button_Speed4_Disabled"];
                }

                move_speed = base_move_speed * (Main.CombatSpeed / 2);

                Menu.AddButton(new ButtonOptions
                {
                    id = Handler.GetID(),
                    font = AssetManager.Fonts["ControlFont"],
                    name = "Retreat",
                    text = "Retreat",
                    texture = AssetManager.Textures["ButtonFrame"],
                    texture_highlight = AssetManager.Textures["ButtonFrame_Highlight"],
                    region = new Region(0, 0, 0, 0),
                    draw_color = Color.White,
                    draw_color_selected = Color.White,
                    text_color = Color.White,
                    text_selected_color = Color.Red,
                    enabled = true,
                    visible = true
                });

                Menu.AddPicture(Handler.GetID(), "Result", AssetManager.Textures["Victory"], new Region(0, 0, 0, 0), Color.White, false);

                Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"],
                    new Region(0, 0, 0, 0), false);

                Menu.AddPicture(Handler.GetID(), "MouseClick", AssetManager.Textures["LeftClick"], new Region(0, 0, 0, 0), Color.White, false);
                Picture mouseClick = Menu.GetPicture("MouseClick");
                mouseClick.Image = new Rectangle(0, 0, mouseClick.Texture.Width / 4, mouseClick.Texture.Height);

                Menu.AddButton(new ButtonOptions
                {
                    id = Handler.GetID(),
                    font = AssetManager.Fonts["ControlFont"],
                    name = "Result",
                    texture = AssetManager.Textures["TextFrame"],
                    texture_highlight = AssetManager.Textures["TextFrame"],
                    region = new Region(0, 0, 0, 0),
                    draw_color = Color.White,
                    draw_color_selected = Color.Red,
                    text_color = Color.Red,
                    text_selected_color = Color.White,
                    enabled = true,
                    visible = false
                });

                LoadGrids();

                Menu.Visible = true;

                Resize(Main.Game.Resolution);

                Handler.CombatTimer.Start();
                Handler.CombatTimer_Tiles.Start();
            }
        }

        public override void Resize(Point point)
        {
            base.Resize(point);

            if (World.Maps.Any())
            {
                Map map = World.Maps[0];

                WorldUtil.Resize_OnCombat(World);

                int height = Main.Game.MenuSize.X;

                Layer ground = map.GetLayer("Ground");
                Tile tile = ground.Tiles[0];
                Menu.GetPicture("Background").Region = new Region(0, 0, Main.Game.Resolution.X, tile.Region.Y);

                base_move_speed = tile.Region.Width / 8;
                move_speed = base_move_speed * (Main.CombatSpeed / 2);

                Menu.GetPicture("Result").Region = new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y);

                Button result = Menu.GetButton("Result");
                result.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize.X * 4), 
                    Main.Game.ScreenHeight - (Main.Game.MenuSize.X * 5), Main.Game.MenuSize.X * 8, height * 3);

                Menu.GetButton("PlayPause").Region = new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize.X, Main.Game.MenuSize.Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);
                Menu.GetButton("Speed").Region = new Region(Main.Game.ScreenWidth / 2, Main.Game.MenuSize.Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);
                Menu.GetButton("Retreat").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize.X * 2), result.Region.Y + result.Region.Height, Main.Game.MenuSize.X * 4, height);
                Menu.GetPicture("MouseClick").Region = new Region(result.Region.X + result.Region.Width, result.Region.Y + result.Region.Height - height, height, height);

                Menu.GetLabel("Examine").Region = new Region(0, 0, 0, 0);

                Picture base_image = Menu.GetPicture("Base");
                if (base_image != null)
                {
                    base_image.Region = new Region(0, 0, Main.Game.Resolution.X, tile.Region.Y + (tile.Region.Height / 2));
                }

                ResizeGrids();
            }
        }

        #endregion
    }
}

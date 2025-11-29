using System.Collections.Generic;
using System.Linq;
using System.Timers;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Characters;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Inventories;
using OP_Engine.Menus;
using OP_Engine.Rendering;
using OP_Engine.Scenes;
using OP_Engine.Sounds;
using OP_Engine.Tiles;
using OP_Engine.Time;
using OP_Engine.Utility;
using OP_Engine.Weathers;

using DoS1.Util;

namespace DoS1.Scenes
{
    public class Combat : Scene
    {
        #region Variables

        private bool step;

        private Squad ally_squad;
        private Squad enemy_squad;

        private List<Character> targets = new List<Character>();
        private Character current_character = null;
        private string attack_type = "";
        private bool hero_killed = false;

        private List<Character> counter_attackers = new List<Character>();
        private Character initial_attacker = null;
        private bool counter_attacking = false;
        private bool skip_turn = false;

        private float base_move_speed = 0;
        private float move_speed = 0;

        private int character_frame = 0;
        private int effect_frame = 0;
        private readonly int animation_speed = 16;
        private readonly int label_speed = 4;

        private string combat_state = "StatusEffects";

        private int ally_total_damage;
        private int enemy_total_damage;

        private bool won_battle;
        private int gold;
        private int xp;
        private int xp_base = 2;
        private int rp;
        private int rp_base = 1;

        #endregion

        #region Constructors

        public Combat(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Combat";
            Load(content);

            Handler.CombatTimer.Elapsed += Timer_Elapsed;
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
                    if (Handler.StoryStep > 44)
                    {
                        UpdateControls();
                    }

                    AnimateCharacters();
                    UpdateGrids();
                }

                if ((Handler.StoryStep >= 38 && Handler.StoryStep <= 44) ||
                    (Handler.StoryStep >= 50 && Handler.StoryStep <= 54))
                {
                    GameUtil.Alert_Story(Menu);
                }
            }
        }

        public override void DrawWorld(SpriteBatch spriteBatch, Point resolution, Color color)
        {
            if (Visible)
            {
                Picture background = Menu.GetPicture("Background");
                background.DrawColor = RenderingManager.Lighting.DrawColor;
                background.Draw(spriteBatch);

                Menu.GetPicture("Highlight").Draw(spriteBatch);

                Color lightColor = Color.Lerp(Color.White, RenderingManager.Lighting.DrawColor, 0.5f);

                if (ally_squad != null)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            Character character = ally_squad.GetCharacter(new Vector2(x, y));
                            if (character != null)
                            {
                                Something damage = character.GetStatusEffect("Damage");
                                Label damage_label = Menu.GetLabel(character.ID);
                                if (damage != null &&
                                    damage_label != null)
                                {
                                    CharacterUtil.DrawCharacter_Combat(spriteBatch, character, damage.DrawColor);
                                    damage.DrawColor = Color.Lerp(damage.DrawColor, lightColor, 0.025f);
                                }
                                else
                                {
                                    CharacterUtil.DrawCharacter_Combat(spriteBatch, character, lightColor);
                                }

                                DrawDamageEffects(spriteBatch, character);
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
                                Something damage = character.GetStatusEffect("Damage");
                                Label damage_label = Menu.GetLabel(character.ID);
                                if (damage != null &&
                                    damage_label != null)
                                {
                                    CharacterUtil.DrawCharacter_Combat(spriteBatch, character, damage.DrawColor);
                                    damage.DrawColor = Color.Lerp(damage.DrawColor, lightColor, 0.025f);
                                }
                                else
                                {
                                    CharacterUtil.DrawCharacter_Combat(spriteBatch, character, lightColor);
                                }

                                DrawDamageEffects(spriteBatch, character);
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
                        picture.Name != "Background" &&
                        picture.Name != "Highlight")
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

        private void DrawDamageEffects(SpriteBatch spriteBatch, Character character)
        {
            for (int i = 0; i < Menu.Pictures.Count; i++)
            {
                Picture picture = Menu.Pictures[i];
                if (picture.ID == character.ID)
                {
                    if (picture.Name == "Damage" ||
                        picture.Name == "Cast")
                    {
                        if (picture.Texture.Name == "Lightning" ||
                            picture.Texture.Name == "Fire" ||
                            picture.Texture.Name == "Ice" ||
                            picture.Texture.Name == "Heal" ||
                            picture.Texture.Name == "Thump")
                        {
                            int radius = 1;
                            if (picture.Texture.Name == "Fire")
                            {
                                radius = 5;
                            }

                            ShaderUtil.Apply_GaussianBlur(spriteBatch, radius, picture.Texture, picture.Region, picture.Image, picture.DrawColor, picture.Opacity);
                        }
                        else
                        {
                            spriteBatch.Draw(picture.Texture, picture.Region.ToRectangle, picture.Image, picture.DrawColor * picture.Opacity);
                        }
                    }
                }
            }

            for (int i = 0; i < Menu.Labels.Count; i++)
            {
                Label label = Menu.Labels[i];
                if (label.ID == character.ID &&
                    label.Name == "Damage")
                {
                    label.Draw(spriteBatch);
                }
            }
        }

        private void UpdateControls()
        {
            bool found_button = HoveringButton();
            bool found_grid = HoveringGrid();

            if (!found_button &&
                !found_grid)
            {
                Menu.GetLabel("Examine").Visible = false;
            }

            if (InputManager.KeyPressed("Space"))
            {
                GameUtil.Toggle_Pause_Combat(true);
            }
        }

        private bool HoveringButton()
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
                            found = false;
                            CheckClick(button);

                            button.Opacity = 0.9f;
                            button.Selected = false;

                            break;
                        }
                    }
                    else if (InputManager.Mouse.Moved)
                    {
                        button.Opacity = 0.9f;
                        button.Selected = false;
                    }
                }
            }

            return found;
        }

        private bool HoveringGrid()
        {
            Picture highlight = Menu.GetPicture("Highlight");
            
            Map map = World.Maps[0];
            Layer ground = map.GetLayer("Ground");

            int count = ground.Tiles.Count;
            for (int i = 0; i < count; i++)
            {
                Tile tile = ground.Tiles[i];

                if (InputManager.MouseWithin(tile.Region.ToRectangle))
                {
                    Character character = null;
                    if (ally_squad != null &&
                        ally_squad.Characters.Count > 0)
                    {
                        foreach (Character existing in ally_squad.Characters)
                        {
                            if (existing.Formation.X + 7 == tile.Location.X &&
                                existing.Formation.Y + 1 == tile.Location.Y)
                            {
                                character = existing;
                                break;
                            }
                        }
                    }

                    if (character == null)
                    {
                        if (enemy_squad != null &&
                            enemy_squad.Characters.Count > 0)
                        {
                            foreach (Character existing in enemy_squad.Characters)
                            {
                                if (existing.Formation.X + 1 == tile.Location.X &&
                                    existing.Formation.Y + 1 == tile.Location.Y)
                                {
                                    character = existing;
                                    break;
                                }
                            }
                        }
                    }

                    if (character != null)
                    {
                        highlight.Region = new Region(tile.Region.X, tile.Region.Y, tile.Region.Width, tile.Region.Height);
                        highlight.Visible = true;

                        CharacterUtil.ExamineCharacter(Menu, character);

                        return true;
                    }
                }
            }

            highlight.Visible = false;
            return false;
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse.Flush();

            if (button.Name == "PlayPause")
            {
                GameUtil.Toggle_Pause_Combat(true);
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
                    ally_army.Squads.Count == 0)
                {
                    ResetCombat_Final();
                    Handler.Combat = false;
                    Main.Timer.Start();

                    SoundManager.StopMusic();
                    SoundManager.NeedMusic = true;

                    SceneManager.ChangeScene("GameOver");

                    hero_killed = false;
                }
                else
                {
                    Leave();
                }
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!step)
            {
                if (!TimeManager.Paused &&
                    !Handler.CombatPause)
                {
                    step = true;

                    if (current_character != null)
                    {
                        Tile origin_tile = CombatUtil.OriginTile(World, current_character);
                        Tile target_tile = CombatUtil.TargetTile(World, current_character);

                        if (origin_tile != null &&
                            target_tile != null)
                        {
                            Squad target_squad = null;
                            if (targets.Any())
                            {
                                target_squad = ArmyUtil.Get_Squad(targets[0].ID);
                            }

                            switch (combat_state)
                            {
                                case "StatusEffects":
                                    #region StatusEffects

                                    if (current_character.StatusEffects.Any())
                                    {
                                        for (int i = 0; i < current_character.StatusEffects.Count; i++)
                                        {
                                            Something statusEffect = current_character.StatusEffects[i];
                                            if (statusEffect.Name != "Damage")
                                            {
                                                if (statusEffect.Name == "Petrified" ||
                                                    statusEffect.Name == "Stunned" ||
                                                    statusEffect.Name == "Frozen" ||
                                                    statusEffect.Name == "Shocked")
                                                {
                                                    skip_turn = true;
                                                }

                                                CombatUtil.DoDamage_ForStatus(Menu, current_character, statusEffect, ref ally_total_damage, ref enemy_total_damage);

                                                if (statusEffect.Name == "Weak" ||
                                                    statusEffect.Name == "Melting" ||
                                                    statusEffect.Name == "Burning" ||
                                                    statusEffect.Name == "Regenerating" ||
                                                    statusEffect.Name == "Charging" ||
                                                    statusEffect.Name == "Stunned" ||
                                                    statusEffect.Name == "Slow" ||
                                                    statusEffect.Name == "Frozen" ||
                                                    statusEffect.Name == "Shocked")
                                                {
                                                    statusEffect.Amount--;
                                                    if (statusEffect.Amount <= 0)
                                                    {
                                                        current_character.StatusEffects.Remove(statusEffect);
                                                        RuneUtil.AddCombatLabel(Menu, current_character, "-" + statusEffect.Name, Color.White);
                                                        i--;
                                                    }
                                                }
                                            }
                                        }

                                        combat_state = "AnimateStatusEffects";
                                    }
                                    else
                                    {
                                        combat_state = "GetTargets";
                                    }

                                    #endregion
                                    break;

                                case "AnimateStatusEffects":
                                    #region AnimateStatusEffects

                                    if (DamageEffectExists())
                                    {
                                        if (effect_frame >= animation_speed * 4)
                                        {
                                            ClearDamageEffects();
                                            effect_frame = 0;
                                            combat_state = "AnimateStatusDamageLabels";
                                        }
                                        else if (effect_frame % animation_speed == 0)
                                        {
                                            AnimateDamageEffects();
                                            effect_frame += Main.CombatSpeed;
                                        }
                                        else
                                        {
                                            effect_frame += Main.CombatSpeed;
                                        }
                                    }
                                    else
                                    {
                                        combat_state = "AnimateStatusDamageLabels";
                                    }

                                    #endregion
                                    break;

                                case "AnimateStatusDamageLabels":
                                    #region AnimateStatusDamageLabels

                                    if (DamageLabelExists())
                                    {
                                        if (effect_frame % label_speed == 0)
                                        {
                                            AnimateDamageLabels();
                                            effect_frame += Main.CombatSpeed;
                                        }
                                        else
                                        {
                                            effect_frame += Main.CombatSpeed;
                                        }
                                    }
                                    else
                                    {
                                        effect_frame = 0;

                                        if (skip_turn)
                                        {
                                            combat_state = "Finish";
                                        }
                                        else
                                        {
                                            combat_state = "GetTargets";
                                        }
                                    }

                                    #endregion
                                    break;

                                case "GetTargets":
                                    #region GetTargets

                                    bool can_attack = true;

                                    if (current_character.Dead)
                                    {
                                        can_attack = false;
                                    }

                                    int epCost = InventoryUtil.Get_EP_Cost(current_character);
                                    if (epCost > 0 &&
                                        current_character.ManaBar.Value < epCost)
                                    {
                                        can_attack = false;
                                    }

                                    if (can_attack)
                                    {
                                        if (counter_attacking)
                                        {
                                            targets.Add(initial_attacker);
                                        }
                                        else
                                        {
                                            targets = CombatUtil.GetTargets(current_character, ally_squad, enemy_squad);
                                            if (!targets.Any())
                                            {
                                                can_attack = false;
                                                FinishCombat();
                                            }
                                        }
                                    }

                                    if (can_attack)
                                    {
                                        attack_type = CharacterUtil.AttackType(current_character);
                                        CombatUtil.SwitchAnimation(current_character, attack_type);
                                        InventoryUtil.Get_EquippedItem(current_character, "Eyes").Visible = true;

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
                                    else
                                    {
                                        combat_state = "Finish";
                                    }

                                    #endregion
                                    break;

                                case "MoveForward":
                                    #region MoveForward

                                    if (CombatUtil.AtTile(current_character, target_tile, move_speed))
                                    {
                                        combat_state = "Attack";
                                    }
                                    else
                                    {
                                        CombatUtil.MoveForward(current_character, move_speed);
                                    }

                                    #endregion
                                    break;

                                case "Attack":
                                    #region Attack

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
                                            combat_state = "AnimateDamageEffects";
                                        }

                                        character_frame += Main.CombatSpeed;
                                    }
                                    else
                                    {
                                        character_frame += Main.CombatSpeed;
                                    }

                                    #endregion
                                    break;

                                case "AnimateDamageEffects":
                                    #region AnimateDamageEffects

                                    if (target_squad == null)
                                    {
                                        ClearDamageEffects();
                                        effect_frame = 0;
                                        combat_state = "AnimateDamageLabels";
                                    }
                                    else
                                    {
                                        if (DamageEffectExists())
                                        {
                                            if (effect_frame % 8 == 0)
                                            {
                                                AnimateDamageShake();
                                            }

                                            if (effect_frame >= animation_speed * 4)
                                            {
                                                bool enemies_ready = CombatUtil.SquadReady(World, enemy_squad, move_speed);
                                                bool allies_ready = CombatUtil.SquadReady(World, ally_squad, move_speed);

                                                if (enemies_ready &&
                                                    allies_ready)
                                                {
                                                    ClearDamageShake();
                                                    ClearDamageEffects();
                                                    effect_frame = 0;
                                                    combat_state = "AnimateDamageLabels";
                                                }
                                            }
                                            else if (effect_frame % animation_speed == 0)
                                            {
                                                AnimateDamageEffects();
                                                effect_frame += Main.CombatSpeed;
                                            }
                                            else
                                            {
                                                effect_frame += Main.CombatSpeed;
                                            }
                                        }
                                        else
                                        {
                                            bool enemies_ready = CombatUtil.SquadReady(World, enemy_squad, move_speed);
                                            bool allies_ready = CombatUtil.SquadReady(World, ally_squad, move_speed);

                                            if (enemies_ready &&
                                                allies_ready)
                                            {
                                                ClearDamageShake();
                                                effect_frame = 0;
                                                combat_state = "AnimateDamageLabels";
                                            }
                                        }
                                    }

                                    #endregion
                                    break;

                                case "AnimateDamageLabels":
                                    #region AnimateDamageLabels

                                    if (target_squad == null)
                                    {
                                        ClearDamageLabels();
                                        effect_frame = 0;
                                        combat_state = "EndAttack";
                                    }
                                    else
                                    {
                                        if (DamageLabelExists())
                                        {
                                            if (effect_frame % label_speed == 0)
                                            {
                                                AnimateDamageLabels();
                                                effect_frame += Main.CombatSpeed;
                                            }
                                            else
                                            {
                                                effect_frame += Main.CombatSpeed;
                                            }
                                        }
                                        else
                                        {
                                            effect_frame = 0;
                                            combat_state = "EndAttack";
                                        }
                                    }

                                    #endregion
                                    break;

                                case "EndAttack":
                                    #region EndAttack

                                    RemoveDamageStatusEffects();

                                    if (target_squad == null)
                                    {
                                        combat_state = "MoveBackward";
                                    }
                                    else
                                    {
                                        if (!counter_attacking &&
                                            counter_attackers.Any())
                                        {
                                            //Save original attacker for when counters are finished
                                            initial_attacker = current_character;

                                            ResetCombat();
                                            current_character = counter_attackers[0];
                                            counter_attackers.Remove(current_character);
                                            counter_attacking = true;
                                        }
                                        else if (!counter_attacking &&
                                                 RuneUtil.Time_AttackChance(Menu, current_character))
                                        {
                                            ResetCombat();
                                        }
                                        else
                                        {
                                            combat_state = "MoveBackward";
                                        }
                                    }

                                    #endregion
                                    break;

                                case "MoveBackward":
                                    #region MoveBackward

                                    if (CombatUtil.AtTile(current_character, origin_tile, move_speed))
                                    {
                                        combat_state = "Finish";
                                    }
                                    else
                                    {
                                        CombatUtil.MoveBack(current_character, move_speed);
                                    }

                                    #endregion
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

                    step = false;
                }
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
                            if (character != null)
                            {
                                int epCost = InventoryUtil.Get_EP_Cost(character);

                                if (character.CombatStep == 0 &&
                                    character.ManaBar.Value >= epCost &&
                                    !character.Dead)
                                {
                                    current_character = character;
                                    break;
                                }
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
                            if (character != null)
                            {
                                int epCost = InventoryUtil.Get_EP_Cost(character);

                                if (character.CombatStep == 0 &&
                                    character.ManaBar.Value >= epCost &&
                                    !character.Dead)
                                {
                                    current_character = character;
                                    break;
                                }
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
                        Color.White, true);
            }
            else if (current_character.Type == "Enemy")
            {
                Menu.AddPicture(Handler.GetID(), "Cast", AssetManager.Textures["EvilCast"],
                    new Region(current_character.Region.X, current_character.Region.Y, current_character.Region.Width, current_character.Region.Height),
                        Color.White, true);
            }

            Picture cast = Menu.GetPicture("Cast");
            cast.Image = new Rectangle(0, 0, cast.Texture.Width / 4, cast.Texture.Height);
        }

        private void Attack()
        {
            if (current_character != null)
            {
                Item weapon = InventoryUtil.Get_EquippedItem(current_character, "Weapon");

                Offense(weapon);
                Defense(weapon);

                int epCost = InventoryUtil.Get_EP_Cost(current_character);
                current_character.ManaBar.Value -= epCost;
                if (current_character.ManaBar.Value < 0)
                {
                    current_character.ManaBar.Value = 0;
                }
                current_character.ManaBar.Update();
            }
        }

        private void Defense(Item weapon)
        {
            if (InventoryUtil.Item_HasArea_ForElement(weapon, "Health") &&
                RuneUtil.ApplyArea(weapon, "Health"))
            {
                //Full party heal HP
                int hp = RuneUtil.Area_PairedLevel(weapon, "Health");
                if (current_character.Type == "Enemy")
                {
                    foreach (Character character in enemy_squad.Characters)
                    {
                        RuneUtil.Health(Menu, character, weapon, hp);
                        CombatUtil.CureStatusEffects(Menu, character);
                        RuneUtil.StatusEffect(Menu, character, weapon, "Health", true);
                    }
                }
                else
                {
                    foreach (Character character in ally_squad.Characters)
                    {
                        RuneUtil.Health(Menu, character, weapon, hp);
                        CombatUtil.CureStatusEffects(Menu, character);
                        RuneUtil.StatusEffect(Menu, character, weapon, "Health", true);
                    }
                }
            }
            else if (InventoryUtil.Item_HasElement(weapon, "Health"))
            {
                //Single target heal HP
                Character target;

                //Heal least HP
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
                    RuneUtil.Health(Menu, target, weapon, hp);
                    CombatUtil.CureStatusEffects(Menu, target);
                    RuneUtil.StatusEffect(Menu, target, weapon, "Health", true);
                }
            }

            if (InventoryUtil.Item_HasArea_ForElement(weapon, "Energy") &&
                RuneUtil.ApplyArea(weapon, "Energy"))
            {
                //Full party heal EP
                int ep = RuneUtil.Area_PairedLevel(weapon, "Energy");
                if (current_character.Type == "Enemy")
                {
                    foreach (Character character in enemy_squad.Characters)
                    {
                        RuneUtil.Energy(Menu, character, weapon, ep);
                        RuneUtil.StatusEffect(Menu, character, weapon, "Energy", true);
                    }
                }
                else
                {
                    foreach (Character character in ally_squad.Characters)
                    {
                        RuneUtil.Energy(Menu, character, weapon, ep);
                        RuneUtil.StatusEffect(Menu, character, weapon, "Energy", true);
                    }
                }
            }
            else if (InventoryUtil.Item_HasElement(weapon, "Energy"))
            {
                //Single target heal EP
                Character target;

                //Heal least EP
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
                    RuneUtil.Energy(Menu, target, weapon, ep);
                    RuneUtil.StatusEffect(Menu, target, weapon, "Energy", true);
                }
            }
        }

        private void Offense(Item weapon)
        {
            if (weapon != null)
            {
                Squad defender_squad = null;

                #region Initial Attack

                //Handle initial weapon damage
                foreach (Character target in targets)
                {
                    if (defender_squad == null)
                    {
                        defender_squad = ArmyUtil.Get_Squad(target.ID);
                    }

                    if (RuneUtil.Time_DodgeChance(target, defender_squad))
                    {
                        RuneUtil.Time_Dodge(Menu, target);
                    }
                    else
                    {
                        RuneUtil.Death(Menu, current_character, target, weapon);
                        if (!target.Dead)
                        {
                            RuneUtil.DisarmWeapon(Menu, current_character, target, weapon);

                            string[] elements = { "Earth", "Fire", "Physical", "Ice", "Lightning" };

                            for (int i = 0; i < elements.Length; i++)
                            {
                                string element = elements[i];

                                if (InventoryUtil.Item_HasElement(weapon, element))
                                {
                                    bool can_dodge = false;
                                    bool dodged = false;

                                    //Can only dodge melee/ranged physical attacks
                                    if ((InventoryUtil.Weapon_IsMelee(weapon) || weapon.Categories[0] == "Bow") &&
                                        element == "Physical")
                                    {
                                        can_dodge = true;
                                    }

                                    if (can_dodge)
                                    {
                                        dodged = CombatUtil.Dodge(Menu, current_character, target);
                                    }

                                    if (!dodged)
                                    {
                                        CombatUtil.DoDamage_ForElement(Menu, current_character, target, weapon, element, ref ally_total_damage, ref enemy_total_damage);
                                    }
                                }
                            }

                            if (InventoryUtil.Item_HasElement(weapon, "Time"))
                            {
                                RuneUtil.StatusEffect(Menu, target, weapon, "Time", false);
                            }

                            if (target.Dead)
                            {
                                if (target.Type == "Enemy")
                                {
                                    Reward_CharacterKilled();
                                }
                                else if (target.ID == Handler.MainCharacter_ID)
                                {
                                    hero_killed = true;
                                    break;
                                }
                                else if (target.Type == "Ally")
                                {
                                    foreach (Character character in enemy_squad.Characters)
                                    {
                                        CharacterUtil.Increase_XP(character, xp_base);
                                        CombatUtil.Gain_RP(character, rp_base);
                                    }
                                }
                            }
                            else if (!counter_attacking &&
                                     RuneUtil.CounterArmor(Menu, target))
                            {
                                counter_attackers.Add(target);
                            }
                        }
                    }
                }

                #endregion

                if (!hero_killed &&
                    defender_squad != null &&
                    weapon != null)
                {
                    #region Area Death

                    //Handle weapon Area Death
                    if (InventoryUtil.Item_HasArea_ForElement(weapon, "Death") &&
                        RuneUtil.ApplyArea(weapon, "Death"))
                    {
                        foreach (Character target in defender_squad.Characters)
                        {
                            //Ignore initial targets
                            if (!targets.Contains(target))
                            {
                                RuneUtil.Death(Menu, current_character, target, weapon);

                                if (target.Dead)
                                {
                                    if (target.Type == "Enemy")
                                    {
                                        Reward_CharacterKilled();
                                    }
                                    else if (target.ID == Handler.MainCharacter_ID)
                                    {
                                        hero_killed = true;
                                        break;
                                    }
                                    else if (target.Type == "Ally")
                                    {
                                        foreach (Character character in enemy_squad.Characters)
                                        {
                                            CharacterUtil.Increase_XP(character, xp_base);
                                            CombatUtil.Gain_RP(character, rp_base);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region Area Disarm

                    //Handle weapon Area Disarm
                    if (InventoryUtil.Item_HasArea_ForElement(weapon, "Disarm") &&
                        RuneUtil.ApplyArea(weapon, "Disarm"))
                    {
                        foreach (Character target in defender_squad.Characters)
                        {
                            //Ignore initial targets or dead ones
                            if (!targets.Contains(target) &&
                                !target.Dead)
                            {
                                RuneUtil.DisarmWeapon(Menu, current_character, target, weapon);
                            }
                        }
                    }

                    #endregion

                    #region Area Elements

                    //Handle weapon Area elements
                    if (!hero_killed)
                    {
                        foreach (Character target in defender_squad.Characters)
                        {
                            //Ignore initial targets or dead ones
                            if (!targets.Contains(target) &&
                                !target.Dead)
                            {
                                if (RuneUtil.Time_DodgeChance(target, defender_squad))
                                {
                                    RuneUtil.Time_Dodge(Menu, target);
                                }
                                else
                                {
                                    string[] elements = { "Physical", "Fire", "Lightning", "Earth", "Ice" };

                                    for (int i = 0; i < elements.Length; i++)
                                    {
                                        string element = elements[i];

                                        //Only do damage for paired Area runes
                                        if (InventoryUtil.Item_HasArea_ForElement(weapon, element) &&
                                            RuneUtil.ApplyArea(weapon, element))
                                        {
                                            CombatUtil.DoDamage_ForElement(Menu, current_character, target, weapon, element, ref ally_total_damage, ref enemy_total_damage);

                                            if (target.Dead)
                                            {
                                                if (target.Type == "Enemy")
                                                {
                                                    Reward_CharacterKilled();
                                                }
                                                else if (target.ID == Handler.MainCharacter_ID)
                                                {
                                                    hero_killed = true;
                                                    break;
                                                }
                                                else if (target.Type == "Ally")
                                                {
                                                    foreach (Character character in enemy_squad.Characters)
                                                    {
                                                        CharacterUtil.Increase_XP(character, xp_base);
                                                        CombatUtil.Gain_RP(character, rp_base);
                                                    }
                                                }
                                            }
                                            else if (!counter_attacking &&
                                                     RuneUtil.CounterArmor(Menu, target))
                                            {
                                                counter_attackers.Add(target);
                                            }
                                        }

                                        if (hero_killed)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }

                            if (hero_killed)
                            {
                                break;
                            }
                        }
                    }

                    #endregion
                }
            }
            else
            {
                #region Unarmed Attack

                foreach (Character target in targets)
                {
                    if (!CombatUtil.Dodge(Menu, current_character, target))
                    {
                        CombatUtil.DoDamage_Unarmed(Menu, target, ref ally_total_damage, ref enemy_total_damage);

                        if (target.Dead)
                        {
                            if (target.Type == "Enemy")
                            {
                                Reward_CharacterKilled();
                            }
                            else if (target.ID == Handler.MainCharacter_ID)
                            {
                                hero_killed = true;
                                break;
                            }
                            else if (target.Type == "Ally")
                            {
                                foreach (Character character in enemy_squad.Characters)
                                {
                                    CharacterUtil.Increase_XP(character, xp_base);
                                    CombatUtil.Gain_RP(character, rp_base);
                                }
                            }
                        }
                        else if (!counter_attacking &&
                                 RuneUtil.CounterArmor(Menu, target))
                        {
                            counter_attackers.Add(target);
                        }
                    }
                }

                #endregion
            }

            if (hero_killed)
            {
                MainCharacterKilled();
            }
        }

        private bool DamageEffectExists()
        {
            for (int i = 0; i < Menu.Pictures.Count; i++)
            {
                Picture picture = Menu.Pictures[i];
                if (picture.Name == "Damage")
                {
                    return true;
                }
            }

            return false;
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

        private void ClearDamageEffects()
        {
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

            foreach (Character character in targets)
            {
                Tile origin_tile = CombatUtil.OriginTile(World, character);
                if (character.Region.X != origin_tile.Region.X)
                {
                    character.Region = new Region(origin_tile.Region.X, character.Region.Y, character.Region.Width, character.Region.Height);
                    CharacterUtil.UpdateGear(character);
                }
            }
        }

        private bool DamageLabelExists()
        {
            for (int i = 0; i < Menu.Labels.Count; i++)
            {
                Label label = Menu.Labels[i];
                if (label.Name == "Damage")
                {
                    return true;
                }
            }

            return false;
        }

        private void AnimateDamageLabels()
        {
            if (ally_squad != null)
            {
                for (int i = 0; i < ally_squad.Characters.Count; i++)
                {
                    Character character = ally_squad.Characters[i];

                    for (int d = 0; d < Menu.Labels.Count; d++)
                    {
                        Label label = Menu.Labels[d];
                        if (label.Name == "Damage" &&
                            label.ID == character.ID &&
                            label.Region != null)
                        {
                            label.Visible = true;
                            label.Region.Y -= 1;
                            label.Opacity -= 0.05f;

                            if (label.Opacity <= 0)
                            {
                                Menu.Labels.Remove(label);
                            }

                            break;
                        }
                    }
                }
            }

            if (enemy_squad != null)
            {
                for (int i = 0; i < enemy_squad.Characters.Count; i++)
                {
                    Character character = enemy_squad.Characters[i];

                    for (int d = 0; d < Menu.Labels.Count; d++)
                    {
                        Label label = Menu.Labels[d];
                        if (label.Name == "Damage" &&
                            label.ID == character.ID &&
                            label.Region != null)
                        {
                            label.Visible = true;
                            label.Region.Y -= 1;
                            label.Opacity -= 0.05f;

                            if (label.Opacity <= 0)
                            {
                                Menu.Labels.Remove(label);
                            }

                            break;
                        }
                    }
                }
            }
        }

        private void ClearDamageLabels()
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

        private void AnimateDamageShake()
        {
            foreach (Character character in targets)
            {
                Something damage = character.GetStatusEffect("Damage");
                if (damage != null)
                {
                    Label damage_label = Menu.GetLabel(damage.ID);
                    if (damage_label != null)
                    {
                        bool shakeFound = false;
                        for (int i = 0; i < character.Tags.Count; i++)
                        {
                            if (character.Tags[i].Contains("Shake"))
                            {
                                shakeFound = true;
                                break;
                            }
                        }

                        if (!shakeFound)
                        {
                            character.Tags.Add("Shake1");
                        }
                        else if (character.Tags.Contains("Shake1"))
                        {
                            character.Tags.Remove("Shake1");
                            character.Tags.Add("Shake2");
                        }
                        else if (character.Tags.Contains("Shake2"))
                        {
                            character.Tags.Remove("Shake2");
                            character.Tags.Add("Shake3");
                        }
                        else if (character.Tags.Contains("Shake3"))
                        {
                            character.Tags.Remove("Shake3");
                            character.Tags.Add("Shake4");
                        }
                    }
                }
            }
        }

        private void ClearDamageShake()
        {
            foreach (Character character in enemy_squad.Characters)
            {
                for (int i = 0; i < character.Tags.Count; i++)
                {
                    if (character.Tags[i].Contains("Shake"))
                    {
                        character.Tags.Remove(character.Tags[i]);
                        i--;
                    }
                }
            }

            foreach (Character character in ally_squad.Characters)
            {
                for (int i = 0; i < character.Tags.Count; i++)
                {
                    if (character.Tags[i].Contains("Shake"))
                    {
                        character.Tags.Remove(character.Tags[i]);
                        i--;
                    }
                }
            }
        }

        private void AnimateCharacters()
        {
            if (!Handler.CombatPause)
            {
                if (ally_squad != null)
                {
                    for (int i = 0; i < ally_squad.Characters.Count; i++)
                    {
                        Character character = ally_squad.Characters[i];
                        if (!character.Dead &&
                            character.Tags.Contains("Animation_Idle"))
                        {
                            CharacterUtil.AnimateIdle(character);
                        }
                    }
                }

                if (enemy_squad != null)
                {
                    for (int i = 0; i < enemy_squad.Characters.Count; i++)
                    {
                        Character character = enemy_squad.Characters[i];
                        if (!character.Dead &&
                            character.Tags.Contains("Animation_Idle"))
                        {
                            CharacterUtil.AnimateIdle(character);
                        }
                    }
                }

                for (int i = 0; i < targets.Count; i++)
                {
                    Character character = targets[i];
                    if (!character.Dead)
                    {
                        Tile origin_tile = CombatUtil.OriginTile(World, character);
                        Tile target_tile = CombatUtil.TargetTile(World, character);

                        float speed = 1;

                        CryptoRandom random = new CryptoRandom();
                        int choice = random.Next(1, 5);
                        switch (choice)
                        {
                            case 1:
                                speed = 2;
                                break;

                            case 2:
                                speed = 4;
                                break;

                            case 3:
                                speed = 8;
                                break;

                            case 4:
                                speed = 12;
                                break;
                        }

                        if (character.Tags.Contains("Shake1"))
                        {
                            float x = origin_tile.Region.X;
                            float distance = origin_tile.Region.Width / 4;

                            if (character.Region.X >= origin_tile.Region.X - origin_tile.Region.Width &&
                                character.Region.X <= origin_tile.Region.X + origin_tile.Region.Width)
                            {
                                x = origin_tile.Region.X;
                            }
                            else if (character.Region.X >= target_tile.Region.X - target_tile.Region.Width &&
                                     character.Region.X <= target_tile.Region.X + target_tile.Region.Width)
                            {
                                x = target_tile.Region.X;
                            }

                            if (character.Type == "Ally")
                            {
                                x += distance;
                                if (character.Region.X < x)
                                {
                                    CombatUtil.MoveBack(character, distance / speed);
                                }
                            }
                            else if (character.Type == "Enemy")
                            {
                                x -= distance;
                                if (character.Region.X > x)
                                {
                                    CombatUtil.MoveBack(character, distance / speed);
                                }
                            }
                        }
                        else if (character.Tags.Contains("Shake2"))
                        {
                            float x = origin_tile.Region.X;
                            float distance = origin_tile.Region.Width / 4;

                            if (character.Region.X >= origin_tile.Region.X - origin_tile.Region.Width &&
                                character.Region.X <= origin_tile.Region.X + origin_tile.Region.Width)
                            {
                                x = origin_tile.Region.X;
                            }
                            else if (character.Region.X >= target_tile.Region.X - target_tile.Region.Width &&
                                     character.Region.X <= target_tile.Region.X + target_tile.Region.Width)
                            {
                                x = target_tile.Region.X;
                            }

                            if (character.Type == "Ally")
                            {
                                x -= distance;
                                if (character.Region.X > x)
                                {
                                    CombatUtil.MoveForward(character, distance / speed);
                                }
                            }
                            else if (character.Type == "Enemy")
                            {
                                x += distance;
                                if (character.Region.X < x)
                                {
                                    CombatUtil.MoveForward(character, distance / speed);
                                }
                            }
                        }
                        else if (character.Tags.Contains("Shake3"))
                        {
                            float x = origin_tile.Region.X;
                            float distance = origin_tile.Region.Width / 8;

                            if (character.Region.X >= origin_tile.Region.X - origin_tile.Region.Width &&
                                character.Region.X <= origin_tile.Region.X + origin_tile.Region.Width)
                            {
                                x = origin_tile.Region.X;
                            }
                            else if (character.Region.X >= target_tile.Region.X - target_tile.Region.Width &&
                                     character.Region.X <= target_tile.Region.X + target_tile.Region.Width)
                            {
                                x = target_tile.Region.X;
                            }

                            if (character.Type == "Ally")
                            {
                                x += distance;
                                if (character.Region.X < x)
                                {
                                    CombatUtil.MoveBack(character, distance / speed);
                                }
                            }
                            else if (character.Type == "Enemy")
                            {
                                x -= distance;
                                if (character.Region.X > x)
                                {
                                    CombatUtil.MoveBack(character, distance / speed);
                                }
                            }
                        }
                        else if (character.Tags.Contains("Shake4"))
                        {
                            float distance = 0;

                            if (character.Region.X >= origin_tile.Region.X - origin_tile.Region.Width &&
                                character.Region.X <= origin_tile.Region.X + origin_tile.Region.Width)
                            {
                                if (character.Type == "Ally")
                                {
                                    distance = character.Region.X - origin_tile.Region.X;
                                }
                                else if (character.Type == "Enemy")
                                {
                                    distance = origin_tile.Region.X - character.Region.X;
                                }

                                if (character.Region.X != origin_tile.Region.X)
                                {
                                    CombatUtil.MoveForward(character, distance / (speed / 2));
                                }
                            }
                            else if (character.Region.X >= target_tile.Region.X - target_tile.Region.Width &&
                                     character.Region.X <= target_tile.Region.X + target_tile.Region.Width)
                            {
                                if (character.Type == "Ally")
                                {
                                    distance = character.Region.X - target_tile.Region.X;
                                }
                                else if (character.Type == "Enemy")
                                {
                                    distance = target_tile.Region.X - character.Region.X;
                                }

                                if (character.Region.X != target_tile.Region.X)
                                {
                                    CombatUtil.MoveForward(character, distance / (speed / 2));
                                }
                            }
                        }
                    }
                }
            }
        }

        private void RemoveDamageStatusEffects()
        {
            if (ally_squad != null)
            {
                for (int i = 0; i < ally_squad.Characters.Count; i++)
                {
                    Character character = ally_squad.Characters[i];

                    for (int s = 0; s < character.StatusEffects.Count; s++)
                    {
                        Something effect = character.StatusEffects[s];
                        if (effect.Name == "Damage")
                        {
                            character.StatusEffects.Remove(effect);
                            s--;
                        }
                    }
                }
            }

            if (enemy_squad != null)
            {
                for (int i = 0; i < enemy_squad.Characters.Count; i++)
                {
                    Character character = enemy_squad.Characters[i];

                    for (int s = 0; s < character.StatusEffects.Count; s++)
                    {
                        Something effect = character.StatusEffects[s];
                        if (effect.Name == "Damage")
                        {
                            character.StatusEffects.Remove(effect);
                            s--;
                        }
                    }
                }
            }
        }

        private void FinishAttack()
        {
            UpdateGrids();

            if (current_character != null)
            {
                CombatUtil.SwitchAnimation(current_character, "Idle");

                if (ally_squad != null)
                {
                    for (int i = 0; i < ally_squad.Characters.Count; i++)
                    {
                        Character character = ally_squad.Characters[i];
                        if (character.Dead)
                        {
                            CombatUtil.Kill(character);
                            i--;
                        }
                    }

                    if (!counter_attacking)
                    {
                        foreach (Character character in ally_squad.Characters)
                        {
                            if (current_character != null &&
                                character.ID == current_character.ID)
                            {
                                character.CombatStep = 1;
                                break;
                            }
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
                            CombatUtil.Kill(character);
                            i--;
                        }
                    }

                    if (!counter_attacking)
                    {
                        foreach (Character character in enemy_squad.Characters)
                        {
                            if (current_character != null &&
                                character.ID == current_character.ID)
                            {
                                character.CombatStep = 1;
                                break;
                            }
                        }
                    }
                }
            }

            ResetCombat();

            if (counter_attacking)
            {
                if (counter_attackers.Any())
                {
                    current_character = counter_attackers[0];
                    counter_attackers.Remove(current_character);
                }
                else
                {
                    counter_attacking = false;
                    current_character = initial_attacker;
                    combat_state = "EndAttack";
                }
            }
        }

        private void ResetCombat()
        {
            skip_turn = false;
            combat_state = "StatusEffects";
            character_frame = 0;
            effect_frame = 0;
            attack_type = "";
            targets.Clear();
            current_character = null;
        }

        private void ResetCombat_Final()
        {
            ResetCombat();
            counter_attackers.Clear();
            gold = 0;
            xp = 0;
            rp = 0;
            won_battle = false;
            counter_attacking = false;
            ally_total_damage = 0;
            enemy_total_damage = 0;

            if (ally_squad != null)
            {
                foreach (Character character in ally_squad.Characters)
                {
                    character.Tags.Clear();
                }
            }

            if (enemy_squad != null)
            {
                foreach (Character character in enemy_squad.Characters)
                {
                    character.Tags.Clear();
                }
            }
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

            if (current_character == null)
            {
                FinishCombat();
            }
        }

        private void FinishCombat()
        {
            Handler.CombatFinishing = true;
            SoundManager.AmbientPaused = true;
            Handler.CombatTimer.Stop();

            if (!ally_squad.Characters.Any())
            {
                CharacterManager.GetArmy("Ally").Squads.Remove(ally_squad);
            }
            else
            {
                foreach (Character character in ally_squad.Characters)
                {
                    for (int i = 0; i < character.StatusEffects.Count; i++)
                    {
                        Something statusEffect = character.StatusEffects[i];

                        if (statusEffect.Name == "Weak" ||
                            statusEffect.Name == "Melting" ||
                            statusEffect.Name == "Burning" ||
                            statusEffect.Name == "Regenerating" ||
                            statusEffect.Name == "Charging" ||
                            statusEffect.Name == "Stunned" ||
                            statusEffect.Name == "Slow" ||
                            statusEffect.Name == "Frozen" ||
                            statusEffect.Name == "Shocked")
                        {
                            character.StatusEffects.Remove(statusEffect);
                            i--;
                        }
                    }
                }
            }

            if (!enemy_squad.Characters.Any())
            {
                CharacterManager.GetArmy("Enemy").Squads.Remove(enemy_squad);
            }
            else
            {
                bool rest_needed = false;

                foreach (Character character in enemy_squad.Characters)
                {
                    int epCost = InventoryUtil.Get_EP_Cost(character);
                    if (character.ManaBar.Value < epCost)
                    {
                        rest_needed = true;
                    }

                    for (int i = 0; i < character.StatusEffects.Count; i++)
                    {
                        Something statusEffect = character.StatusEffects[i];

                        if (statusEffect.Name == "Weak" ||
                            statusEffect.Name == "Melting" ||
                            statusEffect.Name == "Burning" ||
                            statusEffect.Name == "Regenerating" ||
                            statusEffect.Name == "Charging" ||
                            statusEffect.Name == "Stunned" ||
                            statusEffect.Name == "Slow" ||
                            statusEffect.Name == "Frozen" ||
                            statusEffect.Name == "Shocked")
                        {
                            character.StatusEffects.Remove(statusEffect);
                            i--;
                        }
                    }
                }

                if (rest_needed)
                {
                    enemy_squad.Assignment = "Rest";

                    Army army = CharacterManager.GetArmy("Enemy");
                    Scene localmap = SceneManager.GetScene("Localmap");
                    Map map = localmap.World.Maps[Handler.Level];
                    Layer ground = map.GetLayer("Ground");

                    AI_Util.Set_NextTarget(map, ground, army, enemy_squad);
                }
            }

            Label label = Menu.GetLabel("Result");
            label.Visible = true;

            string text;

            if (!enemy_squad.Characters.Any())
            {
                won_battle = true;
                Menu.GetPicture("Result").Texture = AssetManager.Textures["Victory"];
                text = ally_squad.Name + " was victorious!";
            }
            else if (!ally_squad.Characters.Any())
            {
                won_battle = false;
                Menu.GetPicture("Result").Texture = AssetManager.Textures["Defeat"];
                text = ally_squad.Name + " was defeated!";
            }
            else if (enemy_total_damage > ally_total_damage)
            {
                won_battle = false;
                Menu.GetPicture("Result").Texture = AssetManager.Textures["Defeat"];
                text = ally_squad.Name + " was defeated!\n\n" +
                    ally_squad.Name + " Total Damage: " + ally_total_damage + "\n" +
                    enemy_squad.Name + " Total Damage: " + enemy_total_damage;
            }
            else if (ally_total_damage > enemy_total_damage)
            {
                won_battle = true;
                Menu.GetPicture("Result").Texture = AssetManager.Textures["Victory"];
                text = ally_squad.Name + " was victorious!\n\n" +
                    ally_squad.Name + " Total Damage: " + ally_total_damage + "\n" +
                    enemy_squad.Name + " Total Damage: " + enemy_total_damage;
            }
            else
            {
                won_battle = false;
                Menu.GetPicture("Result").Texture = AssetManager.Textures["Draw"];
                text = "Both parties are retreating.";
            }

            Menu.GetButton("Retreat").Visible = false;
            Menu.GetButton("PlayPause").Visible = false;
            Menu.GetButton("Speed").Visible = false;

            Menu.GetPicture("Result").Visible = true;
            Menu.GetButton("Result").Visible = true;

            if (gold > 0 ||
                xp > 0)
            {
                text = Reward_Combat(text);
            }

            label.Text = GameUtil.WrapText_Dialogue(text);

            if (ally_squad.Characters.Any())
            {
                Character leader = ally_squad.GetLeader();
                if (won_battle &&
                    !enemy_squad.Characters.Any())
                {
                    //If leader not killed and no enemies left, remove enemy as target
                    leader.Target_ID = 0;
                }
            }
        }

        private void Retreat()
        {
            Handler.Retreating = true;
            Handler.CombatFinishing = true;
            SoundManager.AmbientPaused = true;
            Handler.CombatTimer.Stop();

            Picture battleResult = Menu.GetPicture("Result");
            battleResult.Texture = AssetManager.Textures["Defeat"];
            battleResult.Visible = true;

            Label label = Menu.GetLabel("Result");
            label.Visible = true;

            string text = "    " + ally_squad.Name + " is retreating...    ";
            
            Menu.GetButton("Result").Visible = true;
            Menu.GetButton("Retreat").Visible = false;

            if (gold > 0 ||
                xp > 0)
            {
                text = Reward_Combat(text);
            }
            else
            {
                label.Margin = 20;
            }

            label.Text = GameUtil.WrapText_Dialogue(text);
        }

        private void MainCharacterKilled()
        {
            SoundManager.AmbientPaused = true;
            Handler.CombatTimer.Stop();

            Picture battleResult = Menu.GetPicture("Result");
            battleResult.Texture = AssetManager.Textures["Defeat"];
            battleResult.Visible = true;

            Label label = Menu.GetLabel("Result");
            label.Text = GameUtil.WrapText_Dialogue(ally_squad.Name + " has been slain!\n\nThe story cannot continue without its hero...");
            label.Visible = true;

            Menu.GetButton("Result").Visible = true;
            Menu.GetButton("Retreat").Visible = false;
            Menu.GetButton("PlayPause").Visible = false;
            Menu.GetButton("Speed").Visible = false;
        }

        private void Reward_CharacterKilled()
        {
            gold += 200;
            xp += 2;
            rp++;
        }

        private string Reward_Combat(string text)
        {
            text += "\n\n" + gold + " Gold looted!";
            text += "\n" + xp + " XP and " + rp + " RP gained!";

            Handler.Gold += gold;

            foreach (Character character in ally_squad.Characters)
            {
                int levels_gained = CharacterUtil.Increase_XP(character, xp);
                if (levels_gained > 0)
                {
                    text += "\n" + character.Name + " is now Level " + character.Level + "!";
                }

                CombatUtil.Gain_RP(character, rp);
            }

            return text;
        }

        private void Leave()
        {
            if (!won_battle &&
                ally_squad.Characters.Any())
            {
                CombatUtil.BumpAllyAway(ally_squad, enemy_squad);
            }
            else if (won_battle &&
                     enemy_squad.Characters.Any())
            {
                CombatUtil.BumpEnemyAway(ally_squad, enemy_squad);
            }

            Handler.CombatTimer.Stop();
            Handler.Combat = false;
            Handler.CombatFinishing = false;

            ResetCombat_Final();

            Menu ui = MenuManager.GetMenu("UI");
            ui.Active = true;
            ui.Visible = true;

            WorldUtil.ResetMap_Combat(ally_squad);
            SceneManager.ChangeScene("Localmap");

            SoundManager.StopMusic();
            SoundManager.NeedMusic = true;

            Main.Timer.Start();

            if (!Handler.Retreating)
            {
                SoundManager.AmbientPaused = false;
                GameUtil.Toggle_Pause(false);
            }
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
            int width = (int)(Main.Game.MenuSize.X * 1.5f);
            int height = (int)(Main.Game.MenuSize.Y * 1.5f);
            int starting_x = Main.Game.MenuSize.X / 2;
            int starting_y = Main.Game.MenuSize.Y / 2;

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    Picture box = Menu.GetPicture("Enemy,x:" + x.ToString() + ",y:" + y.ToString());
                    box.Region = new Region(starting_x + (width * x), starting_y + (height * y), width, height);

                    Character character = enemy_squad.GetCharacter(new Vector2(x, y));
                    if (character != null)
                    {
                        Label name_label = Menu.GetLabel(character.ID + "_Name,x:" + x.ToString() + ",y:" + y.ToString());
                        if (name_label != null)
                        {
                            name_label.Region = new Region(box.Region.X, box.Region.Y + (height / 8), width, height / 4);
                        }

                        Label hp_label = Menu.GetLabel(character.ID + "_HP,x:" + x.ToString() + ",y:" + y.ToString());
                        if (hp_label != null)
                        {
                            hp_label.Region = new Region(box.Region.X, box.Region.Y + (height / 8) + (height / 4), width, height / 4);
                        }

                        Label ep_label = Menu.GetLabel(character.ID + "_EP,x:" + x.ToString() + ",y:" + y.ToString());
                        if (ep_label != null)
                        {
                            ep_label.Region = new Region(box.Region.X, box.Region.Y + (height / 8) + (height / 2), width, height / 4);
                        }
                    }
                }
            }

            starting_x = Main.Game.Resolution.X - (width * 3) - (Main.Game.MenuSize.X / 2);
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    Picture box = Menu.GetPicture("Ally,x:" + x.ToString() + ",y:" + y.ToString());
                    box.Region = new Region(starting_x + (width * x), starting_y + (height * y), width, height);

                    Character character = ally_squad.GetCharacter(new Vector2(x, y));
                    if (character != null)
                    {
                        Label name_label = Menu.GetLabel(character.ID + "_Name,x:" + x.ToString() + ",y:" + y.ToString());
                        if (name_label != null)
                        {
                            name_label.Region = new Region(box.Region.X, box.Region.Y + (height / 8), width, height / 4);
                        }

                        Label hp_label = Menu.GetLabel(character.ID + "_HP,x:" + x.ToString() + ",y:" + y.ToString());
                        if (hp_label != null)
                        {
                            hp_label.Region = new Region(box.Region.X, box.Region.Y + (height / 8) + (height / 4), width, height / 4);
                        }

                        Label ep_label = Menu.GetLabel(character.ID + "_EP,x:" + x.ToString() + ",y:" + y.ToString());
                        if (ep_label != null)
                        {
                            ep_label.Region = new Region(box.Region.X, box.Region.Y + (height / 8) + (height / 2), width, height / 4);
                        }
                    }
                }
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

            SaveUtil.ExportINI();

            effect_frame = 0;
        }

        public override void Load()
        {
            Menu.Clear();

            if (!string.IsNullOrEmpty(Handler.Combat_Terrain))
            {
                WorldGen.GenCombatMap();

                ally_squad = CharacterManager.GetArmy("Ally").GetSquad(Handler.Combat_Ally_Squad);

                enemy_squad = CharacterManager.GetArmy("Enemy").GetSquad(Handler.Combat_Enemy_Squad);
                if (enemy_squad == null)
                {
                    enemy_squad = CharacterManager.GetArmy("Special").GetSquad(Handler.Combat_Enemy_Squad);
                }

                foreach (Character character in ally_squad.Characters)
                {
                    CombatUtil.SwitchAnimation(character, "Idle");
                }

                foreach (Character character in enemy_squad.Characters)
                {
                    CombatUtil.SwitchAnimation(character, "Idle");
                }

                Color backdropColor = Color.White;

                if (Handler.Combat_Terrain == "Grass")
                {
                    Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Grass"], new Region(0, 0, 0, 0), backdropColor, true);
                }
                else if (Handler.Combat_Terrain == "Water")
                {
                    Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Water"], new Region(0, 0, 0, 0), backdropColor, true);
                }
                else if (Handler.Combat_Terrain == "Desert")
                {
                    Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Desert"], new Region(0, 0, 0, 0), backdropColor, true);
                }
                else if (Handler.Combat_Terrain == "Snow")
                {
                    Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Snow"], new Region(0, 0, 0, 0), backdropColor, true);
                }
                else if (Handler.Combat_Terrain == "Ice")
                {
                    Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Ice"], new Region(0, 0, 0, 0), backdropColor, true);
                }
                else if (Handler.Combat_Terrain.Contains("Forest"))
                {
                    if (Handler.Combat_Terrain.Contains("Snow"))
                    {
                        Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Forest_Snow"], new Region(0, 0, 0, 0), backdropColor, true);
                    }
                    else
                    {
                        Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Forest"], new Region(0, 0, 0, 0), backdropColor, true);
                    }
                }
                else if (Handler.Combat_Terrain.Contains("Mountains"))
                {
                    if (Handler.Combat_Terrain.Contains("Snow"))
                    {
                        Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Mountains_Snow"], new Region(0, 0, 0, 0), backdropColor, true);
                    }
                    else if (Handler.Combat_Terrain.Contains("Desert"))
                    {
                        Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Mountains_Desert"], new Region(0, 0, 0, 0), backdropColor, true);
                    }
                    else
                    {
                        Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Mountains"], new Region(0, 0, 0, 0), backdropColor, true);
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
                    texture_highlight = AssetManager.Textures["ButtonFrame"],
                    region = new Region(0, 0, 0, 0),
                    draw_color = Color.White,
                    draw_color_selected = Color.White,
                    text_color = Color.Black,
                    text_selected_color = Color.White,
                    enabled = true,
                    visible = true
                });

                Menu.AddPicture(Handler.GetID(), "Result", AssetManager.Textures["Victory"], new Region(0, 0, 0, 0), Color.White, false);

                Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Debug", "Debugging", Color.White, new Region(0, 0, 0, 0), Main.Game.Debugging);
                Menu.AddPicture(Handler.GetID(), "Highlight", AssetManager.Textures["Grid_Hover"], new Region(0, 0, 0, 0), Color.White, false);
                Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"],
                    new Region(0, 0, 0, 0), false);

                Menu.AddLabel(new LabelOptions
                {
                    id = Handler.GetID(),
                    font = AssetManager.Fonts["ControlFont"],
                    name = "Result",
                    texture = AssetManager.Textures["Frame_Text"],
                    text_color = new Color(99, 82, 71),
                    alignment_verticle = Alignment.Center,
                    alignment_horizontal = Alignment.Center,
                    region = new Region(0, 0, 0, 0),
                    draw_color = Color.White,
                    visible = false
                });

                Menu.AddButton(new ButtonOptions
                {
                    id = Handler.GetID(),
                    name = "Result",
                    text = "[Click here to continue]",
                    font = AssetManager.Fonts["ControlFont"],
                    texture = AssetManager.Textures["ButtonFrame_Wide"],
                    texture_highlight = AssetManager.Textures["ButtonFrame_Wide"],
                    region = new Region(0, 0, 0, 0),
                    draw_color = Color.White * 0.9f,
                    draw_color_selected = Color.White,
                    text_color = Color.Black,
                    text_selected_color = Color.White,
                    enabled = true,
                    visible = false
                });

                LoadGrids();

                Menu.Visible = true;

                Resize(Main.Game.Resolution);

                Handler.CombatTimer.Start();
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
                Menu.GetPicture("Background").Region = new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y);

                base_move_speed = tile.Region.Width / 8;
                move_speed = base_move_speed * (Main.CombatSpeed / 2);

                Menu.GetPicture("Result").Region = new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y);

                int Y = Main.Game.ScreenHeight - (height * 6);
                Label result_label = Menu.GetLabel("Result");
                result_label.Region = new Region((Main.Game.ScreenWidth / 2) - (height * 5), Y, height * 10, height * 4);

                Button result = Menu.GetButton("Result");
                result.Region = new Region(result_label.Region.X, result_label.Region.Y + result_label.Region.Height, result_label.Region.Width, (result_label.Region.Height / 6));

                Menu.GetButton("PlayPause").Region = new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize.X, Main.Game.MenuSize.Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);
                Menu.GetButton("Speed").Region = new Region(Main.Game.ScreenWidth / 2, Main.Game.MenuSize.Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);
                Menu.GetButton("Retreat").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize.X * 2), (Main.Game.MenuSize.Y * 3), Main.Game.MenuSize.X * 4, height);

                Menu.GetLabel("Debug").Region = new Region((Main.Game.Resolution.X / 2) - (Main.Game.MenuSize.X * 5), 0, Main.Game.MenuSize.X * 10, height);
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

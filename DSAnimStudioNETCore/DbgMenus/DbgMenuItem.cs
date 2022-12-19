using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DbgMenus
{
    public enum DbgMenuOpenState
    {
        Closed,
        Visible,
        Open
    }

    public class DbgMenuItem
    {
        public static void Init()
        {
            CurrentMenu.Text = "Main Menu";
            CurrentMenu.Items = new List<DbgMenuItem>()
            {
                new DbgMenuItem() { Text = "[DEBUG MENU MOSTLY SCRAPPED]"},
                new DbgMenuItem()
                {
                    Text = "[Graphics Settings]",
                    Items = new List<DbgMenuItem>
                    {
                        new DbgMenuItemEnum<LODMode>("LOD Mode", v => GFX.LODMode = v, () => GFX.LODMode,
                        nameOverrides: new Dictionary<LODMode, string>
                        {
                            { LODMode.ForceFullRes, "Force Full Resolution" },
                            { LODMode.ForceLOD1, "Force LOD Level 1" },
                            { LODMode.ForceLOD2, "Force LOD Level 2" },
                        }),
                        new DbgMenuItemNumber("LOD1 Distance", 0, 10000, 1,
                            (f) => GFX.LOD1Distance = f, () => GFX.LOD1Distance),
                        new DbgMenuItemNumber("LOD2 Distance", 0, 10000, 1,
                            (f) => GFX.LOD2Distance = f, () => GFX.LOD2Distance),
                        new DbgMenuItemBool("Show Debug Primitive Nametags", "YES", "NO",
                            (b) => DBG.ShowPrimitiveNametags = b, () => DBG.ShowPrimitiveNametags),
                        new DbgMenuItemBool("Simple Text Label Size", "YES", "NO",
                            (b) => DBG.SimpleTextLabelSize = b, () => DBG.SimpleTextLabelSize),
                        new DbgMenuItemNumber("Debug Primitive Nametag Size", 0.01f, 5.0f, 0.01f,
                            (v) => DBG.PrimitiveNametagSize = v, () => DBG.PrimitiveNametagSize),
                        new DbgMenuItemBool("Show Model Names", "YES", "NO",
                            (b) => DBG.ShowModelNames = b, () => DBG.ShowModelNames),
                        new DbgMenuItemBool("Show Model Bounding Boxes", "YES", "NO",
                            (b) => DBG.ShowModelBoundingBoxes = b, () => DBG.ShowModelBoundingBoxes),
                        new DbgMenuItemBool("Show Model Submesh Bounding Boxes", "YES", "NO",
                            (b) => DBG.ShowModelSubmeshBoundingBoxes = b, () => DBG.ShowModelSubmeshBoundingBoxes),
                        //new DbgMenuItemBool("Show Grid", "YES", "NO",
                        //    (b) => DBG.ShowGrid = b, () => DBG.ShowGrid),
                        new DbgMenuItemBool("Textures", "ON", "OFF",
                            (b) => GFX.EnableTextures = b, () => GFX.EnableTextures),
                        //new DbgMenuItemBool("Realtime Lighting", "ON", "OFF",
                        //    (b) => GFX.EnableLighting = b, () => GFX.EnableLighting),
                        //new DbgMenuItemBool("Enable Automatic Light Spin Test", "ON", "OFF",
                        //    (b) => GFX.TestLightSpin = b, () => GFX.TestLightSpin),
                        //new DbgMenuItemBool("Enable Headlight", "ON", "OFF",
                        //    (b) => GFX.EnableHeadlight = b, () => GFX.EnableHeadlight),
                        //new DbgMenuItemBool("Lightmapping", "ON", "OFF",
                        //    (b) => GFX.EnableLightmapping = b, () => GFX.EnableLightmapping),
                        new DbgMenuItemBool("Wireframe Mode", "ON", "OFF",
                            (b) => GFX.Wireframe = b, () => GFX.Wireframe),
                        //new DbgMenuItemBool("View Frustum Culling (Experimental)", "ON", "OFF",
                        //    (b) => GFX.EnableFrustumCulling = b, () => GFX.EnableFrustumCulling),
                        new DbgMenuItemNumber("Vertical Field of View (Degrees)", 20, 150, 1,
                            (f) => GFX.CurrentWorldView.ProjectionVerticalFoV = f, () => GFX.CurrentWorldView.ProjectionVerticalFoV,
                            (f) => $"{((int)(Math.Round(f)))}"),
                        new DbgMenuItemNumber("Near Clip Distance", 0.0001f, 5, 0.0001f,
                            (f) => GFX.CurrentWorldView.ProjectionNearClipDist = f, () => GFX.CurrentWorldView.ProjectionNearClipDist),
                        new DbgMenuItemNumber("Far Clip Distance", 100, 1000000, 100,
                            (f) => GFX.CurrentWorldView.ProjectionFarClipDist = f, () => GFX.CurrentWorldView.ProjectionFarClipDist),
                        new DbgMenuItem()
                        {
                            Text = "Reset All To Default",
                            Items = new List<DbgMenuItem>
                            {
                                new DbgMenuItem() { Text = "Are you sure you want to reset all Graphics settings to default?" },
                                new DbgMenuItem()
                                {
                                    Text = "No",
                                    ClickAction = (m) => REQUEST_GO_BACK = true
                                },
                                new DbgMenuItem()
                                {
                                    Text = "Yes",
                                    ClickAction = (m) =>
                                    {
                                        CFG.ResetGraphics();
                                        REQUEST_GO_BACK = true;
                                    }
                                }
                            }
                        },
                    }
                },
                new DbgMenuItem()
                {
                    Text = "[Display Settings]",
                    Items = new List<DbgMenuItem>
                    {
                        new DbgMenuItemResolutionChange(),
                        new DbgMenuItemBool("Fullscreen", "YES", "NO", v => GFX.Display.Fullscreen = v, () => GFX.Display.Fullscreen),
                        new DbgMenuItemBool("Vsync", "ON", "OFF", v => GFX.Display.Vsync = v, () => GFX.Display.Vsync),
                        //new DbgMenuItemBool("Simple MSAA", "ON", "OFF", v => GFX.Display.SimpleMSAA = v, () => GFX.Display.SimpleMSAA),
                        new DbgMenuItem()
                        {
                            Text = "Reset All To Default",
                            Items = new List<DbgMenuItem>
                            {
                                new DbgMenuItem() { Text = "Are you sure you want to reset all Display settings to default?" },
                                new DbgMenuItem()
                                {
                                    Text = "No",
                                    ClickAction = (m) => REQUEST_GO_BACK = true
                                },
                                new DbgMenuItem()
                                {
                                    Text = "Yes",
                                    ClickAction = (m) =>
                                    {
                                        CFG.ResetDisplay();
                                        REQUEST_GO_BACK = true;
                                    }
                                }
                            }
                        },
                        new DbgMenuItem()
                        {
                            Text = "Apply Changes",
                            ClickAction = (m) => GFX.Display.Apply(),
                        }
                    }
                },
                new DbgMenuItem()
                {
                    Text = "Return Camera to Origin",
                    ClickAction = m => GFX.CurrentWorldView.NewRecenter(),
                },
                new DbgMenuItem()
                {
                    Text = "[Help]",
                    Items = new List<DbgMenuItem>
                    {
                        new DbgMenuItem()
                        {
                            Text = "[Menu Overlay Controls (Gamepad)]",
                            Items = new List<DbgMenuItem>
                            {
                                new DbgMenuItem() { Text = "Back: Toggle Menu (Active/Visible/Hidden)" },
                                new DbgMenuItem() { Text = "D-Pad Up/Down: Move Cursor Up/Down" },
                                new DbgMenuItem() { Text = "A: Enter/Activate (when applicable)" },
                                new DbgMenuItem() { Text = "B: Go Back (when applicable)" },
                                new DbgMenuItem() { Text = "D-Pad Left/Right: Decrease/Increase" },
                                new DbgMenuItem() { Text = "Start: Reset Value to Default" },
                                new DbgMenuItem() { Text = "Hold LB: Increase/Decrease 10x Faster" },
                                new DbgMenuItem() { Text = "Hold X: Increase/Decrease 100x Faster" },
                                new DbgMenuItem() { Text = "Hold RB + Move LS: Move Menu" },
                                new DbgMenuItem() { Text = "Hold RB + Move RS: Resize Menu" },
                                new DbgMenuItem() { Text = "Hold LB + Move or Resize Menu: Move or Resize Menu Faster" },
                                new DbgMenuItem() { Text = "Click RS: Toggle 3D Render Pause" },
                            }
                        },
                        new DbgMenuItem()
                        {
                            Text = "[General 3D Controls (Gamepad)]",
                            Items = new List<DbgMenuItem>
                            {
                                new DbgMenuItem() { Text = "LS: Move Camera Laterally" },
                                new DbgMenuItem() { Text = "LT: Move Camera Directly Downward" },
                                new DbgMenuItem() { Text = "RT: Move Camera Directly Upward" },
                                new DbgMenuItem() { Text = "RS: Turn Camera" },
                                new DbgMenuItem() { Text = "Hold LB: Move Camera More Slowly" },
                                new DbgMenuItem() { Text = "Hold RB: Move Camera More Quickly" },
                                new DbgMenuItem() { Text = "Click LS and Hold: Turn Light With RS Instead of Camera" },
                                new DbgMenuItem() { Text = "Click RS: Reset Camera To Origin" },
                            }
                        },
                        new DbgMenuItem()
                        {
                            Text = "[Menu Overlay Controls (Mouse & Keyboard)]",
                            Items = new List<DbgMenuItem>
                            {
                                new DbgMenuItem() { Text = "Tilde (~): Toggle Menu (Active/Visible/Hidden)" },

                                new DbgMenuItem() { Text = "Move Mouse Cursor: Move Cursor" },
                                new DbgMenuItem() { Text = "Hold Spacebar + Scroll Mouse Wheel: Change Values" },
                                new DbgMenuItem() { Text = "Mouse Wheel: Scroll Menu" },
                                new DbgMenuItem() { Text = "Enter/Left Click: Enter/Activate (when applicable)" },
                                new DbgMenuItem() { Text = "Backspace/Right Click: Go Back (when applicable)" },
                                new DbgMenuItem() { Text = "Up/Down: Move Cursor Up/Down" },
                                new DbgMenuItem() { Text = "Left/Right: Decrease/Increase" },
                                new DbgMenuItem() { Text = "Home/Middle Click: Reset Value to Default" },
                                new DbgMenuItem() { Text = "Hold Shift: Increase/Decrease 10x Faster" },
                                new DbgMenuItem() { Text = "Hold Ctrl: Increase/Decrease 100x Faster" },
                                new DbgMenuItem() { Text = "Pause Key: Toggle 3D Render Pause" },
                            }
                        },
                        new DbgMenuItem()
                        {
                            Text = "[General 3D Controls (Mouse & Keyboard)]",
                            Items = new List<DbgMenuItem>
                            {
                                new DbgMenuItem() { Text = "WASD: Move Camera Laterally" },
                                new DbgMenuItem() { Text = "Q: Move Camera Directly Downward" },
                                new DbgMenuItem() { Text = "E: Move Camera Directly Upward" },
                                new DbgMenuItem() { Text = "Right Click + Move Mouse: Turn Camera" },
                                new DbgMenuItem() { Text = "Hold Shift: Move Camera More Slowly" },
                                new DbgMenuItem() { Text = "Hold Ctrl: Move Camera More Quickly" },
                                new DbgMenuItem() { Text = "Hold Spacebar: Turn Light With Mouse Instead of Camera" },
                                new DbgMenuItem() { Text = "R: Reset Camera To Origin" },
                            }
                        },
                    }
                },
                new DbgMenuItem()
                {
                    Text = "Exit",
                    Items = new List<DbgMenuItem>
                    {
                        new DbgMenuItem() { Text = "Are you sure you want to exit?" },
                        new DbgMenuItem()
                        {
                            Text = "No",
                            ClickAction = (m) => REQUEST_GO_BACK = true
                        },
                        new DbgMenuItem()
                        {
                            Text = "Yes",
                            ClickAction = (m) => Main.REQUEST_EXIT = true
                        }
                    }
                },

            };
        }

        public const int ITEM_PADDING_REDUCE = 0;

        public const float MENU_MIN_SIZE_X = 256;
        public const float MENU_MIN_SIZE_Y = 128;

        public static SpriteFont FONT => DBG.DEBUG_FONT;
        public static DbgMenuOpenState MenuOpenState = DbgMenuOpenState.Open;
        public static bool IsPauseRendering = false;
        public static DbgMenuItem CurrentMenu = new DbgMenuItem();
        public static Stack<DbgMenuItem> DbgMenuStack = new Stack<DbgMenuItem>();
        public static Vector2 MenuPosition = Vector2.One * 4;
        public static Vector2 MenuSize = new Vector2(1200, 720);
        public static Rectangle MenuRect => new Rectangle(
            (int)MenuPosition.X, (int)MenuPosition.Y, (int)MenuSize.X, (int)MenuSize.Y);
        public static Rectangle SubMenuRect => new Rectangle(MenuRect.Left, 
            MenuRect.Top + 30, MenuRect.Width, MenuRect.Height - 30);

        public static Rectangle DbgMenuTopLeftButtonRect => new Rectangle(MenuRect.X, MenuRect.Y, 30, 30);

        public const float UICursorBlinkTimerMax = 0.5f;
        public static float UICursorBlinkTimer = 0;
        public static bool UICursorBlinkState = false;
        public static string UICursorBlinkString => UICursorBlinkState ? "◆" : "◇";

        public static void EnterNewSubMenu(DbgMenuItem menu)
        {
            CFG.Save();

            menu.RequestTextRefresh();
            DbgMenuStack.Push(CurrentMenu);
            CurrentMenu = menu;
        }

        public static void GoBack()
        {
            CFG.Save();

            if (DbgMenuStack.Count > 0)
                CurrentMenu = DbgMenuStack.Pop();
        }

        public static bool REQUEST_GO_BACK = false;

        public string Text = " ";
        public int SelectedIndex = 0;
        public float Scroll;
        public float MaxScroll;
        public List<DbgMenuItem> Items = new List<DbgMenuItem>();
        private int prevFrameItemCount = 0;
        private float menuHeight = 0;
        public DbgMenuItem SelectedItem => SelectedIndex == -1 ? null : Items[SelectedIndex];
        public Action<DbgMenuItem> ClickAction = null;
        public Func<string> RefreshTextFunction = null;
        public Func<Color> CustomColorFunction = null;

        public virtual void OnClick()
        {
            CFG.Save();
            ClickAction?.Invoke(this);
        }

        public virtual void OnResetDefault()
        {
            
        }

        public virtual void OnIncrease(bool isRepeat, int incrementAmount)
        {

        }

        public virtual void OnDecrease(bool isRepeat, int incrementAmount)
        {

        }

        public virtual void OnRequestTextRefresh()
        {
            Text = RefreshTextFunction?.Invoke() ?? Text;
        }

        public void RequestTextRefresh()
        {
            OnRequestTextRefresh();
            foreach (var item in Items)
            {
                item.OnRequestTextRefresh();
            }
        }

        public void GoDown(bool isRepeat, int incrementAmount)
        {
            int prevIndex = SelectedIndex;
            SelectedIndex += incrementAmount;

            //If upper bound reached
            if (SelectedIndex >= Items.Count)
            {
                //If already at end and just tapped button
                if (prevIndex == Items.Count - 1 && !isRepeat)
                    SelectedIndex = 0; //Wrap Around
                else
                    SelectedIndex = Items.Count - 1; //Stop
            }
        }

        public void GoUp(bool isRepeat, int incrementAmount)
        {
            int prevIndex = SelectedIndex;
            SelectedIndex -= incrementAmount;

            //If upper bound reached
            if (SelectedIndex < 0)
            {
                //If already at end and just tapped button
                if (prevIndex == 0 && !isRepeat)
                    SelectedIndex = Items.Count - 1; //Wrap Around
                else
                    SelectedIndex = 0; //Stop
            }
        }

        public static void UpdateInput(float elapsedSeconds)
        {
            DbgMenuPad.Update(elapsedSeconds);

            if (MenuOpenState == DbgMenuOpenState.Open)
            {
                int incrementAmount = 1;
                if (DbgMenuPad.MoveFastHeld)
                    incrementAmount *= 10;
                if (DbgMenuPad.MoveFasterHeld)
                    incrementAmount *= 100;

                if (DbgMenuPad.Up.State)
                    CurrentMenu.GoUp(!DbgMenuPad.Up.IsInitalButtonTap, incrementAmount);

                if (DbgMenuPad.Down.State)
                    CurrentMenu.GoDown(!DbgMenuPad.Down.IsInitalButtonTap, incrementAmount);

                if (DbgMenuPad.Left.State)
                    CurrentMenu.SelectedItem.OnDecrease(!DbgMenuPad.Left.IsInitalButtonTap, incrementAmount);

                if (DbgMenuPad.Right.State)
                    CurrentMenu.SelectedItem.OnIncrease(!DbgMenuPad.Right.IsInitalButtonTap, incrementAmount);

                if (DbgMenuPad.PauseRendering.State)
                    IsPauseRendering = !IsPauseRendering;

                if (DbgMenuPad.Enter.State)
                {
                    CurrentMenu.SelectedItem.OnClick();
                    if (CurrentMenu.SelectedItem.Items.Count > 0)
                    {
                        EnterNewSubMenu(CurrentMenu.SelectedItem);
                    }
                }

                if (DbgMenuPad.Cancel.State)
                    GoBack();

                if (REQUEST_GO_BACK)
                {
                    REQUEST_GO_BACK = false;
                    GoBack();
                }

                if (DbgMenuPad.ResetDefault.State)
                    CurrentMenu.SelectedItem.OnResetDefault();

                if (DbgMenuPad.MenuRectMove != Vector2.Zero)
                {
                    MenuPosition += DbgMenuPad.MenuRectMove;
                    MenuPosition.X = MathHelper.Clamp(MenuPosition.X, 0, GFX.Device.Viewport.Width - MenuSize.X);
                    MenuPosition.Y = MathHelper.Clamp(MenuPosition.Y, 0, GFX.Device.Viewport.Height - MenuSize.Y);
                }

                if (DbgMenuPad.MenuRectResize != Vector2.Zero)
                {
                    MenuSize += DbgMenuPad.MenuRectResize;
                    MenuSize.X = MathHelper.Clamp(MenuSize.X, MENU_MIN_SIZE_X, GFX.Device.Viewport.Width - MenuPosition.X);
                    MenuSize.Y = MathHelper.Clamp(MenuSize.Y, MENU_MIN_SIZE_Y, GFX.Device.Viewport.Height - MenuPosition.Y);
                }

                if (DbgMenuPad.IsMouseMovedThisFrame || DbgMenuPad.MouseWheelDelta != 0)
                {
                    for (int i = 0; i < CurrentMenu.Items.Count; i++)
                    {
                        var rect = CurrentMenu.GetItemDisplayRect(i, SubMenuRect);
                        if (rect.Contains(DbgMenuPad.MousePos))
                        {
                            CurrentMenu.SelectedIndex = i;
                            break;
                        }
                    }

                }

                var currentItemDisplayRect = CurrentMenu.GetItemDisplayRect(CurrentMenu.SelectedIndex, SubMenuRect);
                if (currentItemDisplayRect.Contains(DbgMenuPad.MousePos))
                {
                    if (DbgMenuPad.ClickMouse.State)
                    {
                        CurrentMenu.SelectedItem.OnClick();
                        if (CurrentMenu.SelectedItem.Items.Count > 0)
                        {
                            EnterNewSubMenu(CurrentMenu.SelectedItem);
                        }
                    }
                    else if (DbgMenuPad.MiddleClickMouse.State)
                    {
                        CurrentMenu.SelectedItem.OnResetDefault();
                    }
                }

                

                if (DbgMenuPad.MouseWheelDelta != 0)
                {
                    if (DbgMenuPad.IsSpacebarHeld)
                    {
                        bool isIncrease = DbgMenuPad.MouseWheelDelta > 0;
                        int incr = (int)Math.Abs(Math.Round(DbgMenuPad.MouseWheelDelta / 150));
                        for (int i = 0; i < incr; i++)
                        {
                            if (isIncrease)
                                CurrentMenu.SelectedItem.OnIncrease(false, incrementAmount);
                            else
                                CurrentMenu.SelectedItem.OnDecrease(false, incrementAmount);
                        }
                    }
                    else
                    {
                        CurrentMenu.Scroll -= DbgMenuPad.MouseWheelDelta;
                    }

                    
                }
            }
        }

        public virtual void UpdateUI()
        {

        }

        private Rectangle GetItemDisplayRect(int index, Rectangle menuRect)
        {
            float top = 0;
            for (int i = 0; i < index; i++)
            {
                top += GetItemSize(i).Y;
            }
            var thisItemSize = GetItemSize(index);
            return new Rectangle(menuRect.Left, (int)(menuRect.Top + top - Scroll), (int)(thisItemSize.X), (int)(thisItemSize.Y));
        }

        public string GetActualItemDisplayText(int i)
        {
            return $"{(SelectedIndex == i ? $"  {UICursorBlinkString} " : "     ")}{Items[i].Text}" +
                       $"{(Items[i].Items.Count > 0 ? $" ({Items[i].Items.Count})" : "")}";
        }

        private Vector2 GetItemSize(int i)
        {
            return FONT.MeasureString(GetActualItemDisplayText(i));
        }

        private float GetEntireMenuHeight()
        {
            float height = 0;
            for (int i = 0; i < Items.Count; i++)
            {
                height += GetItemSize(i).Y;
            }
            return height;
        }

        public static void UICursorBlinkUpdate(float elapsedSeconds)
        {
            if (MenuOpenState == DbgMenuOpenState.Open)
            {
                UICursorBlinkTimer -= elapsedSeconds;
                if (UICursorBlinkTimer <= 0)
                {
                    UICursorBlinkState = !UICursorBlinkState;
                    UICursorBlinkTimer = UICursorBlinkTimerMax;
                }
            }
            else
            {
                UICursorBlinkTimer = UICursorBlinkTimerMax;
                // If menu is closed, have the cursor visible, ready for when its reopened
                // If menu is visible but not closed, make cursor not visible
                UICursorBlinkState = MenuOpenState == DbgMenuOpenState.Closed;
            }
        }

        public void Draw()
        {
            var darkTitleRect = new Rectangle(MenuRect.X + DbgMenuTopLeftButtonRect.Width, MenuRect.Y, MenuRect.Width - DbgMenuTopLeftButtonRect.Width, DbgMenuTopLeftButtonRect.Height);


            GFX.SpriteBatchBeginForText();

            var clickMeSize = DBG.DEBUG_FONT_SMALL.MeasureString("MENU");

            GFX.SpriteBatch.Draw(Main.WHITE_TEXTURE, DbgMenuTopLeftButtonRect, Color.Black * 0.85f);

            GFX.SpriteBatch.DrawString(DBG.DEBUG_FONT_SMALL, "MENU", DbgMenuTopLeftButtonRect.Center(), Color.White,  0, clickMeSize / 2, 1, SpriteEffects.None, 0);

            if (MenuOpenState == DbgMenuOpenState.Closed)
            {
                GFX.SpriteBatchEnd();
                return;
            }

            UpdateUI();

            float menuBackgroundOpacityMult = MenuOpenState == DbgMenuOpenState.Open ? 1.0f : 0f;

            // Draw menu background rect

            //---- Full Background
            GFX.SpriteBatch.Draw(Main.WHITE_TEXTURE, SubMenuRect, Color.Black * 0.5f * menuBackgroundOpacityMult);
            //---- Slightly Darker Part On Top
            GFX.SpriteBatch.Draw(Main.WHITE_TEXTURE, darkTitleRect,
                Color.Black * 0.75f * menuBackgroundOpacityMult);

            if (MenuOpenState == DbgMenuOpenState.Open)
            {
                var renderPauseStr = $"Render Pause:{(IsPauseRendering ? "Active" : "Inactive")}\n(Click RS / Press Pause Key)";
                var renderPauseStrScale = DBG.DEBUG_FONT.MeasureString(renderPauseStr);
                var renderPauseStrColor = !IsPauseRendering ? Color.White : Color.Yellow;

                DBG.DrawOutlinedText(renderPauseStr,
                    new Vector2(8, GFX.Device.Viewport.Height - 20),
                    renderPauseStrColor, DBG.DEBUG_FONT, scaleOrigin: new Vector2(0, renderPauseStrScale.Y), 
                    //scale: IsPauseRendering ? 1f : 0.75f,
                    startAndEndSpriteBatchForMe: false);
            }
                
            // Draw name on top
            var sb = new StringBuilder();
            //---- If in submenu, append the stack of menues preceding this one
            if (DbgMenuStack.Count > 0)
            {
                bool first = true;
                foreach (var chain in DbgMenuStack.Reverse())
                {
                    if (first)
                        first = false;
                    else
                        sb.Append(" > ");
                    sb.Append($"{chain.Text}{(chain.Items.Count > 0 ? $" ({chain.Items.Count})" : "")}");
                }
                sb.Append(" > ");
            }
            //---- Append the current menu name.
            sb.Append($"{Text}{(Items.Count > 0 ? $" ({Items.Count})" : "")}");

            //---- Draw full menu name
            DBG.DrawOutlinedText(sb.ToString(), darkTitleRect.TopLeftCorner() + new Vector2(8, 4), 
                CustomColorFunction?.Invoke() ?? Color.White, DBG.DEBUG_FONT, startAndEndSpriteBatchForMe: false);

            if (Items.Count != 0)
            {
                if (SelectedIndex < 0)
                    SelectedIndex = 0;
                else if (SelectedIndex >= Items.Count)
                    SelectedIndex = Items.Count - 1;

                var selectedItemRect = GetItemDisplayRect(SelectedIndex, SubMenuRect);

                if (Items.Count != prevFrameItemCount)
                    menuHeight = GetEntireMenuHeight();

                // Only need to calculate scroll stuff if there's text that reaches past the bottom.
                if (menuHeight > SubMenuRect.Height)
                {
                    // Scroll selected into view.

                    //---- If item is ABOVE view
                    if (selectedItemRect.Top < SubMenuRect.Top)
                    {
                        int distanceNeededToScroll = SubMenuRect.Top - selectedItemRect.Top;
                        Scroll -= distanceNeededToScroll;
                    }
                    //---- If item is BELOW view
                    if (selectedItemRect.Bottom > SubMenuRect.Bottom)
                    {
                        int distanceNeededToScroll = selectedItemRect.Bottom - SubMenuRect.Bottom;
                        Scroll += distanceNeededToScroll;
                    }
                }

                // Clamp scroll

                MaxScroll = Math.Max(GetEntireMenuHeight() - SubMenuRect.Height, 0);
                if (Scroll > MaxScroll)
                    Scroll = MaxScroll;
                else if (Scroll < 0)
                    Scroll = 0;

                // Debug display of menu item rectangles:
                //for (int i = 0; i < Items.Count; i++)
                //{
                //    var TEST_DebugDrawItemRect = GetItemDisplayRect(i, SubMenuRect);

                //    GFX.SpriteBatch.Begin();
                //    GFX.SpriteBatch.Draw(MODEL_VIEWER_MAIN.DEFAULT_TEXTURE_DIFFUSE, TEST_DebugDrawItemRect, Color.Yellow);
                //    GFX.SpriteBatch.End();
                //}

                // ONLY draw the menu items that are in-frame

                int roughStartDrawIndex = (int)((Scroll / menuHeight) * (Items.Count - 1)) - 1;
                int roughEndDrawIndex = (int)(((Scroll + MenuRect.Height) / menuHeight) * (Items.Count - 1)) + 1;

                if (roughStartDrawIndex < 0)
                    roughStartDrawIndex = 0;
                else if (roughStartDrawIndex >= Items.Count)
                    roughStartDrawIndex = Items.Count - 1;

                if (roughEndDrawIndex < 0)
                    roughEndDrawIndex = 0;
                else if (roughEndDrawIndex >= Items.Count)
                    roughEndDrawIndex = Items.Count - 1;

                GFX.SpriteBatchEnd();

                // Store current viewport, then switch viewport to JUST the menu rect
                var oldViewport = GFX.Device.Viewport;
                GFX.Device.Viewport = new Viewport(
                    oldViewport.X + SubMenuRect.X,
                    oldViewport.Y + SubMenuRect.Y,
                    SubMenuRect.Width,
                    SubMenuRect.Height);
                
                GFX.SpriteBatchBegin();
                // ---- These braces manually force a smaller scope so we 
                //      don't forget to return to the old viewport immediately afterward.
                {
                    // Draw Items

                    var selectionPrefixTextSize = Vector2.Zero;// FONT.MeasureString($"  {UICursorBlinkString} ");

                    for (int i = roughStartDrawIndex; i <= roughEndDrawIndex; i++)
                    {
                        Items[i].UpdateUI();
                        var entryText = GetActualItemDisplayText(i);

                        var itemRect = GetItemDisplayRect(i, SubMenuRect);

                        // Check if this item is inside the actual menu rectangle.
                        if (SubMenuRect.Intersects(itemRect))
                        {
                            var itemTextColor = Items[i].CustomColorFunction?.Invoke() ?? ((SelectedIndex == i
                                && MenuOpenState == DbgMenuOpenState.Open)
                                ? Color.LightGreen : Color.White);

                            if (SelectedIndex == i && MenuOpenState == DbgMenuOpenState.Open)
                            {
                                var underlineRect = new Rectangle(
                                    itemRect.X - SubMenuRect.X + (int)selectionPrefixTextSize.X - 4,
                                    itemRect.Y - SubMenuRect.Y - 1,
                                    MenuRect.Width - (int)(selectionPrefixTextSize.X) + 4,
                                    itemRect.Height + 2);


                                if (menuHeight > SubMenuRect.Height)
                                {
                                    underlineRect = new Rectangle(underlineRect.X + 12, underlineRect.Y, underlineRect.Width - 4, underlineRect.Height);
                                }

                                GFX.SpriteBatch.Draw(Main.WHITE_TEXTURE, underlineRect, 
                                Color.Black);
                            }

                                
                            // We have to SUBTRACT the menu top/left coord because the string 
                            // drawing is relative to the VIEWPORT, which takes up just the actual menu rect
                            DBG.DrawOutlinedText(entryText,
                                new Vector2(itemRect.X - SubMenuRect.X, itemRect.Y - SubMenuRect.Y),
                                itemTextColor, FONT, startAndEndSpriteBatchForMe: false);
                        }

                    }

                    // Draw Scrollbar
                    // Only if there's stuff that passes the bottom of the menu.
                    if (menuHeight > SubMenuRect.Height)
                    {
                        //---- Draw Scrollbar Background
                        GFX.SpriteBatch.Draw(Main.WHITE_TEXTURE,
                            new Rectangle(0, 0, 8, SubMenuRect.Height), Color.White * 0.5f * menuBackgroundOpacityMult);

                        float curScrollRectTop = (Scroll / menuHeight) * SubMenuRect.Height;
                        float curScrollRectHeight = (SubMenuRect.Height / menuHeight) * SubMenuRect.Height;

                        //---- Scroll Scrollbar current scroll
                        GFX.SpriteBatch.Draw(Main.WHITE_TEXTURE,
                            new Rectangle(0, (int)curScrollRectTop, 8, (int)curScrollRectHeight),
                            Color.White * 0.75f * menuBackgroundOpacityMult);
                    }
                }
                //---- Return to old viewport
                GFX.SpriteBatchEnd();
                GFX.Device.Viewport = oldViewport;
                

                
                

                prevFrameItemCount = Items.Count;
            }
            
        }
    }
}

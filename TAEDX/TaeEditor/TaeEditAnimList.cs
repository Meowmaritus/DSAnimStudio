using MeowDSIO.DataFiles;
using MeowDSIO.DataTypes.TAE;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAEDX.TaeEditor
{
    public class TaeEditAnimList
    {
        private class TaeEditAnimListGroup
        {
            public string GroupName;
            public TAE Tae;
            public Dictionary<string, AnimationRef> AnimNameMap = new Dictionary<string, AnimationRef>();
        }

        public Rectangle Rect;
        public readonly TaeEditorScreen MainScreen;

        private TaeScrollViewer ScrollViewer;

        private int AnimHeight = 24;

        private List<TaeEditAnimListGroup> AnimGroups = new List<TaeEditAnimListGroup>();

        private float EntireListHeight = 0;

        public TaeEditAnimList(TaeEditorScreen mainScreen)
        {
            MainScreen = mainScreen;

            if (MainScreen.Anibnd.StandardTAE != null)
            {
                var group = new TaeEditAnimListGroup();
                group.GroupName = System.IO.Path.GetFileName(MainScreen.Anibnd.StandardTAE.FilePath ?? MainScreen.Anibnd.StandardTAE.VirtualUri);
                foreach (var anim in MainScreen.Anibnd.StandardTAE.Animations)
                {
                    group.AnimNameMap.Add($"a{(anim.ID / 10000):D2}_{(anim.ID % 10000):D4}", anim);
                    EntireListHeight += AnimHeight;
                }
                AnimGroups.Add(group);
                EntireListHeight += AnimHeight;
            }

            if (MainScreen.Anibnd.PlayerTAE != null)
            {
                foreach (var kvp in MainScreen.Anibnd.PlayerTAE)
                {
                    var group = new TaeEditAnimListGroup();
                    group.GroupName = System.IO.Path.GetFileName(kvp.Value.FilePath ?? kvp.Value.VirtualUri);
                    foreach (var anim in kvp.Value.Animations)
                    {
                        group.AnimNameMap.Add($"a{kvp.Key:D2}_{anim.ID:D4}", anim);
                        EntireListHeight += AnimHeight;
                    }
                    AnimGroups.Add(group);
                    EntireListHeight += AnimHeight;
                }
            }

            

            ScrollViewer = new TaeScrollViewer();
        }

        public void Update(float elapsedSeconds, bool allowMouseUpdate)
        {
            if (!allowMouseUpdate)
                return;

            ScrollViewer.UpdateInput(MainScreen.Input, elapsedSeconds, true);

            if (Rect.Contains(MainScreen.Input.MousePositionPoint))
            {
                MainScreen.Input.CursorType = MouseCursorType.Arrow;

                if (MainScreen.Input.LeftClickDown)
                {
                    var mouseCheckPoint = new Point(
                        (int)(MainScreen.Input.MousePosition.X - Rect.X + ScrollViewer.Scroll.X),
                        (int)(MainScreen.Input.MousePosition.Y - Rect.Y + ScrollViewer.Scroll.Y));

                    int i = 0;
                    foreach (var group in AnimGroups)
                    {
                        i++; //Group Header
                        foreach (var anim in group.AnimNameMap)
                        {
                            var thisAnimRect = new Rectangle(0, i * AnimHeight, ScrollViewer.Viewport.Width, AnimHeight);
                            if (thisAnimRect.Contains(mouseCheckPoint))
                                MainScreen.SelectNewAnimRef(group.Tae, anim.Value);
                            i++;
                        }
                    }
                }
            }

            
        }

        public void UpdateMouseOutsideRect(float elapsedSeconds, bool allowMouseUpdate)
        {
            if (!allowMouseUpdate)
                return;

            ScrollViewer.UpdateInput(MainScreen.Input, elapsedSeconds, allowScrollWheel: false);
        }

        public void Draw(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex, SpriteFont font)
        {
            ScrollViewer.SetDisplayRect(Rect, new Point(Rect.Width, (int)EntireListHeight));

            ScrollViewer.Draw(gd, sb, boxTex, font);

            var oldViewport = gd.Viewport;
            gd.Viewport = new Viewport(ScrollViewer.Viewport);
            {
                sb.Begin(transformMatrix: ScrollViewer.GetScrollMatrix());

                //sb.Draw(texture: boxTex,
                //    position: Vector2.Zero,
                //    sourceRectangle: null,
                //    color: Color.DarkGray,
                //    rotation: 0,
                //    origin: Vector2.Zero,
                //    scale: new Vector2(Rect.Width, Rect.Height),
                //    effects: SpriteEffects.None,
                //    layerDepth: 0
                //    );

                int i = 0;

                foreach (var group in AnimGroups)
                {
                    var thisGroupRect = new Rectangle(0, i * AnimHeight, ScrollViewer.Viewport.Width, AnimHeight);
                    sb.Draw(boxTex, thisGroupRect, Color.Gray);
                    sb.DrawString(font, $"[{group.GroupName}]", new Vector2(4, (int)(i * AnimHeight)) + Vector2.One, Color.Black);
                    sb.DrawString(font, $"[{group.GroupName}]", new Vector2(4, (int)(i * AnimHeight)) + (Vector2.One * 2), Color.Black);
                    sb.DrawString(font, $"[{group.GroupName}]", new Vector2(4, (int)(i * AnimHeight)), Color.White);

                    i++;

                    foreach (var anim in group.AnimNameMap)
                    {
                        string animNameStr = (anim.Value.IsModified ? $"{anim.Key}*" : anim.Key);

                        if (anim.Value == MainScreen.SelectedTaeAnim)
                        {
                            var thisAnimRect = new Rectangle(0, i * AnimHeight, ScrollViewer.Viewport.Width, AnimHeight);
                            sb.Draw(boxTex, thisAnimRect, MainScreen.Config.EnableColorBlindMode ? Color.White : Color.CornflowerBlue);

                            if (MainScreen.Config.EnableColorBlindMode)
                            {
                                sb.DrawString(font, animNameStr, new Vector2(4, (int)(i * AnimHeight)), Color.Black);
                            }
                            else
                            {
                                sb.DrawString(font, animNameStr, new Vector2(4, (int)(i * AnimHeight)) + Vector2.One, Color.Black);
                                sb.DrawString(font, animNameStr, new Vector2(4, (int)(i * AnimHeight)), Color.White);
                            }
                        }
                        else
                        {
                            sb.DrawString(font, animNameStr, new Vector2(4, (int)(i * AnimHeight)) + Vector2.One, Color.Black);
                            sb.DrawString(font, animNameStr, new Vector2(4, (int)(i * AnimHeight)), Color.White);
                        }

                        i++;
                    }
                }

                

                sb.End();
            }
            gd.Viewport = oldViewport;
        }
    }
}

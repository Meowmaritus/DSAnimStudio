using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class NotificationManager
    {
        public class Notification
        {
            public string Text;
            public Vector2 TextSize;
            public float Timer;
            public float ShowDuration = 4;
            public float FadeDuration = 0.5f;
            public float Opacity = 1;
            public Color Color;
            public bool Kill;
            public void Update(float deltaTime)
            {
                Timer += deltaTime;
                if (Timer < ShowDuration)
                    Opacity = 1;
                else
                    Opacity = MathHelper.Clamp(1 - ((Timer - ShowDuration) / FadeDuration), 0, 1);
                Opacity *= Opacity;
                if (Timer > ShowDuration + FadeDuration)
                    Kill = true;
            }
        }

        public static Color ColorDefault => Color.White;
        public static Color ColorWarning => Color.Orange;
        public static Color ColorError => Color.Red;

        private static List<Notification> notifications = new List<Notification>();
        public static IReadOnlyList<Notification> Notifications => notifications;
        private static object _lock_notifications = new object();
        public static void PushNotification(string text, float showDuration = 2.5f, float fadeDuration = 0.35f, Color? color = null)
        {
            lock (_lock_notifications)
            {
                var notif = new Notification()
                {
                    Text = text,
                    ShowDuration = showDuration,
                    FadeDuration = fadeDuration,
                    Color = color ?? Color.White,
                };
                if (notifications.Count >= 50)
                    notifications.RemoveAt(0);
                notifications.Add(notif);
            }
        }

        public static void UpdateAll(float elapsedTime)
        {
            lock (_lock_notifications)
            {
                foreach (var notification in notifications)
                    notification.Update(elapsedTime);
                notifications.RemoveAll(x => x.Kill);
            }
        }

        const float VerticalSpacingBetweenNotifs = 4;
        const float PaddingFromScreenLeft = 32;
        const float PaddingFromScreenBottom = 32;
        const float PaddingAroundStrings = 8;
        const int PaddingAroundInnerRect = 2;

        public static void DrawAll()
        {
            lock (_lock_notifications)
            {
                GFX.SpriteBatchBegin();
                float height = 0;
                foreach (var notification in notifications)
                {
                    //if (notification.TextSize == Vector2.Zero)
                    //    notification.TextSize = DBG.DEBUG_FONT_SMALL.MeasureString(notification.Text);
                    notification.TextSize = ImGuiDebugDrawer.MeasureString(notification.Text);
                    bool first = height == 0;
                    height += notification.TextSize.Y + (PaddingAroundStrings * 2);
                    if (!first)
                        height += VerticalSpacingBetweenNotifs;
                }
                Vector2 currentPos = new Vector2(PaddingFromScreenLeft, GFX.Device.Viewport.Height - PaddingFromScreenBottom - height);
                foreach (var notification in notifications)
                {
                    Vector2 actualPos = currentPos + new Vector2(PaddingAroundStrings, PaddingAroundStrings);
                    Rectangle bgRect = new Rectangle(
                        (int)Math.Round(currentPos.X),
                        (int)Math.Round(currentPos.Y),
                        (int)Math.Round(notification.TextSize.X + (PaddingAroundStrings * 2)),
                        (int)Math.Round(notification.TextSize.Y + (PaddingAroundStrings * 2)));

                    // Only draw if onscreen.
                    if (bgRect.Bottom >= 0)
                    {
                        Rectangle innerRect = new Rectangle(bgRect.X + PaddingAroundInnerRect, bgRect.Y + PaddingAroundInnerRect,
                            bgRect.Width - (PaddingAroundInnerRect * 2), bgRect.Height - (PaddingAroundInnerRect * 2));
                        //GFX.SpriteBatch.Draw(Main.WHITE_TEXTURE, bgRect, Color.White * notification.Opacity * notification.Opacity);
                        //GFX.SpriteBatch.Draw(Main.WHITE_TEXTURE, innerRect, new Color(0.3f, 0.3f, 0.3f) * (MathHelper.Clamp(notification.Opacity * 1.5f, 0, 1)));

                        ImGuiDebugDrawer.DrawRect(bgRect, Color.White * notification.Opacity * notification.Opacity, 6);
                        ImGuiDebugDrawer.DrawRect(innerRect, new Color(0.3f, 0.3f, 0.3f) * (MathHelper.Clamp(notification.Opacity * 1.5f, 0, 1)), 6);

                        DBG.DrawOutlinedText(notification.Text, actualPos,
                                notification.Color * notification.Opacity, DBG.DEBUG_FONT_SMALL, startAndEndSpriteBatchForMe: false);
                    }
                    currentPos += new Vector2(0, notification.TextSize.Y + (PaddingAroundStrings * 2) + VerticalSpacingBetweenNotifs);
                    
                }
                GFX.SpriteBatchEnd();
            }
        }
    }
}

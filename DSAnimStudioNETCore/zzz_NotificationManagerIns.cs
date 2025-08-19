using DSAnimStudio.ImguiOSD;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class zzz_NotificationManagerIns
    {
        //public zzz_DocumentIns ParentDocument;
        //public zzz_NotificationManagerIns(zzz_DocumentIns parentDocument)
        //{
        //    ParentDocument = parentDocument;
        //}






        public class Notification
        {
            public string Text;
            public Color Color;
        }

        public class NotificationPopup
        {
            public Notification Notif;
            public Vector2 TextSize;
            public float Timer;
            public float ShowDuration = 4;
            public float FadeDuration = 0.5f;
            public float Opacity = 1;

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


        public static bool UseNotifHistoryMax = true;
        public static int NotifHistoryMax = 1000;

        // public List<Notification> GetNotificationHistory()
        // {
        //     List<Notification> result = null;
        //     lock (_lock_notifications)
        //     {
        //         result = NotificationHistory.ToList();
        //     }
        //
        //     return result;
        // }

        // public void ClearNotificationHistory()
        // {
        //     lock (_lock_notifications)
        //     {
        //         NotificationHistory.Clear();
        //     }
        // }

        public static Queue<Notification> NotificationHistory = new Queue<Notification>();

        private static List<NotificationPopup> notificationPopups = new List<NotificationPopup>();
        public static IReadOnlyList<NotificationPopup> NotificationPopups => notificationPopups;
        public static object _lock_notifications = new object();
        public static void PushNotification(string text, float showDuration = 2.5f, float fadeDuration = 0.35f, Color? color = null)
        {
            lock (_lock_notifications)
            {
                var notif = new Notification()
                {
                    Text = text,
                    Color = color ?? Color.White,
                };

                var notifPopup = new NotificationPopup()
                {
                    Notif = notif,
                    ShowDuration = showDuration,
                    FadeDuration = fadeDuration,
                };
                if (notificationPopups.Count >= 20)
                    notificationPopups.RemoveAt(0);
                notificationPopups.Add(notifPopup);

                NotificationHistory.Enqueue(notif);

                if (UseNotifHistoryMax)
                {
                    while (NotificationHistory.Count > NotifHistoryMax)
                    {
                        NotificationHistory.Dequeue();
                    }
                }

                OSD.SpWindowNotifications.RequestScrollDown = true;
            }
        }

        public static void UpdateAll(float elapsedTime)
        {
            lock (_lock_notifications)
            {
                foreach (var notification in notificationPopups)
                    notification.Update(elapsedTime);
                notificationPopups.RemoveAll(x => x.Kill);
            }
        }

        const float VerticalSpacingBetweenNotifs = 1;
        const float PaddingFromGraphLeft = 32;
        const float PaddingFromGraphBottom = 32;
        public static Vector2 PaddingAroundStrings = new Vector2(4, 2);
        const int PaddingAroundInnerRect = 1;

        public static void DrawAll(Rectangle rect)
        {
            //return;

            lock (_lock_notifications)
            {
                GFX.SpriteBatchBegin();
                float height = 0;
                foreach (var notification in notificationPopups)
                {
                    //if (notification.TextSize == Vector2.Zero)
                    //    notification.TextSize = DBG.DEBUG_FONT_SMALL.MeasureString(notification.Text);
                    notification.TextSize = ImGuiDebugDrawer.MeasureString(notification.Notif.Text);
                    bool first = height == 0;
                    height += notification.TextSize.Y + (PaddingAroundStrings.Y * 2);
                    if (!first)
                        height += VerticalSpacingBetweenNotifs;
                }
                Vector2 currentPos = new Vector2(rect.Left + PaddingFromGraphLeft, rect.Bottom - PaddingFromGraphBottom - height);
                foreach (var notification in notificationPopups)
                {
                    Vector2 actualPos = currentPos + PaddingAroundStrings;
                    Rectangle bgRect = new Rectangle(
                        (int)Math.Round(currentPos.X),
                        (int)Math.Round(currentPos.Y),
                        (int)Math.Round(notification.TextSize.X + (PaddingAroundStrings.X * 2)),
                        (int)Math.Round(notification.TextSize.Y + (PaddingAroundStrings.Y * 2)));

                    // Only draw if onscreen.
                    if (bgRect.Bottom >= 0)
                    {
                        Rectangle innerRect = new Rectangle(bgRect.X + PaddingAroundInnerRect, bgRect.Y + PaddingAroundInnerRect,
                            bgRect.Width - (PaddingAroundInnerRect * 2), bgRect.Height - (PaddingAroundInnerRect * 2));
                        //GFX.SpriteBatch.Draw(Main.WHITE_TEXTURE, bgRect, Color.White * notification.Opacity * notification.Opacity);
                        //GFX.SpriteBatch.Draw(Main.WHITE_TEXTURE, innerRect, new Color(0.3f, 0.3f, 0.3f) * (MathHelper.Clamp(notification.Opacity * 1.5f, 0, 1)));

                        ImGuiDebugDrawer.DrawRect(bgRect, Color.White * notification.Opacity * notification.Opacity, 3);
                        ImGuiDebugDrawer.DrawRect(innerRect, new Color(0.3f, 0.3f, 0.3f) * (MathHelper.Clamp(notification.Opacity * 1.5f, 0, 1)), 3);

                        DBG.DrawOutlinedText(notification.Notif.Text, actualPos,
                                notification.Notif.Color * notification.Opacity, DBG.DEBUG_FONT_SMALL, startAndEndSpriteBatchForMe: false);
                    }
                    currentPos += new Vector2(0, notification.TextSize.Y + (PaddingAroundStrings.Y * 2) + VerticalSpacingBetweenNotifs);

                }
                GFX.SpriteBatchEnd();
            }
        }
    }
}

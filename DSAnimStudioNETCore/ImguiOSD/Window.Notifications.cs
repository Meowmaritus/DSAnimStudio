using DSAnimStudio.TaeEditor;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public class Notifications : Window
        {
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.SaveAlways;

            public override string NewImguiWindowTitle => "Notifications";

            public bool RequestScrollDown = false;
            
            protected override void Init()
            {
                
            }

            protected override void BuildContents(ref bool anyFieldFocused)
            {
                lock (zzz_NotificationManagerIns._lock_notifications)
                {
                    if (Tools.SimpleClickButton("Clear Notification History"))
                    {
                        zzz_NotificationManagerIns.NotificationHistory.Clear();
                    }
                    // if (Tools.SimpleClickButton("TEST NOTIF"))
                    // {
                    //     NotificationManager.PushNotification($"TEST{Guid.NewGuid().ToString()}", 2.5f, 0.35f, Color.Red);
                    // }
                    var cursorYAtListStart = ImGui.GetCursorPosY();
                    ImGui.BeginChild("NotificationManager_Child", new Vector2(LastSize.X - 16, LastSize.Y - cursorYAtListStart - 8), true, ImGuiWindowFlags.AlwaysVerticalScrollbar);
                    int i = 0;
                    foreach (var notif in zzz_NotificationManagerIns.NotificationHistory)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, notif.Color.PackedValue);
                        
                        ImGui.TextWrapped(notif.Text);

                        
                        
                        //ImGui.TreeNodeEx(notif.Text, ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen);
                        
                        ImGui.PopStyleColor();

                        ImGui.Separator();
                        
                        i++;
                    }

                    if (RequestScrollDown)
                    {
                        ImGui.SetScrollHereY();
                        RequestScrollDown = false;
                    }

                    ImGui.EndChild();
                }
                
            }
        }
    }
}

using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public class TooltipManager
    {
        //private static WinformsTooltipHelper tooltipHelper = new WinformsTooltipHelper();
        private Window.Tooltip TooltipWindow = new Window.Tooltip();

        private float tooltipTimer = 0;
        public float TooltipDelay => (Main.Config?.TooltipDelayMS ?? 1000) / 1000f;

        public float MouseMoveDistToClose = 1;
        
        private bool hoveringOverAnythingThisFrame = false;
        private string currentHoverIDKey = null;
        private string prevHoverIDKey = null;

        private string desiredTooltipText = null;

        private System.Numerics.Vector2 mouseLocationSpawnedFrom;

        private ImguiOSD.Window lastWindowFocused = null;

        public void CancelTooltip()
        {
            tooltipTimer = 0;
            currentHoverIDKey = null;
            desiredTooltipText = null;
        }

        public void DoTooltip(string idKey, string text)
        {
            bool hovered = ImGui.IsItemHovered();
            DoTooltipManual(idKey, text, hovered);
        }
        
        public void DoTooltipManual(string idKey, string text, bool isHovering)
        {
            if (isHovering)
            {
                
                currentHoverIDKey = idKey;
                desiredTooltipText = text;
                hoveringOverAnythingThisFrame = true;

                if (prevHoverIDKey != currentHoverIDKey)
                {
                    mouseLocationSpawnedFrom = Main.Input.MousePosition.ToNumerics();
                    //tooltipTimer = 0;
                }
            }
            
        }

        public void MouseMove()
        {
            if ((Main.Input.MousePosition - mouseLocationSpawnedFrom).LengthSquared() >= (MouseMoveDistToClose * MouseMoveDistToClose))
            {
                currentHoverIDKey = null;
                tooltipTimer = 0;
                TooltipWindow.IsOpen = false;
                if (lastWindowFocused != null)
                {
                    ImGui.SetWindowFocus(lastWindowFocused.ImguiTag);
                    OSD.ActualFocusedWindow = lastWindowFocused;
                    lastWindowFocused = null;
                }
            }
        }
        
        public void PostUpdate(float deltaTime, float offsetX, float offsetY)
        {
            if (Main.Input.AnyMouseButtonDown)
            {
                currentHoverIDKey = null;
                tooltipTimer = 0;
                TooltipWindow.IsOpen = false;
                if (lastWindowFocused != null)
                {
                    ImGui.SetWindowFocus(lastWindowFocused.ImguiTag);
                    OSD.ActualFocusedWindow = lastWindowFocused;
                    lastWindowFocused = null;
                }
            }
            
            if (currentHoverIDKey != null)
            {
                if (tooltipTimer >= TooltipDelay && !OSD.AnyFieldFocused)
                {
                    var curMousePos = Main.Input.MousePosition * Main.DPI;
                    TooltipWindow.TooltipPos = new System.Numerics.Vector2(curMousePos.X + offsetX + (32 * Main.DPI),
                        curMousePos.Y + offsetY + (32 * Main.DPI));
                    TooltipWindow.SetTooltipText(desiredTooltipText, 600 * Main.DPI);
                    TooltipWindow.IsOpen = true;
                }
                else
                {
                    mouseLocationSpawnedFrom = Main.Input.MousePosition.ToNumerics();
                    lastWindowFocused = OSD.ActualFocusedWindow;
                    tooltipTimer += deltaTime;
                }

                
            }
            else
            {
                TooltipWindow.IsOpen = false;
                
                tooltipTimer = 0;
            }

            TooltipWindow.UniqueImguiInstanceNoSavedSettings = true;
            Window currentFocusedWindow = null;
            bool anyFieldFocused = false;
            TooltipWindow.Update(ref currentFocusedWindow, ref anyFieldFocused);
            //
            // ImGuiDebugDrawer.DrawText($"currentHoverIDKey: {currentHoverIDKey}" +
            //                           $"\nprevHoverIDKey: {prevHoverIDKey}" +
            //                           $"\ntooltipTimer: {tooltipTimer}", new Vector2(32, 32), Color.Cyan);
            //
            // ImGuiDebugDrawer.DrawRect(mouseLocationSpawnedFrom, new Vector2(32, 32), Color.Cyan, 0, false);
            //
            prevHoverIDKey = currentHoverIDKey;
            //hoveringOverAnythingThisFrame = false;
        }

        public void PreUpdate(float elapsedTime, float offsetX, float offsetY)
        {
            //var mousePos = ImGui.GetMousePos();

            // if (hoveringOverAnythingThisFrame)
            // {
            //     TooltipWindow.Position = new System.Numerics.Vector2(desiredMousePosition.X + offsetX + 16,
            //         desiredMousePosition.Y + offsetY + 16);
            //     TooltipWindow.SetTooltipText(desiredTooltipText, 300);
            //     TooltipWindow.IsOpen = true;
            //     TooltipWindow.Update();
            // }
            // else
            // {
            //     TooltipWindow.IsOpen = false;
            //     TooltipWindow.Update();
            // }
            
            
            
            
            //bruh
            // tooltipHelper.DrawPosition = new Vector2(mousePos.X + offsetX + 16, mousePos.Y + offsetY + 16);
            //
            // tooltipHelper.Update(currentHoverIDKey != null, elapsedTime);
            //
            // tooltipHelper.UpdateTooltip(Main.WinForm, currentHoverIDKey, desiredTooltipText);

            //var curModelViewerBounds = zzz_DocumentManager.CurrentDocument.EditorScreen.ModelViewerBounds;

            //if (!IsInit && oldModelViewerBounds != curModelViewerBounds)
            //{
            //    QueuedWindowPosChange += new Vector2(curModelViewerBounds.Width - oldModelViewerBounds.Width, 0);
            //}


            hoveringOverAnythingThisFrame = false;
            currentHoverIDKey = null;
        }
    }
}

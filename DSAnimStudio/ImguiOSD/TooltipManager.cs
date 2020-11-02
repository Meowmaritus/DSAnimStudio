using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public static class TooltipManager
    {
        private static WinformsTooltipHelper tooltipHelper = new WinformsTooltipHelper();

        private static float tooltipTimer = 0;
        public static float TooltipDelay = 0.25f;

        private static bool hoveringOverAnythingThisFrame = false;
        private static string currentHoverIDKey = null;
        private static string prevHoverIDKey = null;

        private static string desiredTooltipText = null;

        public static void CancelTooltip()
        {
            tooltipTimer = 0;
            currentHoverIDKey = null;
            desiredTooltipText = null;
        }

        public static void DoTooltip(string idKey, string text)
        {
            if (ImGui.IsItemHovered())
            {
                currentHoverIDKey = idKey;
                desiredTooltipText = text;
                //if (!tooltipTexts.ContainsKey(idKey))
                //    tooltipTexts.Add(idKey, text);
                //else
                //    tooltipTexts[idKey] = text;
                hoveringOverAnythingThisFrame = true;
            }
        }

        public static void PostUpdate()
        {
            if (!hoveringOverAnythingThisFrame)
            {
                tooltipTimer = 0;
                desiredTooltipText = null;
                currentHoverIDKey = null;
            }
            else
            {
                if (currentHoverIDKey != prevHoverIDKey)
                {
                    tooltipTimer = 0;

                    tooltipHelper.Update(false, 0);
                }

                //if (currentHoverIDKey != null)
                //{
                //    if (tooltipTimer < TooltipDelay)
                //    {
                //        tooltipTimer += elapsedTime;
                //    }
                //    else
                //    {
                //        ImGui.SetTooltip(desiredTooltipText);
                //    }
                //}


            }



            prevHoverIDKey = currentHoverIDKey;
        }

        public static void PreUpdate(float elapsedTime, float offsetX, float offsetY)
        {
            var mousePos = ImGui.GetMousePos();
            tooltipHelper.DrawPosition = new Vector2(mousePos.X + offsetX + 16, mousePos.Y + offsetY + 16);

            tooltipHelper.Update(currentHoverIDKey != null, elapsedTime);

            tooltipHelper.UpdateTooltip(Main.WinForm, currentHoverIDKey, desiredTooltipText);

            var curModelViewerBounds = Main.TAE_EDITOR.ModelViewerBounds;

            //if (!IsInit && oldModelViewerBounds != curModelViewerBounds)
            //{
            //    QueuedWindowPosChange += new Vector2(curModelViewerBounds.Width - oldModelViewerBounds.Width, 0);
            //}


            hoveringOverAnythingThisFrame = false;
        }
    }
}

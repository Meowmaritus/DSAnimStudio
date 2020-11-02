using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Dialog
    {
        [Flags]
        public enum CancelTypes : int
        {
            None = 0,
            ClickTitleBarX = 1 << 0,
            PressEscape = 1 << 1,
            PressEnter = 1 << 2,

            Combo_ClickTitleBarX_PressEscape = ClickTitleBarX | PressEscape,
            Combo_All = ClickTitleBarX | PressEscape | PressEnter,
        }

        public string Title;

        public CancelTypes AllowedCancelTypes = CancelTypes.None;

        public bool WasCancelled => CancelType != CancelTypes.None;
        public CancelTypes CancelType { get; private set; } = CancelTypes.None;

        public bool AllowsCancelType(CancelTypes type)
        {
            return (AllowedCancelTypes & type) != 0;
        }

        public bool IsDismissed { get; private set; }
        protected void Dismiss()
        {
            IsDismissed = true;
            Task.Run(OnDismiss);
            //OnDismiss?.Invoke();

        }

        public Action OnDismiss;

        protected abstract void BuildInsideOfWindow();

        public void Update(bool isTopMost)
        {
            if (Title == null)
                throw new InvalidOperationException("Dialog title cannot be null.");

            if (isTopMost)
                ImGui.OpenPopup(Title);
            bool isOpen = true;

            bool didPopupWindow;
            if (AllowsCancelType(CancelTypes.ClickTitleBarX))
            {
                didPopupWindow = ImGui.BeginPopupModal(Title, ref isOpen,
                    ImGuiWindowFlags.AlwaysAutoResize |
                    ImGuiWindowFlags.NoSavedSettings |
                    ImGuiWindowFlags.NoCollapse);
            }
            else
            {
                didPopupWindow = ImGuiEx.BeginPopupModal(Title,
                    ImGuiWindowFlags.AlwaysAutoResize |
                    ImGuiWindowFlags.NoSavedSettings |
                    ImGuiWindowFlags.NoCollapse);
            }

            if (didPopupWindow)
            {
                BuildInsideOfWindow();



                ImGui.EndPopup();
            }

            if (!isOpen && AllowsCancelType(CancelTypes.ClickTitleBarX))
            {
                CancelType = CancelTypes.ClickTitleBarX;
                Dismiss();
            }

            if (AllowsCancelType(CancelTypes.PressEscape) && Main.Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                CancelType = CancelTypes.PressEscape;
                Dismiss();
            }
            else if (AllowsCancelType(CancelTypes.PressEnter) && Main.Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.Enter))
            {
                CancelType = CancelTypes.PressEnter;
                Dismiss();
            }
        }
    }
}

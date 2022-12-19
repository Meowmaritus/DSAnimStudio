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
            ClickedAcceptButton = 1 << 3,

            Combo_ClickTitleBarX_PressEscape = ClickTitleBarX | PressEscape,
            Combo_All = ClickTitleBarX | PressEscape | PressEnter | ClickedAcceptButton,
        }

        public readonly Guid UniqueInstanceGUID = Guid.NewGuid();

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

            // Check for topmost
            if (isTopMost)
                ImGui.OpenPopup($"{Title}##{UniqueInstanceGUID}");
            bool isOpen = true;

            

            bool didPopupWindow;
            if (AllowsCancelType(CancelTypes.ClickTitleBarX))
            {
                didPopupWindow = ImGui.BeginPopupModal($"{Title}##{UniqueInstanceGUID}", ref isOpen,
                    ImGuiWindowFlags.AlwaysAutoResize |
                    ImGuiWindowFlags.NoSavedSettings |
                    ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.Popup);
            }
            else
            {
                didPopupWindow = ImGuiEx.BeginPopupModal($"{Title}##{UniqueInstanceGUID}",
                    ImGuiWindowFlags.AlwaysAutoResize |
                    ImGuiWindowFlags.NoSavedSettings |
                    ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.Popup);
            }

            if (didPopupWindow)
            {
                if (isTopMost)
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

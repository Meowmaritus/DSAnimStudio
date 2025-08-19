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
        public enum ResultTypes : byte
        {
            None = 0,
            Accept = 1 << 0,
            Cancel = 1 << 1,
            Refuse = 1 << 2,
        }
        
        [Flags]
        public enum InputFlag : byte
        {
            None = 0,
            EnterKeyToAccept = 1 << 0,
            EscapeKeyToCancel = 1 << 1,
            TitleBarXToCancel = 1 << 2,
        }

        private bool prevFrameBuiltWindow = false;
        public bool IsFirstFrameBuildingWindow = true;
        
        public readonly string GUID = Guid.NewGuid().ToString();

        public string Title;

        public System.Numerics.Vector2 LastPosition = new System.Numerics.Vector2(-2, -2);
        public System.Numerics.Vector2 LastSize = new System.Numerics.Vector2(2, 2);

        public RectF GetRect(bool subtractScrollBar = false)
        {
            float titleBarHeight = 18 * Main.DPI;
            float x = (LastPosition.X / Main.DPI);
            float y = ((LastPosition.Y + titleBarHeight) / Main.DPI);
            float w = ((LastSize.X - (subtractScrollBar ? (Main.ImGuiScrollBarPixelSize * Main.DPI) : 0)) / Main.DPI);
            float h = ((LastSize.Y - (subtractScrollBar ? (Main.ImGuiScrollBarPixelSize * Main.DPI) : 0) - titleBarHeight) / Main.DPI);

            return new RectF(x, y, w, h);
        }

        protected Dialog(string title)
        {
            Title = title;
        }

        public bool AllowsResultAccept
        {
            get => AllowsResultType(ResultTypes.Accept);
            set => AllowedResultTypes |= ResultTypes.Accept;
        }
        public bool AllowsResultCancel
        {
            get => AllowsResultType(ResultTypes.Cancel);
            set => AllowedResultTypes |= ResultTypes.Cancel;
        }
        
        public bool AllowsResultRefuse
        {
            get => AllowsResultType(ResultTypes.Refuse);
            set => AllowedResultTypes |= ResultTypes.Refuse;
        }

        public bool CancelHandledByInheritor = false;
        public bool AcceptHandledByInheritor = false;

        public bool EnterKeyToAccept
        {
            get => HasInputFlag(InputFlag.EnterKeyToAccept);
            set => InputFlags |= InputFlag.EnterKeyToAccept;
        }
        public bool EscapeKeyToCancel
        {
            get => HasInputFlag(InputFlag.EscapeKeyToCancel);
            set => InputFlags |= InputFlag.EscapeKeyToCancel;
        }
        public bool TitleBarXToCancel
        {
            get => HasInputFlag(InputFlag.TitleBarXToCancel);
            set => InputFlags |= InputFlag.TitleBarXToCancel;
        }

        public ResultTypes AllowedResultTypes = ResultTypes.None;

        public InputFlag InputFlags = InputFlag.EscapeKeyToCancel | InputFlag.TitleBarXToCancel;

        public bool IsTitleBarXRequested = false;

        public bool IsEscapeKeyRequested = false;
        public bool IsEnterKeyRequested = false;

        public bool AutoResize = true;
        public bool NoMove = false;
        public bool NoResize = false;

        public bool HasInputFlag(InputFlag type)
        {
            return (InputFlags & type) != 0;
        }

        public bool HasResult => ResultType != ResultTypes.None;
        public ResultTypes ResultType { get; private set; } = ResultTypes.None;

        public bool AllowsResultType(ResultTypes type)
        {
            return (AllowedResultTypes & type) != 0;
        }

        public bool IsDismissed { get; private set; }
        protected void Dismiss()
        {
            IsDismissed = true;
            Main.WinForm.Invoke(OnDismiss ?? (() => { }));
            //OnDismiss?.Invoke();

        }

        public Action OnDismiss;

        protected abstract void BuildInsideOfWindow();

        protected virtual void PreUpdate()
        {
            IsEscapeKeyRequested = false;
            IsEnterKeyRequested = false;
        }

        protected virtual void BeforeBuildWindow()
        {

        }

        public void Update(bool isTopMost)
        {
            PreUpdate();
            
            if (Title == null)
                throw new InvalidOperationException("Dialog title cannot be null.");
            
            
            if (isTopMost)
                ImGui.OpenPopup($"{Title}##{GUID}");

            bool isOpen = true;
            
            bool isUsingTitleBarX = TitleBarXToCancel && AllowsResultCancel;

            if (!isTopMost)
                ImGuiDebugDrawer.PushDisabled();

            var flags = ImGuiWindowFlags.NoSavedSettings |
                    ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.Popup;

            if (NoMove)
                flags |= ImGuiWindowFlags.NoMove;

            if (NoResize)
                flags |= ImGuiWindowFlags.NoResize;

            if (AutoResize)
                flags |= ImGuiWindowFlags.AlwaysAutoResize;

            bool didPopupWindow;
            if (isUsingTitleBarX)
                didPopupWindow = ImGui.BeginPopupModal($"{Title}##{GUID}", ref isOpen, flags);
            else
                didPopupWindow = ImGuiEx.BeginPopupModal($"{Title}##{GUID}", flags);
            

            bool buildWindowThisFrame = didPopupWindow;

            IsFirstFrameBuildingWindow = buildWindowThisFrame && !prevFrameBuiltWindow;

            IsEscapeKeyRequested = DialogManager.Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.Escape);
            IsEnterKeyRequested = DialogManager.Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.Enter);

            if (didPopupWindow)
            {
                if (buildWindowThisFrame)
                {
                    

                    BuildInsideOfWindow();

                    if (IsTitleBarXRequested)
                    {
                        IsTitleBarXRequested = false;
                    }
                    
                    
                }

                LastPosition = ImGui.GetWindowPos();
                LastSize = ImGui.GetWindowSize();

                ImGui.EndPopup();
            }
            
            if (!isTopMost)
                ImGuiDebugDrawer.PopDisabled();
            // else
            // {
            //     Console.WriteLine("what the fuck");
            // }

            //isOpen = ImGui.IsPopupOpen($"{Title}##{GUID}");
            
            if (!isOpen && isUsingTitleBarX && isTopMost)
            {
                IsTitleBarXRequested = true;
                if (!CancelHandledByInheritor)
                {
                    ResultType = ResultTypes.Cancel;
                    Dismiss();
                }
                
            }

            

            if (IsEscapeKeyRequested)
            {
                if (EscapeKeyToCancel && AllowsResultCancel && !CancelHandledByInheritor && isTopMost)
                {
                    ResultType = ResultTypes.Cancel;
                    Dismiss();
                }
            }

            if (IsEnterKeyRequested)
            {
                if (EnterKeyToAccept && AllowsResultAccept && !AcceptHandledByInheritor && isTopMost)
                {
                    ResultType = ResultTypes.Accept;
                    Dismiss();
                }
            }

            
            
            prevFrameBuiltWindow = buildWindowThisFrame;
        }
    }
}

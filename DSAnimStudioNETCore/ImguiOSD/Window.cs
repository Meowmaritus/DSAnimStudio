using DSAnimStudio.TaeEditor;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public static TaeEditorScreen Tae => Main.TAE_EDITOR;
        public abstract string Title { get; }
        public virtual string ImguiTag => Title;
        public bool IsOpen;
        public bool IsFirstFrameOpen { get; private set; } = false;
        private bool prevIsOpen;
        public virtual ImGuiWindowFlags Flags => ImGuiWindowFlags.None;

        protected virtual void PreUpdate()
        {

        }

        protected virtual void PostUpdate()
        {

        }

        protected abstract void BuildContents();
        public void Update()
        {
            IsFirstFrameOpen = IsOpen && !prevIsOpen;
            PreUpdate();
            if (IsOpen)
            {
                ImGui.Begin($"{Title}###Window|{ImguiTag}", ref IsOpen, Flags);
                BuildContents();
                ImGui.End();
            }
            PostUpdate();
            prevIsOpen = IsOpen;
        }
    }
}

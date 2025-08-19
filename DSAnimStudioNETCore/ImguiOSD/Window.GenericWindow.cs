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
        public class GenericWindow : Window
        {
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.NoSave;

            public readonly Action<GenericWindow> InitAction;
            public readonly Action<GenericWindow> BuildAction;

            public string Title;
            public override string NewImguiWindowTitle => Title;

            protected override void Init()
            {
                IsOpen = true;
                InitAction?.Invoke(this);
            }

            protected override void PreUpdate()
            {
                
            }

            protected override void PostUpdate()
            {
                
            }

            public GenericWindow(Action<GenericWindow> initAction, Action<GenericWindow> buildAction)
            {
                InitAction = initAction;
                BuildAction = buildAction;
            }

            protected override void BuildContents(ref bool anyFieldFocused)
            {
                BuildAction?.Invoke(this);
            }
        }
    }
}

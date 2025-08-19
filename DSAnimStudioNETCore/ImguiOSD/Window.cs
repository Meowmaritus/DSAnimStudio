using DSAnimStudio.TaeEditor;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        private readonly string GUID = Guid.NewGuid().ToString();

        public bool UniqueImguiInstanceNoSavedSettings = false;

        public enum SaveOpenStateTypes
        {
            NoSave,
            SaveOnlyInDebug,
            SaveAlways,
        }

        public abstract SaveOpenStateTypes GetSaveOpenStateType();

        public bool RequestOpenFromConfig = false;
        public bool RequestFocusFromConfig = false;

        public string ImguiTag => UniqueImguiInstanceNoSavedSettings ? $"{NewImguiWindowTitle}##{GUID}" : NewImguiWindowTitle;

        /// <summary>
        /// Use this to give an absolute Imgui tag that will be the same every time (and also will be saved in the imgui config file correctly)
        /// If this is not overrided to return something other than null, the GUID will be used as an imgui tag, which means the tag will
        /// change every time.
        /// </summary>
        //protected virtual string OverrideSingletonImguiTag => null;


        public static DSAProj MainProj => MainTaeScreen?.Proj;
        public static zzz_DocumentIns MainDoc => MainTaeScreen?.ParentDocument;

        public static TaeEditorScreen MainTaeScreen => Main.TAE_EDITOR;
        public bool IsOpen;
        private bool isFirstUpdateCycle = true;
        public bool IsFirstFrameOpen { get; private set; } = false;
        private bool prevIsOpen;
        public ImGuiWindowFlags Flags = ImGuiWindowFlags.None;

        public bool IsDockable = true;

        public System.Numerics.Vector4? CustomBorderColor_Focused = null;
        public System.Numerics.Vector4? CustomBorderColor_Unfocused = null;
        public System.Numerics.Vector4? CustomBackgroundColor = null;
        public System.Numerics.Vector4? CustomTextColor = null;

        public System.Numerics.Vector2? RequestWindowPosition = null;
        public System.Numerics.Vector2? RequestWindowSize = null;

        public bool ClipDockTabArea = true;

        public static float ApproximateTitleBarHeight => 18 * Main.DPI;
        
        public RectF GetRect(bool subtractScrollBar = false, bool subtractTitleBar = true)
        {
            float x = (LastPosition.X / Main.DPI);
            float y = ((LastPosition.Y + (subtractTitleBar ? ApproximateTitleBarHeight : 0)) / Main.DPI);
            float w = ((LastSize.X - (subtractScrollBar ? (Main.ImGuiScrollBarPixelSize * Main.DPI) : 0)) / Main.DPI);
            float h = ((LastSize.Y - (subtractScrollBar ? (Main.ImGuiScrollBarPixelSize * Main.DPI) : 0) - (subtractTitleBar ? ApproximateTitleBarHeight : 0)) / Main.DPI);

            return new RectF(x, y, w, h);
        }
        
        
        
        public System.Numerics.Vector2 LastPosition = new System.Numerics.Vector2(-2, -2);
        public System.Numerics.Vector2 LastSize = new System.Numerics.Vector2(2, 2);

        public bool Focused = false;
        public bool Hovered = false;

        public bool IsRequestFocus = false;

        public int RequestMaintainScrollFrames = 0;
        private float? RequestMaintainScrollFrames_DesiredScroll = null;
        private float? RequestScrollY = -1;
        
        protected abstract void Init();

        protected virtual void PreUpdate()
        {

        }

        protected virtual void PostUpdate()
        {

        }

        protected virtual void PreBuild()
        {

        }

        protected virtual void PostBuild()
        {

        }

        protected abstract void BuildContents(ref bool anyFieldFocused);

        public abstract string NewImguiWindowTitle { get; }

        public void Update(ref Window actualFocusedWindow, ref Window currentFocusedWindow, ref bool anyWindowHovered, ref bool anyFieldFocused)
        {
            Update(ref anyFieldFocused);
            if (Focused)
            {
                currentFocusedWindow = this;
                actualFocusedWindow = this;
            }

            if (Hovered)
                anyWindowHovered = true;
        }

        public void Update(ref Window actualFocusedWindow, ref bool anyFieldFocused)
        {
            // im crying and shitting irl
            bool anyFieldFocused_Inner = false;
            Update(ref anyFieldFocused_Inner);

            if (anyFieldFocused_Inner)
                anyFieldFocused = true;

            if (Focused)
                actualFocusedWindow = this;
        }
        
        public void Update(ref bool anyFieldFocused)
        {
            IsFirstFrameOpen = IsOpen && !prevIsOpen;

            if (isFirstUpdateCycle)
                Init();
            
            if (OSD.DebugIsForceAllStaticWindowsOpen && !UniqueImguiInstanceNoSavedSettings)
                IsOpen = true;


            if (RequestOpenFromConfig)
                IsOpen = true;

            PreUpdate();

            if (IsOpen)
            {
                if (RequestOpenFromConfig)
                    RequestOpenFromConfig = false;

                if (RequestFocusFromConfig)
                {
                    IsRequestFocus = true;
                }
            }
            
            if (Focused)
            {
                if (RequestFocusFromConfig)
                {
                    RequestFocusFromConfig = false;
                }
            }

            if (OSD.DebugIsForceAllStaticWindowsOpen && !UniqueImguiInstanceNoSavedSettings)
                IsOpen = true;
            
            IsFirstFrameOpen = IsOpen && !prevIsOpen;
            
            if (IsOpen)
            {
                ImGui.PushStyleColor(ImGuiCol.Border,
                    Focused ? (CustomBorderColor_Focused ?? new System.Numerics.Vector4(1,1,1,1)) : 
                        (CustomBorderColor_Unfocused ?? new System.Numerics.Vector4(0.5f, 0.43f, 0.43f, 0.5f)));

                var customBackgroundColorCopy = CustomBackgroundColor;
                var customTextColorCopy = CustomTextColor;
                
                //quick hotfix
                //customBackgroundColorCopy = new System.Numerics.Vector4(0, 0, 0, 0);
                
                if (customBackgroundColorCopy != null)
                    ImGui.PushStyleColor(ImGuiCol.WindowBg, customBackgroundColorCopy.Value);
                
                if (customTextColorCopy != null)
                    ImGui.PushStyleColor(ImGuiCol.Text, customTextColorCopy.Value);
                
                try
                {
                    var flags = Flags;
                    if (UniqueImguiInstanceNoSavedSettings)
                        flags |= ImGuiWindowFlags.NoSavedSettings;
                    if (OSD.DebugIsDockEditMode)
                    {
                        flags &= ~ImGuiWindowFlags.NoInputs;
                        flags &= ~ImGuiWindowFlags.NoMove;
                    }
                    else
                    {
                        flags |= ImGuiWindowFlags.NoMove;
                    }

                    if (IsDockable)
                    {
                        flags &= ~ImGuiWindowFlags.NoDocking;
                    }
                    else
                    {
                        flags |= ImGuiWindowFlags.NoDocking;
                    }

                    flags |= ImGuiWindowFlags.HorizontalScrollbar;



                    ImGui.Begin(ImguiTag, ref IsOpen, flags);
                    try
                    {
                        if (IsRequestFocus)
                        {
                            ImGui.SetWindowFocus();
                            IsRequestFocus = false;
                        }

                        if (!OSD.DebugIsDockEditMode && !ImGui.IsWindowDocked() && !Focused)
                            IsOpen = false;
                        
                        // if (IsFirstFrameOpen)
                        // {
                        //     Position = ImGui.GetWindowPos();
                        //     Size = ImGui.GetWindowSize();
                        // }
                        // else
                        // {
                        //     ImGui.SetWindowPos(Position);
                        //     ImGui.SetWindowSize(Size);
                        // }
                        
                        if (RequestWindowPosition.HasValue)
                            ImGui.SetWindowPos(RequestWindowPosition.Value);
                        RequestWindowPosition = null;
                        if (RequestWindowSize.HasValue)
                            ImGui.SetWindowSize(RequestWindowSize.Value);
                        RequestWindowSize = null;
                        
                        Focused = ImGui.IsWindowFocused();
                        Hovered = ImGui.IsWindowHovered();

                        var rect = GetRect().DpiScaled();

                        bool clipDockTabArea = ClipDockTabArea;

                        if (clipDockTabArea)
                            ImGui.PushClipRect(rect.TopLeftCornerN(), rect.BottomRightCornerN(), true);
                        
                        ImGui.PushItemWidth(OSD.DefaultItemWidth);
                        
                        PreBuild();

                        float startScrollY = ImGui.GetScrollY();
                        int prevRequestMaintainScrollFrames = RequestMaintainScrollFrames;

                        if (RequestScrollY.HasValue)
                        {
                            ImGui.SetScrollY(RequestScrollY.Value);
                        }
                        
                        BuildContents(ref anyFieldFocused);
                        PostBuild();
                        
                        ImGui.PopItemWidth();

                        if (clipDockTabArea)
                            ImGui.PopClipRect();
                        
                        if (RequestMaintainScrollFrames > 0)
                        {
                            // set this only when on first frame
                            if (prevRequestMaintainScrollFrames == 0 || RequestMaintainScrollFrames_DesiredScroll == null)
                                RequestMaintainScrollFrames_DesiredScroll = startScrollY;
                            
                            RequestScrollY = RequestMaintainScrollFrames_DesiredScroll ?? RequestScrollY;
                            
                            RequestMaintainScrollFrames--;
                        }
                        else
                        {
                            RequestMaintainScrollFrames_DesiredScroll = null;
                        }
                        
                        if (RequestScrollY.HasValue)
                        {
                            ImGui.SetScrollY(RequestScrollY.Value);
                            RequestScrollY = null;
                        }
                        
                        
                        LastPosition = ImGui.GetWindowPos();
                        LastSize = ImGui.GetWindowSize();
                    }
                    finally
                    {
                        ImGui.End();
                    }
                    
                   
                }
                finally
                {
                    if (customTextColorCopy != null)
                        ImGui.PopStyleColor();
                    
                    if (customBackgroundColorCopy != null)
                        ImGui.PopStyleColor();
                    
                    ImGui.PopStyleColor();
                }
                
            }
            else
            {
                Focused = false;
                Hovered = false;
            }
            PostUpdate();
            prevIsOpen = IsOpen;

            isFirstUpdateCycle = false;
        }
    }
}

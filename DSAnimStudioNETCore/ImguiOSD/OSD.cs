using DSAnimStudio.TaeEditor;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace DSAnimStudio.ImguiOSD
{
    public static class OSD
    {
        private static unsafe void LoadDefaults(ImGuiIOPtr io, DefaultLayoutTypes layoutType)
        {
            string iniFileName = $"{Main.Directory}\\imgui.ini";

            string layoutIniResourceName = null;

            if (layoutType is DefaultLayoutTypes.V5)
                layoutIniResourceName = "/EmbRes/imgui_default_V5.ini";
            else if (layoutType is DefaultLayoutTypes.Legacy)
                layoutIniResourceName = "/EmbRes/imgui_default_Legacy.ini";
            else
                throw new NotImplementedException();

            if (layoutIniResourceName == null)
                return;

            var iniText = Main.GetEmbeddedResourceText(layoutIniResourceName);

            File.WriteAllText(iniFileName, iniText);

            ImGui.LoadIniSettingsFromDisk(iniFileName);
        }

        public static TaeEditorScreen Tae => Main.TAE_EDITOR;

        public static TooltipManager TooltipManager_GraphActions = new TooltipManager();
        public static TooltipManager TooltipManager_AnimationList = new TooltipManager();
        public static TooltipManager TooltipManager_ListBoxPicker = new TooltipManager();
        public static TooltipManager TooltipManager_ColorPicker = new TooltipManager();
        public static TooltipManager TooltipManager_Scene = new TooltipManager();
        public static TooltipManager TooltipManager_Toolbox = new TooltipManager();
        
        public static List<WindowOpenStateEntry> GetWindowOpenStateEntries()
        {
            var result = new List<WindowOpenStateEntry>();
            CreateStaticWindowList();
            var staticWindows = StaticWindowList.ToList();

            foreach (var win in staticWindows)
            {
                var openStateSaveType = win.GetSaveOpenStateType();
                if (openStateSaveType == Window.SaveOpenStateTypes.SaveAlways || (openStateSaveType == Window.SaveOpenStateTypes.SaveOnlyInDebug && Main.IsDebugBuild))
                {

                    var entry = new WindowOpenStateEntry();
                    entry.WindowName = win.NewImguiWindowTitle;
                    entry.IsOpen = win.IsOpen;
                    entry.IsFocused = win.Focused;
                    result.Add(entry);

                }


            }
            return result;
        }

        public static void SetWindowOpenStatesFromConfig(List<WindowOpenStateEntry> entries)
        {
            CreateStaticWindowList();
            var staticWindows = StaticWindowList.ToList();

            foreach (var entry in entries)
            {
                var matchingWindow = staticWindows.FirstOrDefault(w => w.NewImguiWindowTitle == entry.WindowName);
                if (matchingWindow != null)
                {
                    if (entry.IsOpen)
                        matchingWindow.RequestOpenFromConfig = true;
                    if (entry.IsFocused)
                        matchingWindow.RequestFocusFromConfig = true;
                }
            }
        }

        public static void ForAllTooltipManagers(Action<TooltipManager> doAction)
        {
            doAction(TooltipManager_GraphActions);
            doAction(TooltipManager_AnimationList);
            doAction(TooltipManager_ListBoxPicker);
            doAction(TooltipManager_ColorPicker);
            doAction(TooltipManager_Scene);
            doAction(TooltipManager_Toolbox);
        }
        
        public static bool RequestCollapse = true;
        public static bool RequestExpandAllTreeNodes = true;
        public static bool DummyPolyListOpen = false;
        public static bool RenderConfigOpen = false;
        public static bool AnyFieldFocused;
        public static bool Focused;
        public static Window FocusedWindow = null;
        public static Window ActualFocusedWindow = null;
        public static bool Hovered;

        private static int focusedHist = 0;
        private static int hoveredHist = 0;

        public static Vector2 QueuedWindowPosChange = Vector2.Zero;
        public static Vector2 QueuedWindowSizeChange = Vector2.Zero;
        public static float RenderScale = 1;
        public static float WidthScale = 1;
        public static float RenderScaleTarget = 100;
        public static float WidthScaleTarget = 100;

        public static float DocManToolbarOffset => 20 + Main.DPI;
        public static float DocManToolbarHeight => 4 * Main.DPI;

        public static int DefaultItemWidth => (int)Math.Round(128 * Main.DPI * OSD.RenderScale * OSD.WidthScale);
        
        public static bool EnableDebugMenu => Main.IsDebugBuild;
        public static bool EnableDebug_QuickDebug => Main.Debug.EnableImGuiDebugMenu_QuickDebug;
        
        public static bool IsInit { get; private set; } = true;


        public static bool DebugIsDockEditMode => Main.Debug.EnableImguiDebugDockEdit;
        public static bool DebugIsForceAllStaticWindowsOpen => Main.Debug.EnableImguiDebugForceAllStaticWindowsOpen;

        private static object _lock_StaticWindowList = new object();
        private static List<Window> StaticWindowList = null;
        private static void CreateStaticWindowList()
        {
            lock (_lock_StaticWindowList)
            {
                if (StaticWindowList == null)
                {
                    StaticWindowList = new List<Window>();
                    var staticFields = typeof(OSD).GetFields(BindingFlags.Public | BindingFlags.Static);
                    foreach (var f in staticFields)
                    {
                        if (f.GetValue(null) is Window staticWindow)
                            StaticWindowList.Add(staticWindow);
                    }
                }
            }
        }

        public static void ForAllStaticWindows(Action<Window> doAction)
        {
            CreateStaticWindowList();
            lock (_lock_StaticWindowList)
            {
                if (StaticWindowList != null)
                {
                    foreach (var w in StaticWindowList)
                        doAction(w);
                }
            }
        }
        

        public static Window.Debug WindowDebug = new Window.Debug();
        public static Window.Equipment WindowEquipment = new Window.Equipment();
        public static Window.Help WindowHelp = new Window.Help();
        public static Window.SceneWindow WindowScene = new Window.SceneWindow();
        public static Window.Toolbox WindowToolbox = new Window.Toolbox();
        public static Window.Entity WindowEntity = new Window.Entity();
        public static Window.Sound WindowSound = new Window.Sound();


        public static Window.Parameters SpWindowParameters = new Window.Parameters();
        //public static Window.Animations SpWindowAnimations = new Window.Animations();
        public static Window.Graph SpWindowGraph = new Window.Graph();
        public static Window.Viewport SpWindowViewport = new Window.Viewport();
        public static Window.NewTransport SpWindowNewTransport = new Window.NewTransport();
        
        public static Window.Graph_Tracks SpWindowGraph_Tracks = new Window.Graph_Tracks();
        public static Window.Graph_Actions SpWindowGraph_Actions = new Window.Graph_Actions();
        
        public static Window.ERRORS SpWindowERRORS = new Window.ERRORS();
        public static Window.Notifications SpWindowNotifications = new Window.Notifications();

        public static Window.DocManToolbar SpWindowDocManToolbar = new Window.DocManToolbar();



        public static Window.NewComboViewer WindowComboViewer = new Window.NewComboViewer();
        public static Window.Find WindowFind = new Window.Find();

        public static Window.Project WindowProject = new Window.Project();


        public static int AuxFocus = 0;
        public static void PreUpdate()
        {
            AuxFocus = 0;
        }

        public static void SetAuxFocus()
        {
            AuxFocus += 2;
        }


        private static object _lock_TempWindowList = new object();
        private static List<Window> TempWindowList = new List<Window>();

        private static Window RequestBringToFrontWindow = null;
        public static void RequestWindowBringToFront(Window window)
        {
            RequestBringToFrontWindow = window;
        }

        public static void RegistTempWindow(Window tempWindow)
        {
            lock (_lock_TempWindowList)
            {
                tempWindow.IsOpen = true;
                if (!TempWindowList.Contains(tempWindow))
                    TempWindowList.Add(tempWindow);
            }
        }

        public static void ClearTempWindows()
        {
            lock (_lock_TempWindowList)
            {
                TempWindowList.Clear();
            }
        }

        private static void UpdateTempWindows(ref Window actualFocusedWindow, ref Window currentFocusedWindow, ref bool anyWindowHovered, ref bool anyFieldFocused)
        {
            lock (_lock_TempWindowList)
            {
                var tempWindowsToDelete = new List<Window>();
                foreach (var tw in TempWindowList)
                {
                    tw.Update(ref actualFocusedWindow, ref currentFocusedWindow, ref anyWindowHovered, ref anyFieldFocused);
                    if (!tw.IsOpen)
                        tempWindowsToDelete.Add(tw);
                }

                foreach (var tw in tempWindowsToDelete)
                {
                    if (TempWindowList.Contains(tw))
                        TempWindowList.Remove(tw);
                }
            }
        }

        public static void NewGenericWindow(Action<Window.GenericWindow> initAction, Action<Window.GenericWindow> buildAction)
        {
            var newWindow = new Window.GenericWindow(initAction, buildAction);
            newWindow.UniqueImguiInstanceNoSavedSettings = true;
            RegistTempWindow(newWindow);
        }

        public static void AllTooltipManagers_MouseMove()
        {
            ForAllTooltipManagers(t => t.MouseMove());
        }

        

        public static void PostBuild(float elapsedTime, float offsetX, float offsetY)
        {
            ForAllTooltipManagers(t => t.PostUpdate(elapsedTime, offsetX, offsetY));
        }
        
        public enum DefaultLayoutTypes
        {
            None,
            V5,
            Legacy
        }

        public static bool RequestShowWelcome = false;

        public static DefaultLayoutTypes RequestLoadDefaultLayout = DefaultLayoutTypes.None;

        //public static bool RequestSaveLayout = false;

        //private static unsafe void SaveLayout(ImGuiIOPtr io)
        //{
        //    io->IniFilename
        //}

        public static void Build(float elapsedTime, float offsetX, float offsetY)
        {
            var io = ImGui.GetIO();
            //io.FontGlobalScale = Main.DPI;

            if (RequestLoadDefaultLayout != DefaultLayoutTypes.None)
            {
                LoadDefaults(io, RequestLoadDefaultLayout);
                RequestLoadDefaultLayout = DefaultLayoutTypes.None;
            }

            //if (RequestSaveLayout)
            //{
            //    SaveLayout(io);
            //    RequestSaveLayout = false;
            //}

            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

            io.ConfigWindowsMoveFromTitleBarOnly = true;
            io.ConfigWindowsResizeFromEdges = true;

            bool disableEntireGuiThisFrame = Application.OpenForms.Count > 1;

            if (disableEntireGuiThisFrame)
                ImGuiDebugDrawer.PushDisabled(true);

            
            
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(4, 0));
            ImGui.PushStyleVar(ImGuiStyleVar.TabRounding, 0);
            try
            {

                // DOCKING TEST
                // ImGuiViewportPtr vp = ImGui.GetMainViewport();
                // ImGui.SetNextWindowPos(vp.Pos + new System.Numerics.Vector2(0, 24));
                // ImGui.SetNextWindowSize(vp.Size + new System.Numerics.Vector2(0, -24));
                // ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
                // ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
                // ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
                // ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse |
                //                          ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
                // flags |= ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.MenuBar;
                // flags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;
                // flags |= ImGuiWindowFlags.NoBackground;
                // ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
                // if (ImGui.Begin("DockSpace_W", flags))
                // {
                // }

                ImGuiViewportPtr viewport = ImGui.GetMainViewport();
                var vpWorkSize = viewport.WorkSize;
                var vpWorkPos = viewport.WorkPos;

                ImGui.SetNextWindowPos(new Vector2(vpWorkPos.X, vpWorkPos.Y + DocManToolbarHeight + DocManToolbarOffset));
                ImGui.SetNextWindowSize(new Vector2(vpWorkSize.X, vpWorkSize.Y - DocManToolbarHeight - DocManToolbarOffset));

                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

                ImGui.Begin("MainDockSpace###Window_ForMainDockSpace", 
                    ImGuiWindowFlags.NoDocking | 
                    ImGuiWindowFlags.NoTitleBar | 
                    ImGuiWindowFlags.NoCollapse |
                    ImGuiWindowFlags.NoResize | 
                    ImGuiWindowFlags.NoMove | 
                    ImGuiWindowFlags.NoBringToFrontOnFocus |
                    ImGuiWindowFlags.NoNavFocus | 
                    ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration);

                var dsid = ImGui.GetID("MainDockSpace");

                var dockNodeFlags = ImGuiDockNodeFlags.None;

                //dockNodeFlags = Main.Debug.EnableImguiDebugDockEdit ? ImGuiDockNodeFlags.None : ImGuiDockNodeFlags.NoDockingInCentralNode;
                dockNodeFlags = ImGuiDockNodeFlags.None;

                ImGui.DockSpace(dsid, new Vector2(0, 0), dockNodeFlags);

                ImGui.End();

                ImGui.PopStyleVar();


                
                // ImGui.PopStyleVar();
                // ImGui.PopStyleVar();
                // ImGui.PopStyleVar();
                //
                // ImGui.End();
                //





                ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 2.0f);
                ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4));


                bool anyFieldFocused = false;

                DialogManager.UpdateDialogs();

                if (ImGui.BeginMainMenuBar())
                {
                    bool menuBarFocused = false;
                    MenuBar.BuildMenuBar(ref menuBarFocused);
                    if (menuBarFocused)
                        anyFieldFocused = true;
                    var playbackCursor = Main.TAE_EDITOR?.PlaybackCursor;
                    var transport = Main.TAE_EDITOR?.Transport;
                    if (transport != null)
                        transport.PlaybackCursor = playbackCursor;
                    Main.TAE_EDITOR?.Transport?.Draw(GFX.Device, null, null, null);
                    ImGui.EndMainMenuBar();
                }

                if (RequestShowWelcome)
                {
                    DialogManager.ShowWelcome();
                    RequestShowWelcome = false;
                }

                ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.15f, 0.15f, 0.15f, Focused ? 1 : 0.65f));
                // Test - completely transparent windows
                //ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0f,0, 0f, 1));

                ForAllTooltipManagers(t => t.PreUpdate(elapsedTime, offsetX, offsetY));
                
                bool anyWindowHovered = false;
                
                Window currentFocusedWindow = null;
                Window actualFocusedWindow = null;
                
                
                
                lock (_lock_StaticWindowList)
                {
                    
                    
                    WindowDebug.Update(ref actualFocusedWindow, ref anyFieldFocused);
                    WindowEquipment.Update(ref actualFocusedWindow, ref currentFocusedWindow, ref anyWindowHovered,
                        ref anyFieldFocused);
                    WindowHelp.Update(ref actualFocusedWindow, ref anyFieldFocused);
                    WindowScene.Update(ref actualFocusedWindow, ref currentFocusedWindow, ref anyWindowHovered,
                        ref anyFieldFocused);
                    WindowToolbox.Update(ref actualFocusedWindow, ref currentFocusedWindow, ref anyWindowHovered,
                        ref anyFieldFocused);
                    WindowEntity.Update(ref actualFocusedWindow, ref anyFieldFocused);
                    
                    WindowSound.Update(ref actualFocusedWindow, ref currentFocusedWindow, ref anyWindowHovered,
                            ref anyFieldFocused);
                    
                    SpWindowParameters.Update(ref actualFocusedWindow, ref currentFocusedWindow, ref anyWindowHovered,
                        ref anyFieldFocused);
                    
                    
                    zzz_DocumentManager.CurrentDocument?.SpWindowAnimations.Update(ref actualFocusedWindow, ref anyFieldFocused);
                    
                    SpWindowViewport.Update(ref actualFocusedWindow, ref anyFieldFocused);
                    SpWindowNewTransport.Update(ref actualFocusedWindow, ref anyFieldFocused);

                    SpWindowERRORS.Update(ref actualFocusedWindow, ref anyFieldFocused);

                    SpWindowGraph.Update(ref actualFocusedWindow, ref anyFieldFocused);


                    SpWindowGraph_Tracks.Update(ref actualFocusedWindow, ref anyFieldFocused);
                    SpWindowGraph_Actions.Update(ref actualFocusedWindow, ref anyFieldFocused);

                    SpWindowNotifications.Update(ref actualFocusedWindow, ref anyFieldFocused);

                    WindowComboViewer.Update(ref actualFocusedWindow, ref currentFocusedWindow, ref anyWindowHovered,
                        ref anyFieldFocused);

                    WindowFind.Update(ref actualFocusedWindow, ref currentFocusedWindow, ref anyWindowHovered,
                        ref anyFieldFocused);
                    WindowProject.Update(ref actualFocusedWindow, ref currentFocusedWindow, ref anyWindowHovered,
                        ref anyFieldFocused);

                    SpWindowDocManToolbar.Update(ref actualFocusedWindow, ref currentFocusedWindow, ref anyWindowHovered,
                        ref anyFieldFocused);
                }

                // ForAllStaticWindows(window =>
                // {
                //     ImGuiDebugDrawer.DrawText(window.ImguiWindowNameKey, window.LastPosition, Color.Yellow);
                //     ImGuiDebugDrawer.DrawRect(window.LastPosition, window.LastSize, Color.Yellow, filled: false);
                // });

                UpdateTempWindows(ref actualFocusedWindow, ref currentFocusedWindow, ref anyWindowHovered, ref anyFieldFocused);

                AnyFieldFocused = anyFieldFocused;

                if (AuxFocus > 0)
                {
                    AnyFieldFocused = true;
                    AuxFocus--;
                }

                if (IsInit)
                {
                    ImGui.SetWindowFocus(null);
                }



                ImGui.PopStyleColor();
                ImGui.PopStyleVar(4);

                IsInit = false;

                RequestExpandAllTreeNodes = false;
                RequestCollapse = false;


                //bool anyWindowFocused = ImGui.IsWindowFocused(ImGuiFocusedFlags.AnyWindow);
                //bool anyWindowHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow);

                FocusedWindow = currentFocusedWindow;
                ActualFocusedWindow = actualFocusedWindow;

                var isFocused = ImGui.IsAnyItemFocused() || currentFocusedWindow != null ||
                                DialogManager.AnyDialogsShowing;


                var isHovered = anyWindowHovered || ImGui.IsAnyItemHovered();

                if (isFocused)
                    focusedHist = 2;

                if (isHovered)
                    hoveredHist = 2;

                Focused = focusedHist > 0;
                Hovered = hoveredHist > 0;

                if (focusedHist > 0)
                    focusedHist--;

                if (hoveredHist > 0)
                    hoveredHist--;

                //Focused = io.WantCaptureKeyboard;
                //Hovered = io.WantCaptureMouse;

                if (Focused || Hovered)
                {
                    Main.Input.CursorType = MouseCursorType.Arrow;
                }
                
                Main.TAE_EDITOR?.Transport?.Draw(GFX.Device, null, null, null);

                if (disableEntireGuiThisFrame)
                    ImGuiDebugDrawer.PopDisabled();
            }
            finally
            {
                ImGui.PopStyleVar(2);
            }
        }
    }
}

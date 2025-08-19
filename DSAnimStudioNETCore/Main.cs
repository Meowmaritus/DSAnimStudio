using DSAnimStudio.DbgMenus;
using DSAnimStudio.GFXShaders;
using DSAnimStudio.ImguiOSD;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Reflection;
using System.Xml;
using DSAnimStudio.TaeEditor;
using System.Runtime;

namespace DSAnimStudio
{
    public class Main : Game
    {
        // STOP MOVING THESE FIELDS DOWN LMAO
        public const string DSAS_VERSION_STRING = "5.0-RC4.1";
        public static bool IsPatreonBuild => false;


        private static bool NeedsShowWelcome = true;

        public static void InitTaeEditor()
        {
            //zzz_DocumentManager.CurrentDocument.EditorScreen = new TaeEditor.TaeEditorScreen((Form)Form.FromHandle(Program.MainInstance.Window.Handle));
        }

        public static TaeEditorScreen TAE_EDITOR => zzz_DocumentManager.CurrentDocument?.EditorScreen;

#if DEBUG
        public static bool IsDebugBuild => true;
        public static bool IsNightfallBuild => false;
#else
        public static bool IsDebugBuild => false;
        public static bool IsNightfallBuild => false;
#endif
        public static TimeSpan TIME = TimeSpan.FromMilliseconds(0);

        public static HelperDrawConfig HelperDraw = HelperDrawConfig.Default;

        private static void UpdateTime(TimeSpan elapsedTime)
        {
            TIME += elapsedTime;
        }

        public static float GetPulseLerpS(float cycleLength)
        {
            return (float)Math.Sin(Math.PI * ((TIME.TotalSeconds / cycleLength) % 1));
        }

        public static Color PulseLerpColor(Color a, Color b, float cycleLength)
        {
            return Color.Lerp(a, b, GetPulseLerpS(cycleLength));
        }

        //private static bool DEFAULT_ERROR_HANDLER_TOGGLE_FOR_DEBUG = true;

        public static void HandleError(string errorName, Exception ex)
        {
            string msg = $"ERROR HANDLED: {errorName}\n\n{ex}";
            ErrorLog.LogError(msg);
            zzz_NotificationManagerIns.PushNotification(msg, color: Color.Red);
        }
        
        public static DPC_EnableErrorHandler EnableErrorHandler = new DPC_EnableErrorHandler();
        public static DPC_Debug Debug = new DPC_Debug();
        
        public class DPC_EnableErrorHandler : DebugPropertiesContainer<DPC_EnableErrorHandler>
        {
            public bool MainInit;
            public bool MainUpdateLoop = true;
            public bool HardReset = true;
            public bool LoadTaeTemplate = true;
            public bool ApplyTaeTemplate = true;
            public bool ParamManager = true;
            public bool LoadHKX = true;
            public bool ModelSkeletonRemapper = true;
            public bool DoDrawStep = true;
            public bool NewChrAsmUpdate = true;
            public bool AC6NpcPartsUpdate = true;
            public bool TaeFileContainerLoad = true;
            public bool TextureFetchRequest = true;
            public bool ActionParametersWrite = true;
            public bool ActionSimUpdate = true;
            public bool ActionSimUpdate_Inner = true;
            public bool ViewportStatusDisplay = true;
            public bool SoundUpdate = true;
            public bool ReadInternalSimField = true;
            public bool AnimListBuild = true;
            public bool DrawTransport = true;
            public bool SceneWindowModelNode = true;
            public bool WriteConfigFile = true;
            public bool ReadConfigFile = true;
            public bool ModalFormFocus = true;
            public bool AnimContainer_LoadBaseANIBND = true;
            public bool NewChrAsm_Draw_AfterAnimUpdateCall = true;
            public bool ModelDraw = true;
            public bool NewMeshDraw = true;
            public bool HavokBoneGluer = true;
            public bool LoadingTask = true;
            public bool ParameterWindow = true;
        }

        public class DPC_Debug : DebugPropertiesContainer<DPC_Debug>
        {
            public bool EnableImGuiDebugMenu_QuickDebug;
            public bool EnableImguiDebugDockEdit;
            public bool EnableImguiDebugForceAllStaticWindowsOpen;
            public bool EnableImguiDebugListAllStaticWindows;
            public bool EnableGraphDebug;
            public bool EnableViewportAnimLayerDebug;
            public bool AnimSlotDisableBlend;
            public NewAnimSlot.SlotTypes DebugSoloSlotType;
            public bool EnableAnimListDebug;
            public bool DisableAnimListCulling;
            public bool InputPrintTrueKeys;
            public bool InputPrintTrueKeys_DownOnly;
            public bool InputPrintTrueKeys_UpOnly;
            public bool HavokBoneGluerPrintEachGlue;
            public bool BreakOnBadBoneFKMatrixWrite;
            public bool GenTrackNamesIncludeExtraInfo;
            public bool EnableTaeTemplateAssert;
            public bool EnableImGuiFocusDebug;
        }

        public static string DebugXmlPath => $"{Directory}\\DEBUG.xml"; 
        
        public static void DebugToggleXmlRead()
        {
            if (IsDebugBuild)
            {
                if (File.Exists(DebugXmlPath))
                {
                    try
                    {
                        var xml = new XmlDocument();
                        xml.Load(DebugXmlPath);
                        XmlNode rootNode = xml.SelectSingleNode("DEBUG");
                        EnableErrorHandler.ReadXmlElement(rootNode);
                        Debug.ReadXmlElement(rootNode);
                    }
                    catch (Exception ex)
                    {
                        Main.HandleError("Debug XML Read", ex);
                    }
                }
            }
        }

        public static void DebugToggleXmlWrite()
        {
            if (IsDebugBuild)
            {
                using (var testStream = new MemoryStream())
                {
                    using (var writer = XmlWriter.Create(testStream, new XmlWriterSettings()
                           {
                               Indent = true,
                               Encoding = System.Text.Encoding.ASCII,
                           }))
                    {
                        writer.WriteStartDocument();
                        {
                            writer.WriteStartElement("DEBUG");
                            {
                                EnableErrorHandler.WriteXmlElement(writer);
                                Debug.WriteXmlElement(writer);
                            }
                            writer.WriteEndElement();
                        }
                        writer.WriteEndDocument();
                    }

                    File.WriteAllBytes(DebugXmlPath, testStream.ToArray());
                }
            }
        }

        public static void BuildDebugToggleImgui()
        {
            if (IsDebugBuild)
            {
                EnableErrorHandler.BuildImguiWidget("[ERROR HANDLERS]");
                Debug.BuildImguiWidget("[DEBUG FLAGS]");
            }
        }
        
        public static void DebugToggleUpdate()
        {
            if (!IsDebugBuild)
            {
                EnableErrorHandler.SetAllToggles(true);
                Debug.SetAllToggles(false);
            }
        }

        // public static void DebugToggleInit()
        // {
        //     if (IsDebugBuild)
        //     {
        //         EnableErrorHandler.SetAllToggles(DEFAULT_ERROR_HANDLER_TOGGLE_FOR_DEBUG);
        //     }
        //     else
        //     {
        //         
        //     }
        //
        //     DebugToggleXmlRead();
        // }

        
        
    
        public static byte[] BasicEffectBytecode;
        public static byte[] SpriteEffectBytecode;

        
        public static byte[] GetEmbeddedResourceBytes(string resourcePath)
        {
            resourcePath = resourcePath.Trim().Trim('/');
            resourcePath = $"{nameof(DSAnimStudio)}.{resourcePath.Replace("/", ".")}";

            byte[] result = null;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    result = reader.ReadBytes((int)stream.Length);
                }
            }
            return result;
        }

        public static string GetEmbeddedResourceText(string resourcePath)
        {
            resourcePath = resourcePath.Trim().Trim('/');
            resourcePath = $"{nameof(DSAnimStudio)}.{resourcePath.Replace("/", ".")}";
            string result = null;

            var ass = Assembly.GetExecutingAssembly();
            //var debug_ResNames = ass.GetManifestResourceNames();

            using (Stream stream = ass.GetManifestResourceStream(resourcePath))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }

        

        public const double TAE_FRAME_30 = 1.0 / 30.0;
        public const double TAE_FRAME_60 = 1.0 / 60.0;

        public static void GCCollect()
        {
            Task.Run(() =>
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false);
            });
        }

        

        public class LazyDispatchAction
        {
            public Action Act;
            public int GhettoTimer = 3;
            public bool WaitForAllLoadingTasks = false;
        }
        
        public class LazyDispatchTask
        {
            public Task Act;
            public int GhettoTimer = 3;
            public bool WaitForAllLoadingTasks = false;
        }

        private static object _lock_LazyDispatch = new object();
        private static Queue<LazyDispatchAction> LazyDispatchActionQueue = new Queue<LazyDispatchAction>();
        private static Queue<LazyDispatchTask> LazyDispatchTaskQueue = new Queue<LazyDispatchTask>();
        private static List<Action> LazyDispatchASAPActionList = new List<Action>();
        public static void MainThreadLazyDispatch(Action act, int frameDelay = 0, bool waitForAllLoadingTasks = false)
        {
            lock (_lock_LazyDispatch)
            {
                LazyDispatchActionQueue.Enqueue(new LazyDispatchAction() 
                { 
                    Act = act, 
                    GhettoTimer = frameDelay,
                    WaitForAllLoadingTasks = waitForAllLoadingTasks,
                });
            }
        }
        
        public static LazyDispatchTask MainThreadLazyDispatchTask(Task task, int frameDelay = 0, bool waitForAllLoadingTasks = false)
        {
            LazyDispatchTask t = null;
            lock (_lock_LazyDispatch)
            {
                t = new LazyDispatchTask()
                {
                    Act = task,
                    GhettoTimer = frameDelay,
                    WaitForAllLoadingTasks = waitForAllLoadingTasks,
                };
                LazyDispatchTaskQueue.Enqueue(t);
            }
            
            // UpdateLazyDispatch(MenuBar.IsAnyMenuOpen);
            // UpdateLazyDispatch(MenuBar.IsAnyMenuOpen);
            // UpdateLazyDispatch(MenuBar.IsAnyMenuOpen);
                
            return t;
        }

        public static void MainThreadNextFrameDispatch(Action act)
        {
            lock (_lock_LazyDispatch)
            {
                LazyDispatchASAPActionList.Add(act);
            }
        }

        private static void UpdateLazyDispatch(bool menusOpen)
        {
            List<Action> asapActionListCopy = null;
            Action nextLazyAction = null;
            lock (_lock_LazyDispatch)
            {
                
                asapActionListCopy = LazyDispatchASAPActionList.ToList();
                // Clear while locked to prevent desync
                LazyDispatchASAPActionList.Clear();

                if (!menusOpen)
                {
                    if (LazyDispatchActionQueue.Count > 0)
                    {
                        var nextLazy = LazyDispatchActionQueue.Peek();

                        if (!nextLazy.WaitForAllLoadingTasks || !zzz_DocumentManager.AnyDocumentWithANYLoadingTasks())
                        {
                            nextLazy.GhettoTimer--;

                            if (nextLazy.GhettoTimer <= 0)
                            {
                                nextLazyAction = LazyDispatchActionQueue.Dequeue().Act;
                            }
                        }
                    }
                }

                if (nextLazyAction == null && LazyDispatchTaskQueue.Count > 0)
                {
                    var nextLazy = LazyDispatchTaskQueue.Peek();

                    if (!nextLazy.WaitForAllLoadingTasks || !zzz_DocumentManager.AnyDocumentWithANYLoadingTasks())
                    {
                        nextLazy.GhettoTimer--;

                        if (nextLazy.GhettoTimer <= 0)
                        {
                            var nextTask = LazyDispatchTaskQueue.Dequeue();
                            nextLazyAction = () =>
                            {
                                nextTask.Act.RunSynchronously();
                            };
                        }
                    }
                }
                
            }

            foreach (var act in asapActionListCopy)
            {
                act?.Invoke();
            }

            if (nextLazyAction != null)
            {
                nextLazyAction?.Invoke();
            }
        }


        

        public static void Invoke(Action doStuff)
        {
            WinForm.Invoke(doStuff);
        }

        //public static string ImGuiFontName => $@"{Directory}\Content\Fonts\NotoSansCJKjp-Medium.otf";
        //public static float ImGuiFontGlyphMinAdvanceX = 5.0f;
        //public static int ImGuiFontOversampleH = 5;
        //public static int ImGuiFontOversampleV = 5;
        //public static bool ImGuiFontPixelSnapH = true;
        //public static float ImGuiFontPixelSize = 18.0f;

        public static string ImGuiFontName => $@"{Directory}\Content\Fonts\NotoSansMonoCJKjp-Bold.otf";
        public static float ImGuiFontGlyphMinAdvanceX = 5.0f;
        public static int ImGuiFontOversampleH = 1;
        public static int ImGuiFontOversampleV = 1;
        public static bool ImGuiFontPixelSnapH = true;
        public const float ImGuiFontPixelSize = 19.0f;
        public const float ImGuiScrollBarPixelSize = 20f;


        

        public static bool WindowShown = false;

        static object _lock_ChangeRenderTarget = new object();
        static System.Threading.Tasks.Task renderTargetBuildTask = null;

        public static int WindowResizeCommitTimer = 0;
        public static int WindowResizeCommitTimerMax = 20;
        public static bool HasUncommittedWindowResize => WindowResizeCommitTimer < WindowResizeCommitTimerMax;
        public Rectangle BoundsLastUpdatedFor;
        Rectangle prevWinBounds;

        public static void OnClosing()
        {
            zzz_DocumentManager.DestroyAllDocs();
        }

        void UpdateWindowResizeStable()
        {
            var bounds = Window.ClientBounds;

            var windowMinimized = bounds.Width < 1000 || bounds.Height < 500;

            void doActualUpdate()
            {
                if (IgnoreSizeChanges)
                    return;

                WindowResizeCommitTimer = WindowResizeCommitTimerMax;

                RequestHideOSD = RequestHideOSD_MAX;
                UpdateActiveState();
                
                LastBounds = bounds;
                lastActualBounds = bounds;

                BoundsLastUpdatedFor = bounds;
            }

            if (!WindowShown)
            {

                if (!windowMinimized)
                {
                    WindowShown = true;
                    // TAE_EDITOR.Rect = new Rectangle(0, 0, bounds.Width, bounds.Height - 2);
                    // TAE_EDITOR.DefaultLayout();

                    doActualUpdate();

                    //ImguiOSD.DialogManager.DialogOK("Welcome", "Thank you for being a Patreon supporter.\n\n" +
                    //    "This build has only been tested on Elden Ring. Past games might be broken.\nAdditionally, most textures will not work.\n" +
                    //    "However, it runs better than old versions and has some bugfixes." +
                    //    "\n\nSpecial thanks: " +
                    //    "\n    Skyth (blueskythlikesclouds on GitHub) - Created Havoc library which allows this application to load Havok tagfiles natively without downgrading." +
                    //    "\n    lingsamuel - Added some rough but perfectly functional Havok 2018.1 support to Havoc for Elden Ring animation support." +
                    //    "\n\nPlease enjoy and report any bugs to my Discord server, the link to which can be found under 'Help' in the menu bar.");
                }
            }
            else
            {

                //Console.WriteLine($"bounds:[{bounds.X},{bounds.Y},{bounds.Width},{bounds.Height}]");

                if (prevWinBounds != bounds || windowMinimized)
                {
                    WindowResizeCommitTimer = 0;
                }

                if (WindowResizeCommitTimer >= WindowResizeCommitTimerMax)
                {
                    if (bounds != BoundsLastUpdatedFor)
                    {
                        doActualUpdate();
                    }
                }
                else
                {
                    WindowResizeCommitTimer++;
                }

                prevWinBounds = bounds;
            }
        }

        public static T ReloadMonoGameContent<T>(string path, string origPath)
        {
            path = Path.GetFullPath(path);

            if (path.ToLower().EndsWith(".xnb"))
                path = path.Substring(0, path.Length - 4);
            MainContentManager.UnloadAsset(origPath);
            MainContentManager.UnloadAsset(origPath.Replace("\\", "/"));
            MainContentManager.UnloadAsset(path);
            MainContentManager.UnloadAsset(path.Replace("\\", "/"));
            return MainContentManager.Load<T>(path);
        }

        public static void CenterForm(Form form)
        {
            var x = Main.WinForm.Location.X + (Main.WinForm.Width - form.Width) / 2;
            var y = Main.WinForm.Location.Y + (Main.WinForm.Height - form.Height) / 2;
            form.Location = new System.Drawing.Point(Math.Max(x, 0), Math.Max(y, 0));
        }

        protected override void Dispose(bool disposing)
        {
            DestroyImguiFont();
            
            WindowsMouseHook.Unhook();

            RemoManager.DisposeAllModels();

            zzz_DocumentManager.DestroyAllDocs();

            base.Dispose(disposing);
        }

        public static bool IgnoreSizeChanges = false;

        public static Rectangle LastBounds = Rectangle.Empty;
        private static Rectangle lastActualBounds = Rectangle.Empty;

        public static ColorConfig Colors = new ColorConfig();

        public static Form WinForm;
        
        public static bool WinFormDisposed()
        {
            return WinForm?.IsDisposed != false;
        }
        
        public static float DPICustomMult = 1;

        private static float BaseDPI = 1;

        public static float DPI => BaseDPI * DPICustomMult;

        public static Vector2 DPIVector => new Vector2(DPI, DPI);
        public static System.Numerics.Vector2 DPIVectorN => new System.Numerics.Vector2(DPI, DPI);

        public static Matrix DPIMatrix => Matrix.CreateScale(DPI, DPI, 1);

        public static FancyInputHandler Input;

        public const int ConfigFileIOMaxTries = 10;

        public static bool NeedsToLoadConfigFileForFirstTime { get; private set; } = true;

        public static bool DisableConfigFileAutoSave = false;

        private static object _lock_actualConfig = new object();
        private static TaeEditor.TaeConfigFile actualConfig = new TaeEditor.TaeConfigFile();
        public static TaeEditor.TaeConfigFile Config
        {
            get
            {
                TaeEditor.TaeConfigFile result = null;
                lock (_lock_actualConfig)
                {
                    result = actualConfig;
                }
                return result;
            }
            set
            {
                lock (_lock_actualConfig)
                {
                    actualConfig = value;
                }
            }
        }

        private static string ConfigFilePath = null;

        public const string ConfigFileShortName = "DSAnimStudio_Config.json";

        private static void CheckConfigFilePath()
        {
            if (ConfigFilePath == null)
            {
                var currentAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var currentAssemblyDir = System.IO.Path.GetDirectoryName(currentAssemblyPath);
                ConfigFilePath = System.IO.Path.Combine(currentAssemblyDir, ConfigFileShortName);
            }
        }

        public static void ResetConfigToDefault(bool preserveRecentFilesList)
        {
            var recentFilesList = Config.RecentFilesList.ToList();
            var newConfig = new TaeConfigFile();
            if (preserveRecentFilesList)
            {
                newConfig.RecentFilesList = recentFilesList;
            }

            Config = newConfig;
            Config.AfterLoading(TAE_EDITOR);
            Config.AfterLoadingFirstTime(TAE_EDITOR);
            SaveConfig();
            LoadConfig();
        }

        public static void LoadConfig(bool isManual = false)
        {
            DebugToggleXmlRead();
            
            if (!isManual && DisableConfigFileAutoSave)
                return;
            CheckConfigFilePath();
            if (!System.IO.File.Exists(ConfigFilePath))
            {
                Config = new TaeEditor.TaeConfigFile();
                Config.AfterLoading(TAE_EDITOR);
                Config.AfterLoadingFirstTime(TAE_EDITOR);
                SaveConfig();
            }
            string jsonText = null;
            int tryCounter = 0;

            while (jsonText == null)
            {
                bool giveUp = false;
                try
                {
                    if (tryCounter < ConfigFileIOMaxTries)
                    {
                        jsonText = System.IO.File.ReadAllText(ConfigFilePath);
                    }
                    else
                    {
                        var ask = System.Windows.Forms.MessageBox.Show($"Failed 10 times in a row to read configuration file '{ConfigFileShortName}' from the application folder. " +
                            "It may have been in use by another " +
                            "application (e.g. another instance of DS Anim Studio). " +
                            "\n\nWould you like to RETRY the configuration file reading operation or CANCEL, " +
                            "disabling configuration file autosaving to be safe?", "Configuration File IO Failure",
                            MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);

                        if (ask == DialogResult.Retry)
                        {
                            giveUp = false;
                            tryCounter = 0;
                        }
                        else
                        {
                            giveUp = true;
                        }
                    }
                }
                catch (Exception handled_ex) when (Main.EnableErrorHandler.ReadConfigFile)
                {
                    Main.HandleError(nameof(Main.EnableErrorHandler.ReadConfigFile), handled_ex);
                    tryCounter++;
                }

                if (giveUp)
                {
                    DisableConfigFileAutoSave = true;
                    return;
                }
            }

            void jsonFailure()
            {
                var ask = System.Windows.Forms.MessageBox.Show($"Failed to parse configuration file '{ConfigFileShortName}' in the application folder. " +
                            "It may have been saved by an incompatible version of the application or corrupted. " +
                            "\n\nWould you like to overwrite it with default settings? " +
                            "\n\nIf not, configuration file autosaving will be disabled to keep the file as-is.", "Configuration File Parse Failure",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (ask == DialogResult.Yes)
                {
                    Config = new TaeEditor.TaeConfigFile();
                    SaveConfig(isManual: true);
                }
                else
                {
                    DisableConfigFileAutoSave = true;
                }
            }

            try
            {
                Config = Newtonsoft.Json.JsonConvert.DeserializeObject<TaeEditor.TaeConfigFile>(jsonText);

                
            }
            catch (Newtonsoft.Json.JsonException)
            {
                jsonFailure();
            }

            if (Config == null)
            {
                jsonFailure();
            }

            Config.AfterLoading(TAE_EDITOR);

            if (NeedsToLoadConfigFileForFirstTime)
            {
                Config.AfterLoadingFirstTime(TAE_EDITOR);
            }

            NeedsToLoadConfigFileForFirstTime = false;
        }

        public static void SaveConfig(bool isManual = false)
        {

            DebugToggleXmlWrite();
            
            if (!isManual && DisableConfigFileAutoSave)
                return;
            lock (Main.Config._lock_ThreadSensitiveStuff)
            {
                if (TAE_EDITOR?.Graph != null)
                {
                    // I'm sorry; this is pecularily placed.
                    TAE_EDITOR.Graph?.ViewportInteractor?.SaveChrAsm();
                }

                Config.BeforeSaving(TAE_EDITOR);
                CheckConfigFilePath();

                var jsonText = Newtonsoft.Json.JsonConvert
                    .SerializeObject(Config,
                    Newtonsoft.Json.Formatting.Indented);

                bool success = false;

                int tryCounter = 0;

                while (!success)
                {
                    bool giveUp = false;
                    try
                    {
                        if (tryCounter < ConfigFileIOMaxTries)
                        {
                            System.IO.File.WriteAllText(ConfigFilePath, jsonText);
                            success = true;
                        }
                        else
                        {
                            var ask = System.Windows.Forms.MessageBox.Show($"Failed 10 times in a row to write configuration file '{ConfigFileShortName}' in the application folder. " +
                                "It may have been in use by another " +
                                "application (e.g. another instance of DS Anim Studio). " +
                                "\n\nWould you like to RETRY the configuration file writing operation or CANCEL, " +
                                "disabling configuration file autosaving to be safe?", "Configuration File IO Failure",
                                MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);

                            if (ask == DialogResult.Retry)
                            {
                                giveUp = false;
                                tryCounter = 0;
                            }
                            else
                            {
                                giveUp = true;
                            }
                        }
                    }
                    catch (Exception handled_ex) when (Main.EnableErrorHandler.WriteConfigFile)
                    {
                        Main.HandleError(nameof(Main.EnableErrorHandler.WriteConfigFile), handled_ex);
                        tryCounter++;
                    }

                    if (giveUp)
                    {
                        DisableConfigFileAutoSave = true;
                        return;
                    }
                }

            }
        }

        

        public static Random Rand = new Random();
        public static float RandFloat()
        {
            return (float)Rand.NextDouble();
        }
        public static float RandSignedFloat()
        {
            return (float)((Rand.NextDouble() * 2) - 1);
        }
        public static Vector3 RandSignedVector3()
        {
            return new Vector3(RandSignedFloat(), RandSignedFloat(), RandSignedFloat());
        }

        public static string Directory = null;

        

        public static bool FIXED_TIME_STEP = false;

        public static bool REQUEST_EXIT = false;
        public static bool REQUEST_EXIT_NEXT_IS_AUTOMATIC = false;
        public static bool REQUEST_EXIT_GUARANTEED_FINAL = false;

        public static bool REQUEST_REINIT_EDITOR = false;

        public static bool REQUEST_DISABLE_SOUND = false;

        public static float DELTA_UPDATE_RATIO_VS_60HZ => DELTA_UPDATE / (1f / 60f);

        public static float DELTA_UPDATE;
        public static float DELTA_UPDATE_ROUNDED;
        public static float DELTA_DRAW;

        public static ImGuiRenderer ImGuiDraw;

        public static void ClearAllDynamicBind()
        {
            lock (_lock_dynamicBindDict)
            {
                dynamicBindDict.Clear();
            }
        }

        private static object _lock_dynamicBindDict = new object();
        private static Dictionary<IntPtr, IntPtr> dynamicBindDict = new();
        public static IntPtr SetDynamicBindTexture(Texture2D tex)
        {
            if (tex == null || tex.IsDisposed)
            {
                return IntPtr.Zero;
            }
            IntPtr result = IntPtr.Zero;
            var nativePtr = tex.GetNativePointer();
            lock (_lock_dynamicBindDict)
            {
                if (dynamicBindDict.ContainsKey(nativePtr))
                {
                    result = dynamicBindDict[nativePtr];
                }
                else
                {
                    result = ImGuiDraw.BindTexture(tex);
                }
            }
            return result;
        }

        public static void UnsetDynamicBindTexture(Texture2D tex)
        {
            if (tex == null || tex.IsDisposed)
                return;

            var nativePtr = tex.GetNativePointer();
            lock (_lock_dynamicBindDict)
            {
                if (dynamicBindDict.ContainsKey(nativePtr))
                {
                    ImGuiDraw.UnbindTexture(dynamicBindDict[nativePtr]);
                    dynamicBindDict.Remove(nativePtr);
                }
            }
        }

        public static Vector2 GlobalTaeEditorFontOffset = new Vector2(0, -3);

        public static IServiceProvider MainContentServiceProvider = null;

        private bool prevFrameWasLoadingTaskRunning = false;
        public static bool Active { get; private set; }
        public static HysteresisBool ActiveHyst = new HysteresisBool(0, 5);

        private static object _lock_ForceDrawTimer = new object();
        private static float ForceDrawTimer = 0;
        public static void AddForceDrawTime(float time)
        {
            lock (_lock_ForceDrawTimer)
            {
                ForceDrawTimer += time;
            }
            
        }

        public static bool prevActive { get; private set; }

        public static bool IsFirstUpdateLoop { get; private set; } = true;
        public static bool IsFirstFrameActive { get; private set; } = false;

        public static bool Minimized { get; private set; }


        private static float MemoryUsageCheckTimer = 0;
        private static long MemoryUsage_Unmanaged = 0;
        private static long MemoryUsage_Managed = 0;
        private const float MemoryUsageCheckInterval = 0.25f;

        public static readonly Color SELECTED_MESH_COLOR = Color.Yellow * 0.05f;
        //public static readonly Color SELECTED_MESH_WIREFRAME_COLOR = Color.Yellow;

        public static Texture2D GRID_CELL_TEXTURE;
        public static Texture2D GRID_CELL_TEXTURE_THICK_X;
        public static Texture2D GRID_CELL_TEXTURE_THICK_Y;
        public static Texture2D GRID_ORIGIN_CROSS_TEXTURE;
        public static Texture2D GRID_CELL_TEXTURE_2;

        public static Texture2D WHITE_TEXTURE;
        public static Texture2D BLACK_TEXTURE;
        public static Texture2D GRAY_SRGB_TEXTURE;
        public static Texture2D DEFAULT_TEXTURE_DIFFUSE;
        public static Texture2D DEFAULT_TEXTURE_SPECULAR;
        public static Texture2D DEFAULT_TEXTURE_SPECULAR_DS2;
        public static Texture2D DEFAULT_TEXTURE_NORMAL;
        public static Texture2D DEFAULT_TEXTURE_NORMAL_DS2;
        public static Texture2D DEFAULT_TEXTURE_MISSING;
        public static TextureCube DEFAULT_TEXTURE_MISSING_CUBE;
        public static Texture2D DEFAULT_TEXTURE_EMISSIVE;
        public static Texture2D DEFAULT_TEXTURE_METALLIC;
        public string DEFAULT_TEXTURE_MISSING_NAME => $@"{Main.Directory}\Content\Utility\MissingTexture";

        //public static TaeEditor.TaeEditorScreen TAE_EDITOR;
        private static SpriteBatch TaeEditorSpriteBatch;
        public static Texture2D TAE_EDITOR_BLANK_TEX;
        public static SpriteFont TAE_EDITOR_FONT;
        public static SpriteFont TAE_EDITOR_FONT_SMALL;
        public static Texture2D TAE_EDITOR_SCROLLVIEWER_ARROW;

        public static FlverTonemapShader MainFlverTonemapShader = null;
        public static string FlverTonemapShader__Name => $@"{Main.Directory}\Content\Shaders\FlverTonemapShader";

        //public static Stopwatch UpdateStopwatch = new Stopwatch();
        //public static TimeSpan MeasuredTotalTime = TimeSpan.Zero;
        //public static TimeSpan MeasuredElapsedTime = TimeSpan.Zero;

        public bool IsLoadingTaskRunning = false;
        public HysteresisBool IsLoadingTaskRunningHyst = new HysteresisBool(0, 5);

        public static ContentManager MainContentManager = null;

        public enum RenderTargetViewTypes
        {
            None = 0,
            Color = 1,
            //BlurMask = 2,
        }

        public static RenderTargetViewTypes ViewRenderTarget = RenderTargetViewTypes.None;

        public static RenderTarget2D RenderTarget0_Color = null;
        //public static RenderTarget2D RenderTarget0_BlurMask = null;
        //public static RenderTarget2D RenderTarget1_Color = null;
        //public static RenderTarget2D UnusedRendertarget0 = null;
        public static int UnusedRenderTarget0Padding = 0;

        //public static float RenderTargetBlurAnisoPower = 32f;
        //public static float RenderTargetBlurAnisoMin = 0.5f;
        //public static float RenderTargetBlurAnisoMax = 1;

        //// BLUR DIRECTIONS (Default 16.0 - More is better but slower)
        //public static float RenderTargetBlurDirections = 16.0f;

        //// BLUR QUALITY (Default 4.0 - More is better but slower)
        //public static float RenderTargetBlurQuality = 3.0f;

        //// BLUR SIZE (Radius)
        //public static float RenderTargetBlurSize = 8.0f;
        //public static bool RenderTargetDebugBlurDisp = false;
        //public static bool RenderTargetDebugBlurMaskDisp = false;
        //public static bool RenderTargetDebugDisableBlur = true;

        public static int RequestHideOSD = 0;
        public static int RequestHideOSD_MAX = 10;

        public static bool RequestViewportRenderTargetResolutionChange = false;
        private const float TimeBeforeNextRenderTargetUpdate_Max = 0.5f;
        private static float TimeBeforeNextRenderTargetUpdate = 0;

        public static ImFontPtr ImGuiFontPointer;

        public Rectangle TAEScreenBounds
        {
            get => GFX.Device.Viewport.Bounds;
            set
            {
                if (value != TAEScreenBounds)
                {
                    GFX.Device.Viewport = new Viewport(value);
                }
            }
        }

        public Rectangle ClientBounds => Window.ClientBounds;

        private static GraphicsDeviceManager graphics;
        //public ContentManager Content;
        //public bool IsActive = true;

        public static List<DisplayMode> GetAllResolutions()
        {
            List<DisplayMode> result = new List<DisplayMode>();
            foreach (var mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                result.Add(mode);
            }
            return result;
        }

        public static void ApplyPresentationParameters(int width, int height, SurfaceFormat format,
            bool vsync, bool fullscreen)
        {
            WinForm.Invoke(() =>
            {
                graphics.PreferredBackBufferWidth = width;
                graphics.PreferredBackBufferHeight = height;
                graphics.PreferredBackBufferFormat = GFX.BackBufferFormat;
                graphics.IsFullScreen = fullscreen;
                graphics.SynchronizeWithVerticalRetrace = vsync;

                if (GFX.MSAA > 0)
                {
                    graphics.PreferMultiSampling = true;
                    graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = GFX.MSAA;
                }
                else
                {
                    graphics.PreferMultiSampling = false;
                    graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = 1;
                    graphics.GraphicsDevice.PresentationParameters.DepthStencilFormat = DepthFormat.Depth24Stencil8;
                }

                //graphics.PreferMultiSampling = false;
                //graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = 1;

                graphics.ApplyChanges();
            });
            
        }



        //MCG MCGTEST_MCG;



        public Main()
        {
            

            WinForm = (Form)Form.FromHandle(Window.Handle);
            WinForm.FormClosing += WinForm_FormClosing;
            WinForm.KeyPreview = true;
            

            WindowsMouseHook.Hook(Window.Handle);
            WinForm.AutoScaleMode = AutoScaleMode.Dpi;

            WinForm.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);

            BaseDPI = BaseDPI = WinForm.DeviceDpi / 96f;
            WinForm.DpiChanged += WinForm_DpiChanged;

            Directory = new FileInfo(typeof(Main).Assembly.Location).DirectoryName;

            graphics = new GraphicsDeviceManager(this);
            graphics.DeviceCreated += Graphics_DeviceCreated;
            graphics.DeviceReset += Graphics_DeviceReset;

            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromTicks(166667);
            // Setting this max higher allows it to skip frames instead of do slow motion.
            MaxElapsedTime = TimeSpan.FromSeconds(0.5);

            //IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = GFX.Display.Vsync;
            graphics.IsFullScreen = GFX.Display.Fullscreen;
            //graphics.PreferMultiSampling = GFX.Display.SimpleMSAA;
            graphics.PreferredBackBufferWidth = (int)Math.Round(GFX.Display.Width * DPI);
            graphics.PreferredBackBufferHeight = (int)Math.Round(GFX.Display.Height * DPI);
            if (!GraphicsAdapter.DefaultAdapter.IsProfileSupported(GraphicsProfile.HiDef))
            {
                System.Windows.Forms.MessageBox.Show("MonoGame is detecting your GPU as too " +
                    "low-end and refusing to enter the non-mobile Graphics Profile, " +
                    "which is needed for the model viewer. The app will likely crash now.");

                graphics.GraphicsProfile = GraphicsProfile.Reach;
            }
            else
            {
                graphics.GraphicsProfile = GraphicsProfile.HiDef;
            }

            graphics.PreferredBackBufferFormat = GFX.BackBufferFormat;

            graphics.PreferMultiSampling = false;

            graphics.ApplyChanges();

            Window.AllowUserResizing = true;

            Window.ClientSizeChanged += Window_ClientSizeChanged;

            WinForm.Shown += (o, e) =>
            {
                LoadConfig();
                
            };

            this.Activated += Main_Activated;
            this.Deactivated += Main_Deactivated;

            GFX.Display.SetFromDisplayMode(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode);
            

            Input = new FancyInputHandler();
            //GFX.Device.Viewport = new Viewport(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
        }

        private void WinForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (zzz_DocumentManager.AnyDocumentWithCriticalLoadingTasks())
                e.Cancel = true;

            if (zzz_DocumentManager.AnyDocumentWithUnsavedChanges())
                e.Cancel = true;

            if (zzz_DocumentManager.AnyDocumentsRequestingToClose())
                e.Cancel = true;

            if (!REQUEST_EXIT_NEXT_IS_AUTOMATIC)
                zzz_DocumentManager.RequestCloseAllDocuments();

            REQUEST_EXIT_NEXT_IS_AUTOMATIC = false;

            if (e.Cancel)
            {
                Main.REQUEST_EXIT_NEXT_IS_AUTOMATIC = true;
                REQUEST_EXIT = true; // Keep retrying to exit lol
            }
            else
            {
                REQUEST_EXIT_GUARANTEED_FINAL = true;
                SaveConfig();
            }
        }

        private void WinForm_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            UpdateDpiStuff();
        }

        public void UpdateDpiStuff()
        {
            float newDpi = WinForm.DeviceDpi / 96f;
            BaseDPI = BaseDPI = newDpi;

            RequestViewportRenderTargetResolutionChange = true;
        }

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //        FmodManager.Shutdown();

        //    base.Dispose(disposing);
        //}

        private void Main_Deactivated(object sender, EventArgs e)
        {
            UpdateActiveState();
#if !DEBUG
            zzz_DocumentManager.CurrentDocument?.SoundManager?.StopAllSounds();
#endif
        }

        private void Main_Activated(object sender, EventArgs e)
        {
            IsFirstFrameActive = true;
            UpdateActiveState();
        }

        

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            WindowResizeCommitTimer = 0;
        }

        public void RebuildRenderTarget(bool urgent)
        {
           

            void doRenderTargetBuild(bool useLock)
            {
                GFX.ClampAntialiasingOptions();

                int msaa = GFX.MSAA;
                int ssaa = GFX.SSAA;

                //SceneRenderTarget?.Dispose();
                //GC.Collect();

                var newRenderTarget0_Color = new RenderTarget2D(GFX.Device, TAE_EDITOR.ModelViewerBounds.DpiScaled().Width * ssaa,
                       TAE_EDITOR.ModelViewerBounds.DpiScaled().Height * ssaa, false, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8,
                        msaa, RenderTargetUsage.DiscardContents);

                //var newRenderTarget0_BlurMask = new RenderTarget2D(GFX.Device, TAE_EDITOR.ModelViewerBounds.DpiScaled().Width * ssaa,
                //       TAE_EDITOR.ModelViewerBounds.DpiScaled().Height * ssaa, false, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8,
                //       1, RenderTargetUsage.DiscardContents);

                // var newSceneRenderTarget = new RenderTarget2D(GFX.Device, TAE_EDITOR.ModelViewerBounds.DpiScaled().Width * ssaa,
                //     TAE_EDITOR.ModelViewerBounds.DpiScaled().Height * ssaa, false, SurfaceFormat.Vector4, DepthFormat.Depth24,
                //     msaa, RenderTargetUsage.DiscardContents);

                RenderTarget2D oldRenderTarget0_Color = null;
                //RenderTarget2D oldRenderTarget0_BlurMask = null;
                if (useLock)
                {
                    lock (_lock_ChangeRenderTarget)
                    {
                        oldRenderTarget0_Color = RenderTarget0_Color;
                        RenderTarget0_Color = newRenderTarget0_Color;
                        //oldRenderTarget0_BlurMask = RenderTarget0_BlurMask;
                        //RenderTarget0_BlurMask = newRenderTarget0_BlurMask;
                    }
                }
                else
                {
                    oldRenderTarget0_Color = RenderTarget0_Color;
                    RenderTarget0_Color = newRenderTarget0_Color;
                    //oldRenderTarget0_BlurMask = RenderTarget0_BlurMask;
                    //RenderTarget0_BlurMask = newRenderTarget0_BlurMask;
                }

                oldRenderTarget0_Color?.Dispose();
                //oldRenderTarget0_BlurMask?.Dispose();


                TimeBeforeNextRenderTargetUpdate = TimeBeforeNextRenderTargetUpdate_Max;

                RequestViewportRenderTargetResolutionChange = false;

                GFX.EffectiveSSAA = ssaa;
                GFX.EffectiveMSAA = msaa;
            }

            if (urgent)
            {
                renderTargetBuildTask = null;
                doRenderTargetBuild(useLock: false);
            }
            else
            {
                if (TimeBeforeNextRenderTargetUpdate > 0 || renderTargetBuildTask?.Status == System.Threading.Tasks.TaskStatus.Running)
                    return;

                renderTargetBuildTask = System.Threading.Tasks.Task.Run(() =>
                {
                    doRenderTargetBuild(useLock: true);
                });
            }
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            CFG.Save();

            Main.SaveConfig();

            DSAnimStudio.LiveRefresh.Memory.CloseHandle();

            base.OnExiting(sender, args);
        }

        private void Graphics_DeviceCreated(object sender, System.EventArgs e)
        {
            GFX.Device = GraphicsDevice;
        }

        private void Graphics_DeviceReset(object sender, System.EventArgs e)
        {
            GFX.Device = GraphicsDevice;
        }

        protected override void Initialize()
        {
            try
            {
                var winForm = (Form)Control.FromHandle(Window.Handle);
                winForm.AllowDrop = true;
                winForm.DragEnter += GameWindowForm_DragEnter;
                winForm.DragDrop += GameWindowForm_DragDrop;

                IsMouseVisible = true;

                DEFAULT_TEXTURE_DIFFUSE = new Texture2D(GraphicsDevice, 1, 1);
                DEFAULT_TEXTURE_DIFFUSE.SetData(new Color[] { new Color(1f, 1f, 1f) });

                WHITE_TEXTURE = new Texture2D(GraphicsDevice, 1, 1);
                WHITE_TEXTURE.SetData(new Color[] { new Color(1.0f, 1.0f, 1.0f) });

                BLACK_TEXTURE = new Texture2D(GraphicsDevice, 1, 1);
                BLACK_TEXTURE.SetData(new Color[] { new Color(0.0f, 0.0f, 0.0f) });

                GRAY_SRGB_TEXTURE = new Texture2D(GraphicsDevice, 1, 1);
                GRAY_SRGB_TEXTURE.SetData(new Color[] { new Color(0.5f * 0.5f, 0.5f * 0.5f, 0.5f * 0.5f) });

                DEFAULT_TEXTURE_SPECULAR = new Texture2D(GraphicsDevice, 1, 1);
                DEFAULT_TEXTURE_SPECULAR.SetData(new Color[] { new Color(0f, 0f, 0f) });

                DEFAULT_TEXTURE_SPECULAR_DS2 = new Texture2D(GraphicsDevice, 1, 1);
                DEFAULT_TEXTURE_SPECULAR_DS2.SetData(new Color[] { new Color(0.5f, 0.5f, 0.5f) });

                DEFAULT_TEXTURE_NORMAL = new Texture2D(GraphicsDevice, 1, 1);
                DEFAULT_TEXTURE_NORMAL.SetData(new Color[] { new Color(0.5f, 0.5f, 0.0f) });

                DEFAULT_TEXTURE_NORMAL_DS2 = new Texture2D(GraphicsDevice, 1, 1);
                DEFAULT_TEXTURE_NORMAL_DS2.SetData(new Color[] { new Color(0.5f, 0.5f, 0.5f, 0.5f) });

                DEFAULT_TEXTURE_EMISSIVE = new Texture2D(GraphicsDevice, 1, 1);
                DEFAULT_TEXTURE_EMISSIVE.SetData(new Color[] { Color.Black });

                DEFAULT_TEXTURE_METALLIC = new Texture2D(GraphicsDevice, 1, 1);
                DEFAULT_TEXTURE_METALLIC.SetData(new Color[] { new Color(0.5f, 0f, 1f) });

                DEFAULT_TEXTURE_MISSING = Content.Load<Texture2D>(DEFAULT_TEXTURE_MISSING_NAME);

                DEFAULT_TEXTURE_MISSING_CUBE = new TextureCube(GraphicsDevice, 1, false, SurfaceFormat.Color);
                DEFAULT_TEXTURE_MISSING_CUBE.SetData(CubeMapFace.PositiveX, new Color[] { Color.Fuchsia });
                DEFAULT_TEXTURE_MISSING_CUBE.SetData(CubeMapFace.PositiveY, new Color[] { Color.Fuchsia });
                DEFAULT_TEXTURE_MISSING_CUBE.SetData(CubeMapFace.PositiveZ, new Color[] { Color.Fuchsia });
                DEFAULT_TEXTURE_MISSING_CUBE.SetData(CubeMapFace.NegativeX, new Color[] { Color.Fuchsia });
                DEFAULT_TEXTURE_MISSING_CUBE.SetData(CubeMapFace.NegativeY, new Color[] { Color.Fuchsia });
                DEFAULT_TEXTURE_MISSING_CUBE.SetData(CubeMapFace.NegativeZ, new Color[] { Color.Fuchsia });

                GFX.Device = GraphicsDevice;

                ImGuiDraw = new ImGuiRenderer(this);
                //ImGuiDraw.RebuildFontAtlas();

                zzz_DocumentManager.Init();

                base.Initialize();
            }
            catch (Exception ex) when (Main.EnableErrorHandler.MainInit)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Error occurred while initializing DS Anim Studio (please report):\n\n{ex.ToString()}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        

        private static Microsoft.Xna.Framework.Vector3 currModelAddOffset = Microsoft.Xna.Framework.Vector3.Zero;
        private void GameWindowForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] modelFiles = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            //TAE_EDITOR.

            void LoadOneFile(string file)
            {
                if (file.ToUpper().EndsWith(".FLVER") || file.ToUpper().EndsWith(".FLVER.DCX"))
                {
                    if (FLVER2.Is(file))
                    {
                        //currModelAddOffset.X += 3;
                        var m = new Model(zzz_DocumentManager.CurrentDocument, FLVER2.Read(file), false);
                        m.StartTransform = new Transform(currModelAddOffset, Microsoft.Xna.Framework.Quaternion.Identity);
                        zzz_DocumentManager.CurrentDocument.Scene.ClearSceneAndAddModel(m);
                    }
                    else if (FLVER0.Is(file))
                    {
                        //currModelAddOffset.X += 3;
                        var m = new Model(zzz_DocumentManager.CurrentDocument, FLVER0.Read(file), false);
                        m.StartTransform = new Transform(currModelAddOffset, Microsoft.Xna.Framework.Quaternion.Identity);
                        zzz_DocumentManager.CurrentDocument.Scene.ClearSceneAndAddModel(m);
                    }
                }
                else if (file.ToUpper().EndsWith(".CHRBND") || file.ToUpper().EndsWith(".CHRBND.DCX"))
                {
                    zzz_DocumentManager.CurrentDocument.Scene.ClearScene();
                    //currModelAddOffset.X += 3;
                    zzz_DocumentManager.CurrentDocument.GameRoot.InitializeFromBND(file);
                    var mArr = zzz_DocumentManager.CurrentDocument.GameRoot.LoadCharacter(Utils.GetShortIngameFileName(file), null);
                    foreach (var m in mArr)
                    {
                        m.StartTransform = m.CurrentTransform = new Transform(currModelAddOffset, Microsoft.Xna.Framework.Quaternion.Identity);
                        m.AnimContainer?.RequestDefaultAnim();
                        
                        m.AnimContainer.ForcePlayAnim = true;
                        //m.UpdateAnimation();
                    }
                    
                    //Scene.ClearSceneAndAddModel(m);
                }
                else if (file.ToUpper().EndsWith(".OBJBND") || file.ToUpper().EndsWith(".OBJBND.DCX"))
                {
                    zzz_DocumentManager.CurrentDocument.Scene.ClearScene();
                    //currModelAddOffset.X += 3;
                    zzz_DocumentManager.CurrentDocument.GameRoot.InitializeFromBND(file);
                    var m = zzz_DocumentManager.CurrentDocument.GameRoot.LoadObject(Utils.GetShortIngameFileName(file));
                    m.StartTransform = m.CurrentTransform = new Transform(currModelAddOffset, Microsoft.Xna.Framework.Quaternion.Identity);
                    m.AnimContainer?.RequestDefaultAnim();
                    
                    m.AnimContainer.ForcePlayAnim = true;
                    //m.UpdateAnimation();
                    //Scene.ClearSceneAndAddModel(m);
                }
                //else if (file.ToUpper().EndsWith(".HKX"))
                //{
                //    var anim = KeyboardInput.Show("Enter Anim ID", "Enter name to save the dragged and dropped HKX file to e.g. a01_3000.");
                //    string name = anim.Result;
                //    byte[] animData = File.ReadAllBytes(file);
                //    TAE_EDITOR.FileContainer.AddNewHKX(name, animData, out byte[] dataForAnimContainer);
                //    if (dataForAnimContainer != null)
                //        TAE_EDITOR.Graph.ViewportInteractor.CurrentModel.AnimContainer.AddNewHKXToLoad(name + ".hkx", dataForAnimContainer);
                //    else
                //        DialogManager.DialogOK("Failed", "Failed to save (TagTools refused to work), just try again.");
                //}
            }

            if (modelFiles.Length == 1)
            {
                string f = modelFiles[0].ToLower();

                if (f.EndsWith(".fbx"))
                {
                    TAE_EDITOR.Config.LastUsedImportConfig_FLVER2.AssetPath = modelFiles[0];
                    TAE_EDITOR.BringUpImporter_FLVER2();
                    TAE_EDITOR.Config.LastUsedImportConfig_FLVER2.AssetPath = modelFiles[0];
                    TAE_EDITOR.ImporterWindow_FLVER2.LoadValuesFromConfig();
                }
                else
                {
                    LoadOneFile(modelFiles[0]);
                }
            }
            else
            {

                zzz_DocumentManager.CurrentDocument.LoadingTaskMan.DoLoadingTask("LoadingDroppedModel", "Loading dropped model(s)...", prog =>
                {
                    foreach (var file in modelFiles)
                    {
                        LoadOneFile(file);
                    }

                }, disableProgressBarByDefault: true);
            }

            //LoadDragDroppedFiles(modelFiles.ToDictionary(f => f, f => File.ReadAllBytes(f)));
        }

        static bool IsValidDragDropModelFile(string f)
        {
            return (/*f.ToUpper().EndsWith(".HKX") || */BND3.Is(f) || BND4.Is(f) || FLVER2.Is(f));
        }

        private void GameWindowForm_DragEnter(object sender, DragEventArgs e)
        {
            IsFirstFrameActive = true;

            bool isValid = false;

            if (e.Data.GetDataPresent(DataFormats.FileDrop) && !zzz_DocumentManager.CurrentDocument.LoadingTaskMan.AnyInteractionBlockingTasks())
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                

                if (files.Length == 1)
                {
                    string f = files[0].ToLower();

                    if (f.EndsWith(".fbx"))
                    {
                        isValid = zzz_DocumentManager.CurrentDocument.Scene.IsModelLoaded;
                    }
                    else if (f.EndsWith(".flver.dcx") || f.EndsWith(".flver") || f.EndsWith(".chrbnd") || f.EndsWith(".chrbnd.dcx") || f.EndsWith(".objbnd") || f.EndsWith(".objbnd.dcx"))
                    {
                        isValid = true;
                    }
                    else if (f.EndsWith(".hkx"))
                    {
                        isValid = true;
                    }
                }
                // If multiple files are dragged they must all be regularly 
                // loadable rather than the specific case ones above
                else if (files.All(f => IsValidDragDropModelFile(f)))
                    isValid = true;


            }

            e.Effect = isValid ? DragDropEffects.Link : DragDropEffects.None;
        }

        //public static void RESET_ALL()
        //{
        //    zzz_DocumentManager.CurrentDocument.EditorScreen.CleanupForReinit();
        //    zzz_DocumentManager.CurrentDocument.EditorScreen = new TaeEditor.TaeEditorScreen((Form)Form.FromHandle(Program.MainInstance.Window.Handle));
        //    zzz_DocumentManager.CurrentDocument.Scene.ClearScene();
        //    DialogManager.ClearAll();
        //    zzz_DocumentManager.CurrentDocument.GameRoot.ClearInterroot();
        //    zzz_DocumentManager.CurrentDocument.GameData.ClearAll();
        //}

        private byte[] ReadXnbCompressedMGFX(string assetName)
        {
            byte[] result = null;
            using (var basicEffectStream = File.OpenRead($@"{assetName}.xnb"))
            {
                using (var xnbReader = new BinaryReader(basicEffectStream))
                {
                    using (var reader = Content.GetContentReaderFromXnb(assetName, basicEffectStream, xnbReader, null))
                    {
                        int numberOfReaders = reader.Read7BitEncodedInt();
                        string[] readerNames = new string[numberOfReaders];
                        for (int r = 0; r < numberOfReaders; r++)
                        {
                            readerNames[r] = reader.ReadString();
                        }
                        reader.ReadInt32();
                        int sharedResourceCount = reader.Read7BitEncodedInt();
                        int readerIndexPlusOne = reader.Read7BitEncodedInt();
                        string readerType = readerNames[readerIndexPlusOne - 1];
                        if (readerType.StartsWith("Microsoft.Xna.Framework.Content.EffectReader"))
                        {
                            int dataSize = reader.ReadInt32();
                            result = reader.ReadBytes(dataSize);
                        }
                        
                        //File.WriteAllBytes($"{Main.Directory}\\Debug_BasicEffectDecompressed.bin", BasicEffectBytecode);
                    }
                }
            }
            return result;
        }
        
        protected override void LoadContent()
        {
            MainContentServiceProvider = Content.ServiceProvider;
            MainContentManager = Content;

            BasicEffectBytecode = ReadXnbCompressedMGFX($@"{Main.Directory}\Content\Shaders\BasicEffectEx\BasicEffect");
            SpriteEffectBytecode = ReadXnbCompressedMGFX($@"{Main.Directory}\Content\Shaders\BasicEffectEx\SpriteEffect");

            //File.WriteAllBytes(@$"{Main.Directory}\Dump_BasicEffectBytecode.bin", BasicEffectBytecode);
            //File.WriteAllBytes(@$"{Main.Directory}\Dump_SpriteEffectBytecode.bin", SpriteEffectBytecode);


            GFX.Init(Content);
            DBG.LoadContent(Content);
            //InterrootLoader.OnLoadError += InterrootLoader_OnLoadError;

            DBG.CreateDebugPrimitives();

            //DBG.EnableMenu = true;
            //DBG.EnableMouseInput = true;
            //DBG.EnableKeyboardInput = true;
            if (DBG.EnableMenu)
                DbgMenuItem.Init();

            UpdateMemoryUsage();

            CFG.AttemptLoadOrDefault();

            Main.LoadConfig();

            if ((Config.HasAcceptedSplashBefore || IsPatreonBuild) && !SplashManager.Debug_IgnoreAlreadySeenFlag)
            {
                if (!SplashManager.Debug_IgnoreAlreadySeenFlag)
                {
                    SplashManager.SplashFadeInDuration = 0.25f;
                    SplashManager.SplashFadeOutDuration = 0.25f;
                    SplashManager.SplashMinShowBeforeDismiss = 0;
                    SplashManager.SplashBarEnabled = false;
                    //SplashState = SplashScreenStates.Complete;
                }
            }

            TAE_EDITOR_FONT = Content.Load<SpriteFont>($@"{Main.Directory}\Content\Fonts\DbgMenuFontSmall");
            TAE_EDITOR_FONT_SMALL = Content.Load<SpriteFont>($@"{Main.Directory}\Content\Fonts\DbgMenuFontSmaller");
            TAE_EDITOR_BLANK_TEX = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            TAE_EDITOR_BLANK_TEX.SetData(new Color[] { Color.White }, 0, 1);
            TAE_EDITOR_SCROLLVIEWER_ARROW = Content.Load<Texture2D>($@"{Main.Directory}\Content\Utility\TaeEditorScrollbarArrow");

            GRID_CELL_TEXTURE = Content.Load<Texture2D>($@"{Main.Directory}\Content\GridCell");
            GRID_CELL_TEXTURE_THICK_X = Content.Load<Texture2D>($@"{Main.Directory}\Content\GridCellThickX");
            GRID_CELL_TEXTURE_THICK_Y = Content.Load<Texture2D>($@"{Main.Directory}\Content\GridCellThickY");
            GRID_ORIGIN_CROSS_TEXTURE = Content.Load<Texture2D>($@"{Main.Directory}\Content\GridOriginCross");
            GRID_CELL_TEXTURE_2 = Content.Load<Texture2D>($@"{Main.Directory}\Content\GridCell2");

            //var gridCellMeme = $"__ds_anim_studio__grid_cell{Guid.NewGuid().ToString()}";
            //TexturePool.AddFetchDDS(File.ReadAllBytes($@"{Main.Directory}\Content\GridCell.dds"), gridCellMeme);
            //GRID_CELL_TEXTURE = TexturePool.Fetches[gridCellMeme].Fetch2D();

            InitTaeEditor();

            TaeEditorSpriteBatch = new SpriteBatch(GFX.Device, Main.SpriteEffectBytecode);

            if (Program.ARGS.Length > 0)
            {
                var HasHandledArgs = false;

                for (var i = 0; i < Program.ARGS.Length; i++) {
                    string ProgramArg = Program.ARGS[i];

                    if (DSASURILoader.IsValid(ProgramArg))
                    {
                        DSASURILoader URILoader = new(ProgramArg, TAE_EDITOR);

                        HasHandledArgs = URILoader.Process();
                    }
                }

                if (!HasHandledArgs) {
                    //TODO

                }

                //LoadDragDroppedFiles(Program.ARGS.ToDictionary(f => f, f => File.ReadAllBytes(f)));
            }

            MainFlverTonemapShader = new FlverTonemapShader(Content.Load<Effect>(FlverTonemapShader__Name));

            DoActualImguiFontBuild();

            TAE_EDITOR.LoadContent(Content);

            UpdateDpiStuff();

            SplashManager.LoadSplash(Content);
        }

        private static IntPtr PTR_ImguiFont;
        
        private unsafe static void DestroyImguiFont()
        {
            if (PTR_ImguiFont != IntPtr.Zero)
                ImGui.MemFree(PTR_ImguiFont);
        }
        
        private unsafe static void DoActualImguiFontBuild()
        {
            var fonts = ImGuiNET.ImGui.GetIO().Fonts;
            //var fontFile = File.ReadAllBytes($@"{Directory}\Content\Fonts\NotoSansCJKjp-Medium.otf");
            var fontFile = File.ReadAllBytes(ImGuiFontName);
            fonts.Clear();
            PTR_ImguiFont = ImGui.MemAlloc((uint)fontFile.Length);
            Marshal.Copy(fontFile, 0, PTR_ImguiFont, fontFile.Length);
            ImVector ranges;
            ImFontGlyphRangesBuilder* rawPtr = ImGuiNative.ImFontGlyphRangesBuilder_ImFontGlyphRangesBuilder();
            var builder = new ImFontGlyphRangesBuilderPtr(rawPtr);
            var ccm = CCM.Read($@"{Directory}\Content\Fonts\dbgfont14h_ds3.ccm");

            // test
                //var bw = new BinaryWriterEx(false);
            foreach (var g in ccm.Glyphs)
            {
                builder.AddChar((ushort)g.Key);
                //bw.WriteUInt16((ushort)g.Key);
            }
            //var glyphsBytes = bw.FinishBytes();
            //File.WriteAllBytes($@"{Directory}\ImguiFontGlyphs.bin", glyphsBytes);
                    
            builder.BuildRanges(out ranges);
            var ptr = ImGuiNET.ImGuiNative.ImFontConfig_ImFontConfig();
            var cfg = new ImGuiNET.ImFontConfigPtr(ptr);
            cfg.GlyphMinAdvanceX = ImGuiFontGlyphMinAdvanceX;
            cfg.OversampleH = ImGuiFontOversampleH;
            cfg.OversampleV = ImGuiFontOversampleV;
            cfg.PixelSnapH = ImGuiFontPixelSnapH;
                    
            ImGuiFontPointer = fonts.AddFontFromMemoryTTF(PTR_ImguiFont, fontFile.Length, ImGuiFontPixelSize * Main.DPI, cfg, ranges.Data);
            builder.Destroy();
            fonts.Build();
            ImGuiDraw.RebuildFontAtlas();

            var test = ImGui.GetStyle();
            test.ScrollbarSize = ImGuiScrollBarPixelSize * Main.DPI;
        }

        public static bool ImguiFontRebuildRequested = false;
        public static void BuildImguiFonts()
        {
            ImguiFontRebuildRequested = true;
        }

        private void InterrootLoader_OnLoadError(string contentName, string error)
        {
            Console.WriteLine($"CONTENT LOAD ERROR\nCONTENT NAME:{contentName}\nERROR:{error}");
        }

        private string GetMemoryUseString(string prefix, long MemoryUsage)
        {
            const double MEM_KB = 1024f;
            const double MEM_MB = 1024f * 1024f;
            //const double MEM_GB = 1024f * 1024f * 1024f;

            if (MemoryUsage < MEM_KB)
                return $"{prefix}{(1.0 * MemoryUsage):0} B";
            else if (MemoryUsage < MEM_MB)
                return $"{prefix}{(1.0 * MemoryUsage / MEM_KB):0.00} KB";
            else// if (MemoryUsage < MEM_GB)
                return $"{prefix}{(1.0 * MemoryUsage / MEM_MB):0.00} MB";
            //else
            //    return $"{prefix}{(1.0 * MemoryUsage / MEM_GB):0.00} GB";
        }

        private Color GetMemoryUseColor(long MemoryUsage, bool audio = false)
        {
            //const double MEM_KB = 1024f;
            //const double MEM_MB = 1024f * 1024f;
            const double MEM_GB = 1024f * 1024f * 1024f;

            if (MemoryUsage < MEM_GB / (audio ? 4 : 1))
                return Colors.GuiColorMemoryUseTextGood;
            else if (MemoryUsage < (MEM_GB * 2) / (audio ? 4 : 1))
                return Colors.GuiColorMemoryUseTextOkay;
            else
                return Colors.GuiColorMemoryUseTextBad;
        }

        private void DrawMemoryUsage()
        {
            float scale = Config.ViewportMemoryTextSize / 100f;
            if (scale <= 0)
                return;
            
            var str_managed = GetMemoryUseString("CLR Mem:  ", MemoryUsage_Managed);
            var str_unmanaged = GetMemoryUseString("RAM USE:  ", MemoryUsage_Unmanaged);

            //var strSize_managed = DBG.DEBUG_FONT_SMALL.MeasureString(str_managed);
            //var strSize_unmanaged = DBG.DEBUG_FONT_SMALL.MeasureString(str_unmanaged);

            var strSize_managed = ImGuiDebugDrawer.MeasureString(str_managed, 16 * scale);
            var strSize_unmanaged = ImGuiDebugDrawer.MeasureString(str_unmanaged, 16 * scale);

            GFX.SpriteBatchBeginForText();

            var soundMan = zzz_DocumentManager.CurrentDocument?.SoundManager;
            if (soundMan != null)
            {
                var engineType = soundMan?.EngineType;

                if (engineType is zzz_SoundManagerIns.EngineTypes.Wwise)
                {
                    var wwinfo = soundMan.WwiseManager.GetMemoryInfo();
                    if (wwinfo != null)
                    {
                        var str_wwise = $"{(wwinfo.AnySoundsLoading ? "[LOADING SOUNDS...]\n" : "")}Sounds Loaded: {wwinfo.SoundFileCount}\n" + GetMemoryUseString("Sound Memory:  ", wwinfo.ByteCount);
                        //var strSize_wwise = DBG.DEBUG_FONT_SMALL.MeasureString(str_wwise);
                        var strSize_wwise = ImGuiDebugDrawer.MeasureString(str_wwise, 16 * scale);
                        //DBG.DrawOutlinedText(str_wwise, new Vector2(GFX.Device.Viewport.Width - 6,
                        //GFX.Device.Viewport.Height - 40) / DPIVector,
                        //GetMemoryUseColor(wwinfo.ByteCount, audio: true), DBG.DEBUG_FONT_SMALL, scale: 1, scaleOrigin: strSize_wwise);

                        ImGuiDebugDrawer.DrawText(str_wwise, (new Vector2(GFX.Device.Viewport.Width - 6,
                        GFX.Device.Viewport.Height - 32 * scale) / DPIVector) - strSize_wwise, GetMemoryUseColor(wwinfo.ByteCount, audio: true),
                        Color.Black, 16 * scale);
                    }
                }
                else if (engineType is zzz_SoundManagerIns.EngineTypes.MagicOrchestra)
                {
                    var moinfo = soundMan.MagicOrchestraManager.GetMemoryInfo();
                    if (moinfo != null)
                    {
                        var str_wwise = $"{(moinfo.AnySoundsLoading ? "[LOADING SOUNDS...]\n" : "")}Sounds Loaded: {moinfo.SoundFileCount}\n" + GetMemoryUseString("Sound Memory:  ", moinfo.ByteCount);
                        //var strSize_wwise = DBG.DEBUG_FONT_SMALL.MeasureString(str_wwise);
                        var strSize_wwise = ImGuiDebugDrawer.MeasureString(str_wwise, 16 * scale);
                        //DBG.DrawOutlinedText(str_wwise, new Vector2(GFX.Device.Viewport.Width - 6,
                        //GFX.Device.Viewport.Height - 40) / DPIVector,
                        //GetMemoryUseColor(wwinfo.ByteCount, audio: true), DBG.DEBUG_FONT_SMALL, scale: 1, scaleOrigin: strSize_wwise);

                        ImGuiDebugDrawer.DrawText(str_wwise, (new Vector2(GFX.Device.Viewport.Width - 6,
                        GFX.Device.Viewport.Height - 32 * scale) / DPIVector) - strSize_wwise, GetMemoryUseColor(moinfo.ByteCount, audio: true),
                        Color.Black, 16 * scale);
                    }
                }
            }
            

            //DBG.DrawOutlinedText(str_unmanaged, new Vector2(GFX.Device.Viewport.Width - 6,
            //    GFX.Device.Viewport.Height - 16) / DPIVector,
            //    GetMemoryUseColor(MemoryUsage_Unmanaged), DBG.DEBUG_FONT_SMALL, scale: 1, scaleOrigin: strSize_unmanaged);
            //DBG.DrawOutlinedText(str_managed, new Vector2(GFX.Device.Viewport.Width - 6,
            //    GFX.Device.Viewport.Height) / DPIVector,
            //    GetMemoryUseColor(MemoryUsage_Managed), DBG.DEBUG_FONT_SMALL, scale: 1, scaleOrigin: strSize_managed);

            ImGuiDebugDrawer.DrawText(str_unmanaged, (new Vector2(GFX.Device.Viewport.Width - 6,
                GFX.Device.Viewport.Height - 16 * scale) / DPIVector) - strSize_unmanaged, GetMemoryUseColor(MemoryUsage_Unmanaged),
                Color.Black, 16 * scale);

            ImGuiDebugDrawer.DrawText(str_managed, (new Vector2(GFX.Device.Viewport.Width - 6,
                GFX.Device.Viewport.Height) / DPIVector) - strSize_managed, GetMemoryUseColor(MemoryUsage_Managed),
                Color.Black, 16 * scale);

            GFX.SpriteBatchEnd();
        }

        private void UpdateMemoryUsage()
        {
            using (var proc = Process.GetCurrentProcess())
            {
                MemoryUsage_Unmanaged = proc.PrivateMemorySize64;
            }
            MemoryUsage_Managed = GC.GetTotalMemory(forceFullCollection: false);
        }

        private void UpdateActiveState(bool? forceActive = null)
        {
            Minimized = !(Window.ClientBounds.Width > 0 && Window.ClientBounds.Height > 0);

            Active = forceActive ?? (!Minimized && IsActive && ApplicationIsActivated());

            ActiveHyst.Update(Active);

            if (!prevActive && Active)
            {
                IsFirstFrameActive = true;
            }

            prevActive = Active;
        }

        /// <summary>Returns true if the current application has focus, false otherwise</summary>
        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        public static bool IsFirstFrameAfterDrawing = false;

        private static void PreUpdate()
        {
            Input.PreUpdate();
            DialogManager.Input.PreUpdate();

            //Input.Update(new Rectangle(0, 0, Program.MainInstance.BoundsLastUpdatedFor.Width, Program.MainInstance.BoundsLastUpdatedFor.Height).InverseDpiScaled(),
            //                            forceUpdate: false, disableIfFieldsFocused: true);

            //DialogManager.Input.Update(new Rectangle(0, 0, Program.MainInstance.BoundsLastUpdatedFor.Width, Program.MainInstance.BoundsLastUpdatedFor.Height).InverseDpiScaled(),
            //                            forceUpdate: true, disableIfFieldsFocused: true);

            OSD.PreUpdate();
        }

        private static bool prevFrameKeyM = false;
        protected override void Update(GameTime gameTime)
        {
            if (REQUEST_EXIT && !REQUEST_EXIT_GUARANTEED_FINAL)
                WinForm.Close();

            if (REQUEST_EXIT_GUARANTEED_FINAL)
            {
                return;
            }

            //if (WinForm?.IsDisposed != false)
            //{
            //    Exit();
            //    return;
            //}

            if (true || IsFirstFrameAfterDrawing)
            {
                PreUpdate();
                IsFirstFrameAfterDrawing = false;
            }


            UpdateActiveState();

            if (ActiveHyst)
            {

                UpdateLazyDispatch(MenuBar.IsAnyMenuOpen);

                GlobalInputState.Update();

                bool ctrlHeld = GlobalInputState.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) ||
                    GlobalInputState.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl);

                bool shiftHeld = GlobalInputState.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) ||
                    GlobalInputState.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift);

                bool mHeld = GlobalInputState.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.M);

                if (ctrlHeld && shiftHeld && (mHeld && !prevFrameKeyM))
                {
                    MainThreadLazyDispatch(() =>
                    {
                        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                        GC.Collect();
                        //zzz_DocumentManager.CurrentDocument?.NotificationManager?.PushNotification("Memory cleaned.");
                    }, waitForAllLoadingTasks: true);
                }

                prevFrameKeyM = mHeld;
            }

            //lock (zzz_DocumentManager._lock_CurrentDocument)
            //{
            //    if (zzz_DocumentManager.CurrentDocument)
            //}


            lock (zzz_DocumentManager._lock_CurrentDocument)
            {
                Input?.UpdateEmergencyMouseUnlock();

                UpdateTime(gameTime.ElapsedGameTime);

                var newTitle = $"DS ANIM STUDIO {DSAS_VERSION_STRING}";

                if (!string.IsNullOrWhiteSpace(TAE_EDITOR?.NewFileContainerName))
                    newTitle = $"{System.IO.Path.GetFileName(TAE_EDITOR.NewFileContainerName)}" +
                        $"{(TAE_EDITOR.IsModified ? "*" : "")}" +
                        $"{(TAE_EDITOR.IsReadOnlyFileMode ? " !READ ONLY!" : "")}" +
                        $" - DS ANIM STUDIO {DSAS_VERSION_STRING}";

                if (IsPatreonBuild)
                    newTitle += " [PATREON EXLUSIVE VER, DO NOT REDISTRIBUTE]";

                if (IsNightfallBuild)
                    newTitle += " [NIGHTFALL MOD DEV VER]";


                if (IsLoadingTaskRunning)
                {
                    newTitle = "[Loading...] " + newTitle;
                }

                if (Main.IsDebugBuild)
                {

                    newTitle += $" [ActiveHyst={ActiveHyst.State},IsFixedTimeStep={IsFixedTimeStep},TargetElapsedTime(ms)={TargetElapsedTime.TotalMilliseconds},,MaxElapsedTime(ms)={MaxElapsedTime.TotalMilliseconds}]";
                }

                Window.Title = newTitle;

                if (ImguiFontRebuildRequested)
                {
                    DoActualImguiFontBuild();
                    ImguiFontRebuildRequested = false;
                }

                DebugToggleUpdate();

                UpdateWindowResizeStable();

                if (SplashManager.SplashState != SplashManager.SplashScreenStates.Complete)
                {
                    UpdateActiveState(true);
                    GlobalInputState.Update();
                    Input.Update(GFX.Device.Viewport.Bounds, forceUpdate: true, disableIfFieldsFocused: true);
                    DELTA_UPDATE = (float)gameTime.ElapsedGameTime.TotalSeconds;


                    SplashManager.UpdateSplash(DELTA_UPDATE);
                }
                else
                {
                    UpdateActiveState();
                }

                if (SplashManager.SplashState is SplashManager.SplashScreenStates.FadeIn or SplashManager.SplashScreenStates.Showing)
                    return;

                if (NeedsShowWelcome)
                {
                    if (!Config.WelcomeMessageDisabled)
                    {
                        OSD.RequestShowWelcome = true;
                        
                    }

                    NeedsShowWelcome = false;
                }

                if (REQUEST_REINIT_EDITOR)
                {
                    //RESET_ALL();
                    zzz_DocumentManager.KillCurrentDocForLoadFail();
                    REQUEST_REINIT_EDITOR = false;
                }

                if (REQUEST_DISABLE_SOUND)
                {
                    Main.Config.SimEnabled_Sounds = false;
                    zzz_DocumentManager.CurrentDocument.SoundManager.DisposeAll();
                    REQUEST_DISABLE_SOUND = false;
                }

                float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

                lock (_lock_ForceDrawTimer)
                {
                    if (ForceDrawTimer > 0)
                    {
                        ForceDrawTimer -= elapsed;
                        if (ForceDrawTimer < 0)
                            ForceDrawTimer = 0;
                    }
                }



                zzz_NotificationManagerIns.UpdateAll(elapsed);

                zzz_DocumentManager.CurrentDocument?.ParamManager?.UpdateAutoParamReload(elapsed);

                IsLoadingTaskRunning = zzz_DocumentManager.CurrentDocument?.LoadingTaskMan?.AnyInteractionBlockingTasks() ?? false;
                IsLoadingTaskRunningHyst.Update(IsLoadingTaskRunning);
                try
                {
                    bool isMainFormFocused = false;
                    if (WinForm.ContainsFocus)
                    {
                        isMainFormFocused = true;
                    }
                    else
                    {
                        Input.UnlockMouseCursor();
                    }

                    try
                    {

                        foreach (System.Windows.Forms.Form form in Application.OpenForms)
                        {
                            if (form.Modal)
                            {
                                UpdateActiveState();
                                GlobalInputState.Update();
                                try
                                {
                                    form.Invoke(new Action(() =>
                                    {
                                        try
                                        {
                                            if (isMainFormFocused && Active && GlobalInputState.Mouse.LeftButton ==
                                                Microsoft.Xna.Framework.Input.ButtonState.Released)
                                            {
                                                //UpdateActiveState(false);
                                                form.Activate();
                                            }
                                        }
                                        catch (Exception handled_ex) when (Main.EnableErrorHandler.ModalFormFocus)
                                        {
                                            //Main.HandleError(nameof(Main.EnableErrorHandler.ModalFormFocus), handled_ex);
                                        }
                                    }));
                                }
                                catch (Exception handled_ex) when (Main.EnableErrorHandler.ModalFormFocus)
                                {
                                    Main.HandleError(nameof(Main.EnableErrorHandler.ModalFormFocus), handled_ex);
                                }

                                //return;
                            }
                        }
                    }
                    catch (Exception handled_ex) when (Main.EnableErrorHandler.ModalFormFocus)
                    {
                        //Main.HandleError(nameof(Main.EnableErrorHandler.ModalFormFocus), handled_ex);
                    }



                    //GFX.Display.TargetFPS = 60;

                    zzz_DocumentManager.CurrentDocument?.LoadingTaskMan?.Update(elapsed);

                    MaxElapsedTime = TimeSpan.FromSeconds(1.0);

                    // Hotfix for mouse cursor getting locked when window loses focus while dragging editor timeline.
                    if (!ActiveHyst)
                    {
                        var tae = zzz_DocumentManager.CurrentDocument?.EditorScreen;
                        if (tae != null)
                        {
                            tae.Graph?.InputMan?.UpdateCurrentDrag(tae.Input, 0, isReadOnly: true,
                                forceReleaseDrag: true);
                        }
                    }


                    if (ActiveHyst || !GFX.Display.LimitFPSWhenWindowUnfocused)
                    {
                        IsFixedTimeStep = FIXED_TIME_STEP;

                        if (GFX.Display.DisableFPSLimit)
                            // Target = 0.1ms, so 10000 FPS
                            TargetElapsedTime = TimeSpan.FromTicks(1000);
                        else
                            // Target = TargetFPS
                            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / GFX.Display.TargetFPS);
                    }
                    else
                    {
                        if (zzz_DocumentManager.CurrentDocument.LoadingTaskMan.AnyInteractionBlockingTasks())
                        {
                            // Target = 60 FPS
                            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
                        }
                        else
                        {
                            // Target = 10 FPS
                            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 10.0);
                        }

                        IsFixedTimeStep = true;
                    }



                    bool forceDrawTimerActive = false;

                    lock (_lock_ForceDrawTimer)
                    {
                        if (ForceDrawTimer > 0)
                            forceDrawTimerActive = true;
                    }

                    if (!GFX.Display.StopUpdatingWhenWindowUnfocused)
                        forceDrawTimerActive = true;

                    if (ActiveHyst ||
                        zzz_DocumentManager.CurrentDocument.LoadingTaskMan.AnyInteractionBlockingTasks() ||
                        forceDrawTimerActive || !GFX.Display.StopUpdatingWhenWindowUnfocused)
                    {
                        GlobalInputState.Update();



                        FlverMaterialDefInfo.UpdateMem();

                        DELTA_UPDATE =
                            (float)gameTime.ElapsedGameTime
                                .TotalSeconds; //(float)(Math.Max(gameTime.ElapsedGameTime.TotalMilliseconds, 10) / 1000.0);

                        if (IsFirstFrameActive)
                            DELTA_UPDATE = 0;

                        //GFX.FlverDitherTime += DELTA_UPDATE;
                        //GFX.FlverDitherTime = GFX.FlverDitherTime % GFX.FlverDitherTimeMod;

                        if (!FIXED_TIME_STEP && GFX.AverageFPS >= 200)
                        {
                            DELTA_UPDATE_ROUNDED =
                                (float)(Math.Max(gameTime.ElapsedGameTime.TotalMilliseconds, 10) / 1000.0);
                        }
                        else
                        {
                            DELTA_UPDATE_ROUNDED = DELTA_UPDATE;
                        }

                        //if (!LoadingTaskMan.AnyTasksRunning())
                        //    Scene.UpdateAnimation();








                        if (DBG.EnableMenu)
                        {
                            DbgMenuItem.UpdateInput(elapsed);
                            DbgMenuItem.UICursorBlinkUpdate(elapsed);
                        }

                        //if (DbgMenuItem.MenuOpenState != DbgMenuOpenState.Open)
                        //{
                        //    // Only update input if debug menu isnt fully open.
                        //    GFX.World.UpdateInput(this, gameTime);
                        //}





                        MemoryUsageCheckTimer += elapsed;
                        if (MemoryUsageCheckTimer >= MemoryUsageCheckInterval)
                        {
                            MemoryUsageCheckTimer = 0;
                            UpdateMemoryUsage();
                        }


                        // BELOW IS TAE EDITOR STUFF

                        if (IsLoadingTaskRunning)
                        {
                            Input.CursorType = MouseCursorType.Loading;
                        }

                        //if (IsLoadingTaskRunning != prevFrameWasLoadingTaskRunning)
                        //{
                        //    TAE_EDITOR.GameWindowAsForm.Invoke(new Action(() =>
                        //    {
                        //        if (IsLoadingTaskRunning)
                        //        {
                        //            Mouse.SetCursor(MouseCursor.Wait);
                        //        }

                        //        foreach (Control c in TAE_EDITOR.GameWindowAsForm.Controls)
                        //        {
                        //            c.Enabled = !IsLoadingTaskRunning;
                        //        }


                        //    }));

                        //    // Undo an infinite loading cursor on an aborted file load.
                        //    if (!IsLoadingTaskRunning)
                        //    {
                        //        Mouse.SetCursor(MouseCursor.Arrow);
                        //    }
                        //}

                        if (!IsLoadingTaskRunning)
                        {
                            //MeasuredElapsedTime = UpdateStopwatch.Elapsed;
                            //MeasuredTotalTime = MeasuredTotalTime.Add(MeasuredElapsedTime);

                            //UpdateStopwatch.Restart();

                            // if (!TAE_EDITOR.Rect.Contains(TAE_EDITOR.Input.MousePositionPoint))
                            //     TAE_EDITOR.Input.CursorType = MouseCursorType.Arrow;

                            if (prevFrameWasLoadingTaskRunning)
                            {
                                TAE_EDITOR.Input.CursorType = MouseCursorType.Arrow;
                            }


                            if (Active || forceDrawTimerActive)
                            {
                                if (ActiveHyst)
                                {
                                    Input.Update(
                                        new Rectangle(0, 0, BoundsLastUpdatedFor.Width, BoundsLastUpdatedFor.Height)
                                            .InverseDpiScaled(),
                                        forceUpdate: false, disableIfFieldsFocused: true);

                                    GFX.CurrentWorldView?.UpdateInput();
                                }

                                if (zzz_DocumentManager.CurrentDocument.Scene.CheckIfDrawing())
                                    TAE_EDITOR.Update(DELTA_UPDATE);
                            }
                            else
                            {
                                TAE_EDITOR.Input.CursorType = MouseCursorType.Arrow;
                            }





                        }



                        prevFrameWasLoadingTaskRunning = IsLoadingTaskRunning;

                        IsFirstFrameActive = false;

                        GFX.CurrentWorldView.Update(DELTA_UPDATE);

                        try
                        {
                            zzz_DocumentManager.CurrentDocument.SoundManager.Update(DELTA_UPDATE,
                                GFX.CurrentWorldView.CameraLocationInWorld.WorldMatrix,
                                TAE_EDITOR);
                        }
                        catch (Exception handled_ex) when (Main.EnableErrorHandler.SoundUpdate)
                        {
                            Main.HandleError(nameof(Main.EnableErrorHandler.SoundUpdate), handled_ex);
                        }

                        TAE_EDITOR?.Graph?.AllBoxesEveryFrameUpdate();

                        base.Update(gameTime);
                    }
                }
                catch (Exception handled_ex) when (Main.EnableErrorHandler.MainUpdateLoop)
                {
                    Main.HandleError(nameof(Main.EnableErrorHandler.MainUpdateLoop), handled_ex);
                }
                finally
                {
                    UpdateLazyDispatch(MenuBar.IsAnyMenuOpen);
                }
                IsFirstUpdateLoop = false;
            }

            zzz_DocumentManager.UpdateDocuments();
            
        }

        private void InitTonemapShader()
        {

        }

        protected override void Draw(GameTime gameTime)
        {
            if (REQUEST_EXIT_GUARANTEED_FINAL)
            {
                return;
            }

            //if (WinForm?.IsDisposed != false)
            //{
            //    Exit();
            //    return;
            //}
            IsFirstFrameAfterDrawing = true;
            lock (zzz_DocumentManager._lock_CurrentDocument)
            {
                lock (_lock_ChangeRenderTarget)
                {



#if !DEBUG
            try
            {
#endif
                    if (SplashManager.SplashState is SplashManager.SplashScreenStates.FadeIn or SplashManager.SplashScreenStates.Showing)
                    {
                        GFX.Device.Clear(Color.Black);

                        ImGuiDraw.BeforeLayout(gameTime, 0, 0, BoundsLastUpdatedFor.Width, BoundsLastUpdatedFor.Height, 0);
                        ImGuiDebugDrawer.Begin();
                        ImGuiDebugDrawer.ViewportOffset = Vector2.Zero;

                        SplashManager.DrawSplash(clear: false);
                        GFX.Device.Viewport = new Viewport(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
                        ImGuiDebugDrawer.DrawTtest();
                        zzz_NotificationManagerIns.DrawAll(new Rectangle(32, 32, Window.ClientBounds.Width - 64, Window.ClientBounds.Height - 64));
                        ImGuiDebugDrawer.End();
                        ImGuiDraw.AfterLayout(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height, 0);
                        return;
                    }

                    bool forceDrawTimerActive = false;

                    lock (_lock_ForceDrawTimer)
                    {
                        if (ForceDrawTimer > 0)
                            forceDrawTimerActive = true;
                    }
                    if (!GFX.Display.StopUpdatingWhenWindowUnfocused)
                        forceDrawTimerActive = true;

                    bool updateInput = (ActiveHyst);


                    if (true || updateInput)
                    {
                        if (updateInput)
                        {
                            Input.Update(new Rectangle(0, 0, BoundsLastUpdatedFor.Width, BoundsLastUpdatedFor.Height).InverseDpiScaled(),
                                forceUpdate: false, disableIfFieldsFocused: true);
                        }

                        Colors.ReadColorsFromConfig();

                        DELTA_DRAW = (float)gameTime.ElapsedGameTime.TotalSeconds;// (float)(Math.Max(gameTime.ElapsedGameTime.TotalMilliseconds, 10) / 1000.0);

                        GFX.Device.Clear(Colors.MainColorBackground);

                        ImGuiDraw.BeforeLayout(gameTime, 0, 0, BoundsLastUpdatedFor.Width, BoundsLastUpdatedFor.Height, 0);


                        ImGuiDebugDrawer.Begin();

                        OSD.Build(Main.DELTA_DRAW, 0, 0);



                        if (DbgMenuItem.MenuOpenState != DbgMenuOpenState.Open)
                        {
                            // Only update input if debug menu isnt fully open.
                            if (updateInput)
                                GFX.CurrentWorldView.UpdateInput();
                        }

                        if (TAE_EDITOR.ModelViewerBounds.Width > 0 && TAE_EDITOR.ModelViewerBounds.Height > 0)
                        {

                            //TEST_DX11FLVER?.Draw(GFX.World.Matrix_World, GFX.World.Matrix_View, GFX.World.Matrix_Projection);
                            //ImGuiDebugDrawer.DrawTtest();
                            //ImGuiDebugDrawer.End();
                            //ImGuiDraw.AfterLayout(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height, 0);
                            //return;

                            if (RenderTarget0_Color == null)
                            {
                                RebuildRenderTarget(true);
                                if (TimeBeforeNextRenderTargetUpdate > 0)
                                    TimeBeforeNextRenderTargetUpdate -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                            }
                            else if (RequestViewportRenderTargetResolutionChange)
                            {
                                RebuildRenderTarget(false);

                                if (TimeBeforeNextRenderTargetUpdate > 0)
                                    TimeBeforeNextRenderTargetUpdate -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                            }


                            //GFX.Device.SetRenderTarget(UnusedRendertarget0);

                            //GFX.Device.Clear(Colors.MainColorViewportBackground);

                            //GFX.Device.Viewport = new Viewport(0, 0, UnusedRendertarget0.Width, UnusedRendertarget0.Height);

                            //GFX.LastViewport = new Viewport(TAE_EDITOR.ModelViewerBounds.DpiScaled());

                            //GFX.BeginDraw();
                            ////GFX.InitDepthStencil(writeDepth: false);
                            //DBG.DrawSkybox();

                            //GFX.Device.SetRenderTarget(null);


                            //GFX.Bokeh.Draw(SkyboxRenderTarget, GFX.BokehShapeHexagon, GFX.BokehRenderTarget,
                            //    GFX.BokehBrightness, GFX.BokehSize, GFX.BokehDownsize, GFX.BokehIsFullPrecision, GFX.BokehIsDynamicDownsize);

                            GFX.Device.SetRenderTarget(null);

                            //test


                            //GFX.Device.Viewport = new Viewport(TAE_EDITOR.ModelViewerBounds.DpiScaled());

                            //GFX.Device.SetRenderTarget(SceneRenderTarget);

                            GFX.Device.SetRenderTargets(RenderTarget0_Color);

                            GFX.Device.Clear(Colors.MainColorViewportBackground);

                            GFX.Device.Viewport = new Viewport(0, 0, RenderTarget0_Color.Width, RenderTarget0_Color.Height);

                            GFX.LastViewport = new Viewport(TAE_EDITOR.ModelViewerBounds.DpiScaled());

                            //GFX.SpriteBatchBegin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                            //GFX.SpriteBatch.Draw(SkyboxRenderTarget,
                            //new Rectangle(-SkyboxRenderTargetPadding, -SkyboxRenderTargetPadding,
                            //(TAE_EDITOR.ModelViewerBounds.Width + (SkyboxRenderTargetPadding * 2)) * GFX.EffectiveSSAA,
                            //(TAE_EDITOR.ModelViewerBounds.Height + (SkyboxRenderTargetPadding * 2)) * GFX.EffectiveSSAA), Color.White);
                            //GFX.SpriteBatchEnd();

                            GFX.Device.Clear(ClearOptions.DepthBuffer, Color.Transparent, 1, 0);
                            //GFX.Device.Clear(ClearOptions.Stencil, Color.Transparent, 1, 0);
                            GFX.BeginDraw();
                            DBG.DrawSkybox();
                            //TaeInterop.TaeViewportDrawPre(gameTime);
                            GFX.DrawScene3D();





                            //if (!DBG.DbgPrimXRay)
                            //    GFX.DrawSceneOver3D();
                            DBG.DrawGrid();

                            if (Main.HelperDraw.MASTER == true)
                                GFX.DrawPrimRespectDepth();

                            if (Main.HelperDraw.EnableXRayMode)
                                GFX.Device.Clear(ClearOptions.DepthBuffer, Color.Transparent, 1, 0);

                            ImGuiDebugDrawer.ViewportOffset = TAE_EDITOR.ModelViewerBounds.DpiScaled().TopLeftCorner();

                            TAE_EDITOR?.Graph?.ViewportInteractor?.GeneralUpdate_BeforePrimsDraw();

                            if (Main.HelperDraw.MASTER == true)
                                GFX.DrawSceneOver3D();

                            ImGuiDebugDrawer.ViewportOffset = Vector2.Zero;

                            GFX.Device.Clear(ClearOptions.DepthBuffer, Color.Transparent, 1, 0);

                            if (Main.HelperDraw.MASTER == true)
                                GFX.DrawPrimDisrespectDepth();

                            //GFX.Device.SetRenderTarget(null);
                            GFX.Device.SetRenderTargets();

                            GFX.Device.Clear(Colors.MainColorBackground);

                            GFX.Device.Viewport = new Viewport(TAE_EDITOR.ModelViewerBounds.DpiScaled());






                            if (ViewRenderTarget == RenderTargetViewTypes.None)
                            {
                                InitTonemapShader();
                                GFX.SpriteBatchBegin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                                if (GFX.UseTonemap)
                                {
                                    //MainFlverTonemapShader.Effect.Parameters["SpriteTexture"]?.SetValue(RenderTarget0_Color);

                                    MainFlverTonemapShader.Effect.SpriteTexture = RenderTarget0_Color;

                                    //MainFlverTonemapShader.Effect.Parameters["BlurMaskTexture"]?.SetValue(RenderTarget0_BlurMask);

                                    //MainFlverTonemapShader.Effect.Parameters["BlurAnisoPower"].SetValue(RenderTargetBlurAnisoPower);
                                    //MainFlverTonemapShader.Effect.Parameters["BlurAnisoMin"].SetValue(RenderTargetBlurAnisoMin);
                                    //MainFlverTonemapShader.Effect.Parameters["BlurAnisoMax"].SetValue(RenderTargetBlurAnisoMax);

                                    //MainFlverTonemapShader.Effect.Parameters["BlurDirections"].SetValue(RenderTargetBlurDirections);
                                    //MainFlverTonemapShader.Effect.Parameters["BlurQuality"].SetValue(RenderTargetBlurQuality);
                                    //MainFlverTonemapShader.Effect.Parameters["BlurSize"].SetValue(RenderTargetBlurSize);

                                    //MainFlverTonemapShader.Effect.Parameters["DebugBlurDisp"]?.SetValue(RenderTargetDebugBlurDisp);
                                    //MainFlverTonemapShader.Effect.Parameters["DebugBlurMaskDisp"]?.SetValue(RenderTargetDebugBlurMaskDisp);
                                    //MainFlverTonemapShader.Effect.Parameters["DebugDisableBlur"]?.SetValue(RenderTargetDebugDisableBlur);

                                    //MainFlverTonemapShader.Effect.Parameters["DebugDisableBlur"]?.SetValue(true);

                                    MainFlverTonemapShader.SSAA = GFX.EffectiveSSAA;

                                    MainFlverTonemapShader.ScreenSize = new Vector2(
                                        TAE_EDITOR.ModelViewerBounds.Width * Main.DPI,
                                        TAE_EDITOR.ModelViewerBounds.Height * Main.DPI);
                                    MainFlverTonemapShader.Effect.CurrentTechnique.Passes[0].Apply();
                                }

                                GFX.SpriteBatch.Draw(RenderTarget0_Color,
                                    new Rectangle(0, 0, TAE_EDITOR.ModelViewerBounds.Width, TAE_EDITOR.ModelViewerBounds.Height), Color.White);
                            }
                            else if (ViewRenderTarget == RenderTargetViewTypes.Color)
                            {
                                GFX.SpriteBatchBegin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                                GFX.SpriteBatch.Draw(RenderTarget0_Color,
                                    new Rectangle(0, 0, TAE_EDITOR.ModelViewerBounds.Width, TAE_EDITOR.ModelViewerBounds.Height), Color.White);
                            }
                            //else if (ViewRenderTarget == RenderTargetViewTypes.BlurMask)
                            //{
                            //    GFX.SpriteBatchBegin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                            //    GFX.SpriteBatch.Draw(RenderTarget0_BlurMask,
                            //        new Rectangle(0, 0, TAE_EDITOR.ModelViewerBounds.Width, TAE_EDITOR.ModelViewerBounds.Height), Color.White);
                            //}
                            else
                            {
                                throw new NotImplementedException();
                            }



                            if (RemoEventSim.CurrentFadeColor.HasValue)
                            {
                                GFX.SpriteBatchEnd();

                                GFX.Device.Viewport = new Viewport(TAE_EDITOR.ModelViewerBounds.DpiScaled());
                                GFX.SpriteBatchBegin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

                                GFX.SpriteBatch.Draw(TAE_EDITOR_BLANK_TEX,
                                        new Rectangle(0, 0, TAE_EDITOR.ModelViewerBounds.Width,
                                        TAE_EDITOR.ModelViewerBounds.Height), RemoEventSim.CurrentFadeColor.Value);
                            }

                            GFX.SpriteBatchEnd();

                            //if (GFX.CurrentWorldView.ShowGridType == WorldView.ShowGridTypes.NewGrid)
                            //{
                            //    //GFX.Device.Viewport = new Viewport(0, 0, RenderTarget0_Color.Width, RenderTarget0_Color.Height);
                            //    //GFX.Device.Clear(ClearOptions.DepthBuffer, Color.Transparent, 1, 0);
                            //    DBG.DrawGrid();
                            //}

                            //try
                            //{
                            //    using (var renderTarget3DScene = new RenderTarget2D(GFX.Device, TAE_EDITOR.ModelViewerBounds.Width * GFX.SSAA,
                            //   TAE_EDITOR.ModelViewerBounds.Height * GFX.SSAA, true, SurfaceFormat.Rgba1010102, DepthFormat.Depth24))
                            //    {
                            //        GFX.Device.SetRenderTarget(renderTarget3DScene);

                            //        GFX.Device.Clear(new Color(80, 80, 80, 255));

                            //        GFX.Device.Viewport = new Viewport(0, 0, TAE_EDITOR.ModelViewerBounds.Width * GFX.SSAA, TAE_EDITOR.ModelViewerBounds.Height * GFX.SSAA);
                            //        TaeInterop.TaeViewportDrawPre(gameTime);
                            //        GFX.DrawScene3D(gameTime);

                            //        GFX.Device.SetRenderTarget(null);

                            //        GFX.Device.Clear(new Color(80, 80, 80, 255));

                            //        GFX.Device.Viewport = new Viewport(TAE_EDITOR.ModelViewerBounds);

                            //        InitTonemapShader();
                            //        GFX.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                            //        //MainFlverTonemapShader.Effect.CurrentTechnique.Passes[0].Apply();
                            //        GFX.SpriteBatch.Draw(renderTarget3DScene,
                            //            new Rectangle(0, 0, TAE_EDITOR.ModelViewerBounds.Width, TAE_EDITOR.ModelViewerBounds.Height), Color.White);
                            //        GFX.SpriteBatch.End();
                            //    }
                            //}
                            //catch (SharpDX.SharpDXException ex)
                            //{
                            //    GFX.Device.Viewport = new Viewport(TAE_EDITOR.ModelViewerBounds);
                            //    GFX.Device.Clear(new Color(80, 80, 80, 255));

                            //    GFX.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                            //    //MainFlverTonemapShader.Effect.CurrentTechnique.Passes[0].Apply();
                            //    var errorStr = $"FAILED TO RENDER VIEWPORT AT {(zzz_DocumentManager.CurrentDocument.EditorScreen.ModelViewerBounds.Width * GFX.SSAA)}x{(zzz_DocumentManager.CurrentDocument.EditorScreen.ModelViewerBounds.Height * GFX.SSAA)} Resolution";
                            //    var errorStrPos = (Vector2.One * new Vector2(TAE_EDITOR.ModelViewerBounds.Width, TAE_EDITOR.ModelViewerBounds.Height) / 2.0f);

                            //    errorStrPos -= DBG.DEBUG_FONT.MeasureString(errorStr) / 2.0f;

                            //    GFX.SpriteBatch.DrawString(DBG.DEBUG_FONT, errorStr, errorStrPos - Vector2.One, Color.Black);
                            //    GFX.SpriteBatch.DrawString(DBG.DEBUG_FONT, errorStr, errorStrPos, Color.Red);
                            //    GFX.SpriteBatch.End();
                            //}

                        }



                        GFX.Device.Viewport = new Viewport(TAE_EDITOR.ModelViewerBounds.DpiScaled());
                        //DBG.DrawPrimitiveNames(gameTime);



                        //if (DBG.DbgPrimXRay)
                        //    GFX.DrawSceneOver3D();

                        GFX.DrawSceneGUI();


                        try
                        {
                            if (Config.ShowStatusInViewport is TaeEditor.TaeConfigFile.ViewportStatusTypes.Full
                                or TaeEditor.TaeConfigFile.ViewportStatusTypes.Condensed)
                                TAE_EDITOR?.Graph?.ViewportInteractor?.DrawStatusInViewport(Config.ShowStatusInViewport);
                        }
                        catch (Exception handled_ex) when (Main.EnableErrorHandler.ViewportStatusDisplay)
                        {
                            Main.HandleError(nameof(Main.EnableErrorHandler.ViewportStatusDisplay), handled_ex);
                        }

                        DrawMemoryUsage();

                        zzz_DocumentManager.CurrentDocument.LoadingTaskMan.DrawAllTasks();



                        GFX.Device.Viewport = new Viewport(0, 0, (int)Math.Ceiling(Window.ClientBounds.Width * 1f), (int)Math.Ceiling(Window.ClientBounds.Height * 1f));

                        try
                        {
                            TAE_EDITOR.Draw(GraphicsDevice, TaeEditorSpriteBatch,
                                TAE_EDITOR_BLANK_TEX, TAE_EDITOR_FONT,
                                (float)gameTime.ElapsedGameTime.TotalSeconds, TAE_EDITOR_FONT_SMALL,
                                TAE_EDITOR_SCROLLVIEWER_ARROW);
                        }
                        catch
                        {

                        }

                        GFX.Device.Viewport = new Viewport(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);



                        ImGuiDebugDrawer.DrawTtest();


                        if (SplashManager.SplashState is SplashManager.SplashScreenStates.FadeOut)
                        {
                            SplashManager.DrawSplash(clear: false);
                        }

                        zzz_NotificationManagerIns.DrawAll(new Rectangle(32, 32, Window.ClientBounds.Width - 64, Window.ClientBounds.Height - 64));

                        if (zzz_DocumentManager.CurrentDocument.SoundManager.DebugShowDiagnostics)
                        {
                            GFX.SpriteBatchBeginForText();
                            var diag = zzz_DocumentManager.CurrentDocument.SoundManager.GetDebugDiagnosticString();
                            var diagSize = ImGuiDebugDrawer.MeasureString(diag, 16);
                            var bgRect = new Rectangle((int)(64 - 16), (int)(64 - 16), Window.ClientBounds.Width - 96, Window.ClientBounds.Height - 96);
                            ImGuiDebugDrawer.DrawRect(bgRect, Color.Black * 0.6f, 4);
                            //GFX.SpriteBatch.Draw(TAE_EDITOR_BLANK_TEX, bgRect, null, Color.Black * 0.5f, 0, Vector2.Zero, SpriteEffects.None, 0.1f);
                            //DBG.DrawOutlinedText(diag, Vector2.One * 64, Color.Cyan, Main.TAE_EDITOR_FONT_SMALL, scale);
                            ImGuiDebugDrawer.DrawText(diag, Vector2.One * 64, Color.Cyan, Color.Black, 16);
                            GFX.SpriteBatchEnd();
                        }

                        OSD.PostBuild(Main.DELTA_DRAW, 0, 0);



                        ImGuiDebugDrawer.End();

                        ImGuiDraw.AfterLayout(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height, 0);




                        //DrawImGui(gameTime, 0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
                    }
                    //else
                    //{
                    //    // TESTING
                    //    GFX.Device.Clear(Color.Fuchsia);
                    //}
#if !DEBUG
            }
            catch (Exception ex)
            {
                if (!ErrorLog.HandleException(ex, "Fatal error ocurred during rendering"))
                {
                    Main.WinForm.Close();
                }
                GFX.Device.SetRenderTarget(null);
            }
#endif




                }
            }
            
        }

        static public bool IsProtocolHandlerInstalled()
        {
            return Registry.CurrentUser.OpenSubKey("SOFTWARE\\Classes\\dsas", false) != null;
        }

        static public void RegisterProtocolHandler()
        {
            const string URIScheme = "dsas";
            const string HandlerDisplayName = "DS Anim Studio";

            using (var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + URIScheme))
            {
                string applicationLocation = typeof(Main).Assembly.Location.Replace(".dll", ".exe");

                key.SetValue("", "URL:" + HandlerDisplayName);
                key.SetValue("URL Protocol", URIScheme);

                using (var defaultIcon = key.CreateSubKey("DefaultIcon"))
                {
                    defaultIcon.SetValue("", applicationLocation + ",1");
                }

                using (var commandKey = key.CreateSubKey(@"shell\open\command"))
                {
                    commandKey.SetValue("", "\"" + applicationLocation + "\" \"%1\"");
                }
            }
        }

        static public void UnregisterProtocolHandler()
        {
            Registry.CurrentUser.DeleteSubKey("SOFTWARE\\Classes\\dsas\\shell\\open\\command", false);
            Registry.CurrentUser.DeleteSubKey("SOFTWARE\\Classes\\dsas\\shell\\open", false);
            Registry.CurrentUser.DeleteSubKey("SOFTWARE\\Classes\\dsas\\shell", false);
            Registry.CurrentUser.DeleteSubKey("SOFTWARE\\Classes\\dsas\\DefaultIcon", false);
            Registry.CurrentUser.DeleteSubKey("SOFTWARE\\Classes\\dsas", false);
        }

    }
}

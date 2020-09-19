using DSAnimStudio.GFXShaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SoulsFormats;
using SFAnimExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static DSAnimStudio.TaeEditor.TaeEditAnimEventGraph;
using System.Diagnostics;
using SharpDX.DirectWrite;
using System.IO;

namespace DSAnimStudio.TaeEditor
{
    public class TaeEditorScreen
    {
        public const string BackupExtension = ".dsasbak";

        private ContentManager DebugReloadContentManager = null;

        private void BuildDebugMenuBar()
        {

            MenuBar.AddTopItem("Tools");

            MenuBar.AddItem("Tools", "Combo Viewer|F8", () =>
            {
                ShowComboMenu();
            }, startDisabled: true);
#if DEBUG
            MenuBar.AddItem("Tools", "Scan for Unused Animations", () =>
            {
                List<string> usedAnims = new List<string>();
                List<string> unusedAnims = new List<string>();
                foreach (var anim in SelectedTae.Animations)
                {
                    string hkx = Graph.ViewportInteractor.GetFinalAnimFileName(SelectedTae, anim);
                    if (!usedAnims.Contains(hkx))
                        usedAnims.Add(hkx);
                }
                foreach (var anim in Graph.ViewportInteractor.CurrentModel.AnimContainer.Animations.Keys)
                {
                    if (!usedAnims.Contains(anim) && !Graph.ViewportInteractor.CurrentModel.AnimContainer.AdditiveBlendOverlayNames.Contains(anim) 
                    && !unusedAnims.Contains(anim))
                        unusedAnims.Add(anim);
                }
                //var sb = new StringBuilder();
                foreach (var anim in unusedAnims)
                {
                    //sb.AppendLine(anim);
                    int id = int.Parse(anim.Replace(".hkx", "").Replace("_", "").Replace("a", ""));
                    var newAnim = new TAE.Animation(9_000_000000 + id, new TAE.Animation.AnimMiniHeader.Standard()
                    {
                        ImportHKXSourceAnimID = id,
                        ImportsHKX = true,
                    }, $"UNUSED:{anim.Replace(".hkx", "")}");
                    SelectedTae.Animations.Add(newAnim);
                }
                RecreateAnimList();
                //System.Windows.Forms.MessageBox.Show(sb.ToString());
            }, startDisabled: false);

            MenuBar.AddSeparator("Tools");
#endif

            MenuBar.AddItem("Tools", "Downgrade Sekiro/DS1R ANIBND(s)...", () =>
            {

                Main.WinForm.Invoke(new Action(() =>
                {
                    var browseDlg = new System.Windows.Forms.OpenFileDialog()
                    {
                        Filter = "*.ANIBND.DCX|*.ANIBND.DCX",
                        ValidateNames = true,
                        CheckFileExists = true,
                        CheckPathExists = true,
                        //ShowReadOnly = true,
                        Title = "Choose *.ANIBND.DCX file(s) to downgrade to Havok 2010.",
                        Multiselect = true,
                    };

                    var decision = browseDlg.ShowDialog();

                    if (decision == System.Windows.Forms.DialogResult.OK)
                    {
                        //if (System.IO.File.Exists(browseDlg.FileName + ".2010"))
                        //{
                        //    var nextDecision = System.Windows.Forms.MessageBox.Show("A '*.anibnd.dcx.2010' version of the selected file already exists. Would you like to downgrade all animations and overwrite this file anyways?", "Convert Again?", System.Windows.Forms.MessageBoxButtons.YesNo);

                        //    if (nextDecision == System.Windows.Forms.DialogResult.No)
                        //        return;
                        //}

                        LoadingTaskMan.DoLoadingTask("PreprocessSekiroAnimations_Multi", "Downgrading all selected ANIBNDs...", prog =>
                        {
                            int numNames = browseDlg.FileNames.Length;
                            for (int i = 0; i < numNames; i++)
                            {
                                string curName = browseDlg.FileNames[i];

                                LoadingTaskMan.DoLoadingTaskSynchronous($"PreprocessSekiroAnimations_{curName}", $"Downgrading {Utils.GetShortIngameFileName(curName)}...", prog2 =>
                                {

                                    try
                                    {

                                        string debug_testConvert_filename = curName;

                                        if (string.IsNullOrWhiteSpace(debug_testConvert_filename))
                                            throw new Exception("WHAT THE FUCKING FUCK GOD FUCKING DAMMIT");

                                        string folder = new System.IO.FileInfo(debug_testConvert_filename).DirectoryName;

                                        int lastSlashInFolder = folder.LastIndexOf("\\");

                                        string interroot = folder.Substring(0, lastSlashInFolder);

                                        string oodleSource = Utils.Frankenpath(interroot, "oo2core_6_win64.dll");

                                        string oodleTarget = Utils.Frankenpath(Main.Directory, "oo2core_6_win64.dll");

                                        // modengine check
                                        if (!File.Exists(oodleSource))
                                        {
                                            oodleSource = Utils.Frankenpath(interroot, @"..\oo2core_6_win64.dll");
                                        }

                                        if (System.IO.File.Exists(oodleSource) && !System.IO.File.Exists(oodleTarget))
                                        {
                                            System.IO.File.Copy(oodleSource, oodleTarget, true);

                                            System.Windows.Forms.MessageBox.Show("Oodle compression library was automatically copied from game directory " +
                                                "to editor's '/lib' directory and Sekiro files will load.\n\n", "Required Library Copied",
                                                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                                        }


                                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();


                                        HavokDowngrade.DowngradeAnibnd(debug_testConvert_filename, prog2);
                                        stopwatch.Stop();
                                        int minutes = (int)(stopwatch.Elapsed.TotalSeconds / 60);
                                        int seconds = (int)(Math.Round(stopwatch.Elapsed.TotalSeconds % 60));

                                        //if (minutes > 0)
                                        //    System.Windows.Forms.MessageBox.Show($"Created downgraded file (\"*.anibnd.dcx.2010\").\nElapsed Time: {minutes}m{seconds}s", "Animation Downgrading Complete");
                                        //else
                                        //    System.Windows.Forms.MessageBox.Show($"Created downgraded file (\"*.anibnd.dcx.2010\").\nElapsed Time: {seconds}s", "Animation Downgrading Complete");

                                    }
                                    catch (DllNotFoundException)
                                    {
                                        System.Windows.Forms.MessageBox.Show("Was unable to automatically find the " +
                                            "`oo2core_6_win64.dll` file in the Sekiro folder. Please copy that file to the " +
                                            "'lib' folder next to DS Anim Studio.exe in order to load Sekiro files.", "Unable to find compression DLL",
                                            System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                                    }

                                });

                                prog.Report(1.0 * (i + 1) / browseDlg.FileNames.Length);
                                //LoadingTaskMan.DoLoadingTaskSynchronous($"PreprocessSekiroAnimations_Multi_{i}",
                                //    $"Downgrading anim {(i + 1)} of {browseDlg.FileNames.Length}...", prog2 =>
                                //    {
                                //        DoAnimDowngrade(browseDlg.FileNames[i]);
                                //        prog.Report(1.0 * (i + 1) / browseDlg.FileNames.Length);

                                //    });
                            }
                        });



                    }

                }));






            });


            MenuBar.AddItem("Tools", "Import All PTDE ANIBNDs to DS1R...", () =>
            {
                var browseDlgPTDE = new System.Windows.Forms.OpenFileDialog()
                {
                    Filter = "*.EXE|*.EXE",
                    ValidateNames = true,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    //ShowReadOnly = true,
                    Title = "Select your PTDE DARKSOULS.EXE...",
                    Multiselect = false,
                };

                var browseDlgDS1R = new System.Windows.Forms.OpenFileDialog()
                {
                    Filter = "*.EXE|*.EXE",
                    ValidateNames = true,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    //ShowReadOnly = true,
                    Title = "Select your DS1R DarkSoulsRemastered.EXE...",
                    Multiselect = false,
                };

                if (browseDlgPTDE.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (browseDlgDS1R.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string folderPTDE = Utils.Frankenpath(new System.IO.FileInfo(browseDlgPTDE.FileName).DirectoryName, "chr\\");
                        string folderDS1R = Utils.Frankenpath(new System.IO.FileInfo(browseDlgDS1R.FileName).DirectoryName, "chr\\");

                        foreach (var file in System.IO.Directory.GetFiles(folderPTDE, "*.anibnd"))
                        {
                            string nickname = Utils.GetShortIngameFileName(file);
                            System.IO.File.Copy(file, Utils.Frankenpath(folderDS1R, $"{nickname}.anibnd.dcx.2010"), true);
                        }

                        System.Windows.Forms.MessageBox.Show("Imported all from PTDE.");
                    }
                }
            });

            MenuBar.AddItem("Tools", "Export Current TAE...", () =>
            {

                Main.WinForm.Invoke(new Action(() =>
                {
                    var browseDlg = new System.Windows.Forms.SaveFileDialog()
                    {
                        Filter = "TAE Files (*.tae)|*.tae",
                        ValidateNames = true,
                        CheckPathExists = true,
                        //ShowReadOnly = true,
                        Title = "Choose where to save loose TAE file.",

                    };

                    var decision = browseDlg.ShowDialog();

                    if (decision == System.Windows.Forms.DialogResult.OK)
                    {
                        try
                        {
                            SelectedTae.Write(browseDlg.FileName);
                            System.Windows.Forms.MessageBox.Show("TAE saved successfully.", "Saved");
                        }
                        catch (Exception exc)
                        {
                            System.Windows.Forms.MessageBox.Show($"Error saving TAE file:\n\n{exc}", "Failed to Save",
                                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        }
                    }

                }));






            }, startDisabled: true, closeOnClick: true, getEnabled: () => SelectedTae != null);


            //MenuBar.AddItem("[DEBUG]", "Load c5010", () =>
            //{
            //    LoadingTaskMan.DoLoadingTask("Debug_Load_c5010", "[Debug] Loading c5010", prog =>
            //    {
            //        var c5010 = GameDataManager.LoadCharacter("c5010");
            //        c5010.CurrentTransform = c5010.StartTransform = new Transform(Matrix.CreateTranslation(3, 0, 0));
            //        Scene.AddModel(c5010);
            //    });
                
            //});

            //MenuBar.AddItem("Tools", "TEST: Convert SplineCompressed to InterleavedUncompressed", () =>
            //{
            //    Havok.HKAnimRetarget.ConvertSplineXMLtoUncompressedXML(
            //        @"C:\DarkSoulsModding\HKXtreme\a00_0500_Spline.xml",
            //        @"C:\DarkSoulsModding\HKXtreme\c5270_a00_3017_Player.xml",
            //        @"C:\DarkSoulsModding\HKXtreme\c5270_a00_3017.xml",
            //        @"C:\DarkSoulsModding\HKXtreme\c0000_Skeleton.hkx",
            //        @"C:\DarkSoulsModding\HKXtreme\c5270_Skeleton.hkx");

            //    System.Windows.Forms.MessageBox.Show("DONE");
            //});

            //MenuBar.AddItem("Tools", "TEST: MIRROR ANIM", () =>
            //{
            //    Havok.HKAnimRetarget.MirrorSplineCompressedXMLtoInterleavedUncompressedXML(
            //        @"C:\DarkSoulsModding\HKXtreme\c0000_a25_3300.xml",
            //        @"C:\DarkSoulsModding\HKXtreme\c0000_a25_5300.xml",
            //        @"C:\DarkSoulsModding\HKXtreme\c0000_Skeleton.hkx");

            //    System.Windows.Forms.MessageBox.Show("DONE");
            //});

            //MenuBar.AddSeparator("Tools");

            //MenuBar.AddItem("Tools", "TEST: Fuck with current anim", () =>
            //{
            //    lock (Scene._lock_ModelLoad_Draw)
            //    {
            //        if (Scene.Models.Count > 0 && Scene.Models[0].AnimContainer.CurrentAnimation is NewHavokAnimation_InterleavedUncompressed anim)
            //        {
            //            for (int i = 0; i < anim.Transforms.Count; i++)
            //            {
            //                var t = anim.Transforms[i];

            //                t.Translation += Main.RandSignedVector3() * 0.1f;

            //                t.Rotation *= Quaternion.Normalize(Quaternion.CreateFromAxisAngle(Main.RandSignedVector3(), MathHelper.Pi * Main.RandSignedFloat() * 0.1f));

            //                anim.Transforms[i] = t;
            //            }
            //        }
            //    }

            //});

            //MenuBar.AddTopItem("[TEST: SCAN HKXPWV]", () =>
            //{

            //    foreach (var chrbndName in System.IO.Directory.GetFiles(
            //        @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls Prepare to Die Edition\DATA\chr", 
            //        "*.chrbnd"))
            //    {
            //        var bnd = BND3.Read(chrbndName);
            //        var chrShortName = Utils.GetShortIngameFileName(chrbndName);
            //        foreach (var hkxpwvEntry in bnd.Files.Where(x => x.Name.ToUpper().EndsWith(".HKXPWV")))
            //        {
            //            var hkxpwv = HKXPWV.Read(hkxpwvEntry.Bytes);

            //            HKX.HKASkeleton ragdollSkeleton = null;

            //            foreach (var ragdollHkxEntry in bnd.Files.Where(x => x.Name.ToLower().EndsWith($"{chrShortName}.hkx")))
            //            {
            //                try
            //                {
            //                    var skeletonHkx = HKX.Read(ragdollHkxEntry.Bytes, HKX.HKXVariation.HKXDS1);
            //                    foreach (var obj in skeletonHkx.DataSection.Objects)
            //                    {
            //                        if (obj is HKX.HKASkeleton asSkeleton)
            //                        {
            //                            ragdollSkeleton = asSkeleton;
            //                        }
            //                    }
            //                }
            //               catch
            //                {
            //                    Console.WriteLine($"FAILED TO READ RAGDOLL FOR {chrShortName}");
            //                }
            //            }

            //                Console.WriteLine($"{chrShortName}");

            //            Dictionary<int, List<string>> groups = new Dictionary<int, List<string>>();

            //            for (int i = 0; i < hkxpwv.RagdollBoneEntries.Count; i++)
            //            {
            //                if (hkxpwv.RagdollBoneEntries[i].NPCPartGroupIndex == 0)
            //                    continue;

            //                if (!groups.ContainsKey(hkxpwv.RagdollBoneEntries[i].NPCPartGroupIndex))
            //                    groups.Add(hkxpwv.RagdollBoneEntries[i].NPCPartGroupIndex, new List<string>());

            //                string boneName = null;
            //                if (ragdollSkeleton != null)
            //                    boneName = ragdollSkeleton.Bones[i].Name.GetString();

            //                groups[hkxpwv.RagdollBoneEntries[i].NPCPartGroupIndex].Add(boneName ?? $"RagdollBone[{i}]");
            //            }

            //            foreach (var kvp in groups)
            //            {
            //                Console.WriteLine($"    Part Group {kvp.Key}");
            //                foreach (var thing in kvp.Value)
            //                {
            //                    Console.WriteLine($"        {thing}");
            //                }
            //            }
            //        }
            //    }

            //    Console.WriteLine("DONE.");
            //});
            //MenuBar.AddTopItem("[TEST: NEW MODEL VIEWER]", () =>
            //{
            //    //GameDataManager.InterrootPath = @"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS III\Game";
            //    //GameDataManager.GameType = GameDataManager.GameTypes.DS3;

            //    //GameDataManager.LoadCharacter("c6200");

            //    LoadingTaskMan.DoLoadingTask("TEST MODEL VIEWER", "MODEL VIEWER TEST...", progress =>
            //    {
            //        //GameDataManager.Init(
            //        //    GameDataManager.GameTypes.DS1, 
            //        //    @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls Prepare to Die Edition\DATA");

            //        //GameDataManager.Init(
            //        //   GameDataManager.GameTypes.BB,
            //        //   @"E:\BloodborneRips\BB Fake Interroot\dvdroot_ps4");

            //        //var player = GameDataManager.LoadCharacter("c0000");

            //        GameDataManager.Init(
            //          GameDataManager.GameTypes.DS3,
            //          @"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS III\Game");

            //        var player = GameDataManager.LoadCharacter("c0000");

            //        player.CreateChrAsm();


            //        //player.ChrAsm.RightWeaponID = 306000; // DS1 Stone Greatsword


            //        //BB Foreign Set
            //        //player.ChrAsm.HeadID = 230000;
            //        //player.ChrAsm.BodyID = 231000;
            //        //player.ChrAsm.ArmsID = 232000;
            //        //player.ChrAsm.LegsID = 233000;

            //        //player.ChrAsm.RightWeaponID = 7100000; // BB Saw Spear

            //        //DS3 Firelink Set
            //        player.ChrAsm.HeadID = 21000000;
            //        player.ChrAsm.BodyID = 21001000;
            //        player.ChrAsm.ArmsID = 21002000;
            //        player.ChrAsm.LegsID = 21003000;
            //        player.ChrAsm.RightWeaponID = 12000000; // DS3 Whip
            //        player.ChrAsm.LeftWeaponID = 1455000;

            //        player.ChrAsm.UpdateModels();

            //        var test = Scene.Models;

            //        //foreach (var hitbox in player.ChrAsm.RightWeaponModel.DummyPolyMan.HitboxPrimitiveInfos)
            //        //{
            //        //    foreach (var prim in hitbox.Value.Primitives)
            //        //    {
            //        //        prim.EnableDraw = true;
            //        //    }
            //        //}

            //        //player.ChrAsm.RightWeaponModel.DummyPolyMan.ActivateAllHitboxes();
            //        player.DummyPolyMan.ActivateHitbox(ParamManager.AtkParam_Pc[4300000]);

            //        player.ChrAsm.RightWeaponModel.AnimContainer.CurrentAnimationName = "a043_030000.hkx";
            //        player.AnimContainer.CurrentAnimationName = "a043_030000.hkx";

            //        GameWindowAsForm.Invoke(new Action(() =>
            //        {
            //            var cboiceDict = new Dictionary<string, Action>();
            //            foreach (var kvp in player.ChrAsm.RightWeaponModel.AnimContainer.Animations)
            //            {
            //                cboiceDict.Add(kvp.Key, () => player.ChrAsm.RightWeaponModel.AnimContainer.CurrentAnimationName = kvp.Key);


            //            }

            //            MenuBar.AddItem("[WPN TEST]", "WPN Anim", cboiceDict, () => player.ChrAsm.RightWeaponModel.AnimContainer.CurrentAnimationName);
            //        }));

            //        Console.WriteLine("fatcat");
            //    });

            //    //GameDataManager.InterrootPath = @"E:\BloodborneRips\BB Fake Interroot\dvdroot_ps4";
            //    //GameDataManager.GameType = GameDataManager.GameTypes.BB;

            //    //GameDataManager.LoadCharacter("c2510");

            //    DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.DummyPoly] = true;

            //});

            //MenuBar.AddTopItem("[Reload Shaders]", () =>
            //{
            //    if (DebugReloadContentManager != null)
            //    {
            //        DebugReloadContentManager.Unload();
            //        DebugReloadContentManager.Dispose();
            //    }

            //    DebugReloadContentManager = new ContentManager(Main.ContentServiceProvider);


            //    GFX.FlverShader.Effect.Dispose();
            //    GFX.FlverShader = null;
            //    GFX.FlverShader = new FlverShader(DebugReloadContentManager.Load<Effect>(GFX.FlverShader__Name));

            //    Main.MainFlverTonemapShader.Effect.Dispose();
            //    Main.MainFlverTonemapShader = null;
            //    Main.MainFlverTonemapShader  = new FlverTonemapShader(DebugReloadContentManager.Load<Effect>($@"Content\Shaders\FlverTonemapShader"));

            //    GFX.InitShaders();
            //});

            //var useTonemapperCheckbox = new System.Windows.Forms.ToolStripMenuItem("Use Tonemapper");
            //useTonemapperCheckbox.CheckOnClick = true;
            //useTonemapperCheckbox.Checked = GFX.UseTonemap;
            //useTonemapperCheckbox.Text = $"Use Tonemapper: {(useTonemapperCheckbox.Checked ? "YES" : "NO")}";
            //useTonemapperCheckbox.CheckedChanged += (x, y) =>
            //{
            //    GFX.UseTonemap = useTonemapperCheckbox.Checked;
            //    useTonemapperCheckbox.Text = $"Use Tonemapper: {(useTonemapperCheckbox.Checked ? "YES" : "NO")}";
            //};

            //WinFormsMenuStrip.Items.Add(useTonemapperCheckbox);gon

            //MenuBar.AddItem("Debug", "Scan All TAE Test", () =>
            //{
            //    var dir = @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls Prepare to Die Edition\DATA\chr";
            //    var anibndNames = System.IO.Directory.GetFiles(dir, "*.anibnd");
            //    foreach (var anibnd in anibndNames)
            //    {
            //        var bnd = BND3.Read(anibnd);
            //        foreach (var bndFile in bnd.Files)
            //        {
            //            if (TAE.Is(bndFile.Bytes))
            //            {
            //                var tae = TAE.Read(bndFile.Bytes);
            //                tae.ApplyTemplate(TAE.Template.ReadXMLFile("Res\\TAE.Template.DS1.xml"));
            //                foreach (var anim in tae.Animations)
            //                {
            //                    foreach (var ev in anim.Events)
            //                    {
            //                        if (ev.Type == 120)
            //                        {
            //                            //Console.WriteLine("Breakpoint hit");
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //});
        }

        public void ShowComboMenu()
        {
            GameWindowAsForm.Invoke(new Action(() =>
            {
                if (!MenuBar["Tools\\Combo Viewer"].Enabled)
                    return;

                if (ComboMenu == null || ComboMenu.IsDisposed)
                {
                    ComboMenu = new TaeComboMenu();
                    ComboMenu.Owner = GameWindowAsForm;
                    ComboMenu.MainScreen = this;
                    ComboMenu.SetupTaeComboBoxes();
                }

                ComboMenu.Show();
                ComboMenu.Activate();
            }));
            
        }

        public TaeComboMenu ComboMenu = null;

        public float AnimSwitchRenderCooldown = 0;
        public float AnimSwitchRenderCooldownMax = 0.3f;
        public float AnimSwitchRenderCooldownFadeLength = 0.1f;
        public Color AnimSwitchRenderCooldownColor = Color.Black * 0.35f;

        private bool HasntSelectedAnAnimYetAfterBuildingAnimList = true;

        public void SetInspectorVisibility(bool visible)
        {
            inspectorWinFormsControl.Visible = visible;
        }

        enum DividerDragMode
        {
            None,
            LeftVertical,
            RightVertical,
            RightPaneHorizontal,
        }

        public enum ScreenMouseHoverKind
        {
            None,
            AnimList,
            EventGraph,
            Inspector,
            ModelViewer,
            DividerBetweenCenterAndLeftPane,
            DividerBetweenCenterAndRightPane,
            DividerRightPaneHorizontal,
            ShaderAdjuster
        }

        public DbgMenus.DbgMenuPadRepeater NextAnimRepeaterButton = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadDown, 0.4f, 0.016666667f);
        public DbgMenus.DbgMenuPadRepeater PrevAnimRepeaterButton = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadUp, 0.4f, 0.016666667f);

        public DbgMenus.DbgMenuPadRepeater NextFrameRepeaterButton = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadLeft, 0.3f, 0.016666667f);
        public DbgMenus.DbgMenuPadRepeater PrevFrameRepeaterButton = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadRight, 0.3f, 0.016666667f);

        public static bool CurrentlyEditingSomethingInInspector;
        
        public class FindInfoKeep
        {
            public enum TaeSearchType : int
            {
                ParameterValue = 0,
                ParameterName = 1,
                EventName = 2,
                EventType = 3,
            }

            public string SearchQuery;
            public bool MatchEntireString;
            public List<TaeFindResult> Results;
            public int HighlightedIndex;
            public TaeSearchType SearchType;
        }

        public FindInfoKeep LastFindInfo = null;
        public TaeFindValueDialog FindValueDialog = null;

        public TaePlaybackCursor PlaybackCursor => Graph?.PlaybackCursor;

        public Rectangle ModelViewerBounds;
        public Rectangle ModelViewerBounds_InputArea;

        private const int RECENT_FILES_MAX = 32;

        private int TopMenuBarMargin => (int)Math.Round(WinFormsMenuStrip.Size.Height / Main.DPIY);

        private int TopOfGraphAnimInfoMargin = 20;

        private int TransportHeight = 28;

        public TaeTransport Transport;

        private int ButtonEditCurrentAnimInfoWidth = 128;
        public NoPaddingButton ButtonEditCurrentAnimInfo;

        private int ButtonGotoEventSourceWidth = 140;
        public NoPaddingButton ButtonGotoEventSource;

        public void GoToEventSource()
        {
            if (Graph.AnimRef.MiniHeader is TAE.Animation.AnimMiniHeader.ImportOtherAnim asImportOtherAnim)
            {
                var animRef = FileContainer.GetAnimRefFull(asImportOtherAnim.ImportFromAnimID);

                SelectNewAnimRef(animRef.Item1, animRef.Item2);
            }
            else if (Graph.AnimRef.MiniHeader is TAE.Animation.AnimMiniHeader.Standard asStandard)
            {
                if (asStandard.AllowDelayLoad)
                {
                    var animRef = FileContainer.GetAnimRefFull(asStandard.ImportHKXSourceAnimID);

                    SelectNewAnimRef(animRef.Item1, animRef.Item2);
                }
            }
        }

        private int EditTaeHeaderButtonMargin = 32;
        private int EditTaeHeaderButtonHeight = 20;
        public NoPaddingButton ButtonEditCurrentTaeHeader;

        //public bool CtrlHeld;
        //public bool ShiftHeld;
        //public bool AltHeld;

        const string HELP_TEXT =
            "Left Click + Drag on Timeline:\n" +
            "    Scrub animation frame.\n" +
            "Left Click + Hold Shift + Drag on Timeline:\n" +
            "    Scrub animation frame while ignoring autoscroll.\n" +
            "Left Click + Drag Middle of Event:\n" +
            "    Move whole event.\n" +
            "Left Click + Drag Left/Right Side of Event:\n" +
            "    Move start/end of event.\n" +
            "Left Click:\n" +
            "    Highlight event under mouse cursor.\n" +
            "Left Click and Drag:\n" +
            "    Drag selection rectangle to highlight multiple events.\n" +
            "Shift + Left Click:\n" +
            "    Add to current selection (works for multiselect as well).\n" +
            "Ctrl + Left Click:\n" +
            "    Subtract from current selection (works for multiselect as well).\n" +
            "Right Click:\n" +
            "    Place copy of last highlighted event at mouse cursor.\n" +
            "Delete Key:\n" +
            "    Delete highlighted event.\n" +
            "Ctrl+X/Ctrl+C/Ctrl+V:\n" +
            "    CUT/COPY/PASTE.\n" +
            "Ctrl+Z/Ctrl+Y:\n" +
            "    UNDO/REDO.\n" +
            "F1 Key:\n" +
            "    Change type of highlighted event.\n" +
            "Space Bar:\n" +
            "    Play/Pause Anim.\n" +
            "Shift+Space Bar:\n" +
            "    Play Anim from beginning.\n" +
            "R Key:\n" +
            "    Reset root motion (useful for root motion accumulation option).\n" +
            "Ctrl+Mouse Wheel:\n" +
            "    Zoom timeline in/out.\n" +
            "Ctrl+(+/-/0):\n" +
            "   Zoom in/out/reset, exactly like in web browsers.\n" +
            "Left/Right Arrow Keys:\n" +
            "    Goes to previous/next frame.\n" +
            "Home/End:\n" +
            "    Without Shift: Go to playback start point / playback end point.\n" +
            "    With Shift: Go to start of animation / end of animation.\n" +
            "    With Ctrl: Stop playing animation after jumping.\n" +
            "    (Ctrl and Shift can be combined.)\n";

        private static object _lock_PauseUpdate = new object();
        private bool _PauseUpdate;
        private bool PauseUpdate
        {
            get
            {
                lock (_lock_PauseUpdate)
                    return _PauseUpdate;
            }
            set
            {
                lock (_lock_PauseUpdate)
                    _PauseUpdate = value;
            }
        }
        //private float _PauseUpdateTotalTime;
        //private float PauseUpdateTotalTime
        //{
        //    get
        //    {
        //        lock (_lock_PauseUpdate)
        //            return _PauseUpdateTotalTime;
        //    }
        //    set
        //    {
        //        lock (_lock_PauseUpdate)
        //            _PauseUpdateTotalTime = value;
        //    }
        //}

        public Rectangle Rect;

        public Dictionary<TAE.Animation, TaeUndoMan> UndoManDictionary 
            = new Dictionary<TAE.Animation, TaeUndoMan>();

        public TaeUndoMan UndoMan
        {
            get
            {
                if (!UndoManDictionary.ContainsKey(SelectedTaeAnim))
                {
                    var newUndoMan = new TaeUndoMan(this);
                    newUndoMan.CanUndoMaybeChanged += UndoMan_CanUndoMaybeChanged;
                    newUndoMan.CanRedoMaybeChanged += UndoMan_CanRedoMaybeChanged;
                    UndoManDictionary.Add(SelectedTaeAnim, newUndoMan);
                }
                return UndoManDictionary[SelectedTaeAnim];
            }
        }

        public bool IsModified
        {
            get
            {
                try
                {
                    return (SelectedTae?.Animations.Any(a => a.GetIsModified()) ?? false) ||
                    (FileContainer?.AllTAE.Any(t => t.GetIsModified()) ?? false) || (FileContainer?.IsModified ?? false);
                }
                catch
                {
                    return true;
                }
            }
        }
            

        public void UpdateIsModifiedStuff()
        {
            GameWindowAsForm.Invoke(new Action(() =>
            {
                MenuBar["File\\Save"].Enabled = IsModified;
            }));
        }

        public TaeMenuBarBuilder MenuBar { get; private set; }

        private void PushNewRecentFile(string fileName)
        {
            while (Config.RecentFilesList.Contains(fileName))
                Config.RecentFilesList.Remove(fileName);

            while (Config.RecentFilesList.Count >= RECENT_FILES_MAX)
                Config.RecentFilesList.RemoveAt(Config.RecentFilesList.Count - 1);

            Config.RecentFilesList.Insert(0, fileName);

            SaveConfig();

            CreateRecentFilesList();
        }

        private void CreateRecentFilesList()
        {
            GameWindowAsForm.Invoke(new Action(() =>
            {
                MenuBar["File\\Recent Files"].DropDownItems.Clear();
                var toolStripFileRecentClear = new System.Windows.Forms.ToolStripMenuItem("Clear All Recent Files...");
                toolStripFileRecentClear.Click += (s, e) =>
                {
                    var askYesNoResult = System.Windows.Forms.MessageBox.Show(
                        "Are you sure you wish to remove all recent files?",
                        "Remove All Recent Files?",
                        System.Windows.Forms.MessageBoxButtons.YesNo);

                    if (askYesNoResult == System.Windows.Forms.DialogResult.Yes)
                    {
                        Config.RecentFilesList.Clear();
                        SaveConfig();
                        CreateRecentFilesList();
                    }
                };
                MenuBar["File\\Recent Files"].DropDownItems.Add(toolStripFileRecentClear);
                MenuBar["File\\Recent Files"].DropDownItems.Add(new System.Windows.Forms.ToolStripSeparator());
                foreach (var f in Config.RecentFilesList)
                {
                    var thisRecentFileEntry = new System.Windows.Forms.ToolStripMenuItem(f);
                    thisRecentFileEntry.Click += (s, e) =>
                    {
                        DirectOpenFile(f);
                    };
                    MenuBar["File\\Recent Files"].DropDownItems.Add(thisRecentFileEntry);
                }
            }));
        }

        private void UndoMan_CanRedoMaybeChanged(object sender, EventArgs e)
        {
            MenuBar["Edit\\Redo"].Enabled = UndoMan.CanRedo;
        }

        private void UndoMan_CanUndoMaybeChanged(object sender, EventArgs e)
        {
            MenuBar["Edit\\Undo"].Enabled = UndoMan.CanUndo;
        }


        private TaeButtonRepeater UndoButton = new TaeButtonRepeater(0.4f, 0.05f);
        private TaeButtonRepeater RedoButton = new TaeButtonRepeater(0.4f, 0.05f);

        private float LeftSectionWidth = 236;
        private const float LeftSectionWidthMin = 120;
        private float DividerLeftVisibleStartX => Rect.Left + LeftSectionWidth;
        private float DividerLeftVisibleEndX => Rect.Left + LeftSectionWidth + DividerVisiblePad;

        private float RightSectionWidth = 600;
        private const float RightSectionWidthMin = 320;
        private float DividerRightVisibleStartX => Rect.Right - RightSectionWidth - DividerVisiblePad;
        private float DividerRightVisibleEndX => Rect.Right - RightSectionWidth;


        private float DividerRightCenterX => DividerRightVisibleStartX + ((DividerRightVisibleEndX - DividerRightVisibleStartX) / 2);
        private float DividerLeftCenterX => DividerLeftVisibleStartX + ((DividerLeftVisibleEndX - DividerLeftVisibleStartX) / 2);

        private float DividerRightGrabStartX => DividerRightCenterX - (DividerHitboxPad / 2);
        private float DividerRightGrabEndX => DividerRightCenterX + (DividerHitboxPad / 2);

        private float DividerLeftGrabStartX => DividerLeftCenterX - (DividerHitboxPad / 2);
        private float DividerLeftGrabEndX => DividerLeftCenterX + (DividerHitboxPad / 2);

        private float TopRightPaneHeight = 600;
        private const float TopRightPaneHeightMinNew = 128;
        private const float BottomRightPaneHeightMinNew = 64;

        private float DividerRightPaneHorizontalVisibleStartY => Rect.Top + TopRightPaneHeight + TopMenuBarMargin + TransportHeight;
        private float DividerRightPaneHorizontalVisibleEndY => Rect.Top + TopRightPaneHeight + DividerVisiblePad + TopMenuBarMargin + TransportHeight;
        private float DividerRightPaneHorizontalCenterY => DividerRightPaneHorizontalVisibleStartY + ((DividerRightPaneHorizontalVisibleEndY - DividerRightPaneHorizontalVisibleStartY) / 2);

        private float DividerRightPaneHorizontalGrabStartY => DividerRightPaneHorizontalCenterY - (DividerHitboxPad / 2);
        private float DividerRightPaneHorizontalGrabEndY => DividerRightPaneHorizontalCenterY + (DividerHitboxPad / 2);

        private float LeftSectionStartX => Rect.Left;
        private float MiddleSectionStartX => DividerLeftVisibleEndX;
        private float RightSectionStartX => Rect.Right - RightSectionWidth;

        private float MiddleSectionWidth => DividerRightVisibleStartX - DividerLeftVisibleEndX;
        private const float MiddleSectionWidthMin = 128;

        private float DividerVisiblePad = 3;
        private int DividerHitboxPad = 10;

        private DividerDragMode CurrentDividerDragMode = DividerDragMode.None;

        public ScreenMouseHoverKind MouseHoverKind = ScreenMouseHoverKind.None;
        private ScreenMouseHoverKind oldMouseHoverKind = ScreenMouseHoverKind.None;
        public ScreenMouseHoverKind WhereCurrentMouseClickStarted = ScreenMouseHoverKind.None;

        public TaeFileContainer FileContainer;

        public TAE SelectedTae { get; private set; }

        public TAE.Animation SelectedTaeAnim { get; private set; }
        private TaeScrollingString SelectedTaeAnimInfoScrollingText = new TaeScrollingString();

        public readonly System.Windows.Forms.Form GameWindowAsForm;

        public void UpdateInspectorToSelection()
        {
            GameWindowAsForm.Invoke(new Action(() =>
            {
                if (SelectedEventBox == null)
                {
                    //if (MultiSelectedEventBoxes.Count == 1)
                    //{
                    //    SelectedEventBox = MultiSelectedEventBoxes[0];
                    //    MultiSelectedEventBoxes.Clear();
                    //    inspectorWinFormsControl.labelEventType.Text =
                    //        SelectedEventBox.MyEvent.EventType.ToString();
                    //    inspectorWinFormsControl.buttonChangeType.Enabled = true;
                    //}
                    if (MultiSelectedEventBoxes.Count > 0)
                    {
                        inspectorWinFormsControl.labelEventType.Text = "Multiple Events Selected\nSelect single event to edit.";
                        inspectorWinFormsControl.buttonChangeType.Enabled = false;
                        inspectorWinFormsControl.buttonChangeType.Visible = false;
                    }
                    else
                    {
                        inspectorWinFormsControl.labelEventType.Text = "No Event Selected\nSelect event to edit.";
                        inspectorWinFormsControl.buttonChangeType.Enabled = false;
                        inspectorWinFormsControl.buttonChangeType.Visible = false;
                    }
                }
                else
                {
                    inspectorWinFormsControl.labelEventType.Text =
                        (SelectedEventBox.MyEvent.TypeName != null ? ($"{SelectedEventBox.MyEvent.TypeName}[{SelectedEventBox.MyEvent.Type}]") : $"Event Type {SelectedEventBox.MyEvent.Type}")
                        + $"\n{SelectedEventBox.MyEvent.StartTime:0.00} -> {SelectedEventBox.MyEvent.EndTime:0.00} (Duration: {(SelectedEventBox.MyEvent.EndTime - SelectedEventBox.MyEvent.StartTime):0.00})";
                        inspectorWinFormsControl.buttonChangeType.Enabled = true;
                        inspectorWinFormsControl.buttonChangeType.Visible = true;
                }
            }));
        }

        public void RefocusInspectorToPreventBeepWhenYouHitSpace()
        {
            inspectorWinFormsControl.Focus();
        }

        public TaeEditAnimEventBox PrevHoveringOverEventBox = null;

        public TaeEditAnimEventBox HoveringOverEventBox = null;

        private TaeEditAnimEventBox _selectedEventBox = null;
        public TaeEditAnimEventBox SelectedEventBox
        {
            get => _selectedEventBox;
            set
            {
                //inspectorWinFormsControl.DumpDataGridValuesToEvent();

                //if (value != null && value != _selectedEventBox)
                //{
                //    if (Config.UseGamesMenuSounds)
                //        FmodManager.PlaySE("f000000000");
                //}

                _selectedEventBox = value;

                if (_selectedEventBox == null)
                {
                    //inspectorWinFormsControl.buttonChangeType.Enabled = false;
                }
                else
                {
                    //inspectorWinFormsControl.buttonChangeType.Enabled = true;

                    // If one box was just selected, clear the multi-select
                    MultiSelectedEventBoxes.Clear();
                }
                if (inspectorWinFormsControl.SelectedEventBox != null)
                    inspectorWinFormsControl.SelectedEventBox.UpdateEventText();

                inspectorWinFormsControl.SelectedEventBox = _selectedEventBox;

                UpdateInspectorToSelection();
            }
        }

        public List<TaeEditAnimEventBox> MultiSelectedEventBoxes = new List<TaeEditAnimEventBox>();
        private int multiSelectedEventBoxesCountLastFrame = 0;

        public TaeEditAnimList AnimationListScreen;
        public TaeEditAnimEventGraph Graph { get; private set; }
        public bool IsCurrentlyLoadingGraph { get; private set; } = false;
        //private Dictionary<TAE.Animation, TaeEditAnimEventGraph> GraphLookup = new Dictionary<TAE.Animation, TaeEditAnimEventGraph>();
        //private TaeEditAnimEventGraphInspector editScreenGraphInspector;

        private Color ColorInspectorBG = Color.DarkGray;
        public TaeInspectorWinFormsControl inspectorWinFormsControl;

        public FancyInputHandler Input => Main.Input;

        public System.Windows.Forms.MenuStrip WinFormsMenuStrip;

        public string FileContainerName = "";
        public string FileContainerName_2010 => FileContainerName + ".2010";

        public bool IsReadOnlyFileMode = false;

        public TaeConfigFile Config = new TaeConfigFile();

        private static string ConfigFilePath = null;

        private static void CheckConfigFilePath()
        {
            if (ConfigFilePath == null)
            {
                var currentAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var currentAssemblyDir = System.IO.Path.GetDirectoryName(currentAssemblyPath);
                ConfigFilePath = System.IO.Path.Combine(currentAssemblyDir, "DSAnimStudio_Config.json");
            }
        }

        public void LoadConfig()
        {
            CheckConfigFilePath();
            if (!System.IO.File.Exists(ConfigFilePath))
            {
                Config = new TaeConfigFile();
                SaveConfig();
            }

            var jsonText = System.IO.File.ReadAllText(ConfigFilePath);

            Config = Newtonsoft.Json.JsonConvert.DeserializeObject<TaeConfigFile>(jsonText);

            Config.AfterLoading();
        }

        public void SaveConfig()
        {
            if (Graph != null)
            {
                // I'm sorry; this is pecularily placed.
                Graph.ViewportInteractor.SaveChrAsm();
            }

            Config.BeforeSaving();

            CheckConfigFilePath();

            var jsonText = Newtonsoft.Json.JsonConvert
                .SerializeObject(Config,
                Newtonsoft.Json.Formatting.Indented);

            System.IO.File.WriteAllText(ConfigFilePath, jsonText);
        }

        public bool? LoadCurrentFile()
        {
            Graph?.ViewportInteractor?.CloseEquipForm();

            IsCurrentlyLoadingGraph = true;
            Scene.DisableModelDrawing();
            Scene.DisableModelDrawing2();

            // Even if it fails to load, just always push it to the recent files list
            PushNewRecentFile(FileContainerName);

            //string templateName = BrowseForXMLTemplate();

            //if (templateName == null)
            //{
            //    return false;
            //}

            if (System.IO.File.Exists(FileContainerName))
            {
                FileContainer = new TaeFileContainer();

                try
                {
                    string folder = new System.IO.FileInfo(FileContainerName).DirectoryName;

                    int lastSlashInFolder = folder.LastIndexOf("\\");

                    string interroot = folder.Substring(0, lastSlashInFolder);

                    string oodleSource = Utils.Frankenpath(interroot, "oo2core_6_win64.dll");

                    // modengine check
                    if (!File.Exists(oodleSource))
                    {
                        oodleSource = Utils.Frankenpath(interroot, @"..\oo2core_6_win64.dll");
                    }

                    //if (!File.Exists(oodleSource))
                    //{
                    //    System.Windows.Forms.MessageBox.Show("Was unable to automatically find the " +
                    //    "`oo2core_6_win64.dll` file in the Sekiro folder. Please copy that file to the " +
                    //    "'lib' folder next to DS Anim Studio.exe in order to load Sekiro files.", "Unable to find compression DLL",
                    //    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);

                    //    return false;
                    //}

                    string oodleTarget = Utils.Frankenpath(Main.Directory, "oo2core_6_win64.dll");

                    if (System.IO.File.Exists(oodleSource) && !System.IO.File.Exists(oodleTarget))
                    {
                        System.IO.File.Copy(oodleSource, oodleTarget, true);

                        System.Windows.Forms.MessageBox.Show("Oodle compression library was automatically copied from game directory " +
                            "to editor's '/lib' directory and Sekiro files will load.\n\n", "Required Library Copied", 
                            System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    }

                    FileContainer.LoadFromPath(FileContainerName);
                }
                catch (System.DllNotFoundException)
                {
                    //System.Windows.Forms.MessageBox.Show("Cannot open Sekiro files unless you " +
                    //    "copy the `oo2core_6_win64.dll` file from the Sekiro folder into the " +
                    //    "'lib' folder next to DS Anim Studio.exe.", "Additional DLL Required", 
                    //    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);

                    System.Windows.Forms.MessageBox.Show("Was unable to automatically find the " +
                        "`oo2core_6_win64.dll` file in the \"Sekiro\" folder. Please copy that file to the " +
                        "'lib' folder next to DS Anim Studio.exe in order to load SDT files.", "Unable to find compression DLL",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);

                    return false;
                }
                

                if (!FileContainer.AllTAE.Any())
                {
                    return true;
                }

                LoadTaeFileContainer(FileContainer);

                GameWindowAsForm.Invoke(new Action(() =>
                {
                    MenuBar["File\\Save As..."].Enabled = !IsReadOnlyFileMode;
                    MenuBar["File\\Force Ingame Character Reload Now (DS3/DS1R Only)"].Enabled = !IsReadOnlyFileMode;
                    MenuBar["File\\Reload GameParam"].Enabled = true;
                }));

                //if (templateName != null)
                //{
                //    LoadTAETemplate(templateName);
                //}

                IsCurrentlyLoadingGraph = false;

                return true;
            }
            else
            {
                return null;
            }
        }

        private void CheckAutoLoadXMLTemplate()
        {
            var objCheck = Utils.GetFileNameWithoutAnyExtensions(Utils.GetFileNameWithoutDirectoryOrExtension(FileContainerName)).ToLower().StartsWith("o");

            //var xmlPath = System.IO.Path.Combine(
            //    new System.IO.FileInfo(typeof(TaeEditorScreen).Assembly.Location).DirectoryName,
            //    $@"Res\TAE.Template.{(FileContainer.IsBloodborne ? "BB" : SelectedTae.Format.ToString())}{(objCheck ? ".OBJ" : "")}.xml");

            var xmlPath = System.IO.Path.Combine(
                new System.IO.FileInfo(typeof(TaeEditorScreen).Assembly.Location).DirectoryName,
                $@"Res\TAE.Template.{SelectedTae.Format}{((objCheck && (GameDataManager.GameType == GameDataManager.GameTypes.DS1 || GameDataManager.GameType == GameDataManager.GameTypes.DS1R)) ? ".OBJ" : "")}.xml");

            if (System.IO.File.Exists(xmlPath))
                LoadTAETemplate(xmlPath);
        }

        public void SaveCurrentFile(Action afterSaveAction = null, string saveMessage = "Saving ANIBND...")
        {
            if (IsReadOnlyFileMode)
            {
                System.Windows.Forms.MessageBox.Show("Read-only mode is" +
                    " active so nothing was saved. To open a file in re-saveable mode," +
                    " make sure the Read-Only checkbox is unchecked in the open" +
                    " file dialog.", "Read-Only Mode Active",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Stop);
                return;
            }

            if (System.IO.File.Exists(FileContainerName) && 
                !System.IO.File.Exists(FileContainerName + BackupExtension))
            {
                System.IO.File.Copy(FileContainerName, FileContainerName + BackupExtension);
                System.Windows.Forms.MessageBox.Show(
                    "A backup was not found and was created:\n" + FileContainerName + BackupExtension,
                    "Backup Created", System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Information);
            }

            LoadingTaskMan.DoLoadingTask("SaveFile", saveMessage, progress =>
            {
                FileContainer.SaveToPath(FileContainerName, progress);

                if (Config.LiveRefreshOnSave)
                {
                    LiveRefresh();
                }

                progress.Report(1.0);

                afterSaveAction?.Invoke();
            });

            
        }

        private void LoadAnimIntoGraph(TAE.Animation anim)
        {
            //if (!GraphLookup.ContainsKey(anim))
            //{
            //    var graph = new TaeEditAnimEventGraph(this, false, anim);
            //    GraphLookup.Add(anim, graph);

            //}

            //Graph = GraphLookup[anim];

            if (Graph == null)
                Graph = new TaeEditAnimEventGraph(this, false, anim);
            else
                Graph.ChangeToNewAnimRef(anim);
        }

        private void LoadTaeFileContainer(TaeFileContainer fileContainer)
        {
            TaeExtensionMethods.ClearMemes();
            FileContainer = fileContainer;
            SelectedTae = FileContainer.AllTAE.First();
            GameWindowAsForm.Invoke(new Action(() =>
            {
                ButtonEditCurrentTaeHeader.Enabled = false;
                // Since form close event is hooked this should
                // take care of nulling it out for us.
                FindValueDialog?.Close();
                ComboMenu?.Close();
                ComboMenu = null;
            }));
            SelectedTaeAnim = SelectedTae.Animations[0];
            AnimationListScreen = new TaeEditAnimList(this);

            
            Graph = null;
            LoadAnimIntoGraph(SelectedTaeAnim);
            IsCurrentlyLoadingGraph = false;

            //if (FileContainer.ContainerType != TaeFileContainer.TaeFileContainerType.TAE)
            //{
            //    TaeInterop.OnLoadANIBND(MenuBar, progress);
            //}
            CheckAutoLoadXMLTemplate();
            SelectNewAnimRef(SelectedTae, SelectedTae.Animations[0]);
            GameWindowAsForm.Invoke(new Action(() =>
            {
                ButtonEditCurrentAnimInfo.Enabled = true;
                ButtonEditCurrentAnimInfo.Visible = true;
                ButtonGotoEventSource.Enabled = false;
                ButtonGotoEventSource.Visible = false;
                //MenuBar["Edit\\Find First Event of Type..."].Enabled = true;
                MenuBar["Edit\\Find Value..."].Enabled = true;
                MenuBar["Edit\\Go To Animation ID..."].Enabled = true;
                MenuBar["Edit\\Go To Animation Section ID..."].Enabled = true;
                MenuBar["Edit\\Collapse All TAE Sections"].Enabled = true;
                MenuBar["Edit\\Expand All TAE Sections"].Enabled = true;
                MenuBar["Edit\\Set Animation Name..."].Enabled = true;
                MenuBar["Animation\\Set Playback Speed..."].Enabled = true;
                MenuBar["Tools\\Combo Viewer"].Enabled = true;
                LastFindInfo = null;
            }));
        }

        public void RecreateAnimList()
        {
            Vector2 oldScroll = AnimationListScreen.ScrollViewer.Scroll;
            var sectionsCollapsed = AnimationListScreen
                .AnimTaeSections
                .ToDictionary(x => x.Key, x => x.Value.Collapsed);

            AnimationListScreen = new TaeEditAnimList(this);

            foreach (var section in AnimationListScreen.AnimTaeSections)
            {
                if (sectionsCollapsed.ContainsKey(section.Key))
                    section.Value.Collapsed = sectionsCollapsed[section.Key];
            }
            
            AnimationListScreen.ScrollViewer.Scroll = oldScroll;

            HasntSelectedAnAnimYetAfterBuildingAnimList = true;
        }

        public void AddNewAnimation()
        {
            TAE.Animation.AnimMiniHeader header = null;



            var newAnimRef = new TAE.Animation(
                SelectedTaeAnim.ID, new TAE.Animation.AnimMiniHeader.Standard(), 
                SelectedTaeAnim.AnimFileName);

            if (SelectedTaeAnim.MiniHeader is TAE.Animation.AnimMiniHeader.Standard stand)
            {
                var standardHeader = new TAE.Animation.AnimMiniHeader.Standard();

                standardHeader.AllowDelayLoad = false;

                if (stand.ImportHKXSourceAnimID >= 0)
                {
                    if (stand.ImportsHKX)
                    {
                        standardHeader.ImportsHKX = true;
                        
                    }

                    if (stand.AllowDelayLoad)
                    {

                    }
                }


                header = standardHeader;
            }
            else if (SelectedTaeAnim.MiniHeader is TAE.Animation.AnimMiniHeader.ImportOtherAnim imp)
            {
                header = imp;
            }



                var index = SelectedTae.Animations.IndexOf(SelectedTaeAnim);
            SelectedTae.Animations.Insert(index + 1, newAnimRef);

            RecreateAnimList();

            SelectNewAnimRef(SelectedTae, newAnimRef);
        }

        public void LoadTAETemplate(string xmlFile)
        {
            try
            {
                foreach (var tae in FileContainer.AllTAE)
                {
                    tae.ApplyTemplate(TAE.Template.ReadXMLFile(xmlFile));
                }

                foreach (var box in Graph.EventBoxes)
                {
                    box.UpdateEventText();
                }

                var wasSelecting = SelectedEventBox;
                SelectedEventBox = null;
                SelectedEventBox = wasSelecting;

                if (SelectedTae.BankTemplate != null)
                {
                    GameWindowAsForm.Invoke(new Action(() =>
                    {
                        inspectorWinFormsControl.buttonChangeType.Enabled = true;
                        inspectorWinFormsControl.buttonChangeType.Visible = true;
                    }));
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Failed to apply TAE template:\n\n{ex}",
                    "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        public TaeEditorScreen(System.Windows.Forms.Form gameWindowAsForm)
        {
            LoadConfig();

            gameWindowAsForm.FormClosing += GameWindowAsForm_FormClosing;

            GameWindowAsForm = gameWindowAsForm;

            GameWindowAsForm.MinimumSize = new System.Drawing.Size(1280  - 64, 720 - 64);

            //Input = new FancyInputHandler();

            //editScreenAnimList = new TaeEditAnimList(this);
            //editScreenCurrentAnim = new TaeEditAnimEventGraph(this);
            //editScreenGraphInspector = new TaeEditAnimEventGraphInspector(this);

            inspectorWinFormsControl = new TaeInspectorWinFormsControl();

            inspectorWinFormsControl.Visible = false;

            //// This might change in the future if I actually add text description attributes to some things.
            //inspectorWinFormsControl.propertyGrid.HelpVisible = false;

            //inspectorWinFormsControl.propertyGrid.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            //inspectorWinFormsControl.propertyGrid.ToolbarVisible = false;

            ////inspectorPropertyGrid.ViewBackColor = System.Drawing.Color.FromArgb(
            ////    ColorInspectorBG.A, ColorInspectorBG.R, ColorInspectorBG.G, ColorInspectorBG.B);

            //inspectorWinFormsControl.propertyGrid.LargeButtons = true;

            //inspectorWinFormsControl.propertyGrid.CanShowVisualStyleGlyphs = false;

            inspectorWinFormsControl.buttonChangeType.Enabled = false;
            inspectorWinFormsControl.buttonChangeType.Visible = false;

            inspectorWinFormsControl.buttonChangeType.Click += ButtonChangeType_Click;

            inspectorWinFormsControl.TaeEventValueChanged += InspectorWinFormsControl_TaeEventValueChanged;

            GameWindowAsForm.Controls.Add(inspectorWinFormsControl);

            WinFormsMenuStrip = new System.Windows.Forms.MenuStrip();

            WinFormsMenuStrip.BackColor = inspectorWinFormsControl.BackColor;
            WinFormsMenuStrip.ForeColor = inspectorWinFormsControl.ForeColor;

            WinFormsMenuStrip.MouseEnter += (o, e) =>
            {
                Input.CursorType = MouseCursorType.Arrow;
            };

            MenuBar = new TaeMenuBarBuilder(WinFormsMenuStrip);

            //////////
            // File //
            //////////

            MenuBar.AddItem("File", "Open...", () => File_Open());
            MenuBar.AddSeparator("File");
            MenuBar.AddItem("File", "Load Custom TAE Data Template XML...", () =>
            {
                var selectedTemplate = BrowseForXMLTemplate();
                if (selectedTemplate != null)
                    LoadTAETemplate(selectedTemplate);
            });
            MenuBar.AddSeparator("File");
            MenuBar.AddItem("File", "Recent Files");
            CreateRecentFilesList();
            MenuBar.AddSeparator("File");
            MenuBar.AddItem("File", "Reload GameParam", () =>
            {
                LoadingTaskMan.DoLoadingTask("FileReloadGameParam", "Reloading GameParam...", prog =>
                {
                    GameDataManager.ReloadParams();
                    //var curTime = Graph.PlaybackCursor.CurrentTime;
                    //SelectNewAnimRef(SelectedTae, SelectedTaeAnim);
                    //Graph.PlaybackCursor.CurrentTime = curTime;
                    Graph.ViewportInteractor.OnScrubFrameChange();
                }, disableProgressBarByDefault: true);
            }, startDisabled: true);
            //MenuBar.AddItem("File", "Reload Text", () =>
            //{
            //    LoadingTaskMan.DoLoadingTask("FileReloadMSG", "Reloading Text...", prog =>
            //    {
            //        GameDataManager.ReloadFmgs();
            //        SelectNewAnimRef(SelectedTae, SelectedTaeAnim);
            //    }, disableProgressBarByDefault: true);
            //}, startDisabled: true);
            MenuBar.AddSeparator("File");
            MenuBar.AddItem("File", "Save", () => SaveCurrentFile(), startDisabled: true);
            MenuBar.AddItem("File", "Save As...", () => File_SaveAs(), startDisabled: true);
            MenuBar.AddSeparator("File");
            MenuBar.AddItem("File", "Force Ingame Character Reload Now (DS3/DS1R Only)|F5", () => LiveRefresh(), startDisabled: true);
            MenuBar.AddItem("File", "Force Ingame Character Reload When Saving (DS3/DS1R Only)", () => Config.LiveRefreshOnSave, b => Config.LiveRefreshOnSave = b);
            MenuBar.AddSeparator("File");
            MenuBar.AddItem("File", "Manually Save Config", () =>
            {
                SaveConfig();
                System.Windows.Forms.MessageBox.Show("Configuration saved.", "Save Complete");
            });
            MenuBar.AddSeparator("File");
            MenuBar.AddItem("File", "Exit", () => GameWindowAsForm.Close());

            /////////////////////
            // Entity Settings //
            /////////////////////

            void BrowseForMoreTextures()
            {
                List<string> texturesToLoad = new List<string>();
                foreach (var m in Scene.Models)
                {
                    lock (Scene._lock_ModelLoad_Draw)
                        texturesToLoad.AddRange(m.MainMesh.GetAllTexNamesToLoad());
                }

                var browseDlg = new System.Windows.Forms.OpenFileDialog()
                {
                    //Filter = "TPF[.DCX]/TFPBHD|*.TPF*",
                    Multiselect = true,
                    Title = "Load textures from CHRBND(s), TEXBND(s), TPF(s), and/or TPFBHD(s)..."
                };
                if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var texFileNames = browseDlg.FileNames;
                    List<string> bxfDupeCheck = new List<string>();
                    LoadingTaskMan.DoLoadingTask("BrowseForEntityTextures", "Scanning files for relevant textures...", progress =>
                    {
                        double i = 0;
                        foreach (var tfn in texFileNames)
                        {
                            var shortName = Utils.GetShortIngameFileName(tfn);
                            if (!bxfDupeCheck.Contains(shortName))
                            {
                                if (TPF.Is(tfn))
                                {
                                    TexturePool.AddTpfFromPath(tfn);
                                }
                                else
                                {
                                    TexturePool.AddSpecificTexturesFromBinder(tfn, texturesToLoad);
                                }
                                
                                bxfDupeCheck.Add(shortName);
                            }
                            progress.Report(++i / texFileNames.Length);
                        }
                        Scene.RequestTextureLoad();
                        progress.Report(1);
                    });
                    
                    
                }


            }

            MenuBar.AddTopItem("NPC Settings");
            MenuBar["NPC Settings"].Enabled = false;
            MenuBar["NPC Settings"].Visible = false;
            MenuBar["NPC Settings"].Font = 
                new System.Drawing.Font(MenuBar["NPC Settings"].Font, 
                System.Drawing.FontStyle.Bold);
            MenuBar["NPC Settings"].ForeColor = System.Drawing.Color.Cyan;
            MenuBar.AddItem("NPC Settings", "Load Additional Texture File(s)...", () => BrowseForMoreTextures());


            MenuBar.AddTopItem("Player Settings");
            MenuBar["Player Settings"].Enabled = false;
            MenuBar["Player Settings"].Visible = false;
            MenuBar["Player Settings"].Font = 
                new System.Drawing.Font(MenuBar["Player Settings"].Font, 
                System.Drawing.FontStyle.Bold);
            MenuBar["Player Settings"].ForeColor = System.Drawing.Color.Cyan;

            MenuBar.AddItem("Player Settings", "Show Equip Change Menu", () => Graph?.ViewportInteractor?.BringUpEquipForm());

            MenuBar.AddSeparator("Player Settings");

            MenuBar.AddItem($"Player Settings", "Behavior / Hitbox Source", new Dictionary<string, Action>
            {
                { "Body", () =>
                    {
                        Graph.ViewportInteractor.EventSim.OnNewAnimSelected(Graph.EventBoxes);
                        Config.HitViewDummyPolySource = ParamData.AtkParam.DummyPolySource.Body;
                        Graph.ViewportInteractor.EventSim.OnNewAnimSelected(Graph.EventBoxes);
                        Graph.ViewportInteractor.OnScrubFrameChange();
                        
                    } 
                },
                { "Right Weapon", () =>
                    {
                        Graph.ViewportInteractor.EventSim.OnNewAnimSelected(Graph.EventBoxes);
                        Config.HitViewDummyPolySource = ParamData.AtkParam.DummyPolySource.RightWeapon0;
                        Graph.ViewportInteractor.EventSim.OnNewAnimSelected(Graph.EventBoxes);
                        Graph.ViewportInteractor.OnScrubFrameChange();
                    }
                },
                { "Left Weapon", () =>
                    {
                        Graph.ViewportInteractor.EventSim.OnNewAnimSelected(Graph.EventBoxes);
                        Config.HitViewDummyPolySource = ParamData.AtkParam.DummyPolySource.LeftWeapon0;
                        Graph.ViewportInteractor.EventSim.OnNewAnimSelected(Graph.EventBoxes);
                        Graph.ViewportInteractor.OnScrubFrameChange();
                        
                    }
                },
            }, () =>
            {
                if (Config.HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.Body)
                    return "Body";
                else if (Config.HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.RightWeapon0)
                    return "Right Weapon";
                else if (Config.HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.LeftWeapon0)
                    return "Left Weapon";
                else
                    return "None";
            });

            MenuBar.AddSeparator("Player Settings");

            //MenuBar.AddItem("Player Settings\\Select Right Weapon Anim", "Weapon has no animations.");
            //MenuBar.AddItem("Player Settings\\Select Left Weapon Anim", "Weapon has no animations.");

            MenuBar.AddTopItem("Object Settings");
            MenuBar["Object Settings"].Enabled = false;
            MenuBar["Object Settings"].Visible = false;
            MenuBar["Object Settings"].Font = 
                new System.Drawing.Font(MenuBar["Object Settings"].Font, 
                System.Drawing.FontStyle.Bold);
            MenuBar["Object Settings"].ForeColor = System.Drawing.Color.Cyan;
            MenuBar.AddItem("Object Settings", "Load Additional Texture File(s)...", () => BrowseForMoreTextures());

            MenuBar.AddTopItem("Animated Equipment Settings");
            MenuBar["Animated Equipment Settings"].Enabled = false;
            MenuBar["Animated Equipment Settings"].Visible = false;
            MenuBar["Animated Equipment Settings"].Font =
                new System.Drawing.Font(MenuBar["Animated Equipment Settings"].Font,
                System.Drawing.FontStyle.Bold);
            MenuBar["Animated Equipment Settings"].ForeColor = System.Drawing.Color.Cyan;

            MenuBar.AddTopItem("Cutscene Settings");
            MenuBar["Cutscene Settings"].Enabled = false;
            MenuBar["Cutscene Settings"].Visible = false;
            MenuBar["Cutscene Settings"].Font =
                new System.Drawing.Font(MenuBar["Cutscene Settings"].Font,
                System.Drawing.FontStyle.Bold);
            MenuBar["Cutscene Settings"].ForeColor = System.Drawing.Color.Cyan;
            MenuBar.AddItem("Cutscene Settings", "Load Additional Texture File(s)...", () => BrowseForMoreTextures());

            //////////
            // Edit //
            //////////

            MenuBar.AddItem("Edit", "Undo|Ctrl+Z", () => UndoMan.Undo(), startDisabled: true);
            MenuBar.AddItem("Edit", "Redo|Ctrl+Y", () => UndoMan.Redo(), startDisabled: true);
            MenuBar.AddSeparator("Edit");
            MenuBar.AddItem("Edit", "Collapse All TAE Sections", () =>
            {
                foreach (var kvp in AnimationListScreen.AnimTaeSections.Values)
                {
                    kvp.Collapsed = true;
                }
            }, startDisabled: true);
            MenuBar.AddItem("Edit", "Expand All TAE Sections", () =>
            {
                foreach (var kvp in AnimationListScreen.AnimTaeSections.Values)
                {
                    kvp.Collapsed = false;
                }
            }, startDisabled: true);
            MenuBar.AddSeparator("Edit");
            //MenuBar.AddItem("Edit", "Find First Event of Type...|Ctrl+F", () => ShowDialogFind(), startDisabled: true);
            MenuBar.AddItem("Edit", "Find Value...|Ctrl+F", () => ShowDialogFind(), startDisabled: true);
            MenuBar.AddItem("Edit", "Go To Animation ID...|Ctrl+G", () => ShowDialogGotoAnimID(), startDisabled: true);
            MenuBar.AddItem("Edit", "Go To Animation Section ID...|Ctrl+H", () => ShowDialogGotoAnimSectionID(), startDisabled: true);
            MenuBar.AddItem("Edit", "Set Animation Name...|F2", () => ShowDialogChangeAnimName(), startDisabled: true);

            /////////////////
            // Event Graph //
            /////////////////

            //MenuBar.AddItem("Event Graph", "Snap To 30 Hz Increments", () => Config.EnableSnapTo30FPSIncrements, b => Config.EnableSnapTo30FPSIncrements = b);

            MenuBar.AddItem("Event Graph", "Snap Events To Framerate", new Dictionary<string, Action>
            {
                { "None" , () => Config.EventSnapType = TaeConfigFile.EventSnapTypes.None },
                { "30 FPS (used by FromSoft)" , () => Config.EventSnapType = TaeConfigFile.EventSnapTypes.FPS30 },
                { "60 FPS" , () => Config.EventSnapType = TaeConfigFile.EventSnapTypes.FPS60 },
            }, () =>
            {
                if (Config.EventSnapType == TaeConfigFile.EventSnapTypes.None)
                    return "None";
                else if (Config.EventSnapType == TaeConfigFile.EventSnapTypes.FPS30)
                    return "30 FPS (used by FromSoft)";
                else
                    return "60 FPS";
            });

            //MenuBar.AddItem("Event Graph", "High Contrast Mode", () => Config.EnableColorBlindMode, b => Config.EnableColorBlindMode = b);
            MenuBar.AddSeparator("Event Graph");
            MenuBar.AddItem("Event Graph", "Use New Graph Design", () => Config.IsNewGraphVisiMode, b => Config.IsNewGraphVisiMode = b);
            MenuBar.AddItem("Event Graph", "Use Fancy Text Scrolling", () => Config.EnableFancyScrollingStrings, b => Config.EnableFancyScrollingStrings = b);
            MenuBar.AddItem("Event Graph", "Fancy Text Scroll Speed", new Dictionary<string, Action>
                {
                    { "Extremely Slow (4 px/s)", () => Config.FancyScrollingStringsScrollSpeed = 4 },
                    { "Very Slow (8 px/s)", () => Config.FancyScrollingStringsScrollSpeed = 8 },
                    { "Slow (16 px/s)", () => Config.FancyScrollingStringsScrollSpeed = 16 },
                    { "Medium Speed (32 px/s)", () => Config.FancyScrollingStringsScrollSpeed = 32 },
                    { "Fast (64 px/s)",  () => Config.FancyScrollingStringsScrollSpeed = 64 },
                    { "Very Fast (128 px/s)",  () => Config.FancyScrollingStringsScrollSpeed = 128 },
                    {  "Extremely Fast (256 px/s)", () => Config.FancyScrollingStringsScrollSpeed = 256 },
                },
                defaultChoice: "Fast (64 px/s)");
            MenuBar.AddSeparator("Event Graph");
            MenuBar.AddItem("Event Graph", "Start with all TAE sections collapsed", 
                () => Config.AutoCollapseAllTaeSections, b => Config.AutoCollapseAllTaeSections = b);
            MenuBar.AddSeparator("Event Graph");
            MenuBar.AddItem("Event Graph", "Auto-scroll During Anim Playback", 
                () => Config.AutoScrollDuringAnimPlayback, b => Config.AutoScrollDuringAnimPlayback = b);
            MenuBar.AddSeparator("Event Graph");
            MenuBar.AddItem("Event Graph", "Solo Highlight Event on Hover",
                () => Config.SoloHighlightEventOnHover, b => Config.SoloHighlightEventOnHover = b);
            MenuBar.AddItem("Event Graph", "Show Event Info Popup When Hovering Over Event",
                () => Config.ShowEventHoverInfo, b => Config.ShowEventHoverInfo = b);

            //MenuBar.AddSeparator("Event Graph");

            //MenuBar.AddItem("Event Graph", "Show SFX Spawn Events With Cyan Markers", () => TaeInterop.ShowSFXSpawnWithCyanMarkers, b => TaeInterop.ShowSFXSpawnWithCyanMarkers = b);
            //MenuBar.AddItem("Event Graph", "Beep Upon Hitting Sound Events", () => TaeInterop.PlaySoundEffectOnSoundEvents, b => TaeInterop.PlaySoundEffectOnSoundEvents = b);
            //MenuBar.AddItem("Event Graph", "Beep Upon Hitting Highlighted Event(s)", () => TaeInterop.PlaySoundEffectOnHighlightedEvents, b => TaeInterop.PlaySoundEffectOnHighlightedEvents = b);
            //MenuBar.AddItem("Event Graph", "Sustain Sound Effect Loop For Duration Of Highlighted Event(s)", () => TaeInterop.PlaySoundEffectOnHighlightedEvents_Loop, b => TaeInterop.PlaySoundEffectOnHighlightedEvents_Loop = b);

            ////////////////
            // Simulation //
            ////////////////

            MenuBar.AddTopItem("Simulation");
            MenuBar["Simulation"].Enabled = false;
            MenuBar["Simulation"].Visible = false;

            ///////////
            // Scene //
            ///////////

            MenuBar.AddItem("Scene", "Render Meshes", () => !GFX.HideFLVERs,
                    b => GFX.HideFLVERs = !b);

            // Okay, look, I know this entire next part is hyper ultra stupid, but,
            // I spent like multiple hours trying to get other less stupid ways
            // to work and the UI just never updated correctly.
            MenuBar.AddItem("Scene", "Models", menu =>
            {
                foreach (var m in Scene.Models)
                {
                    var maskDict = m.GetMaterialNamesPerMask();

                    foreach (var kvp in maskDict.OrderBy(k => k.Key))
                    {
                        if (kvp.Key >= 0 && kvp.Key < m.DrawMask.Length)
                        {
                            menu.AddItem($"Scene\\Models\\{m.Name}", $"Mask {(kvp.Key)}",
                            () => m.DefaultDrawMask[kvp.Key], 
                            b =>
                            {
                                m.DrawMask[kvp.Key] = m.DefaultDrawMask[kvp.Key] = b;

                                for (int j = 0; j < m.MainMesh.Submeshes.Count; j++)
                                {
                                    if (m.MainMesh.Submeshes[j].ModelMaskIndex >= 0)
                                    {
                                        var mi2 = MenuBar[$"Scene\\Models\\{m.Name}\\Mesh {(j + 1)}: {m.MainMesh.Submeshes[j].FullMaterialName}"];
                                        if (mi2 is System.Windows.Forms.ToolStripMenuItem tsmi2)
                                        {
                                            // Note: Tag is used as like a flag for the render code I'm sorry.
                                            //       See CoolDarkToolStripRenderer.OnRenderItemText
                                            //       
                                            //       Makes text grayed out like it's disabled, but it's not.
                                            mi2.Tag = !m.DefaultDrawMask[m.MainMesh.Submeshes[j].ModelMaskIndex];
                                            mi2.Invalidate();
                                        }
                                    }
                                }

                                //var miName = $"Scene\\Models\\{m.Name}\\Mask {(kvp.Key)}";

                                //var mi = MenuBar[miName];
                                //if (mi is System.Windows.Forms.ToolStripMenuItem tsmi)
                                //{
                                //    WinFormsMenuStrip.Focus();
                                //    tsmi.Owner?.Focus();
                                //}
                            });
                        }
                    }

                    menu.AddSeparator($"Scene\\Models\\{m.Name}");

                    menu.AddItem($"Scene\\Models\\{m.Name}", $"Hide All Meshes", () =>
                    {
                        for (int j = 0; j < m.MainMesh.Submeshes.Count; j++)
                        {
                            m.MainMesh.Submeshes[j].IsVisible = false;

                            var miName = $"Scene\\Models\\{m.Name}\\Mesh {(j + 1)}: {m.MainMesh.Submeshes[j].FullMaterialName}";

                            var mi = MenuBar[miName];
                            if (mi is System.Windows.Forms.ToolStripMenuItem tsmi)
                            {
                                tsmi.Checked = false;

                                // On last one, proc weird update
                                //if (j >= m.MainMesh.Submeshes.Count - 1)
                                //{
                                //    WinFormsMenuStrip.Focus();
                                //    tsmi.Owner.Focus();
                                //}
                            }
                        }
                    }, closeOnClick: false);
                    menu.AddItem($"Scene\\Models\\{m.Name}", $"Show All Meshes", () =>
                    {
                        for (int j = 0; j < m.MainMesh.Submeshes.Count; j++)
                        {
                            m.MainMesh.Submeshes[j].IsVisible = true;

                            var mi = MenuBar[$"Scene\\Models\\{m.Name}\\Mesh {(j + 1)}: {m.MainMesh.Submeshes[j].FullMaterialName}"];
                            if (mi is System.Windows.Forms.ToolStripMenuItem tsmi)
                            {
                                tsmi.Checked = true;

                                // On last one, proc weird update
                                //if (j >= m.MainMesh.Submeshes.Count - 1)
                                //{
                                //    WinFormsMenuStrip.Focus();
                                //    tsmi.Owner.Focus();
                                //}
                            }
                        }
                    }, closeOnClick: false);
                    menu.AddSeparator($"Scene\\Models\\{m.Name}");

                    int i = 1;
                    foreach (var sm in m.MainMesh.Submeshes)
                    {
                        menu.AddItem($"Scene\\Models\\{m.Name}", $"Mesh {(i++)}: {sm.FullMaterialName}" +
                            (sm.ModelMaskIndex >= 0 && maskDict.ContainsKey(sm.ModelMaskIndex) 
                            ? $"|[Mask {sm.ModelMaskIndex}]" : ""), 
                            checkState: () => sm.IsVisible, b => sm.IsVisible = b, 
                            getEnabled: () => true,
                            getMemeTag: () => !(sm.ModelMaskIndex < 0 || m.MainMesh.DrawMask[sm.ModelMaskIndex]));
                    }
                }
            });

            MenuBar.AddSeparator("Scene");

            //MenuBar.AddItem("Scene", $"Render HKX Skeleton ({DBG.COLOR_HKX_BONE_NAME})",
            //    () => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.HkxBone],
            //        b => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.HkxBone] = b);

            MenuBar.AddItem("Scene", $"Helper: FLVER Skeleton", 
                () => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBone],
                b => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBone] = b);

            MenuBar.AddItem("Scene", $"Helper: FLVER Skeleton Boxes", 
                () => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBoneBoundingBox],
                b => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBoneBoundingBox] = b);

            MenuBar.AddItem("Scene", $"Helper: DummyPoly", 
                () => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.DummyPoly],
                    b => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.DummyPoly] = b);

            MenuBar.AddItem("Scene", $"Helper: DummyPoly IDs",
                () => DBG.CategoryEnableNameDraw[DebugPrimitives.DbgPrimCategory.DummyPoly],
                   b => DBG.CategoryEnableNameDraw[DebugPrimitives.DbgPrimCategory.DummyPoly] = b);

            MenuBar.AddItem("Scene", "Show c0000 Weapon Global DummyPoly ID Values (10000+)", () => NewDummyPolyManager.ShowGlobalIDOffset,
                    b => NewDummyPolyManager.ShowGlobalIDOffset = b);

            MenuBar.AddItem("Scene", $"Helper: Sound Event Locations",
                () => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.SoundEvent],
                   b => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.SoundEvent] = b);

            MenuBar.AddSeparator("Scene");

            MenuBar.AddItem("Scene", "Helper X-Ray Mode", () => Config.DbgPrimXRay,
                    b => Config.DbgPrimXRay = b);

            ///////////////
            // Animation //
            ///////////////

            MenuBar.AddItem("Animation", "Lock to Frame Rate Defined in HKX",
                () => Config.LockFramerateToOriginalAnimFramerate,
                b => Config.LockFramerateToOriginalAnimFramerate = b);

            MenuBar.AddItem("Animation", "Set Playback Speed...", () =>
            {
                PauseUpdate = true;
                var speed = KeyboardInput.Show("Set Playback Speed", "Set animation playback speed.", PlaybackCursor.BasePlaybackSpeed.ToString("0.00"));
                speed.Wait();
                if (speed.IsCompleted && !speed.IsCanceled)
                {
                    if (speed.Result != null)
                    {
                        if (float.TryParse(speed.Result, out float newSpeed))
                        {
                            PlaybackCursor.BasePlaybackSpeed = newSpeed;
                            MenuBar["Animation\\Set Playback Speed..."].ShortcutKeyDisplayString = $"({PlaybackCursor.BasePlaybackSpeed:0.00})";
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("Not a valid number.");
                        }
                    }
                }

                PauseUpdate = false;
            }, startDisabled: true);

            MenuBar.AddSeparator("Animation");

            MenuBar.AddItem("Animation", "Enable Root Motion",
                () => Config.EnableAnimRootMotion,
                b => Config.EnableAnimRootMotion = b);

            MenuBar.AddItem("Animation", "Camera Follows Root Motion Translation",
                () => Config.CameraFollowsRootMotion,
                b => Config.CameraFollowsRootMotion = b);

            MenuBar.AddItem("Animation", "Camera Follows Root Motion Rotation",
                () => Config.CameraFollowsRootMotionRotation,
                b => Config.CameraFollowsRootMotionRotation = b);

            MenuBar.AddItem("Animation", "Prevent Root Motion From Reaching End Of Grid",
                () => Config.WrapRootMotion,
                b => Config.WrapRootMotion = b);

            //MenuBar.AddItem("Animation", "Accumulate Root Motion",
            //    () => Config.AccumulateRootMotion,
            //    b => Config.AccumulateRootMotion = b);

            MenuBar["Animation"].DropDownOpening += (o, e) =>
            {
                if (PlaybackCursor != null)
                    MenuBar["Animation\\Set Playback Speed..."].ShortcutKeyDisplayString = $"({PlaybackCursor.BasePlaybackSpeed:0.00})";
            };


            //////////////
            // Viewport //
            //////////////

            //MenuBar.AddItem("3D Viewport", "Vsync", () => GFX.Display.Vsync, b =>
            //{
            //    GFX.Display.Vsync = b;
            //    GFX.Display.Width = GFX.Device.Viewport.Width;
            //    GFX.Display.Height = GFX.Device.Viewport.Height;
            //    GFX.Display.Fullscreen = false;
            //    GFX.Display.Apply();
            //});

            //MenuBar.AddItem("3D Viewport", "Slow Light Spin (overrides below option)", () => GFX.FlverAutoRotateLight, b => GFX.FlverAutoRotateLight = b);
            //MenuBar.AddItem("3D Viewport", "Light Follows Camera", () => GFX.FlverLightFollowsCamera, b => GFX.FlverLightFollowsCamera = b);

            //Dictionary<string, Action> shadingModeChoicesDict = new Dictionary<string, Action>();

            //void AddShadingModeChoice(FlverShadingMode mode)
            //{
            //    shadingModeChoicesDict.Add(GFX.FlverShadingModeNames[mode], () => GFX.ForcedFlverShadingMode = mode);
            //}

            //shadingModeChoicesDict.Add("Do Not Override", () => GFX.ForcedFlverShadingMode = null);

            //shadingModeChoicesDict.Add("SEPARATOR:0", null);

            //AddShadingModeChoice(FlverShadingMode.PBR_GLOSS_DS3);
            //AddShadingModeChoice(FlverShadingMode.PBR_GLOSS_BB);
            //AddShadingModeChoice(FlverShadingMode.CLASSIC_DIFFUSE_PTDE);
            //AddShadingModeChoice(FlverShadingMode.LEGACY);

            //shadingModeChoicesDict.Add("SEPARATOR:1", null);

            //AddShadingModeChoice(FlverShadingMode.TEXDEBUG_DIFFUSEMAP);
            //AddShadingModeChoice(FlverShadingMode.TEXDEBUG_SPECULARMAP);
            //AddShadingModeChoice(FlverShadingMode.TEXDEBUG_NORMALMAP);
            //AddShadingModeChoice(FlverShadingMode.TEXDEBUG_EMISSIVEMAP);
            //AddShadingModeChoice(FlverShadingMode.TEXDEBUG_BLENDMASKMAP);
            //AddShadingModeChoice(FlverShadingMode.TEXDEBUG_SHININESSMAP);
            //AddShadingModeChoice(FlverShadingMode.TEXDEBUG_NORMALMAP_BLUE);

            //shadingModeChoicesDict.Add("SEPARATOR:2", null);

            //AddShadingModeChoice(FlverShadingMode.MESHDEBUG_NORMALS);
            //AddShadingModeChoice(FlverShadingMode.MESHDEBUG_NORMALS_MESH_ONLY);
            //AddShadingModeChoice(FlverShadingMode.MESHDEBUG_VERTEX_COLOR_ALPHA);

            //MenuBar.AddSeparator("3D Viewport");

            //MenuBar.AddItem("3D Viewport", "Disable Texture Alphas", () => GFX.FlverShader.Effect.DisableAlpha, b => GFX.FlverShader.Effect.DisableAlpha = b);
            //MenuBar.AddItem("3D Viewport", "Disable Texture Blending", () => GFX.FlverDisableTextureBlending, b => GFX.FlverDisableTextureBlending = b);

            //MenuBar.AddItem("3D Viewport", "Override FLVER Shading Mode", shadingModeChoicesDict, () =>
            //{
            //    if (GFX.ForcedFlverShadingMode == null)
            //    {
            //        return "Do Not Override";
            //    }
            //    else
            //    {
            //        return GFX.FlverShadingModeNames[GFX.ForcedFlverShadingMode.Value];
            //    }

            //});

            //MenuBar.AddItem("3D Viewport", $@"Reload FLVER Shader (./Content/Shaders/FlverShader.xnb)", () =>
            //{
            //    if (DebugReloadContentManager != null)
            //    {
            //        DebugReloadContentManager.Unload();
            //        DebugReloadContentManager.Dispose();
            //    }

            //    using (DebugReloadContentManager = new ContentManager(Main.ContentServiceProvider))
            //    {
            //        GFX.FlverShader.Effect.Dispose();
            //        GFX.FlverShader = null;
            //        GFX.FlverShader = new FlverShader(DebugReloadContentManager.Load<Effect>(GFX.FlverShader__Name));
            //    }

            //    GFX.InitShaders();
            //});

            //MenuBar.AddSeparator("3D Viewport");

            //if (Environment.Cubemaps.ContainsKey(Config.LastCubemapUsed))
            //{
            //    Environment.CubemapNameIndex = 
            //        Environment.Cubemaps.Keys.ToList().IndexOf(Config.LastCubemapUsed);
            //}

            //MenuBar.AddItem("3D Viewport", "Cubemap", () =>
            //{
            //    var result = new Dictionary<string, Action>();
            //    foreach (var kvp in Environment.Cubemaps)
            //    {
            //        result.Add(kvp.Key, () =>
            //        {
            //            Environment.CubemapNameIndex = Environment.Cubemaps.Keys.ToList().IndexOf(kvp.Key);
            //            Config.LastCubemapUsed = kvp.Key;
            //        });
            //    }
            //    return result;
            //},
            //() => Environment.CurrentCubemapName);

            //MenuBar.AddItem("3D Viewport", "Resolution Multiplier", new Dictionary<string, Action>
            //{
            //    { "1x", () => GFX.SSAA = 1 },
            //    { "2x", () => GFX.SSAA = 2 },
            //    { "3x", () => GFX.SSAA = 3 },
            //    { "4x", () => GFX.SSAA = 4 },
            //    //{ "5x", () => GFX.SSAA = 5 },
            //    //{ "6x", () => GFX.SSAA = 6 },
            //    //{ "7x", () => GFX.SSAA = 7 },
            //    //{ "8x", () => GFX.SSAA = 8 },
            //    //{ "16x", () => GFX.SSAA = 16 },
            //    //{ "32x", () => GFX.SSAA = 32 },
            //}, () => $"{GFX.SSAA}x");

            MenuBar.AddTopItem("Sound");

            MenuBar.AddItem("Help", "Basic Controls", () => System.Windows.Forms.MessageBox.Show(HELP_TEXT, "DS Anim Studio Help - Basic Controls",
                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information));

            BuildDebugMenuBar();

            MenuBar.AddTopItem("Support Meowmaritus");

            MenuBar["Support Meowmaritus"].Tag = System.Drawing.Color.Lime;

            MenuBar.AddItem("Support Meowmaritus", "Patreon", () =>
            {
                Process.Start("https://www.patreon.com/Meowmaritus");
            });

            MenuBar.AddItem("Support Meowmaritus", "PayPal", () =>
            {
                Process.Start("https://paypal.me/Meowmaritus");
            });

            MenuBar["Support Meowmaritus\\Patreon"].Tag = System.Drawing.Color.Lime;
            MenuBar["Support Meowmaritus\\PayPal"].Tag = System.Drawing.Color.Lime;

            WinFormsMenuStrip.MenuActivate += WinFormsMenuStrip_MenuActivate;
            WinFormsMenuStrip.MenuDeactivate += WinFormsMenuStrip_MenuDeactivate;

            GameWindowAsForm.Controls.Add(WinFormsMenuStrip);

            ButtonEditCurrentAnimInfo = new NoPaddingButton();
            ButtonEditCurrentAnimInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            ButtonEditCurrentAnimInfo.TabStop = false;
            ButtonEditCurrentAnimInfo.OwnerDrawText = "Edit Anim Info (F3)";
            ButtonEditCurrentAnimInfo.Click += ButtonEditCurrentAnimInfo_Click;
            ButtonEditCurrentAnimInfo.BackColor = inspectorWinFormsControl.BackColor;
            ButtonEditCurrentAnimInfo.ForeColor = inspectorWinFormsControl.ForeColor;
            ButtonEditCurrentAnimInfo.Enabled = false;
            ButtonEditCurrentAnimInfo.Visible = false;
            ButtonEditCurrentAnimInfo.Padding = new System.Windows.Forms.Padding(0);
            ButtonEditCurrentAnimInfo.Margin = new System.Windows.Forms.Padding(0);

            GameWindowAsForm.Controls.Add(ButtonEditCurrentAnimInfo);

            ButtonGotoEventSource = new NoPaddingButton();
            ButtonGotoEventSource.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            ButtonGotoEventSource.TabStop = false;
            ButtonGotoEventSource.OwnerDrawText = "Goto Event Source (F4)";
            ButtonGotoEventSource.Click += ButtonGotoEventSource_Click;
            ButtonGotoEventSource.BackColor = inspectorWinFormsControl.BackColor;
            ButtonGotoEventSource.ForeColor = inspectorWinFormsControl.ForeColor;
            ButtonGotoEventSource.Enabled = false;
            ButtonGotoEventSource.Visible = false;
            ButtonGotoEventSource.Padding = new System.Windows.Forms.Padding(0);
            ButtonGotoEventSource.Margin = new System.Windows.Forms.Padding(0);

            GameWindowAsForm.Controls.Add(ButtonGotoEventSource);

            ButtonEditCurrentTaeHeader = new NoPaddingButton();
            ButtonEditCurrentTaeHeader.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            ButtonEditCurrentTaeHeader.TabStop = false;
            ButtonEditCurrentTaeHeader.OwnerDrawText = "Edit TAE Header...";
            ButtonEditCurrentTaeHeader.Click += ButtonEditCurrentTaeHeader_Click;
            ButtonEditCurrentTaeHeader.BackColor = inspectorWinFormsControl.BackColor;
            ButtonEditCurrentTaeHeader.ForeColor = inspectorWinFormsControl.ForeColor;
            ButtonEditCurrentTaeHeader.Enabled = false;
            ButtonEditCurrentTaeHeader.Visible = false;
            ButtonEditCurrentTaeHeader.Padding = new System.Windows.Forms.Padding(0);
            ButtonEditCurrentTaeHeader.Margin = new System.Windows.Forms.Padding(0);

            GameWindowAsForm.Controls.Add(ButtonEditCurrentTaeHeader);

            //ShaderAdjuster.BackColor = inspectorWinFormsControl.BackColor;
            //ShaderAdjuster.ForeColor = inspectorWinFormsControl.ForeColor;

            //GameWindowAsForm.BackColor = System.Drawing.Color.Fuchsia;
            GameWindowAsForm.AllowTransparency = false;
            //GameWindowAsForm.TransparencyKey = System.Drawing.Color.Fuchsia;

            Transport = new TaeTransport(this);

            UpdateLayout();
        }

        public void LoadContent(ContentManager c)
        {
            Transport.LoadContent(c);
        }

        private void ButtonGotoEventSource_Click(object sender, EventArgs e)
        {
            GoToEventSource();
        }

        private void InspectorWinFormsControl_TaeEventValueChanged(object sender, EventArgs e)
        {
            //var gridReference = (System.Windows.Forms.DataGridView)sender;
            var boxReference = SelectedEventBox;
            //var newValReference = e.ChangedItem.Value;
            //var oldValReference = e.OldValue;

            //UndoMan.NewAction(doAction: () =>
            //{
            //    e.ChangedItem.PropertyDescriptor.SetValue(boxReference.MyEvent, newValReference);

            //    SelectedTaeAnim.SetIsModified(!IsReadOnlyFileMode);
            //    IsModified = true;

            //    gridReference.Refresh();
            //},
            //undoAction: () =>
            //{
            //    e.ChangedItem.PropertyDescriptor.SetValue(boxReference.MyEvent, oldValReference);

            //    SelectedTaeAnim.SetIsModified(!IsReadOnlyFileMode);
            //    IsModified = true;

            //    gridReference.Refresh();
            //});

            SelectedTaeAnim.SetIsModified(!IsReadOnlyFileMode);

            //gridReference.Refresh();
        }

        private void LiveRefresh()
        {
            var chrNameBase = Utils.GetFileNameWithoutAnyExtensions(Utils.GetFileNameWithoutDirectoryOrExtension(FileContainerName)).ToLower();

            if (chrNameBase.StartsWith("c"))
            {
                DSAnimStudio.LiveRefresh.RequestFileReload.RequestReload(
                    DSAnimStudio.LiveRefresh.RequestFileReload.ReloadType.Chr, chrNameBase);
            }
            else if (chrNameBase.StartsWith("o"))
            {
                DSAnimStudio.LiveRefresh.RequestFileReload.RequestReload(
                    DSAnimStudio.LiveRefresh.RequestFileReload.ReloadType.Object, chrNameBase);
            }

            
            //if (FileContainer.ReloadType == TaeFileContainer.TaeFileContainerReloadType.CHR_PTDE || FileContainer.ReloadType == TaeFileContainer.TaeFileContainerReloadType.CHR_DS1R)
            //{
            //    var chrName = Utils.GetFileNameWithoutDirectoryOrExtension(FileContainerName);

            //    //In case of .anibnd.dcx
            //    chrName = Utils.GetFileNameWithoutDirectoryOrExtension(chrName);

            //    if (chrName.ToLower().StartsWith("c") && chrName.Length == 5)
            //    {
            //        if (FileContainer.ReloadType == TaeFileContainer.TaeFileContainerReloadType.CHR_PTDE)
            //        {
            //            TaeLiveRefresh.ForceReloadCHR_PTDE(chrName);
            //        }
            //        else
            //        {
            //            TaeLiveRefresh.ForceReloadCHR_DS1R(chrName);
            //        }
            //    }
            //}
        }

        private void ButtonEditCurrentTaeHeader_Click(object sender, EventArgs e)
        {
            inspectorWinFormsControl.Focus();
            ShowDialogEditTaeHeader();
        }

        public void ShowDialogEditTaeHeader()
        {
            PauseUpdate = true;
            var editForm = new TaeEditTaeHeaderForm(SelectedTae);
            editForm.Owner = GameWindowAsForm;
            editForm.ShowDialog();

            if (editForm.WereThingsChanged)
            {
                SelectedTae.SetIsModified(true);

                UpdateSelectedTaeAnimInfoText();
            }

            PauseUpdate = false;
        }

        public bool DoesAnimIDExist(int id)
        {
            foreach (var s in AnimationListScreen.AnimTaeSections.Values)
            {
                var matchedAnims = s.InfoMap.Where(x => x.Value.FullID == id);
                if (matchedAnims.Any())
                {
                    return true;
                }
            }
            return false;
        }

        public bool GotoAnimSectionID(int id, bool scrollOnCenter)
        {
            if (FileContainer.AllTAEDict.Count > 1)
            {
                if (AnimationListScreen.AnimTaeSections.ContainsKey(id))
                {
                    var firstAnimInSection = AnimationListScreen.AnimTaeSections[id].Tae.Animations.FirstOrDefault();
                    if (firstAnimInSection != null)
                    {
                        SelectNewAnimRef(AnimationListScreen.AnimTaeSections[id].Tae, firstAnimInSection, scrollOnCenter);
                        return true;
                    }
                }
            }
            else
            {
                foreach (var anim in SelectedTae.Animations)
                {
                    long sectionOfAnim = GameDataManager.GameTypeHasLongAnimIDs ? (anim.ID / 1_000000) : (anim.ID / 1_0000);
                    if (sectionOfAnim == id)
                    {
                        SelectNewAnimRef(SelectedTae, anim, scrollOnCenter);
                        return true;
                    }
                }
            }

            return false;
        }

        public bool GotoAnimID(long id, bool scrollOnCenter)
        {
            foreach (var s in AnimationListScreen.AnimTaeSections.Values)
            {
                var matchedAnims = s.InfoMap.Where(x => x.Value.FullID == id);
                if (matchedAnims.Any())
                {
                    var anim = matchedAnims.First().Value.Ref;
                    SelectNewAnimRef(s.Tae, anim, scrollOnCenter);
                    return true;
                }
            }
            return false;
        }

        public void ShowDialogEditCurrentAnimInfo()
        {
            PauseUpdate = true;
            var editForm = new TaeEditAnimPropertiesForm(SelectedTaeAnim, FileContainer.AllTAE.Count() == 1);
            editForm.Owner = GameWindowAsForm;
            editForm.ShowDialog();

            if (editForm.WasAnimDeleted)
            {
                if (SelectedTae.Animations.Count <= 1)
                {
                    System.Windows.Forms.MessageBox.Show(
                        "Cannot delete the only animation remaining in the TAE.",
                        "Can't Delete Last Animation",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Stop);
                }
                else
                {
                    var indexOfCurrentAnim = SelectedTae.Animations.IndexOf(SelectedTaeAnim);
                    SelectedTae.Animations.Remove(SelectedTaeAnim);
                    RecreateAnimList();

                    if (indexOfCurrentAnim > SelectedTae.Animations.Count - 1)
                        indexOfCurrentAnim = SelectedTae.Animations.Count - 1;

                    if (indexOfCurrentAnim >= 0)
                        SelectNewAnimRef(SelectedTae, SelectedTae.Animations[indexOfCurrentAnim]);
                    else
                        SelectNewAnimRef(SelectedTae, SelectedTae.Animations[0]);

                    SelectedTae.SetIsModified(!IsReadOnlyFileMode);
                }
            }
            else
            {
                bool needsAnimReload = false;
                if (editForm.WasAnimIDChanged)
                {
                    SelectedTaeAnim.SetIsModified(!IsReadOnlyFileMode);
                    SelectedTae.SetIsModified(!IsReadOnlyFileMode);
                    RecreateAnimList();
                    UpdateSelectedTaeAnimInfoText();
                    Graph.InitGhostEventBoxes();
                    needsAnimReload = true;
                }

                if (editForm.WereThingsChanged)
                {
                    SelectedTaeAnim.SetIsModified(!IsReadOnlyFileMode);
                    SelectedTae.SetIsModified(!IsReadOnlyFileMode);
                    UpdateSelectedTaeAnimInfoText();
                    Graph.InitGhostEventBoxes();
                    needsAnimReload = true;
                }

                if (needsAnimReload)
                    Graph.ViewportInteractor.OnNewAnimSelected();

            }

            PauseUpdate = false;
        }

        private void ButtonEditCurrentAnimInfo_Click(object sender, EventArgs e)
        {
            inspectorWinFormsControl.Focus();
            ShowDialogEditCurrentAnimInfo();
        }

        private void GameWindowAsForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            SaveConfig();

            var unsavedChanges = IsModified && !IsReadOnlyFileMode;

            if (!unsavedChanges && FileContainer != null)
            {
                if (FileContainer.IsModified)
                {
                    unsavedChanges = true;
                }
                else
                {
                    foreach (var tae in FileContainer.AllTAE)
                    {
                        foreach (var anim in tae.Animations)
                        {
                            if (anim.GetIsModified() && !IsReadOnlyFileMode)
                            {
                                unsavedChanges = true;
                                break;
                            }
                        }
                    }
                }
            }

            if (unsavedChanges)
            {
                e.Cancel = true;

                var confirmDlg = System.Windows.Forms.MessageBox.Show(
                    $"File \"{System.IO.Path.GetFileName(FileContainerName)}\" has " +
                    $"unsaved changes. Would you like to save these changes before " +
                    $"closing?", "Save Unsaved Changes?",
                    System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                    System.Windows.Forms.MessageBoxIcon.None);

                if (confirmDlg == System.Windows.Forms.DialogResult.Yes)
                {
                    SaveCurrentFile(afterSaveAction: () =>
                    {
                        Main.REQUEST_EXIT = true;
                    },
                    saveMessage: "Saving ANIBND and then exiting...");
                }
                else if (confirmDlg == System.Windows.Forms.DialogResult.No)
                {
                    e.Cancel = false;
                }
            }
            else
            {
                e.Cancel = false;
            }

            if (!e.Cancel)
            {
                FmodManager.Shutdown();
            }
        }

        private void WinFormsMenuStrip_MenuDeactivate(object sender, EventArgs e)
        {
            //PauseUpdate = false;
        }

        private void WinFormsMenuStrip_MenuActivate(object sender, EventArgs e)
        {
            PauseUpdate = true;
            Input.CursorType = MouseCursorType.Arrow;
        }

        private void DirectOpenFile(string fileName)
        {
            
            void DoActualFileOpen()
            {
                LoadingTaskMan.DoLoadingTask("DirectOpenFile", "Opening ANIBND and associated model(s)...", progress =>
                {
                    string oldFileContainerName = FileContainerName.ToString();
                    var oldFileContainer = FileContainer;

                    FileContainerName = fileName;
                    var loadFileResult = LoadCurrentFile();
                    if (loadFileResult == false)
                    {
                        FileContainerName = oldFileContainerName;
                        FileContainer = oldFileContainer;
                        return;
                    }
                    else if (loadFileResult == null)
                    {
                        System.Windows.Forms.ToolStripMenuItem matchingRecentFileItem = null;

                        foreach (var x in MenuBar["File\\Recent Files"].DropDownItems)
                        {
                            if (x is System.Windows.Forms.ToolStripMenuItem item)
                            {
                                if (item.Text == fileName)
                                {
                                    matchingRecentFileItem = item;
                                }
                            }
                        }

                        if (matchingRecentFileItem == null)
                        {
                            System.Windows.Forms.MessageBox.Show(
                                $"File '{fileName}' no longer exists.",
                                "File Does Not Exist",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Warning);
                        }
                        else
                        {
                            var ask = System.Windows.Forms.MessageBox.Show(
                                $"File '{fileName}' no longer exists. Would you like to " +
                                $"remove it from the recent files list?",
                                "File Does Not Exist",
                                System.Windows.Forms.MessageBoxButtons.YesNo,
                                System.Windows.Forms.MessageBoxIcon.Warning)
                                    == System.Windows.Forms.DialogResult.Yes;

                            if (ask)
                            {
                                GameWindowAsForm.Invoke(new Action(() =>
                                {
                                    if (MenuBar["File\\Recent Files"].DropDownItems.Contains(matchingRecentFileItem))
                                        MenuBar["File\\Recent Files"].DropDownItems.Remove(matchingRecentFileItem);
                                }));

                                if (Config.RecentFilesList.Contains(fileName))
                                    Config.RecentFilesList.Remove(fileName);
                            }
                        }

                        FileContainerName = oldFileContainerName;
                        FileContainer = oldFileContainer;
                        return;
                    }

                    if (!FileContainer.AllTAE.Any())
                    {
                        FileContainerName = oldFileContainerName;
                        FileContainer = oldFileContainer;
                        System.Windows.Forms.MessageBox.Show(
                            "Selected file had no TAE files within. " +
                            "Cancelling load operation.", "Invalid File",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Stop);
                    }
                    else if (loadFileResult == null)
                    {
                        FileContainerName = oldFileContainerName;
                        FileContainer = oldFileContainer;
                        System.Windows.Forms.MessageBox.Show(
                            "File did not exist.", "File Does Not Exist",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Stop);
                    }
                }, disableProgressBarByDefault: true);
            }

            if (FileContainer != null && !IsReadOnlyFileMode && (IsModified || FileContainer.AllTAE.Any(x => x.Animations.Any(a => a.GetIsModified()))))
            {
                var yesNoCancel = System.Windows.Forms.MessageBox.Show(
                    $"File \"{System.IO.Path.GetFileName(FileContainerName)}\" has " +
                    $"unsaved changes. Would you like to save these changes before " +
                    $"loading a new file?", "Save Unsaved Changes?",
                    System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                    System.Windows.Forms.MessageBoxIcon.None);

                if (yesNoCancel == System.Windows.Forms.DialogResult.Yes)
                {
                    SaveCurrentFile(afterSaveAction: () =>
                    {
                        try
                        {
                            DoActualFileOpen();
                        }
                        catch (Exception ex)
                        {
                            System.Windows.Forms.MessageBox.Show(ex.ToString(),
                                "Error While Loading File",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Error);
                        }
                    },
                    saveMessage: "Saving ANIBND then loading new one...");
                    return;
                }
                else if (yesNoCancel == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
                //If they chose no, continue as normal.
            }

            try
            {
                DoActualFileOpen();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString(), 
                    "Error While Loading File", 
                    System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
                
            
        }

        public void File_Open()
        {
            FmodManager.StopAllSounds();
            if (FileContainer != null && !IsReadOnlyFileMode && FileContainer.AllTAE.Any(x => x.Animations.Any(a => a.GetIsModified())))
            {
                var yesNoCancel = System.Windows.Forms.MessageBox.Show(
                    $"File \"{System.IO.Path.GetFileName(FileContainerName)}\" has " +
                    $"unsaved changes. Would you like to save these changes before " +
                    $"loading a new file?", "Save Unsaved Changes?",
                    System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                    System.Windows.Forms.MessageBoxIcon.None);

                if (yesNoCancel == System.Windows.Forms.DialogResult.Yes)
                {
                    SaveCurrentFile();
                }
                else if (yesNoCancel == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
                //If they chose no, continue as normal.
            }

            var browseDlg = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = TaeFileContainer.DefaultSaveFilter,
                ValidateNames = true,
                CheckFileExists = true,
                CheckPathExists = true,
                //ShowReadOnly = true,
            };

            if (System.IO.File.Exists(FileContainerName))
            {
                browseDlg.InitialDirectory = System.IO.Path.GetDirectoryName(FileContainerName);
                browseDlg.FileName = System.IO.Path.GetFileName(FileContainerName);
            }

            if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                IsReadOnlyFileMode = browseDlg.ReadOnlyChecked;
                FileContainerName = browseDlg.FileName;

                LoadingTaskMan.DoLoadingTask("File_Open", "Loading ANIBND and associated model(s)...", progress =>
                {
                    var loadFileResult = LoadCurrentFile();
                    if (loadFileResult == false || !FileContainer.AllTAE.Any())
                    {
                        FileContainerName = "";
                        System.Windows.Forms.MessageBox.Show(
                            "Selected file had no TAE files within. " +
                            "Cancelling load operation.", "Invalid File",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Stop);
                    }
                    else if (loadFileResult == null)
                    {
                        FileContainerName = "";
                        System.Windows.Forms.MessageBox.Show(
                            "Selected file did not exist (how did you " +
                            "get this message to appear, anyways?).", "File Does Not Exist",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Stop);
                    }
                }, disableProgressBarByDefault: true);

               
            }
        }

        private string BrowseForXMLTemplateLoop()
        {
            string selectedTemplate = null;
            do
            {
                var templateOption = System.Windows.Forms.MessageBox.Show("Select an XML template file to open.", "Open Template", System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Information);
                if (templateOption == System.Windows.Forms.DialogResult.Cancel)
                {
                    return null;
                }
                else
                {
                    selectedTemplate = BrowseForXMLTemplate();
                }
            }
            while (selectedTemplate == null);

            return selectedTemplate;
        }

        private string BrowseForXMLTemplate()
        {
            var browseDlg = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "Open TAE Template XML File",
                Filter = "TAE Templates (*.XML)|*.XML",
                ValidateNames = true,
                CheckFileExists = true,
                CheckPathExists = true,
            };

            browseDlg.InitialDirectory = System.IO.Path.Combine(new System.IO.FileInfo(typeof(TaeEditorScreen).Assembly.Location).DirectoryName, "Res");

            if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                IsReadOnlyFileMode = browseDlg.ReadOnlyChecked;
                return browseDlg.FileName;
            }

            return null;
        }

        private void File_SaveAs()
        {
            var browseDlg = new System.Windows.Forms.SaveFileDialog()
            {
                Filter = FileContainer?.GetResaveFilter()
                           ?? TaeFileContainer.DefaultSaveFilter,
                ValidateNames = true,
                CheckFileExists = false,
                CheckPathExists = true,
            };

            if (System.IO.File.Exists(FileContainerName))
            {
                browseDlg.InitialDirectory = System.IO.Path.GetDirectoryName(FileContainerName);
                browseDlg.FileName = System.IO.Path.GetFileName(FileContainerName);
            }

            if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileContainerName = browseDlg.FileName;
                SaveCurrentFile();
            }
        }

        private void ChangeTypeOfSelectedEvent()
        {
            if (SelectedEventBox == null)
                return;

            PauseUpdate = true;

            var changeTypeDlg = new TaeInspectorFormChangeEventType();
            changeTypeDlg.TAEReference = SelectedTae;
            changeTypeDlg.CurrentTemplate = SelectedEventBox.MyEvent.Template;
            changeTypeDlg.NewEventType = SelectedEventBox.MyEvent.Type;

            if (changeTypeDlg.ShowDialog(GameWindowAsForm) == System.Windows.Forms.DialogResult.OK)
            {
                if (changeTypeDlg.NewEventType != SelectedEventBox.MyEvent.Type)
                {
                    var referenceToEventBox = SelectedEventBox;
                    var referenceToPreviousEvent = referenceToEventBox.MyEvent;
                    int index = SelectedTaeAnim.Events.IndexOf(referenceToEventBox.MyEvent);
                    int row = referenceToEventBox.Row;

                    UndoMan.NewAction(
                        doAction: () =>
                        {
                            SelectedTaeAnim.Events.Remove(referenceToPreviousEvent);

                            referenceToEventBox.ChangeEvent(
                                new TAE.Event(referenceToPreviousEvent.StartTime, referenceToPreviousEvent.EndTime,
                                changeTypeDlg.NewEventType, referenceToPreviousEvent.Unk04, SelectedTae.BigEndian,
                                SelectedTae.BankTemplate[changeTypeDlg.NewEventType]), SelectedTaeAnim);

                            SelectedTaeAnim.Events.Insert(index, referenceToEventBox.MyEvent);

                            SelectedEventBox = null;
                            SelectedEventBox = referenceToEventBox;

                            SelectedEventBox.Row = row;

                            Graph.RegisterEventBoxExistance(SelectedEventBox);

                            SelectedTaeAnim.SetIsModified(!IsReadOnlyFileMode);
                            SelectedTae.SetIsModified(!IsReadOnlyFileMode);
                        },
                        undoAction: () =>
                        {
                            SelectedTaeAnim.Events.RemoveAt(index);
                            referenceToEventBox.ChangeEvent(referenceToPreviousEvent, SelectedTaeAnim);
                            SelectedTaeAnim.Events.Insert(index, referenceToPreviousEvent);

                            SelectedEventBox = null;
                            SelectedEventBox = referenceToEventBox;

                            SelectedEventBox.Row = row;

                            Graph.RegisterEventBoxExistance(SelectedEventBox);

                            SelectedTaeAnim.SetIsModified(!IsReadOnlyFileMode);
                            SelectedTae.SetIsModified(!IsReadOnlyFileMode);
                        });
                }
            }

            PauseUpdate = false;
        }

        private void ButtonChangeType_Click(object sender, EventArgs e)
        {
            inspectorWinFormsControl.dataGridView1.Focus();
            ChangeTypeOfSelectedEvent();
        }

        private (long Upper, long Lower) GetSplitAnimID(long id)
        {
            return ((GameDataManager.GameType == GameDataManager.GameTypes.BB ||
                GameDataManager.GameType == GameDataManager.GameTypes.DS3) 
                ? (id / 1000000) : (id / 10000),
                (GameDataManager.GameType == GameDataManager.GameTypes.BB || 
                GameDataManager.GameType == GameDataManager.GameTypes.DS3) 
                ? (id % 1000000) : (id % 10000));
        }

        private string HKXNameFromCompositeID(long compositeID)
        {
            if (compositeID < 0)
                return "<NONE>";

            var splitID = GetSplitAnimID(compositeID);

            if (GameDataManager.GameType == GameDataManager.GameTypes.BB ||
                GameDataManager.GameType == GameDataManager.GameTypes.DS3)
            {
                return $"a{splitID.Upper:D3}_{splitID.Lower:D6}";
            }
            else
            {
                return $"a{splitID.Upper:D2}_{splitID.Lower:D4}";
            }
        }

        private string HKXSubIDDispNameFromInt(long subID)
        {
            if (FileContainer.AllTAE.Count() == 1)
            {
                return HKXNameFromCompositeID(subID);
            }
            else
            {
                if (GameDataManager.GameType == GameDataManager.GameTypes.BB ||
                GameDataManager.GameType == GameDataManager.GameTypes.DS3)
                {
                    return $"aXXX_{subID:D6}";
                }
                else
                {
                    return $"aXX_{subID:D4}";
                }
            }
            
        }

        public void UpdateSelectedTaeAnimInfoText()
        {
            var stringBuilder = new StringBuilder();

            if (SelectedTaeAnim == null)
            {
                stringBuilder.Append("(No Animation Selected)");
            }
            else
            {
                stringBuilder.Append($"{HKXSubIDDispNameFromInt(SelectedTaeAnim.ID)}");

                if (SelectedTaeAnim.MiniHeader is TAE.Animation.AnimMiniHeader.Standard asStandard)
                {
                    if (asStandard.IsLoopByDefault)
                        stringBuilder.Append($" [{nameof(TAE.Animation.AnimMiniHeader.Standard.IsLoopByDefault)}]");

                    if (asStandard.AllowDelayLoad && asStandard.ImportsHKX)
                    {
                        stringBuilder.Append($" [IMPORTS EVENTS + HKX FROM: {HKXNameFromCompositeID(asStandard.ImportHKXSourceAnimID)}]");
                    }
                    else if (asStandard.AllowDelayLoad && !asStandard.ImportsHKX)
                    {
                        stringBuilder.Append($" [IMPORTS EVENTS FROM: {HKXNameFromCompositeID(asStandard.ImportHKXSourceAnimID)}]");
                    }
                    else if (!asStandard.AllowDelayLoad && asStandard.ImportsHKX)
                    {
                        stringBuilder.Append($" [IMPORTS HKX FROM: {HKXNameFromCompositeID(asStandard.ImportHKXSourceAnimID)}]");
                    }

                }
                else if (SelectedTaeAnim.MiniHeader is TAE.Animation.AnimMiniHeader.ImportOtherAnim asImportOtherAnim)
                {
                    stringBuilder.Append($" [IMPORTS ALL FROM: {HKXNameFromCompositeID(asImportOtherAnim.ImportFromAnimID)}]");
                    stringBuilder.Append($" [UNK: {asImportOtherAnim.Unknown}]");
                }

                //SFTODO

                //if (SelectedTaeAnim.AnimFileReference)
                //{
                //    stringBuilder.Append($" [RefID: {SelectedTaeAnim.RefAnimID}]");
                //}
                //else
                //{
                //    stringBuilder.Append($" [\"{SelectedTaeAnim.Unknown1}\"]");
                //    stringBuilder.Append($" [\"{SelectedTaeAnim.Unknown2}\"]");

                //    //if (SelectedTaeAnim.IsLoopingObjAnim)
                //    //    stringBuilder.Append($" [ObjLoop]");

                //    //if (SelectedTaeAnim.UseHKXOnly)
                //    //    stringBuilder.Append($" [HKXOnly]");

                //    //if (SelectedTaeAnim.TAEDataOnly)
                //    //    stringBuilder.Append($" [TAEOnly]");

                //    //if (SelectedTaeAnim.OriginalAnimID >= 0)
                //    //    stringBuilder.Append($" [OrigID: {SelectedTaeAnim.OriginalAnimID}]");
                //}
            }

            SelectedTaeAnimInfoScrollingText.SetText(stringBuilder.ToString());
        }

        public void SelectEvent(TAE.Event ev)
        {
            var box = Graph.EventBoxes.First(x => x.MyEvent == ev);
            SelectedEventBox = box;

            float left = Graph.ScrollViewer.Scroll.X;
            float top = Graph.ScrollViewer.Scroll.Y;
            float right = Graph.ScrollViewer.Scroll.X + Graph.ScrollViewer.Viewport.Width;
            float bottom = Graph.ScrollViewer.Scroll.Y + Graph.ScrollViewer.Viewport.Height;

            Graph.ScrollViewer.Scroll.X = box.Left - (Graph.ScrollViewer.Viewport.Width / 2f);
            Graph.ScrollViewer.Scroll.Y = (box.Row * Graph.RowHeight) - (Graph.ScrollViewer.Viewport.Height / 2f);
            Graph.ScrollViewer.ClampScroll();
        }

        public void SelectNewAnimRef(TAE tae, TAE.Animation animRef, bool scrollOnCenter = false)
        {
            bool isBlend = (PlaybackCursor.IsPlaying || Graph.ViewportInteractor.IsComboRecording) && Graph.ViewportInteractor.IsBlendingActive;

            AnimSwitchRenderCooldown = AnimSwitchRenderCooldownMax;

            PlaybackCursor.IsStepping = false;

            SelectedTae = tae;

            GameWindowAsForm.Invoke(new Action(() =>
            {
                ButtonEditCurrentTaeHeader.Enabled = true;
                ButtonEditCurrentTaeHeader.Visible = true;
            }));

            SelectedTaeAnim = animRef;

            UpdateSelectedTaeAnimInfoText();

            if (SelectedTaeAnim != null)
            {
                GameWindowAsForm.Invoke(new Action(() =>
                {
                    MenuBar["Edit\\Undo"].Enabled = UndoMan.CanUndo;
                    MenuBar["Edit\\Redo"].Enabled = UndoMan.CanRedo;
                }));

                SelectedEventBox = null;

                //bool wasFirstAnimSelected = false;

                //if (Graph == null)
                //{
                //    wasFirstAnimSelected = true;
                //    Graph = new TaeEditAnimEventGraph(this, false, SelectedTaeAnim);
                //}

                

                LoadAnimIntoGraph(SelectedTaeAnim);

                if (HasntSelectedAnAnimYetAfterBuildingAnimList)
                {
                    UpdateLayout(); // Fixes scroll when you first open anibnd and when you rebuild anim list.
                    HasntSelectedAnAnimYetAfterBuildingAnimList = false;
                }

                AnimationListScreen.ScrollToAnimRef(SelectedTaeAnim, scrollOnCenter);

                Graph.ViewportInteractor.OnNewAnimSelected();
                Graph.PlaybackCursor.ResetAll();
                Graph.PlaybackCursor.RestartFromBeginning();

                if (!isBlend)
                {
                    Graph.ViewportInteractor.CurrentModel.AnimContainer?.ResetAll();
                    Graph.ViewportInteractor.RootMotionSendHome();
                    Graph.ViewportInteractor.CurrentModel.CurrentDirection = 0;
                    Graph.ViewportInteractor.CurrentModel.ChrAsm?.RightWeaponModel0?.ResetCurrentDirection();
                    Graph.ViewportInteractor.CurrentModel.ChrAsm?.RightWeaponModel1?.ResetCurrentDirection();
                    Graph.ViewportInteractor.CurrentModel.ChrAsm?.RightWeaponModel2?.ResetCurrentDirection();
                    Graph.ViewportInteractor.CurrentModel.ChrAsm?.RightWeaponModel3?.ResetCurrentDirection();
                    Graph.ViewportInteractor.CurrentModel.ChrAsm?.LeftWeaponModel0?.ResetCurrentDirection();
                    Graph.ViewportInteractor.CurrentModel.ChrAsm?.LeftWeaponModel1?.ResetCurrentDirection();
                    Graph.ViewportInteractor.CurrentModel.ChrAsm?.LeftWeaponModel2?.ResetCurrentDirection();
                    Graph.ViewportInteractor.CurrentModel.ChrAsm?.LeftWeaponModel3?.ResetCurrentDirection();
                    Graph.ViewportInteractor.ResetRootMotion();
                    Graph.ViewportInteractor.RemoveTransition();
                    Graph.ViewportInteractor.CurrentModel.AnimContainer?.ResetAll();
                }
            }
            else
            {
                GameWindowAsForm.Invoke(new Action(() =>
                {
                    MenuBar["Edit\\Undo"].Enabled = false;
                    MenuBar["Edit\\Redo"].Enabled = false;
                }));
                
                SelectedEventBox = null;

                Graph = null;
            }
        }

        public void ShowDialogChangeAnimName()
        {
            if (SelectedTaeAnim != null)
            {
                PauseUpdate = true;
                var keyboardTask = KeyboardInput.Show("Set Animation Name", "Set the name of the current animation.", SelectedTaeAnim.AnimFileName);
                keyboardTask.Wait();
                if (keyboardTask.Result != null)
                {
                    if (SelectedTaeAnim.AnimFileName != keyboardTask.Result)
                    {
                        SelectedTaeAnim.AnimFileName = keyboardTask.Result;
                        SelectedTaeAnim.SetIsModified(true);
                    }
                }
                PauseUpdate = false;
            }
        }

        public void ShowDialogFind()
        {
            if (FileContainerName == null || SelectedTae == null)
                return;
            //PauseUpdate = true;

            if (FindValueDialog == null)
            {
                FindValueDialog = new TaeFindValueDialog();
                FindValueDialog.LastFindInfo = LastFindInfo;
                FindValueDialog.EditorRef = this;
                FindValueDialog.Owner = GameWindowAsForm;
                FindValueDialog.Show();
                FindValueDialog.FormClosed += FindValueDialog_FormClosed;
            }

            

            //var find = KeyboardInput.Show("Quick Find Event", "Finds the very first animation containing the event with the specified ID number or name (according to template).", "");
            //if (int.TryParse(find.Result, out int typeID))
            //{
            //    var gotoAnim = SelectedTae.Animations.Where(x => x.Events.Any(ev => (int)ev.Type == typeID));
            //    if (gotoAnim.Any())
            //        SelectNewAnimRef(SelectedTae, gotoAnim.First());
            //    else
            //        MessageBox.Show("None Found", "No events of that type found within the currently loaded files.", new[] { "OK" });
            //}
            //else 
            //{
            //    var gotoAnim = SelectedTae.Animations.Where(x => x.Events.Any(ev => ev.TypeName == find.Result));
            //    if (gotoAnim.Any())
            //        SelectNewAnimRef(SelectedTae, gotoAnim.First());
            //    else
            //        MessageBox.Show("None Found", "No events of that type found within the currently loaded files.", new[] { "OK" });
            //}


            
            //PauseUpdate = false;
        }

        private void FindValueDialog_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            FindValueDialog.FormClosed -= FindValueDialog_FormClosed;
            FindValueDialog = null;
        }

        public void ShowDialogGotoAnimSectionID()
        {
            if (FileContainer == null || SelectedTae == null)
                return;
            PauseUpdate = true;
            var anim = KeyboardInput.Show("Goto Anim Section", "Goes to the animation section with the ID\n" +
                "entered, if applicable.");

            if (!anim.IsCanceled && anim.Result != null)
            {
                if (int.TryParse(anim.Result.Replace("a", "").Replace("_", ""), out int id))
                {
                    if (!GotoAnimSectionID(id, scrollOnCenter: true))
                    {
                        MessageBox.Show("Goto Failed", $"Unable to find anim section {id}.", new[] { "OK" });
                    }
                }
                else
                {
                    MessageBox.Show("Goto Failed", $"\"{anim.Result}\" is not a valid integer.", new[] { "OK" });
                }
            }

            PauseUpdate = false;
        }

        public void ShowDialogGotoAnimID()
        {
            if (FileContainer == null || SelectedTae == null)
                return;
            PauseUpdate = true;
            var anim = KeyboardInput.Show("Goto Anim", "Goes to the animation with the ID\n" +
                "entered, if applicable.");

            if (!anim.IsCanceled && anim.Result != null)
            {
                if (int.TryParse(anim.Result.Replace("a", "").Replace("_", ""), out int id))
                {
                    if (!GotoAnimID(id, scrollOnCenter: true))
                    {
                        MessageBox.Show("Goto Failed", $"Unable to find anim {id}.", new[] { "OK" });
                    }
                }
                else
                {
                    MessageBox.Show("Goto Failed", $"\"{anim.Result}\" is not a valid integer.", new[] { "OK" });
                }
            }
            
            PauseUpdate = false;
        }

        private void NextAnim(bool shiftPressed, bool ctrlPressed)
        {
            try
            {
                if (SelectedTae != null)
                {
                    if (SelectedTaeAnim != null)
                    {
                        var taeList = FileContainer.AllTAE.ToList();

                        int currentAnimIndex = SelectedTae.Animations.IndexOf(SelectedTaeAnim);
                        int currentTaeIndex = taeList.IndexOf(SelectedTae);

                        int startingTaeIndex = currentTaeIndex;

                        void DoSmallStep()
                        {
                            if (currentAnimIndex >= taeList[currentTaeIndex].Animations.Count - 1)
                            {
                                currentAnimIndex = 0;

                                if (taeList.Count > 1)
                                {
                                    if (currentTaeIndex >= taeList.Count - 1)
                                    {
                                        currentTaeIndex = 0;
                                    }
                                    else
                                    {
                                        currentTaeIndex++;
                                        if (taeList[currentTaeIndex].Animations.Count == 0)
                                            DoSmallStep();
                                    }
                                }
                            }
                            else
                            {
                                currentAnimIndex++;
                            }
                        }

                        void DoBigStep()
                        {
                            if (taeList.Count > 1)
                            {
                                while (currentTaeIndex == startingTaeIndex)
                                {
                                    DoSmallStep();
                                }

                                currentAnimIndex = 0;
                            }
                            else
                            {
                                var startSection = GameDataManager.GameTypeHasLongAnimIDs ? (SelectedTaeAnim.ID / 1_000000) : (SelectedTaeAnim.ID / 1_0000);

                                //long stopAtSection = -1;
                                for (int i = currentAnimIndex; i < SelectedTae.Animations.Count; i++)
                                {
                                    var thisSection = GameDataManager.GameTypeHasLongAnimIDs ? (SelectedTae.Animations[i].ID / 1_000000) : (SelectedTae.Animations[i].ID / 1_0000);
                                    if (startSection != thisSection)
                                    {
                                        currentAnimIndex = i;
                                        return;
                                        //if (stopAtSection == -1)
                                        //{
                                        //    stopAtSection = thisSection;
                                        //    currentAnimIndex = i;
                                        //}
                                        //else
                                        //{
                                        //    if (thisSection == stopAtSection)
                                        //    {
                                        //        currentAnimIndex = i;
                                        //    }
                                        //    else
                                        //    {
                                        //        return;
                                        //    }
                                        //}
                                    }
                                }

                                currentAnimIndex = 0;
                            }

                            
                        }

                        void DoStep()
                        {
                            if (ctrlPressed)
                                DoBigStep();
                            else
                                DoSmallStep();
                        }

                        if (shiftPressed)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                DoStep();
                            }
                        }
                        else
                        {
                            DoStep();
                        }

                        try
                        {
                            SelectNewAnimRef(taeList[currentTaeIndex], taeList[currentTaeIndex].Animations[currentAnimIndex], scrollOnCenter: Input.ShiftHeld || Input.CtrlHeld);
                        }
                        catch// (Exception innerEx)
                        {
                            //Console.WriteLine("FATCAT");
                        }
                        
                    }
                }
            }
            catch// (Exception ex)
            {
                //Console.WriteLine("Fatcat");
            }
        }

        public void PrevAnim(bool shiftPressed, bool ctrlPressed)
        {
            try
            {
                if (SelectedTae != null)
                {
                    if (SelectedTaeAnim != null)
                    {
                        var taeList = FileContainer.AllTAE.ToList();

                        int currentAnimIndex = SelectedTae.Animations.IndexOf(SelectedTaeAnim);

                        int currentTaeIndex = taeList.IndexOf(SelectedTae);

                        int startingTaeIndex = currentTaeIndex;

                        void DoSmallStep()
                        {
                            if (currentAnimIndex <= 0)
                            {
                                if (taeList.Count > 1)
                                {
                                    if (currentTaeIndex <= 0)
                                    {
                                        currentTaeIndex = taeList.Count - 1;
                                    }
                                    else
                                    {
                                        currentTaeIndex--;
                                        if (taeList[currentTaeIndex].Animations.Count == 0)
                                            DoSmallStep();
                                    }
                                }

                                currentAnimIndex = taeList[currentTaeIndex].Animations.Count - 1;
                            }
                            else
                            {
                                currentAnimIndex--;
                            }
                        }

                        void DoBigStep()
                        {
                            if (taeList.Count > 1)
                            {
                                while (currentTaeIndex == startingTaeIndex)
                                {
                                    DoSmallStep();
                                }

                                currentAnimIndex = 0;
                            }
                            else
                            {
                                var startSection = GameDataManager.GameTypeHasLongAnimIDs ? (SelectedTaeAnim.ID / 1_000000) : (SelectedTaeAnim.ID / 1_0000);
                                if (currentAnimIndex == 0)
                                    currentAnimIndex = SelectedTae.Animations.Count - 1;
                                long stopAtSection = -1;
                                for (int i = currentAnimIndex; i >= 0; i--)
                                {
                                    var thisSection = GameDataManager.GameTypeHasLongAnimIDs ? (SelectedTae.Animations[i].ID / 1_000000) : (SelectedTae.Animations[i].ID / 1_0000);
                                    if (startSection != thisSection)
                                    {
                                        if (stopAtSection == -1)
                                        {
                                            stopAtSection = thisSection;
                                            currentAnimIndex = i;
                                        }
                                        else
                                        {
                                            if (thisSection == stopAtSection)
                                            {
                                                currentAnimIndex = i;
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }
                                    }
                                }

                                //currentAnimIndex = 0;
                            }

                            
                        }

                        void DoStep()
                        {
                            if (ctrlPressed)
                                DoBigStep();
                            else
                                DoSmallStep();
                        }

                        if (shiftPressed)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                DoStep();
                            }
                        }
                        else
                        {
                            DoStep();
                        }

                        SelectNewAnimRef(taeList[currentTaeIndex], taeList[currentTaeIndex].Animations[currentAnimIndex], scrollOnCenter: Input.ShiftHeld || Input.CtrlHeld);
                    }
                }
            }
            catch
            {

            }
        }

        public void TransportNextFrame()
        {
            PlaybackCursor.IsPlaying = false;
            PlaybackCursor.IsStepping = true;

            PlaybackCursor.CurrentTime += PlaybackCursor.CurrentSnapInterval;
            PlaybackCursor.CurrentTime = Math.Floor(PlaybackCursor.CurrentTime / PlaybackCursor.CurrentSnapInterval) * PlaybackCursor.CurrentSnapInterval;

            if (PlaybackCursor.CurrentTime > PlaybackCursor.MaxTime)
                PlaybackCursor.CurrentTime %= PlaybackCursor.MaxTime;

            //PlaybackCursor.StartTime = PlaybackCursor.CurrentTime;
            Graph.ScrollToPlaybackCursor(1);

        }

        public void TransportPreviousFrame()
        {
            PlaybackCursor.IsPlaying = false;
            PlaybackCursor.IsStepping = true;

            PlaybackCursor.CurrentTime -= PlaybackCursor.CurrentSnapInterval;
            PlaybackCursor.CurrentTime = Math.Floor(PlaybackCursor.CurrentTime / PlaybackCursor.CurrentSnapInterval) * PlaybackCursor.CurrentSnapInterval;

            if (PlaybackCursor.CurrentTime < 0)
                PlaybackCursor.CurrentTime += PlaybackCursor.MaxTime;

            //PlaybackCursor.StartTime = PlaybackCursor.CurrentTime;

            Graph.ScrollToPlaybackCursor(1);
        }

        public void HardReset()
        {
            if (Graph == null)
                return;
            Graph.ViewportInteractor.CurrentModel.AnimContainer?.ResetAll();
            Graph.ViewportInteractor.RootMotionSendHome();
            Graph.ViewportInteractor.CurrentModel.CurrentDirection = 0;
            Graph.ViewportInteractor.CurrentModel.ChrAsm?.RightWeaponModel0?.ResetCurrentDirection();
            Graph.ViewportInteractor.CurrentModel.ChrAsm?.RightWeaponModel1?.ResetCurrentDirection();
            Graph.ViewportInteractor.CurrentModel.ChrAsm?.RightWeaponModel2?.ResetCurrentDirection();
            Graph.ViewportInteractor.CurrentModel.ChrAsm?.RightWeaponModel3?.ResetCurrentDirection();
            Graph.ViewportInteractor.CurrentModel.ChrAsm?.LeftWeaponModel0?.ResetCurrentDirection();
            Graph.ViewportInteractor.CurrentModel.ChrAsm?.LeftWeaponModel1?.ResetCurrentDirection();
            Graph.ViewportInteractor.CurrentModel.ChrAsm?.LeftWeaponModel2?.ResetCurrentDirection();
            Graph.ViewportInteractor.CurrentModel.ChrAsm?.LeftWeaponModel3?.ResetCurrentDirection();
            Graph.ViewportInteractor.ResetRootMotion();
            Graph.ViewportInteractor.RemoveTransition();
            Graph.PlaybackCursor.RestartFromBeginning();
            Graph.ViewportInteractor.CurrentModel.AnimContainer?.ResetAll();
        }

        public void Update()
        {
            if (!Input.LeftClickHeld)
            {
                Graph?.ReleaseCurrentDrag();
            }

            if (!Main.Active)
            {
                FmodManager.StopAllSounds();
                MenuBar.CloseAll();
            }

            PauseUpdate = MenuBar.BlockInput;

            if (!PauseUpdate)
            {
                //bad hotfix warning
                if (Graph != null &&
                    !Graph.ScrollViewer.DisableVerticalScroll &&
                    Input.MousePosition.X >=
                      Graph.Rect.Right -
                      Graph.ScrollViewer.ScrollBarThickness &&
                    Input.MousePosition.X < DividerRightGrabStartX &&
                    Input.MousePosition.Y >= Graph.Rect.Top &&
                    Input.MousePosition.Y < Graph.Rect.Bottom)
                {
                    Input.CursorType = MouseCursorType.Arrow;
                }

                // Another bad hotfix
                Rectangle killCursorRect = new Rectangle(
                      (int)(DividerLeftGrabEndX),
                      0,
                      (int)(DividerRightGrabStartX - DividerLeftGrabEndX),
                      TopOfGraphAnimInfoMargin + Rect.Top + TopMenuBarMargin);

                if (killCursorRect.Contains(Input.MousePositionPoint))
                {
                    Input.CursorType = MouseCursorType.Arrow;
                }

                if (!Input.LeftClickHeld)
                    Graph?.MouseReleaseStuff();
            }

            //if (MultiSelectedEventBoxes.Count > 0 && multiSelectedEventBoxesCountLastFrame < MultiSelectedEventBoxes.Count)
            //{
            //    if (Config.UseGamesMenuSounds)
            //        FmodManager.PlaySE("f000000000");
            //}

            multiSelectedEventBoxesCountLastFrame = MultiSelectedEventBoxes.Count;

            // Always update playback regardless of GUI memes.
            // Still only allow hitting spacebar to play/pause
            // if the window is in focus.
            // Also for Interactor
            if (Graph != null)
            {
                Graph.UpdatePlaybackCursor(allowPlayPauseInput: Main.Active);
                Graph.ViewportInteractor?.GeneralUpdate();
            }

            

            if (PauseUpdate)
            {
                return;
            }

            Transport.Update(Main.DELTA_UPDATE);

            bool isOtherPaneFocused = ModelViewerBounds.Contains((int)Input.LeftClickDownAnchor.X, (int)Input.LeftClickDownAnchor.Y) ||
                inspectorWinFormsControl.Bounds.Contains(Input.LeftClickDownAnchor.ToDrawingPoint());

            if (Main.Active)
            {
                if (Input.KeyDown(Keys.Escape))
                    FmodManager.StopAllSounds();

                if (Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.F1))
                    ChangeTypeOfSelectedEvent();

                if (Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.F2))
                    ShowDialogChangeAnimName();

                if (Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.F3))
                    ShowDialogEditCurrentAnimInfo();

                if (Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.F4))
                    GoToEventSource();

                if (Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.F5))
                    LiveRefresh();

                if (Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.F8))
                    ShowComboMenu();

                var zHeld = Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.Z);
                var yHeld = Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.Y);

                if (Input.CtrlHeld && !Input.ShiftHeld && !Input.AltHeld)
                {
                    if ((Input.KeyDown(Keys.OemPlus) || Input.KeyDown(Keys.Add)) && !isOtherPaneFocused)
                    {
                        Graph?.ZoomInOneNotch(
                            (float)(
                            (Graph.PlaybackCursor.GUICurrentTime * Graph.SecondsPixelSize)
                            - Graph.ScrollViewer.Scroll.X));
                    }
                    else if ((Input.KeyDown(Keys.OemMinus) || Input.KeyDown(Keys.Subtract)) && !isOtherPaneFocused)
                    {
                        Graph?.ZoomOutOneNotch(
                            (float)(
                            (Graph.PlaybackCursor.GUICurrentTime * Graph.SecondsPixelSize)
                            - Graph.ScrollViewer.Scroll.X));
                    }
                    else if ((Input.KeyDown(Keys.D0) || Input.KeyDown(Keys.NumPad0)) && !isOtherPaneFocused)
                    {
                        Graph?.ResetZoom(0);
                    }
                    else if (!CurrentlyEditingSomethingInInspector && Input.KeyDown(Keys.C) && 
                        WhereCurrentMouseClickStarted != ScreenMouseHoverKind.Inspector && !isOtherPaneFocused)
                    {
                        Graph?.DoCopy();
                    }
                    else if (!CurrentlyEditingSomethingInInspector && Input.KeyDown(Keys.X) && 
                        WhereCurrentMouseClickStarted != ScreenMouseHoverKind.Inspector && !isOtherPaneFocused)
                    {
                        Graph?.DoCut();
                    }
                    else if (!CurrentlyEditingSomethingInInspector && Input.KeyDown(Keys.V) && 
                        WhereCurrentMouseClickStarted != ScreenMouseHoverKind.Inspector && !isOtherPaneFocused)
                    {
                        Graph?.DoPaste(isAbsoluteLocation: false);
                    }
                    else if (!CurrentlyEditingSomethingInInspector && Input.KeyDown(Keys.A) && !isOtherPaneFocused)
                    {
                        if (Graph != null && Graph.currentDrag.DragType == BoxDragType.None)
                        {
                            SelectedEventBox = null;
                            MultiSelectedEventBoxes.Clear();
                            foreach (var box in Graph.EventBoxes)
                            {
                                MultiSelectedEventBoxes.Add(box);
                            }
                            UpdateInspectorToSelection();
                        }
                    }
                    else if (Input.KeyDown(Keys.F))
                    {
                        ShowDialogFind();
                    }
                    else if (Input.KeyDown(Keys.G))
                    {
                        ShowDialogGotoAnimID();
                    }
                    else if (Input.KeyDown(Keys.H))
                    {
                        ShowDialogGotoAnimSectionID();
                    }
                    else if (Input.KeyDown(Keys.S))
                    {
                        SaveCurrentFile();
                    }
                    else if (Graph != null && Input.KeyDown(Keys.R))
                    {
                        HardReset();
                    }
                }

                if (Input.CtrlHeld && Input.ShiftHeld && !Input.AltHeld)
                {
                    if (Input.KeyDown(Keys.V) && !isOtherPaneFocused)
                    {
                        Graph.DoPaste(isAbsoluteLocation: true);
                    }
                    else if (Input.KeyDown(Keys.S))
                    {
                        File_SaveAs();
                    }
                }

                if (!Input.CtrlHeld && Input.ShiftHeld && !Input.AltHeld)
                {
                    if (Input.KeyDown(Keys.D))
                    {
                        if (SelectedEventBox != null)
                            SelectedEventBox = null;
                        if (MultiSelectedEventBoxes.Count > 0)
                            MultiSelectedEventBoxes.Clear();
                    }
                }

                if (Input.KeyDown(Keys.Delete) && !isOtherPaneFocused)
                {
                    Graph.DeleteSelectedEvent();
                }

                //if (Graph != null && Input.KeyDown(Keys.Home) && !Graph.PlaybackCursor.Scrubbing)
                //{
                //    if (CtrlHeld)
                //        Graph.PlaybackCursor.IsPlaying = false;

                //    Graph.PlaybackCursor.CurrentTime = ShiftHeld ? 0 : Graph.PlaybackCursor.StartTime;
                //    Graph.ViewportInteractor.ResetRootMotion(0);

                //    Graph.ScrollToPlaybackCursor(1);
                //}

                

                //if (Graph != null && Input.KeyDown(Keys.End) && !Graph.PlaybackCursor.Scrubbing)
                //{
                //    if (CtrlHeld)
                //        Graph.PlaybackCursor.IsPlaying = false;

                //    Graph.PlaybackCursor.CurrentTime = Graph.PlaybackCursor.MaxTime;
                //    Graph.ViewportInteractor.ResetRootMotion((float)Graph.PlaybackCursor.MaxFrame);

                //    Graph.ScrollToPlaybackCursor(1);
                //}

                NextAnimRepeaterButton.Update(GamePadState.Default, Main.DELTA_UPDATE, Input.KeyHeld(Keys.Down) && !Input.KeyHeld(Keys.Up));

                if (NextAnimRepeaterButton.State)
                {
                    Graph?.ViewportInteractor?.CancelCombo();
                    NextAnim(Input.ShiftHeld, Input.CtrlHeld);
                }

                PrevAnimRepeaterButton.Update(GamePadState.Default, Main.DELTA_UPDATE, Input.KeyHeld(Keys.Up) && !Input.KeyHeld(Keys.Down));

                if (PrevAnimRepeaterButton.State)
                {
                    Graph?.ViewportInteractor?.CancelCombo();
                    PrevAnim(Input.ShiftHeld, Input.CtrlHeld);
                }

                if (PlaybackCursor != null)
                {
                    NextFrameRepeaterButton.Update(GamePadState.Default, Main.DELTA_UPDATE, Input.KeyHeld(Keys.Right));

                    if (NextFrameRepeaterButton.State)
                    {
                        TransportNextFrame();
                    }

                    PrevFrameRepeaterButton.Update(GamePadState.Default, Main.DELTA_UPDATE, Input.KeyHeld(Keys.Left));

                    if (PrevFrameRepeaterButton.State)
                    {
                        TransportPreviousFrame();
                    }
                }

                if (Input.KeyDown(Keys.Space) && Input.CtrlHeld && !Input.AltHeld)
                {
                    if (SelectedTae != null)
                    {
                        if (SelectedTaeAnim != null)
                        {
                            SelectNewAnimRef(SelectedTae, SelectedTaeAnim);
                            if (Input.ShiftHeld)
                            {
                                Graph?.ViewportInteractor?.RemoveTransition();
                            }
                        }
                    }
                }

                if (Input.KeyDown(Keys.Back) && !isOtherPaneFocused)
                {
                    Graph?.ViewportInteractor?.RemoveTransition();
                }

                if (UndoButton.Update(Main.DELTA_UPDATE, (Input.CtrlHeld && !Input.ShiftHeld && !Input.AltHeld) && (zHeld && !yHeld)) && !isOtherPaneFocused)
                {
                    UndoMan.Undo();
                }

                if (RedoButton.Update(Main.DELTA_UPDATE, (Input.CtrlHeld && !Input.ShiftHeld && !Input.AltHeld) && (!zHeld && yHeld)) && !isOtherPaneFocused)
                {
                    UndoMan.Redo();
                }
            }

            if (!Input.LeftClickHeld)
                WhereCurrentMouseClickStarted = ScreenMouseHoverKind.None;


            if (WhereCurrentMouseClickStarted == ScreenMouseHoverKind.None)
            {
                if (Input.MousePosition.Y >= TopMenuBarMargin && Input.MousePosition.Y <= Rect.Bottom
                    && Input.MousePosition.X >= DividerLeftGrabStartX && Input.MousePosition.X <= DividerLeftGrabEndX)
                {
                    MouseHoverKind = ScreenMouseHoverKind.DividerBetweenCenterAndLeftPane;
                    //Input.CursorType = MouseCursorType.DragX;
                    if (Input.LeftClickDown)
                    {
                        CurrentDividerDragMode = DividerDragMode.LeftVertical;
                        WhereCurrentMouseClickStarted = ScreenMouseHoverKind.DividerBetweenCenterAndLeftPane;
                    }
                }
                else if (Input.MousePosition.Y >= TopMenuBarMargin && Input.MousePosition.Y <= Rect.Bottom
                    && Input.MousePosition.X >= DividerRightGrabStartX && Input.MousePosition.X <= DividerRightGrabEndX)
                {
                    MouseHoverKind = ScreenMouseHoverKind.DividerBetweenCenterAndRightPane;
                    //Input.CursorType = MouseCursorType.DragX;
                    if (Input.LeftClickDown)
                    {
                        CurrentDividerDragMode = DividerDragMode.RightVertical;
                        WhereCurrentMouseClickStarted = ScreenMouseHoverKind.DividerBetweenCenterAndRightPane;
                    }
                }
                else if (Input.MousePosition.X >= RightSectionStartX && Input.MousePosition.X <= Rect.Right 
                    && Input.MousePosition.Y >= DividerRightPaneHorizontalGrabStartY && Input.MousePosition.Y <= DividerRightPaneHorizontalGrabEndY)
                {
                    MouseHoverKind = ScreenMouseHoverKind.DividerRightPaneHorizontal;
                    //Input.CursorType = MouseCursorType.DragX;
                    if (Input.LeftClickDown)
                    {
                        CurrentDividerDragMode = DividerDragMode.RightPaneHorizontal;
                        WhereCurrentMouseClickStarted = ScreenMouseHoverKind.DividerRightPaneHorizontal;
                    }
                }
                else if (MouseHoverKind == ScreenMouseHoverKind.DividerBetweenCenterAndLeftPane
                    || MouseHoverKind == ScreenMouseHoverKind.DividerBetweenCenterAndRightPane
                    || MouseHoverKind == ScreenMouseHoverKind.DividerRightPaneHorizontal)
                {
                    MouseHoverKind = ScreenMouseHoverKind.None;
                }
            }

            if (MouseHoverKind == ScreenMouseHoverKind.DividerBetweenCenterAndLeftPane 
                || WhereCurrentMouseClickStarted == ScreenMouseHoverKind.DividerBetweenCenterAndLeftPane
                || MouseHoverKind == ScreenMouseHoverKind.DividerBetweenCenterAndRightPane 
                || WhereCurrentMouseClickStarted == ScreenMouseHoverKind.DividerBetweenCenterAndRightPane)
            {
                Input.CursorType = MouseCursorType.DragX;
            }
            else if (MouseHoverKind == ScreenMouseHoverKind.DividerRightPaneHorizontal 
                || WhereCurrentMouseClickStarted == ScreenMouseHoverKind.DividerRightPaneHorizontal)
            {
                Input.CursorType = MouseCursorType.DragY;
            }

            if (CurrentDividerDragMode == DividerDragMode.LeftVertical)
            {
                if (Input.LeftClickHeld)
                {
                    //Input.CursorType = MouseCursorType.DragX;
                    LeftSectionWidth = MathHelper.Max((Input.MousePosition.X - Rect.X) - (DividerVisiblePad / 2), LeftSectionWidthMin);
                    LeftSectionWidth = MathHelper.Min(LeftSectionWidth, Rect.Width - MiddleSectionWidthMin - RightSectionWidth - (DividerVisiblePad * 2));
                    MouseHoverKind = ScreenMouseHoverKind.DividerBetweenCenterAndLeftPane;
                    Main.RequestViewportRenderTargetResolutionChange = true;
                    Main.RequestHideOSD = Main.RequestHideOSD_MAX;
                }
                else
                {
                    //Input.CursorType = MouseCursorType.Arrow;
                    CurrentDividerDragMode = DividerDragMode.None;
                    WhereCurrentMouseClickStarted = ScreenMouseHoverKind.None;
                }
            }
            else if (CurrentDividerDragMode == DividerDragMode.RightVertical)
            {
                if (Input.LeftClickHeld)
                {
                    //Input.CursorType = MouseCursorType.DragX;
                    RightSectionWidth = MathHelper.Max((Rect.Right - Input.MousePosition.X) + (DividerVisiblePad / 2), RightSectionWidthMin);
                    RightSectionWidth = MathHelper.Min(RightSectionWidth, Rect.Width - MiddleSectionWidthMin - LeftSectionWidth - (DividerVisiblePad * 2));
                    MouseHoverKind = ScreenMouseHoverKind.DividerBetweenCenterAndRightPane;
                    Main.RequestViewportRenderTargetResolutionChange = true;
                    Main.RequestHideOSD = Main.RequestHideOSD_MAX;
                }
                else
                {
                    //Input.CursorType = MouseCursorType.Arrow;
                    CurrentDividerDragMode = DividerDragMode.None;
                    WhereCurrentMouseClickStarted = ScreenMouseHoverKind.None;
                }
            }
            else if (CurrentDividerDragMode == DividerDragMode.RightPaneHorizontal)
            {
                if (Input.LeftClickHeld)
                {
                    //Input.CursorType = MouseCursorType.DragY;
                    TopRightPaneHeight = MathHelper.Max((Input.MousePosition.Y - Rect.Top - TopMenuBarMargin - TransportHeight) + (DividerVisiblePad / 2), TopRightPaneHeightMinNew);
                    TopRightPaneHeight = MathHelper.Min(TopRightPaneHeight, Rect.Height - BottomRightPaneHeightMinNew - DividerVisiblePad - TopMenuBarMargin - TransportHeight);
                    MouseHoverKind = ScreenMouseHoverKind.DividerBetweenCenterAndRightPane;
                    Main.RequestViewportRenderTargetResolutionChange = true;
                    Main.RequestHideOSD = Main.RequestHideOSD_MAX;
                }
                else
                {
                    //Input.CursorType = MouseCursorType.Arrow;
                    CurrentDividerDragMode = DividerDragMode.None;
                    WhereCurrentMouseClickStarted = ScreenMouseHoverKind.None;
                }
            }

            if (!Rect.Contains(Input.MousePositionPoint))
            {
                MouseHoverKind = ScreenMouseHoverKind.None;
            }

            // Very specific edge case to handle before you load an anibnd so that
            // it won't have the resize cursor randomly. This box spans all the way
            // from left of screen to the hitbox of the right vertical divider and
            // just immediately clears the resize cursor in that entire huge region.
            if (AnimationListScreen == null && Graph == null
                    && new Rectangle(Rect.Left, Rect.Top, (int)(DividerRightGrabStartX - Rect.Left), Rect.Height).Contains(Input.MousePositionPoint))
            {
                MouseHoverKind = ScreenMouseHoverKind.None;
                Input.CursorType = MouseCursorType.Arrow;
            }

            // Check if currently dragging to resize panes.
            if (WhereCurrentMouseClickStarted == ScreenMouseHoverKind.DividerBetweenCenterAndLeftPane
                || WhereCurrentMouseClickStarted == ScreenMouseHoverKind.DividerBetweenCenterAndRightPane)
            {
                Input.CursorType = MouseCursorType.DragX;
                GFX.World.DisableAllInput = true;
                return;
            }
            else if (WhereCurrentMouseClickStarted == ScreenMouseHoverKind.DividerRightPaneHorizontal)
            {
                Input.CursorType = MouseCursorType.DragY;
                GFX.World.DisableAllInput = true;
                return;
            }
            else if (WhereCurrentMouseClickStarted == ScreenMouseHoverKind.ShaderAdjuster)
            {
                GFX.World.DisableAllInput = true;
                return;
            }
            else if (!(MouseHoverKind == ScreenMouseHoverKind.DividerBetweenCenterAndRightPane
                || MouseHoverKind == ScreenMouseHoverKind.DividerBetweenCenterAndLeftPane
                || MouseHoverKind == ScreenMouseHoverKind.DividerRightPaneHorizontal))
            {
                if (AnimationListScreen != null && AnimationListScreen.Rect.Contains(Input.MousePositionPoint))
                    MouseHoverKind = ScreenMouseHoverKind.AnimList;
                else if (Graph != null && Graph.Rect.Contains(Input.MousePositionPoint))
                    MouseHoverKind = ScreenMouseHoverKind.EventGraph;
                else if (
                    new Rectangle(
                        inspectorWinFormsControl.Bounds.Left,
                        inspectorWinFormsControl.Bounds.Top,
                        inspectorWinFormsControl.Bounds.Width,
                        inspectorWinFormsControl.Bounds.Height
                        ).InverseDpiScaled()
                        .Contains(Input.MousePositionPoint))
                    MouseHoverKind = ScreenMouseHoverKind.Inspector;
                //else if (ShaderAdjuster.Bounds.Contains(new System.Drawing.Point(Input.MousePositionPoint.X, Input.MousePositionPoint.Y)))
                //    MouseHoverKind = ScreenMouseHoverKind.ShaderAdjuster;
                else if (
                    ModelViewerBounds_InputArea.Contains(Input.MousePositionPoint))
                {
                    MouseHoverKind = ScreenMouseHoverKind.ModelViewer;
                }
                else
                    MouseHoverKind = ScreenMouseHoverKind.None;

                if (Input.LeftClickDown)
                {
                    WhereCurrentMouseClickStarted = MouseHoverKind;
                }

                if (AnimationListScreen != null)
                {

                    if (MouseHoverKind == ScreenMouseHoverKind.AnimList || 
                        WhereCurrentMouseClickStarted == ScreenMouseHoverKind.AnimList)
                    {
                        Input.CursorType = MouseCursorType.Arrow;
                        AnimationListScreen.Update(Main.DELTA_UPDATE, 
                            allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);
                    }
                    else
                    {
                        AnimationListScreen.UpdateMouseOutsideRect(Main.DELTA_UPDATE, 
                            allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);
                    }
                }

                if (Graph != null)
                {
                    Graph.UpdateMiddleClickPan();

                    if (!Graph.Rect.Contains(Input.MousePositionPoint))
                    {
                        HoveringOverEventBox = null;
                    }

                    if (MouseHoverKind == ScreenMouseHoverKind.EventGraph || 
                        WhereCurrentMouseClickStarted == ScreenMouseHoverKind.EventGraph)
                    {
                        Graph.Update(allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);
                    }
                    else
                    {
                        Graph.UpdateMouseOutsideRect(Main.DELTA_UPDATE, allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);
                    }
                }

                if (MouseHoverKind == ScreenMouseHoverKind.ModelViewer || 
                    WhereCurrentMouseClickStarted == ScreenMouseHoverKind.ModelViewer)
                {
                    Input.CursorType = MouseCursorType.Arrow;
                    GFX.World.DisableAllInput = false;
                }
                else
                {
                    //GFX.World.DisableAllInput = true;
                }

                if (MouseHoverKind == ScreenMouseHoverKind.Inspector || 
                    WhereCurrentMouseClickStarted == ScreenMouseHoverKind.Inspector)
                {
                    Input.CursorType = MouseCursorType.Arrow;
                }
            }

            //else
            //{
            //    Input.CursorType = MouseCursorType.Arrow;
            //}

            //if (MouseHoverKind == ScreenMouseHoverKind.Inspector)
            //    Input.CursorType = MouseCursorType.StopUpdating;

            //if (editScreenGraphInspector.Rect.Contains(Input.MousePositionPoint))
            //    editScreenGraphInspector.Update(elapsedSeconds, allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);
            //else
            //    editScreenGraphInspector.UpdateMouseOutsideRect(elapsedSeconds, allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);

            oldMouseHoverKind = MouseHoverKind;
        }

        public void HandleWindowResize(Rectangle oldBounds, Rectangle newBounds)
        {
            if (oldBounds.Width > 0 && oldBounds.Height > 0 && newBounds.Width > 0 && newBounds.Height > 0)
            {
                float ratioW = 1.0f * newBounds.Width / oldBounds.Width;
                float ratioH = 1.0f * newBounds.Height / oldBounds.Height;

                RightSectionWidth = RightSectionWidth * ratioW;
                TopRightPaneHeight = TopRightPaneHeight * ratioH;

                UpdateLayout();
            }
            
        }

        private void UpdateLayout()
        {
           

            if (Rect.IsEmpty)
            {
                return;
            }

            GameWindowAsForm.Invoke(new Action(() =>
            {
                if (TopRightPaneHeight < TopRightPaneHeightMinNew)
                    TopRightPaneHeight = TopRightPaneHeightMinNew;




                if (RightSectionWidth < RightSectionWidthMin)
                    RightSectionWidth = RightSectionWidthMin;

                if (TopRightPaneHeight > (Rect.Height - BottomRightPaneHeightMinNew - TopMenuBarMargin - TransportHeight))
                {
                    TopRightPaneHeight = (Rect.Height - BottomRightPaneHeightMinNew - TopMenuBarMargin - TransportHeight);
                    Main.RequestViewportRenderTargetResolutionChange = true;
                    Main.RequestHideOSD = Main.RequestHideOSD_MAX;
                }

                if (AnimationListScreen != null && Graph != null)
                {
                    if (LeftSectionWidth < LeftSectionWidthMin)
                    {
                        LeftSectionWidth = LeftSectionWidthMin;
                        Main.RequestViewportRenderTargetResolutionChange = true;
                        Main.RequestHideOSD = Main.RequestHideOSD_MAX;
                    }


                    if (MiddleSectionWidth < MiddleSectionWidthMin)
                    {
                        var adjustment = MiddleSectionWidthMin - MiddleSectionWidth;
                        RightSectionWidth -= adjustment;
                        Main.RequestViewportRenderTargetResolutionChange = true;
                        Main.RequestHideOSD = Main.RequestHideOSD_MAX;
                    }

                    AnimationListScreen.Rect = new Rectangle(
                        (int)LeftSectionStartX,
                        Rect.Top + TopMenuBarMargin,
                        (int)LeftSectionWidth,
                        Rect.Height - TopMenuBarMargin - EditTaeHeaderButtonMargin);

                    Graph.Rect = new Rectangle(
                        (int)MiddleSectionStartX,
                        Rect.Top + TopMenuBarMargin + TopOfGraphAnimInfoMargin,
                        (int)MiddleSectionWidth,
                        Rect.Height - TopMenuBarMargin - TopOfGraphAnimInfoMargin);

                    var plannedGraphRect = new Rectangle(
                        (int)MiddleSectionStartX,
                        Rect.Top + TopMenuBarMargin + TopOfGraphAnimInfoMargin,
                        (int)MiddleSectionWidth,
                        Rect.Height - TopMenuBarMargin - TopOfGraphAnimInfoMargin);

                    ButtonEditCurrentAnimInfo.Bounds = new System.Drawing.Rectangle(
                        plannedGraphRect.Right - 4 - ButtonEditCurrentAnimInfoWidth,
                        Rect.Top + TopMenuBarMargin,
                        ButtonEditCurrentAnimInfoWidth,
                        TopOfGraphAnimInfoMargin).DpiScaled();

                    ButtonGotoEventSource.Bounds = new System.Drawing.Rectangle(
                        plannedGraphRect.Right - 4 - ButtonEditCurrentAnimInfoWidth - 8 - ButtonGotoEventSourceWidth,
                        Rect.Top + TopMenuBarMargin,
                        ButtonGotoEventSourceWidth,
                        TopOfGraphAnimInfoMargin).DpiScaled();

                }
                else
                {
                    var plannedGraphRect = new Rectangle(
                        (int)MiddleSectionStartX,
                        Rect.Top + TopMenuBarMargin + TopOfGraphAnimInfoMargin,
                        (int)MiddleSectionWidth,
                        Rect.Height - TopMenuBarMargin - TopOfGraphAnimInfoMargin);

                    ButtonEditCurrentAnimInfo.Bounds = new System.Drawing.Rectangle(
                        plannedGraphRect.Right - 4 - ButtonEditCurrentAnimInfoWidth,
                        Rect.Top + TopMenuBarMargin - 4,
                        ButtonEditCurrentAnimInfoWidth,
                        TopOfGraphAnimInfoMargin).DpiScaled();

                    ButtonEditCurrentAnimInfo.Bounds = new System.Drawing.Rectangle(
                        plannedGraphRect.Right - 4 - ButtonEditCurrentAnimInfoWidth - 8 - ButtonGotoEventSourceWidth,
                        Rect.Top + TopMenuBarMargin - 4,
                        ButtonGotoEventSourceWidth,
                        TopOfGraphAnimInfoMargin).DpiScaled();
                }

                Transport.Rect = new Rectangle(
                        (int)RightSectionStartX,
                        Rect.Top + TopMenuBarMargin,
                        (int)RightSectionWidth,
                        (int)(TransportHeight));

                ButtonEditCurrentTaeHeader.Bounds = new System.Drawing.Rectangle(
                        (int)LeftSectionStartX,
                        Rect.Bottom - EditTaeHeaderButtonHeight,
                        (int)LeftSectionWidth,
                        EditTaeHeaderButtonHeight).DpiScaled();

                //editScreenGraphInspector.Rect = new Rectangle(Rect.Width - LayoutInspectorWidth, 0, LayoutInspectorWidth, Rect.Height);


                //inspectorWinFormsControl.Bounds = new System.Drawing.Rectangle((int)RightSectionStartX, Rect.Top + TopMenuBarMargin, (int)RightSectionWidth, (int)(Rect.Height - TopMenuBarMargin - BottomRightPaneHeight - DividerVisiblePad));
                //ModelViewerBounds = new Rectangle((int)RightSectionStartX, (int)(Rect.Bottom - BottomRightPaneHeight), (int)RightSectionWidth, (int)(BottomRightPaneHeight));

                //ShaderAdjuster.Size = new System.Drawing.Size((int)RightSectionWidth, ShaderAdjuster.Size.Height);
                ModelViewerBounds = new Rectangle(
                    (int)RightSectionStartX, 
                    Rect.Top + TopMenuBarMargin + TransportHeight, 
                    (int)RightSectionWidth, 
                    (int)(TopRightPaneHeight));
                ModelViewerBounds_InputArea = new Rectangle(
                    ModelViewerBounds.X + (DividerHitboxPad / 2),
                    ModelViewerBounds.Y + (DividerHitboxPad / 2),
                    ModelViewerBounds.Width - DividerHitboxPad,
                    ModelViewerBounds.Height - DividerHitboxPad);
                inspectorWinFormsControl.Bounds = new System.Drawing.Rectangle(
                    (int)RightSectionStartX, 
                    (int)(Rect.Top + TopMenuBarMargin + TopRightPaneHeight + DividerVisiblePad + TransportHeight), 
                    (int)RightSectionWidth, 
                    (int)(Rect.Height - TopRightPaneHeight - DividerVisiblePad - TopMenuBarMargin - TransportHeight)).DpiScaled();
                
                //ShaderAdjuster.Location = new System.Drawing.Point(Rect.Right - ShaderAdjuster.Size.Width, Rect.Top + TopMenuBarMargin);
            }));

           
        }

        public void DrawDimmingRect(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex)
        {
            sb.Begin(transformMatrix: Main.DPIMatrix);
            try
            {
                sb.Draw(boxTex, new Rectangle(Rect.Left, Rect.Top, (int)RightSectionStartX - Rect.X, Rect.Height), Color.Black * 0.25f);
            }
            finally { sb.End(); }
        }

        public void Draw(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex,
            SpriteFont font, float elapsedSeconds, SpriteFont smallFont, Texture2D scrollbarArrowTex)
        {
            sb.Begin();
            try
            {
                sb.Draw(boxTex, new Rectangle(Rect.X, Rect.Y, (int)RightSectionStartX - Rect.X, Rect.Height).DpiScaled(), Main.Colors.MainColorBackground);

                // Draw model viewer background lel
                //sb.Draw(boxTex, ModelViewerBounds, Color.Gray);

            }
            finally { sb.End(); }



            //throw new Exception("TaeUndoMan");

            //throw new Exception("Make left/right edges of events line up to same vertical lines so the rounding doesnt make them 1 pixel off");
            //throw new Exception("Make dragging edges of scrollbar box do zoom");
            //throw new Exception("make ctrl+scroll zoom centered on mouse cursor pos");

            UpdateLayout();

            if (AnimationListScreen != null)
            {
                AnimationListScreen.Draw(gd, sb, boxTex, font, scrollbarArrowTex);

                Rectangle curAnimInfoTextRect = new Rectangle(
                    (int)(MiddleSectionStartX),
                    Rect.Top + TopMenuBarMargin,
                    (int)(MiddleSectionWidth - ButtonEditCurrentAnimInfoWidth),
                    TopOfGraphAnimInfoMargin);

                sb.Begin();
                try
                {
                    if (Config.EnableFancyScrollingStrings)
                    {
                        SelectedTaeAnimInfoScrollingText.Draw(gd, sb, Main.DPIMatrix, curAnimInfoTextRect, font, elapsedSeconds, Main.GlobalTaeEditorFontOffset);
                    }
                    else
                    {
                        var curAnimInfoTextPos = curAnimInfoTextRect.Location.ToVector2();

                        sb.DrawString(font, SelectedTaeAnimInfoScrollingText.Text, curAnimInfoTextPos + Vector2.One + Main.GlobalTaeEditorFontOffset, Color.Black);
                        sb.DrawString(font, SelectedTaeAnimInfoScrollingText.Text, curAnimInfoTextPos + (Vector2.One * 2) + Main.GlobalTaeEditorFontOffset, Color.Black);
                        sb.DrawString(font, SelectedTaeAnimInfoScrollingText.Text, curAnimInfoTextPos + Main.GlobalTaeEditorFontOffset, Color.White);
                    }

                    //sb.DrawString(font, SelectedTaeAnimInfoScrollingText, curAnimInfoTextPos + Vector2.One, Color.Black);
                    //sb.DrawString(font, SelectedTaeAnimInfoScrollingText, curAnimInfoTextPos + (Vector2.One * 2), Color.Black);
                    //sb.DrawString(font, SelectedTaeAnimInfoScrollingText, curAnimInfoTextPos, Color.White);
                }
                finally { sb.End(); }



            }

            if (Graph != null)
            {
                Graph.Draw(gd, sb, boxTex, font, elapsedSeconds, smallFont, scrollbarArrowTex);

                
            }
            else
            {
                // Draws a very, very blank graph is none is loaded:

                //var graphRect = new Rectangle(
                //        (int)MiddleSectionStartX,
                //        Rect.Top + TopMenuBarMargin + TopOfGraphAnimInfoMargin,
                //        (int)MiddleSectionWidth,
                //        Rect.Height - TopMenuBarMargin - TopOfGraphAnimInfoMargin);

                //sb.Begin();
                //sb.Draw(texture: boxTex,
                //    position: new Vector2(graphRect.X, graphRect.Y),
                //    sourceRectangle: null,
                //    color: new Color(120, 120, 120, 255),
                //    rotation: 0,
                //    origin: Vector2.Zero,
                //    scale: new Vector2(graphRect.Width, graphRect.Height),
                //    effects: SpriteEffects.None,
                //    layerDepth: 0
                //    );
                
                //sb.Draw(texture: boxTex,
                //    position: new Vector2(graphRect.X, graphRect.Y),
                //    sourceRectangle: null,
                //    color: new Color(64, 64, 64, 255),
                //    rotation: 0,
                //    origin: Vector2.Zero,
                //    scale: new Vector2(graphRect.Width, TaeEditAnimEventGraph.TimeLineHeight),
                //    effects: SpriteEffects.None,
                //    layerDepth: 0
                //    );
                //sb.End();
            }

            Transport.Draw(gd, sb, boxTex, smallFont);

            if (AnimSwitchRenderCooldown > 0)
            {
                AnimSwitchRenderCooldown -= Main.DELTA_UPDATE;

                //float ratio = Math.Max(0, Math.Min(1, MathHelper.Lerp(0, 1, AnimSwitchRenderCooldown / AnimSwitchRenderCooldownFadeLength)));
                //sb.Begin();
                //sb.Draw(boxTex, graphRect, AnimSwitchRenderCooldownColor * ratio);
                //sb.End();
            }

            //editScreenGraphInspector.Draw(gd, sb, boxTex, font);

            //var oldViewport = gd.Viewport;
            //gd.Viewport = new Viewport(Rect.X, Rect.Y, Rect.Width, TopMargin);
            //{
            //    sb.Begin();

            //    sb.DrawString(font, $"{TaeFileName}", new Vector2(4, 4) + Vector2.One, Color.Black);
            //    sb.DrawString(font, $"{TaeFileName}", new Vector2(4, 4), Color.White);

            //    sb.End();
            //}
            //gd.Viewport = oldViewport;
        }
    }
}

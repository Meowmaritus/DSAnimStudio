using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public static partial class MenuBar
    {
        public static void BuildMenuBar_050_Tools(ref bool anyItemFocused, ref bool isAnyMenuExpanded)
        {
            if (ImGui.BeginMenu("Tools"))
            {
                isAnyMenuExpanded = true;

                DoWindow(OSD.WindowComboViewer, "F8");


                if (Main.IsDebugBuild)
                {
                    ImGui.Separator();
                    
                    if (ClickItem("Scan for Unused Animations", shortcut: "[DEBUG]",
                            textColor: Color.Red, shortcutColor: Color.Red))
                        Main.MainThreadLazyDispatch(() =>
                        {
                            Tae.Tools_ScanForUnusedAnimations();
                        });
                    
                    // if (ClickItem("[DEBUG] Manage Anim Sections", shortcut: "[DEBUG]",
                    //         textColor: Color.Red, shortcutColor: Color.Red))
                    //     Main.MainThreadLazyDispatch(() =>
                    //     {
                    //         Tae.ShowManageTaeSectionsDialog();
                    //     });
                    //
                    // if (ClickItem("[DEBUG] Manage Animations", shortcut: "[DEBUG]",
                    //         textColor: Color.Red, shortcutColor: Color.Red))
                    //     Main.MainThreadLazyDispatch(() =>
                    //     {
                    //         Tae.ShowManageAnimationsDialog();
                    //     });
                    
                    if (ClickItem("[DEBUG] Import FBX Animation", shortcut: "[DEBUG]",
                            textColor: Color.Red, shortcutColor: Color.Red))
                        Main.MainThreadLazyDispatch(() =>
                        {
                            Tae.BringUpImporter_FBXAnim();
                        });
                }

                ImGui.Separator();

                if (ClickItem("Export Skeleton & Animations...", enabled: Tae?.IsFileOpen == true && !zzz_DocumentManager.CurrentDocument.LoadingTaskMan.AnyInteractionBlockingTasks()))
                    Main.MainThreadLazyDispatch(() =>
                    {
                        Tae.ShowExportAllAnimsMenu();
                    });

                ImGui.Separator();

                if (ClickItem("Export All Actions to Text File...", enabled: Tae?.IsFileOpen == true && !zzz_DocumentManager.CurrentDocument.LoadingTaskMan.AnyInteractionBlockingTasks()))
                    Main.MainThreadLazyDispatch(() =>
                    {
                        Tae.ShowExportAllActionsToTextFileDialog();
                    });

                ImGui.Separator();

                if (ClickItem("Export All Animation Names...", enabled: Tae?.IsFileOpen == true && !zzz_DocumentManager.CurrentDocument.LoadingTaskMan.AnyInteractionBlockingTasks()))
                    Main.MainThreadLazyDispatch(() =>
                    {
                        Tae.ShowExportAllAnimNamesDialog();
                    });

                if (ClickItem("Import All Animation Names...", enabled: Tae?.IsFileOpen == true && !zzz_DocumentManager.CurrentDocument.LoadingTaskMan.AnyInteractionBlockingTasks()))
                    Main.MainThreadLazyDispatch(() =>
                    {
                        Tae.ShowImportAllAnimNamesDialog();
                    });
                
                ImGui.Separator();

                Main.Config.EnableGameDataIOLogging = Checkbox(
                    "Enable GameData Logging",
                    Main.Config.EnableGameDataIOLogging);
                
                ImGui.EndMenu();
            }
            
            
            
            
        }
    }
}

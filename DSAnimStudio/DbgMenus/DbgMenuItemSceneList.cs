using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DbgMenus
{
    public class DbgMenuItemSceneList : DbgMenuItem
    {
        public readonly bool IsModelGroupingKind;

        private Dictionary<DbgMenuItem, ModelInstance> MapMenuEntriesToModels
            = new Dictionary<DbgMenuItem, ModelInstance>();

        public DbgMenuItemSceneList(bool isModelGroupingKind)
        {
            IsModelGroupingKind = isModelGroupingKind;
            BuildSceneItems();
        }

        private List<DbgMenuItem> baseMenuItems = new List<DbgMenuItem>
        {
            new DbgMenuItem()
            {
                Text = "[Click to Delete All]",
                ClickAction = (m) => GFX.ModelDrawer.ClearScene()
            },
            new DbgMenuItem()
            {
                Text = "[Click to Hide All]",
                ClickAction = (m) => GFX.ModelDrawer.HideAll()
            },
            new DbgMenuItem()
            {
                Text = "[Click to Show All]",
                ClickAction = (m) => GFX.ModelDrawer.ShowAll()
            },
            new DbgMenuItem()
            {
                Text = "[Click to Toggle All]",
                ClickAction = (m) => GFX.ModelDrawer.InvertVisibility()
            }
        };

        private static void SetModelPrefixVisibility(string prefix, bool visibility)
        {
            throw new NotImplementedException();
            //lock (ModelDrawer._lock_ModelLoad_Draw)
            //{
            //    var modelsWithPrefix = GFX.ModelDrawer.ModelInstanceList.Where(x => x.Name.StartsWith(prefix));
            //    if (modelsWithPrefix.Any())
            //    {
            //        foreach (var m in modelsWithPrefix)
            //        {
            //            m.IsVisible = visibility;
            //        }
            //    }
            //}

        }
        private static bool GetModelPrefixVisibility(string prefix)
        {
            throw new NotImplementedException();
            //lock (ModelDrawer._lock_ModelLoad_Draw)
            //{
            //    var modelsWithPrefix = GFX.ModelDrawer.ModelInstanceList.Where(x => x.Name.StartsWith(prefix));
            //    if (modelsWithPrefix.Any())
            //        return modelsWithPrefix.First().IsVisible;
            //}

            //return false;
        }

        private void BuildSceneItems()
        {
            lock (ModelDrawer._lock_ModelLoad_Draw)
            {
                Items.Clear();
                foreach (var it in baseMenuItems)
                {
                    Items.Add(it);
                }

                if (IsModelGroupingKind)
                {
                    Dictionary<string, int> modelNamePrefixes = new Dictionary<string, int>();

                    //foreach (var md in GFX.ModelDrawer.ModelInstanceList)
                    //{
                    //    var underscoreIndex = md.Name.IndexOf('_');
                    //    string modelNamePrefix = null;
                    //    if (underscoreIndex >= 0)
                    //        modelNamePrefix = md.Name.Substring(0, underscoreIndex);
                    //    else
                    //        modelNamePrefix = md.Name;

                    //    if (!modelNamePrefixes.ContainsKey(modelNamePrefix))
                    //        modelNamePrefixes.Add(modelNamePrefix, 1);
                    //    else
                    //        modelNamePrefixes[modelNamePrefix] = modelNamePrefixes[modelNamePrefix] + 1;
                    //}

                    foreach (var prefix in modelNamePrefixes)
                    {
                        Items.Add(new DbgMenuItemBool($"{prefix.Key} (x{prefix.Value.ToString()})", "SHOW", "HIDE",
                            (b) => SetModelPrefixVisibility(prefix.Key, b), () => GetModelPrefixVisibility(prefix.Key)));
                    }
                }
                else
                {
                    MapMenuEntriesToModels.Clear();

                    //foreach (var md in GFX.ModelDrawer.ModelInstanceList)
                    //{
                    //    var menuItem = new DbgMenuItemBool($"{md.Name}", "SHOW", "HIDE",
                    //        (b) => md.IsVisible = b, () => md.IsVisible);

                    //    Items.Add(menuItem);
                    //    MapMenuEntriesToModels.Add(menuItem, md);
                    //}
                }

                
            }

        }

        public override void UpdateUI()
        {
            if (CurrentMenu == this)
            {
                //// If the amount of models changes, just rebuild the whole thing. 
                //if (Items.Count != (GFX.ModelDrawer.ModelInstanceList.Count + baseMenuItems.Count))
                //{
                //    BuildSceneItems();
                //}
                
                if (!IsModelGroupingKind)
                {
                    if (SelectedIndex >= baseMenuItems.Count && SelectedIndex < Items.Count)
                    {
                        GFX.ModelDrawer.Selected = MapMenuEntriesToModels[Items[SelectedIndex]];
                    }
                    else
                    {
                        GFX.ModelDrawer.Selected = null;
                    }
                }
            }

            base.UpdateUI();
        }

        public override void OnRequestTextRefresh()
        {
            UpdateUI();
        }
    }
}

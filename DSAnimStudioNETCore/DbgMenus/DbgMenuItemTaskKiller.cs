using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DbgMenus
{
    public class DbgMenuItemTaskKiller : DbgMenuItem
    {
        private Dictionary<DbgMenuItem, string> MapMenuEntriesToTaskKeys
            = new Dictionary<DbgMenuItem, string>();

        public DbgMenuItemTaskKiller()
        {
            BuildSceneItems();
        }

        private List<DbgMenuItem> baseMenuItems = new List<DbgMenuItem>
        {
            new DbgMenuItem()
            {
                Text = "Cyan = Task is running.",
                CustomColorFunction = () => Color.Cyan
            },
            new DbgMenuItem()
            {
                Text = "Red = Task is marked to be killed when it gets to a stopping point.",
                CustomColorFunction = () => Color.Red
            },
            new DbgMenuItem()
            {
                Text = @"<-- CLICK TASK BELOW TO KILL -->",
            },
        };

        private void BuildSceneItems()
        {
            lock (LoadingTaskMan._lock_TaskDictEdit)
            {
                MapMenuEntriesToTaskKeys.Clear();
                Items.Clear();

                foreach (var it in baseMenuItems)
                {
                    Items.Add(it);
                }

                foreach (var kvp in LoadingTaskMan.TaskDict)
                {
                    //var menuItem = new DbgMenuItem()
                    //{
                    //    ClickAction = (m) => LoadingTaskMan.KillTask(kvp.Key),
                    //    RefreshTextFunction = () => $"{kvp.Key} [{kvp.Value.ProgressRatio:0.00}] [\"{kvp.Value.DisplayString}\"]",
                    //    CustomColorFunction = () => kvp.Value.IsBeingKilledManually ? Color.Red : Color.Cyan
                    //};

                    //Items.Add(menuItem);
                    //MapMenuEntriesToTaskKeys.Add(menuItem, kvp.Key);
                }

                RequestTextRefresh();
            }
        }

        public override void UpdateUI()
        {
            if (CurrentMenu == this)
            {
                // If the amount of models changes, just rebuild the whole thing. 
                if (Items.Count != (LoadingTaskMan.TaskDict.Count + baseMenuItems.Count))
                {
                    BuildSceneItems();
                }

                RequestTextRefresh();
            }

            base.UpdateUI();
        }
    }
}

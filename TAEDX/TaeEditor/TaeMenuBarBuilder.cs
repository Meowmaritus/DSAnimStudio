using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TAEDX.TaeEditor
{
    public class TaeMenuBarBuilder
    {
        private Dictionary<string, ToolStripMenuItem> items = new Dictionary<string, ToolStripMenuItem>();

        private MenuStrip Menustrip;
        public TaeMenuBarBuilder(MenuStrip toolStrip)
        {
            Menustrip = toolStrip;
        }

        public ToolStripMenuItem this[string path]
        {
            get
            {
                if (items.ContainsKey(path))
                    return items[path];
                else
                    return null;
            }
        }

        void SetColorOfItem(ToolStripMenuItem item)
        {
            //item.BackColor = Menustrip.BackColor;
            //item.ForeColor = Menustrip.ForeColor;

            foreach (ToolStripMenuItem subItem in item.DropDownItems.OfType<ToolStripMenuItem>())
            {
                SetColorOfItem(subItem);
            }
        }


        public void SetColorsOfAll(System.Drawing.Color backColor, System.Drawing.Color foreColor)
        {
            //Menustrip.BackColor = backColor;
            //Menustrip.ForeColor = foreColor;

            //ToolStripManager.VisualStylesEnabled = true;
            //ToolStripManager.Renderer = new ToolStripProfessionalRenderer(new CustomProfessionalColors());

            foreach (ToolStripMenuItem topItem in Menustrip.Items)
            {
                SetColorOfItem(topItem);
            }
        }

        private ToolStripMenuItem FindOrCreateItem(string path, ToolStripMenuItem parent, string itemName)
        {
            foreach (ToolStripMenuItem c in parent.DropDownItems)
            {
                if (c.Text == itemName)
                    return c;
            }

            var newItem = new ToolStripMenuItem(itemName);

            SetColorOfItem(newItem);

            parent.DropDownItems.Add(newItem);

            if (!items.ContainsKey(path))
                items.Add(path, newItem);

            return newItem;
        }

        private ToolStripMenuItem FindOrCreateItem(string path, ToolStrip parent, string itemName)
        {
            foreach (ToolStripMenuItem c in parent.Items)
            {
                if (c.Text == itemName)
                    return c;
            }

            var newItem = new ToolStripMenuItem(itemName);

            SetColorOfItem(newItem);

            parent.Items.Add(newItem);

            if (!items.ContainsKey(path))
                items.Add(path, newItem);

            return newItem;
        }

        public void AddItem(string path, string itemName, Dictionary<string, Action> choicesAndChooseAction, string defaultChoice)
        {
            string[] pathStops = path.Split('/');

            string currentPath = pathStops[0];

            ToolStripMenuItem baseItem = FindOrCreateItem(currentPath, Menustrip, pathStops[0]);

            for (int i = 1; i < pathStops.Length; i++)
            {
                currentPath += "/" + pathStops[i];
                baseItem = FindOrCreateItem(currentPath, baseItem, pathStops[i]);
            }

            string shortcutText = null;

            if (itemName.Contains("|"))
            {
                var split = itemName.Split('|');
                shortcutText = split[1];
                itemName = split[0];
            }

            var newItem = new ToolStripMenuItem(itemName);

            if (shortcutText != null)
                newItem.ShortcutKeyDisplayString = shortcutText;

            foreach (var kvp in choicesAndChooseAction)
            {
                var newSubItem = new ToolStripMenuItem(kvp.Key);
                if (kvp.Key == defaultChoice)
                    newSubItem.Checked = true;
                newItem.DropDownItems.Add(newSubItem);
            }

            foreach (ToolStripMenuItem newSubItem in newItem.DropDownItems)
            {
                newSubItem.Click += (o, e) =>
                {
                    foreach (ToolStripMenuItem h in newItem.DropDownItems)
                    {
                        h.Checked = false;
                    }
                    newSubItem.Checked = true;
                    choicesAndChooseAction[newSubItem.Text].Invoke();
                };
            }

            currentPath += "/" + itemName;
            if (!items.ContainsKey(currentPath))
                items.Add(currentPath, newItem);

            SetColorOfItem(newItem);

            baseItem.DropDownItems.Add(newItem);
        }

        public void AddItem(string path, string itemName, Func<bool> checkState, Action<bool> onCheckChange)
        {
            string[] pathStops = path.Split('/');



            string currentPath = pathStops[0];

            ToolStripMenuItem baseItem = FindOrCreateItem(currentPath, Menustrip, pathStops[0]);

            for (int i = 1; i < pathStops.Length; i++)
            {
                currentPath += "/" + pathStops[i];
                baseItem = FindOrCreateItem(currentPath, baseItem, pathStops[i]);
            }

            string shortcutText = null;

            if (itemName.Contains("|"))
            {
                var split = itemName.Split('|');
                shortcutText = split[1];
                itemName = split[0];
            }

            var newItem = new ToolStripMenuItem(itemName);

            if (shortcutText != null)
                newItem.ShortcutKeyDisplayString = shortcutText;

            //newItem.CheckOnClick = true;

            baseItem.DropDownOpening += (o, e) =>
            {
                newItem.Checked = checkState.Invoke();
            };

            newItem.Click += (o, e) =>
            {
                newItem.Checked = !newItem.Checked;
                onCheckChange(newItem.Checked);
            };

            currentPath += "/" + itemName;
            if (!items.ContainsKey(currentPath))
                items.Add(currentPath, newItem);

            SetColorOfItem(newItem);

            baseItem.DropDownItems.Add(newItem);
        }

        public void AddItem(string path, string itemName, Action click = null, bool startDisabled = false)
        {
            string[] pathStops = path.Split('/');

            

            string currentPath = pathStops[0];

            ToolStripMenuItem baseItem = FindOrCreateItem(currentPath, Menustrip, pathStops[0]);

            for (int i = 1; i < pathStops.Length; i++)
            {
                currentPath += "/" + pathStops[i];
                baseItem = FindOrCreateItem(currentPath, baseItem, pathStops[i]);
            }

            string shortcutText = null;

            if (itemName.Contains("|"))
            {
                var split = itemName.Split('|');
                shortcutText = split[1];
                itemName = split[0];
            }

            var newItem = new ToolStripMenuItem(itemName);

            if (shortcutText != null)
                newItem.ShortcutKeyDisplayString = shortcutText;

            if (click != null)
                newItem.Click += (o, e) => click();

            if (startDisabled)
                newItem.Enabled = false;

            currentPath += "/" + itemName;
            if (!items.ContainsKey(currentPath))
                items.Add(currentPath, newItem);

            SetColorOfItem(newItem);

            baseItem.DropDownItems.Add(newItem);
        }

        public void AddItem(string path, ToolStripMenuItem item)
        {
            string[] pathStops = path.Split('/');

            string currentPath = pathStops[0];

            ToolStripMenuItem baseItem = FindOrCreateItem(currentPath, Menustrip, pathStops[0]);

            for (int i = 1; i < pathStops.Length; i++)
            {
                currentPath += "/" + pathStops[i];
                baseItem = FindOrCreateItem(currentPath, baseItem, pathStops[i]);
            }

            currentPath += "/" + item.Text;
            if (!items.ContainsKey(currentPath))
                items.Add(currentPath, item);

            SetColorOfItem(item);

            baseItem.DropDownItems.Add(item);
        }

        public void AddSeparator(string path)
        {
            string[] pathStops = path.Split('/');

            string currentPath = pathStops[0];

            ToolStripMenuItem baseItem = FindOrCreateItem(currentPath, Menustrip, pathStops[0]);

            for (int i = 1; i < pathStops.Length; i++)
            {
                currentPath += "/" + pathStops[i];
                baseItem = FindOrCreateItem(currentPath, baseItem, pathStops[i]);
            }

            baseItem.DropDownItems.Add(new ToolStripSeparator()
            {
                BackColor = Menustrip.BackColor,
                ForeColor = Menustrip.ForeColor,
            });
        }

        public void ClearItem(string fullPath)
        {
            if (items.ContainsKey(fullPath))
                items[fullPath].DropDownItems.Clear();
        }

    }
}

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSAnimStudio.TaeEditor
{
    public class TaeMenuBarBuilder
    {
        private Dictionary<string, ToolStripMenuItem> _items = new Dictionary<string, ToolStripMenuItem>();

        private MenuStrip Menustrip;

        static System.Drawing.Color BackColor;
        static System.Drawing.Color BackColor_Selected = System.Drawing.Color.FromArgb(255, 40, 40, 40);
        static System.Drawing.Color OutlineColor = System.Drawing.Color.FromArgb(255, 80, 80, 80);
        static System.Drawing.Color ForeColor;

        static int BorderThickness = 2;

        static System.Drawing.Bitmap CustomCheck = null;

        private static object _lock_blockInput = new object();
        private bool _blockInput;
        public bool BlockInput
        {
            get
            {
                lock (_lock_blockInput)
                    return _blockInput;
            }
            set
            {
                lock (_lock_blockInput)
                    _blockInput = value;
            }
        }

        public void SetupEvents(ToolStripMenuItem parent, ToolStripMenuItem child)
        {
            child.MouseEnter += (o, e) =>
            {
                parent.DropDown.AutoClose = false;
                BlockInput = true;
            };
            child.MouseLeave += (o, e) =>
            {
                parent.DropDown.AutoClose = true;
                BlockInput = false;
            };
        }

        public TaeMenuBarBuilder(MenuStrip toolStrip)
        {
            Menustrip = toolStrip;

            BackColor = Menustrip.BackColor;
            ForeColor = Menustrip.ForeColor;

            Menustrip.Renderer = new CoolDarkToolStripRenderer();
        }

        public ToolStripMenuItem this[string path]
        {
            get
            {
                if (_items.ContainsKey(path))
                    return _items[path];
                else
                    return null;
            }
            set
            {
                if (_items.ContainsKey(path))
                {
                    if (value != null)
                    {
                        _items[path] = value;
                    }
                    else
                    {
                        _items.Remove(path);
                    }
                }
                else
                {
                    if (value != null)
                    {
                        _items.Add(path, value);
                    }
                }
            }
        }

        void SetColorOfItem(ToolStripMenuItem item)
        {
            item.BackColor = Menustrip.BackColor;
            item.ForeColor = Menustrip.ForeColor;

            foreach (ToolStripMenuItem subItem in item.DropDownItems.OfType<ToolStripMenuItem>())
            {
                SetColorOfItem(subItem);
            }
        }


        public void SetColorsOfAll(System.Drawing.Color backColor, System.Drawing.Color foreColor)
        {
            Menustrip.BackColor = backColor;
            Menustrip.ForeColor = foreColor;

            //ToolStripManager.VisualStylesEnabled = true;
            //ToolStripManager.Renderer = new ToolStripProfessionalRenderer(new CustomProfessionalColors());

            foreach (ToolStripMenuItem topItem in Menustrip.Items)
            {
                SetColorOfItem(topItem);
            }
        }

        private ToolStripMenuItem FindOrCreateItem(string path, ToolStripMenuItem parent, string itemName)
        {
            foreach (var c in parent.DropDownItems)
            {
                if (c is ToolStripMenuItem item)
                {
                    if (item.Text == itemName)
                        return item;
                }
                
            }

            var newItem = new ToolStripMenuItem(itemName);

            //newItem.DropDown.AutoClose = false;

            SetupEvents(parent, newItem);

            SetColorOfItem(newItem);

            parent.DropDownItems.Add(newItem);

            this[path] = newItem;

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

            //newItem.DropDown.AutoClose = false;

            

            SetColorOfItem(newItem);

            parent.Items.Add(newItem);

            this[path] = newItem;

            return newItem;
        }

        public void AddItem(string path, string itemName, Dictionary<string, Action> choicesAndChooseAction, string defaultChoice)
        {
            string[] pathStops = path.Split('\\');

            string currentPath = pathStops[0];

            ToolStripMenuItem baseItem = FindOrCreateItem(currentPath, Menustrip, pathStops[0]);

            for (int i = 1; i < pathStops.Length; i++)
            {
                currentPath += "\\" + pathStops[i];
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

                SetupEvents(newItem, newSubItem);

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

            currentPath += "\\" + itemName;

            this[currentPath] = newItem;

            SetColorOfItem(newItem);

            SetupEvents(baseItem, newItem);

            baseItem.DropDownItems.Add(newItem);
        }

        public void AddItem(string path, string itemName, Func<Dictionary<string, Action>> getChoicesAndChooseAction, Func<string> getWhichIsSelected)
        {
            string[] pathStops = path.Split('\\');

            string currentPath = pathStops[0];

            ToolStripMenuItem baseItem = FindOrCreateItem(currentPath, Menustrip, pathStops[0]);

            for (int i = 1; i < pathStops.Length; i++)
            {
                currentPath += "\\" + pathStops[i];
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

            void createDropdownItems()
            {
                newItem.DropDownItems.Clear();

                var choicesAndChooseAction = getChoicesAndChooseAction.Invoke();

                var selected = getWhichIsSelected.Invoke();

                foreach (var kvp in choicesAndChooseAction)
                {
                    var newSubItem = new ToolStripMenuItem(kvp.Key);
                    if (kvp.Key == selected)
                        newSubItem.Checked = true;

                    SetupEvents(newItem, newSubItem);

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
            }

            newItem.MouseEnter += (x, y) =>
            {
                createDropdownItems();
            };

            // For init so it has the > arrow on it and stuff
            createDropdownItems();

            currentPath += "\\" + itemName;

            this[currentPath] = newItem;

            SetColorOfItem(newItem);

            SetupEvents(baseItem, newItem);

            baseItem.DropDownItems.Add(newItem);
        }

        private string GetItemNameWithShortcut(ToolStripMenuItem item)
        {
            if (item.ShortcutKeyDisplayString != null)
                return $"{item.Text}|{item.ShortcutKeyDisplayString}";
            else
                return item.Text;
        }

        public void AddItem(string path, string itemName, Dictionary<string, Action> choicesAndChooseAction, Func<string> getWhichIsSelected)
        {
            string[] pathStops = path.Split('\\');

            string currentPath = pathStops[0];

            ToolStripMenuItem baseItem = FindOrCreateItem(currentPath, Menustrip, pathStops[0]);

            for (int i = 1; i < pathStops.Length; i++)
            {
                currentPath += "\\" + pathStops[i];
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
                if (kvp.Key.StartsWith("SEPARATOR:"))
                {
                    newItem.DropDownItems.Add(new ToolStripSeparator());
                }
                else
                {
                    string newSubItemName = kvp.Key;
                    string newSubItemShortcutText = null;

                    if (newSubItemName.Contains("|"))
                    {
                        var split = newSubItemName.Split('|');
                        newSubItemShortcutText = split[1];
                        newSubItemName = split[0];
                    }

                    var newSubItem = new ToolStripMenuItem(newSubItemName);

                    if (newSubItemShortcutText != null)
                        newSubItem.ShortcutKeyDisplayString = newSubItemShortcutText;

                    SetupEvents(newItem, newSubItem);

                    newItem.DropDownItems.Add(newSubItem);
                }
            }

            newItem.MouseEnter += (o, e) =>
            {
                string selected = getWhichIsSelected.Invoke();

                foreach (var item in newItem.DropDownItems)
                {
                    if (item is ToolStripMenuItem h)
                    {
                        h.Checked = GetItemNameWithShortcut(h).Equals(selected);
                    }
                }
            };

            foreach (var item in newItem.DropDownItems)
            {
                if (item is ToolStripMenuItem newSubItem)
                {
                    newSubItem.Click += (o, e) =>
                    {
                        foreach (var it in newItem.DropDownItems)
                        {
                            if (it is ToolStripMenuItem h)
                            {
                                h.Checked = false;
                            }
                        }
                        newSubItem.Checked = true;
                        var fullName = GetItemNameWithShortcut(newSubItem);
                        choicesAndChooseAction[fullName].Invoke();
                    };
                }
            }

            currentPath += "\\" + itemName;

            this[currentPath] = newItem;

            SetColorOfItem(newItem);

            SetupEvents(baseItem, newItem);

            baseItem.DropDownItems.Add(newItem);
        }

        public void AddItem(string path, string itemName, Func<bool> checkState, 
            Action<bool> onCheckChange, Func<bool> getEnabled = null, Func<object> getMemeTag = null)
        {
            string[] pathStops = path.Split('\\');



            string currentPath = pathStops[0];

            ToolStripMenuItem baseItem = FindOrCreateItem(currentPath, Menustrip, pathStops[0]);

            for (int i = 1; i < pathStops.Length; i++)
            {
                currentPath += "\\" + pathStops[i];
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

            baseItem.MouseEnter += (o, e) =>
            {
                newItem.Checked = checkState.Invoke();
                newItem.Enabled = getEnabled?.Invoke() ?? true;
                newItem.Tag = getMemeTag?.Invoke();
            };

            newItem.Click += (o, e) =>
            {
                newItem.Checked = !newItem.Checked;
                onCheckChange(newItem.Checked);
            };

            currentPath += "\\" + itemName;

            this[currentPath] = newItem;

            SetColorOfItem(newItem);

            SetupEvents(baseItem, newItem);

            baseItem.DropDownItems.Add(newItem);
        }

        public void AddItem(string path, string itemName, Action<TaeMenuBarBuilder> addEntries, bool startDisabled = false)
        {
            string[] pathStops = path.Split('\\');

            string currentPath = pathStops[0];

            ToolStripMenuItem baseItem = FindOrCreateItem(currentPath, Menustrip, pathStops[0]);

            for (int i = 1; i < pathStops.Length; i++)
            {
                currentPath += "\\" + pathStops[i];
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

            if (startDisabled)
                newItem.Enabled = false;

            string basePath = currentPath;

            currentPath += "\\" + itemName;

            this[currentPath] = newItem;

            SetColorOfItem(newItem);

            baseItem.MouseEnter += (o, e) =>
            {
                ClearItem(currentPath);
                //newItem.DropDownItems.Clear();
                addEntries.Invoke(this);
            };

            SetupEvents(baseItem, newItem);

            baseItem.DropDownItems.Add(newItem);
        }

        public void AddItem(string path, string itemName, Action click = null, bool startDisabled = false, bool closeOnClick = true, Func<bool> getEnabled = null)
        {
            string[] pathStops = path.Split('\\');



            string currentPath = pathStops[0];

            ToolStripMenuItem baseItem = FindOrCreateItem(currentPath, Menustrip, pathStops[0]);

            for (int i = 1; i < pathStops.Length; i++)
            {
                currentPath += "\\" + pathStops[i];
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
                newItem.Click += (o, e) =>
                {
                    if (closeOnClick)
                        CloseAll();
                    click();
                };

            if (startDisabled)
                newItem.Enabled = false;

            baseItem.MouseEnter += (o, e) =>
            {
                if (getEnabled != null)
                {
                    newItem.Enabled = getEnabled.Invoke();
                }
            };

            currentPath += "\\" + itemName;

            this[currentPath] = newItem;

            SetColorOfItem(newItem);

            SetupEvents(baseItem, newItem);

            baseItem.DropDownItems.Add(newItem);
        }

        public void AddTopItem(string itemName, Action click = null, bool startDisabled = false)
        {
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

            this[itemName] = newItem;

            SetColorOfItem(newItem);

            Menustrip.Items.Add(newItem);
        }

        public void AddItem(string path, ToolStripMenuItem item)
        {
            string[] pathStops = path.Split('\\');

            string currentPath = pathStops[0];

            ToolStripMenuItem baseItem = FindOrCreateItem(currentPath, Menustrip, pathStops[0]);

            for (int i = 1; i < pathStops.Length; i++)
            {
                currentPath += "\\" + pathStops[i];
                baseItem = FindOrCreateItem(currentPath, baseItem, pathStops[i]);
            }

            currentPath += "\\" + item.Text;

            this[currentPath] = item;

            SetColorOfItem(item);

            SetupEvents(baseItem, item);

            baseItem.DropDownItems.Add(item);
        }

        public void AddSeparator(string path)
        {
            string[] pathStops = path.Split('\\');

            string currentPath = pathStops[0];

            ToolStripMenuItem baseItem = FindOrCreateItem(currentPath, Menustrip, pathStops[0]);

            for (int i = 1; i < pathStops.Length; i++)
            {
                currentPath += "\\" + pathStops[i];
                baseItem = FindOrCreateItem(currentPath, baseItem, pathStops[i]);
            }

            var sep = new ToolStripSeparator()
            {
                BackColor = Menustrip.BackColor,
                ForeColor = Menustrip.ForeColor,
            };

            baseItem.DropDownItems.Add(sep);

            sep.MouseEnter += (o, e) => baseItem.DropDown.AutoClose = false;
            sep.MouseLeave += (o, e) => baseItem.DropDown.AutoClose = true;
        }

        public void ClearItem(string fullPath)
        {
            if (this[fullPath] != null)
            {
                foreach (var ddi in this[fullPath].DropDownItems)
                {
                    if (ddi is ToolStripMenuItem tsmi)
                    {
                        this[fullPath + "\\" + tsmi.Text] = null;
                    }
                }
                this[fullPath].DropDownItems.Clear();
            }
                
        }

        public void CompletelyDestroyItem(string dirPath, string itemName)
        {

            if (this[dirPath + "\\" + itemName] != null)
            {
                ClearItem(dirPath + "\\" + itemName);
                if (this[dirPath] != null)
                {
                    List<ToolStripMenuItem> itemsToDestroy = new List<ToolStripMenuItem>();
                    foreach (var item in this[dirPath].DropDownItems)
                    {
                        if (item is ToolStripMenuItem asToolStripMenuItem)
                        {
                            if (asToolStripMenuItem.Text == itemName)
                                itemsToDestroy.Add(asToolStripMenuItem);
                        }
                    }
                    foreach (var item in itemsToDestroy)
                    {
                        this[dirPath].DropDownItems.Remove(item);
                    }
                }
            }
        }

        public void CloseAll()
        {
            foreach (var thing in Menustrip.Items.OfType<ToolStripMenuItem>())
            {
                thing.DropDown.Close();
            }
        }

        class CoolDarkToolStripRenderer : ToolStripSystemRenderer
        {
            protected override void InitializeItem(ToolStripItem item)
            {
                base.InitializeItem(item);
            }

            protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
            {
                using (var brush = new System.Drawing.SolidBrush(BackColor))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, e.Item.Bounds.Width, e.Item.Bounds.Height);
                }

                using (var brush = new System.Drawing.SolidBrush(BackColor_Selected))
                {
                    e.Graphics.FillRectangle(brush, 0, (e.Item.Bounds.Height / 2.0f) - 1, e.Item.Bounds.Width, 2);
                }
            }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                if (e.Item.Selected)
                {
                    using (var brush = new System.Drawing.SolidBrush(BackColor_Selected))
                    {
                        e.Graphics.FillRectangle(brush, -2, -2, e.Item.Bounds.Width + 4, e.Item.Bounds.Height + 4);
                    }
                }
                else
                {
                    using (var brush = new System.Drawing.SolidBrush(BackColor))
                    {
                        e.Graphics.FillRectangle(brush, -2, -2, e.Item.Bounds.Width + 4, e.Item.Bounds.Height + 4);
                    }
                }
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                // IM SO SORRY
                if (e.Item.Text == "NPC Settings" || e.Item.Text == "Player Settings" ||
                    e.Item.Text == "Object Settings" || e.Item.Text == "Animated Equipment Settings" ||
                    e.Item.Text == "Cutscene Settings")
                {
                    e.Item.ForeColor = System.Drawing.Color.Cyan;
                    base.OnRenderItemText(e);
                    return;
                }

                

                if (e.Item.Tag is bool asBool && asBool)
                {
                    e.Item.ForeColor = System.Drawing.Color.Gray;
                }
                else if (e.Item.Tag is System.Drawing.Color asColor)
                {
                    e.Item.ForeColor = asColor;
                }
                else
                {
                    if (!e.Item.Enabled)
                    {
                        //using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.Gray))
                        //    e.Graphics.DrawString(e.Item.Text, e.Item.Font, brush, e.Item.Padding.Size.Width, e.Item.Padding.Top);

                        if (e.Item.Selected)
                        {
                            e.Item.ForeColor = System.Drawing.Color.Black;
                        }
                        else
                        {
                            e.Item.ForeColor = System.Drawing.Color.Gray;
                        }
                    }
                    else
                    {
                        e.Item.ForeColor = ForeColor;

                        //using (var brush = new System.Drawing.SolidBrush(ForeColor))
                        //    e.Graphics.DrawString(e.Item.Text, e.Item.Font, brush, e.Item.Padding.Size.Width, e.Item.Padding.Top);
                    }
                }

                base.OnRenderItemText(e);
            }

            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                //base.OnRenderToolStripBackground(e);
                using (var brush = new System.Drawing.SolidBrush(BackColor))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, e.ToolStrip.Bounds.Width, e.ToolStrip.Bounds.Height);
                }

                using (var brush = new System.Drawing.SolidBrush(OutlineColor))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, e.ToolStrip.Bounds.Width, BorderThickness);
                    e.Graphics.FillRectangle(brush, 0, 0, BorderThickness, e.ToolStrip.Bounds.Height);
                    e.Graphics.FillRectangle(brush, 0, e.ToolStrip.Bounds.Height - BorderThickness, e.ToolStrip.Bounds.Width, BorderThickness);
                    e.Graphics.FillRectangle(brush, e.ToolStrip.Bounds.Width - BorderThickness, 0, BorderThickness, e.ToolStrip.Bounds.Height);
                }
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                using (var brush = new System.Drawing.SolidBrush(OutlineColor))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, e.ToolStrip.Bounds.Width, BorderThickness);
                    e.Graphics.FillRectangle(brush, 0, 0, BorderThickness, e.ToolStrip.Bounds.Height);
                    e.Graphics.FillRectangle(brush, 0, e.ToolStrip.Bounds.Height - BorderThickness, e.ToolStrip.Bounds.Width, BorderThickness);
                    e.Graphics.FillRectangle(brush, e.ToolStrip.Bounds.Width - BorderThickness, 0, BorderThickness, e.ToolStrip.Bounds.Height);
                }
                //base.OnRenderToolStripBorder(e);
            }

            protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
            {
                if (CustomCheck == null)
                {
                    CustomCheck = new System.Drawing.Bitmap(e.Image);
                    for (int y = 0; (y <= (CustomCheck.Height - 1)); y++)
                    {
                        for (int x = 0; (x <= (CustomCheck.Width - 1)); x++)
                        {
                            var c = CustomCheck.GetPixel(x, y);
                            CustomCheck.SetPixel(x, y, System.Drawing.Color.FromArgb(c.A, 255 - c.R, 255 - c.G, 255 - c.B));
                        }
                    }
                }

                e.Graphics.DrawImage(CustomCheck, e.ImageRectangle);
            }
        }


    }


}

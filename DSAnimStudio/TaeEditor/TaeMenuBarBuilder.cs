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
        private Dictionary<string, ToolStripMenuItem> items = new Dictionary<string, ToolStripMenuItem>();

        private MenuStrip Menustrip;

        static System.Drawing.Color BackColor;
        static System.Drawing.Color BackColor_Selected = System.Drawing.Color.DimGray;
        static System.Drawing.Color OutlineColor = System.Drawing.Color.FromArgb(255, 80, 80, 80);
        static System.Drawing.Color ForeColor;

        static int BorderThickness = 2;

        static System.Drawing.Bitmap CustomCheck = null;

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
                if (items.ContainsKey(path))
                    return items[path];
                else
                    return null;
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
                if (!e.Item.Enabled)
                {
                    //using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.Gray))
                    //    e.Graphics.DrawString(e.Item.Text, e.Item.Font, brush, e.Item.Padding.Size.Width, e.Item.Padding.Top);

                    e.Item.ForeColor = System.Drawing.Color.Gray;
                }
                else
                {
                    e.Item.ForeColor = ForeColor;

                    //using (var brush = new System.Drawing.SolidBrush(ForeColor))
                    //    e.Graphics.DrawString(e.Item.Text, e.Item.Font, brush, e.Item.Padding.Size.Width, e.Item.Padding.Top);
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

        public void AddItem(string path, string itemName, Func<Dictionary<string, Action>> getChoicesAndChooseAction, Func<string> getWhichIsSelected)
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

            newItem.DropDownOpening += (x, y) =>
            {
                createDropdownItems();
            };

            // For init so it has the > arrow on it and stuff
            createDropdownItems();

            currentPath += "/" + itemName;
            if (!items.ContainsKey(currentPath))
                items.Add(currentPath, newItem);

            SetColorOfItem(newItem);

            baseItem.DropDownItems.Add(newItem);
        }

        public void AddItem(string path, string itemName, Dictionary<string, Action> choicesAndChooseAction, Func<string> getWhichIsSelected)
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
                newItem.DropDownItems.Add(newSubItem);
            }

            newItem.DropDownOpening += (o, e) =>
            {
                string selected = getWhichIsSelected.Invoke();
                foreach (ToolStripMenuItem h in newItem.DropDownItems)
                {
                    h.Checked = h.Text == selected;
                }
            };

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

            if (!items.ContainsKey(itemName))
                items.Add(itemName, newItem);

            SetColorOfItem(newItem);

            Menustrip.Items.Add(newItem);
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

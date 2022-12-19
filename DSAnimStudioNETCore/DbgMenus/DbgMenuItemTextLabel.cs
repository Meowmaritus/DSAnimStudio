using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DbgMenus
{
    public class DbgMenuItemTextLabel : DbgMenuItem
    {
        public readonly Func<string> GetText;

        public DbgMenuItemTextLabel(Func<string> getText)
        {
            GetText = getText;
        }

        public override void UpdateUI()
        {
            Text = GetText.Invoke();
            base.UpdateUI();
        }

        public override void OnRequestTextRefresh()
        {
            UpdateUI();
        }
    }
}

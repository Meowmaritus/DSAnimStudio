using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DbgMenus
{
    public class DbgMenuItemBool : DbgMenuItem
    {
        public string OptionName = "?BoolName?";
        public string YesText = "?YesText?";
        public string NoText = "?NoText?";
        public Action<bool> SetValue;
        public Func<bool> GetValue;

        private bool defaultValue;

        public DbgMenuItemBool(string optionName, string yesText, 
            string noText, Action<bool> setValue, Func<bool> getValue)
        {
            OptionName = optionName;
            YesText = yesText;
            NoText = noText;
            SetValue = setValue;
            GetValue = getValue;
            defaultValue = GetValue.Invoke();
            ClickAction = (m) => SetValue.Invoke(!GetValue.Invoke());
        }

        public override void OnResetDefault()
        {
            SetValue.Invoke(defaultValue);
            base.OnResetDefault();
        }

        public override void OnIncrease(bool isRepeat, int incrementAmount)
        {
            if (!isRepeat)
                SetValue.Invoke(!GetValue.Invoke());
        }

        public override void OnDecrease(bool isRepeat, int incrementAmount)
        {
            if (!isRepeat)
                SetValue.Invoke(!GetValue.Invoke());
        }

        public override void UpdateUI()
        {
            Text = $"{OptionName}: <{(GetValue.Invoke() ? YesText : NoText)}>";

            base.UpdateUI();
        }

        public override void OnRequestTextRefresh()
        {
            UpdateUI();
        }
    }
}

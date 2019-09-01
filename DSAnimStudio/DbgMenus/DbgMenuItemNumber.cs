using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DbgMenus
{
    public class DbgMenuItemNumber : DbgMenuItem
    {
        public readonly string Name;
        public readonly float MinValue = 0;
        public readonly float MaxValue = 1;
        public readonly float BaseChangeAmount;
        public readonly Action<float> SetValue;
        public readonly Func<float> GetValue;
        public readonly Func<float, string> GetValueString;
        public readonly float DefaultValue;

        public DbgMenuItemNumber(string name, float min, float max, float change, 
            Action<float> setValue, Func<float> getValue, Func<float, string> getValueString = null)
        {
            Name = name;
            MinValue = min;
            MaxValue = max;
            BaseChangeAmount = change;
            SetValue = setValue;
            GetValue = getValue;
            GetValueString = getValueString ?? new Func<float, string>((f) => f.ToString());

            DefaultValue = GetValue.Invoke();

            UpdateText();
        }

        public void UpdateText()
        {
            Text = $"{Name}: <{(GetValueString.Invoke(GetValue.Invoke()))}>";
        }

        public override void OnIncrease(bool isRepeat, int incrementAmount)
        {
            float prevValue = GetValue.Invoke();
            float newValue = prevValue + incrementAmount * BaseChangeAmount;

            //If upper bound reached
            if (newValue >= MaxValue)
            {
                //If already at end and just tapped button
                if (prevValue == MaxValue && !isRepeat)
                    newValue = MinValue; //Wrap Around
                else
                    newValue = MaxValue; //Stop
            }

            SetValue.Invoke(newValue);

            UpdateText();
        }

        public override void OnDecrease(bool isRepeat, int incrementAmount)
        {
            float prevValue = GetValue.Invoke();
            float newValue = prevValue - incrementAmount * BaseChangeAmount;

            //If lower bound reached
            if (newValue <= MinValue)
            {
                //If already at start and just tapped button
                if (prevValue == MinValue && !isRepeat)
                    newValue = MaxValue; //Wrap Around
                else
                    newValue = MinValue; //Stop
            }

            SetValue.Invoke(newValue);

            UpdateText();
        }

        public override void OnResetDefault()
        {
            SetValue.Invoke(DefaultValue);
            UpdateText();
            base.OnResetDefault();
        }

        public override void OnRequestTextRefresh()
        {
            UpdateText();
        }
    }
}

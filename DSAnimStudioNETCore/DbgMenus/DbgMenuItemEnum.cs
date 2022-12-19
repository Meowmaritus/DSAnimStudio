using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DbgMenus
{
    public class DbgMenuItemEnum<T> : DbgMenuItem
        where T : Enum
    {
        public readonly string Name;
        public readonly Action<T> SetValue;
        public readonly Func<T> GetValue;
        public readonly T DefaultValue;
        public readonly T[] EnumValues;
        public readonly Dictionary<T, string> NameOverrides = null;
        private int currentValueIndex;

        public DbgMenuItemEnum(string name, Action<T> setValue, Func<T> getValue, 
            List<T> enumItemsNotToShow = null, Dictionary<T, string> nameOverrides = null)
        {
            Name = name;
            SetValue = setValue;
            GetValue = getValue;
            DefaultValue = GetValue.Invoke();

            var enumValueList = ((T[])Enum.GetValues(typeof(T))).ToList();

            if (enumItemsNotToShow != null)
            {
                foreach (var badEgg in enumItemsNotToShow)
                {
                    enumValueList.Remove(badEgg);
                }
            }

            EnumValues = enumValueList.ToArray();

            NameOverrides = nameOverrides;

            UpdateText();
        }

        private string GetEnumValueName(T val)
        {
            if (NameOverrides != null && NameOverrides.ContainsKey(val))
                return NameOverrides[val];
            else
                return val.ToString();
        }

        public void UpdateText()
        {
            var currentValue = GetValue.Invoke();
            currentValueIndex = Array.IndexOf(EnumValues, currentValue);

            Text = $"{Name}: <{GetEnumValueName(currentValue)}>";
        }

        public override void OnIncrease(bool isRepeat, int incrementAmount)
        {
            int prevIndex = currentValueIndex;
            currentValueIndex += incrementAmount;

            //If upper bound reached
            if (currentValueIndex >= EnumValues.Length)
            {
                //If already at end and just tapped button
                if (prevIndex == EnumValues.Length - 1 && !isRepeat)
                    currentValueIndex = 0; //Wrap Around
                else
                    currentValueIndex = EnumValues.Length - 1; //Stop
            }

            SetValue.Invoke(EnumValues[currentValueIndex]);

            UpdateText();
        }


        public override void OnDecrease(bool isRepeat, int incrementAmount)
        {
            int prevIndex = currentValueIndex;
            currentValueIndex -= incrementAmount;

            //If upper bound reached
            if (currentValueIndex < 0)
            {
                //If already at end and just tapped button
                if (prevIndex == 0 && !isRepeat)
                    currentValueIndex = EnumValues.Length - 1; //Wrap Around
                else
                    currentValueIndex = 0; //Stop
            }

            SetValue.Invoke(EnumValues[currentValueIndex]);

            UpdateText();
        }

        public override void OnRequestTextRefresh()
        {
            UpdateText();
        }
    }
}

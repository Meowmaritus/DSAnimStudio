using System;

namespace DSAnimStudio
{
    public class OptionNameAttribute : Attribute
    {
        public string OptionName { get; private set; }
        
        public OptionNameAttribute(string optionName)
        {
            OptionName = optionName;
        }
    }
}
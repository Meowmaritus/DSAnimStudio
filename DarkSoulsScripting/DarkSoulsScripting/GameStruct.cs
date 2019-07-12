using System;
using System.ComponentModel;

namespace DarkSoulsScripting
{
    public abstract class GameStruct
    {
        public int Address
        {
            get { return AddressReadFunc(); }
        }

        private Func<int> _addressReadFunc;
        internal Func<int> AddressReadFunc
        {
            get => _addressReadFunc;
            set
            {
                _addressReadFunc = value;
                InitSubStructures();
            }
        }

        protected abstract void InitSubStructures();
    }
}

using System;

namespace HatlessEngine
{
    /// <summary>
    /// Struct used as method return type.
    /// Contains and is implicitly castable to bool, and contains another object of choice.
    /// </summary>
    /// <typeparam name="T">The object of choice.</typeparam>
    public struct BoolAndData<T>
    {
        public bool Bool;
        public T Data;

        public BoolAndData(bool bewl, T data)
        {
            Bool = bewl;
            Data = data;
        }

        public static implicit operator bool(BoolAndData<T> boolAndData)
        {
            return boolAndData.Bool;
        }
    }
}

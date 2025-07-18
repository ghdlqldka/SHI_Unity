﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _Base_Framework
{
    public abstract class _ChartParser
    {
        public abstract bool SetPathRelativeTo(string pathObject);
        /// <summary>
        /// returns null if object not found
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public abstract object GetObject(string name);
        public abstract object GetChildObject(object obj, string name);
        public abstract string GetChildObjectValue(object obj, string name);
        public abstract string GetItem(object arr, int item);
        public abstract object GetItemObject(object arr, int item);
        public abstract int GetArraySize(object arr);
        public abstract string ObjectValue(object obj);
    }
}

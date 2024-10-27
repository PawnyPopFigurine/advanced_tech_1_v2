using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace JZK.Utility
{
    public static class JZK_Utility
    {
        public static T[] ArrayAdd<T>(this T[] target, params T[] items)
        {
            if (target == null)
            {
                target = new T[] { };
            }
            if (items == null)
            {
                items = new T[] { };
            }

            T[] result = new T[target.Length + items.Length];
            target.CopyTo(result, 0);
            items.CopyTo(result, target.Length);
            return result;
        }
    }
}
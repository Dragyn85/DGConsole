using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragynGames
{
    public class ReadableTypes
    {
        // All the readable names of accepted types
        private static readonly Dictionary<Type, string> typeReadableNames = new Dictionary<Type, string>()
        {
            {typeof(string), "String"},
            {typeof(bool), "Boolean"},
            {typeof(int), "Integer"},
            {typeof(uint), "Unsigned Integer"},
            {typeof(long), "Long"},
            {typeof(ulong), "Unsigned Long"},
            {typeof(byte), "Byte"},
            {typeof(sbyte), "Short Byte"},
            {typeof(short), "Short"},
            {typeof(ushort), "Unsigned Short"},
            {typeof(char), "Char"},
            {typeof(float), "Float"},
            {typeof(double), "Double"},
            {typeof(decimal), "Decimal"}
        };

        public static string GetReadableTypename(Type type)
        {
            return typeReadableNames[type];
        }
        public static void AddReadableName(Type type, string name)
        {
            typeReadableNames[type] = name;
        }

        internal static void Remove(Type type)
        {
            typeReadableNames.Remove(type);
        }

        internal static bool TryGetReadableName(Type type, out string result)
        {
            return typeReadableNames.TryGetValue(type, out result);
        }
    }
}

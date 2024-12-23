using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DragynGames.Commands
{
    public class CommandTypeParser
    {
        
        // Command parameter delimeter groups
        private static readonly string[] inputDelimiters = new string[] {"\"\"", "''", "{}", "()", "[]"};
        public static string[] GetDelimiters() => inputDelimiters;
        public delegate bool ParseFunction(string input, out object output);
        
        private static readonly Dictionary<Type, ParseFunction> parseFunctions = new Dictionary<Type, ParseFunction>()
            {
                {typeof(string), ParseString},
                {typeof(bool), ParseBool},
                {typeof(int), ParseInt},
                {typeof(uint), ParseUInt},
                {typeof(long), ParseLong},
                {typeof(ulong), ParseULong},
                {typeof(byte), ParseByte},
                {typeof(sbyte), ParseSByte},
                {typeof(short), ParseShort},
                {typeof(ushort), ParseUShort},
                {typeof(char), ParseChar},
                {typeof(float), ParseFloat},
                {typeof(double), ParseDouble},
                {typeof(decimal), ParseDecimal},
                {typeof(Vector2), ParseVector2},
                {typeof(Vector3), ParseVector3},
                {typeof(Vector4), ParseVector4},
                {typeof(Quaternion), ParseQuaternion},
                {typeof(Color), ParseColor},
                {typeof(Color32), ParseColor32},
                {typeof(Rect), ParseRect},
                {typeof(RectOffset), ParseRectOffset},
                {typeof(Bounds), ParseBounds},
                {typeof(GameObject), ParseGameObject},
#if UNITY_2017_2_OR_NEWER
                {typeof(Vector2Int), ParseVector2Int},
                {typeof(Vector3Int), ParseVector3Int},
                {typeof(RectInt), ParseRectInt},
                {typeof(BoundsInt), ParseBoundsInt},
#endif
            };

        public static void AddCustomParameterType(Type type, ParseFunction parseFunction,
            string typeReadableName = null)
        {
            if (type == null)
            {
                Debug.LogError("Parameter type can't be null!");
                return;
            }
            else if (parseFunction == null)
            {
                Debug.LogError("Parameter parseFunction can't be null!");
                return;
            }

            AddParser(type, parseFunction);

            if (!string.IsNullOrEmpty(typeReadableName))
                ReadableTypes.AddReadableName(type, typeReadableName);
        }
        
        

        public static bool ParseArgument(string input, Type argumentType, out object output)
        {
            ParseFunction parseFunction;
            if (parseFunctions.TryGetValue(argumentType, out parseFunction))
                return parseFunction(input, out output);
            else if (typeof(Component).IsAssignableFrom(argumentType))
                return ParseComponent(input, argumentType, out output);
            else if (argumentType.IsEnum)
                return ParseEnum(input, argumentType, out output);
            else if (IsSupportedArrayType(argumentType))
                return ParseArray(input, argumentType, out output);
            else
            {
                output = null;
                return false;
            }
        }
        
        public static bool IsSupportedArrayType(Type type)
        {
            if (type.IsArray)
            {
                if (type.GetArrayRank() != 1)
                    return false;

                type = type.GetElementType();
            }
            else if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() != typeof(List<>))
                    return false;

                type = type.GetGenericArguments()[0];
            }
            else
                return false;

            return HasParserForType(type) || typeof(Component).IsAssignableFrom(type) || type.IsEnum;
        }

        #region Initial Parsers
        public static bool ParseString(string input, out object output)
        {
            output = input;
            return true;
        }

        public static bool ParseBool(string input, out object output)
        {
            if (input == "1" || input.ToLowerInvariant() == "true" || input.ToLowerInvariant() == "yes" ||
                input.ToLowerInvariant() == "on") 
            {
                output = true;
                return true;
            }

            if (input == "0" || input.ToLowerInvariant() == "false" || input.ToLowerInvariant() == "no" ||
                input.ToLowerInvariant() == "off")
            {
                output = false;
                return true;
            }

            output = false;
            return false;
        }

        public static bool ParseInt(string input, out object output)
        {
            int value;
            bool result = int.TryParse(input, out value);

            output = value;
            return result;
        }

        public static bool ParseUInt(string input, out object output)
        {
            uint value;
            bool result = uint.TryParse(input, out value);

            output = value;
            return result;
        }

        public static bool ParseLong(string input, out object output)
        {
            long value;
            bool result =
                long.TryParse(
                    !input.EndsWith("L", StringComparison.OrdinalIgnoreCase)
                        ? input
                        : input.Substring(0, input.Length - 1), out value);

            output = value;
            return result;
        }

        public static bool ParseULong(string input, out object output)
        {
            ulong value;
            bool result =
                ulong.TryParse(
                    !input.EndsWith("L", StringComparison.OrdinalIgnoreCase)
                        ? input
                        : input.Substring(0, input.Length - 1), out value);

            output = value;
            return result;
        }

        public static bool ParseByte(string input, out object output)
        {
            byte value;
            bool result = byte.TryParse(input, out value);

            output = value;
            return result;
        }

        public static bool ParseSByte(string input, out object output)
        {
            sbyte value;
            bool result = sbyte.TryParse(input, out value);

            output = value;
            return result;
        }

        public static bool ParseShort(string input, out object output)
        {
            short value;
            bool result = short.TryParse(input, out value);

            output = value;
            return result;
        }

        public static bool ParseUShort(string input, out object output)
        {
            ushort value;
            bool result = ushort.TryParse(input, out value);

            output = value;
            return result;
        }

        public static bool ParseChar(string input, out object output)
        {
            char value;
            bool result = char.TryParse(input, out value);

            output = value;
            return result;
        }

        public static bool ParseFloat(string input, out object output)
        {
            float value;
            bool result =
                float.TryParse(
                    !input.EndsWith("f", StringComparison.OrdinalIgnoreCase)
                        ? input
                        : input.Substring(0, input.Length - 1), out value);

            output = value;
            return result;
        }

        public static bool ParseDouble(string input, out object output)
        {
            double value;
            bool result =
                double.TryParse(
                    !input.EndsWith("f", StringComparison.OrdinalIgnoreCase)
                        ? input
                        : input.Substring(0, input.Length - 1), out value);

            output = value;
            return result;
        }

        public static bool ParseDecimal(string input, out object output)
        {
            decimal value;
            bool result =
                decimal.TryParse(
                    !input.EndsWith("f", StringComparison.OrdinalIgnoreCase)
                        ? input
                        : input.Substring(0, input.Length - 1), out value);

            output = value;
            return result;
        }

        public static bool ParseVector2(string input, out object output)
        {
            return ParseVector(input, typeof(Vector2), out output);
        }

        public static bool ParseVector3(string input, out object output)
        {
            return ParseVector(input, typeof(Vector3), out output);
        }

        public static bool ParseVector4(string input, out object output)
        {
            return ParseVector(input, typeof(Vector4), out output);
        }

        public static bool ParseQuaternion(string input, out object output)
        {
            return ParseVector(input, typeof(Quaternion), out output);
        }

        public static bool ParseColor(string input, out object output)
        {
            if (ColorUtility.TryParseHtmlString(input, out var color))
            {
                output = color;
                return true;
            }
            return ParseVector(input, typeof(Color), out output);
        }

        public static bool ParseColor32(string input, out object output)
        {
            if (ColorUtility.TryParseHtmlString(input, out var color))
            {
                output = color;
                return true;
            }
            return ParseVector(input, typeof(Color32), out output);
        }

        public static bool ParseRect(string input, out object output)
        {
            return ParseVector(input, typeof(Rect), out output);
        }

        public static bool ParseRectOffset(string input, out object output)
        {
            return ParseVector(input, typeof(RectOffset), out output);
        }

        public static bool ParseBounds(string input, out object output)
        {
            return ParseVector(input, typeof(Bounds), out output);
        }

#if UNITY_2017_2_OR_NEWER
        public static bool ParseVector2Int(string input, out object output)
        {
            return ParseVector(input, typeof(Vector2Int), out output);
        }

        public static bool ParseVector3Int(string input, out object output)
        {
            return ParseVector(input, typeof(Vector3Int), out output);
        }

        public static bool ParseRectInt(string input, out object output)
        {
            return ParseVector(input, typeof(RectInt), out output);
        }

        public static bool ParseBoundsInt(string input, out object output)
        {
            return ParseVector(input, typeof(BoundsInt), out output);
        }
#endif

        public static bool ParseGameObject(string input, out object output)
        {
            output = input == "null" ? null : GameObject.Find(input);
            return true;
        }

        public static bool ParseComponent(string input, Type componentType, out object output)
        {
            GameObject gameObject = input == "null" ? null : GameObject.Find(input);
            output = gameObject ? gameObject.GetComponent(componentType) : null;
            return true;
        }

        public static bool ParseEnum(string input, Type enumType, out object output)
        {
            const int NONE = 0, OR = 1, AND = 2;

            int outputInt = 0;
            int operation = NONE; // 0: nothing, 1: OR with outputInt, 2: AND with outputInt
            for (int i = 0; i < input.Length; i++)
            {
                string enumStr;
                int orIndex = input.IndexOf('|', i);
                int andIndex = input.IndexOf('&', i);
                if (orIndex < 0)
                    enumStr = input.Substring(i, (andIndex < 0 ? input.Length : andIndex) - i).Trim();
                else
                    enumStr = input.Substring(i, (andIndex < 0 ? orIndex : Mathf.Min(andIndex, orIndex)) - i).Trim();

                int value;
                if (!int.TryParse(enumStr, out value))
                {
                    try
                    {
                        // Case-insensitive enum parsing
                        value = Convert.ToInt32(Enum.Parse(enumType, enumStr, true));
                    }
                    catch
                    {
                        output = null;
                        return false;
                    }
                }

                if (operation == NONE)
                    outputInt = value;
                else if (operation == OR)
                    outputInt |= value;
                else
                    outputInt &= value;

                if (orIndex >= 0)
                {
                    if (andIndex > orIndex)
                    {
                        operation = AND;
                        i = andIndex;
                    }
                    else
                    {
                        operation = OR;
                        i = orIndex;
                    }
                }
                else if (andIndex >= 0)
                {
                    operation = AND;
                    i = andIndex;
                }
                else
                    i = input.Length;
            }

            output = Enum.ToObject(enumType, outputInt);
            return true;
        }

        public static bool ParseArray(string input, Type arrayType, out object output)
        {
            List<string> valuesToParse = new List<string>(2);
            
            SplitIntoArgumentsForCommand(input, valuesToParse);

            IList result = (IList)Activator.CreateInstance(arrayType, new object[1] { valuesToParse.Count });
            output = result;

            if (arrayType.IsArray)
            {
                Type elementType = arrayType.GetElementType();
                for (int i = 0; i < valuesToParse.Count; i++)
                {
                    object obj;
                    if (!ParseArgument(valuesToParse[i], elementType, out obj))
                        return false;

                    result[i] = obj;
                }
            }
            else
            {
                Type elementType = arrayType.GetGenericArguments()[0];
                for (int i = 0; i < valuesToParse.Count; i++)
                {
                    object obj;
                    if (!ParseArgument(valuesToParse[i], elementType, out obj))
                        return false;

                    result.Add(obj);
                }
            }

            return true;
        }

        // Create a vector of specified type (fill the blank slots with 0 or ignore unnecessary slots)
        private static bool ParseVector(string input, Type vectorType, out object output)
        {
            List<string> tokens = new List<string>(input.Replace(',', ' ').Trim().Split(' '));
            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                tokens[i] = tokens[i].Trim();
                if (tokens[i].Length == 0)
                    tokens.RemoveAt(i);
            }

            float[] tokenValues = new float[tokens.Count];
            for (int i = 0; i < tokens.Count; i++)
            {
                object val;
                if (!ParseFloat(tokens[i], out val))
                {
                    if (vectorType == typeof(Vector3))
                        output = Vector3.zero;
                    else if (vectorType == typeof(Vector2))
                        output = Vector2.zero;
                    else
                        output = Vector4.zero;

                    return false;
                }

                tokenValues[i] = (float)val;
            }

            if (vectorType == typeof(Vector3))
            {
                Vector3 result = Vector3.zero;

                for (int i = 0; i < tokenValues.Length && i < 3; i++)
                    result[i] = tokenValues[i];

                output = result;
            }
            else if (vectorType == typeof(Vector2))
            {
                Vector2 result = Vector2.zero;

                for (int i = 0; i < tokenValues.Length && i < 2; i++)
                    result[i] = tokenValues[i];

                output = result;
            }
            else if (vectorType == typeof(Vector4))
            {
                Vector4 result = Vector4.zero;

                for (int i = 0; i < tokenValues.Length && i < 4; i++)
                    result[i] = tokenValues[i];

                output = result;
            }
            else if (vectorType == typeof(Quaternion))
            {
                Quaternion result = Quaternion.identity;

                for (int i = 0; i < tokenValues.Length && i < 4; i++)
                    result[i] = tokenValues[i];

                output = result;
            }
            else if (vectorType == typeof(Color))
            {
                Color result = Color.black;

                for (int i = 0; i < tokenValues.Length && i < 4; i++)
                    result[i] = tokenValues[i];

                output = result;
            }
            else if (vectorType == typeof(Color32))
            {
                Color32 result = new Color32(0, 0, 0, 255);

                if (tokenValues.Length > 0)
                    result.r = (byte)Mathf.RoundToInt(tokenValues[0]);
                if (tokenValues.Length > 1)
                    result.g = (byte)Mathf.RoundToInt(tokenValues[1]);
                if (tokenValues.Length > 2)
                    result.b = (byte)Mathf.RoundToInt(tokenValues[2]);
                if (tokenValues.Length > 3)
                    result.a = (byte)Mathf.RoundToInt(tokenValues[3]);

                output = result;
            }
            else if (vectorType == typeof(Rect))
            {
                Rect result = Rect.zero;

                if (tokenValues.Length > 0)
                    result.x = tokenValues[0];
                if (tokenValues.Length > 1)
                    result.y = tokenValues[1];
                if (tokenValues.Length > 2)
                    result.width = tokenValues[2];
                if (tokenValues.Length > 3)
                    result.height = tokenValues[3];

                output = result;
            }
            else if (vectorType == typeof(RectOffset))
            {
                RectOffset result = new RectOffset();

                if (tokenValues.Length > 0)
                    result.left = Mathf.RoundToInt(tokenValues[0]);
                if (tokenValues.Length > 1)
                    result.right = Mathf.RoundToInt(tokenValues[1]);
                if (tokenValues.Length > 2)
                    result.top = Mathf.RoundToInt(tokenValues[2]);
                if (tokenValues.Length > 3)
                    result.bottom = Mathf.RoundToInt(tokenValues[3]);

                output = result;
            }
            else if (vectorType == typeof(Bounds))
            {
                Vector3 center = Vector3.zero;
                for (int i = 0; i < tokenValues.Length && i < 3; i++)
                    center[i] = tokenValues[i];

                Vector3 size = Vector3.zero;
                for (int i = 3; i < tokenValues.Length && i < 6; i++)
                    size[i - 3] = tokenValues[i];

                output = new Bounds(center, size);
            }
#if UNITY_2017_2_OR_NEWER
            else if (vectorType == typeof(Vector3Int))
            {
                Vector3Int result = Vector3Int.zero;

                for (int i = 0; i < tokenValues.Length && i < 3; i++)
                    result[i] = Mathf.RoundToInt(tokenValues[i]);

                output = result;
            }
            else if (vectorType == typeof(Vector2Int))
            {
                Vector2Int result = Vector2Int.zero;

                for (int i = 0; i < tokenValues.Length && i < 2; i++)
                    result[i] = Mathf.RoundToInt(tokenValues[i]);

                output = result;
            }
            else if (vectorType == typeof(RectInt))
            {
                RectInt result = new RectInt();

                if (tokenValues.Length > 0)
                    result.x = Mathf.RoundToInt(tokenValues[0]);
                if (tokenValues.Length > 1)
                    result.y = Mathf.RoundToInt(tokenValues[1]);
                if (tokenValues.Length > 2)
                    result.width = Mathf.RoundToInt(tokenValues[2]);
                if (tokenValues.Length > 3)
                    result.height = Mathf.RoundToInt(tokenValues[3]);

                output = result;
            }
            else if (vectorType == typeof(BoundsInt))
            {
                Vector3Int center = Vector3Int.zero;
                for (int i = 0; i < tokenValues.Length && i < 3; i++)
                    center[i] = Mathf.RoundToInt(tokenValues[i]);

                Vector3Int size = Vector3Int.zero;
                for (int i = 3; i < tokenValues.Length && i < 6; i++)
                    size[i - 3] = Mathf.RoundToInt(tokenValues[i]);

                output = new BoundsInt(center, size);
            }
#endif
            else
            {
                output = null;
                return false;
            }

            return true;
        }

        internal static void RemoveType(Type type)
        {
            parseFunctions.Remove(type);
        }

        internal static void AddParser(Type type, ParseFunction parseFunction)
        {
            parseFunctions[type] = parseFunction;
        }

        internal static bool HasParserForType(Type parameterType)
        {
            return parseFunctions.ContainsKey(parameterType);
        }

        #endregion
        
        public static void SplitIntoArgumentsForCommand(string command, List<string> commandArguments)
        {
            for (int i = 0; i < command.Length; i++)
            {
                if (char.IsWhiteSpace(command[i]))
                    continue;

                int delimiterIndex = IndexOfDelimiterGroup(command[i]);
                if (delimiterIndex >= 0)
                {
                    int endIndex = IndexOfDelimiterGroupEnd(command, delimiterIndex, i + 1);
                    commandArguments.Add(command.Substring(i + 1, endIndex - i - 1));
                    i = (endIndex < command.Length - 1 && command[endIndex + 1] == ',') ? endIndex + 1 : endIndex;
                }
                else
                {
                    int endIndex = IndexOfChar(command, ' ', i + 1);
                    commandArguments.Add(command.Substring(i,
                        command[endIndex - 1] == ',' ? endIndex - 1 - i : endIndex - i));
                    i = endIndex;
                }
            }
        }
        
        public static List<string> SplitIntoArgumentsForCommand(string command,out string commandTarget, char objIdentifier)
        {
            commandTarget = ExtractTarget(ref command, objIdentifier);
            
            List<string> commandArguments = new List<string>();
            for (int i = 0; i < command.Length; i++)
            {
                if (char.IsWhiteSpace(command[i]))
                    continue;

                int delimiterIndex = IndexOfDelimiterGroup(command[i]);
                if (delimiterIndex >= 0)
                {
                    int endIndex = IndexOfDelimiterGroupEnd(command, delimiterIndex, i + 1);
                    commandArguments.Add(command.Substring(i + 1, endIndex - i - 1));
                    i = (endIndex < command.Length - 1 && command[endIndex + 1] == ',') ? endIndex + 1 : endIndex;
                }
                else
                {
                    int endIndex = IndexOfChar(command, ' ', i + 1);
                    commandArguments.Add(command.Substring(i,
                        command[endIndex - 1] == ',' ? endIndex - 1 - i : endIndex - i));
                    i = endIndex;
                }
            }

            return commandArguments;
        }

        private static string ExtractTarget(ref string command, char objIdentifier)
        {
            string commandTarget = null;
            string objIdentifierStr = objIdentifier.ToString()+objIdentifier.ToString();
            if(command.Contains(objIdentifierStr))
            {
                commandTarget = "@@";
                command = command.Substring(0, command.IndexOf(objIdentifierStr));
            }
            {
                
            }
            if (command.Contains(objIdentifier))
            {
                // get everything after the object identifier and pass it to commandTarget
                commandTarget = command.Substring(command.IndexOf(objIdentifier) + 1);
                // remove the object identifier and everthing behind from the command
                command = command.Substring(0, command.IndexOf(objIdentifier));
            }
            

            return commandTarget;
        }

        // Find the index of the delimiter group that 'c' belongs to
        internal static int IndexOfDelimiterGroup(char c)
        {
            for (int i = 0; i < inputDelimiters.Length; i++)
            {
                if (c == inputDelimiters[i][0])
                    return i;
            }

            return -1;
        }

        internal static int IndexOfDelimiterGroupEnd(string command, int delimiterIndex, int startIndex)
        {
            char startChar = inputDelimiters[delimiterIndex][0];
            char endChar = inputDelimiters[delimiterIndex][1];

            // Check delimiter's depth for array support (e.g. [[1 2] [3 4]] for Vector2 array)
            int depth = 1;

            for (int i = startIndex; i < command.Length; i++)
            {
                char c = command[i];
                if (c == endChar && --depth <= 0)
                    return i;
                else if (c == startChar)
                    depth++;
            }

            return command.Length;
        }
        
        // Find the index of char in the string, or return the length of string instead of -1
        internal static int IndexOfChar(string command, char c, int startIndex)
        {
            int result = command.IndexOf(c, startIndex);
            if (result < 0)
                result = command.Length;

            return result;
        }
        
        
    }
}

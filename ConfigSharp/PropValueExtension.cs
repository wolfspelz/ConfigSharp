using System;
using System.Reflection;

namespace ConfigSharp
{
    public static class PropValueExtension
    {
        public static object GetMemberValue(this Object obj, string name)
        {
            if (obj == null) { return null; }

            foreach (string part in name.Split('.')) {

                Type type = obj.GetType();
                PropertyInfo pi = type.GetProperty(part);
                if (pi != null) {
                    obj = pi.GetValue(obj, null);
                } else {
                    FieldInfo fi = type.GetField(part);
                    if (fi != null) {
                        obj = fi.GetValue(obj);
                    } else {
                        obj = null;
                    }
                }

            }

            return obj;
        }

        public static T GetMemberValue<T>(this object obj, string name, T defaultValue)
        {
            object value = GetMemberValue(obj, name);
            if (value == null) {
                return defaultValue;
            }

            // throws InvalidCastException if types are incompatible
            return (T)value;
        }
    }
}

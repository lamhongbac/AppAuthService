using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSA.Utils
{
    public static class EnumExtentions
    {


        public static List<EnumDataInfo> GetEnumDataInfo(this Enum enumObject)
        {
            return enumObject.GetType().GetEnumDataInfo();
        }
        public static List<EnumDataInfo> GetEnumDataInfo(this Type enumType)
        {
            List<EnumDataInfo> list = new List<EnumDataInfo>();
            bool flag = enumType != null;
            if (flag)
            {
                string[] names = Enum.GetNames(enumType);
                Array values = Enum.GetValues(enumType);
                for (int i = 0; i < names.Count<string>(); i++)
                {
                    list.Add(new EnumDataInfo
                    {
                        Value = values.GetValue(i),
                        Name = names[i],
                        ID = names[i],
                        Ordinal = i + 1
                    });
                }
            }
            return list;
        }
        public static List<SimpleDataSource> GetSimpleDataSource(this Type enumType)
        {
            List<SimpleDataSource> list = new List<SimpleDataSource>();

            if (enumType != null)
            {
                string[] names = Enum.GetNames(enumType);
                Array values = Enum.GetValues(enumType);
                for (int i = 0; i < names.Count<string>(); i++)
                {
                    list.Add(new SimpleDataSource
                    {
                        ID = Convert.ToInt32(values.GetValue(i)),
                        Name = names[i]

                    });
                }
            }
            return list;
        }
    }
    public class EnumDataInfo
    {
        public class FieldNames
        {
            public const string ID = "ID";
            public const string Name = "Name";
            public const string Value = "Value";
            public const string Ordinal = "Ordinal";
            public const string Translate = "Translate";
        }
        private string translate;
        public bool Checked
        {
            get;
            set;
        }
        public string ID
        {
            get;
            set;
        }
        public int Ordinal
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        public object Value
        {
            get;
            set;
        }
        public string Translate
        {
            get
            {
                bool flag = string.IsNullOrWhiteSpace(this.translate);
                if (flag)
                {
                    this.translate = this.ToString();
                }
                return this.translate;
            }
            set
            {
                this.translate = value;
            }
        }
    }
    public class SimpleDataSource
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
    public static class EnumHelper<T>
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
        /// <summary>
        /// Chuyen 1 string value thanh 1 enum value of T
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T Parse(string value, T defaultValue)
        {
            if (Enum.IsDefined(typeof(T), value))
                return (T)Enum.Parse(typeof(T), value);

            int num;
            if (int.TryParse(value, out num))
            {
                if (Enum.IsDefined(typeof(T), num))
                    return (T)Enum.ToObject(typeof(T), num);
            }

            return defaultValue;
        }
        /// <summary>
        /// Convert an enum to a dictionary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dic"></param>
        public static Dictionary<int, string> ConvertToDictionary<T>()
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            foreach (T foo in Enum.GetValues(typeof(T)))
            {
                dic.Add(Convert.ToInt32(foo), foo.ToString());
            }
            return dic;
        }
        /// <summary>
        /// add to a dic
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static int AddToDictionary<T>(ref Dictionary<int, string> dic)
        {
            int i = 0;
            foreach (T foo in Enum.GetValues(typeof(T)))
            {
                dic.Add(Convert.ToInt32(foo), foo.ToString());
                i++;
            }
            return i;
        }
    }
}

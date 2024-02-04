/*
           Напишите 2 метода использующие рефлексию
           1 - сохраняет информацию о классе в строку
           2 - позволяет восстановить класс из строки с информацией о методе
           В качестве примере класса используйте класс TestClass.
           Шаблоны методов для реализации:
           */

using System.Text;
using System.Reflection;

namespace Seminar7
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var v = CreateTestClassInstance(1,"ssad", 3, new char[] { '4', '3', '8', '2' }); 
            string str = ObjectToString(v);
            Console.WriteLine(str);
            object str2 = StringToObject(str);

            str = ObjectToString(str2);
            Console.WriteLine(str);
        }

        public static TestClass CreateTestClassInstance(int i, string s, decimal d, char[] c)
        {
            var testClassType = typeof(TestClass);
            var testClass = Activator.CreateInstance(testClassType) as TestClass;

            var testClassTwo = Activator.CreateInstance(testClassType, new object[] { i });

            var testClassThird = Activator.CreateInstance(testClassType, new object[] { i, s, d, c });

            return testClassThird as TestClass;
        }

        public static object StringToObject(string endString)
        {
            string[] str = endString.Split("\n");
            var typeName = str[2];
            var assemblyName = str[1];
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == assemblyName);
            if (assembly != null)
            {
                var type = assembly.GetTypes().FirstOrDefault(t => t.FullName == typeName);
                if (type != null)
                {
                    var obj = Activator.CreateInstance(type);

                    Dictionary<string, string> atribProp = CreateDictProp(type);

                    var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    for (int i = 3; i < str.Length - 1; i++)
                    {
                        var propertyString = str[i].Split(":");
                        var propertyName = propertyString[0].Trim();
                        var propertyValue = propertyString[1].Trim();
                        propertyName = atribProp.ContainsKey(propertyName) ? atribProp[propertyName] : propertyName;


                        var property = type.GetProperty(propertyName);
                        if (property != null)
                        {
                            if (property.PropertyType == typeof(int)) property.SetValue(obj, Convert.ToInt32(propertyValue));
                            else if (property.PropertyType == typeof(string)) property.SetValue(obj, propertyValue);
                            else if (property.PropertyType == typeof(decimal)) property.SetValue(obj, Convert.ToDecimal(propertyValue));
                        }
                    }
                    return obj;
                }
            }
            return null;
        }

        private static Dictionary<string, string> CreateDictProp(Type type)
        {
            Dictionary<string, string> atribProp = new Dictionary<string, string>();
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                string propertyName = GetCustomName(prop);
                atribProp.Add(propertyName, prop.Name);
            }
            return atribProp;
        }

        private static string GetCustomName(PropertyInfo prop)
        {
            var customNameAttribute = (CustomNameAttribute)Attribute.GetCustomAttribute(prop, typeof(CustomNameAttribute));
            return customNameAttribute != null ? customNameAttribute.CustomFieldName : prop.Name;
        }

        public static string ObjectToString(object obj)
        {
            StringBuilder stringBuilder = new StringBuilder();
            var type = obj.GetType();
            stringBuilder.Append(type.ToString() + "\n");
            stringBuilder.Append(type.Assembly + "\n");
            stringBuilder.Append(type.Name + "\n");
            var properties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            foreach (var item in properties)
            {
                var value = item.GetValue(obj);
                string str = GetCustomName(item);

                stringBuilder.Append(str + ":");
                if (item.PropertyType == typeof(char[]))
                {
                    stringBuilder.Append(new String(value as char[]) + "\n");
                }
                else
                {
                    stringBuilder.Append(value + "\n");
                }
            }
            return stringBuilder.ToString();
        }
    }
}
internal class TestClass
{
    [CustomName("Интеджер 1")]
    public int I { get; set; }
    [CustomName("Строка")]
    public string? S { get; set; }
    [CustomName("Децимал")]
    public decimal? D { get; set; }
    [CustomName("Массив символов 1")]
    public char[]? C { get; set; }

    public TestClass() { }
    public TestClass(int i) { I = i; }
    public TestClass(int i, string s, decimal d, char[] c): this(i)
    { S = s; D = d; C = c; }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class CustomNameAttribute : Attribute
{
    public string CustomFieldName { get; }

    public CustomNameAttribute(string customFieldName)
    {
        CustomFieldName = customFieldName;
    }
}
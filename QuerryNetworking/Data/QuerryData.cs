using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace QuerryNetworking.Data
{
    public static class QuerryData
    {
        public static string Convert<T>(T Original)
        {
            string Data = "";
            foreach(var item in Original.GetType().GetProperties())
            {
                Console.WriteLine(item.Name);
                if (item.PropertyType.IsPrimitive || item.PropertyType == typeof(string) || !item.CanWrite || item.PropertyType.IsEnum)
                {
                    object VALUE = Original.GetType().GetProperty(item.Name).GetValue(Original);
                    if(VALUE != null)
                    {
                        Data += item.Name + ":" + VALUE.ToString() + "|";
                    }
                    else
                    {
                        Data += item.Name + ":" + "null" + "|";
                    }
                }
                else
                {
                    Type Goof = item.PropertyType;
                    var ConvertMethod = typeof(QuerryData).GetMethod(nameof(QuerryData.Convert));
                    var NewRef = ConvertMethod.MakeGenericMethod(Goof);
                    object VALUE = Original.GetType().GetProperty(item.Name).GetValue(Original);
                    if(VALUE != null)
                    {
                        string S = (string)NewRef.Invoke(null, new object?[1] { VALUE });

                        Data += item.Name + ":" + "{" + S + "}" + "|";
                    }
                    else
                    {
                        Data += item.Name + ":" + "null" + "|";
                    }
                }
            }
            Data = Data.TrimEnd('|');
            return Data;
        }

        public static T UnConvert<T>(string Original)
        {
            T NewInstance = (T)Activator.CreateInstance(typeof(T));

            string ReadString = "";

            string CurrentProperty = "";
            int AmountRead = 0;
            int PropertyLength = 0;
            int ValueLength = 0;
            bool ReadingValue = false;
            bool ReadingProperty = true;

            bool SubClass = false;

            foreach (var character in Original)
            {
                AmountRead++;
                if(character == '{')
                {
                    SubClass = true;
                }
                if(character == '{')
                {
                    SubClass = false;
                }
                if(SubClass)
                {
                    ValueLength++;
                }
                else if (character == ':')
                {
                    // remove everything after
                    CurrentProperty = Original.Remove(AmountRead, Original.Count() - AmountRead);

                    // remove everything before
                    CurrentProperty = CurrentProperty.Remove(0, CurrentProperty.Count() - PropertyLength-1);

                    if (CurrentProperty.Contains(":"))
                    {
                        CurrentProperty = CurrentProperty.Replace(":", "");
                    }
                    ReadingValue = true;
                    ReadingProperty = false;
                    PropertyLength = 0;
                }
                else if (character == '|')
                {
                    ReadingProperty = true;

                    //remove everything after
                    string CurrentValue = Original.Remove(AmountRead, Original.Count() - AmountRead);

                    if (CurrentValue.Contains("|"))
                    {
                        CurrentValue = CurrentValue.Replace("|", "");
                    }

                    //remove everything before
                    CurrentValue = CurrentValue.Remove(0, CurrentValue.Count() - ValueLength);

                    if (CurrentValue.Contains(":"))
                    {
                        CurrentValue = CurrentValue.Replace(":", "");
                    }

                    if (CurrentValue.Contains(CurrentProperty))
                    {
                        CurrentValue = CurrentValue.Replace(CurrentProperty, "");
                    }

                    Type Goof = NewInstance.GetType().GetProperty(CurrentProperty).PropertyType;

                    if (Goof.IsPrimitive || Goof == typeof(string))
                    {
                        object NewO = System.Convert.ChangeType(CurrentValue, Goof);
                        NewInstance.GetType().GetProperty(CurrentProperty).SetValue(NewInstance, NewO);
                    }
                    else
                    {
                        CurrentValue = CurrentValue.Replace("{", "");
                        CurrentValue = CurrentValue.Replace("}", "");
                        object NewO = System.Convert.ChangeType(CurrentValue, Goof);
                        var UnConvertMethod = typeof(QuerryData).GetMethod("UnConvert");
                        var NewRef = UnConvertMethod.MakeGenericMethod(Goof);
                        string S = (string)NewRef.Invoke(null, new object?[1] { CurrentValue });
                        NewInstance.GetType().GetProperty(CurrentProperty).SetValue(NewInstance, S);
                    }
                    

                    ValueLength = 0;
                }
                else
                {
                    if (ReadingValue)
                    {
                        ValueLength++;
                    }
                    if(ReadingProperty)
                    {
                        PropertyLength++;
                    }
                }
                ReadString += character;
            }

            return NewInstance;
        }
    }
}

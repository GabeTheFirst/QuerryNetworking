using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace QuerryNetworking.Data
{
    // I am not done commenting in here!
    public static class QuerryData
    {
        // convert data from a class to a string! (except if they use lists for now)
        public static string Convert<T>(T Original)
        {
            // the string that is returned
            string Data = "";

            // foreach property in the original data
            foreach(var item in Original.GetType().GetProperties())
            {
                // if it's a primitive, string, can't be written, or is an enum, use this
                if (item.PropertyType.IsPrimitive || item.PropertyType == typeof(string) || !item.CanWrite || item.PropertyType.IsEnum)
                {
                    // the value
                    object VALUE = Original.GetType().GetProperty(item.Name).GetValue(Original);
                    
                    // if the value is not null
                    if(VALUE != null)
                    {
                        // add the value into the string
                        Data += item.Name + ":" + VALUE.ToString() + "|";
                    }
                    else
                    {
                        // add null
                        Data += item.Name + ":" + "null" + "|";
                    }
                }
                else
                {
                    // "Goof" is the type of the current item we are looking at
                    Type Goof = item.PropertyType;

                    // the method to convert it 
                    var ConvertMethod = typeof(QuerryData).GetMethod(nameof(QuerryData.Convert));
                    
                    // that method but generic
                    var NewRef = ConvertMethod.MakeGenericMethod(Goof);

                    // the value of the property
                    object VALUE = Original.GetType().GetProperty(item.Name).GetValue(Original);

                    // if it's not null, add the value into the string
                    if(VALUE != null)
                    {
                        // use the convert method to convert the object to a string
                        string S = (string)NewRef.Invoke(null, new object?[1] { VALUE });

                        // actually add it to the string
                        Data += item.Name + ":" + "{" + S + "}" + "|";
                    }
                    else
                    {
                        // add null to the string
                        Data += item.Name + ":" + "null" + "|";
                    }
                }
            }
            // remove the final "|"
            Data = Data.TrimEnd('|');
            // return the resulting data
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

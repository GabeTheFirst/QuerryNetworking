using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Security.Cryptography;

namespace QuerryNetworking.Data
{
    // How data should probably look {string:"test",int:1,bool:true,list:["only","string","lists"]}
    // honestly I don't feel like making this right now, JSON it is!

    // I did not end up doing anything like what's above, it's not even the same type of system
    // sorry for no comments here!!

    // databases without asp.net are really dumb so I'm going to make some type of saving data system here, idk how good it will be
    // ok it's probably not that good haha, but here it is! (so far)
    public static class DataManager
    {
        public static T ReadSingle<T>(string Member, string Value)
        {
            string[] f = File.ReadAllLines(Directory.GetCurrentDirectory() + "/" + typeof(T).Name + ".qd");
            int MemberAt = 0;
            string CurrentMember = "";
            int MemberAtValue = 0;
            object o = Activator.CreateInstance(typeof(T));
            bool InValid = false;
            foreach (var item in f)
            {
                //Console.WriteLine(CurrentMember + " vs " + Member + " and " + item + " vs " + Value);
                
                if(item.EndsWith(":") && !InValid)
                {
                    CurrentMember = item.Replace(":", "");
                    //Console.WriteLine("In valid for " + CurrentMember);
                    InValid = true;
                    MemberAt = 0;
                }
                if(InValid)
                {
                    //Console.WriteLine(CurrentMember + " vs " + Member + " and " + item + " vs " + Value);
                    if (CurrentMember == Member && item == Value)
                    {
                        //Console.WriteLine("Member found!");
                        MemberAtValue = MemberAt;
                    }
                }
                if(InValid && item == "END")
                {
                    InValid = false;
                    MemberAt = 0;
                    //Console.WriteLine("No longer valid for " + CurrentMember);
                }
                MemberAt++;
            }

            //Console.WriteLine("The member is at: " + MemberAtValue);
            MemberAt = 0;
            InValid = false;

            if(MemberAtValue == 0)
            {
                return default(T);
            }
            
            foreach (var item in f)
            {
                if (item.EndsWith(":") && !InValid)
                {
                    CurrentMember = item.Replace(":","");
                    InValid = true;
                    MemberAt = 0;
                }
                if (InValid)
                {
                    if (MemberAtValue == MemberAt)
                    {
                        Type Goof = o.GetType().GetProperty(CurrentMember).PropertyType;
                        object NewO = Convert.ChangeType(item, Goof);
                        o.GetType().GetProperty(CurrentMember).SetValue(o, NewO);
                    }
                    else
                    {
                    }
                }
                if (InValid && item == "END")
                {
                    InValid = false;
                    MemberAt = 0;
                }
                MemberAt++;
            }

            return (T)o;
        }

        public static List<T> ReadMultiple<T>(string Member, string Equals)
        {
            string[] f = File.ReadAllLines(Directory.GetCurrentDirectory() + "/" + typeof(T).Name + ".qd");

            int MemberAt = 0;
            string CurrentMember = "";
            List<int> MemberAtValues = new List<int>();
            object? o = Activator.CreateInstance(typeof(T));
            bool InValid = false;
            foreach (var item in f)
            {
                
                if (item.EndsWith(":") && !InValid)
                {
                    CurrentMember = item.Replace(":", "");
                    InValid = true;
                }
                if (InValid)
                {
                    if (item == Equals)
                    {
                        if (CurrentMember == Member)
                        {
                            MemberAtValues.Add(MemberAt);
                        }
                    }
                    else
                    {
                    }
                }
                if (InValid && item == "END")
                {
                    InValid = false;
                }
                MemberAt++;
            }
            MemberAt = 0;
            InValid = false;
            List<T> ToReturn = new List<T>();
            foreach(var MemberVal in MemberAtValues)
            {
                foreach (var item in f)
                {
                    
                    if (item.EndsWith(":") && !InValid)
                    {
                        CurrentMember = item.Replace(":", "");
                        InValid = true;
                    }
                    if (InValid)
                    {
                        if (MemberAt == MemberVal)
                        {
                            Type Goof = o.GetType().GetProperty(CurrentMember).PropertyType;
                            o.GetType().GetProperty(CurrentMember).SetValue(o, Convert.ChangeType(item, Goof));
                        }
                        else
                        {
                        }
                    }
                    if (InValid && item == "END")
                    {
                        InValid = false;
                    }
                    MemberAt++;
                }
                MemberAt = 0;
                ToReturn.Add((T)o);
            }

            return ToReturn;
        }

        public static async Task<T> AddNew<T>(T NewInstance)
        {
            List<string> f = File.ReadAllLines(Directory.GetCurrentDirectory() + "/" + typeof(T).Name + ".qd").ToList();
            int MemberAt = 0;
            string CurrentMember = "";

            bool InValid = false;


            int Count = f.Count;
            for (int i = 0; i < Count; i++)
            {
                if (f[i].EndsWith(":") && !InValid)
                {
                    CurrentMember = f[i].Replace(":", "");
                    InValid = true;
                    MemberAt = 0;
                }
                if (InValid && f[i] == "END")
                {
                    //Console.WriteLine(f[i] + " to " + NewInstance.GetType().GetProperty(CurrentMember).GetValue(NewInstance).ToString());
                    f.Insert(i, NewInstance.GetType().GetProperty(CurrentMember).GetValue(NewInstance).ToString());
                    i++;
                    Count++;
                    InValid = false;
                    MemberAt = 0;
                }
                MemberAt++;
                
            }
            string NewString = "";
            foreach(var item in f)
            {
                //Console.WriteLine(item);
                NewString += item;
                NewString += Environment.NewLine;
            }

            await File.WriteAllTextAsync(Directory.GetCurrentDirectory() + "/" + typeof(T).Name + ".qd", NewString);

            return NewInstance;
        }

        public static async Task<T> UpdateSingle<T>(string Member, string Value, T NewInstance)
        {
            List<string> f = new List<string>();
            try
            {
                f = (await File.ReadAllLinesAsync(Directory.GetCurrentDirectory() + "/" + typeof(T).Name + ".qd")).ToList();
            }
            catch
            {
                Console.WriteLine("[FAILED TO UPDATE FILE]");
                return default(T);
            }
            int MemberAt = 0;
            string CurrentMember = "";
            int MemberAtValue = 0;

            bool InValid = false;

            foreach (var item in f)
            {
                //Console.WriteLine(CurrentMember + " vs " + Member + " and " + item + " vs " + Value);

                if (item.EndsWith(":") && !InValid)
                {
                    CurrentMember = item.Replace(":", "");
                    //Console.WriteLine("In valid for " + CurrentMember);
                    InValid = true;
                    MemberAt = 0;
                }
                if (InValid)
                {
                    //Console.WriteLine(CurrentMember + " vs " + Member + " and " + item + " vs " + Value);
                    if (CurrentMember == Member && item == Value)
                    {
                        //Console.WriteLine("Member found!");
                        MemberAtValue = MemberAt;
                    }
                }
                if (InValid && item == "END")
                {
                    InValid = false;
                    MemberAt = 0;
                    //Console.WriteLine("No longer valid for " + CurrentMember);
                }
                MemberAt++;
            }

            //Console.WriteLine("The member is at: " + MemberAtValue);
            MemberAt = 0;
            InValid = false;

            if (MemberAtValue == 0)
            {
                return default(T);
            }

            for (int item = 0; item < f.Count; item++)
            {
                if (f[item].EndsWith(":") && !InValid)
                {
                    CurrentMember = f[item].Replace(":", "");
                    InValid = true;
                    MemberAt = 0;
                }
                if (InValid)
                {
                    if (MemberAtValue == MemberAt)
                    {
                        f[item] = NewInstance.GetType().GetProperty(CurrentMember).GetValue(NewInstance).ToString();
                    }
                }
                if (InValid && f[item] == "END")
                {
                    InValid = false;
                    MemberAt = 0;
                }
                MemberAt++;
            }

            string NewString = "";
            foreach (var item in f)
            {
                NewString += item;
                NewString += Environment.NewLine;
            }

            using (FileStream fs = File.OpenWrite(Directory.GetCurrentDirectory() + "/" + typeof(T).Name + ".qd"))
            {
                byte[] Data = Encoding.UTF8.GetBytes(NewString);
                fs.WriteAsync(Data, 0, Data.Length);
                fs.Close();
            }


            return NewInstance;
        }

        public static async Task CreateFileIfNotExists<T>()
        {
            if(File.Exists(Directory.GetCurrentDirectory() + "/" + typeof(T).Name + ".qd"))
            {
                return;
            }
            MemberInfo[] members = typeof(T).GetMembers();
            List<string> Values = new List<string>();
            foreach(var item in members)
            {
                if(item.Name.StartsWith("get_"))
                {
                    Values.Add(item.Name.Substring(4));
                }
            }
            int AmountWritten = 0;
            using (StreamWriter s = File.CreateText(Directory.GetCurrentDirectory() + "/" + typeof(T).Name + ".qd"))
            {
                for(int i = 0; i < Values.Count; i++)
                {
                    s.Write(Values[i]);
                    s.Write(":");
                    s.WriteLine();
                    s.Write("END");
                    s.WriteLine();
                }
            }
        }
    }
}

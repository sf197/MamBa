using PlugInProgram;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MamBa
{
    class Class1
    {
        public static List<string> GetLibDir(string path) {
            List<string> dirlist = new List<string>();
            DirectoryInfo root = new DirectoryInfo(path);
            try
            {
                foreach (FileInfo f in root.GetFiles())
                {
                    if (f.Extension.Equals(".dll"))
                    {
                        string name = f.Name;
                        dirlist.Add(name);
                    }
                }
                return dirlist;
            }
            catch (DirectoryNotFoundException e)
            {
                MessageBox.Show(e.Message, "Exception ERROR!");
            }
            return null;
        }

        /// <summary>
        /// 根据全名和路径构造对象
        /// </summary>
        /// <param name="sFilePath">程序集路径</param>
        /// <returns></returns>
        public static List<UserControlBase> CreatePluginInstance(string sFilePath, Type hostType = null)
        {
            List<UserControlBase> lUc = new List<UserControlBase>();
            try
            {
                lUc = ReflectionFactory<UserControlBase>.CreateInstance(sFilePath, new string[] { "Uc" }, hostType);
            }
            catch (Exception ex)
            {
                Console.WriteLine("CreateInstance: " + ex.Message);
            }

            return lUc;
        }
    }
}

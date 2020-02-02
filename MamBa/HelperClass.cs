using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace MamBa
{
    class HelperClass
    {
        //获取插件目录
        public static readonly string PlugInsDir = GetPathWithSlash(System.Environment.CurrentDirectory, true) + "Plugins\\";

        public delegate void DlgtVoidMethod();
        public delegate void DlgtVoidMethod_withParam(object obj);
        public static void RunInAdditionalThread(DlgtVoidMethod execMethod, DlgtVoidMethod_withParam onException = null, Control control = null)
        {
            if (control != null)
            {
                new Thread(delegate ()
                {
                    try
                    {
                        control.BeginInvoke(execMethod);
                    }
                    catch (Exception ex)
                    {
                        if (onException != null)
                        {
                            onException(ex);
                        }
                        else
                        {
                            Console.WriteLine(ex);
                        }
                    }
                })
                {
                    IsBackground = true
                }.Start();
            }
            else
            {
                ThreadPool.QueueUserWorkItem(delegate (object p)
                {
                    try
                    {
                        execMethod();
                    }
                    catch (Exception ex)
                    {
                        if (onException != null)
                        {
                            onException(ex);
                        }
                        Console.WriteLine(ex);
                    }
                }, null);
            }
        }

        public static void RunInAdditionalThread(DlgtVoidMethod_withParam execMethod, object param, DlgtVoidMethod_withParam onException = null, Control control = null)
        {
            if (control != null)
            {
                new Thread(delegate()
                {
                    try
                    {
                        control.BeginInvoke(execMethod, new object[]
                        {
                            param
                        });
                    }
                    catch (Exception ex)
                    {
                        if (onException != null)
                        {
                            onException(ex);
                        }
                        else
                        {
                            Console.WriteLine(ex);
                        }
                    }
                })
                {
                    IsBackground = true
                }.Start();
            }
            else
            {
                ThreadPool.QueueUserWorkItem(delegate (object p)
                {
                    try
                    {
                        execMethod(p);
                    }
                    catch (Exception ex)
                    {
                        if (onException != null)
                        {
                            onException(ex);
                        }
                        Console.WriteLine(ex);
                    }
                }, param);
            }
        }

        public static string GetPathWithSlash(string dirPath, bool createIfNotExists = false)
        {
            if (!Directory.Exists(dirPath) & createIfNotExists)
            {
                Directory.CreateDirectory(dirPath);
            }
            string result;
            if (dirPath.Trim().EndsWith("\\"))
            {
                result = dirPath.Trim();
            }
            else
            {
                result = dirPath.Trim() + "\\";
            }
            return result;
        }


    }

    public class ReflectionFactory<T>
    {
        /// <summary>
        /// 反射创建实例
        /// </summary>
        /// <param name="sFilePath"></param>
        /// <param name="typeFeature"></param>
        /// <param name="hostType"></param>
        /// <param name="dynamicLoad"></param>
        /// <returns></returns>
        public static List<T> CreateInstance(string sFilePath, string[] typeFeature, Type hostType = null, bool dynamicLoad = true)
        {
            var UCList = new List<T>();
            Assembly assemblyObj = null;

            if (!dynamicLoad)
            {
                #region 方法一：直接从DLL路径加载
                assemblyObj = Assembly.LoadFrom(sFilePath);
                #endregion
            }
            else
            {
                #region 方法二：先把DLL加载到内存，再从内存中加载（可在程序运行时动态更新dll文件，比借助AppDomain方便多了！）
                using (FileStream fs = new FileStream(sFilePath, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        byte[] bFile = br.ReadBytes((int)fs.Length);
                        br.Close();
                        fs.Close();
                        assemblyObj = Assembly.Load(bFile);
                    }
                }
                #endregion
            }

            if (assemblyObj != null)
            {
                #region 读取dll内的所有类，生成实例（这样可省去提供 命名空间 的步骤）
                // 程序集（命名空间）中的各种类
                foreach (Type type in assemblyObj.GetTypes())
                {
                    try
                    {
                        if (type.ToString().Contains("<>")) continue;
                        if (typeFeature != null)
                        {
                            bool invalidInstance = true;
                            foreach (var tf in typeFeature)
                            {
                                if (type.ToString().Contains(tf))   //必须包含指定的关键词
                                {
                                    invalidInstance = false;
                                    break;
                                }
                            }
                            if (invalidInstance) continue;
                        }

                        var uc = (T)assemblyObj.CreateInstance(type.ToString()); //反射创建 
                        UCList.Add(uc);

                        if (hostType != null)
                        {
                            AssemblyInfoHelper aih = new AssemblyInfoHelper(hostType);
                        }
                    }
                    catch (InvalidCastException icex)
                    {
                        Console.WriteLine(icex);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Create " + sFilePath + "(" + type.ToString() + ") occur " + ex.GetType().Name + ":\r\n" + ex.Message + (ex.InnerException != null ? "(" + ex.InnerException.Message + ")" : ""));
                    }
                }
                #endregion
            }

            return UCList;
        }
    }
}

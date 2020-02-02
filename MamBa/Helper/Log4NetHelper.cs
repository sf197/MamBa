using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MamBa
{
    partial class Form1
    {
        public static log4net.ILog loginfo = log4net.LogManager.GetLogger("Index");
        public static log4net.ILog logerror = log4net.LogManager.GetLogger("Index");


        public void Log(string info)
        {
            if (loginfo.IsInfoEnabled)
            {
                richTextBox1.AppendText(info + "\n");
                loginfo.Info(info);
            }
        }

        public void Log(string info, Exception ex)
        {
            if (logerror.IsErrorEnabled)
            {
                richTextBox1.AppendText(info + "\n");
                logerror.Error(info, ex);
            }
        }
    }
}

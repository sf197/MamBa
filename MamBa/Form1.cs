using PlugInProgram;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static MamBa.HelperClass;

namespace MamBa
{
    public partial class Form1 : Form
    {
        Dictionary<string, List<UserControlBase>> dicLoadedUCs = new Dictionary<string, List<UserControlBase>>();
        List<UserControlBase> lPlugIn = new List<UserControlBase>();
        Panel pnlUC
        {
            get
            {
                return panel1;
            }
        }

        public Form1()
        {
            InitializeComponent();
            if (!File.Exists(HelperClass.PlugInsDir)) {
                Directory.CreateDirectory(HelperClass.PlugInsDir);
            }
            panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            HelperClass.RunInAdditionalThread(new DlgtVoidMethod(Plugins_Load), null, this);
        }

        private void AboutMEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log("AboutME被访问");
            AboutME aboutme = new AboutME();
            aboutme.ShowIcon = false;
            aboutme.Show();
        }


        /// <summary>
        /// 加载插件
        /// </summary>
        public void Plugins_Load()
        {
            // 整理UI
            treeView1.Nodes.Clear();
            lPlugIn.Clear();
            dicLoadedUCs.Clear();

            #region 加载插件
            string[] DllFiles = Directory.GetFiles(HelperClass.PlugInsDir);
            string dllFile = "";
            for (int f = 0; f < DllFiles.Length; f++)
            {
                dllFile = DllFiles[f];

                FileInfo fi = new FileInfo(dllFile);
                if (!fi.Name.EndsWith(".dll")) continue;    //跳过非dll文件
                HelperClass.RunInAdditionalThread(new DlgtVoidMethod(() =>
                {
                    // 该部分在另一线程中完成，所以不会卡住当前窗体
                    //foreach (var uc in CreatePluginInstance(fi.FullName, this.GetType()))
                    foreach (var uc in Class1.CreatePluginInstance(fi.FullName, this.GetType()))

                    {
                        if (uc != null)
                        {
                            // 保存到已加载UC字典
                            if (!dicLoadedUCs.ContainsKey(uc.UCName))
                            {
                                dicLoadedUCs.Add(uc.UCName, new List<UserControlBase>());
                                dicLoadedUCs[uc.UCName].Add(uc);
                                lPlugIn.Add(uc);

                                // 这里通知窗体线程，加载到插件树控件中（供用户点击选择相应控件）
                                HelperClass.RunInAdditionalThread(new DlgtVoidMethod_withParam((Object obj) =>
                                {
                                    UserControlBase _uc = obj as UserControlBase;

                                    TreeNode _tn_ = null;
                                    foreach (TreeNode n in treeView1.Nodes)
                                    {
                                        if (n.Text == _uc.UCTpye)
                                        {
                                            _tn_ = n;
                                            break;
                                        }
                                    }
                                    if (_tn_ == null)
                                    {
                                        _tn_ = new TreeNode(_uc.UCTpye);
                                        treeView1.Nodes.Add(_tn_);
                                    }
                                    TreeNode _n_ = new TreeNode(_uc.UCName);
                                    _n_.ToolTipText = _uc.Recommend;
                                    _tn_.Nodes.Add(_n_);

                                    treeView1.ExpandAll();      //打开树中所有节点

                                    Log("App:"+"成功加载：" + _uc.UCName);
                                })
                                , uc
                                , new DlgtVoidMethod_withParam(delegate (Object oEx)
                                {
                                    MessageBox.Show((oEx as Exception).Message);
                                })
                                , treeView1);
                            }
                        }
                    }
                })
                , new DlgtVoidMethod_withParam(delegate (Object oEx)
                {
                    MessageBox.Show((oEx as Exception).Message);
                }));
            }
            #endregion
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode == null
                && treeView1.Nodes.Count > 0)
            {
                treeView1.SelectedNode = treeView1.Nodes[0];
            }
            if (treeView1.SelectedNode == null) return;

            if (treeView1.SelectedNode.Parent == null
                && treeView1.SelectedNode.Nodes.Count > 0)
            {
                treeView1.SelectedNode = treeView1.SelectedNode.Nodes[0];
            }

            foreach (var _uc in lPlugIn)
            {
                if (_uc.UCName == treeView1.SelectedNode.Text)
                {
                    HelperClass.RunInAdditionalThread(new DlgtVoidMethod(() =>
                    {
                        LoadUC(_uc, false);
                    }),
                    null,
                    pnlUC);
                    break;
                }
            }
        }

        void LoadUC(UserControlBase _uc, bool stayTabIdx)
        {
            #region 注入日志记录功能
            if (_uc.Log == null)
                _uc.Log = new DelgLog(delegate (string info)
                {
                    Log(info);
                });
            #endregion

            #region Panel加载
            pnlUC.Controls.Clear();
            HelperClass.RunInAdditionalThread(new DlgtVoidMethod_withParam(delegate (Object o)
            {
                pnlUC.Controls.Add((o as UserControlBase));
            })
                , _uc
                , new DlgtVoidMethod_withParam(delegate (Object oEx)
                {
                    MessageBox.Show((oEx as Exception).Message);
                })
                , pnlUC);
            #endregion

            this.Text = "MamBa（" + _uc.UCName + "）";
            if (_uc.UcIcon != null)
            {
                this.BeginInvoke(new DlgtVoidMethod_withParam(delegate (Object obj)
                {
                    this.Icon = obj as Icon;
                })
                , _uc.UcIcon);
            }
        }

        private void 如何使用ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/sf197/MamBa");
        }

        private void 版本信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("ManBa v1.0");
        }
    }
}

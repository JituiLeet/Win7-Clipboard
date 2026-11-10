using System;
using System.Drawing;
using System.Windows.Forms;
namespace Win7Clipboard {
 internal sealed class SettingsForm:Form {
  internal int Limit=100; internal bool English,Images=true,Files=true,Rich=true,Startup;
  NumericUpDown limit; CheckBox images,files,rich,startup; ComboBox lang;
  internal SettingsForm(int max,bool en,bool im,bool fi,bool rt,bool st){
   Limit=max;English=en;Images=im;Files=fi;Rich=rt;Startup=st;
   Text=en?"Settings":"设置";FormBorderStyle=FormBorderStyle.FixedDialog;StartPosition=FormStartPosition.CenterParent;MaximizeBox=false;MinimizeBox=false;ClientSize=new Size(350,260);
   images=Add(en?"Capture images":"记录图片",18,18,im);files=Add(en?"Capture files and folders":"记录文件和文件夹",18,50,fi);rich=Add(en?"Preserve HTML / RTF":"保留 HTML / RTF 富文本",18,82,rt);startup=Add(en?"Start with Windows":"开机启动",18,114,st);
   Controls.Add(new Label{Text=en?"History limit":"历史记录上限",Location=new Point(18,153),AutoSize=true});
   limit=new NumericUpDown{Minimum=10,Maximum=1000,Increment=10,Value=Math.Max(10,Math.Min(1000,max)),Location=new Point(210,149),Width=100};Controls.Add(limit);
   Controls.Add(new Label{Text=en?"Language":"语言",Location=new Point(18,188),AutoSize=true});
   lang=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,Location=new Point(210,184),Width=100};lang.Items.Add("简体中文");lang.Items.Add("English");lang.SelectedIndex=en?1:0;Controls.Add(lang);
   Button ok=new Button{Text=en?"Save":"保存",Location=new Point(145,220),DialogResult=DialogResult.OK};ok.Click+=delegate{Limit=(int)limit.Value;Images=images.Checked;Files=files.Checked;Rich=rich.Checked;Startup=startup.Checked;English=lang.SelectedIndex==1;};Controls.Add(ok);Button cancel=new Button{Text=en?"Cancel":"取消",Location=new Point(230,220),DialogResult=DialogResult.Cancel};Controls.Add(cancel);AcceptButton=ok;CancelButton=cancel;
  }
  CheckBox Add(string t,int x,int y,bool v){CheckBox c=new CheckBox{Text=t,Location=new Point(x,y),AutoSize=true,Checked=v};Controls.Add(c);return c;}
 }
}
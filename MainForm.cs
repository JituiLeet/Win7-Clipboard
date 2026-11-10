using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Collections.Specialized;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microsoft.Win32;
namespace Win7Clipboard {
 public sealed class MainForm:Form {
  const int HK=778; List<Entry> history=new List<Entry>(); string folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"Win7Clipboard");
  FlowLayoutPanel list=new FlowLayoutPanel(); TextBox search=new TextBox(); Label empty=new Label(); NotifyIcon tray=new NotifyIcon(); Timer capture=new Timer(), suppressTimer=new Timer();
  bool en=false,images=true,files=true,rich=true,startup=false,suppress=false,registered=false,pinnedTab=false; int limit=100; IntPtr pasteTarget;
  string HistoryPath{get{return Path.Combine(folder,"history.xml");}} string SettingsPath{get{return Path.Combine(folder,"settings.xml");}}
  public MainForm(){
   LoadData();Text="剪贴板";Size=new Size(410,610);MinimumSize=new Size(340,420);FormBorderStyle=FormBorderStyle.FixedToolWindow;StartPosition=FormStartPosition.Manual;ShowInTaskbar=false;TopMost=true;BackColor=Color.FromArgb(245,246,249);Font=new Font("Segoe UI",9);
   Build();SetupTray();capture.Interval=350;capture.Tick+=delegate{capture.Stop();CaptureClipboard();};suppressTimer.Interval=1200;suppressTimer.Tick+=delegate{suppressTimer.Stop();suppress=false;};
   Shown+=delegate{registered=Native.RegisterHotKey(Handle,HK,Native.MOD_WIN|Native.MOD_NOREPEAT,0x56);Native.AddClipboardFormatListener(Handle);Hide();};
   FormClosing+=delegate{if(registered)Native.UnregisterHotKey(Handle,HK);Native.RemoveClipboardFormatListener(Handle);SaveHistory();tray.Visible=false;tray.Dispose();};
  }
  void Build(){
   Panel head=new Panel{Dock=DockStyle.Top,Height=112,BackColor=Color.White};Controls.Add(head);
   Label title=new Label{Text=en?"Clipboard":"剪贴板",Font=new Font("Segoe UI",17,FontStyle.Bold),AutoSize=true,Location=new Point(15,10)};head.Controls.Add(title);
   Button set=new Button{Text="⚙",Location=new Point(302,12),Size=new Size(34,28),Anchor=AnchorStyles.Top|AnchorStyles.Right};set.Click+=delegate{OpenSettings();};head.Controls.Add(set);
   Button close=new Button{Text="×",Location=new Point(340,12),Size=new Size(34,28),Anchor=AnchorStyles.Top|AnchorStyles.Right};close.Click+=delegate{Hide();};head.Controls.Add(close);
   search.Location=new Point(15,55);search.Size=new Size(360,29);search.Anchor=AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Top;search.Text=en?"Search clipboard...":"搜索剪贴板...";search.ForeColor=Color.Gray;search.Tag=search.Text;search.GotFocus+=delegate{if((string)search.Tag==search.Text){search.Text="";search.ForeColor=Color.Black;}};search.LostFocus+=delegate{if(search.Text==""){search.Text=(string)search.Tag;search.ForeColor=Color.Gray;}};search.TextChanged+=delegate{Render();};head.Controls.Add(search);
   Panel bar=new Panel{Dock=DockStyle.Top,Height=42};Controls.Add(bar);
   AddBar(bar,en?"All":"全部",12,delegate{pinnedTab=false;Render();});AddBar(bar,en?"Pinned":"固定",75,delegate{pinnedTab=true;Render();});AddBar(bar,en?"Clear":"清空",145,Clear);
   list.Dock=DockStyle.Fill;list.AutoScroll=true;list.FlowDirection=FlowDirection.TopDown;list.WrapContents=false;list.Padding=new Padding(12,8,12,12);list.BackColor=BackColor;Controls.Add(list);
   empty.Dock=DockStyle.Fill;empty.TextAlign=ContentAlignment.MiddleCenter;empty.ForeColor=Color.Gray;Controls.Add(empty);Render();
  }
  void AddBar(Control p,string text,int x,Action action){Button b=new Button{Text=text,Location=new Point(x,6),Size=new Size(58,27)};b.Click+=delegate{action();};p.Controls.Add(b);}
  void SetupTray(){ContextMenuStrip m=new ContextMenuStrip();m.Items.Add(en?"Open":"打开剪贴板",null,delegate{ShowPanel();});m.Items.Add(en?"Settings":"设置",null,delegate{OpenSettings();});m.Items.Add(new ToolStripSeparator());m.Items.Add(en?"Exit":"退出",null,delegate{Close();Application.Exit();});tray.Icon=SystemIcons.Application;tray.Text="Clipboard";tray.ContextMenuStrip=m;tray.Visible=true;tray.DoubleClick+=delegate{ShowPanel();};}
  protected override void WndProc(ref Message m){if(m.Msg==Native.WM_HOTKEY&&m.WParam.ToInt32()==HK){if(Visible)Hide();else ShowPanel();return;}if(m.Msg==Native.WM_CLIPBOARDUPDATE&&!suppress){capture.Stop();capture.Start();}base.WndProc(ref m);}
  void ShowPanel(){pasteTarget=Native.GetForegroundWindow();Rectangle r=Screen.PrimaryScreen.WorkingArea;Location=new Point(Math.Max(r.Left,r.Right-Width-12),Math.Max(r.Top,r.Bottom-Height-12));Show();Activate();search.Focus();Render();}
  void CaptureClipboard(){try{
   IDataObject d=Clipboard.GetDataObject();if(d==null)return;Entry e=null;
   if(files&&d.GetDataPresent(DataFormats.FileDrop)){string[] a=d.GetData(DataFormats.FileDrop) as string[];if(a!=null&&a.Length>0)e=new Entry{Kind="Files",Files=a,Time=DateTime.Now};}
   if(e==null&&images&&d.GetDataPresent(DataFormats.Bitmap)){Image im=Clipboard.GetImage();if(im!=null)using(MemoryStream ms=new MemoryStream()){im.Save(ms,ImageFormat.Png);if(ms.Length<20*1024*1024)e=new Entry{Kind="Image",Image=ms.ToArray(),Time=DateTime.Now};}}
   string html=null,rtf=null;if(rich){try{if(d.GetDataPresent(DataFormats.Html))html=d.GetData(DataFormats.Html) as string;}catch{}try{if(d.GetDataPresent(DataFormats.Rtf))rtf=d.GetData(DataFormats.Rtf) as string;}catch{}}
   if(e==null&&(d.GetDataPresent(DataFormats.UnicodeText)||d.GetDataPresent(DataFormats.Text))){string t=Clipboard.ContainsText()?Clipboard.GetText():"";if(t.Length>0||html!=null||rtf!=null)e=new Entry{Kind="Text",Text=t,Html=html,Rtf=rtf,Time=DateTime.Now};}
   if(e==null)return;if(history.Count>0&&Same(history[0],e))return;e.Id=Guid.NewGuid().ToString("N");history.Insert(0,e);Trim();SaveHistory();if(Visible)Render();
  }catch{}}
  bool Same(Entry a,Entry b){if(a.Kind!=b.Kind)return false;if(a.Kind=="Text")return a.Text==b.Text&&a.Html==b.Html&&a.Rtf==b.Rtf;if(a.Kind=="Files")return String.Join("|",a.Files??new string[0])==String.Join("|",b.Files??new string[0]);return a.Image!=null&&b.Image!=null&&a.Image.Length==b.Image.Length;}
  void Trim(){while(history.Count>limit){int i=history.FindLastIndex(delegate(Entry e){return !e.Pinned;});if(i<0)break;history.RemoveAt(i);}}
  void Render(){if(list.IsDisposed)return;list.SuspendLayout();list.Controls.Clear();string q=search.Text==(string)search.Tag?"":search.Text;var items=history.Where(delegate(Entry e){return (!pinnedTab||e.Pinned)&&(String.IsNullOrEmpty(q)||(e.Preview??"").IndexOf(q,StringComparison.OrdinalIgnoreCase)>=0);}).ToList();empty.Visible=items.Count==0;empty.Text=history.Count==0?(en?"Copied items appear here":"复制的内容会显示在这里"):(en?"No matching items":"没有匹配的记录");
   foreach(Entry e in items){Panel c=new Panel{Width=Math.Max(285,list.ClientSize.Width-42),Height=e.Kind=="Image"?145:112,BackColor=Color.White,BorderStyle=BorderStyle.FixedSingle,Margin=new Padding(0,0,0,9)};
    Label p=new Label{Text=e.Preview,Location=new Point(11,8),Size=new Size(c.Width-22,62),AutoEllipsis=true,Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right,Cursor=Cursors.Hand};p.Click+=delegate{Paste(e);};c.Controls.Add(p);
    if(e.Kind=="Image"&&e.Image!=null)try{using(MemoryStream ms=new MemoryStream(e.Image))using(Image im=Image.FromStream(ms)){PictureBox pb=new PictureBox{Image=new Bitmap(im),Location=new Point(10,7),Size=new Size(130,75),SizeMode=PictureBoxSizeMode.Zoom,Cursor=Cursors.Hand};pb.Click+=delegate{Paste(e);};c.Controls.Add(pb);p.Visible=false;}}catch{}
    Label stamp=new Label{Text=e.Time.ToString("MM-dd HH:mm")+(e.Pinned?"  ★":""),ForeColor=Color.Gray,Location=new Point(11,c.Height-26),Size=new Size(140,18)};c.Controls.Add(stamp);
    Button pin=new Button{Text=e.Pinned?"★":"☆",Location=new Point(c.Width-82,c.Height-31),Size=new Size(32,25),Anchor=AnchorStyles.Right|AnchorStyles.Bottom};pin.Click+=delegate{e.Pinned=!e.Pinned;SaveHistory();Render();};c.Controls.Add(pin);
    Button del=new Button{Text="×",Location=new Point(c.Width-43,c.Height-31),Size=new Size(32,25),Anchor=AnchorStyles.Right|AnchorStyles.Bottom};del.Click+=delegate{history.Remove(e);SaveHistory();Render();};c.Controls.Add(del);
    ContextMenuStrip cm=new ContextMenuStrip();cm.Items.Add(en?"Paste":"粘贴",null,delegate{Paste(e);});cm.Items.Add(en?"Delete":"删除",null,delegate{history.Remove(e);SaveHistory();Render();});c.ContextMenuStrip=cm;p.ContextMenuStrip=cm;list.Controls.Add(c);
   }list.ResumeLayout();}
  void Paste(Entry e){try{suppress=true;DataObject d=new DataObject();if(e.Kind=="Files"){StringCollection sc=new StringCollection();sc.AddRange(e.Files??new string[0]);d.SetFileDropList(sc);}else if(e.Kind=="Image"){using(MemoryStream ms=new MemoryStream(e.Image))using(Image im=Image.FromStream(ms))d.SetImage(new Bitmap(im));}else{if(e.Text!=null)d.SetData(DataFormats.UnicodeText,true,e.Text);if(e.Html!=null)d.SetData(DataFormats.Html,true,e.Html);if(e.Rtf!=null)d.SetData(DataFormats.Rtf,true,e.Rtf);if(e.Text!=null)d.SetData(DataFormats.Text,true,e.Text);}Clipboard.SetDataObject(d,true);suppressTimer.Stop();suppressTimer.Start();Hide();if(pasteTarget!=IntPtr.Zero)Native.SetForegroundWindow(pasteTarget);Timer t=new Timer{Interval=180};t.Tick+=delegate{t.Stop();t.Dispose();Native.Paste();};t.Start();}catch{suppress=false;}}
  void Clear(){if(MessageBox.Show(en?"Delete all unpinned entries?":"确定清空所有未固定记录吗？",Text,MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;history.RemoveAll(delegate(Entry e){return !e.Pinned;});SaveHistory();Render();}
  void OpenSettings(){using(SettingsForm f=new SettingsForm(limit,en,images,files,rich,startup)){if(f.ShowDialog(this)!=DialogResult.OK)return;limit=f.Limit;en=f.English;images=f.Images;files=f.Files;rich=f.Rich;startup=f.Startup;}SaveSettings();ApplyStartup();if(registered)Native.UnregisterHotKey(Handle,HK);registered=Native.RegisterHotKey(Handle,HK,Native.MOD_WIN|Native.MOD_NOREPEAT,0x56);Text=en?"Clipboard":"剪贴板";if(search.Text==(string)search.Tag)search.Text=en?"Search clipboard...":"搜索剪贴板...";search.Tag=search.Text;tray.Text=Text;Render();}
  void ApplyStartup(){try{using(RegistryKey k=Registry.CurrentUser.OpenSubKey(@"Software\\Microsoft\\Windows\\CurrentVersion\\Run",true)){if(k==null)return;if(startup)k.SetValue("Win7Clipboard","\""+Application.ExecutablePath+"\"");else k.DeleteValue("Win7Clipboard",false);}}catch{}}
  void LoadData(){try{if(File.Exists(SettingsPath)){XmlSerializer s=new XmlSerializer(typeof(State));using(FileStream f=File.OpenRead(SettingsPath)){State x=(State)s.Deserialize(f);en=x.En;limit=x.Limit;images=x.Images;files=x.Files;rich=x.Rich;startup=x.Startup;}}}catch{}try{if(File.Exists(HistoryPath)){XmlSerializer s=new XmlSerializer(typeof(List<Entry>));using(FileStream f=File.OpenRead(HistoryPath))history=(List<Entry>)s.Deserialize(f);}}catch{history=new List<Entry>();}}
  void SaveHistory(){try{if(!Directory.Exists(folder))Directory.CreateDirectory(folder);string tmp=HistoryPath+".tmp";XmlSerializer s=new XmlSerializer(typeof(List<Entry>));using(FileStream f=File.Create(tmp))s.Serialize(f,history);if(File.Exists(HistoryPath))File.Delete(HistoryPath);File.Move(tmp,HistoryPath);}catch{}}
  void SaveSettings(){try{if(!Directory.Exists(folder))Directory.CreateDirectory(folder);XmlSerializer s=new XmlSerializer(typeof(State));using(FileStream f=File.Create(SettingsPath))s.Serialize(f,new State{En=en,Limit=limit,Images=images,Files=files,Rich=rich,Startup=startup});}catch{}}
  [Serializable] public class State{public bool En,Images,Files,Rich,Startup;public int Limit;}
 }
}
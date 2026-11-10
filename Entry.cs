using System;
namespace Win7Clipboard {
 [Serializable] public class Entry {
  public string Id,Kind,Text,Html,Rtf; public string[] Files; public byte[] Image; public DateTime Time; public bool Pinned;
  public string Preview { get { if(Kind=="Image") return "[图片 / Image]"; if(Kind=="Files") return Files==null?"Files":String.Join(Environment.NewLine,Files); string s=(Text??"").Replace("\r"," ").Replace("\n"," "); if(s.Length==0)s="[富文本 / Rich text]"; return s.Length>240?s.Substring(0,240)+"…":s; } }
 }
}
using System;
using System.Runtime.InteropServices;
namespace Win7Clipboard {
 internal static class Native {
  internal const int WM_HOTKEY=0x312, WM_CLIPBOARDUPDATE=0x31D, MOD_WIN=8, MOD_NOREPEAT=0x4000;
  [DllImport("user32.dll",SetLastError=true)] internal static extern bool RegisterHotKey(IntPtr h,int id,int mod,int key);
  [DllImport("user32.dll")] internal static extern bool UnregisterHotKey(IntPtr h,int id);
  [DllImport("user32.dll")] internal static extern bool AddClipboardFormatListener(IntPtr h);
  [DllImport("user32.dll")] internal static extern bool RemoveClipboardFormatListener(IntPtr h);
  [DllImport("user32.dll")] internal static extern IntPtr GetForegroundWindow();
  [DllImport("user32.dll")] internal static extern bool SetForegroundWindow(IntPtr h);
  [StructLayout(LayoutKind.Sequential)] struct INPUT { public uint type; public KEYBDINPUT ki; }
  [StructLayout(LayoutKind.Sequential)] struct KEYBDINPUT { public ushort vk,scan; public uint flags,time; public IntPtr extra; }
  [DllImport("user32.dll",SetLastError=true)] static extern uint SendInput(uint n, INPUT[] input,int size);
  internal static void Paste(){ INPUT[] a=new INPUT[4]; ushort[] k={0x11,0x56,0x56,0x11}; uint[] f={0,0,2,2}; for(int i=0;i<4;i++){a[i].type=1;a[i].ki.vk=k[i];a[i].ki.flags=f[i];} SendInput(4,a,Marshal.SizeOf(typeof(INPUT))); }
 }
}
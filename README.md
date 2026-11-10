# 剪贴板（Clipboard）

专为 Windows 7 SP1 设计的剪贴板历史工具，使用 C# Windows Forms 和 .NET Framework 4.0。

## 功能
- Win + V 打开或隐藏剪贴板面板
- 自动记录文本、位图图片、复制的文件和文件夹路径
- 在源应用提供格式时保留 HTML / RTF 数据
- 点击历史卡片恢复剪贴板，并向之前的窗口发送 Ctrl + V
- 固定、取消固定、删除单条记录、清空未固定记录
- 全部 / 固定筛选、文本搜索
- 历史上限 10–1000 条，固定记录不参与自动淘汰
- 系统托盘驻留，托盘菜单可打开、设置或退出
- 可选 Windows 登录启动
- 简体中文 / English
- XML 本地持久化

## 系统要求
Windows 7 SP1 或更高版本、.NET Framework 4.0。提供 x86 和 x64 构建。

## GitHub Actions
在 Actions 中手动运行 **Build Clipboard**，分别生成 x86/x64 ZIP 构建产物。正式发布时手动运行 **Release Clipboard**，输入 tag 和标题，工作流会创建 GitHub Release 并上传两个 ZIP。两个工作流均只使用 workflow_dispatch，不会因 push 或 pull_request 自动运行。

## 使用
解压后运行 Clipboard.exe，按 Win+V 打开。关闭面板不会退出程序，程序会留在系统托盘；从托盘菜单选择“退出”可完全退出。

## 数据位置
%AppData%\Win7Clipboard\

## 注意
- 文件历史保存文件路径，不会复制文件本身；原文件移动或删除后，文件记录可能失效。
- 富文本、图片格式能否完整恢复取决于复制来源和目标应用。
- Win+V 若被其他程序占用，快捷键注册会失败。
- 当前版本仍需在 Windows 7 SP1 实机验证；GitHub Actions 编译成功不等同于完成实机兼容性测试。
- 剪贴板可能包含密码及个人信息，历史数据保存在本机，请谨慎使用。
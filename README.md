# media-center
Application for organizing and watching pictures and videos

Technology stack:
.Net C# WPF MVVM

Third party tools and libs:
- ffmpeg is used to extract metadata from video files
- VLC is used for video playing. I use libvlc with the wrapper library Vlc.DotNet.Wpf (v2.2.1) [https://github.com/ZeBobo5/Vlc.DotNet]
- for displaying wf controls over a WindowsFormHost control (Airspace issue) I use a workaround that I found here [https://github.com/kolorowezworki/Airhack]


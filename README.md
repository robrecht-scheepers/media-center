# media-center
Application for organizing and watching pictures and videos

Technology stack:
C#, WPF (MVVM), SQLite

Third party tools and libs:
- ffmpeg is used to extract metadata from video files
- VLC is used for video playing. I use the library Vlc.DotNet.Wpf (v2.2.1) [https://github.com/ZeBobo5/Vlc.DotNet]
- for displaying wpf controls over the Vlc WindowsFormsHost control (Airspace issue) I use a workaround that I found here [https://github.com/kolorowezworki/Airhack]


![Logo](https://i.imgur.com/WXlq4hc.png)


## Description
WallBrite is a **desktop wallpaper utility** which syncs your wallpapers with the time of day. Add your wallpapers to its library and WallBrite will set **bright** ones during the **day** and **dark** ones at **night**!
![Diagram](https://i.imgur.com/DlSmbQW.png)\
Wallpapers are rotated throughout the day based on the average brightness of images and the time of day. The frequency of rotation, wallpapers in rotation, and times of day which are brightest/darkest are all configurable within the application!


## Installation
[Download latest release](https://github.com/MollyMayhem/WallBrite/releases/download/v1.0.1/WallBrite_Installer.msi)

Simply download the installer (WallBrite_Installer.msi) and run it. WallBrite will be installed with a Start Menu icon and desktop icon.


## Usage
### Getting Started
Upon opening WallBrite you will be greeted with the startup screen:
![Startup Screen](https://i.imgur.com/CbGE7M7.png)\
If you haven't already created a WallBrite library, get started by clicking **Add Files** or **Add Folder** on the left-side panel to add some images to the library.

For most people, **this should be it!**\
Once your images are in the library, you can close WallBrite and it will run in the background, rotating through your added wallpapers throughout the day.

### Automation Settings
If you want to change some of WallBrite's settings, click on the **Automation Settings** panel at the bottom of the screen. This will open up a panel with some useful information and settings:
![Settings Panel](https://i.imgur.com/mds1L4f.png)\
Here you can see some useful information about WallBrite's current status, and you can use this panel to change the update frequency, brightest/darkest time of day, and wallpaper "style" (fill, fit, tile, etc.).

### Library Controls
You can also use the **left-hand panel** to make changes to your library.\
The top three buttons let you open and save libraries, or create a new one. WallBrite **automatically saves your ongoing library** to an AppData folder and opens that file whenever you open WallBrite, so unless you want to have multiple libraries you won't need to use these buttons. :)\
You can also use this panel to add more images to your library or change how they're sorted on WallBrite's display.\
Using the selection area of this panel, you can also disable/enable wallpapers in the rotation, remove them from the library, or manually set one as the current wallpaper (note: **this will be overwritten by the next update!**)

### Taskbar Icon
When you close WallBrite, it will run in the background and be minimized to a **taskbar icon**:
![Taskbar Icon](https://i.imgur.com/YqTKJDG.png)\
You can use this icon to re-open WallBrite, actually exit it, or **toggle WallBrite to run at Windows startup**.


## Known Issues
- WallBrite ~~*may* crash when trying to add an ultra-large image file to the library (giving an Out of Memory exception).~~ will notify you if an image is too large to add to the library. This is due to the fact that WallBrite must convert image files into Bitmaps within .NET in order to easily pull data from pixels (i.e. get the average brightness of an image). For the most part, this shouldn't cause any problems since it only seems to occur with ULTRA large images (>~800MB) on the new 64-bit release but if you have an insanely large image you may not be able use it with WallBrite. Unfortunately, aside from manually pulling pixel data from raw image data (which would differ for each image format and would require a LOT of in-depth reading on how these formats work), I haven't found any workaround so I have to use the memory-inefficient Bitmap types in .NET. If anybody has any ideas for a workaround to this or could help me understand how to work with the raw image data in a simple way, please let me know! :)


## Credits
Me! :)\
Icons made by [Freepik](https://www.flaticon.com/authors/freepik) from [Flaticon](https://www.flaticon.com/)

## License
Released under GPL 3.0 license
GPL © Bradley Cross

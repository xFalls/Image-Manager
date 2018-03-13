# Image-Sorting-Software (name subject to change)

Work in progress - Bugs or unexpected behavior may occur

Drag a folder or file into the window to load them into the view. It will automatically display image files, contents of text files, or thumbnails of videos and other files. An interface showing all showfolders can be toggled, and selecting one of the folders will automatically send the currently displayed item into that folder as the next item gets loaded into view, allowing you to quickly sort through a large amount of files.

# Controls

All modes:
  - Scroll, Left, Right, A, D: Navigate to next or previous image
  - Right click, Space: Enables zooming and dragging an image with the mouse, opens a video in default player, or enables scrolling in a text file
  - Delete: Moves item to recycle bin
  - F: Toggle file previews
  - F1: Open help
  - F2: Rename file
  - F3: Quickly adds a preconfigured text in front of the name of the file
  - F4: Removes the preconfigured text
  - F11: Toggles fullscreen
  - F12: Open settings
  - M: Toggles endless scrolling mode
  - TAB, Middle click: Toggles the sorting menu
  - U: Upscales the current image through waifu2x
  - Z: Undo move or delete
  

In view mode:
- Home: Jump to first loaded image
- End: Jump to last loaded image
- +: Zoom in
- -: Zoom out
  
  
In sort mode:
- Up, Down, W, S: Select folder above or below
- Enter, R: Move currently displayed item to selected folder
- E: Loads the selected folder
- Control: Open typing mode
- Left click on folder: Loads the folder
- Right click on folder: Moves current item inside folder
  
 
In typing mode:
- Any character: Filter list of folders (disables most other shortcuts)
- Up, Down: Select folder above or below
- Enter: Move currently displayed item to selected folder
- Control: Close typing mode
  
  
When loading a new set of images:
- Shift: Only load the folder without the content of any subfolders
- X: Prevents loading of folders with specific preconfigured names
- C: Toggles showing files with the preset prefix
- V: Toggles showing non-media files

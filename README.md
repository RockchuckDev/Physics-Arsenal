# Physics Arsenal v0.2.0-alpha
![See it in action](https://rockchuckdev.github.io/Physics-Arsenal-Docs/assets/showcase.gif)
A toolkit that streamlines the process of adding physics based interactions between the player and the environment. Built for Godot .NET 4.7


# Features
- Grab and manipulate objects in a physics based way
- Independently grab objects with your left and right hands
- A first person character controller with many variables to customize
- Customizable behavior for grabbable objects
- Continued updates, as this toolkit is being developed for my own game


# Requirements
Godot .NET 4.7. Projects made with a non .NET build of Godot can be opened with the .NET build, so you don't have to start over to use this tookit

# Docs
Documentaion with setup instructions, API reference, object customization reference, roadmap, and more: [Physics Arsenal Documentation](https://rockchuckdev.github.io/Physics-Arsenal-Docs/)

If you find any bugs, or have any suggestions, open an [Issue](https://github.com/Rockchuck27/Godot-PhysKit/issues).

Any and all support keeps the lights on, gotta pay for college somehow :)
[Ko-fi](https://ko-fi.com/rockchuckdev)

---
# What This Project Taught Me
## Creating Readable and Polished Documentation
I wanted the documentation for this project to be approachable by someone unfamiliar with programming, so I knew that I couldn't settle for only adding comments to my code. I decided to use Quartz, an open source tool that turns Obsidian (.md) notes into professional looking webpages, both because I was already using Obsidian to document this project, and because it made creating links between documentation pages really easily.
On top of this, I tried to utilize screenshots whenever I could to get rid of as much ambiguity as possible.
## Decoupling
One thing I would change moving forward, would be to start decoupling systems from day one. Systems started to build up a hefty list of dependencies, and this caused making changes to be much more difficult, as I had to make sure that a change in one system didn't completely break another. One thing I did to alleviate this was making Event Bus Deluxe, which is essentially Godot Signals, but they operate on a "Fire and Forget" principle. Meaning that System A can send out a signal without needing to know which system will receive it, and System B can receive and process that signal without needing to worry about who sent it. This ended up saving a lot of time, but retrofitting it onto the tightly coupled systems was far more painful than if I had built it from day one.

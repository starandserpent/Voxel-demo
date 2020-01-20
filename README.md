# Voxel-demo
Demo implementation of Terra using Weltschmerz and Foreman.

The purpose of Voxel-demo is to demonstrate the capabilities of the voxel engine. Generates a voxel world for you to fool around with.

Written in C# using Godot game engine.

# Dependencies
- Godot's mono version + all dependencies.
- Git client

# How to install

1) Dowload and install Godot's Mono version with any required dependencies in a folder of your choice. We recommend using Godot version 3.1.2, for now, rather than the 3.2 branch or newer.
2) Launch an operating system console/terminal.
3) Run `git clone git@github.com:starandserpent/Voxel-demo.git --recursive` in a folder of your choice created for the purpose. If you get "command not found" error you need to install git command line client.
4) `cd Voxel-demo` to switch to the freshly cloned folder.
5) Run `dotnet restore` to make sure you have all dependencies. If you get "command not found" error, you have not setup your C# development environment properly. Setting it up is outside the scope of these instructions and you need to figure it out yourself.
6) Launch Godot Editor.
7) Click "Import" and select the folder where you cloned the Voxel-demo.

# Running the demo

1) Launch Godot Editor (if it's not already running).
2) Select the project from Godot Editor and double click it to open it.
3) When Godot Editor launches click "Play" button (or hit F5) to run.

The demo should now launch. You can play with the settings in the "GameController" node in Godot Editor before clicking "Play" to get different results.

In case there are any problems - let us know. 

# Contact us
https://discord.starandserpent.com/

# Seed

<p align="center">
    <img src="./Seed/Assets/seed-logo-blue-256.png">
</p>

<p align="center">
    A cross-platform launcher for the Flax game engine.
</p>

![image](https://github.com/user-attachments/assets/33533d92-7f90-4c17-b857-292c2e3a8e12)
![image](https://github.com/user-attachments/assets/32856c13-ee48-4211-80ff-12c46c313a0f)


## Features
- Manage multiple installations and platform tools of the Flax game engine, with support for:
    - Stable versions.
    - Nightly CI builds.
- Manage multiple projects:
    - Link your project to a specific engine version.
    - Set engine launch arguments per project.
    - Quick access to clearing the Cache folder.
- Create new projects easily!

## Templates
In addition to the built-in templates, the Seed launcher allows you to mark your own projects as templates, which means you can create multiple different projects and use them when creating a new project, for quicker prototyping and iteration!

[IMAGE HERE]

## Samples
Right from the launcher, you can download the official samples, which are stored as templates, allowing you to create new projects based on them without messing with the original sample files.

## Images

## Issues
Feel free to open issues for any bugs you find or missing features you'd like to see implemented!

## Building
To build the Seed launcher run the `dotnet build` command.

The binary can be found in `bin/Debug/[Net-Version]` (e.g `bin/Debug/Net7.0` )
which can be ran using `dotnet Launcher.dll` command.

For development, it is recommended to use `dotnet run` within the `Launcher` (case sensitive) directory.

## Contributing
Feel free to open PRs for bug fixes or small QOL changes but when deciding to work on bigger PRs, it's better to open an issue first to discuss them. Not doing so might result in wasted time on your part, if the PR doesn't align with the project's vision.

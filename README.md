# Seed

<p align="center">
    <img src="./Seed/Assets/seed-logo-blue-256.png">
</p>

<p align="center">
    A cross-platform launcher for the Flax game engine.
</p>

![image](https://github.com/MineBill/Seed/assets/30367251/99a2beb1-a3fa-403f-a573-9265d2111ff0)


## Features
- Manage multiple installations and platform tools of the Flax game engine, with support for:
    - Stable versions.
    - Nightly CI builds.
    - Local engine builds.
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
To build the Seed launcher run `dotnet build`

The binary can be found in `bin/Debug/[Net-Version]` (e.g `bin/Debug/Net7.0` )
which can be ran using `dotnet Seed.dll`

For development it is recommended to use `dotnet run` within the `Seed` directory

## Contributing
Feel free to open PRs for bug fixes or small QOL changes but when deciding to work on bigger PRs, it's better to open an issue first to discuss them. Not doing so might result in wasted time on your part, if the PR doesn't align with the project's vision.

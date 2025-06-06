#ifndef version
  #define version "0.0.0"
#endif

[Setup]
AppName=Seed
AppVersion={#version}
WizardStyle=modern
DefaultDirName={autopf}\Seed
UninstallDisplayIcon={app}\Launcher.exe
SetupIconFile=../Launcher/Assets/Images/logo.ico
OutputDir=Output
OutputBaseFilename=Seed_v{#version}

[Files]
Source: "Publish\Launcher.exe";         DestDir: "{app}"
Source: "Publish\av_libglesv2.dll";     DestDir: "{app}"
Source: "Publish\git2-a418d9d.dll";     DestDir: "{app}"
Source: "Publish\libHarfBuzzSharp.dll"; DestDir: "{app}"
Source: "Publish\libSkiaSharp.dll";     DestDir: "{app}"
Source: "Publish\NLog.config";          DestDir: "{app}"

[Icons]
Name: "{autoprograms}\Seed Launcher"; Filename: "{app}\Launcher.exe"
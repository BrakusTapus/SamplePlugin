> ⚠️ **Don't click Fork!**
> 
> This is a GitHub Template repo. If you want to use this for a plugin, just [use this template][new-repo] to make a new repo!
>
> ![image](https://github.com/goatcorp/SamplePlugin/assets/16760685/d9732094-e1ed-4769-a70b-58ed2b92580c)

# SamplePlugin

[![Use This Template badge](https://img.shields.io/badge/Use%20This%20Template-0?logo=github&labelColor=grey)][new-repo]


Simple example plugin for Dalamud.

This is not designed to be the simplest possible example, but it is also not designed to cover everything you might want to do. For more detailed questions, come ask in [the Discord](https://discord.gg/holdshift).

## Main Points

* Simple functional plugin
  * Slash command
  * Main UI
  * Settings UI
  * Image loading
  * Plugin json
* Simple, slightly-improved plugin configuration handling
* Project organization
  * Copies all necessary plugin files to the output directory
    * Does not copy dependencies that are provided by dalamud
    * Output directory can be zipped directly and have exactly what is required
  * Hides data files from visual studio to reduce clutter
    * Also allows having data files in different paths than VS would usually allow if done in the IDE directly


The intention is less that any of this is used directly in other projects, and more to show how similar things can be done.

## How To Use

### Getting Started

To begin, [clone this template repository][new-repo] to your own GitHub account. This will automatically bring in everything you need to get a jumpstart on development. You do not need to fork this repository unless you intend to contribute modifications to it.

Be sure to also check out the [Dalamud Developer Docs][dalamud-docs] for helpful information about building your own plugin. The Developer Docs includes helpful information about all sorts of things, including [how to submit][submit] your newly-created plugin to the official repository. Assuming you use this template repository, the provided project build configuration and license are already chosen to make everything a breeze.

[new-repo]: https://github.com/new?template_name=SamplePlugin&template_owner=goatcorp
[dalamud-docs]: https://dalamud.dev
[submit]: https://dalamud.dev/plugin-development/plugin-submission

### Prerequisites

SamplePlugin assumes all the following prerequisites are met:

* XIVLauncher, FINAL FANTASY XIV, and Dalamud have all been installed and the game has been run with Dalamud at least once.
* XIVLauncher is installed to its default directories and configurations.
  * If a custom path is required for Dalamud's dev directory, it must be set with the `DALAMUD_HOME` environment variable.
* A .NET Core 7 SDK has been installed and configured, or is otherwise available. (In most cases, the IDE will take care of this.)

### Building

1. Open up `SamplePlugin.sln` in your C# editor of choice (likely [Visual Studio 2022](https://visualstudio.microsoft.com) or [JetBrains Rider](https://www.jetbrains.com/rider/)).
2. Build the solution. By default, this will build a `Debug` build, but you can switch to `Release` in your IDE.
3. The resulting plugin can be found at `SamplePlugin/bin/x64/Debug/SamplePlugin.dll` (or `Release` if appropriate.)

### Activating in-game

1. Launch the game and use `/xlsettings` in chat or `xlsettings` in the Dalamud Console to open up the Dalamud settings.
    * In here, go to `Experimental`, and add the full path to the `SamplePlugin.dll` to the list of Dev Plugin Locations.
2. Next, use `/xlplugins` (chat) or `xlplugins` (console) to open up the Plugin Installer.
    * In here, go to `Dev Tools > Installed Dev Plugins`, and the `SamplePlugin` should be visible. Enable it.
3. You should now be able to use `/pmycommand` (chat) or `pmycommand` (console)!

Note that you only need to add it to the Dev Plugin Locations once (Step 1); it is preserved afterwards. You can disable, enable, or load your plugin on startup through the Plugin Installer.

### Reconfiguring for your own uses

Basically, just replace all references to `SamplePlugin` in all of the files and filenames with your desired name, then start building the plugin of your dreams. You'll figure it out 😁

Dalamud will load the JSON file (by default, `SamplePlugin/SamplePlugin.json`) next to your DLL and use it for metadata, including the description for your plugin in the Plugin Installer. Make sure to update this with information relevant to _your_ plugin!



### Extra info


The imported project does the following
<Import Project="$(DalamudLibPath)/targets/Dalamud.Plugin.targets"/>

<?xml version="1.0" encoding="utf-8"?>
<Project>
    <!-- PropertyGroup: Defines a group of properties for the project -->
    <PropertyGroup>
        <!-- TargetFramework: Specifies the target framework version for the project. Here, it's .NET 7.0 specifically for Windows -->
        <TargetFramework>net7.0-windows</TargetFramework>

        <!-- Platforms: Specifies the platform target, here it's x64 -->
        <Platforms>x64</Platforms>

        <!-- Nullable: Enables nullable reference types (C# 8.0 feature) -->
        <Nullable>enable</Nullable>

        <!-- LangVersion: Specifies the version of the C# language to use. 'latest' means the latest available version -->
        <LangVersion>latest</LangVersion>

        <!-- AllowUnsafeBlocks: Allows the use of unsafe code blocks in the project -->
        <AllowUnsafeBlocks>true</AllowUnsafeBlocks>

        <!-- ProduceReferenceAssembly: Indicates whether to produce a reference assembly. Set to false here -->
        <ProduceReferenceAssembly>false</ProduceReferenceAssembly>

        <!-- AppendTargetFrameworkToOutputPath: Determines if the target framework should be added to the output path. Set to false here -->
        <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>

        <!-- RestorePackagesWithLockFile: Enables the use of a packages lock file for consistent restore results -->
        <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>

        <!-- CopyLocalLockFileAssemblies: Copies the NuGet package assemblies locally with the lock file -->
        <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>

        <!-- AssemblySearchPaths: Defines additional paths to search for referenced assemblies -->
        <AssemblySearchPaths>$(AssemblySearchPaths);$(DalamudLibPath)</AssemblySearchPaths>
    </PropertyGroup>

    <!-- ItemGroup: Defines a group of items used in the project -->
    <ItemGroup>
        <!-- PackageReference: Specifies NuGet package references for the project -->
        <PackageReference Include="DalamudPackager" Version="2.1.12" />
        <!-- Reference: Specifies assembly references. 'Private=false' means they are not copied to the output directory -->
        <Reference Include="FFXIVClientStructs" Private="false" />
        <Reference Include="Newtonsoft.Json" Private="false" />
        <Reference Include="Dalamud" Private="false" />
        <Reference Include="ImGui.NET" Private="false" />
        <Reference Include="ImGuiScene" Private="false" />
        <Reference Include="Lumina" Private="false" />
        <Reference Include="Lumina.Excel" Private="false" />
    </ItemGroup>

    <!-- Target: Defines a build target with a specific action -->
    <Target Name="Message" BeforeTargets="BeforeBuild">
        <!-- Message: Displays a message during the build process. 'Importance=high' makes it more prominent -->
        <Message Text="Dalamud.Plugin: root at $(DalamudLibPath)" Importance="high" />
    </Target>
</Project>

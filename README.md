
<div align="center">
    <h1>
        <img width="70%" src="LogoBanner.png?raw=true" style="border-radius: 50%;" align="center">
        <br>
    </h1>
    <h3>A community-driven server emulator, written completely from scratch in C#, for MQ!</h3>
    <p>
        <a href="https://github.com/FeroxFoxxo/MQReawakened/blob/master/LICENSE.md">
            <img alt="License" src="https://img.shields.io/github/license/feroxfoxxo/mqreawakened?label=License&style=for-the-badge">
        </a>
        <a href="https://github.com/FeroxFoxxo/MQReawakened/wiki">
            <img alt="Wiki" src="https://img.shields.io/badge/Wiki-View%20Docs-purple?style=for-the-badge">
        </a>
        <a href="https://github.com/FeroxFoxxo/MQReawakened/projects">
            <img alt="Wiki" src="https://img.shields.io/badge/To%20Do-View%20Projects-darkgreen?style=for-the-badge">
        </a>
        <a href="https://github.com/FeroxFoxxo/MQReawakened">
            <img alt="Stars" src="https://img.shields.io/github/stars/feroxfoxxo/MQReawakened?color=gold&style=for-the-badge">
        </a>
    </p>
    <h4>This server is completely free from the original game's assets / intellectual property.</h4>
    <h5>None of the repo, the tool, nor the repo owner is affiliated with or sponsored by any affiliates of the original game.</h4>
    <h1></h1>
</div>

### Information

If you encounter any issues with the code, please submit an issue through [the repo's issue system](https://github.com/FeroxFoxxo/MQReawakened/issues), and it will be assigned to someone accordingly.

If you need to report anything about this repo, or otherwise reach out to me, you can find me at feroxfoxxo@gmail.com.

## Contributing

If you'd like to contribute to this project, please read [CONTRIBUTING.md](CONTRIBUTING.md).

### Prerequisites

- You **must** provide your *own* copy of the game and its associated asset bundles.
- You **must** provide your *own* associated DLLs from the game, which are to be added to the server itself on compile - ensuring this repo does not contain any copywritten code.

## How to set up MQReawakened

Please read the [developer guide here on how to set up MQReawakened](https://github.com/FeroxFoxxo/MQReawakened/wiki/Setting-Up-The-Development-Environment).

If you want to play 2012 or earlier (NOT RECOMMENDED), please use the *new and improved!* [MQClient found here](https://github.com/FeroxFoxxo/MQClient).

Otherwise, simply edit the `settings.txt` and `game/LocalBuildConfig.xml` files from the original game client to point to the MQReawakened server you are trying to connect to, replacing the default supplied.

## Gameplay

The project's goal is to faithfully recreate the game as it was at the time of the targeted build.
While most features are implemented and the game is playable from start to finish, functionality or bugs may be missing.

When hosting a local server, you can access all commands by default (account level: owner).

## Architecture

MQ consists of the following components:

* A login server and asset hosting service over HTTP
* A shard server that speaks the MQ network protocol over TCP

#### For 2012
* A web browser compatible with the old NPAPI plugin interface
* A `.unity3d` bundle that contains the game code and essential resources (loading screen, etc.)

#### For 2013/2014
* A Unity executable file, typically in the /game/ folder, and a launcher.

Both the login and shard server run on the same Asp.Net application, as seen in this git repository.

The original game used the player's actual web browser to launch the game. Still, since then, the NPAPI plugin interface the game relied on has been deprecated and is no longer available in most modern browsers. MQR gets around this issue by distributing an older version of Electron, a specialised web browser software package.

The browser/Electron client opens a web page with an `<embed>` tag of the appropriate MIME type, where the `src` param is the address of the game's `.unity3d` entry point. This triggers the browser to load an NPAPI plugin that handles said MIME type, in this case, the Unity Web Player.

*(similarly to https://github.com/OpenFusionProject/OpenFusion)*

### Other information

- You can also download ILSpy to peek around any dependencies of the original game, understand how they work, and reverse engineer them to develop a server-side implementation.
- https://sourceforge.net/projects/ilspy.mirror

### Running the game client on Linux

While the MQ server supports Windows and Linux (and other Unix-like systems), the game client natively supports only Windows because of the NPAPI Unity Web Player plugin needed to run the game. Nevertheless, the client runs very well in Wine if appropriately configured.

Due to the plethora of Wine prefix managers that people use (in addition to the option of just configuring your Wine prefix by hand), you could set the game up in several ways. Regardless of which you prefer, for MQ, there's a handful of dependencies you need to satisfy:

- Electron (MQClient.exe) needs all fonts to run
- It also needs to be run with the following arguments: --no-sandbox --disable-gpu
- Using DXVK instead of wined3d is highly recommended to avoid graphical glitches like mission indicator rings not rendering

While we have not written a complete guide of how to do this yet, visiting <a href="https://github.com/OpenFusionProject/OpenFusion/wiki/Running-the-game-client-on-Linux">OpenFusion's guide in how to do this</a>, another Unity WebPlayer MMO, may provide a good enough understanding of where to start.

### Open Source Licenses

- <a href="https://github.com/AssetRipper/AssetRipper/">AssetRipper.IO.Endian</a> - Binary read/write utility [LICENSE](https://github.com/FeroxFoxxo/MQReawakened/blob/main/Web.AssetBundles/Licences/LICENSE_ASSETRIPPER)
- <a href="https://github.com/Perfare/AssetStudio/">AssetStudio</a> - Tool for exploring the original assetbundles [LICENSE](https://github.com/FeroxFoxxo/MQReawakened/blob/main/Web.AssetBundles/Licences/LICENSE_ASSETSTUDIO)
- <a href="https://github.com/ServUO/ServUO/">ServUO</a> - Server emulator used as a base, written in C# .NET [LICENSE](https://github.com/FeroxFoxxo/MQReawakened/blob/main/Server.Base/Licences/LICENSE_SERVUO)
- <a href="https://github.com/apache/thrift/">Thrift</a> - Point-to-point RPC implementation [LICENSE](https://github.com/FeroxFoxxo/MQReawakened/blob/main/Server.Reawakened/Licences/LICENSE_THRIFT)
- <a href="https://github.com/discord-net/Discord.Net/">Discord.Net</a> - Reflective class initialization [LICENSE](https://github.com/FeroxFoxxo/MQReawakened/blob/main/Server.Reawakened/Licences/LICENSE_DNET)

### Commemorations

- Z6mbie - Logo designer.
- <a href="https://github.com/victti/">Victti</a> - Concept contributor & mentor.

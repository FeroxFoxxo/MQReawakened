
<div align="center">
    <h1>
        <img width="70%" src="LogoBanner.png?raw=true" style="border-radius: 50%;" align="center">
        <br>
    </h1>
    <h3>A community driven server emulator, written completely from scratch in C#, for MQ!</h3>
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
    <h4>This server emulator is completely free of assets / intellectual property from the original game. If you see any infringing code, please let me know at the email below and I will take it down to be removed ❤️</h4>
    <h5>None of the repo, the tool, nor the repo owner is affiliated with, sponsored, or authorized by any affiliates of the original game.</h4>
    <h1></h1>
</div>

### Information

MQReawakened is a community-driven server emulator meant for **non-commercial, educational use only**. It is meant to show how games like these are created, through reverse engineering how they work. It is not, and should never be, commercialised in any way. It is fundamentally transformative from the original client, as it handles the inverse of requests, and only shares similar data models. Users **must provide** the original game's code, assets, executables etc themselves.

Please email me at feroxfoxxo@gmail.com if anything is in breach, which will be rectified ASAP.

#### Developer information

We've moved to using [Discord](https://discord.gg/gSbZ8apE6V) to handle our to-do list!

If you have anything you wish to report, please submit an issue through [the repo's issue system](https://github.com/FeroxFoxxo/MQReawakened/issues), and it will be assigned to someone accordingly.

## Contributing

If you'd like to contribute to this project, please read [CONTRIBUTING.md](CONTRIBUTING.md).

### Prerequisites

- You **must** have your own copy of the game and its associated asset bundles.
- You **must** have the associated DLL for the game added to your server project, as to ensure it doesn't contain any copywritten code itself.

## Gameplay

The goal of the project is to faithfully recreate the game as it was at the time of the targeted build.
While most features are implemented and the game is playable start to finish, there may be missing functionality or bugs present.

When hosting a local server, you will have access to all commands by default (account level: owner).

## Architecture

MQ consists of the following components:

* A web browser compatible with the old NPAPI plugin interface
* A `.unity3d` bundle that contains the game code and essential resources (loading screen, etc.)
* A login server that speaks the MQ network protocol over TCP
* A shard server that does the same on another port

Both the login and shard server run on the same Asp.Net server application, seen in this git repository.

The original game made use of the player's actual web browser to launch the game, but since then the NPAPI plugin interface the game relied on has been deprecated and is no longer available in most modern browsers. MQ gets around this issue by distributing an older version of Electron, a software package that is essentially a specialized web browser.

The browser/Electron client opens a web page with an `<embed>` tag of the appropriate MIME type, where the `src` param is the address of the game's `.unity3d` entrypoint. This triggers the browser to load an NPAPI plugin that handles said MIME type, in this case the Unity Web Player.

*(similarly to https://github.com/OpenFusionProject/OpenFusion)*

### Other information

- You may also wish to download ILSpy to peek around any dependencies of the original game, to understand how they work, as to reverse engineer them in order to develop a server side implementation.
- https://sourceforge.net/projects/ilspy.mirror

### Open Source Licenses

- <a href="https://github.com/AssetRipper/AssetRipper/">AssetRipper.IO.Endian</a> - Binary read/write utility [LICENSE](https://github.com/FeroxFoxxo/MQReawakened/blob/main/Web.AssetBundles/Licences/LICENSE_ASSETRIPPER)
- <a href="https://github.com/Perfare/AssetStudio/">AssetStudio</a> - Tool for exploring the original assetbundles [LICENSE](https://github.com/FeroxFoxxo/MQReawakened/blob/main/Web.AssetBundles/Licences/LICENSE_ASSETSTUDIO)
- <a href="https://github.com/ServUO/ServUO/">ServUO</a> - Server emulator used as a base, written in C# .NET [LICENSE](https://github.com/FeroxFoxxo/MQReawakened/blob/main/Server.Base/Licences/LICENSE_SERVUO)
- <a href="https://github.com/apache/thrift/">Thrift</a> - Point-to-point RPC implementation [LICENSE](https://github.com/FeroxFoxxo/MQReawakened/blob/main/Server.Reawakened/Licences/LICENSE_THRIFT)
- <a href="https://github.com/discord-net/Discord.Net/">Discord.Net</a> - Reflective class initialization [LICENSE](https://github.com/FeroxFoxxo/MQReawakened/blob/main/Server.Reawakened/Licences/LICENSE_DNET)

### Commemorations

- Z6mbie - Logo designer.
- <a href="https://github.com/victti/">Victti</a> - Concept contributor & mentor.

<div align="center">
    <h1>
        <img width="70%" src="LogoBanner.png?raw=true" style="border-radius: 50%;" align="center">
        <br>
    </h1>
    <h3>Asset Loader Guide</h3>
    <h4>This project is completely free from the original game's assets / intellectual property.</h4>
    <h5>None of the repo, the tool, nor the repo owner is affiliated with or sponsored by any affiliates of the original game.</h5>
    <h1></h1>
</div>

## Welcome

This guide will show you how to switch the asset loader system, reawakened has two different asset systems: Default and Custom

- Default - This asset loader only allows original game content
- Custom - This asset loader allows changing all original game xml data and adding custom content on top of MQ if you have a decompiled client that can make asset bundles

> [!IMPORTANT]
> The `generateFixedBundles` command can only be run from a development environment or the MQRLauncher. 
> Docker can still be used as the final hosted server with UseCustomAssetLoader enabled and the first start after with FORCE_REBUILD enabled.

## TL;DR

- Setup a server from the [Setup Launcher Guide](SETUP_LAUNCHER.md)
- Once the server is setup run `generateFixedBundles` and wait for it to finish
- These fixed bundles can be found in `MQData/Game/Data/Assets/FixedBundles`
- Create a new folder called `MQBundles` (name doesn't matter) with this folder structure: Bundles, Levels and XMLs 
- Copy the fixed bundles to the Bundles folder inside of MQBundles
- Find the assetDictionary/PublishConfiguration files in `MQData/Game/Data/Assets/AssetDictionaries`
- Copy these files both normal .xml and .VGMT.xml to the Bundles folder inside of MQBundles
- Copy the XML files from `MQData/Game/Data/XMLs/XMLFiles` to the XMLs folder inside of MQBundles
- Copy the Level files from `MQData/Game/Data/XMLs/Levels` to the Levels folder inside of MQBundles
- Set `UseCustomAssetLoader` to true in `MQData/Game/Data/Configs/AssetBundle.json`
- Enable `FORCE_REBUILD` in the environment variables
- Restart the server
- Disable `FORCE_REBUILD` in the environment variables

## Prerequisites

- A MQReawakened Server
- Original game files

## How to obtain required files

> [!IMPORTANT]
> You must run the server using the default asset loader at least once for these files to exist.
> You can get the fixed asset bundles by running `generateFixedBundles` in the server console.

- The fixed asset bundles can be found in `MQData/Game/Data/Assets/FixedBundles`
- You can find the AssetDictionaries and PublishConfiguration xml files in `MQData/Game/Data/Assets/AssetDictionaries`
- You can find the XML files in `MQData/Game/Data/XMLs/Levels` and `MQData/Game/Data/XMLs/XMLFiles`

What you want to do is copy those files to the correct folders in the File Placement section, then enable `FORCE_REBUILD` and restart the server and disable it after.

## File Placement

You will want to create a new folder called MQBundles along with the other 3 below.

- `MQBundles/Bundles` - All asset bundles, asset dictionaries and publish configuration files
- `MQBundles/Levels` - All Level/Trail XML files
- `MQBundles/XMLs` - All Game Data XML files

![example](https://cdn.discordapp.com/attachments/1186024656227024966/1543368650998943814/image.png?ex=6a974090&is=6a95ef10&hm=3e8e95a5702bcd94583859e36314fb83e835da999a0fb96835f7a0550623277c&)

## How to enable

Go to the `MQData/Game/Data/Configs` folder then open `AssetBundle.json` .json may not be present depending on your file explorer settings.

Copy this and set `CustomAssetLoader` to true then save the file.
```JSON
  "CacheInfoFile": "",
  "WebPlayerInfoFile": "",
  "FlushCacheOnStart": true,
  "LogProgressBars": false,
  "CustomAssetLoader": false
```

Now you will want to go into your .env file for your server and enable the `FORCE_REBUILD` variable by setting it to 1 then restart the server.

After restarting your server disable `FORCE_REBUILD` by setting it to 0.

You will be prompted to find the `assetDictionary.xml` file inside of `MQBundles/Bundles` double click on that file it then should load the server.
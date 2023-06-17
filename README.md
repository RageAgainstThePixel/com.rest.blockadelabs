# com.rest.blockadelabs

[![Discord](https://img.shields.io/discord/855294214065487932.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/xQgMW9ufN4) [![openupm](https://img.shields.io/npm/v/com.rest.blockadelabs?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.rest.blockadelabs/) [![openupm](https://img.shields.io/badge/dynamic/json?color=brightgreen&label=downloads&query=%24.downloads&suffix=%2Fmonth&url=https%3A%2F%2Fpackage.openupm.com%2Fdownloads%2Fpoint%2Flast-month%2Fcom.rest.blockadelabs)](https://openupm.com/packages/com.rest.blockadelabs/)

A non-official [BlockadeLabs](https://www.blockadelabs.com/) Skybox AI RESTful client for the [Unity](https://unity.com/) Game Engine.

I am not affiliated with BlockadeLabs and an account with api access is required.

***All copyrights, trademarks, logos, and assets are the property of their respective owners.***

## Installing

Requires Unity 2021.3 LTS or higher.

The recommended installation method is though the unity package manager and [OpenUPM](https://openupm.com/packages/com.openai.unity).

### Via Unity Package Manager and OpenUPM

- Open your Unity project settings
- Select the `Package Manager`
![scoped-registries](BlockadeLabs/Packages/com.rest.blockadelabs/Documentation~/images/package-manager-scopes.png)
- Add the OpenUPM package registry:
  - Name: `OpenUPM`
  - URL: `https://package.openupm.com`
  - Scope(s):
    - `com.rest.blockadelabs`
    - `com.utilities`
- Open the Unity Package Manager window
- Change the Registry from Unity to `My Registries`
- Add the `BlockadeLabs` package

### Via Unity Package Manager and Git url

- Open your Unity Package Manager
- Add package from git url: `https://github.com/RageAgainstThePixel/com.rest.blockadelabs.git#upm`
  > Note: this repo has dependencies on other repositories! You are responsible for adding these on your own.
  - [com.utilities.async](https://github.com/RageAgainstThePixel/com.utilities.async)
  - [com.utilities.rest](https://github.com/RageAgainstThePixel/com.utilities.rest)

## Documentation

### Table of Contents

- [Authentication](#authentication)
- [Skyboxes](#skyboxes)
  - [Get Skybox Styles](#get-skybox-styles)
  - [Generate Skybox](#generate-skybox)
  - [Get Skybox by Id](#get-skybox)

### Authentication

There are 4 ways to provide your API keys, in order of precedence:

1. [Pass keys directly with constructor](#pass-keys-directly-with-constructor)
2. [Unity Scriptable Object](#unity-scriptable-object)
3. [Load key from configuration file](#load-key-from-configuration-file)
4. [Use System Environment Variables](#use-system-environment-variables)

#### Pass keys directly with constructor

```csharp
var api = new BlockadeLabsClient("yourApiKey");
```

Or create a `BlockadeLabsAuthentication` object manually

```csharp
var api = new BlockadeLabsClient(new BlockadeLabsAuthentication("yourApiKey"));
```

#### Unity Scriptable Object

You can save the key directly into a scriptable object that is located in the `Assets/Resources` folder.

You can create a new one by using the context menu of the project pane and creating a new `BlockadeLabsConfiguration` scriptable object.

![Create new BlockadeLabsConfiguration](BlockadeLabs/Packages/com.rest.blockadelabs/Documentation~/images/create-scriptable-object.png)

#### Load key from configuration file

Attempts to load api keys from a configuration file, by default `.blockadelabs` in the current directory, optionally traversing up the directory tree or in the user's home directory.

To create a configuration file, create a new text file named `.blockadelabs` and containing the line:

##### Json format

```json
{
  "apiKey": "yourApiKey",
}
```

You can also load the file directly with known path by calling a static method in Authentication:

```csharp
var api = new BlockadeLabsClient(BlockadeLabsAuthentication.Default.LoadFromDirectory("your/path/to/.blockadelabs"));;
```

#### Use System Environment Variables

Use your system's environment variables specify an api key to use.

- Use `BLOCKADE_LABS_API_KEY` for your api key.

```csharp
var api = new BlockadeLabsClient(BlockadeLabsAuthentication.Default.LoadFromEnvironment());
```

### Skyboxes

#### [Get Skybox Styles](https://blockade.cloudshell.run/redoc#tag/skybox/operation/Get_Skybox_Styles_api_v1_skybox_styles_get)

Returns the list of predefined styles that can influence the overall aesthetic of your skybox generation.

```csharp
var api = new BlockadeLabsClient();
var skyboxStyles = await api.SkyboxEndpoint.GetSkyboxStylesAsync();

foreach (var skyboxStyle in skyboxStyles)
{
    Debug.Log($"{skyboxStyle.Name}");
}
```

#### [Generate Skybox](https://blockade.cloudshell.run/redoc#tag/skybox/operation/Generate_Skybox_api_v1_skybox_generate_post)

Generate a skybox image

```csharp
var api = new BlockadeLabsClient();
var request = new SkyboxRequest("underwater", depth: true);
var skyboxInfo = await api.SkyboxEndpoint.GenerateSkyboxAsync(request);
skyboxMaterial.mainTexture = skyboxInfo.MainTexture;
skyboxMaterial.depthTexture = skyboxInfo.DepthTexture;
```

#### [Get Skybox](https://blockade.cloudshell.run/redoc#tag/skybox/operation/Get_Skybox_By_Id_api_v1_skybox_info__id__get)

Returns the skybox metadata for the given skybox id.

```csharp
var skyboxInfo = await api.SkyboxEndpoint.GetSkyboxInfoAsync("skybox-id");
Debug.Log($"Skybox: {result.Id} | {result.MainTextureUrl}");
```

# com.rest.blockadelabs

[![Discord](https://img.shields.io/discord/855294214065487932.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/xQgMW9ufN4) [![openupm](https://img.shields.io/npm/v/com.rest.blockadelabs?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.rest.blockadelabs/) [![openupm](https://img.shields.io/badge/dynamic/json?color=brightgreen&label=downloads&query=%24.downloads&suffix=%2Fmonth&url=https%3A%2F%2Fpackage.openupm.com%2Fdownloads%2Fpoint%2Flast-month%2Fcom.rest.blockadelabs)](https://openupm.com/packages/com.rest.blockadelabs/)

A non-official [BlockadeLabs](https://www.blockadelabs.com/) Skybox AI RESTful client for the [Unity](https://unity.com/) Game Engine.

I am not affiliated with BlockadeLabs and an account with api access is required.

***All copyrights, trademarks, logos, and assets are the property of their respective owners.***

## Installing

Requires Unity 2021.3 LTS or higher.

The recommended installation method is though the unity package manager and [OpenUPM](https://openupm.com/packages/com.rest.blockadelabs).

### Via Unity Package Manager and OpenUPM

- Open your Unity project settings
- Select the `Package Manager`
![scoped-registries](images/package-manager-scopes.png)
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
  - [com.utilities.extensions](https://github.com/RageAgainstThePixel/com.utilities.extensions)
  - [com.utilities.rest](https://github.com/RageAgainstThePixel/com.utilities.rest)

## Documentation

### Table of Contents

- [Authentication](#authentication)
- [Editor Dashboard](#editor-dashboard)
  - [Skybox Dashboard](#skybox-dashboard)
  - [History Dashboard](#history-dashboard)
- [Skyboxes](#skyboxes)
  - [Get Skybox Styles](#get-skybox-styles)
  - [Get Skybox Style Families](#get-skybox-style-families)
  - [Get Skybox Export Options](#get-skybox-export-options)
  - [Generate Skybox](#generate-skybox) :construction: :warning:
  - [Get Skybox by Id](#get-skybox)
  - [Request Skybox Export](#request-skybox-export)
  - [Delete Skybox by Id](#delete-skybox)
  - [Get Skybox History](#get-skybox-history)
  - [Cancel Skybox Generation](#cancel-skybox-generation)
  - [Cancel All Pending Skybox Generations](#cancel-all-pending-skybox-generations)

### Authentication

There are 4 ways to provide your API keys, in order of precedence:

> [!WARNING]
> We recommended using the environment variables to load the API key instead of having it hard coded in your source. It is not recommended use this method in production, but only for accepting user credentials, local testing and quick start scenarios.

1. [Pass keys directly with constructor](#pass-keys-directly-with-constructor) :warning:
2. [Unity Scriptable Object](#unity-scriptable-object) :warning:
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

![Create new BlockadeLabsConfiguration](images/create-scriptable-object.png)

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
var api = new BlockadeLabsClient(new BlockadeLabsAuthentication().LoadFromDirectory("your/path/to/.blockadelabs"));;
```

#### Use System Environment Variables

Use your system's environment variables specify an api key to use.

- Use `BLOCKADE_LABS_API_KEY` for your api key.

```csharp
var api = new BlockadeLabsClient(new BlockadeLabsAuthentication().LoadFromEnvironment());
```

### Editor Dashboard

You can perform all of the same actions from the BlockadeLabs website, in the Editor using the BlockadeLabs Dashboard!

`Window/Dashboards/BlockadeLabs`

![dashboard](images/dashboard.png)

#### Skybox Dashboard

Generate skyboxes and remix them directly in the editor.

![History](images/skybox-editor.png)

#### History Dashboard

You also have access to the full list of all your generated skyboxes, ready for downloading directly into your project.

![History](images/history-editor.png)

### Skyboxes

#### [Get Skybox Styles](https://api-documentation.blockadelabs.com/api/skybox.html#get-skybox-styles)

Returns the list of predefined styles that can influence the overall aesthetic of your skybox generation.

```csharp
var api = new BlockadeLabsClient();
var skyboxStyles = await api.SkyboxEndpoint.GetSkyboxStylesAsync(SkyboxModel.Model3);

foreach (var skyboxStyle in skyboxStyles)
{
    Debug.Log($"{skyboxStyle.Name}");
}
```

#### [Get Skybox Style Families](https://api-documentation.blockadelabs.com/api/skybox.html#get-skybox-style-families)

Returns the list of predefined styles that can influence the overall aesthetic of your skybox generation, sorted by style family. This route can be used in order to build a menu of styles sorted by family.

```csharp
var api = new BlockadeLabsClient();
var skyboxFamilyStyles = await BlockadeLabsClient.SkyboxEndpoint.GetSkyboxStyleFamiliesAsync(SkyboxModel.Model3);

foreach (var skyboxStyle in skyboxFamilyStyles)
{
    Debug.Log($"{skyboxStyle.Name}");
}
```

#### [Get Skybox Export Options](https://api-documentation.blockadelabs.com/api/skybox-exports.html#get-export-types)

Returns the list of all available export types.

```csharp
var api = new BlockadeLabsClient();
var exportOptions = await api.SkyboxEndpoint.GetAllSkyboxExportOptionsAsync();

foreach (var exportOption in exportOptions)
{
    Debug.Log($"{exportOption.Id}: {exportOption.Name} | {exportOption.Key}");
}

var request = new SkyboxRequest("mars", enhancePrompt: true);
// Generates ALL export options for the skybox
var skyboxInfo = await api.SkyboxEndpoint.GenerateSkyboxAsync(request, exportOptions);
Debug.Log($"Successfully created skybox: {skyboxInfo.Id}");
```

#### [Generate Skybox](https://api-documentation.blockadelabs.com/api/skybox.html#generate-skybox)

Generate a skybox.

```csharp
var api = new BlockadeLabsClient();
var skyboxStyles = await BlockadeLabsClient.SkyboxEndpoint.GetSkyboxStylesAsync(SkyboxModel.Model3);
var request = new SkyboxRequest(skyboxStyles.First(), "mars", enhancePrompt: true);

// You can also get progress callbacks when the generation progress has changed/updated
var progress = new Progress<SkyboxInfo>(async progress =>
{
    Debug.Log(progress);
});

var skyboxInfo = await api.SkyboxEndpoint.GenerateSkyboxAsync(request, progressCallback: progress);
Debug.Log($"Successfully created skybox: {skyboxInfo.Id}");

if (skyboxInfo.TryGetAsset<Texture2D>(SkyboxExportOption.Equirectangular_PNG, out var texture))
{
    skyboxMaterial.mainTexture = texture;
}
```

#### [Get Skybox](https://api-documentation.blockadelabs.com/api/skybox.html#get-skybox-by-id)

Returns the skybox metadata for the given skybox id.

```csharp
var skyboxId = 42;
var api = new BlockadeLabsClient();
var skyboxInfo = await api.SkyboxEndpoint.GetSkyboxInfoAsync(skyboxId);
Debug.Log($"Skybox: {result.Id}");
// Note: If you wish to use the returned skybox info textures you'll need to additionally call await SkyboxInfo.LoadAssetsAsync(); before you can assign them to a material property.
await skyboxInfo.LoadAssetsAsync();

if (skyboxInfo.TryGetAsset<Texture2D>(SkyboxExportOption.Equirectangular_PNG, out var texture))
{
    skyboxMaterial.mainTexture = texture;
}
```

#### [Request Skybox Export](https://api-documentation.blockadelabs.com/api/skybox-exports.html#request-export)

Exports the skybox with the requested export type.

> [!NOTE]
> You can also specify the export types when initially generating a skybox.

```csharp
var skyboxId = 42;
var api = new BlockadeLabsClient();
var skyboxInfo = await api.SkyboxEndpoint.GetSkyboxInfoAsync(skyboxId);
skyboxInfo = await api.SkyboxEndpoint.ExportSkyboxAsync(skyboxInfo, DefaultExportOptions.DepthMap_PNG);
await skyboxInfo.LoadAssetsAsync();

if (skyboxInfo.TryGetAsset<Texture2D>(SkyboxExportOption.DepthMap_PNG, out var texture))
{
    skyboxMaterial.depthTexture = texture;
}
```

#### [Delete Skybox](https://api-documentation.blockadelabs.com/api/skybox.html#delete)

Deletes a skybox by id.

```csharp
var skyboxId = 42;
var result = await api.SkyboxEndpoint.DeleteSkyboxAsync(skybox);
// result == true
```

#### [Get Skybox History](https://api-documentation.blockadelabs.com/api/skybox.html#get-history)

Gets the previously generated skyboxes.

```csharp
var history = await api.SkyboxEndpoint.GetSkyboxHistoryAsync();
Debug.Log($"Found {history.TotalCount} skyboxes");

foreach (var skybox in history.Skyboxes)
{
    Debug.Log($"{skybox.Id} {skybox.Title} status: {skybox.Status}");
}
```

#### [Cancel Skybox Generation](https://api-documentation.blockadelabs.com/api/skybox.html#cancel-generation)

Cancels a pending skybox generation request by id.

```csharp
var skyboxId = 42;
var result = await CancelSkyboxGenerationAsync(skyboxId);
// result == true
```

> [!NOTE]
> This is automatically done when cancelling a skybox generation using cancellation token.

#### [Cancel All Pending Skybox Generations](https://api-documentation.blockadelabs.com/api/skybox.html#cancel-all-pending-generations)

Cancels ALL pending skybox generation requests.

```csharp
var result = await api.SkyboxEndpoint.CancelAllPendingSkyboxGenerationsAsync();
Debug.Log(result ? "All pending generations successfully cancelled" : "No pending generations");
```

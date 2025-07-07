# BlazorWith3d

[Demo](https://blazorwith3d-a3avcthnbbf3edf4.westeurope-01.azurewebsites.net/)

Define the messages, then a MemoryPack source generator can generate the serialization.
And my source gen generates the highlevel API and union handling

so source gen supports the low level binary api

if renderer wants, it can implement this high level API only
with potentially another source gen to generate the pure JS wrapper (as sending messages back to .NET needs some boilerplate)


## Findings

Native Blazor interop is sufficient for most cases of JS libraries (but some binary approaches can still be faster than underling JSON) 

Generators can be used to create the binding classes based on an interface. After initial set up, quick nice to work with and extend.

Simple method/event style interfaces can be implemented even with stream only. useful to get websocket connection to Unity Editr as it is slower than other approaches.

Coordinate systems between different rendering libraries can be a pain. Choose carefully, but conversions are always doable.

This repo shows numerous approaches, but in the end, one or two are probably enough for your app.

The level of abstraction how much to do in 3d specific library vs in main app should depend on usecase and the amount of code that should be shared.
e.g. if only one renderer then easier to do more in the renderer. If more are expected than moving more to main Blazor app can be benefitial

This repo show generating a communication channel based on an interface. i.e. shows practical usage of source generators to help with boilerplate

But the approach (as in Unity Editor Websocket debugging) can be used for communication over any channel. 
The generator is not created to work over some kind of generic channel or Stream. Although it could, it is always intended that the final application might need to fork it and adjust it.
As generic apparoaches loose performance (e.g. binary channels with direct response support being faster than binary channel without it, or having Blazor specific code)

This approach can be used for interprocess communication or any other kinds of native integrations. 
As source generators can create the binding code on both sides, or create apis on top of simple two way stream (e.g. as in Unity WebGL interop, the interop is too messy to generate for each method signature, as it is more basic than Blazor/JS interop) 

The serializers can be replaced, e.g. use built in JSON serializers. Although Unity one is very limited, e.g. no property support, no nullable struct support, root object must be class, ....
Although MemoryPack has own limitations, e.g. no Typescript generation for structs (has to be done in fork)

Built in Unity serializer is not used as it breaks compatibility with native Blazor JS interop (which does not support serializing fields)
But can be considered if the required renderers are not using native Blazor JS interop (as the field support can be added)
Or a solution based on Newtonsoft JSON can be used, as that is more feature complete but is an extra dependency.


The TS generation is a bit intertwined with MemoryPack, since you could get rid of it, and in some cases it would work fine. But mixed modes need to sync TS code and C# code which is a bit messier.
So it is kept connected for now to MemoryPack.
But for DirectInterop any other or custom TS type generator from C# types could be used.


## Renderers

- coordinate systems
  - Blazor (adjustable as the app should choose this)
    - RightHanded
    - Screen (0,0) in top left
    - World X:right, Y: up, Z: toCamera
    - camera looks in NegativeZ
    - directionalLight shines in NegativeZ
  - Unity
    - LeftHanded
    - Screen (0,0) is Bottom Left
    - World X:right, Y: up, Z : fromCamera
    - camera looks in PositiveZ
    - Rotation order Z, X, Y (when going local to world)
  - Babylon
    - LeftHanded
    - Screen (0,0) in top left
    - World X:right, Y: up, Z : fromCamera
    - camera looks in PositiveZ
    - Rotation order Y, X, Z (when going local to world)
  - ThreeJS
    - RightHanded
    - Screen (0,0) in center (positive in direction of top right) range: -1;1
    - World X:right, Y: up, Z : toCamera
    - camera looks in NegativeZ
    - Rotation order Z, X, Y (when going local to world) but can be chosen

### Unity WebGL (interop with Unity WASM via Binary Interop API)

Can use both MemoryPack serializer and Unity JsonUtility based serialization.

- MemoryPack has better performance and more comprehensive support for language features (e.g. Properties) but needs extra libraries to be added (in case build size is a concern)
- JsonUtility based approach has less dependencies, but performance is slower (messages take roughly 2x the time), and has very limited feature support (no properties and no nullable fields)

### JS based libraries

There is `npm run watch` command to run in the  `assets` subfolder of the project, e.g. `\samples\BlazorWith3d.ExampleApp.Client.ThreeJS\assets` folder, that does live recompilation of the TS codebase. Mainly to adjust the `app.ts` as needed.

The Typescript code is built when C# Project is. MemoryPack and own generator create `.ts` files as well and after build Webpack is triggered to create single JS file for the Blazor Component to load.

#### BabylonJS (interop with TypeScript via Binary Interop API)

Kept mainly for historical reasons, as ThreeJS seems to be more fitting for this usecase (as this is a game engine first, and threeJS is a renderer first).

#### ThreeJS (interop with TypeScript via Binary Interop API and Blazor interop)

### Pure HTML (developed directly in Blazor)

Uses 2D screenshot for visuals, to look similar.


## Blazor

### Compilation flags

https://learn.microsoft.com/en-us/aspnet/core/blazor/performance?view=aspnetcore-9.0

https://learn.microsoft.com/en-us/aspnet/core/blazor/webassembly-build-tools-and-aot?view=aspnetcore-9.0

https://github.com/dotnet/runtime/blob/main/src/mono/wasm/features.md

### Render modes

All render modes, including Auto mode are supported ( render mode can be chosen on the Home page)

### Maui

Blazor Maui Hybrid is supported (only tested as Desktop app)
https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/tutorials/maui-blazor-web-app?view=aspnetcore-9.0


## Benchmarks

Small msg is a message with a couple of numbers
Large msg is a message with an almost 4k-character long string  

Benchmarks are times for 10k messages.

Specific numbers should be only used as relative comparison, as this was times only on my machine.


### InteractiveServer RenderMode

Timed on my machine with release build.

But in comparison to WebAssembly version, most time is lost on sending data back and forth from server to browser, then on interop specifics.

- Unity with MemoryPack serializer
    - Small msgs took 2596,00 ms (avg 0,26 ms)
    - Large msgs took 3223,00 ms (avg 0,32 ms)
- Unity with JsonUtility serializer
    - Small msgs took 2791,00 ms (avg 0,28 ms)
    - Large msgs took 4055,00 ms (avg 0,41 ms)
- Blazor JS interop with MemoryPack
    - Small msgs took 2069,00 ms (avg 0,21 ms)
    - Large msgs took 2646,00 ms (avg 0,26 ms)
- Blazor JS interop native
    - Small msgs took 1909,00 ms (avg 0,19 ms)
    - Large msgs took 2734,00 ms (avg 0,27 ms)


### InteractiveWebAssembly RenderMode

Mainly for use as relative comparison, as they were timed on my machine with deployed demo app which is AOT compiled for webassembly.

For Javascript based render (e.g. Three.JS or Babylon.JS), Blazor JS native interop is quicker for smaller messages, but slower on big ones. But not significantly.

For Unity based renderer memory pack is faster for small messages and significantly faster for large ones, but it introduces extra dependencies making the build larger.

- Unity with MemoryPack serializer
  - Small msgs took 196.00 ms (avg 0.02 ms)
  - Large msgs took 282.00 ms (avg 0.03 ms)
- Unity with JsonUtility serializer
  - Small msgs took 314.00 ms (avg 0.03 ms)
  - Large msgs took 853.00 ms (avg 0.09 ms)
- Blazor JS interop with MemoryPack
  - Small msgs took 166.00 ms (avg 0.02 ms)
  - Large msgs took 291.00 ms (avg 0.03 ms)
- Blazor JS interop native 
  - Small msgs took 155.00 ms (avg 0.02 ms)
  - Large msgs took 412.00 ms (avg 0.04 ms)

# Initialization order

- controller exists first (ie controller is singleton and gets a single renderer attached, the controller does not handle lifecycle of renderers, only should SetController to null when renderer is being replaced)
- renderer is created and prepares to listen and sets the EventHandler to the controller
- renderer calls SetRenderer on the Controller
- Controller starts sending commands during SetRenderer execution
- ie renderer can send messages to controller even before registering (as the controller has multiple renderers so it is considered more as API, than a tightly bound pair)
- and renderer should expect messages to already arrive during SetRenderer execution

# Deploy issues
CODE: 409 -> https://github.com/projectkudu/kudu/issues/3042#issuecomment-2200340379 


### Prio 0 (improve generic packages)

Handle warnings in Generator

publish generator and supporting libraries as nuget

Add option to generate TS bindings for Direct Interop without MemoryPack
- e.g. set up BabylonJS that way, to have separation

add requirement that non MemoryPack TS types must be marked as sequential struct layout!

### Prio 1 (improve sample app)

- showcase that there does not have to be direct mapping
    - generate wire connecting the instances
    - and generate cube under the gltf mesh

- And drag and drop trigger to add blocks from HTML

- Add context menu to delete block instance

- add Visuals Transform setting so it can be customized and reused 

- implement GLB loading for babylon

- check if current GLTF instancing in Unity is working

### Prio 2 (backlog)

- ensure the generator has fully optional dependence on memory pack, even for js direct interop use case
  - if turned on via csproj config, generate typescript types intended for direct interop
    - ie using asp.net interop types
    - or via attribute, have MemoryPack based attribute, or have .net wasm interop generator via another attribute

- Optimize Typescript dev experience
    - add option to live recompile changes
    - add debugging support to IDEs
    - switch to Vite as everybody's using it ( see https://doc.babylonjs.com/guidedLearning/usingVite/ )
        - https://www.google.com/search?q=aspnet+%22razor+library%22+vite
        - https://khalidabuhakmeh.com/running-vite-with-aspnet-core-web-applications
        - https://github.com/techgems/Vite.NET/tree/master/dotnet-vite
        - https://github.com/Eptagone/Vite.AspNetCore/tree/main
    - better JS isolation
        - https://www.emekaemego.com/blog/blazor-component-js/

- better JS plugin via $ as in https://github.com/Made-For-Gamers/NEAR-Unity-WebGL-API/blob/main/Assets/WebGLSupport/WebGLInput/WebGLInput.jslib

- add explicit serializer interface for Typescript (to be able to override and mainly expose new types serialization)

- Unity debug socket is logging a lot of errors, handle the disconnect cases more explicitly

- cleanup, refactor generator, too many things seemingly hardcoded and edge cases not handled (e.g. namespaces of messages, or if multiple apps are defined)

- Investigate and optimize render modes and stream rendering and better handling of Maui limitations for render mode
  https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes?view=aspnetcore-9.0
  https://learn.microsoft.com/en-us/aspnet/core/blazor/components/rendering?view=aspnetcore-9.0#streaming-rendering

- try again to get matrix for screen to world as that would reduce the need for extra interop call
    - even basic raycast can be then doable in .NET
        - e.g. https://github.com/bepu/bepuphysics2/blob/master/Demos/Demos/RayCastingDemo.cs

- do Isometric or fake-3d in CSS only for HTML version
    - https://developer.mozilla.org/en-US/docs/Web/CSS/transform-function/perspective
    - must refactor it, to recreate the scene approach as in Unity, to make sense of it
    - should use pre-rendered images
    - mainly as otherwise it is hard to render depth
    - add top down thumbnail image of model (for HTML)

- split packages/libraries by abstraction level (raw message, then typed message, then generated API)

- test/add support to generator for defining messages from other assemblies

- with memory pack the Unity build got slower, investigate why exactly!
    - e.g. might be worth having a define or something to switch the serialization libraries (have one for faster compile time and one for faster runtime)
    - could be worth having a method to negotiate the serialization scheme (kinda send supported schemes when connecting to renderer and it picks one)

- switch to nicer ways to share memory in WASM special case
    - https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/?view=aspnetcore-9.0#type-mappings
    - there should be better mapping with arraySegments now, potentially preventing memorycopy when creating array for normal JS interop
    - Not sure if with TS interop it is worth it, as the IMemoryView either way does not expose the internal array, so a copy would be necessary (or touching private method of a type???)
    - But for Unity interop it could be worth it, as the memory could be then set directly into Unity heap
    - https://github.com/dotnet/runtime/blob/main/src/mono/browser/runtime/marshal.ts#L558
    - // RESOLUTION
        - this does not interop all types, so method returning Task<ArraySegment<byte>> has to be split into 2 calls retuning Task and ArraySegment<byte>
        - the interop is staticky, ie you do not have instances to interop with, meaning you need to pass around an ID of the instance (if you want to handle case where you have multiple renders at the same time)
        - NOT worth it for now


### Prio 3 (future ideas)

- consider support for union types to handle collider definition etc (lower prio as this goes a bit into serialization libraries support)

- native Veldrid based renderer https://veldrid.dev/
  - has MAUI support https://github.com/xtuzy/Veldrid.Samples or https://www.nuget.org/packages/Veldrid.Maui/

- Maui app with native Unity build
  - https://github.com/matthewrdev/UnityUaal.Maui (not working in windows, as MAUI in general does not support embedding other exes as views)
    - Unity in Maui is not officially supported. There are ways but more focused on mobile
    - Maui windows does not allow unity exe direct yet. There is a feature request for this

- Winforms app with Blazor and Unity 
  - https://docs.unity3d.com/6000.1/Documentation/Manual/UnityasaLibrary-Windows.html
  - https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/tutorials/windows-forms?view=aspnetcore-9.0

- Evergine 
  - has MAUI support (not tested)
  - has WASM support 
    - Evergine could work better for Maui but wasm Is still weird, as it needs to be hooked into was compilation, not just razor project. 
    - Also wasm build works, but creating reusable razor component is not officially available,  and without payed support harder to achieve as community is very small.

- three port to C# for maui https://github.com/hjoykim/THREE
- Urho https://github.com/Urho-Net/Urho.Net
- https://monogame.net/

- add support for negotiation of serialization modes
    - so unity can do DEBUG build with JSON only serializaion, e.g. for debug builds with embedded WebGL template as using memory pack is cumbersome there

- consider some generic reactive dictionary or patch requests on object support
    - e.g. that both sides can instantiate kind of reactive dictionry and through generic messages they both can be kept automatically in sync, with changes always propagating to the other side
    - kinda like flux https://facebookarchive.github.io/flux/docs/in-depth-overview/

### Not gonna do for now

- switch to ArraySegment instead of byte[]
    - RESOLUTION: JS interop in Blazor generically supports faster conversion only for byte[] type, ArraySegment is supported as memoryView only in WASM interop

- even generate structs for message arguments!
    - RESOLUTION not usable until Code generators can be chained (ie then the code can be memorypack usable)

- consider dev mode using only Unity's JSON serialization, as that is faster to build (less dlls), also to test switching serialization options 
  - RESOLUTION not gonna do, instead there is option for debugging directly in Unity via websockets

- Add ThreeJS version of 3d renderer using Blazor bindings project
  - https://github.com/HomagGroup/Blazor3D-Core
    - RESOLUTION not going to do it as the library is not exactly production ready (e.g. Orbit controls cannot be disabled)

- consider memorypack fork with Typescript struct support // see memorypack-fork branch
    - maybe different serialization library for interop with Typescript would be better, as structs would help and prevent a lot of type conversions
    - Or Do not use binaryApi with Tyoescript?
      - Consider using types direct via normal blazor interop, to generate TS /c# code facilitating the boilerplate  
      - Or at least benchmark it
    - Otherwise the mempack or protobuf could work
    - Or just leave class only? Maybe should benchmark it to be sure
    - !!! try with a struct which has reference type to force the class like serialization!!!
    - RESOLUTION: this works but there is still the issue that System.Numerics classes are not supoprted, so an in between type has to be used.
      - due to this only system unmanaged types are supported and then custom types, no 3rd party lib types
      - FULL SOLUTION NEEDS GENERATION OF typescript readers reading the Unmanaged types
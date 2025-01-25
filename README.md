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
    - Screen (0,0) in center (positive in direction of top right)
    - World X:right, Y: up, Z : toCamera
    - camera looks in NegativeZ
    - Rotation order X, Y, Z (when going local to world) but can be chosen

### Unity WebGL (interop with Unity WASM via Binary Interop API)

### BabylonJS (interop with TypeScript via Binary Interop API)

Kept mainly for historical reasons, as ThreeJS seems to be more fitting for this usecase (as this is a game engine first, and threeJS is a renderer first).

### ThreeJS (interop with TypeScript via Binary Interop API and Blazor interop)

### Pure HTML (developed directly in Blazor)

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

## TODOs

benchmarks
// redo numbers as these are for old PerfCheck message (without huge string)
- Server
  - Interop (avg 0,48 ms)
  - MemoryPack (avg 0,56 ms)
- WASM
  - Interop (avg 0.50 ms)  // slower but works with more types than memorypack, so you could get rid of that dependency (the slowdown is worse when the larger the messages are)
  - MemoryPack (avg 0.33 ms)

### Prio 0 (what to do next)

- switch to nicer ways to share memory in WASM special case
    - https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/?view=aspnetcore-9.0#type-mappings
    - there should be better mapping with arraySegments now, potentially preventing memorycopy when creating array for normal JS interop

- unity editor debug should refresh on new connection (e.g. backend was restarted)
  - and should react if connection was made before the page was opened

- better JS plugin via $ as in https://github.com/Made-For-Gamers/NEAR-Unity-WebGL-API/blob/main/Assets/WebGLSupport/WebGLInput/WebGLInput.jslib

- Investigate and optimize render modes and stream rendering and better handling of Maui limitations for render mode
  https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes?view=aspnetcore-9.0
  https://learn.microsoft.com/en-us/aspnet/core/blazor/components/rendering?view=aspnetcore-9.0#streaming-rendering

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

- cleanup, refactor generator, too many things seemingly hardcoded and edge cases not handled (e.g. namespaces of messages, or if multiple apps are defined)

### Prio 1 (core tasks of the repo)

- Incremental source gen 

- consider special WASM only interop, as that might be faster
  - also it opens to support of span or arraySegment types to reduce copying of memory
  - e.g. new binaryAPIWithResponse with types mapping to memoryView could be nice, as then the rest works as before

- add explicit serializer interface for Typescript (to be able to override and mainly expose new types serialization)
 
### Prio 2 (make it nicer)

- Unify visuals of all renderers
    - add camera transform setting (and getting, as to have a request to set but I can get the real one)
    - unify different coordinate systems
    - add setting of background color
    - even background plane should be just a mesh to load

- try again to get matrix for screen to world as that would reduce the need for extra interop call
    - even basic raycast can be then doable in .NET
        - e.g. https://github.com/bepu/bepuphysics2/blob/master/Demos/Demos/RayCastingDemo.cs

- do Isometric or fake-3d in CSS only for HTML version
    - https://developer.mozilla.org/en-US/docs/Web/CSS/transform-function/perspective
    - must refactor it, to recreate the scene approach as in Unity, to make sense of it
    - should use pre-rendered images
    - mainly as otherwise it is hard to render depth
    - add top down thumbnail image of model (for HTML)

- add Visuals Transform setting so it can be customized and reused

- implement GLB loading for babylon
- check if current GLTF instancing in Unity is working

- split packages/libraries by abstraction level (raw message, then typed message, then generated API)

- test/add support to generator for defining messages from other assemblies

- with memory pack the Unity build got slower, investigate why exactly!
    - e.g. might be worth having a define or something to switch the serialization libraries (have one for faster compile time and one for faster runtime)
    - could be worth having a method to negotiate the serialization scheme (kinda send supported schemes when connecting to renderer and it picks one)

- add serializer interface concept in TS

### Prio 3 (maybe but not really target of the project)

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
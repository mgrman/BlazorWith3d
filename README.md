# BlazorWith3d

[Demo](https://blazorwith3d-a3avcthnbbf3edf4.westeurope-01.azurewebsites.net/)

Define the messages, then a MemoryPack source generator can generate the serialization.
And my source gen generates the highlevel API and union handling

so source gen supports the low level binary api

if renderer wants, it can implement this high level API only
with potentially another source gen to generate the pure JS wrapper (as sending messages back to .NET needs some boilerplate)

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

### Unity WebGL (interop with Unity WASM)

### BabylonJS (interop with TypeScript)

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
  - Interop (avg 0.50 ms)
  - MemoryPack (avg 0.33 ms)

### Prio 0 (what to do next)

- generate blazor interop code, preferably in  another project referencing the types in shared

- try again to get matrix for screen to world as that would reduce the need for extra interop call

- Maui app with native Unity build
  https://docs.unity3d.com/6000.1/Documentation/Manual/UnityasaLibrary-Windows.html
- https://github.com/matthewrdev/UnityUaal.Maui

- three port to C# for maui https://github.com/hjoykim/THREE


### Prio 1 (stretch goals)

- Unify visuals of all renderers
    - add camera transform setting (and getting, as to have a request to set but I can get the real one)
    - unify different coordinate systems
    - add setting of background color
    - even background plane should be just a mesh to load

- do Isometric or fake-3d in CSS only for HTML version
    - https://developer.mozilla.org/en-US/docs/Web/CSS/transform-function/perspective
    - must refactor it, to recreate the scene approach as in Unity, to make sense of it
    - should use pre-rendered images
    - mainly as otherwise it is hard to render depth

- render screen in unity and stream to blazor for debug mode

- Investigate and optimize render modes and stream rendering and better handling of Maui limitations for render mode
  https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes?view=aspnetcore-9.0
  https://learn.microsoft.com/en-us/aspnet/core/blazor/components/rendering?view=aspnetcore-9.0#streaming-rendering


- Optimize Typescript dev experience
    - add option to live recompile changes
    - add debugging support to IDEs
    - switch to Vite as everybody's using it ( see https://doc.babylonjs.com/guidedLearning/usingVite/ )
 

### Prio 2 (make it nicer)

- add Visuals Transform setting so it can be customized and reused

- implement GLB loading for babylon
- check if current GLTF instancing in Unity is working
- add top down thumbnail image of model (for HTML)

- consider support for union types to handle collider definition etc

- Optimize Typescript API
    - remove memory copies during message handling

- consider special WASM only interop, as that might be faster

- generate methods directly creating instance inside, ie if internal struct, then the simple method can create an instance directly inside, to make nicer API
    - add reusable singleton support for messages without fields
    - to support structs and classes
    - test with pregenerated memory pack Typescript files if struct is working (ie if it is only a limitation of memorypack or blazor interop)

- Incremental source gen

- cleanup, refactor generator, too many things seemingly hardcoded and edge cases not handled (e.g. namespaces of messages, or if multiple apps are defined)

- split packages/libraries by abstraction level (raw message, then typed message, then generated API)

- double check catching of exceptions as they happen in "native" code and do not always propagate properly

- test/add support to generator for defining messages from other assemblies

- with memory pack the Unity build got slower, investigate why exactly!
    - e.g. might be worth having a define or something to switch the serialization libraries (have one for faster compile time and one for faster runtime)
    - could be worth having a method to negotiate the serialization scheme (kinda send supported schemes when connecting to renderer and it picks one)

### Prio 3 (maybe but not really target of the project)


- Urho https://github.com/Urho-Net/Urho.Net
- Official Maui integration for Evergine https://evergine.com/download/
- https://monogame.net/

- but even the serialize methods might be worth to be chosen by the generator based on target so potentially messages can be generated from method arguments
    - e.g. use memoryPack if type annotated, otherwise use json serialization
    - hmm, this might be too confusing, but could allow creating own serializer of multiple method arguments into one message
        - ie first byte is which method, then each arg has header specifing length of data and serialized data.
    - types which are not memorypack annotated get serialized as JSON (although )
  
- add support for negotiation of serialization modes
    - so unity can do DEBUG build with JSON only serializaion, e.g. for debug builds with embedded WebGL template as using memory pack is cumbersome there
    - 
- consider some generic reactive dictionary or patch requests on object support
    - e.g. that both sides can instantiate kind of reactive dictionry and through generic messages they both can be kept automatically in sync, with changes always propagating to the other side
    - kinda like flux https://facebookarchive.github.io/flux/docs/in-depth-overview/



### Not gonna do for now
- appInitialized message should be on renderer level, as it means replay state in general
    - RESOLUTION seems to work well on app level for now

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
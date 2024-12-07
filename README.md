# BlazorWith3d
 
[Demo](https://blazorwith3d-a3avcthnbbf3edf4.westeurope-01.azurewebsites.net/)

Define the messages, then a MemoryPack source generator can generate the serialization.
And my source gen generates the highlevel API and union handling

so source gen supports the low level binary api

if renderer wants, it can implement this high level API only
with potentially another source gen to generate the pure JS wrapper (as sending messages back to .NET needs some boilerplate)

## TODOs

### Prio 0

- fix that babylon js file is not there if you do publish on maui app without any build calls before

- do Isometric or fake-3d in CSS only for HTML version
- https://developer.mozilla.org/en-US/docs/Web/CSS/transform-function/perspective
  
- generate Typescript App Api as well (and set it up to generate an npm package for other apps to use)

### Prio 1

- Optimize Typescript dev experience
    - add option to live recompile changes
    - add debugging support to IDEs
    - switch to Vite as everybody's using it ( see https://doc.babylonjs.com/guidedLearning/usingVite/ )

- Optimize Typescript API
    - remove memory copies during message handling
  
- generate methods directly creating instance inside, ie if internal struct, then the simple method can create an instance directly inside, to make nicer API
- add reusable singleton support for messages without fields

- Add BabylonJS/ThreeJS version of 3d renderer 
    - Using JS interop with messages
    - one using Blazor bindings project
  
- Add BabylonJS version of 3d renderer 
    - one using Blazor bindings project

### Prio 2

- Incremental source gen

- cleanup, refactor generator, too many things seemingly hardcoded and edge cases not handled (e.g. namespaces of messages, or if multiple apps are defined)

- split packages/libraries by abstraction level (raw message, then typed message, then generated API)

- double check catching of exceptions as they happen in "native" code and do not always propagate properly


### Prio 3

- add support for defining messages from other assemblies

- consider some generic reactive dictionary or patch requests on object support
    - e.g. that both sides can instantiate kind of reactive dictionry and through generic messages they both can be kept automatically in sync, with changes always propagating to the other side
    - kinda like flux https://facebookarchive.github.io/flux/docs/in-depth-overview/

- with memory pack the Unity build got slower, investigate why!
    - e.g. might be worth having a define or something to switch the serialization libraries (have one for faster compile time and one for faster runtime)

- Add Maui Blazor with Native android Unity as library
    -  https://github.com/Unity-Technologies/uaal-example


### Not gonna do for now
- appInitialized message should be on renderer level, as it means replay state in general
    - RESOLUTION seems to work well on app level for now

- even generate structs for message arguments!
    - RESOLUTION not usable until Code generators can be chained (ie then the code can be memorypack usable)


-  implement own IDs in messages, consider even adding an extra parameter to the communication method. Otherwise you always need to prepend a byte or somethign
- as then this generator knows nothing about memory pack, and different messages can be serialized differently
- add support for multiple app interfaces (either count in one assembly, or have an offset in the attribute)
- Use array segments, encode which message it is using first byte, but add own system as MemoryPack's generator cannot operate on top of mine. Try to keep typed message abstraction layer
- Hmm, due to limitations of JS interop (seems only byte[] is correctly marshaled, memory ends up base64, span does not work, arraySEgment as number array)
- it might be best to postpone until stronger need, as only other option is to break abstraction of base layer and add an extra parameter, not worth it now.
- maybe the ArrayBufferWriter would be still usable here as it still seems faster

| Method            | Mean     | Error    | StdDev    | Median   |
|------------------ |---------:|---------:|----------:|---------:|
| ByteArray         | 49.65 ns | 4.785 ns | 14.109 ns | 43.82 ns |
| ArrayBufferWriter | 34.29 ns | 0.441 ns |  0.413 ns | 34.28 ns |
- sadly without chaining source generators, this does not work. As none of the types generated can be them MemoryPack compatible (as it uses own source gen)

- consider dev mode using only Unity's JSON serialization, as that is faster to build (less dlls), also to test switching serialization options 
  - RESOLUTION not gonna do, instead there is option for debugging directly in Unity via websockets
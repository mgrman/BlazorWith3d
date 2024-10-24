# TODOs

## Prio 0

- Add BabylonJS/ThreeJS version of 3d renderer
  - One version using Blazor interop https://github.com/HomagGroup/Blazor3D-Core
  - One version using own JS app and memorypack typescript generator

## Prio 1

- Add Maui Blazor version with WebGL Unity
  - https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/tutorials/maui-blazor-web-app?view=aspnetcore-9.0

- Blazorwith3d - add purely 2d view, as for block movement we can have 2d only backend without gpu


## Prio 2

- Incremental source gen

- cleanup, refactor generator, too many things seemingly hardcoded and edge cases not handled (e.g. namespaces of messages, or if multiple apps are defined)

- split unity packages/libraries by abstraction level (raw message, then typed message, then generated API)


## Prio 3

- double check catching of exceptions as they happen in "native" code and do not always propagate properly

- consider some generic reactive dictionary or patch requests on object support
  - e.g. that both sides can instantiate kind of reactive dictionry and through generic messages they both can be kept automatically in sync, with changes always propagating to the other side
  - kinda like flux https://facebookarchive.github.io/flux/docs/in-depth-overview/

- with memory pack the Unity build got slower, investigate why!
  - e.g. might be worth having a define or something to switch the serialization libraries (have one for faster compile time and one for faster runtime)

- Add Maui Blazor with Native android Unity as library
  -  https://github.com/Unity-Technologies/uaal-example

  - consider dev mode using only Unity's JSON serialization, as that is faster to build (less dlls), also to test switching serialization options

## Not gonna do for now
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

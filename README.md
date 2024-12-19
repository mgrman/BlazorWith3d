# BlazorWith3d

[Demo](https://blazorwith3d-a3avcthnbbf3edf4.westeurope-01.azurewebsites.net/)

Define the messages, then a MemoryPack source generator can generate the serialization.
And my source gen generates the highlevel API and union handling

so source gen supports the low level binary api

if renderer wants, it can implement this high level API only
with potentially another source gen to generate the pure JS wrapper (as sending messages back to .NET needs some boilerplate)

## TODOs

### Prio 0 (what to do next)

- Add ThreeJS version of 3d renderer 
    - one using Blazor bindings project

- do Isometric or fake-3d in CSS only for HTML version
    - https://developer.mozilla.org/en-US/docs/Web/CSS/transform-function/perspective
    - must refactor it, to recreate the scene approach as in Unity, to make sense of it

- Unify visuals of all 4 renderers

- Implement all API messages (instead of rectangular shape, add GLB loading) 
  - remove rectangular shape
  - implement drag from palette
  - implement GLB
  - add pointer APIs
  - consider pointer move and click to drag to be initiated via HTML (ie not in engine)
    - so all input handling is outside the 3D part (reduce complexity of the 3d part)
    - so in the end the 3d part just renders things where the bussiness logic tells it to
  - add camera transform setting (and getting, as to have a request to set but I can get the real one)

- 
### Prio 1 (stretch goals)

- Consider again the messages with responses to be added via the generated code

- Optimize Typescript dev experience
    - add option to live recompile changes
    - add debugging support to IDEs
    - switch to Vite as everybody's using it ( see https://doc.babylonjs.com/guidedLearning/usingVite/ )
 
- Maui app with native Unity build
    https://docs.unity3d.com/6000.1/Documentation/Manual/UnityasaLibrary-Windows.html

### Prio 2 (make it nicer)

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
# API

// TODO double check catching of exceptions as they happen in "native" code and do not always propagate properly

// TODO consider some generic reactive dictionary or patch requests on object support
// e.g. that both sides can instantiate kind of reactive dictionry and through generic messages they both can be kept automatically in sync, with changes always propagating to the other side
// kinda like flux https://facebookarchive.github.io/flux/docs/in-depth-overview/

// TODO with memory pack the Unity build got slower, investigate why!
// e.g. might be worth having a define or something to switch the serialization libraries (have one for faster compile time and one for faster runtime)

// TODO add benchmarking of speed, e.g. a dedicated project with just benchamarks (method only, string message, binary string message, memory pack message)

// TODO Add Maui Blazor version with WebGL Unity
// https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/tutorials/maui-blazor-web-app?view=aspnetcore-9.0

// TODO Add Maui Blazor with Native android Unity as library
//  https://github.com/Unity-Technologies/uaal-example


// TODO Incremental source gen

// TODO consider dev mode using only Unity's JSON serialization, as that is faster to build (less dlls), also to test switching serialization options

// TODO Blazorwith3d - add purely 2d view, as for block movement we can have 2d only backend without gpu

// TODO  implement own IDs in messages, consider even adding an extra parameter to the communication method. Otherwise you always need to prepend a byte or somethign
// as then this generator knows nothing about memory pack, and different messages can be serialized differently
// add support for multiple app interfaces (either count in one assembly, or have an offset in the attribute)

// TODO even generate structs for message arguments!

// TODO cleanup, refactor generator, too many things seemingly hardcoded and edge cases not handled

// TODO split unity packages/libraries by abstraction level (raw message, then typed message, then generated API)

// TODO create a way to connect Unity instance to running backend, ie a remote instance on 3d component 

// TODO appInitialized message should be on renderer level, as it means replay state in general
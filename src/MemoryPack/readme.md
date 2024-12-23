
Fork of MemoryPack, with the intent to allow structs to have typescript classes generated.
Since the problem is that typescript generator does not create serializer/deserializer for unmanaged types, as those are casted not read field by field
We can force them to be serialized as not unmanaged structs, so they are serialized field by field with header, and then the generated typescript types can read them.

This does not affect other functionality, as without this change, you would have to use a class and that would be serialized the same way. 
This way we reduce the need for classes in C# code and therefore reduce the amount of GC garbage.

Changes:
- Changed GenerateTypeScriptAttribute in MemoryPack.Core to be allowed on structs as well
- in MemoryPackGenerator.Emitter changed EmitSerializeMembers method to force use EmitSerialize method if the member type has GenerateTypeScript attribute
- MemoryPackGenerator.Parser change setting of IsUnmanagedType property, to also need for the type to not have GenerateTypeScript attribute (ie type with the attribute is never considered unmanaged)
- MemoryPackGenerator.Parser change ParseMemberKind method to consider memberType.IsUnmanagedType only if the member type does not have GenerateTypeScript attribute

Rest of changes are more to make the fork work within my repo
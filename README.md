# AddRequiredModifierToCSharpAutoProperties

Point this .NET 7 console app at the root / solution folder of a .NET 7 / C# 11 solution and it will **modify** all relevant `.cs` files to add the `required` modifier to all likely-looking properties.

Be careful with this... it could render your solution unbuildable.

Examine the source code and make you understand **exactly** what it will do before you use it.

Note that it will only operate on auto-properties that are declared on one line, and are properly formatted.

This is crude stuff, but effective.

You could point out myriad problems with the logic, and you'd be right.  But it helped me get most of the way there.

Re-build your solution, and deal with edge cases where `required` should be removed - or - cases where the new keyword has found genuine "bugs" where a property should be initialized, but isn't being initialized.

Sometimes the `[SetsRequiredMembers]` attribute should be applied to constructors.

Reference: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/required


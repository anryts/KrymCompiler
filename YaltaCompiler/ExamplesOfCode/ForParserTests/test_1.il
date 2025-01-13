.assembly program {}
.method public static void Main() cil managed
{
    .entrypoint
    .maxstack 2
    .locals init (
        [0] int32 counter
    )

    // Load 5 onto the stack
    ldc.i4 5
    // Load 2 onto the stack
    ldc.i4 2
    // Add 5 and 2, result is on the stack
    add
    // Store the result in counter
    stloc.0

    // Load the result (counter) onto the stack
    ldloc.0
    // Load 3 onto the stack
    ldc.i4 1
    // Divide the result of (5 + 2) by 3
    div
    // Store the result of division back in counter
    stloc.0

    // Load counter and print it
    ldloc.0
    call void [System.Console]System.Console::WriteLine(int32)

    ret
}

.assembly program {}
.method static void Main() cil managed
{
.entrypoint
.maxstack 128
.locals init (
[0] int32 counter,
[1] int32 a
)
ldc.i4 0
stloc.0

// Start the while loop
L1:
ldloc.0
ldc.i4 10
clt
brfalse L2

ldloc.0
ldc.i4 1
add
stloc.0
ldloc.0
call void [mscorlib]System.Console::WriteLine(int32)

//Jump back to start of loop
br L1

//End of loop (exit point)
L2:
ldc.i4 5
stloc.1
ldloc.1
call void [mscorlib]System.Console::WriteLine(int32)
ret
}

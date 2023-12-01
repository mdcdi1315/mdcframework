// Diassasembled version of System.Runtime.CompilerServices.Unsafe.dll flavor.
// © MDCDI1315. Do not edit.
// This file is built during MDCFR build.

// Metadata version: v4.0.30319
.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 4:0:0:0
}
.assembly System.Runtime.CompilerServices.Unsafe
{
  .custom instance void [mscorlib]System.Reflection.AssemblyProductAttribute::.ctor(string) = ( 01 00 1A 4D 69 63 72 6F 73 6F 66 74 C2 AE 20 2E   // ...Microsoft.. .
                                                                                                4E 45 54 20 46 72 61 6D 65 77 6F 72 6B 00 00 )    // NET Framework..
  .custom instance void [mscorlib]System.CLSCompliantAttribute::.ctor(bool) = ( 01 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Reflection.AssemblyMetadataAttribute::.ctor(string,
                                                                                     string) = ( 01 00 15 2E 4E 45 54 46 72 61 6D 65 77 6F 72 6B   // ....NETFramework
                                                                                                 41 73 73 65 6D 62 6C 79 00 00 00 )                // Assembly...

  // --- The following custom attribute is added automatically, do not uncomment -------
  //  .custom instance void [mscorlib]System.Diagnostics.DebuggableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggableAttribute/DebuggingModes) = ( 01 00 03 00 00 00 00 00 ) 

  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Reflection.AssemblyDescriptionAttribute::.ctor(string) = ( 01 00 26 53 79 73 74 65 6D 2E 52 75 6E 74 69 6D   // ..&System.Runtim
                                                                                                    65 2E 43 6F 6D 70 69 6C 65 72 53 65 72 76 69 63   // e.CompilerServic
                                                                                                    65 73 2E 55 6E 73 61 66 65 00 00 )                // es.Unsafe..
  .custom instance void [mscorlib]System.Reflection.AssemblyFileVersionAttribute::.ctor(string) = ( 01 00 0C 37 2E 30 2E 32 32 2E 31 35 32 30 32 00   // ...7.0.22.15202.
                                                                                                    00 ) 
  .custom instance void [mscorlib]System.Reflection.AssemblyInformationalVersionAttribute::.ctor(string) = ( 01 00 17 37 2E 30 2E 30 2D 70 72 65 76 69 65 77   // ...7.0.0-preview
                                                                                                             2E 32 2E 32 32 31 35 32 2E 32 00 00 )             // .2.22152.2..
  .custom instance void [mscorlib]System.Reflection.AssemblyTitleAttribute::.ctor(string) = ( 01 00 26 53 79 73 74 65 6D 2E 52 75 6E 74 69 6D   // ..&System.Runtim
                                                                                              65 2E 43 6F 6D 70 69 6C 65 72 53 65 72 76 69 63   // e.CompilerServic
                                                                                              65 73 2E 55 6E 73 61 66 65 00 00 )                // es.Unsafe..
  .custom instance void [mscorlib]System.Reflection.AssemblyMetadataAttribute::.ctor(string,
                                                                                     string) = ( 01 00 0B 53 65 72 76 69 63 65 61 62 6C 65 04 54   // ...Serviceable.T
                                                                                                 72 75 65 00 00 )                                  // rue..
  .custom instance void [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                             63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.
  .custom instance void [mscorlib]System.Reflection.AssemblyMetadataAttribute::.ctor(string,
                                                                                     string) = ( 01 00 0B 49 73 54 72 69 6D 6D 61 62 6C 65 04 54   // ...IsTrimmable.T
                                                                                                 72 75 65 00 00 )                                  // rue..
  .custom instance void [mscorlib]System.Reflection.AssemblyCopyrightAttribute::.ctor(string) = ( 01 00 2F C2 A9 20 4D 69 63 72 6F 73 6F 66 74 20   // ../.. Microsoft 
                                                                                                  43 6F 72 70 6F 72 61 74 69 6F 6E 2E 20 20 41 6C   // Corporation.  Al
                                                                                                  6C 20 72 69 67 68 74 73 20 72 65 73 65 72 76 65   // l rights reserve
                                                                                                  64 2E 00 00 )                                     // d...
  .custom instance void [mscorlib]System.Reflection.AssemblyCompanyAttribute::.ctor(string) = ( 01 00 15 4D 69 63 72 6F 73 6F 66 74 20 43 6F 72   // ...Microsoft Cor
                                                                                                70 6F 72 61 74 69 6F 6E 00 00 )                   // poration..
  
  .permissionset reqmin
             = {[mscorlib]System.Security.Permissions.SecurityPermissionAttribute = {property bool 'SkipVerification' = bool(true)}}
  
  .hash algorithm 0x00008004
  .ver 7:0:0:0
}
.module System.Runtime.CompilerServices.Unsafe.dll
// MVID: {EEA8A510-D764-496D-AB35-DB28BADFD517}
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000009    //  ILONLY
// Image base: 0x0B830000


// =============== CLASS MEMBERS DECLARATION ===================

.class public abstract auto ansi sealed beforefieldinit System.Runtime.CompilerServices.Unsafe
       extends [mscorlib]System.Object
{
  .method public hidebysig static !!T  Read<T>(void* source) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldobj      !!T
    IL_0006:  ret
  } // end of method Unsafe::Read

  .method public hidebysig static !!T  ReadUnaligned<T>(void* source) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       10 (0xa)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  unaligned. 1
    IL_0004:  ldobj      !!T
    IL_0009:  ret
  } // end of method Unsafe::ReadUnaligned

  .method public hidebysig static !!T  ReadUnaligned<T>(uint8& source) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       10 (0xa)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  unaligned. 1
    IL_0004:  ldobj      !!T
    IL_0009:  ret
  } // end of method Unsafe::ReadUnaligned

  .method public hidebysig static void  Write<T>(void* destination,
                                                 !!T 'value') cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stobj      !!T
    IL_0007:  ret
  } // end of method Unsafe::Write

  .method public hidebysig static void  WriteUnaligned<T>(void* destination,
                                                          !!T 'value') cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       11 (0xb)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  unaligned. 1
    IL_0005:  stobj      !!T
    IL_000a:  ret
  } // end of method Unsafe::WriteUnaligned

  .method public hidebysig static void  WriteUnaligned<T>(uint8& destination,
                                                          !!T 'value') cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       11 (0xb)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  unaligned. 1
    IL_0005:  stobj      !!T
    IL_000a:  ret
  } // end of method Unsafe::WriteUnaligned

  .method public hidebysig static void  Copy<T>(void* destination,
                                                !!T& source) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldobj      !!T
    IL_0007:  stobj      !!T
    IL_000c:  ret
  } // end of method Unsafe::Copy

  .method public hidebysig static void  Copy<T>(!!T& destination,
                                                void* source) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldobj      !!T
    IL_0007:  stobj      !!T
    IL_000c:  ret
  } // end of method Unsafe::Copy

  .method public hidebysig static void*  AsPointer<T>(!!T& 'value') cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       3 (0x3)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  conv.u
    IL_0002:  ret
  } // end of method Unsafe::AsPointer

  .method public hidebysig static void  SkipInit<T>([out] !!T& 'value') cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method Unsafe::SkipInit

  .method public hidebysig static int32  SizeOf<T>() cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  sizeof     !!T
    IL_0006:  ret
  } // end of method Unsafe::SizeOf

  .method public hidebysig static void  CopyBlock(void* destination,
                                                  void* source,
                                                  uint32 byteCount) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       6 (0x6)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  cpblk
    IL_0005:  ret
  } // end of method Unsafe::CopyBlock

  .method public hidebysig static void  CopyBlock(uint8& destination,
                                                  uint8& source,
                                                  uint32 byteCount) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       6 (0x6)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  cpblk
    IL_0005:  ret
  } // end of method Unsafe::CopyBlock

  .method public hidebysig static void  CopyBlockUnaligned(void* destination,
                                                           void* source,
                                                           uint32 byteCount) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  unaligned. 1
    IL_0006:  cpblk
    IL_0008:  ret
  } // end of method Unsafe::CopyBlockUnaligned

  .method public hidebysig static void  CopyBlockUnaligned(uint8& destination,
                                                           uint8& source,
                                                           uint32 byteCount) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  unaligned. 1
    IL_0006:  cpblk
    IL_0008:  ret
  } // end of method Unsafe::CopyBlockUnaligned

  .method public hidebysig static void  InitBlock(void* startAddress,
                                                  uint8 'value',
                                                  uint32 byteCount) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       6 (0x6)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  initblk
    IL_0005:  ret
  } // end of method Unsafe::InitBlock

  .method public hidebysig static void  InitBlock(uint8& startAddress,
                                                  uint8 'value',
                                                  uint32 byteCount) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       6 (0x6)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  initblk
    IL_0005:  ret
  } // end of method Unsafe::InitBlock

  .method public hidebysig static void  InitBlockUnaligned(void* startAddress,
                                                           uint8 'value',
                                                           uint32 byteCount) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  unaligned. 1
    IL_0006:  initblk
    IL_0008:  ret
  } // end of method Unsafe::InitBlockUnaligned

  .method public hidebysig static void  InitBlockUnaligned(uint8& startAddress,
                                                           uint8 'value',
                                                           uint32 byteCount) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  unaligned. 1
    IL_0006:  initblk
    IL_0008:  ret
  } // end of method Unsafe::InitBlockUnaligned

  .method public hidebysig static !!T  As<class T>(object o) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       2 (0x2)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ret
  } // end of method Unsafe::As

  .method public hidebysig static !!T&  AsRef<T>(void* source) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       4 (0x4)
    .maxstack  8
    .locals (int32& V_0)
    IL_0000:  ldarg.0
    IL_0001:  stloc.0
    IL_0002:  ldloc.0
    IL_0003:  ret
  } // end of method Unsafe::AsRef

  .method public hidebysig static !!T&  AsRef<T>(!!T& source) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    .param [1]
    .custom instance void System.Runtime.CompilerServices.IsReadOnlyAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       2 (0x2)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ret
  } // end of method Unsafe::AsRef

  .method public hidebysig static !!TTo& 
          As<TFrom,TTo>(!!TFrom& source) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       2 (0x2)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ret
  } // end of method Unsafe::As

  .method public hidebysig static !!T&  Unbox<valuetype .ctor ([mscorlib]System.ValueType) T>(object 'box') cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  unbox      !!T
    IL_0006:  ret
  } // end of method Unsafe::Unbox

  .method public hidebysig static !!T&  Add<T>(!!T& source,
                                               int32 elementOffset) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  sizeof     !!T
    IL_0008:  conv.i
    IL_0009:  mul
    IL_000a:  add
    IL_000b:  ret
  } // end of method Unsafe::Add

  .method public hidebysig static void*  Add<T>(void* source,
                                                int32 elementOffset) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  sizeof     !!T
    IL_0008:  conv.i
    IL_0009:  mul
    IL_000a:  add
    IL_000b:  ret
  } // end of method Unsafe::Add

  .method public hidebysig static !!T&  Add<T>(!!T& source,
                                               native int elementOffset) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       11 (0xb)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  sizeof     !!T
    IL_0008:  mul
    IL_0009:  add
    IL_000a:  ret
  } // end of method Unsafe::Add

  .method public hidebysig static !!T&  Add<T>(!!T& source,
                                               native uint elementOffset) cil managed aggressiveinlining
  {
    .param [2]
    .custom instance void System.Runtime.CompilerServices.NativeIntegerAttribute::.ctor() = ( 01 00 00 00 ) 
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       11 (0xb)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  sizeof     !!T
    IL_0008:  mul
    IL_0009:  add
    IL_000a:  ret
  } // end of method Unsafe::Add

  .method public hidebysig static !!T&  AddByteOffset<T>(!!T& source,
                                                         native int byteOffset) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       4 (0x4)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  add
    IL_0003:  ret
  } // end of method Unsafe::AddByteOffset

  .method public hidebysig static !!T&  AddByteOffset<T>(!!T& source,
                                                         native uint byteOffset) cil managed aggressiveinlining
  {
    .param [2]
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    .custom instance void System.Runtime.CompilerServices.NativeIntegerAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       4 (0x4)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  add
    IL_0003:  ret
  } // end of method Unsafe::AddByteOffset

  .method public hidebysig static !!T&  Subtract<T>(!!T& source,
                                                    int32 elementOffset) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  sizeof     !!T
    IL_0008:  conv.i
    IL_0009:  mul
    IL_000a:  sub
    IL_000b:  ret
  } // end of method Unsafe::Subtract

  .method public hidebysig static void*  Subtract<T>(void* source,
                                                     int32 elementOffset) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  sizeof     !!T
    IL_0008:  conv.i
    IL_0009:  mul
    IL_000a:  sub
    IL_000b:  ret
  } // end of method Unsafe::Subtract

  .method public hidebysig static !!T&  Subtract<T>(!!T& source,
                                                    native int elementOffset) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       11 (0xb)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  sizeof     !!T
    IL_0008:  mul
    IL_0009:  sub
    IL_000a:  ret
  } // end of method Unsafe::Subtract

  .method public hidebysig static !!T&  Subtract<T>(!!T& source,
                                                    native uint elementOffset) cil managed aggressiveinlining
  {
    .param [2]
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    .custom instance void System.Runtime.CompilerServices.NativeIntegerAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       11 (0xb)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  sizeof     !!T
    IL_0008:  mul
    IL_0009:  sub
    IL_000a:  ret
  } // end of method Unsafe::Subtract

  .method public hidebysig static !!T&  SubtractByteOffset<T>(!!T& source,
                                                              native int byteOffset) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       4 (0x4)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  sub
    IL_0003:  ret
  } // end of method Unsafe::SubtractByteOffset

  .method public hidebysig static !!T&  SubtractByteOffset<T>(!!T& source,
                                                              native uint byteOffset) cil managed aggressiveinlining
  {
    .param [2]
    .custom instance void System.Runtime.CompilerServices.NativeIntegerAttribute::.ctor() = ( 01 00 00 00 ) 
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       4 (0x4)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  sub
    IL_0003:  ret
  } // end of method Unsafe::SubtractByteOffset

  .method public hidebysig static native int 
          ByteOffset<T>(!!T& origin,
                        !!T& target) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       4 (0x4)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  ldarg.0
    IL_0002:  sub
    IL_0003:  ret
  } // end of method Unsafe::ByteOffset

  .method public hidebysig static bool  AreSame<T>(!!T& left,
                                                   !!T& right) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       5 (0x5)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ceq
    IL_0004:  ret
  } // end of method Unsafe::AreSame

  .method public hidebysig static bool  IsAddressGreaterThan<T>(!!T& left,
                                                                !!T& right) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       5 (0x5)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  cgt.un
    IL_0004:  ret
  } // end of method Unsafe::IsAddressGreaterThan

  .method public hidebysig static bool  IsAddressLessThan<T>(!!T& left,
                                                             !!T& right) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       5 (0x5)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  clt.un
    IL_0004:  ret
  } // end of method Unsafe::IsAddressLessThan

  .method public hidebysig static bool  IsNullRef<T>(!!T& source) cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       6 (0x6)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.0
    IL_0002:  conv.u
    IL_0003:  ceq
    IL_0005:  ret
  } // end of method Unsafe::IsNullRef

  .method public hidebysig static !!T&  NullRef<T>() cil managed aggressiveinlining
  {
    .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       3 (0x3)
    .maxstack  8
    IL_0000:  ldc.i4.0
    IL_0001:  conv.u
    IL_0002:  ret
  } // end of method Unsafe::NullRef

} // end of class System.Runtime.CompilerServices.Unsafe

.class private auto ansi sealed beforefieldinit System.Runtime.Versioning.NonVersionableAttribute
       extends [mscorlib]System.Attribute
{
  .custom instance void [mscorlib]System.AttributeUsageAttribute::.ctor(valuetype [mscorlib]System.AttributeTargets) = ( 01 00 6C 00 00 00 02 00 54 02 0D 41 6C 6C 6F 77   // ..l.....T..Allow
                                                                                                                         4D 75 6C 74 69 70 6C 65 00 54 02 09 49 6E 68 65   // Multiple.T..Inhe
                                                                                                                         72 69 74 65 64 00 )                               // rited.
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Attribute::.ctor()
    IL_0006:  ret
  } // end of method NonVersionableAttribute::.ctor

} // end of class System.Runtime.Versioning.NonVersionableAttribute

.class private auto ansi sealed beforefieldinit System.Runtime.CompilerServices.IsReadOnlyAttribute
       extends [mscorlib]System.Attribute
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Attribute::.ctor()
    IL_0006:  ret
  } // end of method IsReadOnlyAttribute::.ctor

} // end of class System.Runtime.CompilerServices.IsReadOnlyAttribute

.class private auto ansi sealed beforefieldinit System.Runtime.CompilerServices.NativeIntegerAttribute
       extends [mscorlib]System.Attribute
{
  .custom instance void [mscorlib]System.AttributeUsageAttribute::.ctor(valuetype [mscorlib]System.AttributeTargets) = ( 01 00 84 6B 00 00 02 00 54 02 0D 41 6C 6C 6F 77   // ...k....T..Allow
                                                                                                                         4D 75 6C 74 69 70 6C 65 00 54 02 09 49 6E 68 65   // Multiple.T..Inhe
                                                                                                                         72 69 74 65 64 00 )                               // rited.
  .custom instance void Microsoft.CodeAnalysis.EmbeddedAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field public initonly bool[] TransformFlags
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       23 (0x17)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Attribute::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldc.i4.1
    IL_0008:  newarr     [mscorlib]System.Boolean
    IL_000d:  dup
    IL_000e:  ldc.i4.0
    IL_000f:  ldc.i4.1
    IL_0010:  stelem.i1
    IL_0011:  stfld      bool[] System.Runtime.CompilerServices.NativeIntegerAttribute::TransformFlags
    IL_0016:  ret
  } // end of method NativeIntegerAttribute::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(bool[] A_0) cil managed
  {
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Attribute::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      bool[] System.Runtime.CompilerServices.NativeIntegerAttribute::TransformFlags
    IL_000d:  ret
  } // end of method NativeIntegerAttribute::.ctor

} // end of class System.Runtime.CompilerServices.NativeIntegerAttribute

.class private auto ansi sealed beforefieldinit Microsoft.CodeAnalysis.EmbeddedAttribute
       extends [mscorlib]System.Attribute
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void Microsoft.CodeAnalysis.EmbeddedAttribute::.ctor() = ( 01 00 00 00 ) 
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Attribute::.ctor()
    IL_0006:  ret
  } // end of method EmbeddedAttribute::.ctor

} // end of class Microsoft.CodeAnalysis.EmbeddedAttribute



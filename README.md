# soft-math - Work in progress

Unity package. Deterministic math library for online games and more. Based on software floating point implementation.  
Does not reference Unity Engine, so it could be used in a regular C# project.

## Installation

Make sure you have standalone [Git](https://git-scm.com/downloads) installed first. Reboot after installation.  
In Unity, open "Window" -> "Package Manager".  
Click the "+" sign on top left corner -> "Add package from git URL..."  
Paste this: `https://github.com/nilpunch/soft-math.git`  
See minimum required Unity version in the `package.json` file.

## How to use

Library built in such a way that Unity users are most familiar with, but follows C# standards.

Types overview:
- `SoftFloat` - full replacement for built-in float's
- `SoftMath` - math library for soft floats
- `SoftVector3`, `SoftQuaternion`... - math structs, based on soft floats

Each type has a **Soft** prefix in their name, so use the IDE to quickly find what you need.

### SoftFloat

The `SoftFloat` type is the main type that you'll need to use for deterministic float calculations.

The `SoftFloat` type can be constructed in three ways:
- Explicit cast from float:
```csharp
SoftFloat a = (SoftFloat)1.0f;
SoftFloat b = (SoftFloat)(-123.456f);
SoftFloat c = (SoftFloat)float.PositiveInfinity;
SoftFloat d = (SoftFloat)float.NaN;
```
This cast is basically free, since the internal representations are identical.

- Explicit cast from int or uint:
```csharp
SoftFloat a = (SoftFloat)1;
SoftFloat b = (SoftFloat)(-123);
SoftFloat c = (SoftFloat)uint.MaxValue;
```

- Create from raw byte representation
```csharp
SoftFloat a = SoftFloat.FromRaw(0x00000000); // == 0
SoftFloat b = SoftFloat.FromRaw(0x3f800000); // == 1
SoftFloat c = SoftFloat.FromRaw(0xc2f6e979); // == -123.456
SoftFloat d = SoftFloat.FromRaw(0x7f800000); // == Infinity
```
This is also free, it's just the byte representation of the value.

The rest of the operations work just like with floats (addition, multiplication, etc.).  
Note that you should always use a float literal (or a variable that was assigned a float literal before) for explicit casts from floats, since any operation done on floats can be non-deterministic.
```csharp
// OK
float a = 1.0f;
SoftFloat b = (SoftFloat)a + (SoftFloat)123.456f;


// NOT OK
float a = 1.0f;
SoftFloat b = (SoftFloat)(a + 123.456f); // Float addition here, which may be non-deterministic
```

Also, `SoftFloat` has some shorthands for common values, like:
```csharp
SoftFloat inf = SoftFloat.PositiveInfinity;
SoftFloat a = SoftFloat.One;
SoftFloat b = SoftFloat.Zero;
SoftFloat c = SoftFloat.Epsilon; // Smallest precise positive number
```

### Using SoftMath

You can use `SoftMath` just like a regular mathematics library:
```csharp
SoftFloat x = (SoftFloat)2.0f;
SoftFloat squareRoot = SoftMath.Sqrt(x); // SoftMath.Sqrt2 constant

SoftFloat cos = SoftMath.Cos((SoftFloat)3.1415f); // SoftMath.PI constant

SoftFloat max = SoftMath.Max(SoftFloat.One, SoftFloat.Zero);

SoftFloat sign = SoftMath.Sign((SoftFloat)(-1));
```

### Using Vectors, Quaternions and other

Library provide Unity-like math structs with all the useful operations.
They are all starts with Soft prefix
```csharp
SoftVector3 vector = new SoftVector3(SoftFloat.One, SoftFloat.Zero, SoftFloat.Zero);

SoftVector3 cross = SoftVector3.Cross(vector, SoftVector3.Up);

SoftQuaternion quaternion = SoftQuaternion.Identity;

SoftVector3 rotatedVector = quaternion * vector;
```

## Resources

This repository uses the work of:
- [SoftFloat](https://github.com/CodesInChaos/SoftFloat), which implements basic soft float functionality
- [libm](https://github.com/rust-lang/libm), which implements various operations for floating point numbers, including square root, trigonometric functions, transcendental functions, etc. (ported to C#)
- [soft-float-starter-pack](https://github.com/Kimbatt/soft-float-starter-pack), which marry the couple above together

## License

[MIT](https://choosealicense.com/licenses/mit/)
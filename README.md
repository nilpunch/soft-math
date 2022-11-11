# soft-math - Work in progress

Unity package. Deterministic math library for online games and more. Based on software floating point implementation.  
Does not reference Unity Engine, so it could be used in a regular C# project.

This repository uses the work of:  
- [SoftFloat](https://github.com/CodesInChaos/SoftFloat), which implements basic soft float functionality
- [libm](https://github.com/rust-lang/libm), which implements various operations for floating point numbers, including square root, trigonometric functions, transcendental functions, etc. (ported to C#)
- [soft-float-starter-pack](https://github.com/Kimbatt/soft-float-starter-pack), which marry the couple above together

Make sure you have standalone [Git](https://git-scm.com/downloads) installed first.  
In Unity, open "Window" -> "Package Manager".  
Click the "+" sign on top left corner -> "Add package from git URL..."  
Paste this: `https://github.com/nilpunch/soft-math.git`  
See minimum required Unity version in the `package.json` file.

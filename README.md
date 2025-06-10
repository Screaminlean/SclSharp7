# SlcSharp7 overview

You can refer to the documentation over at [Snap7](https://snap7.sourceforge.net/).
This package is based off the last version of Sharp7 [1.1.0.0](https://sourceforge.net/projects/snap7/files/Sharp7/Last/1.1.0/)
The version of this package will start with a base version off 1.1.0.0 and increment as fixes are made.

## Intension
I intend to make this package thread safe and mvvm friendly.

### Refactoring
To try and keep the implementation the same as the original Sharp7 package I have moved code into partial classes and kept the namespaces the same.
As I go along I will add the xml documentation to the code so that it is easier to understand and use.

# Main features
- Fully standard "safe managed" C# code without any dependencies.
- Virtually every hardware with an Ethernet adapter able to run a .NET Core can be connected to an S7 PLC.
- Packed protocol headers to improve performances.
- Helper class to access to all S7 types without worrying about Little-Big endian conversion.
- Compatible with Universal Windows Platform including Win10 IoT for Raspberry.
- ~~One single file.~~ This library has all the classes and structs seperated.
- Made thread safe.
- Many function and methods now have async impementations.

- # How to install

## Package Manager or dotnet CLI
```
PM> Install-Package SclSharp7
```
or
```
> dotnet add package SclSharp7
```

# Get Started

## Supported Targets
- S7 300/400/WinAC CPU (fully supported)
- S7 1200/1500 CPU
- CP (Communication processor - 343/443/IE)

## S7 1200/1500 Notes

An external equipment can access to S71200/1500 CPU using the S7 'base' protocol, only working as an HMI, i.e. only basic data transfer are allowed.

All other PG operations (control/directory/etc..) must follow the extended protocol, not implemented yet.

Particularly **to access a DB in S71500 some additional setting plc-side are needed**.

- Only global DBs can be accessed.

- The optimized block access must be turned off.

![DB_props](http://snap7.sourceforge.net/snap7_client_file/db_1500.bmp)

- The access level must be “full” and the “connection mechanism” must allow GET/PUT.

![DB_sec](http://snap7.sourceforge.net/snap7_client_file/cpu_1500.bmp)
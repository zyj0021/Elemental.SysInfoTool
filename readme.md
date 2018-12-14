# Elemental SysInfo Tool

A dotnet global tool that simply outputs system information about the current machine/environment.

```
# install
dotnet tool install -g Elemental.SysInfoTool
# run
sysinfo
```

I've recently been playing around with Linux machines for the first time, and I always struggle to find basic information about the machine, like IP address and regional settings. I made this tool so I don't need to learn the OS-specific way to get at that information.

## Release Notes

### 0.5.0
- Add SpecialFolders to output. Thanks [@atifaziz](http://github.com/atifaziz/)

### 0.4.2
- Bug fix for drive not ready.

### 0.4.1
- Bug fix for null drive format.

### 0.4.0
- Add storage info

### 0.3.0
- Add ProcessorCount
- Add SystemPageSize
- Add system start/up time

### 0.2.0
- Special handling for LS_COLORS linux environment variable

### 0.1.0
- Initial release.
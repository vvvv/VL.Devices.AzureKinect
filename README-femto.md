# VL.Devices.AzureKinect.Femto
A package for using Orbbec Femto [Bolt](https://www.orbbec.com/products/tof-camera/femto-bolt/) and [Mega](https://www.orbbec.com/products/tof-camera/femto-mega/) devices via the [OrbbecSDK K4A Wrapper](https://github.com/orbbec/OrbbecSDK-K4A-Wrapper). Including body tracking.  

BodyTracking via [Azure Kinect Body Tracking SDK 1.1.2](https://learn.microsoft.com/en-us/previous-versions/azure/kinect-dk/body-sdk-download).

Try it with vvvv, the visual live-programming environment for .NET  
Download: http://vvvv.org

## Firmware Prerequisites
- Based on [K4A-Wrapper 2.0.11](https://github.com/orbbec/OrbbecSDK-K4A-Wrapper/releases/tag/v2.0.11) which lists specific requirements for the firmware of the Femto devices 
- Check [OrbbecFirmware](https://github.com/orbbec/OrbbecFirmware) for firmware upgrade instructions

## BodyTracking Prerequisites
- [BodyTracking SDK 1.1.2](https://learn.microsoft.com/en-us/azure/kinect-dk/body-sdk-download) (in the default install path!)
- [Cuda 11.4](https://developer.nvidia.com/cuda-11-4-0-download-archive) only if wanting to use Cuda instead of DirectML or Gpu as the "Processing Mode"

NOTE: 
- Node names are exactly the same as in the VL.Devices.AzureKinect NuGet, thus this is a drop-in replacement! 
- Once either VL.Devices.AuzureKinect or VL.Devices.AzureKinect.Femto was loaded, switching the NuGet reference is not enough to switch between them. A vvvv restart is also required after changing the reference!
- AzureKinect and Femto devices cannot run in parallel!
- For using Femto and other Orbbec devices via the [OrbbecSDK v2](https://github.com/orbbec/OrbbecSDK_v2) see [VL.Devices.Orbbec](https://www.nuget.org/packages/VL.Devices.Orbbec). NOTE: That option does not include body tracking!

## Using the library
In order to use this library with VL you have to install the nuget that is available via nuget.org. For information on how to use nugets with VL, see [Managing Nugets](https://thegraybook.vvvv.org/reference/libraries/dependencies.html#manage-nugets) in the VL documentation. As described there you go to the commandline and then type:

    nuget install VL.Devices.AzureKinect.Femto

Once the NuGet is installed and referenced in your VL document you'll see the category "AzureKinect" under "Devices" in the nodebrowser. 

Demos are available via the Help Browser!

## Developing 
While developing make sure to have correct .dlls for either AzureKinect or Femto devices in the \runtimes folder!

When done working on help patches, run "help2femto.bat" which duplicates the help patches into \help-femto (but with references set to VL.Devices.AzureKinect.Femto) and commit those. 

The deployment GH Action creates two packs including the correct readme and help patches.

## Sponsoring
Development of this library was partially sponsored by
- [Studio Brüll](https://studiobruell.de)
- [wirmachenbunt](https://wirmachenbunt.de)

For custom development requests, please [get in touch](mailto:devvvvs@vvvv.org).
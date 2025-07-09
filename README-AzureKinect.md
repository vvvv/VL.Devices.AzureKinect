# VL.Devices.AzureKinect
A package for using Azure Kinect depth cameras by Microsoft in VL.  
Based on the [Azure Kinect Sensor SDK 1.4.2](https://github.com/microsoft/Azure-Kinect-Sensor-SDK).

BodyTracking via [Azure Kinect Body Tracking SDK 1.1.2](https://learn.microsoft.com/en-us/previous-versions/azure/kinect-dk/body-sdk-download).

Try it with vvvv, the visual live-programming environment for .NET  
Download: http://vvvv.org

## BodyTracking Prerequisites
- [BodyTracking SDK 1.1.2](https://learn.microsoft.com/en-us/azure/kinect-dk/body-sdk-download) (in the default install path!)
- [Cuda 11.4](https://developer.nvidia.com/cuda-11-4-0-download-archive) only if wanting to use Cuda instead of DirectML or Gpu as the "Processing Mode"

NOTE: 
- Node names are exactly the same as in the VL.Devices.AzureKinect.Femto NuGet, thus this is a drop-in replacement! 
- Once either VL.Devices.AuzureKinect or VL.Devices.AzureKinect.Femto was loaded, switching the NuGet reference is not enough to switch between them. A vvvv restart is also required after changing the reference!
- AzureKinect and Femto devices cannot run in parallel!

## Using the library
In order to use this library with VL you have to install the nuget that is available via nuget.org. For information on how to use nugets with VL, see [Managing Nugets](https://thegraybook.vvvv.org/reference/libraries/dependencies.html#manage-nugets) in the VL documentation. As described there you go to the commandline and then type:

    nuget install VL.Devices.AzureKinect

Once the VL.Devices.AzureKinect nuget is installed and referenced in your VL document you'll see the category "AzureKinect" under "Devices" in the nodebrowser. 

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
# PREACT/WUI-NITY
PREACT/WUI-NITY is licensed under the GNU General Public License v3.0, included third party source code have their own licenses attached.

Important Notice and Disclaimers before downloading the PREACT/WUI-NITY Modelling Platform Tool:
By accessing, downloading and using the PREACT/WUI-NITY Modelling Platform Tool, you expressly agree to the following notices and disclaimers, as well as the End User License Agreement found here.  
The tool simulates fire behavior, and human and traffic movement during a wildfire evacuation at the wildland-urban interface (WUI) and represents a way to enhance situational awareness of Authorities Having Jurisdiction (“AHJs”) as they plan and train for a potential WUI fire scenario.  
This tool and any related suggestions around evacuation are not designed to replace or substitute an AHJ’s decision about evacuation during a wildfire.  
Use of this tool is at the user’s own risk; it is provided AS IS and AS AVAILABLE without guarantee or warranty of any kind, express or implied (including the warranties of merchantability and fitness for a particular purpose) and without representation or warranty regarding its accuracy, completeness, usefulness, timeliness, reliability or appropriateness. 
The creators assumes no responsibility or liability in connection with the information or opinions contained in or expressed by this tool, its use or output.

## General
This is software under active development, and as such incomplete features and bugs are present. PLease report any isses on Github.

WUI-NITY (WUI-nity, WUInity) started as a simulation platform to combine pedestrian and traffic evacuation/simulation in combination with wildfire spread simulation that was built in the game engine Unity. As the software matured it was evident that decoupling from Unity was necessary due to multiple reasons (enabling non-GUI simulations on HPC, potential license/cost issues with Unity, etc.). It was also evident that it could be a useful tool outside of the wildfire realm, so a more general approach combining evacuation and any hazards evolved WUI-NITY into PREACT. WUI-NITY and Unity remains as a part of the software as pure visualizers, while PREACT is the simulation engine (PREACTcore more specifically). PREACT can be ran without using WUI-NITY and Unity via PREACTexecute, which is a command line tool.

The general architecture is as follows:
- PREACTcore is the simulation core of PREACT
- The entrypoint is Engine, which needs an IExternalManager, this is where either WUI-NITY or PREACTexecute hooks in.
- The engine manages one or multiple simulations, started with an EngineTask.
- The IExternalManager receives messages and is able is able to collect data from one simulation for visualization.

Simulations happen in UTM coordinate space, this is of importance for traffic simulation and fire spread simulations when dealing with data.

GDAL has been integrated in order to enable reading landscape files from GeoTIFF, WUI-nity only contains the "glue code" (C# API and C++ wrappers), 
the actual GDAL libraries are borrowed from the SUMO install (currently GDAL 3.9.3), and as such the correct SUMO version (see "Traffic modules") has to be installed for reading GeoTIFF.

A Mapbox access token (https://www.mapbox.com/) is needed to get the visualization of the area of interest, though strictly it is not needed to perform any simulation.
A completely free source for maps is desired, but of low priority right now.

## Requirements
As of now the dev branch of WUI-NITY only works on Windows due to external libraries.

You will need the appropriate version of Unity installed (download the Unity Hub and add the cloned project, it will tell you which version is required).

If you are using Visual Studio and C# it is likely that you have all required  libraries/frameworks, otherwise donwload and install the .NET runtime(https://dotnet.microsoft.com/en-us/download/dotnet-framework/net481).

Since installed SUMO dlls are linked, the logged in user account needs to have access to the PATH pointing to them. Being administrator of the system solves this.

## How to start WUI-NITY
WUI-NITY internally consists of two parts: WUI-NITY as a visualizer, and PREACT as the underlying simulation core. It is possible to run PREACT as a standalone command line tool which enables multiple simulations running aty the same time.
To get WUI-NITY to function it is reequired to compile PREACTcore as it does not make sense to include under development DLLs in source control. This is done by opening the PREACT soultion in
Visual Studio and compile the PREACTcore project. This will automatically copy all of the generated and referenced DLLs to the Unity project (WUI-NITY). Now it is time to open the WUI-NITY project in Unity.

Once the Unity project is cloned and loaded, the WUI-NITY scene has to be started, it is located \Assets\WUIPlatform\WUInity\Scenes and called WUInityMain.unity.
Once the scene is loaded, press play to start the program.

## Current status of modules
### Pedestrian modules 
- MacroHouseHoldSim: work as intended.
- Jupedsim: placeholder in code, hopefully integrated at some point through SUMO. 

A lot of changes in terms of how populations work has been made, the only required input now is a *.csv-file containing
Lat/Lon for household origin (house location), Lat/Lon of vehicle origin (this needs to be connected to the underlying road network, 
else vehicle will be teleported to a valid location), and number of people in household. More data columns will be added in the future.
This new approach means that any outside tool could generate the population and allow for interchangable data.

The old approach using Gridded Population of the World (https://sedac.ciesin.columbia.edu/data/collection/gpw-v4) 
is now instead a tool to generate the *.csv-file needed.

### Traffic modules
- MacroTrafficSim: broken due to changes in code structure, do not use, will maybe get fixed at some point again.
- SUMO: works and should be preferred more or less all the time. The version used to compile the current libraries used in WUI-NITY is 
SUMO 1.26 64-bit which therefore has to be installed on your system for it to run (as we do not want to re-distribute SUMO dlls). Make sure to install the version with all the extras as that is where GDAL is included.
It can be downloaded here: https://eclipse.dev/sumo/

OSM data for region of interest from can be found at e.g. https://www.geofabrik.de/ or any other source, extracting from www.openstreetmap.org
works great for smaller areas.

### Fire modules
- AscImport: allows import of results from Farsite, FlamMap, Prometheus and WISE or any other software as long as they output
*.asc-files. Needed outputs are; fireline intensity (named FI.asc), rate of spread (named ROS.asc), spread direction (named SD.asc),
time of arrival (named TOA.asc). This module is currently recommended as it is the most verified and validated approach.
- SimpleWildfireCA: There are actually 2-3 models in the code with slightly different approach, but none have been fully verified and validated, so not recommended right now.

WUI-nity supports both LCP and GeoTIFF files as landscape formats.

### Smoke spread modules
One smoke model is currently operational, the GlobalSmoke module. This allows the user to specify a global extinction coefficent that can change over time based on user specified input.
Multiple ideas for models exists in the code (box model, advect/diffuse, lagrangian), but funding is needed the develop, verify and validate.

### Trigger buffers
- k-PERIL (https://github.com/nikosuser/k-PERIL/tree/master) is now integrated into WUInity but has not been fully tested and there is currently an issue with 
sending the WUI area from WUInity to k-PERIL (hopefully solved very soon).
- One of the cellular automata models can be run in backwards mode to also generate trigger buffers, but it is not exposed for usage as it is under-developed. 

## Development
As WUInity is now publicly available we are happy to collect issues, bug reports, suggestions and pull requests. 
However, please keep in mind that nobody is developing the software full-time.
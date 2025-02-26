# WUI-NITY
WUI-NITY is licensed under the GNU General Public License v3.0, included third party source code have their own licenses attached.

Important Notice and Disclaimers before downloading the WUI-NITY Modelling Platform Tool:
By accessing, downloading and using the WUI-NITY Modelling Platform Tool, you expressly agree to the following notices and disclaimers, as well as the End User License Agreement found here.  
The tool simulates fire behavior, and human and traffic movement during a wildfire evacuation at the wildland-urban interface (WUI) and represents a way to enhance situational awareness of Authorities Having Jurisdiction (“AHJs”) as they plan and train for a potential WUI fire scenario.  
This tool and any related suggestions around evacuation are not designed to replace or substitute an AHJ’s decision about evacuation during a wildfire.  
Use of this tool is at the user’s own risk; it is provided AS IS and AS AVAILABLE without guarantee or warranty of any kind, express or implied (including the warranties of merchantability and fitness for a particular purpose) and without representation or warranty regarding its accuracy, completeness, usefulness, timeliness, reliability or appropriateness. 
The creators assumes no responsibility or liability in connection with the information or opinions contained in or expressed by this tool, its use or output.

##General
Under-development tool called WUI-NITY (or WUInity), combines pedestrian and traffic evacuation/simulation in combination with wildfire spread simulation.
Uses the game engine Unity as a visualizer.

This is the development branch of WUInity, and as such things are broken and undocumented.

Simulations happen in UTM coordinate space, this is of importance for traffic simulation and fire spread simulations when dealing with data.

A Mapbox access token (https://www.mapbox.com/) is needed to get the visualkization of the area of interest, though strictly it is not needed to perform any simulation.
A completely free source for maps is desired, but of low priority right now.

##Current status of modules
###Pedestrian modules 
-MacroHouseHoldSim: work as intended.
-Jupedsim: placeholder in code, hopefully integrated at some point through SUMO. 

A lot of changes in terms of how populations work has been made, the only required input now is a *.csv-file containing
Lat/Lon for household origin (house location), Lat/Lon of vehicle origin (this needs to be connected to the underlying road network, 
else vehicle will be teleported to a valid location), and number of people in household. More data columns will be added in the future.
This new approach means that any outside tool could generate the population and allow for interchangable data.

The old approach using Gridded Population of the World (https://sedac.ciesin.columbia.edu/data/collection/gpw-v4) 
is now instead a tool to generate the *.csv-file needed.

###Traffic modules
- MacroTrafficSim: broken due to changes in code structure, do not use, will maybe get fixed at some point again.
- SUMO: works and should be preferred more or less all the time.

OSM data for region of interest from can be found at e.g. https://www.geofabrik.de/ or any other source, extracting from www.openstreetmap.org
works great for smaller areas.

###Fire modules
- AscImport: allows import of results from Farsite, FlamMap, Prometheus and WISE or any other software as long as they output
*.asc-files. Needed outputs are; fireline intensity (named FI.asc), rate of spread (named ROS.asc), spread direction (named SD.asc),
time of arrival (named TOA.asc). This module is currently recommended as it is the most verified and validated approach.
- Cellular automata: There are actually 2-3 models in the code with slightly different approach, but none have been fully verified and validated, so not recommended right now.
- Placeholders for Prometheus COM interface (although it is dead now, so most likely will never be developed) and 
Farsite DLL (a version of Farsite source code exists in the public domain, given time this will get directly integrated at some point to allow two-way coupling which AscImport does not allow).

LCP files are currently the only supported landscape format, GeoTIFF will come at a later point.

###Smoke spread
In short, do not activate smoke spread module. Multiple ideas for models exists in the code (box model, advect/diffuse, lagrangian), but funding is needed the develop, verify and validate.

###Trigger buffers
- k-PERIL (https://github.com/nikosuser/k-PERIL/tree/master) is now integrated into WUInity but has not been fully tested and ther eis currently an issue with 
sending the WUI area from WUInity to k-PERIL (hopefully solved very soon).
- One of the cellular automata models can be run in backwards mode to also generate trigger buffers, but it is not exposed for usage as it is under-developed. 

##Development
As WUInity is now publicly available we are happy to collect issues, bug reports, suggestions and pull requests. 
However, please keep in mind that nobody is developing the software full time.
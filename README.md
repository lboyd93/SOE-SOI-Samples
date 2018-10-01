# SOE-SOI-Samples
This repo contains samples of ArcGIS Server extensions.

## SOE Sample - ChangeSymbology
This is an SOE sample that changes the symbology of the layers in a Map Service that is hosted on ArcGIS Server 10.6.1.

Depending on the current color of the symbology, the renderer will change the symbols a different color.

## SOI Sample - DoNotUseThatWord
This is an SOI sample that intercepts REST requests sent to the MapService that is hosted on ArcGIS Server 10.6.1.

If the REST resquest contains the word "Change", an error is logged in the ArcGIS Server machine.

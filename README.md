# Qlik Healthcare Analytics
Provides medical personnel access to analytical information in the field through Augmented Reality

This Unity application will display a 3D Sankey visualization from Qlik Sense. It has been designed and built for the Microsoft HoloLens. 
When a user starts the application they will see a woman in distress on a gurnee. 
Above her is a visualization of the many different procedures a doctor may perform. 
To the left of the viz, there is a filter panel where users can select the specific demographics and symptoms for the patient.
As selections are made, the 3D visualization is updated.

[![Video documentation of Healthcare Analytics](https://img.youtube.com/vi/1g1G2TjnJdw/0.jpg)](https://www.youtube.com/watch?v=1g1G2TjnJdw)

## Natural Language
Users can make selections either through the standard HoloLens "Air Tap" or through verbal commands such as *Male, Female, Smoker No, Overweight, clear, etc.*

A brief verbal summary of the 3D visualization is provided by Yseop through Natural Language Generation and text-to-speech.

## Requirements
- *Used in conjunction with  [Qlik Healthcare Node Server](https://github.com/ImmersiveAnalytics/QlikHealthcareNodeServer)*
- Must have Qlik Sense Server application running with Node.js app
- Must have Unity HoloLens 5.4.0b24-HTP installed
- Must have Visual Studio installed

## To Run
Build a standalone app for HoloLens using Visual Studio. [Instructions](https://developer.microsoft.com/en-us/windows/holographic/exporting_and_building_a_unity_visual_studio_solution)

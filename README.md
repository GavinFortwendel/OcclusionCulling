# Occlusion Culling

This project is a dynamic occlusion culling simulator that allows the user to move around 3 different environments: a basic scene, a house scene, and a city scene. One camera in the environment represents a user view, and another camera shows an overhead view of the environment so that the occlusion culling effect can be seen. Any object that is not within view of the camera or that is occluded by another object will not be rendered, which cuts down on the number of tris and vertices that are rendered. This results in a speed up of the program, and allows complicated scenes to be rendered with less stress on the CPU.

## Demonstration

[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/RM5Ckgr_Rn8/0.jpg)](https://www.youtube.com/watch?v=RM5Ckgr_Rn8)

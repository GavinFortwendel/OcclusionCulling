# Occlusion Culling

This project is a dynamic occlusion culling simulator that allows the user to move around 3 different environments: a basic scene, a house scene, and a city scene. One camera in the environment represents a user view, and another camera shows an overhead view of the environment so that the occlusion culling effect can be seen. Any object that is not within view of the camera or that is occluded by another object will not be rendered, which cuts down on the number of tris and vertices that are rendered. This results in a speed up of the program, and allows complicated scenes to be rendered with less stress on the CPU.

## How It's Made

**Tech Used:** Unity, Blender, C#

All of the complex models used in the house scene and the city scene were created in Blender by the co-creator of this project, Nicholas Reardon. These models were then imported into Unity, and lightmaps were created for these scenes to make sure the lighting was dynamic and looked good on the models. I programmed the occlusion culling algorithm, the frustum culling algorithm, and the BVH that holds the bounding boxes of the models. The occlusion culling algorithm works by shooting rays from the player object to specific spots (the center, edges, and corners) on the bounding boxes stores in the BVH. If at least one of the rays hits the bounding box without hitting another object first, the object is marked as visible, and it is rendered. The frustum culling algorithm works by not rendering the objects within the bounding boxes that are not within the camera's frustum. If any part of the bounding box is within the camera's frustum, all of the objects inside of that bounding box are rendered. The BVH is the data structure used to effectively store and organize the objects within the scenes. It organizes the bounding boxes in a hierarchial structure, at at the top of the hierarchy is the root node, which represents the bounding box that contains the entire scene.

## Optimizations

The occlusion culling algorithm and frustum culling algorithm effectively utilize the BVH to only perform visibility checks on the bounding boxes necessary. This works by marking the nodes containing the bounding boxes as visible and invisible, and if a node is marked as invisible, none of the children nodes are check for visibility since their parent is invisible. This accelerates the process of determining intersections or collisions by reducing the number of visibility checks needed. Also, as the scenes become more complex, this optimization proves to be even more valuable, as a greater number of objects in the scene aren't rendered.

## Demonstration

The Youtube video below shows the 3 different environments and how the occlusion culling works in those environments.

[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/RM5Ckgr_Rn8/0.jpg)](https://www.youtube.com/watch?v=RM5Ckgr_Rn8)

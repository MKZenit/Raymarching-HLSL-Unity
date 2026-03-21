# Raymarching-HLSL-Unity
Preparation for a game made with raymarching. Allowing a more mathematical than artistical world description.
Let's experiment on how performant a raymarching algorithm can be in more and more similar situations than what is to come for the game.

- distance calculation : 
  - Most primitive SDFs are from this site : https://iquilezles.org/articles/distfunctions
  - then refined and optimized for efficient computing

- normal calculation :
  - no general method, specific to each implicit geometry type
  - computes once the normal instead of 6 sdfs, with higher precision.

- debug modes :
  - various views to debug functionalities and gain insight over functionalities interactions.

## Installation guide :

(Odin Serializer and Odin Inspector pricey packages are used in this project and are needed to compile and show inspector correctly)

- Install Unity Hub : https://unity.com/download .
- Launch Unity Hub and install a newer Unity 6 version.
- Create a new project with this version of Unity.
- Replace the "Assets" folder from this project with this repository's one.
- Open "Assets/Scenes/SampleScene.Unity".

# Important classes and functionnalities

- The main component of this project is the **RenderingPipeline** implementation of **CustomPipeline**, that actually works as any pipeline. It may work for any per-pixel compute shaders (hence also for raytracing, without using accelerating architectures).

<img width="648" height="285" alt="image" src="https://github.com/user-attachments/assets/c85a6b40-9bf0-4bb2-bb43-078a1e0777ca" />

Pipeline helps sharing resources, especially parameters held by a kernel to share it through other kernels as needed using Singleton based resources names. Pipelines also hold their computations when a shader is updating which allow shaders hot-reload.

- An example of kernel instance, pipeline allowed classes are implementations of CustomKernel MonoBehaviour.

Here is such an instance :

<img width="1148" height="825" alt="image" src="https://github.com/user-attachments/assets/1dff4255-11b9-4329-af1b-753ec683ea66" />

Here is the related shader :

<img width="1148" height="825" alt="image" src="https://github.com/user-attachments/assets/aae8f2d1-f6ce-4e1e-b964-550f5d4423cf" />

Here is the inspector component :

<img width="649" height="179" alt="image" src="https://github.com/user-attachments/assets/9a5f68b4-2c05-4f8f-a0b1-b8e02001a784" />

(Functionnal note : Camera datas are managed through **ApplicationGpuResources** That allows sharing whatever value with the pipeline in order to load these values in kernels)

- Geometries are loaded randomly for now, later it will be necessary to implement a SdfModeler addon for these specific implicit volumes in order to save/load through serialized datas.

<img width="666" height="508" alt="image" src="https://github.com/user-attachments/assets/9dceba62-bb5f-4937-9ad7-f509e873f785" />

# Render

- Normals

<img width="1238" height="769" alt="image" src="https://github.com/user-attachments/assets/4871fe0e-7aeb-46e0-8365-51a7f8b3f4fb" />

- Depth

<img width="1240" height="767" alt="image" src="https://github.com/user-attachments/assets/828ef5df-2d68-4bf0-980d-72363411e54e" />

- Result

<img width="1243" height="778" alt="image" src="https://github.com/user-attachments/assets/f36ecca8-d3b7-4d65-8a93-4ff8a31b0803" />

## Debug

- Iteration counts (How many times a pixel has iterated in a specific condition, the less constrained condition shown here is a "size encapsulation test")

<img width="1241" height="770" alt="image" src="https://github.com/user-attachments/assets/c9dc8749-f43a-417a-b486-9d4d936fe569" />

- Iteration counts depending to a specific frame to study divergence (green has already converged at that frame, blue pixels are converging). *Render of "frame 4" iteration counts*

<img width="1240" height="769" alt="image" src="https://github.com/user-attachments/assets/8ea70cfe-9b0f-4be9-b21c-0b92b1757492" />


(Note : there is for now no volume exclusion, which is currently a work in progress..., a reason to this being all 4 geometry types used are convex and for optimizations from a destructive point of vue, i shall consider those geometries as concave hence most optimizations are not reusable)

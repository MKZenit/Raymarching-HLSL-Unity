# Raymarching-HLSL-Unity
Preparation for a game made with raymarching. Allowing a more mathematical than artistical world description.
Let's experiment on how performant a raymarching algorithm can be in more and more similar situations than what is to come for the game.

distance calculation : 
  Most primitive SDFs are from this site : https://iquilezles.org/articles/distfunctions
  then refined and optimized for efficient computing

normal calculation :
  no general method, specific to each implicit geometry type
  computes once the normal instead of 6 sdfs, with higher precision.

debug modes :
  various views to debug functionalities and gain insight over functionalities interactions.

# Installation guide :

- Install Unity Hub : https://unity.com/download .
- Launch Unity Hub and install a newer Unity 6 version.
- Create a new project with this version of Unity.
- Replace the "Assets" folder from this project with this repository's one.
- Open "Assets/Scenes/SampleScene.Unity".

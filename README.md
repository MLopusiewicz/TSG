# How to run:

## Bake
* Bake using window Tools/Bake Window. 
* Click AutomaticBounds to get bounds encompassing all SDF items in scene. 
* Use Margin of 2+. 
* Press Bake
	* Results are stored in Levels/{scene name}\_data/

## Simulation
- Change simulation setting in GameObject: "SimulationLogic"
- Use scene UI to simulate.

## Debug
- Red Boxes: SDF item bounds
- Green box: Baking bounds
- Blue box: Simulation spawn bounds

There are gameObjects in "Debug" with component to display voxel, gradient and read values.

# Approach
## Packages

No packages were added over the ones already in the template. But used:
* Burst
* Mathematics
* Collections

## Baking
Baking is based on SDFItem - MonoBehaviour added to scene objects. Mostly for easy, manual setup.  SDF items lack rotation support in and nonuniform scaling in cylinder.

Baking is controlled with editor window.

**Output** is saved in Texture3D (raw distance data) and ScriptableObject holding texture reference and baked volume data.

Baking could be moved to Burst as well to speed up baking, currently low priority.
## Simulation

There is one parallel job to calculate position of the balls. Position and velocities are split to different arrays (instead of 1 struct per ball) as it's faster to move positions to GPU later on.



**Simulation logic** - handles simulation life time, jobs and render 
Simulation lifetime is controlled in coroutine (faster than writing bool checks in update).



## Render

* using ico sphere mesh (basic blender mesh with no subdivisions)
* removed normals in import (vertex position is a normal in sphere - 0.8KB change)
* 2 pass shader - render and shadow pass




## Stress test

FPS fell below 60 with 300k balls.


# Future
- more data safety checks (material is GPU instanced, volume is not null etc.)
- Divide *SimulationLogic*  by responsibilities into smaler scripts.
- 
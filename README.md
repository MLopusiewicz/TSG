# Outcome:

**CPU**:  13th Gen Intel(R) Core(TM) i9-13900KF (3.00 GHz)
**GPU**:  NVIDIA GeForce RTX 4080 (16 GB)

![[Images/scene.png]]
![[Images/profiler.png]]

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
- starts with 10k balls, use buttons to resimulate

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
Plane is baked as BOX with offset to give it volume. Depending on the expected speed the volume might need to be thicker so the balls don't tunnel. Other possible method would be to make distance <0 under the volume in general. Skipped that approach as it's constraining the baker too much.

The distance outside of the SimulationVolume is calculated as distanceToVolume and distance in that voxel, thus there could be small difference in real distance and calulated one.  That's why there is margin added when baking.  

**Output** is saved in Texture3D (raw distance data) and ScriptableObject holding texture reference and baked volume data.

Baking could be moved to Burst as well to speed up baking, currently low priority.

## Simulation
There is one parallel job to calculate position of the balls. Position and velocities are split to different arrays (instead of 1 struct per ball) as it's faster to move positions to GPU later on.

Simple contact model  with friction, bounciness and drag.

**Simulation logic** - handles simulation life time, jobs and render 
Simulation lifetime is controlled in coroutines: Job is scheduled in FixedUpdate so the calculations run in fixed time, render is set every frame. 



## Render
* using ico sphere mesh (basic blender mesh with no subdivisions)
* removed normals in import (vertex position is a normal in sphere - 0.8KB change)
* 2 pass shader - render and shadow pass, 

## Stress test
**CPU**:  13th Gen Intel(R) Core(TM) i9-13900KF (3.00 GHz)
**GPU**:  NVIDIA GeForce RTX 4080 (16 GB)

FPS fell below 60 with 300k balls in basic setup.

bottleneck is GPU: rendering balls into shadowmap exactly (takes up to 75% of time). With shadow cascades  4 -> 1, it reaches up to 800k balls.

# Future
- more data safety checks (material is GPU instanced, volume is not null etc.)
- Divide *SimulationLogic*  by responsibilities into smaler scripts.
- Possible decoupling of rendering positions from position array, so physics could calculate independently from visuals. This would allow the job to run during the frame (as right now it's stopping the thread to wait for end). As the bottleneck is GPU not CPU it is what it is.

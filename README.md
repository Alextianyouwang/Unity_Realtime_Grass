# Real-time grass solution for Unity URP
![ezgif-4-574c809eb1](https://github.com/user-attachments/assets/3569d505-2e2f-4ced-84ac-995e12a74288)

inspired by Ghost of Tsushima GDC talk, rendered as is without post-processing, project version: 2022.3.19f1
* updated to 6000.2.14f1
check out the demo in Main
  
General Features:
- Independent of terrain geometry (height map driven)
- Height map capture tool
- Paintable density mask
- Support rendering multiple types of detail objects
  
Technical Features:
- Indirect instancing
- Prefix sum GPU culling with reduced bank conflict (frustum, distance, occlusion, and mask)
- Bilinear interpolation to smooth pixelation on height map
- Local DDXY for ground normal
- Custom tilling Voronoi buffer for clump visual
- Bezier curve animation and per-blade normal by derivatives
- Sample buffer from spacial hash to minimize per-vertex noise sampling
- Data partitioning and LOD system
- Global wind system simulated by FBM

Visual Features:
- Multiple light support
- Realtime shadow with bias logic
- GI from probes and SH
- Fast sub-surface scattering 
- Alpha clipping
- Tangent space normal
- Albedo atlas for variations



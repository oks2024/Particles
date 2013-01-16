This is a work in progress statefull particles engine running entirely on the GPU and using XNA.

Each particles velocity and position is stored in a texture, and used to update the system.

Particles can follow flowmaps, collide with anything rendered on a texture, be attracted, etc.
You con control particles using the mouse or a kinect sensor.

Interactions:

Left mouse button: 	Attract particles.
RightMouse button: 	Repulse particles.
M:					Mouse control.
K:					Kinect control.
C:					One color for particles.
T:					Background texture for particles color.
V:					Color given by the current velocity.
F:					Postprocess to give a solid/fluid.
Backspace:			Reset particles.
G:					Activate text collision.
N:					Enter a new string.
Enter:				Validate the new string.
1 - 4:				Use CV as collision texture.
5: 					Use string as collision texture.
B:					Show buffers datas.
S:					Activate/Deactivate inertia.
Q:					Enable/disable gravity.
D:					Activate/deactivate flowmaps.
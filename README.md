# Procedural Island
## Description
### Island form
The islands are generated using 3 values. First of all, I generate a Perlin noise texture in a compute shader. Using this texture as a height map yields a random landscape. I then change the values of the Perlin noise texture with a Unity Animation Curve. This allows me to control the shape of the islands
more. I also use Voronoi noise to generate a random falloff Map when I blend the falloff map with the Perlin noise I get the shape of my islands.

![image](https://github.com/user-attachments/assets/418093ee-0fb1-4f90-9fec-6a9ca419e8e8)

### Island color
For the material of the island, I made a custom shader. this shader uses a height array and a color array to change the color of the mesh in the fragment shader. I also added my own specular and diffuse functions to this shader to add more detail to the islands.
### Biome editor
I also made a biome editor that lets you make your own biomes. Each biome has a name, an Animation Curve, a list of regions, and an axiom. you can use the animation curve to get more interesting results on the form of the island,
 and you can use the region list to change the color and the height of those colors like this.
 
![image](https://github.com/user-attachments/assets/c3c54e9a-a3e3-451f-90e3-d4b5f0dbc6bd)
![image](https://github.com/user-attachments/assets/83b77aed-1df2-4909-aacc-748663ace26d)

Originally I also wanted to add textures but due to my inexperience with Texture2DArray's in hlsl I wasn't able to add this. Lastly with the axiom you can change the axiom of the tree generator so that different biomes have different trees.
### Compute shaders
For this project I made 2 compute shaders one of them I wasn't able to completely finish due to the complexity of the task. the first compute shader is used to generate a Perlin noise texture and a Flowfield. The reason that I generate the Perlin noise in a compute shader is because it is way better for
the performance of the game. The second one is used to generate a texture that contains the rivers but like I said earlier I didn't have time to finish it.


### TODO
I will still be working on rivers as I find this topic intriguing. I have decided that i will generate the rivers based on the voronoi instead of using the flowfield as this will be less computationally expensive and easier to make. This is the vision that I have in mind.

![voronoi based rivers](https://github.com/user-attachments/assets/269d0b46-dc51-403c-85ea-019a8dbbc670)

I will be using the Voronoi point as the starting point and let it go to a random point at the edge of the mesh.

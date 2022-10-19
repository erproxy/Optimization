/*
README

Skinned Mesh Combiner Lite
Lylek Games


C# FOR IN-GAME USE
/**

using LylekGames.Tools.SkinnedMeshCombiner;

CombineSkinMeshesLite myCombine; //script reference

myCombine.BeginCombineMeshes();		//combines the Skinned Meshes of your character
myCombine.DissasembleMeshes();		//removes the combined mesh and re-enables the individual Skinned Meshes

*/

FOR USE IN THE EDITOR
Select your character and choose Tools > Combine > Skinned Meshes, or drag and drop the CombineSkinMeshesLite.cs script onto your character.
Follow the prompt for assigning a character armature, and then for correcting the character scale and orientation if required. Once the character 'Looks good!',
press Initiallize. Feel free to modify any settings, such as the material properties, and the desired atlas size. Scroll to the top and press 'Combine Meshes'.

IMPORTANT
- For proper texture atlasing, each material must contain a texture for atlasing. For example, you cannot have some materials use normals maps and others not.
If one of your materials does not require such a texture, please use a texture that will add little-to-no affect, such as the provided 'Default Textures', located
in Resources / DefaultTextures. Or check the box to exclude this texture type completely.
- Each texture of a material must be of identical size. For example, if your BaseMap is 512x512, your normal map and metallic map must be 512x512 as well.
Different materials may use different sized textures. 
- All textures must have Read/Write Enabled, in the Import Settings. Within the Atlasing Properties section of the script, you can press "Force Read/Write Enabled"
to enable this for all textures without trouble.
- Note that not only are the textures on your materials the same size, but that the MaxSize compression in the Import Settings are the same size as well.
- Does not support MeshRenderers or other meshes parented to the character's armature or child bones.
- Requires Unity Standard material.
- There is a vertices limit, in Unity, for any given mesh. The combination of meshes cannot exceed this limit.

SUPPORT
Please, by all means, do not hesitate to send me an email if you have any questions or comments!
support@lylekgames.com, or visit http://www.lylekgames.com/ssl/contacts

LEAVE A REVIEW
Please leave a rating and review! Even a small review will help immensely with prioritizing updates.
Assets with few and infrequent reviews/ratings tend to have less of a priority and my be late or miss-out on crucial compatibility updates, or even be depricated.

I have included a 'RateSkinnedMeshCombinerLite' script which will propmpt the user (you!) to rate this asset. This prompt
should only ever appear the one time, regarless of your choice. If there are any issues with this prompt, please let
me know, or simply delete the script located directly in the CombineSkinnedMeshes folder.

Thank you! =)

*******************************************************************************************

Website
http://www.lylekgames.com/

Support
http://www.lylekgames.com/contacts
*/

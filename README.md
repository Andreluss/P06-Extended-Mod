# P06-Extended-Mod
## 2024: Plugin update!
I'm working on porting this project into an independed _plugin_ format. The plugin version:
- only adds code as a prefix or postfix of a given function
- doesn't overwrite existing code
- heavily relies on C# Reflection
This constraints make the modding process more challenging, but also make it possible to run this mod with other mods! 
  
New repo: [P06X-Plugin](https://github.com/Andreluss/P06X-Plugin) 🎉

## Description
<p><b>P-06: eXtended</b> is a mod (made by me, aka 4ndrelus) for <b>Sonic P-06</b> (a game developed by ChaosX). <br/>
This repo contains new files, and fragments of the original files, containing extra content added or changed by the mod. 
Basically, every class, function and variable with <i>"X"</i> or <i>"X_"</i> prefix has been created and added to the game. </p>
<image src="https://user-images.githubusercontent.com/64368904/212337691-a8486d1a-a3d2-43a2-8296-800dc5d21f92.png" />

<p>Demo video: <a href="https://youtu.be/bUYLfmtYh9E">Youtube</a></p>
<foot>The modding process requires decompiling the C# Assembly, making changes and recompiling it, <br/>so the code may have some visible side effects, like redundant <code>this</code> and <code>base</code> keywords.</foot>

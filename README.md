# phosphorite
<div align="left">
 <img src="https://github.com/BrokenSt0ne/phosphorite/blob/main/preview.png?raw=true" width=50% height=40%</img><br>
 <a href="https://github.com/BrokenSt0ne/phosphorite/releases/latest">
 <img src="https://img.shields.io/github/downloads/BrokenSt0ne/phosphorite/total?label=Downloads&style=flat-square"<img></a>
</div>
A WIP Experimental real-time lighting mod for Gorilla Tag using the GorillaCorp lighting.

## Features
* Realtime lighting using the GorillaCorp lighting.
* Full control over:
  * Light Positions
  * Color
  * Intensities
* Load/save lights via JSON
* Simple in-game UI
* Works in both **modded** and **non-modded** lobbies since it doesnt give an advantage.
* Changing already created lights.

## Usage
You create and load lights via a menu opened by pressing the `V` key on your keyboard (they do *not* autoload as of now).

In the UI you should be greeted with options for position, intensity and color and a couple utility buttons beneath.

To load a json, click the `Load Lights from JSON` button in the ui. [Heres a good example of a json.](https://github.com/BrokenSt0ne/phosphorite/blob/main/data.json) *(note that it must be called data.json as of now and placed inside the folder your phosphorite plugin is in)*

**Warning**: Spawning too many lights will cause lag. Use wisely.

**If it doesn't look like it's working, set your ambient color to #000000 and add a light at your location**

## Contributing & Issues

* Found a bug? [Submit an issue](https://github.com/BrokenSt0ne/phosphorite/issues)
* Know your way around code? Pull requests are welcome!

## Compatibility

* Works in **non-modded lobbies** (visual-only, no advantage)
* May give **slight advantage** if used with mods like [PlayerGlow by The-Graze](https://github.com/The-Graze/PlayerGlow)

###### *This product is not affiliated with Another Axiom Inc. or its videogames Gorilla Tag and Orion Drift and is not endorsed or otherwise sponsored by Another Axiom. Portions of the materials contained herein are property of Another Axiom. ©2021 Another Axiom Inc.*

### Humble Beginnings

This is more or less covered by the ["AI" detour](#ai-detour) above, for those who stayed with me :-). After surprisingly complete failure by Gemini CLI to create even a scaffolded empty solution, I had to roll the sleeves and do the usual thing - copy/paste an existing project - in this case my "Bin Blitz" game - and strip it down to bare minimum. Not much else to tell here. 

At the end of this phase I had a simple profile of the "ground"; plane that could "fly" oblivious to its surroundings; and a (not-a)-dashboard with a few indicators. I've also made a regretful fundamental choice (of my plane texture, not less) that will come biting me back in the future with all its might.

The basic code organisation was settled upon, as well as some conventions and "counter-conventions" going against the grain of industry and FNA prescriptions. To name a few:
- FNA-prescribed "component lifecycle machinery" deliberately ignored, in favor of explicit management.
- Explicit refusal to diligently follow "Disposable pattern", that would only add noise and no benefit to FNA game project.
- FNA-prescribed "Components" split into "Core" part - defining an in-world object and behavior, and "Component" part only concerned with rendering of the Core.

### Animating

What is the main capability expected of an airplane *in a video game*? Why, exploding spectacularly when hitting the ground, of course! And what exactly is an *explosion*? A *sprite*, naturally. Trivial.

I borrowed the primitive Simulation proto-pipeline from my "Bin Blitz" project and implemented simple handling of contacts resolved via the Aether.Physics2D engine, slapping an explosion sprite at the contact point.

Stop. It doesn't look right, does it? Because an explosion is not a static sprite, it is an *animated sprite* (a sequence of static sprites). There are a few good spritesheets for explosions on OpenGameArt.org, but FNA does not provide any animation facilities out of the box. Neither does SDL, which sits even lower in the stack.

My reasoning for the first implementation of animated sprites was simple: a function taking a collection of "phases" with associated textures, plus a core object implementing an abstraction "I can provide a value to pick the current phase". Since that first attempt, the implementation evolved a few thin layers, but stayed true to that reasoning.

The primitive explosions I used early in the game allowed for an even more simplified abstraction that did not require passing in a Core object: given a starting time-point, render a given sequence of N frames uniformly over a given duration. Surprisingly simple and robust.

My animation facility allowed me to implement plane explosions on the ground, and later bomb and bullet explosions of various kinds, sizes and durations.

Later on, after finalizing the TextureAtlas abstraction and consolidating drawing facilities, the animation system was trivially adopted for rendering objects in permanent motion. See the "Plane wings go byak, byak, byak, byak" commit and a few earlier ones.


### Taking it for a Spin!

### Body-Building (no, not of *that* kind)

### All things Ground

### All things flying

### Simulation Voodoo

### Multi-part Multi-sprite Objects

### Textured World

### The magic of phosphorus-backed CRT 

### Fountains and Water Guns

### Nage.Strata

### Oh no, I shouldn't have used that texture

### Will it fly on Linux?

### Will it fly in browser???

### Code Organisation

### Plumbing the Leaks

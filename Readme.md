# VibeSopwith.Game.FNA Project

## What it is

I present you a personal experimental "game" project I started with two goals in mind:
- To further my learning of game development techniques, particularly in FNA/XNA space, and
- To explore potential to productively utilize "agentic AI" workflows for game coding. 

My inspiration was drawn from the venerable 1983 DOS game "Sopwith" ([Wikipedia link](https://en.wikipedia.org/wiki/Sopwith_(video_game))), or "Fly" as I knew it in the 1990s (because the executable  file was named "`fly.com`", of course - or was it "`fly.exe`"?), and its faithful [modern replica](http://www.sopwith.org/)

Now, if you stumbled upon my work while seeking only technical information associated with FNA/XNA, and you aren't interested in my musings, feel free to skip directly to the [next part](#to-higher-places). Everyone else who is willing to persevere, buckle up!

Having advanced a bit alongside the development arc I can confidently state that the project delivered a lot of value for me on the former goal, while failing utterly from the get-go on the latter. 

### "AI" detour

Let me get that latter statement out of way before I focus on the important matters. As evident from the name itself, the project has started with hope to unleash the "power" of modern "artificial intelligence" for realization of my "creative visions". Particularly of modern "agentic" tools and workflows. I harbour(ed) enough scepsis towards "vibe-coding" to be totally unenthusiastic to try it, but as it is sometimes attributed to Niels Bohr - "a horseshoe brings luck whether you believe in it or not" - I decided to jump in with open mind. 

I've already had another FNA game project started where I somewhat settled on chosen architecture, composition and code organisation. That project was/is called "Bin Blitz" (I am staking the name here :-)) and involved a garbage truck with moving arm and scattered around garbage bins of different colors - I may open up the repo some day. So the first task to "agent" (I tried VCode Copilot and Gemini CLI) was to research "Bin Blitz" code and identify/extract useful "patterns" and "invariants" (Copilot loves this word); to create a plan for Sopwith-inspired game; and to attempt reverse-engineer a prompt with which "Bin Blitz" could have been YOLO-ed into existence. Or even not YOLO-ed, just methodically brought into. 

It would be an understatement to say that both VSCode Copilot and Gemini CLI failed on the task. They *utterly, completely, undeniably, irredeemably failed*. I wasn't "vibing" at all - I spent the whole day arguing, steering, nudging, explaining, pushing, reframing, persuading, bribing, cajoling,  filling in markdown files of all kinds. I probably typed more text in aggregate prompts + markdowns than there was code in "Bin Blitz". Well, just kidding, but you get what I mean. In the end there was not a shred of code or another artifact out of that exercise that could be used in further development. The included [Architecture.md](src/Docs/Architecture.md) is a *fully authored by me* document attempting to show the agent what it *should have identified* and *how it should have presented the findings*. Yet even with that all my attempts were proven futile.

Some examples, from the top of my memory:
- Gemini CLI "identified" and adamantly insisted on strict adherence to "Model-View-Controller" pattern. My retort that not every case of logic/rendering separation fits MVC definition fell on deaf "ears".
- Both agents "identified" a lot of very trivial, superficial "patterns" while completely missing those that I considered fundamental for my code.
- I had to fight tooth and nail against their push towards certain "industry conventions", stylistic and structural. As a developer in the third decade of industry experience I am, of course, well aware about most conventions and reasoning behind them. But as a solo proprietor of this project I took liberty of intentionally deviating, not caring of, or even inverting some - in favor of locally-optimal well-justified patterns. 

All that I could take out of two days dancing with agent is another bucketload of evidence to add to my stash of "why agentic LLM coding isn't practical outside of few very explored well-patterned niches" - to put it mildly :-). If, for some reason, you want to hear more about it, contact me.

Saying so, I generally find LLMs of chatbot variety a very useful tools of assistance. So, while not a single line of VibeSopwith was written by *agentic* LLM, and the only remaining references are in the name and stale "Architecture.md", about 5%-10% of the codebase was assisted by Microsoft Copilot in some way. Usually of explanations, "framing" or "turbocharged search". Some local functions, especially with heavy-ish geometric transforms, were written by Copilot - although I ended up brushing line-by-line through each and every one of them, exterminating subtle bugs and wrongly interpreted "invariants" (did I tell you Copilot is in love with this word?) 

And while I hadn't forgotten. My attempts to use image-gen "AI" models to generate assets for this game fell flat even lower than lowest. I can confidently assert that useful overlap between current capabilities of image-gen models and needs of pixel-art-ish game developer is zero. If I was willing to climb the learning curve of tools like Comphy, I could probably move forward within epsilon-proximity (from the left! :-)), but I wasn't.
 
**TL;DR:**  "Agents" aren't there yet - at least for me. Chatbot LLM was useful. The codebase is 100% architected by me and 90% coded exclusively by me, and there is no nook or cranny in the remaining 10% which I'm not as familiar with as my own code. 

### To higher places!

Now that rant-of-frustration is behind me, I can proceed forward "Вперёд, к сияющим вершинам!"
So, what it is:

- An FNA-based game-like application demonstrating so far emergent techniques for
  - Composable rendering pipeline
  - Composable simulation pipeline with Actors and proto-ECS
  - Physics and collision simulation (I use Aether.Physics2D)
  - Spatial object composition
  - Animation pipeline
  - Code organisation
  - Assets organisation
  - Particle system simulation and rendering
  - Multi-platform deployment, inclusing browser-wasm
  (See the important section [below](#what-it-is-not) for clarification)  

- A coding toy that I love to casually fire up - no, not to play - but to add or ponder another "feature", untangle another code conundrum or dare to try something I thought wasn't possible - which inevitably produces more conundrums for future entertainment!

- A basis to try out game building blocks set to be extracted into my "Not-a-Game-Engine-Strata" collection.

- Hopefully a useful example of FNA-based project for people who, like me, are frustrated with "big engines" and want to try their hand with FNA, but unable to find a better starting point. 

- A "portfolio item" which I hope to be able to point to, providing potential answers to certain questions from hiring teams around "ways of reasoning", "communication" or "raw coding ability". 

## What it is not

- Not a finished game.
- It may not even be called a "game" - so fart there is no score, goals, levels or achievements of any kind.
- Not a tutorial or a general-purpose FNA template. All patterns here were discovered or stumbled upon by me during my experimenting, leveraging 25+ years of general software development experience. There is absolutely no assumptions to be made about them being conventional, endorsed by industry or suitable for larger-scale project.
- Not an example of "AI-driven development", let alone "Vibe-Coding".
- Not an example of "Anti-AI-driven development", let alone "Warm Vacuum-Tube Development".
- Not a polished product and never will be - it is an experiment, a learning tool, and a playground.
- It isn't a clone or replica or remake or even a "spiritual child" of original DOS "Sopwith", and uses no code or assets from that game. It is simply inspired by it.
- It is not an example of ossified set of rules or conventions or lack thereof that I am going to defend no-matter-what when working with a team. I fully understand the realities of team-based development and resulting need for coordination, conventions and compromise. 

## The Journey

Below are brief description of various "milestones" or "stumbling blocks" I had to overcome on a path to current spectacular state. Not in any specific order; everything is corroborated via commit history.

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

### Flying, Landing and Autopilot 

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



## Licensing

For asset attribution see [Readme.md at Content Repo](https://github.com/fontmaniac/VibeSopwith.Content)

## Collaboration

Feel free to fork and mutilate this "game" to your liking. 
If you work in game development and see potential for collaboration, I would appreciate being considered.


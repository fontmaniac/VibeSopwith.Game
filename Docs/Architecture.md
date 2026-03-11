# Predicates about architectural and stylistic choices and patterns to keep invariant throughout the code.

The list is unordered and ungrouped.

- Game uses FNA framework.
- Some "prescriptions" of FNA are explicitly rejected in favor of custom pattern.
- Pattern: "components" in FNA term do not use FNA-provided lifecycle management. 
  I do not "register" them and thus do not expect them to be handled by framework. Instead I "manually" call LoadContent, UnloadContent, Update, Draw, etc., as I feel I am constrained by FNA framework more than I am being guided.

- The conventional "disposable" pattern is not followed, deliberately. 
  Also the release/disposing of resources is not consistently implemented. The reasons are:
  - A game is an application which follows rather stable and rigid lifecycle. It is initialized once, runs until user explicitly closes it and then all objects are destroyed at once. As such, being .NET-based, under normal circumstances GC destroys all objects and executes all finalizers upon exit, whether they are accurately wired through the object tree or not. This is why I find consistent adherece to conventional "disposable" pattern wasteful. It only adds unnecessary verbosity.
  - On top of the above, some FNA objects like SpriteBatch, for example, and others behave deceptively. Despite superficially implementing IDisposable interface they are entangled so deeply with some static GPU-bound state that treating them as anything else than "static singletons" causes fatal errors. 

- `Core` folder contains code for game objects and actors. 
- "Core" classes represent predominantly logic-level code, wherever possible separated from UI/rendering.
- "Core" classes are defined in the "world space", not mixed with "screen space".
- As the application is not a library meant to be referenced from other assemblies, all classes should be declared "internal", not "public". Where they are "public" might be due to reasons, or simply an omission on my side.
- `Components` folder holds implementation for rendering routines.
- "Component" classes usually correspond to their counterparts from "Core" set, but not always. There may be "components" which render some custom UI not tied to a single "Core" concept.
- "Component" classes derive from FNA "DrawableGameComponent" base class, to streamline access to some "global" properties that FNA provides.
- "Component" classes do not hold any references to the underlying "Core" objects. Relevant "Core" objects are passed in when needed, mainly into `Draw` method or similar.
- "Component" classes are responsible to "LoadContent" relevant to their function - textures, fonts, etc. The references are held for the full lifetime of the object. 
- Sometimes it makes sense to initialize/load some resources at higher level, when they are to be reused by several components. In this case they are to be passed into components via constructors or via LoadContent methods. I have not decided clearly yet.
- "Component" classes hold references for all necessary "leafs" in the "render tree" that they need to successfully render.
- Following from above, "WorldRender" represents a "root" of the "render tree" and is fully reposnsible for managing and orchestrating all sub-components. 
- Some "Components" to be rendered aren't part of the "world". They are managed in parallel to "WorldRender" instance by "TheGame" class.
- "Core" classes do not hold any references to corresponding render "Components". As per above, they are passed in when needed, mainly into `Draw` method or similar.
- "Core" classes do hold all fields needed to successfully simulate themselves in the game world.
- As follows from above "Core" classes "own" their structural "sub-objects", their initialization and simulation pipelines.
- As follows from above "Core" classes "own" the Aether2D "body"/"bodies" needed for simulation within "collision world"
- "GameWorld" class "owns" and manages "collision world"
- Convention: data-points are declared as fields, not properties throughout. Except where it is specifically warranted for some reason.
- Convention: public field names begin with capital letter.
- Convention: private field names begin with lowercase letter, not with "_" as conventional outside of gamedev.
- Convention: public method names begin with capital letter.
- Convention: private method names begin with lowercase letter.
- Preference: the order of class member declaration matters. Members preferrably declared before they are used, in close proximity. Sometimes it makes sense to group related declarations. 
- Game loop has to adhere to the following general "framework" (pseudocode):

```csharp
  // TheGame.Update override
  protected override void Update(GameTime gameTime)
  {
    base.Update(gameTime);

    // Read mouse/keyboard/other device inpus; 
    // ..

    // Perform "non-game" actions; inform "actors" of inputs; 
    _keyboardCustodian.Process(kc => 
    {
      // Handle F11 - fullscreen  
      if (kc.IsKeyPressed(Keys.F11)) { fullscreen(); }
      // Handle ESC - exit
      if (kc.IsKeyPressed(Keys.Escape)) { this.exit(); }
      // Inform actors
      _world.Actor1.InputW = kc.IsKeyDown(Keys.W) ? Actor.InputW.Activated : Actor.InputW.None;
    })

    // Invoke world simulation
    _world.Simulate(gameTime);

    // Post-simulation computations.
    // ...
  }

  // GameWorld.Simulate
  public void Simulate(GameTime gameTime)
  {
    // Four-Phase Simulation Pipeline

    // 1. Compute "projected" state of user-controlled actors given current inputs
    var actorProjected = Actor1.ApplyInputs(gameTime);

    // 2. Prepare all actors for simulation. May require on-the-fly reconstruction of collision bodies, or other preparations.
    Actor1.PreSimulationPrepare(truckProjected);

    // 3. Run simulation step.
    collisionWorld.Step((float)gameTime.ElapsedGameTime.TotalSeconds);

    // 4. Update actors based on projected state and outcome of simulation
    Actor1.PostSimulationUpdate(truckProjected);
    Actor1.ClearInputs();
    SubActors.ForEach(b => b.PostSimulationUpdate());
  }
```

- The top level class is called `TheGame`, derives from `Microsoft.Xna.Framework.Game`. This is where
  - top-level "GameWorld" object is declared, 
  - top-level "WorldRender" component instantiated, 
  - top-level game look orchestration performed;
  - top-level "Drawing" methods implemented
  - top-level content loading performed

- `TheGame` class owns, manages and exposes "global" `SpriteBatch` instance for everyone to use. Other global FNA objects, as necessary, too.

- Player inputs are modelled through sum types (C# enums as most simple approximation of sum type backed by single boolean field). Simple example (pseudocode):
```csharp
  // Car steering wheel
  public enum SteeringInput { Left, None, Right }
  // Airplane pitch control via yoke
  public enum YokePitchInput { Pushed, Neutral, Pulled }

```




---
title: Lessons Learned
categories: [docs]
layout: custom
---
# Lessons Learned

All software projects are the same. You think you have an idea of what you're doing, quickly realize you have no idea what you're doing, and at the end of it all you wish you could refactor 80% of what you wrote.

So, I had a good time. It was definitely helpful to force myself to make something to start the process.
It'll take a fair bit of refactoring to get useable chunks of code out of what I've written, so I imagine I'm mostly walking away with learning and things to learn.

My key takeaways are:

## Keep it dirty (for game jams)

If it needs to be done fast, do as much as possible within the Unity Editor.
There's no reason to try and setup frameworks or systems when you don't have to.

I "wasted" a lot of time doing things I didn't even end up using because leveraging the systems into content requires time.

## Spend time if it's worth it

On the other hand, I already know that a well-built system is a lifesaver once you expand.
The spaghetti code I started churning out is already a pain to navigate through.
Assuming there's no deadline looming, taking the time to build a proper system is worth it.

I don't regret "wasting" time because my goal was always to learn first. 
Working further with deserialization and data structure storage will help any future game I write (especially in regards to things like modding).

## Doing everything is hard

True one-person indie developers are insane. It seems like a no-brainer to pay others for assets after this experience.

If my goal is to be a one-person dev to chase after people like ZUN then I have a mountain more work ahead of me.

# Things to investigate

As usual, there's a lot I don't know. Software development is all about learning to learn and game development is no different it seems.

## Proper serialization and deserialization

I originally read that Unity's JSON implementation couldn't serialize/deserialize dictionaries.
I figured that was fine and made non-unity data classes, using json.net for serialization.

That feels like a mistake because the implementation I ended up using (a dataclass object within the Unity MonoBehaviour) is clunky.
I assume there's a better way to do this, perhaps by simply choosing a different set of data structures 

Or perhaps this is actually a good way to go about things and I just need a little bit more reorganization.

## Component handling

In general, I didn't (and still don't) have a great mental model for how to split up objects within Unity based on the design goal.

I mostly ended up storing data in a dataclass, which was a variable with a script, which was then attached to a GameObject as a component.
As stated above, clunky.

The biggest issue was the constant "dereferencing" necessary in order to get what I needed.
I think some of that is part of working with Unity. If you have a GameObject variable you have to get components.
If you have a component you have to go through the transform to get to the GameObject.

What I need to understand how to do better is when to reference by GameObject or by Component.
Do you track GameObjects or just InstanceIDs and map them?
When do you navigate a data structure and when do you create a variable reference to shortcut it?

## System interactions

Specifically the interactions between game logic, graphics, UI, FX, etc.

My creation here runs as it runs, whereas future game I design should properly lock it's game logic framerate and graphics framerate (to support netcode, stability, etc.)

My current spaghetti code uses arbitrary waits based on discrete time but I imagine a real system is a proper interaction of events, collisions, queues, etc.

I would say this area is the area I need to learn most in and where taking proper time to setup gives the most gains.

I have no clue how to do this properly and in a way to feel good. Interactions and proper timing between game logic, UI, FX, etc.


## All the other little things

Way more than this list here, but this is what remained of my todo list.

- How to properly scale graphics with resolutions
- Figure out unit tests in for Unity/C#
- Figure out how to make Unity play nice with Git
  - Specifically non-text assets like textures, blender models, sound effects, etc.
- All the Blender graphics things
- All the sound things
- Unity video clip has to be a top-level asset?
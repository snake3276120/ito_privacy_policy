# Idle Tower Offender

## Introduction

Greetings, everyone. Thank you for taking the time to read my post. I understand that it is quite lengthy, so if you are solely interested in obtaining the game, feel free to skip ahead and go to [this](#the-game) section.

While this game is indeed complete, I must admit that it is kinda boring. As the developer behind this project, I make this statement because it is essentially a side project. My motivation and objective lie in traversing the entire path of game development, starting from scratch and concluding with its release. Therefore, for us, it represents more of a "from 0 to 1" experience rather than a pursuit of immediate financial success.

I have decided to partially open-source the project, specifically the game code and data objects. However, assets, scenes, prefabs, and similar elements will not be included. Additionally, I aim to share the insights and lessons I have gained throughout this process. My hope is that these resources can provide assistance not only for your own projects but also for your professional growth and aspirations.

Source can be found in this Github repo: <https://github.com/snake3276120/ito_privacy_policy>.
Don't laugh at me, the `REAMDME.MD` is a privacy policy, this post is also written in Markdown and it's kept in the same repo, called `Actual_Readme.md` lol. Source codes are in `.\src`.

### About Myself

I am currently located in the Greater Toronto Area, specifically Markham, Ontario, Canada. In 2013, I graduated from the University of Toronto with a Bachelor's degree in Electrical Engineering.

I am employed full time as a lead level software engineer at a multinational corporation listed on the NYSE. While it may not be considered a FAANG (Facebook, Amazon, Apple, Netflix, Google) or similar top-tier company, it holds a prominent position in the industry. Prior to my current role, I have worked as a software developer in various domains. Additionally, I have a background of three years as an IC Test Engineer, which aligns with my educational background, albeit not as personally fulfilling.

Throughout my life, I have been an avid gamer, starting from the tender age of five. However, with the arrival of my first child in 2021, my gaming activities have been greatly reduced, although I still manage to find time to play video games, almost on a daily basis, but with much less time.

To summarize, I am a seasoned software developer with professional experience in diverse industries, however, excluding the game industry. In the mean time, I maintain a passionate interest in gaming, which has been an inseparable part of my life since childhood.

### Motivations

I always wanted to design and create my own game, as well as making it my own business. I know this is hard and this is almost impossible. I tried to start many projects or joined other's projects before this one, but they all failed.

By no means I'm comparing my game to Rimworld, but when I see Tynan Sylvester started his campaign on Kickstarter (I am the among the very early adopters of Rimworld). He not only designed the game but also wrote the code and created certain assets. Subsequently, he outsourced other assets and music (presumably to the same individual who contributed to the music in Prison Architect). Witnessing the achievements of a single individual in creating something remarkable deeply inspired me. Although I acknowledge that I am far from reaching his level, I am determined to embark on this journey. After all, every significant undertaking requires taking the first step, and for me, this project represents a monumental leap forward. While the game may currently appear simple and lacking excitement, it is at least a complete and bug-free experience.

### How We Started

To start this project, I found a "co-founder", who's also one of my best friend. We start to look at the games we were playing back then. We decide to make a light weight mobile game so we can complete the development quickly (but in the end it took 4+ years, more on that later).

At the time, we were playing [Egg, Inc.](https://egg-inc.fandom.com/wiki/Egg,_Inc.), [Tap Titans 2](https://gamehive.com/games/tap-titans-2/), and [Infinitode](https://infinitode.prineside.com/). Two idle clicker game and a tower defense game. So the idea of creating an idle clicker tower defense like game quickly came to our mind. However, we want to make something different. That's when the ideal of `Tower Offender` came to our mind - user spam soldiers to rush the AI's tower array. If the soldier gets killed, a certain amount of in game cache is generated. Users use those in game cache to upgrade their soldiers.

We started by creating a couple of prototypes that we thought were challenging at the time, which were:

- A tower defense game with basic towers. It might not be a challenge for most of you, but we have never made any games before, and have almost 0 Unity3D experience
- A random maze generator. It's a MxN array, with each element represent a block on the stage. A single random path is generated from top left to the bottom.

With these prototypes in place and stitched together, we started our journey of making our first game.

Before I continue to introduce the game, let me introduce the "team".

### About the "Team"

At the time of written, there are only 1 additional team member besides me, who joined later and who's not among the first few to start the project. I'll introduce him below as `Artist #2`.

- Myself: founder of the project, did all the programming, most of the internal testing, and releasing. I also contributed in game mechanism design, UI wire frames, and contributed to assets. And of course, all the coordinations, PM works, etc.
- Co-founder: let's just call him the co-founder, as he and I initiated the entire project from the beginning. He performed the majority of design and numbers' tweaking for the game, as well as composed the BGM. Unfortunately he left at the later stage of the development.
- UI/UX designer: When the game is about 30% done, we find that we need some proper UI/UX design and we need some game assets (like sprites, icons, etc). I reached out to this guy, who's also a friend of mine and he gladly joined our project. He's also a pro UI/UX designer working full time. Unfortunately he left us near the end of the project, but at least he did his work well and sent me all the assets. The current UI and most icons are from his hand.
- Artist #1: Every game needs artists. We found this guy via our connections. He didn't contribute that much, but all the turrets are done by him, and he laid out the base line for the intro. However, these base lines for the intro are not production ready, that's where Artist #2 came in.
- Artist #2: Found him via a connection. Young guy still in collage. He did really well in creating digital arts. He completed the final version of the intro, and most icons for the upgrades. He's still with me and we are thinking about the next project.
- Other people
  - A data analyst who wanted to gain some coding experience. Unfortunately he's foundations are not there and didn't contribute that much. Hopefully he learnt something by joining the project
  - A pro full stack software engineer who wants to work on a side project. Unfortunately she doesn't play game that much, so this game is not for her tastes. In addition she was doing part time Master program at the time.

That's it about the team. The core team used to be the co-founder, the UI/UX designer, and me. It's sad to see them go (but we are still close friends, and the co-founder helped quite a lot during the final testing and releasing of the game). In the [Takeaways](#takeaways) section I'll talk in more details there.

## The Game

The game is called **`Idle Tower Offender`**. As described above, it's an idle clicker game focusing on a reversed tower defense - you spawn soldiers to rush the tower array. Here are the download links:

- [Apple App Store]("https://apps.apple.com/ca/app/idle-tower-offender/id1563064100")
- [Google Play Store]("https://play.google.com/store/apps/details?id=com.TurboOverboostStudio.IdleTowerOffender")

There's no in game purchase. Only watching ads for in game rewards.

### Mechanism


### Logic and Code Implementation

## Takeaways

I am going to share my takeaways from both the human factor and technical factor.

### Find the Right People



I'd really vote against `learn coding by making a game in X engine`. 
# Starter game for Assignment 3 in Games and AI

This repo contains an optional starter game for Assignment 3 in Games and Artificial Intelligence Techniques (COSC2527 / COSC2528). It has been adapted from Unity's 2D Roguelike tutorial: https://learn.unity.com/project/2d-roguelike-tutorial.

(Please ignore the name of the repo -- this used to be the second assignment in Games and AI, which is why it's called GAIT_**A2**_template.)

The game has been modified to suit ML-Agents better by adding 8 parallel training pens.

You can try out the game by just pressing the play button without running an ML-Agents training script. Bear in mind though that the game is set up to run extremely fast, so it's difficult for a human to control. Also, when manually controlling the game, the controls move all 8 players at once! (When ML-Agents is training, it controls the agents separately.)

The aim of the game is to reach the highest level possible.
- You progress to the next level upon reaching the exit sign.
- Each move costs 1 food.
- When enemies attack you, you lose 20 food.
- You can collect soda and apples to restore food (+20 for soda, +10 for apples)
- In PlayerAgent.cs, the level reached is logged to TensorBoard, so you can use this as the main performance measure in your report.

## Starting Training

To start training, open a command prompt / terminal window and navigate to the top-level folder of this project. Then run:

```mlagents-learn config/roguelike.yaml --run-id=yourRunID```

**Note that the default parameters in roguelike.yaml may not be optimal for your approach, so you should try tuning them.**

To launch TensorBoard, open a separate command prompt at the same location and run:

```tensorboard --logdir results --port 6006```

## What you need to do

A skeleton PlayerAgent.cs script has been provided, but all rewards are set to zero, and CollectObservations() is returning a dummy array of zeroes. It's up to you to fill in the blanks!

## Known issues

The score overlays for the pens don't display at the right size, so it's hard to see the current status of each pen. You should be able to see better by maximising the "Game" tab. You can try to fix this if it's annoying you :)

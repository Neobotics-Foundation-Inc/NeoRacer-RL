# 🏎️ NeoRacer-RL: The Student Training Guide

Welcome to the NeoRacer-RL team! You are about to teach a multi-dimensional neural network how to drive a racecar. 

Training an autonomous agent isn't magic; it is a combination of data collection, mathematical rewards, and patience. This guide will walk you through the exact pipeline we use to train the NeoRacer from scratch using Unity ML-Agents.

---

## 🛠️ Phase 1: The Human Touch (Behavioral Cloning)
If we drop an AI into a track with zero knowledge, it will blindly crash into walls for hours. To bypass this frustrating "random exploration" phase, we use **Behavioral Cloning (BC)**. You will literally record your own keyboard driving, and the neural network will copy your racing line.

**How to record a demonstration:**
1. Open the NeoRacer simulation in Unity.
2. Click on the `Racecar` object in the Hierarchy. 
3. In the Inspector, find the **Demonstration Recorder** component. 
4. Check the box that says **Record**.
5. Give your recording a name (e.g., `MyAwesomeDrive`).
6. Press **Play** in Unity and drive!
   * *Pro-Tip:* Drive smoothly. Stay near the center line. If you crash, stop the recording and try again. The AI will learn your mistakes, so give it perfect data!
7. Once finished, uncheck the "Record" box. Your `.demo` file will be saved in the `Assets/Demonstrations` folder.

---

## 🧠 Phase 2: Entering The Matrix (Training)
Now we feed your driving data and our custom reward function (the Power Mean math) to the Proximal Policy Optimization (PPO) algorithm.

1. Open your terminal (Anaconda Prompt or Nix environment).
2. Navigate to the root of this repository.
3. Run the training command, pointing to our master configuration file:
   ```bash
   mlagents-learn config/racecar_config.yaml --run-id=Test_Run_01
4. When the terminal says, "Start training by pressing the Play button...", go to Unity and hit Play.
5. Grab a coffee. The Unity timescale will speed up automatically. You are now watching the AI learn at superhuman speeds.

## 📊 Phase 3: Reading the Brain Waves (TensorBoard)
How do you know if the AI is actually getting smarter, or just driving in circles? You need to read its mind using TensorBoard.

While your training is running, open a new terminal window, navigate to your folder, and type:
   ```bash
tensorboard --logdir results
```
Open your web browser and go to `localhost:6006`. Here are the three most important graphs to watch:

**1. Cumulative Reward (The Scoreboard)**
**What it is:** The average points the car earns per lap.

**What you want:** A graph that climbs up and to the right.

**The "Handoff Dip":** Our racecar_config.yaml uses your human .demo file for the first 150,000 steps, then shuts it off so the AI can learn to drive faster than you. You will likely see the reward dip slightly at 150k steps as the "training wheels" come off, followed by a massive spike upward as the AI optimizes the track.

**2. Episode Length (The Stopwatch)**
**What it is:** How many physics steps it takes the car to finish (or crash).

**What you want:** Once the AI stops crashing and starts finishing laps, you want this graph to trend downward. Fewer steps mean a faster lap time!

**3. Policy Loss (The Brain Health)**
**What it is:** The magnitude of how much the neural network is changing its internal weights.

**What you want:** A flat, stable, micro-range line (e.g., oscillating between 0.01 and 0.03). If this graph spikes massively or looks like a violent earthquake, the reward math is broken and the AI is panicking.

## 🎓 Phase 4: Graduation (Deploying the Model)
Once the training hits 500,000 steps, it will automatically stop and compile a finalized brain file.

1. Go to results/Test_Run_01/ in your file explorer.
2. Find the file ending in .onnx (This is the compiled neural network).
3. Drag this file into your Unity Project window.
4. Click your Racecar, find the Behavior Parameters component, and drag your new .onnx file into the Model slot.
5. Change the Behavior Type to Inference Only.
Press Play in Unity. Do not touch your keyboard. Watch your newly trained AI navigate the track autonomously! 🚀

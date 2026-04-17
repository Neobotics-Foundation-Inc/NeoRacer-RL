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

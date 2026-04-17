# NeoRacer-RL: Technical Architecture & Reward Formulation

## 1. Abstract
Training autonomous vehicles using Reinforcement Learning (RL) in confined track environments frequently leads to behavioral collapses, such as the "suicide spiral," where agents intentionally crash to avoid prolonged complex navigation. This report details the architecture of the NeoRacer-RL agent, specifically outlining the transition from sparse to continuous dense rewards, sensor downsampling, and the integration of Imitation Learning to stabilize the Proximal Policy Optimization (PPO) pipeline.

---

## 2. Observation Space & Sensor Downsampling
Early iterations of the model utilized high-density LiDAR sweeps, which flooded the observation space with redundant data and caused vector space mismatches in ML-Agents (`1087` expected vs. `6` actual). 

To optimize inference time and streamline the neural network's focus, the sensor suite was aggressively downsampled:
* **Kinematic Vectors:** 6 continuous observations tracking Linear Velocity (X, Y, Z) and Angular Velocity (X, Y, Z).
* **Proximity Sensors:** A 3D Ray Perception Sensor utilizing exactly 3 rays per direction (creating a 7-ray array: 3 left, 3 right, 1 center). This provides a lightweight "whisker" system that detects track boundaries without the computational overhead of full environmental mapping.

---

## 3. Reward Function Engineering
The most significant architectural shift was the abandonment of purely discrete rewards (e.g., `+1.0` per checkpoint) in favor of a continuous, dense micro-reward gradient evaluated at every physics step.

### 3.1 Resolving the Velocity Farming Exploit
Initially, continuous rewards were calculated by averaging normalized alignment and normalized speed. However, this allowed the agent to discover a "farming exploit" where it could park the car perfectly parallel to the centerline at 0 m/s and continuously farm a `0.5` reward. 

To eliminate this, the variables were coupled using geometric multiplication:
$$StepQuality = normAlignment \times normSpeed$$
By multiplying the variables, the agent receives $0$ points if either speed or alignment drops to zero, forcing the model to prioritize fast, forward momentum.

### 3.2 Power Mean Smoothing
To prevent erratic steering adjustments ("twitching") as the agent chased instantaneous step quality peaks, a rolling queue (window size $n = 10$) was introduced. The raw step quality values are smoothed using a Power Mean calculation before being fed to the PPO algorithm:
$$M_p = \left( \frac{1}{n} \sum_{i=1}^n x_i^p \right)^{\frac{1}{p}}$$
With $p = 0.5$, this metric severely penalizes momentary lapses in trajectory while smoothly rewarding sustained successful driving. This smoothed $M_p$ value is multiplied by a small scaler (e.g., `0.01`) and applied every step.

### 3.3 Failsafes and Time Penalties
To further sculpt the racing line, the following discrete modifiers were integrated:
* **Rollover Failsafe:** If the Dot Product of the car's vertical vector and the world's vertical vector drops below $0$, the agent is deemed to have flipped. This triggers a silent `EndEpisode()` to prevent the physics engine from glitching the model weights.
* **Existential Dread (Time Optimization):** Once the agent successfully learns the track, a microscopic penalty (`-0.0005`) is applied every frame. This acts as a ticking clock, forcing the Value Network to straighten the car's trajectory to cross the finish line in the minimum possible steps.

---

## 4. Training Pipeline & Behavioral Cloning
Relying purely on random exploration in a high-penalty environment traps the agent in local minima. To bypass this, the model was trained using a hybrid Imitation Learning pipeline.

1. **Behavioral Cloning (BC):** Human demonstration data (`ExpertDrive.demo`) was recorded at a reduced timescale (`0.3`) to ensure flawless trajectory mapping. For the first $150,000$ steps of training, the BC `strength` parameter was locked at `1.0`, forcing the neural network to strictly copy the human racing line.
2. **Pure RL Transition:** At step $150,001$, BC was completely disabled. The agent experienced a brief "handoff dip" as it transitioned to trial-and-error, but the pre-trained weights allowed it to quickly optimize the human racing line using the continuous reward function.
3. **Hyperparameter Tuning:** Due to the distance between discrete checkpoints on the track, the `time_horizon` was increased to `256`. At a standard 50 FPS physics rate, this allows the Generalized Advantage Estimation (GAE) to look ahead approximately 5 seconds, providing the temporal memory required for the agent to connect steering actions to distant goals.

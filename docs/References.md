# 📚 References & Further Reading

The NeoRacer-RL architecture builds upon foundational research in Reinforcement Learning, open-source simulation environments, and official software documentation. Below is a curated list of references and reading materials for researchers and students interacting with this repository.

## 1. Foundational Code & Prior Art
This simulation environment is a heavily modified evolution of prior academic autonomous racing platforms. We acknowledge the original creators of the physics and base track infrastructure:
* **Boston University F1Tenth Simulation:** The structural foundation for this repository was initially cloned and adapted from the [F1TenthBU / New-Simulation](https://github.com/F1TenthBU/New-Simulation).
* **MIT F1Tenth Simulation:** The original C# vehicle physics modules (`PhysicsModule.cs`, `Drive.cs`, `CenterOfMass.cs`) and Raycast logics stem from the MIT RACECAR simulation architecture.

## 2. Core Reinforcement Learning Algorithms
The NeoRacer-RL agent relies on Proximal Policy Optimization (PPO) as its primary learning algorithm. For students wishing to understand the mathematics behind the Policy Loss and Value updates, refer to the foundational paper:
* Schulman, J., Wolski, F., Dhariwal, P., Radford, A., & Klimov, O. (2017). **Proximal Policy Optimization Algorithms.** *arXiv preprint arXiv:1707.06347*. [Read the Paper](https://arxiv.org/abs/1707.06347)

## 3. Reward Function Mathematics
To understand the smoothing techniques used to prevent agent oscillation ("twitching"), review the mathematical properties of generalized means:
* **The Power Mean (Generalized Mean):** Used to smooth the continuous reward gradient between alignment and speed. [Wikipedia: Generalized Mean](https://en.wikipedia.org/wiki/Generalized_mean)

## 4. Software & Framework Documentation
For troubleshooting, hyperparameter tuning, and API references, consult the official documentation for the tools used in this project:
* **Unity ML-Agents Toolkit:** The official documentation for setting up environments, editing `yaml` configurations, and training with Imitation Learning. [ML-Agents GitHub Docs](https://github.com/Unity-Technologies/ml-agents/tree/main/docs)
* **TensorBoard:** Guide to tracking ML experiments, understanding loss metrics, and analyzing cumulative reward graphs. [TensorBoard Documentation](https://www.tensorflow.org/tensorboard)

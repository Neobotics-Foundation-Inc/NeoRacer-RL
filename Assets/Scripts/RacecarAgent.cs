using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using NWH.WheelController3D;
using TMPro; 

public class RacecarAgent : Agent
{
    public Racecar racecar;
    public Transform CheckPointsParent; 

    [Header("Reward Shaping (Power Mean)")]
    public int rewardWindowSize = 10;
    public float rewardPowerP = 0.5f;
    private Queue<float> rewardHistory = new Queue<float>();

    [Header("UI Debugging")]
    public TextMeshProUGUI rewardDebuggerText;

    // Checkpoint tracking
    private List<Collider> checkpointsList = new List<Collider>();
    private int targetGateIndex = 0;
    private int totalCheckpointsPassed = 0; // NEW: Keeps track of cumulative score for the UI

    public override void Initialize()
    {
        if (CheckPointsParent != null)
        {
            foreach (Transform child in CheckPointsParent)
            {
                Collider col = child.GetComponent<Collider>();
                if (col != null) checkpointsList.Add(col);
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        Rigidbody rb = racecar.GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            racecar.transform.localPosition = new Vector3(0, 0, 28); 
            racecar.transform.localRotation = Quaternion.Euler(0, 180, 0); 
            rb.isKinematic = false;
        }

        WheelController[] wheelControllers = racecar.GetComponentsInChildren<WheelController>();
        foreach (WheelController wc in wheelControllers)
        {
            wc.wheel.angularVelocity = 0f;
        }

        racecar.Drive.Speed = 0f;
        racecar.Drive.Angle = 0f;
        racecar.Drive.SpeedK = 0f;
        racecar.Drive.AngleK = 0f;
        racecar.Collided = false;
        
        // Reset Logic
        racecar.CheckpointsHit = 0; 
        targetGateIndex = 0;
        totalCheckpointsPassed = 0; 
        rewardHistory.Clear();

        foreach (Collider gate in checkpointsList)
        {
            gate.gameObject.SetActive(true);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(racecar.Physics.LinearVelocity);
        sensor.AddObservation(racecar.Physics.AngularVelocity);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        racecar.Drive.Angle = actions.ContinuousActions[0];
        racecar.Drive.Speed = Mathf.Clamp(actions.ContinuousActions[1], 0f, 1f);
        AddReward(0.001f);
        float rawStepQuality = 0f;
        float pMean = 0f;

        if (Vector3.Dot(racecar.transform.up, Vector3.up) < 0f)
        {
             Debug.Log("Physics Glitch: Car flipped. Silently resetting.");
            EndEpisode();
        }

        // 1. CONTINUOUS DENSE REWARD CALCULATION
        if (targetGateIndex < checkpointsList.Count)
        {
            Transform targetGate = checkpointsList[targetGateIndex].transform;
            
            Vector3 dirToTarget = (targetGate.position - racecar.transform.position).normalized;
            float alignment = Vector3.Dot(racecar.transform.forward, dirToTarget);
            float normAlignment = Mathf.Clamp01((alignment + 1f) / 2f); 

            Rigidbody rb = racecar.GetComponent<Rigidbody>();
            float forwardSpeed = Vector3.Dot(rb.velocity, racecar.transform.forward);
            float normSpeed = Mathf.Clamp01(forwardSpeed / racecar.maxSpeed);

            // CHANGED TO MULTIPLICATION: Prevents the 0 MPH farming exploit!
            rawStepQuality = normAlignment * normSpeed; 
            
            rewardHistory.Enqueue(rawStepQuality);
            if (rewardHistory.Count > rewardWindowSize) rewardHistory.Dequeue();

            pMean = CalculatePowerMean(rewardHistory, rewardPowerP);
            
            // Scaled so it provides a breadcrumb trail without overpowering the checkpoint rewards
            AddReward(pMean * 0.01f); 
        }

        // 2. DISCRETE GATE REWARD
        if (racecar.CheckpointsHit > 0)
        {
            AddReward(1.0f * racecar.CheckpointsHit); 
            targetGateIndex += racecar.CheckpointsHit; 
            totalCheckpointsPassed += racecar.CheckpointsHit; 
            racecar.CheckpointsHit = 0; 

            // --- NEW: THE FINISH LINE LOGIC ---
            if (targetGateIndex >= checkpointsList.Count)
            {
                Debug.Log("Track Completed! Massive Success!");
                AddReward(5.0f); // Massive bonus for finishing the whole track
                EndEpisode();    // Safely end the episode before the car hits the wall
            }
        }

        // 3. WALL PENALTY
        if (racecar.Collided)
        {
            AddReward(-1.0f); 
            EndEpisode();
        }
        
        // 4. UPDATE THE UI SCREEN
        if (rewardDebuggerText != null)
        {
            rewardDebuggerText.text = 
                $"Instant Step Quality: {rawStepQuality:F3}\n" +
                $"Power Mean (p={rewardPowerP}): {pMean:F3}\n" +
                $"Checkpoints Hit: {totalCheckpointsPassed}\n" +
                $"Target Gate Index: {targetGateIndex}";
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }

    private float CalculatePowerMean(Queue<float> window, float p)
    {
        if (window.Count == 0) return 0f;
        
        float sum = 0f;
        foreach (float val in window)
        {
            float safeVal = Mathf.Clamp(val, 0.0001f, 1f); 
            sum += Mathf.Pow(safeVal, p);
        }
        
        float mean = sum / window.Count;
        return Mathf.Pow(mean, 1f / p);
    }
}
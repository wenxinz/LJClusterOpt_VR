using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateDisplayInfo : MonoBehaviour
{
    [SerializeField] private Text simulationType;
    [SerializeField] private Text timesteps;
    [SerializeField] private Text energy;
    [SerializeField] private Text totalTimesteps;

    private const string simulationTypePrefix = "Current Simulation Type: \n";
    private const string timestepsPrefix = "Timesteps: ";
    private const string energyPrefix = "Cluster energy: ";
    private const string totalTimestepsPrefix = "Total timestep: ";

    private void Start()
    {
        updateTimesteps("0");
        updateTotalTimesteps("0");
    }

    public void updateSimulationType(string str)
    {
        simulationType.text = simulationTypePrefix + str;
    }

    public void updateTimesteps(string str)
    {
        timesteps.text = timestepsPrefix + str;
    }

    public void updateEnergy(string str)
    {
        energy.text = energyPrefix + str;
    }
    public void markGlobalMinimum()
    {
        energy.color = Color.red;
        energy.text += "-> (Global Minimum)";
    }

    public void updateTotalTimesteps(string str)
    {
        totalTimesteps.text = totalTimestepsPrefix + str;
    }

    public void clearDisplay()
    {
        simulationType.text = simulationTypePrefix;
        timesteps.text = timestepsPrefix;
        totalTimesteps.text = totalTimestepsPrefix;
        energy.text = energyPrefix;
        energy.color = Color.black;
    }
}

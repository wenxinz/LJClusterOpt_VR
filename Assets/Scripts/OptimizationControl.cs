using System.Collections;
using System.IO;
using UnityEngine;
using Info;

public class OptimizationControl : MonoBehaviour {

    [SerializeField] private GameObject displayScreen;
    private UpdateDisplayInfo updateDisplay;

    private bool startOptimization = false;
    public void stopOptimizeCluster()
    {
        startOptimization = false;
    }
    public void startOptimizeCluster()
    {
        startOptimization = true;
    }
    public bool IsClusterOptimizing()
    {
        return startOptimization;
    }

    enum simulationType{
        steepestDescent,
        monteCarlo
    }

    [SerializeField] private LJCluster targetSystem;
    
    private int MCsteps;

    private int currentStep;
    private int totalSteps;
    private simulationType type;
    public void runSteepDescent()
    {
        startOptimization = true;
        type = simulationType.steepestDescent;
        targetSystem.IsSystemAtGlobalMinimum = false;
        currentStepSize = 0.001;
        currentStep = 0;
    }
    private double currentStepSize;

    private int numberStepsPerUpdate = 10;

    private double cur_min_energy;
    private string dataPath;

    private const int stoplimit = 100000000;

    // information needed to perform optimization:
    // targetsystem, number of mc steps
    private void Start()
    {
        
        updateDisplay = displayScreen.GetComponent<UpdateDisplayInfo>();

        startNewOptimization();

        //StartCoroutine(UpdateGraphics());
    }

    /*private IEnumerator UpdateGraphics()
    {
        while (!isSteamInit && !isTimedOut)
        {
            if(startOptimization)
            {
                targetSystem.updateParticles();       
            }

            if (targetSystem.IsSystemAtGlobalMinimum)
            {
                targetSystem.updateParticles();
                updateDisplay.markGlobalMinimum();
            }

            Debug.Log("return control");

            yield return new WaitForSeconds(.1f);

            Debug.Log("interupt the main to update grahpics");

        }

    }*/

    private void OnDisable()
    {
        //StopCoroutine(UpdateGraphics());
    }

    //float starttime;
    //float endtime;

    private void Update()
    {
        if (GvrControllerInput.AppButtonDown)
        {
            startOptimization = !startOptimization;
            if (targetSystem.IsSystemAtGlobalMinimum)
            {
                targetSystem.restartCluster();
            }

        }

        if (startOptimization)
        {
            if(totalSteps == 0)
            {
                writestatetofile("New_Run");
                writestatetofile(cur_min_energy.ToString() + "\t" + totalSteps.ToString());

                ///starttime = Time.realtimeSinceStartup;
                //Debug.Log(starttime);
            }
            switch (type)
            {
                case simulationType.steepestDescent:
                    updateDisplay.updateSimulationType("SteepestDescent");
                    updateDisplay.updateTimesteps(currentStep.ToString());
                    if (steepestDescent(ref currentStepSize))
                    {
                        type = simulationType.monteCarlo;
                        currentStepSize = 0.04;
                        currentStep = 0;
                    }
                    break;
                case simulationType.monteCarlo:
                    updateDisplay.updateSimulationType("Monte Carlo");
                    updateDisplay.updateTimesteps(currentStep.ToString());
                    if (MonteCarlo(ref currentStepSize))
                    {
                        type = simulationType.steepestDescent;
                        currentStepSize = 0.001;
                        currentStep = 0;
                    }
                    break;
            }

            currentStep += 1;
            totalSteps += 1;

            /*if(totalSteps % 10000 == 0)
            {
                endtime = Time.realtimeSinceStartup;
                Debug.Log(endtime);
                Debug.Log("time used to run " + totalSteps + " is " + (endtime - starttime));
                starttime = Time.realtimeSinceStartup;
            }*/

            if (targetSystem.PotentialE < cur_min_energy)
            {
                cur_min_energy = targetSystem.PotentialE;
                writestatetofile(cur_min_energy.ToString() + "\t" + totalSteps.ToString());
            }

            updateDisplay.updateTotalTimesteps(totalSteps.ToString());

            targetSystem.moveToTargetCenter();

            targetSystem.updateParticles();

            if (targetSystem.IsSystemAtGlobalMinimum)
            {
                updateDisplay.markGlobalMinimum();
                writestatetofile("GlobalMinimum_Reached");
            }else if(totalSteps > stoplimit)
            {
                writestatetofile("Stoplimit_Reached");
                targetSystem.restartCluster();
            }

        }
        else
        {
            updateDisplay.updateSimulationType("No simulation running");
        }
    }

    // perform one step of steepestDescent
    // return whether the system is at a minimum -> stop criterion
    private bool steepestDescent(ref double sdStepSize)
    {
        double potentialEBefore = targetSystem.PotentialE;
        Vector3d[] positionsBefore = new Vector3d[targetSystem.ClusterSize];
        for (int i = 0; i < targetSystem.ClusterSize; i++)
        {
            positionsBefore[i] = targetSystem.getPosition(i);
        }

        bool reject = true;
        bool changeStepSize = false;
        bool converged = false;
        while (reject)
        {

            for (int i = 0; i < targetSystem.ClusterSize; i++)
            {
                Vector3d movement = targetSystem.getForce(i) * sdStepSize;
                if (movement.magnitude > 2.0)
                {
                    changeStepSize = true;      
                    break;
                }
                movement += positionsBefore[i]; //positionsBefore[i], not targetSystem.position
                targetSystem.setPosition(movement, i);
            }

            if (changeStepSize)
            {
                sdStepSize *= 0.5;
                changeStepSize = false;
                continue;
            }

            targetSystem.calcPotentialE();
            if (Mathd.Abs(potentialEBefore - targetSystem.PotentialE) < 1e-12)
            {
                // energy change smaller than 10-12 -> reach a minimum
                converged = true;
            }
            else if (targetSystem.PotentialE > potentialEBefore)
            {
                // energy increase -> sd algorithm is diverging -> reduce stepsize
                sdStepSize *= 0.5;
                continue;
            }

            // this step is accepted
            reject = false;
        }

        // now that positions and potentialE are updated, forces should be updated too
        targetSystem.calcForce();

        // always increasing stepsize
        sdStepSize *= 1.1;

        // if converged, test if current minimum is the global minimum
        if (converged)
        {
            if ((targetSystem.PotentialE - LJClusterInfo.LJClusterEnergy[targetSystem.ClusterSize - 2]) < 5.0e-7)
            {
                targetSystem.IsSystemAtGlobalMinimum = true;
                //stop the optimization process when the system reaches the global minimum
                startOptimization = false;
            }
        }

        return converged;
    }

    // perform one step of MonteCarlo
    // return true if certain number of steps of MonteCarlo has been performed -> stop criterion
    private bool MonteCarlo(ref double mcSize)
    {
        double potentialBefore = targetSystem.PotentialE;
        Vector3d[] positionsBefore = new Vector3d[targetSystem.ClusterSize];
        for(int i = 0; i < targetSystem.ClusterSize; i++)
        {
            positionsBefore[i] = targetSystem.getPosition(i);
        }

        for(int i = 0; i < targetSystem.ClusterSize; i++)
        {
            //Vector3d moveDirection = new Vector3d(Random.value,Random.value,Random.value).normalized;
            Vector3d moveDirection = new Vector3d(Random.Range(-1f,1f),Random.Range(-1f,1f),Random.Range(-1f,1f)).normalized;
            targetSystem.setPosition(targetSystem.getPosition(i)+moveDirection*mcSize,i);
        }
        targetSystem.calcPotentialE();

        if(targetSystem.PotentialE > potentialBefore)
        {
            double acceptProb = Mathd.Exp(-(targetSystem.PotentialE - potentialBefore) / targetSystem.Temperature);
            if(Random.value > acceptProb)
            {
                for (int i = 0; i < targetSystem.ClusterSize; i++)
                {
                    targetSystem.setPosition(positionsBefore[i], i);
                }
                targetSystem.calcPotentialE();
            }
        }

        if (currentStep == MCsteps)
        {
            return true;
        }

        return false;
    }

    public void writestatetofile(string content)
    {
        using (StreamWriter streamwWriter = File.AppendText(dataPath)) {
            streamwWriter.Write(content + "\n");
        }
    }

    public void startNewOptimization()
    {
        MCsteps = (int)(80.964 * Mathd.Pow(1.09453, targetSystem.ClusterSize));
        currentStep = 0;
        totalSteps = 0;
        type = simulationType.steepestDescent;
        currentStepSize = 0.001;
        cur_min_energy = targetSystem.PotentialE;
        dataPath = Path.Combine(Application.persistentDataPath, targetSystem.ClusterSize.ToString());
        Directory.CreateDirectory(dataPath);
        dataPath = Path.Combine(dataPath, "PerformanceData.txt");
        //In player setting > write access: external
        //On adroid device: Application.persistentDataPath -> /storage/emulated/0/Android/data/com.CU.LJClusterOpt/files
    }
}

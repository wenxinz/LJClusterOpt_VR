using UnityEngine;
using Info;

public class LJCluster : MonoBehaviour {

    [SerializeField] private GameObject displayScreen;
    private UpdateDisplayInfo updateDisplay;

    public bool isRotable = true;

    private double boundary = 10.0;

    private int clusterSize = 10;
    public int ClusterSize
    {
        get { return clusterSize; }
        set
        {
            if (value > 1 && value < 29)
            {
                clusterSize = value;
            }
            else
            {
                // invalid value for clusterSize
                // set to default: 10
                clusterSize = 10;
            }
        }
    }

    private Vector3d clusterCenter = new Vector3d(0.0, 0.0, 0.0);

    private double temperature = 0.7;
    public double Temperature
    {
        get { return temperature; }
        set { temperature = value; }
    }

    private GameObject[] cluster;
    public GameObject particle;

    //private Vector3d center;

    public Vector3d[] positions;
    public Vector3d getPosition(int i)
    {
        return positions[i];
    }
    public void setPosition(Vector3d pos, int i)
    {
        positions[i] = pos;
    }

    private Vector3d[] forces;
    public Vector3d getForce(int i)
    {
        return forces[i];
    }

    private double[] particleEnergy;

    private double potentialE;
    public double PotentialE
    {
        get { return this.potentialE; }
    }
    private bool isSystemAtGlobalMinimum = false;
    public bool IsSystemAtGlobalMinimum
    {
        get { return isSystemAtGlobalMinimum; }
        set { isSystemAtGlobalMinimum = value; }
    }

    private Color particleColor;

    // LJ potential
    private double ljPotential(double r)
    {
        double r2 = r * r;
        double r6 = r2 * r2 * r2;
        r6 = 1.0 / r6;
        return 4.0 * r6 * (r6 - 1.0);
    }

    // LJ potential gradient
    private double ljGradient(double r)
    {
        double r2 = r * r;
        double r6 = r2 * r2 * r2;
        r6 = 1.0 / r6;
        return -48.0 * r6 * (r6 - 0.5) / r;
    }

    // calculate system potential energy
    public void calcPotentialE()
    {
        potentialE = 0.0;
        for (int i = 0; i < clusterSize; i++)
        {
            particleEnergy[i] = 0.0;
            for (int j = 0; j < clusterSize; j++)
            {
                if(i!=j)
                    particleEnergy[i] += ljPotential(Vector3d.Distance(positions[i],positions[j]));
            }
            potentialE += particleEnergy[i];
        }
        potentialE *= 0.5;
    }

    // calcuate force for each particle
    public void calcForce()
    {
        for (int i = 0; i < clusterSize; i++)
        {
            Vector3d fi = Vector3d.zero;
            for (int j = 0; j < clusterSize; j++)
            {
                if (j != i)
                {
                    Vector3d rij = positions[i] - positions[j];
                    fi += -ljGradient(rij.magnitude) * rij.normalized;
                }
            }
            forces[i] = fi;
        }
    }

    // Use this for initialization
    // default cluster center: clusterCenter
    void Awake() {
        updateDisplay = displayScreen.GetComponent<UpdateDisplayInfo>();
        clusterInit();
    }

    private void clusterInitPosition()
    {
        Vector3d center = Vector3d.zero;
        bool reject;
        for (int i = 0; i < clusterSize; i++)
        {
            reject = true;

            Vector3d position = new Vector3d(Random.value, Random.value, Random.value) * boundary;

            // test to see if this position is proper for particle i
            // standard: distance with any particle > 1.0;
            //           distance with at least one particle < 2.0
            for (int j = 0; j < i; j++)
            {
                double distance = Vector3d.Distance(position, positions[j]);
                if (distance < 2.0)
                {
                    reject = false;
                    if (distance < 1.0)
                    {
                        reject = true;
                        break;
                    }
                }
            }

            // if the position is rejected, regenerate the position and test again
            if (reject && i != 0)
            {
                i--;
            }
            else
            {
                positions[i] = position;
                center += position;
            }
        }
        center /= clusterSize;
        calcPotentialE();
        calcForce();
        center -= clusterCenter; // move cluster centroid to target position: clusterCenter 
        for (int i = 0; i < clusterSize; i++)
        {
            positions[i] -= center;
        }
    }

    private void clusterInit()
    {
        positions = new Vector3d[clusterSize];
        particleEnergy = new double[clusterSize];
        forces = new Vector3d[clusterSize];

        clusterInitPosition();

        cluster = new GameObject[clusterSize];
        Quaternion rotation = Quaternion.identity;
        for (int i = 0; i < clusterSize; i++)
        {
            cluster[i] = Instantiate(particle, Vector3.zero, rotation);
            cluster[i].name = "LJParticle_" + i;
            cluster[i].transform.parent = this.transform;
            cluster[i].transform.localPosition = (Vector3)positions[i];
        }

        updateParticles();
    }

    public void moveToTargetCenter()
    {
        Vector3d center = Vector3d.zero;
        for(int i = 0; i < clusterSize; i++)
        {
            center += positions[i];
        }
        center /= clusterSize;
        center -= clusterCenter;
        for(int i = 0; i < clusterSize; i++)
        {
            positions[i] -= center;
        }
    }

    public void updateParticles()
    {
        //Debug.Log("update graphics start");

        MaterialPropertyBlock props = new MaterialPropertyBlock();
        double scale;

        for (int i = 0; i < clusterSize; i++)
        {
            Color color1 = new Color(0f, 0f, 0f); //black
            Color color2= new Color(0.34530838f, 0.104251701f, 0.085952321f); //0.2
            Color color3= new Color(0.694333453f, 0.147366584f, 0.135554156f); //0.4
            Color color4= new Color(0.898208609f, 0.765668481f, 0.018019322f); //0.8
            Color color5 = new Color(1f, 1f, 1f); //white

            if(particleEnergy[i] < 0)
            {
                scale = particleEnergy[i] * ClusterSize / LJClusterInfo.LJClusterEnergy[ClusterSize - 2];
                //energy: min->0 (scale: 1->0) -> black->yellow
                if (scale > 0.99) // 1% e_min --- red
                {
                    particleColor = Color.Lerp(color1, color3, 100f*(float)(1 - scale));
                }
                else 
                {
                    particleColor = Color.Lerp(color4, color3, (float)scale/0.99f); 
                }
            }
            else if(particleEnergy[i] < 5) //energy: 0->5 -> yellow->white
            {
                scale = particleEnergy[i]/5;
                particleColor = Color.Lerp(color4, color5, (float)scale); 
            }
            else //energy: >=5 -> white
            {
                particleColor = color5;
            }
            props.SetColor("_Color", particleColor);

            cluster[i].transform.localPosition = new Vector3((float)positions[i].x,(float)positions[i].y,(float)positions[i].z);
            cluster[i].GetComponent<MeshRenderer>().SetPropertyBlock(props);

            updateDisplay.updateEnergy(potentialE.ToString("F4"));
        }

        //Debug.Log("update graphics finish");
    }

    public void updateOneParticleGlobal(Vector3 pos, int i)
    {
        cluster[i].transform.position = pos;
        positions[i] = new Vector3d(cluster[i].transform.localPosition);
        calcPotentialE();

        MaterialPropertyBlock props = new MaterialPropertyBlock();
        double scale;
        Color color1 = new Color(0f, 0f, 0f); //black
        Color color2 = new Color(0.34530838f, 0.104251701f, 0.085952321f); //0.2
        Color color3 = new Color(0.694333453f, 0.147366584f, 0.135554156f); //0.4
        Color color4 = new Color(0.898208609f, 0.765668481f, 0.018019322f); //0.8
        Color color5 = new Color(1f, 1f, 1f); //white

        if (particleEnergy[i] < 0)
        {
            scale = particleEnergy[i] * ClusterSize / LJClusterInfo.LJClusterEnergy[ClusterSize - 2];
            //energy: min->0 (scale: 1->0) -> black->yellow
            if (scale > 0.99) // 1% e_min --- red
            {
                particleColor = Color.Lerp(color1, color3, 100f * (float)(1 - scale));
            }
            else
            {
                particleColor = Color.Lerp(color4, color3, (float)scale / 0.99f);
            }
        }
        else if (particleEnergy[i] < 5) //energy: 0->5 -> yellow->white
        {
            scale = particleEnergy[i] / 5;
            particleColor = Color.Lerp(color4, color5, (float)scale);
        }
        else //energy: >=5 -> white
        {
            particleColor = color5;
        }
        props.SetColor("_Color", particleColor);
        cluster[i].GetComponent<MeshRenderer>().SetPropertyBlock(props);

        updateDisplay.updateEnergy(potentialE.ToString("F4"));
    }

    private void clusterDestroy()
    {
        for (int i = 0; i < clusterSize; i++)
        {
            DestroyImmediate(cluster[i]);
        }
        isSystemAtGlobalMinimum = false;
        updateDisplay.clearDisplay();
    }

    public void changeClusterSize(int new_size)
    {
        clusterDestroy();
        clusterSize = new_size;
        clusterInit();
        this.gameObject.GetComponent<OptimizationControl>().startNewOptimization();
    }

    public void restartCluster()
    {
        updateDisplay.clearDisplay();
        isSystemAtGlobalMinimum = false;
        clusterInitPosition();
        updateParticles();
        this.gameObject.GetComponent<OptimizationControl>().startNewOptimization();
    }

}

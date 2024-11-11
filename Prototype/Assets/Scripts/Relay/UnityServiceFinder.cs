using System.Linq;
using UnityEngine;

public class UnityServiceFinder : MonoBehaviour
{
    public static RelayManager RelayManager => RelayManager.Instances.First();
    public static UnityServiceManager UnityServiceManager => UnityServiceManager.Instances.First();
}
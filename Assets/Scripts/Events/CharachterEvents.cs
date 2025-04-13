using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class CharachterEvents
{
    public static UnityAction<GameObject, int> charachterDamaged;
    public static UnityAction<GameObject, int> charachterHealed;
    public static UnityAction<GameObject> charachterDied;
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BrutalCompany.Component
{
    internal class QuotaAjuster : MonoBehaviour
    {
        public TimeOfDay TOD;
        public float messageTimer = 58;
        public void Awake()
        {
            Plugin.mls.LogWarning("Changing quota variables");
            
        }

        public void Update()
        {
            //float num = TOD.lengthOfHours * RoundManager.Instance.curr;
            //Plugin.mls.LogMessage();
            if (TOD == null)
            {
                TOD = FindFirstObjectByType<TimeOfDay>();
                //Plugin.mls.LogWarning("TimeOfDay is NULL!");
            }
            else
            {
                //TOD.quotaVariables.startingQuota = 1000;
                //TOD.quotaVariables.deadlineDaysAmount = 10;
                TOD.quotaVariables.baseIncrease = 275;
                //TOD.quotaVariables.randomizerMultiplier = 0;
                //TOD.profitQuota = 1000;
            }
            //Plugin.mls.LogMessage((RoundManager.Instance.timeScript.currentDayTime / RoundManager.Instance.timeScript.totalTime).ToString());
            if (messageTimer > 60)
            {
                if (HUDManager.Instance != null)
                {
                    messageTimer = 0;
                    
                }
            }
            else
            {
                //messageTimer += Time.deltaTime;
            }
        }
    }
}
